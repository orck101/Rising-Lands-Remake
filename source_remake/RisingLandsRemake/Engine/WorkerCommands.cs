using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;

public sealed partial class ResearchGame {
    void DrawFoodOrders(){
        if(selected is {Type:7,Owner:0} miner&&miner.MineIndex>=0&&(miner.MineJob!=0||miner.Rock>0)){
            var mine=simulation.World.Mines[miner.MineIndex];var point=ToScreen((mine.Cell&255)*32,(mine.Cell>>8)*32);Outline(new Rectangle((int)point.X,(int)point.Y,(int)(64*zoom),(int)(64*zoom)),gold,2);Text("Rocha: "+mine.Quantity,(int)point.X,(int)point.Y-24,gold,.8f);
        }
        if(selected is not {Owner:0} u||!OriginalWorkerPolicy.Carrier(u.Type)||!VisualCatalog.Visible(u))return;
        Rectangle CellRect(int cell,int width=1,int height=1){var p=ToScreen((cell&255)*32,(cell>>8)*32);return new((int)p.X,(int)p.Y,(int)(width*32*zoom),(int)(height*32*zoom));}
        if(resourceGesture.Pending){var source=simulation.BuildingAt(resourceGesture.SourceCell);var rule=source==null?null:simulation.Rule(source.Type);Outline(CellRect(resourceGesture.SourceCell,rule?.Width??1,rule?.Height??1),Color.LightGreen,3);}
        if(u.FoodField>=0&&u.RepeatFood&&u.FoodJob!=0){
            int x=u.FoodField&255,y=u.FoodField>>8,r=Simulation.FoodFieldRadius;
            for(int dy=-r;dy<=r;dy++)for(int dx=-r;dx<=r;dx++){
                int nx=x+dx,ny=y+dy;if(Math.Abs(dx)+Math.Abs(dy)!=r||nx<0||ny<0||nx>=simulation.World.Width||ny>=simulation.World.Height)continue;
                Outline(CellRect(ny*256+nx),new Color(145,205,105,85));
            }
        }
        if(u.RockGround>=0)Outline(CellRect(u.RockGround),Color.LightSkyBlue,2);
        if(u.FoodSource>=0&&u.FoodJob!=0)Outline(CellRect(u.FoodSource),Color.LightGreen,2);
        if(u.FoodDestinationKind==1)Outline(CellRect(u.FoodDestination),Color.LightSkyBlue,2);
        else if(u.FoodDestinationKind==2&&simulation.World.Buildings.FirstOrDefault(b=>b.Slot==u.FoodDestination) is {} target){var rule=simulation.Rule(target.Type);Outline(CellRect(target.Cell,rule.Width,rule.Height),Color.LightSkyBlue,2);}
    }
    void PrepareMiningCapture(){
        int depot=ResearchTests.Build(simulation,3);SelectOnly(simulation.World.Units.First(u=>u.Type==7&&u.Owner==0));
        if(!simulation.SetRockDestination(selected!,depot)||!simulation.StartMining(selected!,0))throw new InvalidDataException("Mining capture order");
        for(int i=0;i<10000&&selected!.MineJob!=2;i++)simulation.Step();if(selected!.MineJob!=2)throw new InvalidDataException("Mining capture arrival");
        Center();message="Construtor na jazida. M: minerar. E: escolher entrega. B: construcoes.";
    }
    void PrepareHarvestCapture(){
        SelectOnly(simulation.World.Units.First(u=>u.Type==5&&u.Owner==0));
        int source=Enumerable.Range(0,65536).Where(i=>simulation.World.Objects[i] is >=29 and <=52).OrderBy(i=>Math.Abs((i&255)-selected!.X/1024)+Math.Abs((i>>8)-selected!.Y/1024)).First(i=>simulation.Gather(selected!,i));
        int drop=Enumerable.Range(0,65536).Where(i=>simulation.FoodDestinationError(selected!,1,i)==null).OrderBy(i=>Math.Abs((i&255)-(source&255))+Math.Abs((i>>8)-(source>>8))).First(i=>simulation.SetFoodDestination(selected!,1,i));
        camera=new Vector2((source&255)*32-ViewWidth/(2*zoom),(source>>8)*32-ViewHeight/(2*zoom));ClampCamera();
        message="Verde: area de coleta. Azul: destino escolhido para a comida.";
    }
    bool constructionMenu;
    int foodTargetMode;
    Rectangle ConstructionPanel=>new(20,Top+12,Math.Max(300,ViewWidth-40),Math.Max(300,ViewHeight-24));
    int[] ConstructionChoices=>Simulation.BuildData.Buildings.OrderBy(b=>simulation.BuildingUnlocked(b.Type)?0:1).ThenBy(b=>b.Type==3?-1:b.Type).Select(b=>b.Type).ToArray();
    Rectangle ConstructionCard(int index){var p=ConstructionPanel;int width=(p.Width-32)/4,height=(p.Height-118)/6;return new(p.X+16+(index%4)*width,p.Y+76+(index/4)*height,width-8,height-6);}
    Rectangle WorkerButton(int index){int y=MiniRect.Bottom+26+180;return new(ViewWidth+16+(index%2)*108,y+(index/2)*34,104,29);}
    void OpenConstructionMenu(){resourceOrders.Cancel();resourceGesture.Cancel();
        buildMode=false;foodTargetMode=0;dragStart=null;
        if(selected?.Type!=7||selected.Owner!=0)SelectOnly(simulation.World.Units.FirstOrDefault(u=>u.Owner==0&&u.Type==7&&VisualCatalog.Visible(u)));
        constructionMenu=false;viewedBuilding=-1;nativePage=0;
        message=selected!=null?"Escolha o icone da construcao na barra inferior.":"Voce precisa de um construtor.";
    }
    void UpdateConstructionMenu(KeyboardState k,MouseState m){
        if(Press(k,Keys.Escape)||Press(k,Keys.B)||m.RightButton==ButtonState.Pressed&&oldMouse.RightButton==ButtonState.Released){constructionMenu=false;return;}
        if(m.LeftButton!=ButtonState.Pressed||oldMouse.LeftButton!=ButtonState.Released)return;
        var types=ConstructionChoices;
        for(int i=0;i<types.Length;i++)if(ConstructionCard(i).Contains(m.Position)){
            int type=types[i];var error=simulation.BuildChoiceError(type);
            if(error!=null){message=simulation.BuildName(type)+": "+error+".";return;}
            buildType=type;buildMode=true;constructionMenu=false;message=simulation.BuildName(type)+": escolha o local. Botao direito cancela.";return;
        }
    }
    void StartMineOrder(){message="Segure Ctrl, clique na jazida e depois no destino, mantendo Ctrl.";}
    void StartFoodTarget(int kind){
        if(selected==null||selected.Owner!=0||!OriginalWorkerPolicy.Carrier(selected.Type))return;
        resourceGesture.Cancel();
        foodTargetMode=kind;buildMode=false;dragStart=null;
        message=kind==1?"Clique no chao para escolher onde deixar a carga.":"Clique no seu armazem para escolher a entrega.";
    }
    void ChooseFoodDestination(int cell,int kind){
        if(selected==null)return;
        if(!KnownCell(cell)){message="Explore o local antes de coletar ou entregar.";return;}
        var building=simulation.BuildingAt(cell);int target=kind==2?building?.Slot??-1:cell;
        if(selected.Type==7&&selected.Food==0&&selected.FoodJob==0){if(kind==1?simulation.SetRockGround(selected,cell):simulation.SetRockDestination(selected,target)){foodTargetMode=0;message="Destino da rocha definido. M: iniciar mineracao.";}else message="Escolha seu armazem ou um ponto livre no chao.";return;}
        string? error=simulation.FoodDestinationError(selected,kind,target);
        if(error!=null){message=error;return;}
        if(!simulation.SetFoodDestination(selected,kind,target)){message="Nao ha caminho para o destino escolhido.";return;}
        foodTargetMode=0;message="Destino: "+simulation.FoodDestinationName(selected)+". "+(selected.Food>0?"Entregando carga.":"Escolha o alimento para coletar.");
    }
    bool UpdateWorkerButtons(MouseState m){
        if(m.LeftButton!=ButtonState.Pressed||oldMouse.LeftButton!=ButtonState.Released||selected?.Owner!=0)return false;
        if(selected.Type==7){for(int i=0;i<4;i++)if(WorkerButton(i).Contains(m.Position)){switch(i){case 0:OpenConstructionMenu();break;case 1:message="Segure Ctrl e clique na fonte. Mantenha Ctrl e escolha a entrega.";break;case 2:StartFoodTarget(2);break;case 3:selected.RepeatMine=!selected.RepeatMine;selected.RepeatFood=selected.RepeatMine;message=selected.RepeatMine?"Mineracao continua ativada.":"Mineracao encerra apos a carga atual.";break;}return true;}return false;}
        if(!OriginalWorkerPolicy.Carrier(selected.Type))return false;
        for(int i=0;i<4;i++)if(WorkerButton(i).Contains(m.Position)){
            dragStart=null;
            switch(i){case 0:message="Segure Ctrl e clique na fonte. Mantenha Ctrl e escolha a entrega.";break;case 1:StartFoodTarget(2);break;case 2:StartFoodTarget(1);break;case 3:selected.RepeatFood=!selected.RepeatFood;message=selected.RepeatFood?"Repeticao ativada: plantas proximas, mesmo destino.":"Repeticao desativada: termina a carga atual.";break;}
            return true;
        }
        return false;
    }
    void DrawWorkerButtons(){
        if(selected?.Owner!=0)return;
        string[] labels=selected.Type==7?new[]{"B Construir","Ctrl Coletar","Destino",selected.RepeatMine?"Repetir: sim":"Repetir: nao"}:OriginalWorkerPolicy.Carrier(selected.Type)?new[]{"Ctrl Coletar","Destino","L No chao",selected.RepeatFood?"Repetir: sim":"Repetir: nao"}:Array.Empty<string>();
        for(int i=0;i<labels.Length;i++){var r=WorkerButton(i);batch.Draw(white,r,r.Contains(Mouse.GetState().Position)?new Color(61,77,62):new Color(37,49,43));Outline(r,gold);Text(labels[i],r.X+6,r.Y+4,gold,.72f);}
        if(selected.Type==7){var b=simulation.World.Buildings.FirstOrDefault(b=>b.Slot==selected.RockDestination);Text(selected.FoodJob!=0||selected.Food>0?Simulation.ResourceName(selected.CargoKind)+": "+simulation.FoodDestinationName(selected):selected.RockGround>=0?"Rocha: chao ("+(selected.RockGround&255)+", "+(selected.RockGround>>8)+")":b==null?"Ctrl: fonte, depois entrega":"Rocha: "+simulation.BuildName(b.Type),ViewWidth+16,WorkerButton(2).Bottom+6,muted,.65f);}
        if(OriginalWorkerPolicy.Carrier(selected.Type)&&selected.Type!=7){Text("Destino: "+simulation.FoodDestinationName(selected),ViewWidth+16,WorkerButton(2).Bottom+6,muted,.65f);}
    }
    void DrawConstructionMenu(){
        if(!constructionMenu)return;
        var p=ConstructionPanel;batch.Draw(white,View,new Color(0,0,0,170));batch.Draw(white,p,ink);Outline(p,gold,2);
        Text("CONSTRUCOES",p.X+18,p.Y+12,gold,1.1f);
        Text("Escolha uma estrutura. As bloqueadas mostram a pesquisa necessaria.",p.X+18,p.Y+44,muted,.75f);
        var types=ConstructionChoices;
        for(int i=0;i<types.Length;i++){
            int type=types[i];var r=ConstructionCard(i);var rule=simulation.Rule(type);bool unlocked=simulation.BuildingUnlocked(type);bool enabled=simulation.BuildChoiceError(type)==null;
            batch.Draw(white,r,enabled?new Color(40,61,48):new Color(31,39,38));Outline(r,r.Contains(Mouse.GetState().Position)?gold:enabled?new Color(102,125,83):new Color(62,72,67));
            Text(simulation.BuildName(type),r.X+9,r.Y+5,unlocked?gold:muted,.83f);
            Text(rule.Food+" alimento  "+rule.Rock+" rocha"+(rule.Parts>0?"  "+rule.Parts+" pecas":""),r.X+9,r.Y+28,muted,.62f);
            string requirement=enabled?"Construir":simulation.BuildChoiceError(type)!;
            float scale=Math.Min(.60f,(r.Width-72)/Math.Max(1,font.MeasureString(requirement).X));
            Text(requirement,r.X+9,r.Bottom-24,enabled?Color.LightGreen:muted,scale);
            DrawBuildingPreview(rule,new Rectangle(r.Right-55,r.Y+35,46,45),unlocked);
        }
        Text("B / Esc / botao direito: voltar",p.X+18,p.Bottom-29,muted,.72f);
    }
    void DrawBuildingPreview(BuildingRule rule,Rectangle area,bool unlocked){
        var commands=rule.Objects.SelectMany(o=>o.Commands).Where(c=>sprites.ContainsKey(c.Sprite)).ToArray();if(commands.Length==0)return;
        int left=commands.Min(c=>c.X),top=commands.Min(c=>c.Y),right=commands.Max(c=>c.X+sprites[c.Sprite].Width),bottom=commands.Max(c=>c.Y+sprites[c.Sprite].Height);
        float scale=Math.Min(area.Width/(float)Math.Max(1,right-left),area.Height/(float)Math.Max(1,bottom-top));
        foreach(var c in commands){var t=sprites[c.Sprite];var p=new Vector2(area.X+(area.Width-(right-left)*scale)/2+(c.X-left)*scale,area.Y+(area.Height-(bottom-top)*scale)/2+(c.Y-top)*scale);batch.Draw(t,p,null,unlocked?Color.White:new Color(120,135,125,140),0,Vector2.Zero,scale,Microsoft.Xna.Framework.Graphics.SpriteEffects.None,0);}
    }
}
