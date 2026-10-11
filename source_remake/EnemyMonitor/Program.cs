using System.Diagnostics;
using System.Drawing.Imaging;
using RisingLands.Monitoring;
namespace RisingLands.EnemyMonitor;
internal static class Program {
 [STAThread] static int Main(string[] args){
  string? Option(string key){int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:null;}
  ApplicationConfiguration.Initialize();
  string dir=Path.GetFullPath(Option("--data")??Path.Combine(AppContext.BaseDirectory,"..","..","runtime","enemy-monitor"));
  try{using var form=new MonitorWindow(dir);
   if(Option("--capture") is {} capture){form.Show();form.RefreshFeed();Application.DoEvents();if(Option("--tab")=="technology")form.SelectTechnology();if(Option("--tab")=="relation")form.SelectRelation();if(Option("--tab")=="military")form.SelectMilitary();Application.DoEvents();using var bitmap=new Bitmap(form.Width,form.Height);form.DrawToBitmap(bitmap,new Rectangle(Point.Empty,form.Size));bitmap.Save(capture,ImageFormat.Png);form.Close();return 0;}
   if(Option("--check") is {} report){form.RefreshFeed();var lines=form.Summary();if(!form.VerifyRendered())throw new Exception("Os recursos ou a tecnologia não apareceram no monitor.");File.WriteAllText(report,lines);return 0;}
   Application.Run(form);return 0;
  }catch(Exception e){if(Option("--check") is {} report)File.WriteAllText(report,e.ToString());else MessageBox.Show(e.Message,"Monitor dos inimigos",MessageBoxButtons.OK,MessageBoxIcon.Error);return 1;}
 }
}
public sealed class MonitorWindow:Form {
 readonly string directory;readonly ComboBox sessions=new(){DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Fill};
 readonly Label status=new(){Dock=DockStyle.Fill,AutoEllipsis=true,TextAlign=ContentAlignment.MiddleLeft};
 readonly TabControl tabs=new(){Dock=DockStyle.Fill};readonly DataGridView economy=Grid(),technology=Grid(),military=Grid(),relations=Grid();
 readonly ComboBox enemies=new(){DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Fill};
 readonly TreeView discoveries=new(){Dock=DockStyle.Fill,BorderStyle=BorderStyle.None,BackColor=Color.FromArgb(28,32,38),ForeColor=Color.Gainsboro};
 readonly Label research=new(){Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,AutoEllipsis=true};readonly ProgressBar progress=new(){Dock=DockStyle.Fill,Minimum=0,Maximum=1000};
 readonly System.Windows.Forms.Timer timer=new(){Interval=500};MonitorSnapshot? current;bool updating;string detailsKey="";Dictionary<int,MonitorSnapshot> feeds=new();
 static readonly (int Type,string Name)[] MilitaryCatalog={(0,"Espadachim"),(1,"Incendiário"),(2,"Capitão"),(3,"Killer"),(4,"Druida"),(8,"Arqueiro"),(11,"Speeder"),(12,"Boomer"),(14,"Bomber"),(15,"Hellfire"),(16,"Helipede"),(25,"Stomper")};
 void AddMilitaryColumn(int type,string name){
  var column=new DataGridViewTextBoxColumn{Name="troop-"+type,HeaderText=name,Tag=type,Width=Math.Max(95,TextRenderer.MeasureText(name,Font).Width+28),SortMode=DataGridViewColumnSortMode.NotSortable};
  column.DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleCenter;military.Columns.Add(column);
 }
 static string EnemyLabel(EnemySnapshot enemy)=>enemy.Owner switch{1=>"Inimigo 1 · Vermelho",2=>"Inimigo 2 · Amarelo",3=>"Inimigo 3 · Verde",_=>enemy.Name};
 static Color Background=>Color.FromArgb(20,24,29);
 public MonitorWindow(string directory){
  this.directory=directory;Text="Rising Lands · Monitor dos inimigos · v4";Size=new(1000,700);MinimumSize=new(850,580);StartPosition=FormStartPosition.CenterScreen;BackColor=Background;ForeColor=Color.Gainsboro;Font=new("Segoe UI",10);AutoScaleMode=AutoScaleMode.Dpi;
  var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,Padding=new Padding(20)};layout.RowStyles.Add(new(SizeType.Absolute,44));layout.RowStyles.Add(new(SizeType.Absolute,40));layout.RowStyles.Add(new(SizeType.Percent,100));layout.RowStyles.Add(new(SizeType.Absolute,40));Controls.Add(layout);
  layout.Controls.Add(new Label{Text="MONITOR DOS INIMIGOS",Font=new("Segoe UI",18,FontStyle.Bold),Dock=DockStyle.Fill},0,0);
  var selector=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2};selector.ColumnStyles.Add(new(SizeType.Absolute,80));selector.ColumnStyles.Add(new(SizeType.Percent,100));selector.Controls.Add(new Label{Text="Partida",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft});selector.Controls.Add(sessions,1,0);layout.Controls.Add(selector,0,1);
  layout.Controls.Add(tabs,0,2);layout.Controls.Add(status,0,3);
  var resourceTab=new TabPage("Economia"){BackColor=Background,Padding=new Padding(12)};var techTab=new TabPage("Tecnologia"){BackColor=Background,Padding=new Padding(12)};tabs.TabPages.Add(resourceTab);tabs.TabPages.Add(techTab);
  var relationTab=new TabPage("Relação comigo"){BackColor=Background,Padding=new Padding(12)};tabs.TabPages.Add(relationTab);
  var relationLayout=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=2};relationLayout.RowStyles.Add(new(SizeType.Absolute,64));relationLayout.RowStyles.Add(new(SizeType.Percent,100));relationTab.Controls.Add(relationLayout);
  relationLayout.Controls.Add(new Label{Text="Aliança e simpatia reais do jogo. Simpatia é um valor numérico, não uma porcentagem.",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft});relationLayout.Controls.Add(relations,0,1);
  foreach(string name in new[]{"Inimigo","Situação","Relação comigo","Simpatia"})relations.Columns.Add(name,name);
  var militaryTab=new TabPage("Força militar"){BackColor=Background,Padding=new Padding(12)};tabs.TabPages.Add(militaryTab);
  var militaryLayout=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=2};militaryLayout.RowStyles.Add(new(SizeType.Absolute,64));militaryLayout.RowStyles.Add(new(SizeType.Percent,100));militaryTab.Controls.Add(militaryLayout);
  militaryLayout.Controls.Add(new Label{Text="Combatentes vivos por tipo, inclusive recolhidos em prédios.\nEscolha uma partida com indicadores completos no seletor acima.",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft});militaryLayout.Controls.Add(military,0,1);
  military.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.None;
  military.Columns.Add("Inimigo","Inimigo");military.Columns[0].Width=210;military.Columns[0].Frozen=true;
  foreach(var troop in MilitaryCatalog)AddMilitaryColumn(troop.Type,troop.Name);
  military.RowTemplate.Height=62;
  var resourceLayout=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=2};resourceLayout.RowStyles.Add(new(SizeType.Absolute,48));resourceLayout.RowStyles.Add(new(SizeType.Percent,100));resourceLayout.Controls.Add(new Label{Text="Estoque disponível de cada clã · atualização a cada meio segundo",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft});resourceLayout.Controls.Add(economy,0,1);resourceTab.Controls.Add(resourceLayout);
  foreach(string name in new[]{"Inimigo","Situação","Comida","Pedra","Engrenagens","Personagens","Estruturas"})economy.Columns.Add(name,name);
  economy.Columns[0].FillWeight=180;economy.Columns[1].FillWeight=120;
  var techLayout=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=4};techLayout.RowStyles.Add(new(SizeType.Absolute,235));techLayout.RowStyles.Add(new(SizeType.Absolute,40));techLayout.RowStyles.Add(new(SizeType.Absolute,28));techLayout.RowStyles.Add(new(SizeType.Percent,100));techTab.Controls.Add(techLayout);
  techLayout.Controls.Add(technology,0,0);foreach(string name in new[]{"Inimigo","Agricultura","Engenharia","Armamento","Espiritualidade"})technology.Columns.Add(name,name);technology.Columns[0].FillWeight=180;
  var techSelector=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2};techSelector.ColumnStyles.Add(new(SizeType.Absolute,180));techSelector.ColumnStyles.Add(new(SizeType.Percent,100));techSelector.Controls.Add(new Label{Text="Detalhes de pesquisa",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft});techSelector.Controls.Add(enemies,1,0);techLayout.Controls.Add(techSelector,0,1);
  var progressLayout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2};progressLayout.ColumnStyles.Add(new(SizeType.Percent,72));progressLayout.ColumnStyles.Add(new(SizeType.Percent,28));progressLayout.Controls.Add(research);progressLayout.Controls.Add(progress,1,0);techLayout.Controls.Add(progressLayout,0,2);techLayout.Controls.Add(discoveries,0,3);
  sessions.SelectedIndexChanged+=(_,_)=>{if(!updating)RenderSelected();};enemies.SelectedIndexChanged+=(_,_)=>{if(!updating)RenderDetails();};timer.Tick+=(_,_)=>RefreshFeed();Shown+=(_,_)=>{RefreshFeed();timer.Start();};FormClosed+=(_,_)=>timer.Dispose();
 }
 static DataGridView Grid()=>new(){Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,AllowUserToResizeRows=false,RowHeadersVisible=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false,BackgroundColor=Background,BorderStyle=BorderStyle.None,EnableHeadersVisualStyles=false,ColumnHeadersHeight=38,ColumnHeadersDefaultCellStyle=new(){BackColor=Color.FromArgb(40,46,54),ForeColor=Color.White,Font=new("Segoe UI",10,FontStyle.Bold)},DefaultCellStyle=new(){BackColor=Color.FromArgb(28,32,38),ForeColor=Color.Gainsboro,SelectionBackColor=Color.FromArgb(54,68,82),SelectionForeColor=Color.White},AlternatingRowsDefaultCellStyle=new(){BackColor=Color.FromArgb(33,38,45)},RowTemplate={Height=62},GridColor=Color.FromArgb(48,54,62)};
 sealed record SessionChoice(int Id,string Label){public override string ToString()=>Label;}
 sealed record EnemyChoice(int Id,string Label){public override string ToString()=>Label;}
 public void RefreshFeed(){
  var fresh=new List<MonitorSnapshot>();try{if(Directory.Exists(directory))foreach(string path in Directory.EnumerateFiles(directory,"game-*.json")){try{fresh.Add(MonitorFeed.Read(path));}catch(Exception e)when(e is IOException or System.Text.Json.JsonException or InvalidDataException){}}}catch(IOException){}
  int? selected=(sessions.SelectedItem as SessionChoice)?.Id;feeds=fresh.GroupBy(f=>f.ProcessId).ToDictionary(g=>g.Key,g=>g.OrderByDescending(f=>f.UpdatedUtc).First());var ordered=feeds.Values.OrderByDescending(f=>Live(f)).ThenByDescending(CompleteIndicators).ThenByDescending(f=>f.UpdatedUtc).ToArray();
  updating=true;var choices=ordered.Select(f=>new SessionChoice(f.ProcessId,$"Missão {f.Mission} · processo {f.ProcessId} · {(Live(f)?f.Status:"Sem conexão")} · {(CompleteIndicators(f)?"Indicadores completos":"Indicadores limitados")}")).ToArray();
  if(!sessions.Items.Cast<SessionChoice>().SequenceEqual(choices)){sessions.Items.Clear();sessions.Items.AddRange(choices.Cast<object>().ToArray());}
  if(selected is {} id&&feeds.ContainsKey(id)&&(Live(feeds[id])||!ordered.Any(Live)))sessions.SelectedIndex=Array.FindIndex(ordered,f=>f.ProcessId==id);else if(ordered.Length>0)sessions.SelectedIndex=0;
  updating=false;RenderSelected();
 }
 static bool CompleteIndicators(MonitorSnapshot f)=>f.Enemies.Where(e=>e.Present).All(e=>e.Relation!=null&&e.Sympathy!=null&&e.Military!=null);
 static bool Live(MonitorSnapshot f)=>f.Running&&(DateTimeOffset.UtcNow-f.UpdatedUtc).TotalSeconds<4;
 void RenderSelected(){
  current=sessions.SelectedItem is SessionChoice choice?feeds.GetValueOrDefault(choice.Id):null;economy.Rows.Clear();technology.Rows.Clear();military.Rows.Clear();relations.Rows.Clear();
  if(current==null){detailsKey="";status.Text="Aguardando o remake v89 ou posterior. Abra uma missão para conectar.";research.Text="Nenhuma partida conectada";discoveries.Nodes.Clear();progress.Value=0;return;}
  foreach(var troop in current.Enemies.SelectMany(e=>e.Military??Array.Empty<MilitaryUnitSnapshot>()).OrderBy(m=>m.Type))if(!military.Columns.Contains("troop-"+troop.Type))AddMilitaryColumn(troop.Type,troop.Name);
  int previous=(enemies.SelectedItem as EnemyChoice)?.Id??1;updating=true;var enemyChoices=new List<EnemyChoice>();
  foreach(var e in current.Enemies){string state=!e.Present?"Ausente":e.Defeated?"Derrotado":"Ativo";object Amount(int x)=>e.Present?x:"—";
   economy.Rows.Add(EnemyLabel(e),state,Amount(e.Food),Amount(e.Stone),Amount(e.Gears),Amount(e.Units),Amount(e.Buildings));
   technology.Rows.Add(EnemyLabel(e),Level(0),Level(2),Level(1),Level(3));string Level(int branch){var t=e.Technologies.Single(t=>t.Branch==branch);return e.Present?$"{t.Level} / {t.Maximum}":"—";}
      string relation=!e.Present?"—":e.Relation??"Indisponível";object sympathy=!e.Present?"—":e.Sympathy is {} value?value:"Indisponível";
   relations.Rows.Add(EnemyLabel(e),state,relation,sympathy);
   var militaryValues=new List<object>{EnemyLabel(e)};
   foreach(DataGridViewColumn column in military.Columns.Cast<DataGridViewColumn>().Skip(1)){
    int type=(int)column.Tag!;var troop=e.Military?.FirstOrDefault(m=>m.Type==type);
    militaryValues.Add(!e.Present?"—":e.Military==null?"Indisponível":troop==null?"Indisponível":troop.Count);
   }
   military.Rows.Add(militaryValues.ToArray());
   enemyChoices.Add(new EnemyChoice(e.Owner,EnemyLabel(e)+(e.Present?"":" · ausente")));
  }
  if(!enemies.Items.Cast<EnemyChoice>().SequenceEqual(enemyChoices)){enemies.Items.Clear();enemies.Items.AddRange(enemyChoices.Cast<object>().ToArray());}
  enemies.SelectedIndex=Math.Clamp(previous-1,0,2);updating=false;
  string clock=TimeSpan.FromSeconds(current.Tick/30d).ToString(@"hh\:mm\:ss");var age=Math.Max(0,(DateTimeOffset.UtcNow-current.UpdatedUtc).TotalSeconds);
  status.Text=Live(current)?$"● Conectado · Missão {current.Mission} · {clock} de jogo · {current.Speed}x · {current.Status}":$"● Sem conexão · último retrato: {current.UpdatedUtc.ToLocalTime():HH:mm:ss} · dados há {age:0}s";
  if(!CompleteIndicators(current))status.Text+=" · Esta versão do jogo não fornece relação e força militar; selecione uma partida com indicadores completos.";
  status.ForeColor=Live(current)?Color.LightGreen:Color.FromArgb(238,195,114);RenderDetails();
 }
 void RenderDetails(){
  if(current==null||enemies.SelectedItem is not EnemyChoice choice)return;var e=current.Enemies.Single(e=>e.Owner==choice.Id);
  progress.Value=(int)Math.Round(e.ResearchProgress*1000);string branch=e.ResearchBranch>=0?e.Technologies.Single(t=>t.Branch==e.ResearchBranch).Name:"Nenhuma";
  research.Text=!e.Present?"Ausente nesta missão":$"{e.ResearchStatus} · {branch} · {e.ResearchProgress:P0} · {e.Scientists} colonos · {e.Discoveries}/2 concluídas";
  string key=current.Session+"/"+e.Owner+"/"+e.Present+"/"+string.Join(",",e.Technologies.Select(t=>t.Level));if(key==detailsKey)return;detailsKey=key;discoveries.Nodes.Clear();
  if(!e.Present)return;foreach(int b in new[]{0,2,1,3}){var t=e.Technologies.Single(t=>t.Branch==b);var node=new TreeNode($"{t.Name} · nível {t.Level}/{t.Maximum}");foreach(string text in t.Developed)node.Nodes.Add(text);if(t.Developed.Length==0)node.Nodes.Add("Nenhuma tecnologia desenvolvida");discoveries.Nodes.Add(node);}discoveries.ExpandAll();
 }
 public bool VerifyRendered(){
  if(current==null||economy.Rows.Count!=3||technology.Rows.Count!=3||relations.Rows.Count!=3)return false;
  for(int i=0;i<3;i++){var e=current.Enemies[i];if(!e.Present)continue;
   if(!Equals(economy.Rows[i].Cells[2].Value,e.Food)||!Equals(economy.Rows[i].Cells[3].Value,e.Stone)||!Equals(economy.Rows[i].Cells[4].Value,e.Gears))return false;
   if(!Equals(relations.Rows[i].Cells[2].Value,e.Relation??"Indisponível")||!Equals(relations.Rows[i].Cells[3].Value,e.Sympathy is {} sympathyValue?(object)sympathyValue:"Indisponível"))return false;
   if(military.Rows.Count!=3||!Equals(military.Rows[i].Cells[0].Value,EnemyLabel(e)))return false;
   foreach(DataGridViewColumn column in military.Columns.Cast<DataGridViewColumn>().Skip(1)){
    var troop=e.Military?.FirstOrDefault(m=>m.Type==(int)column.Tag!);
    object expected=e.Military==null||troop==null?"Indisponível":troop.Count;
    if(!Equals(military.Rows[i].Cells[column.Index].Value,expected))return false;
   }
   int col=1;foreach(int branch in new[]{0,2,1,3}){var t=e.Technologies.Single(t=>t.Branch==branch);if((string?)technology.Rows[i].Cells[col++].Value!=$"{t.Level} / {t.Maximum}")return false;}
  }return true;
 }
 public void SelectTechnology()=>tabs.SelectedIndex=1; public void SelectRelation()=>tabs.SelectedIndex=2; public void SelectMilitary()=>tabs.SelectedIndex=3;
 public string Summary()=>string.Join("\n",economy.Rows.Cast<DataGridViewRow>().Select(r=>string.Join(" | ",r.Cells.Cast<DataGridViewCell>().Select(c=>c.Value))))+"\n"+string.Join(" | ",technology.Columns.Cast<DataGridViewColumn>().Select(c=>c.HeaderText))+"\n"+research.Text+"\n"+string.Join("\n",relations.Rows.Cast<DataGridViewRow>().Select(r=>string.Join(" | ",r.Cells.Cast<DataGridViewCell>().Select(c=>c.Value))))+"\n"+string.Join(" | ",military.Columns.Cast<DataGridViewColumn>().Select(c=>c.HeaderText))+"\n"+string.Join("\n",military.Rows.Cast<DataGridViewRow>().Select(r=>string.Join(" | ",r.Cells.Cast<DataGridViewCell>().Select(c=>c.Value))));
}
