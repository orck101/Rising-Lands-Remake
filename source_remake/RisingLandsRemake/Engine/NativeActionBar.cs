using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 public static int UnitPortrait(int type)=>0x29cf+(type==25?18:type);
 Texture2D nativePlate=null!;int nativePage;bool showMini=true;
 string WarehouseHint(){var s=simulation.WarehouseSummary();return "Armazens: "+s.Count+" (ativos: "+s.ActiveCount+") | Estoque: "+s.Occupancy+" / "+s.Capacity+" | Livre: "+s.Free+" | "+s.Status;}
 record NativeAction(int Sprite,string Hint,string Kind,int Value,bool Enabled=true);
 float NativeScale=>GraphicsDevice.Viewport.Width/800f;
 int NativeTop=>GraphicsDevice.Viewport.Height-(int)Math.Ceiling(54*NativeScale);
 Rectangle NativeRect(int x,int y,int w,int h)=>new((int)(x*NativeScale),GraphicsDevice.Viewport.Height+(int)((y-600)*NativeScale),(int)Math.Ceiling(w*NativeScale),(int)Math.Ceiling(h*NativeScale));
 void NativeSprite(int id,int x,int y,Color? tint=null){if(sprites.TryGetValue(id,out var t))batch.Draw(t,NativeRect(x,y,t.Width,t.Height),tint??Color.White);}
 NativeAction[] NativeActions(){
  if(ViewedBuilding is {} b){
   if(b.Owner!=0)return Array.Empty<NativeAction>();
   if(b.Type==12)return new[]{new NativeAction(0x29c5,simulation.GatePassable(b.Cell)?"Fechar portao":"Abrir portao","gate",b.Slot)};
   if(b.Type==6)return Simulation.SpecialData.Spells.Where(r=>simulation.SpellUnlocked(r.Id)).Select(r=>new NativeAction(r.Sprite,r.Name+" - aprender: "+r.Food+" comida, "+r.Rock+" pedra","learn",r.Id,simulation.CanLearnSpell(r.Id,b.Slot))).ToArray();
   var recruits=BuildingRecruits(b).Where(simulation.UnitUnlocked).Select(type=>new NativeAction(UnitPortrait(type),RecruitName(type)+" - "+RecruitCost(type),"recruit",type,simulation.CanQueueRecruit(type,b.Slot))).ToArray();if(recruits.Length>0)return recruits;
   if(b.Type==8)return simulation.ExchangeOccupants(b.Slot).GroupBy(a=>a.Type).Select(g=>new NativeAction(UnitPortrait(g.Key),g.Count()+" "+RecruitName(g.Key)+" aguardando na Bolsa. Clique para retirar um.","exchange-release",g.Key)).ToArray();
   if(b.Type==4)return Array.Empty<NativeAction>();
   if(b.Type==7)return new[]{new NativeAction(0x29f0,"Pesquisa de "+ResearchBranches[simulation.ActiveResearchBranch],"research",0)};
   if(b.Type==3)return new[]{new NativeAction(0x29bc,WarehouseHint(),"info",0)};
   return Array.Empty<NativeAction>();
  }
  if(selected is {Owner:0,Type:17} balloon)return simulation.BalloonPassengers(balloon).GroupBy(a=>a.Type).Select(g=>new NativeAction(UnitPortrait(g.Key),g.Count()+" "+RecruitName(g.Key)+" a bordo. Clique para desembarcar.","balloon-unload",g.Key,simulation.CanUnloadBalloon(balloon,g.Key))).ToArray();
  if(selected is {Owner:0,Type:4} druid)return Simulation.SpecialData.Spells.Where(r=>simulation.SpellLearned(r.Id)).Select(r=>new NativeAction(r.Sprite,r.Name+" - "+r.Mana+" mana (atual: "+druid.Mana+")","spell",r.Id,druid.Mana>=r.Mana&&druid.PetrifiedUntil<=simulation.Tick)).ToArray();
  if(selected is {Owner:0,Type:7})return Simulation.BuildData.Buildings.Where(r=>simulation.BuildingUnlocked(r.Type)).OrderBy(r=>r.Type).Select(r=>new NativeAction(0x29b9+r.Type,simulation.BuildName(r.Type)+" - "+r.Food+" alimento, "+r.Rock+" rocha"+(r.Parts>0?", "+r.Parts+" engr.":""),"build",r.Type,simulation.BuildChoiceError(r.Type)==null)).ToArray();
  
  return Array.Empty<NativeAction>();
 }
 bool UpdateNativeActions(KeyboardState k,MouseState m){
  if(Press(k,Keys.Tab))showMini=!showMini;
  bool left=m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released;bool right=m.RightButton==ButtonState.Pressed&&oldMouse.RightButton==ButtonState.Released;
  if((!left&&!right)||m.Y<NativeTop)return false;
  if(right){var cancelActions=NativeActions();for(int i=0;i<4;i++){int index=nativePage*4+i;if(index<cancelActions.Length&&cancelActions[index].Kind=="recruit"&&NativeRect(31+60*i,547,58,52).Contains(m.Position)){message=simulation.CancelQueuedRecruit(cancelActions[index].Value,viewedBuilding)?"Unidade retirada da fila. Custo devolvido.":"Nenhuma unidade pendente cancelavel; treinamento atual preservado.";return true;}}return false;}
  if(UpdateFormationBar(m))return true;
  if(UpdateSelectionClasses(m))return true;
  var actions=NativeActions();nativePage=Math.Clamp(nativePage,0,Math.Max(0,(actions.Length-1)/4));
  if(NativeRect(8,547,21,52).Contains(m.Position)){nativePage=Math.Max(0,nativePage-1);return true;}
  if(NativeRect(272,547,21,52).Contains(m.Position)){nativePage=Math.Min(Math.Max(0,(actions.Length-1)/4),nativePage+1);return true;}
  for(int i=0;i<4;i++){int index=nativePage*4+i;if(index>=actions.Length||!NativeRect(31+60*i,547,58,52).Contains(m.Position))continue;var a=actions[index];
   if(!a.Enabled){message="Sem recursos, colono disponivel ou producao ocupada.";return true;}
   switch(a.Kind){
    case "balloon-unload":message=selected!=null&&simulation.UnloadBalloon(selected,a.Value)>0?"Passageiros desembarcados. Cargas preservadas.":"Pouse sobre terreno livre para desembarcar.";break;
    case "gate":message=simulation.ToggleGate(a.Value)?"Portao acionado.":"Aguarde o portao terminar ou a passagem ficar livre.";break;
    case "learn":message=simulation.LearnSpell(a.Value,viewedBuilding)?"Aprendizado iniciado na biblioteca.":"Aprendizado indisponivel.";break;
    case "spell":spellChoice=a.Value;message=simulation.Spell(a.Value).Name+": clique no alvo. Botao direito cancela.";break;
    case "market":OpenMarket(a.Value);break;
    case "recruit":message=simulation.QueueRecruit(a.Value,viewedBuilding)?"Adicionado ao final da fila. Custo pago.":"Fila indisponivel: ocupacao, tecnologia ou recursos.";break;
    case "build":resourceGesture.Cancel();buildType=a.Value;buildMode=true;constructionMenu=false;dragStart=null;message=simulation.BuildName(a.Value)+": escolha o terreno verde.";break;
    case "stop":foreach(var u in SelectedUnits())simulation.Stop(u);resourceGesture.Cancel();message="Ordens interrompidas.";break;
    case "research":var slot=viewedBuilding;viewedBuilding=-1;OpenResearch();laboratoryIndex=Array.FindIndex(PlayerLaboratories,b=>b.Slot==slot);break;
    case "exchange-release":message=simulation.ReleaseExchangeUnit(viewedBuilding,a.Value)!=null?"Personagem retirado da Bolsa. Selecione-o para dar ordens.":"Sem unidade disponível ou saída livre.";break;
    case "info":message=a.Hint;break;
   }return true;
  }return true;
 }
 void PrepareCooperativeCapture(){
  if(!simulation.Recruit(7))throw new InvalidDataException("Capture cannot recruit second builder");for(int i=0;i<2000&&simulation.Recruitments.Any(j=>!j.Finished);i++)simulation.Step();
  var workers=simulation.World.Units.Where(u=>u.Owner==0&&u.Type==7).ToArray();var first=workers[0];int cell=Enumerable.Range(0,65536).OrderBy(c=>Math.Abs((c&255)-first.X/1024)+Math.Abs((c>>8)-first.Y/1024)).First(c=>simulation.PlacementError(first,3,c)==null);
  if(!simulation.StartConstruction(first,3,cell))throw new InvalidDataException("Capture shared site");var site=simulation.Constructions.Single();SelectOnly(workers[1]);
  var point=ToScreen((cell&255)*32+16,(cell>>8)*32+16);if(!JoinConstructionAt(new Point((int)point.X,(int)point.Y)))throw new InvalidDataException("Capture shared-site click");
  for(int i=0;i<10000&&workers.Any(u=>u.Route.Count>0);i++)simulation.Step();site.Progress=simulation.Rule(3).Duration/2;
  foreach(var work in Simulation.Workers(site)){work.AnimationTick=4;work.StrikeCooldown=16;}
  camera=new Vector2((cell&255)*32-ViewWidth/(2*zoom)+80,(cell>>8)*32-ViewHeight/(2*zoom)+80);ClampCamera();message="Dois construtores trabalhando na mesma obra.";
 }
 void VerifyNativeRecruitClick(){
  viewedBuilding=simulation.World.Buildings.First(b=>b.Type==2&&b.Owner==0).Slot;nativePage=0;selected=null;selectedSlots.Clear();
  var r=NativeRect(31,547,58,52);oldMouse=default;
  var m=new MouseState(r.Center.X,r.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released);
  if(!UpdateNativeActions(new KeyboardState(),m)||!simulation.Recruitments.Any(j=>j.Type==5&&j.BuildingSlot==viewedBuilding))throw new InvalidDataException("Native farmer icon click failed");oldMouse=default;
 }
 void NativeNumber(int value,int x,int y){string number=Math.Clamp(value,0,999).ToString().PadLeft(3,'0');batch.Draw(white,NativeRect(x,y,36,15),Color.Black);for(int i=0;i<3;i++)NativeSprite(0x2a22+(number[i]-'0')*2,x+i*12,y);}
 void DrawNativeActionBar(){
  batch.Draw(nativePlate,new Rectangle(0,NativeTop,GraphicsDevice.Viewport.Width,GraphicsDevice.Viewport.Height-NativeTop),new Rectangle(0,546,800,54),Color.White);
  var actions=NativeActions();nativePage=Math.Clamp(nativePage,0,Math.Max(0,(actions.Length-1)/4));var pointer=Mouse.GetState().Position;string hint=message;
  if(ClassSelectionBar){hint=DrawSelectionClasses(pointer,hint);hint=DrawFormationBar(pointer,hint);}else{
  NativeSprite(nativePage>0?0x2a37:0x29b5,8,547);NativeSprite((nativePage+1)*4<actions.Length?0x2a38:0x29b6,272,547);
  for(int i=0;i<4;i++){
   int index=nativePage*4+i,x=31+i*60;if(index>=actions.Length){NativeSprite(0x29b1+i,x,547);continue;}
   var a=actions[index];NativeSprite(0x29b7,x,547);if(a.Enabled)NativeSprite(a.Sprite,x+2,549);else if(sprites.TryGetValue(a.Sprite,out var disabled))batch.Draw(GrayTexture(a.Sprite,disabled),NativeRect(x+2,549,disabled.Width,disabled.Height),Color.White);
   if(a.Kind=="recruit"&&simulation.Recruitments.FirstOrDefault(j=>j.BuildingSlot==viewedBuilding&&j.Type==a.Value&&!j.Finished) is {} job){int progress=job.Progress*100/simulation.TrainingRule(job.Type).Duration;NativeSprite(0x2a1d-progress*42/100,x+2,549,new Color(0,0,0,150));}
   if(a.Kind=="recruit"){int count=simulation.TrainingCount(viewedBuilding,a.Value);if(count>0){var pos=NativeRect(x+31,581,25,15);Text(count.ToString(),pos.X,pos.Y,Color.White,.48f*NativeScale);}}
   if(a.Kind=="learn"&&simulation.Special.Learning.FirstOrDefault(j=>j.Building==viewedBuilding&&j.Spell==a.Value&&!j.Finished) is {} learning){int progress=learning.Progress/simulation.Spell(a.Value).Learning;NativeSprite(0x2a1d-progress*42/100,x+2,549,new Color(0,0,0,150));if(NativeRect(x,547,58,52).Contains(pointer))hint="Aprendendo "+simulation.Spell(a.Value).Name+": "+progress+"%";}
   if(NativeRect(x,547,58,52).Contains(pointer)){NativeSprite(0x2a36,x+2,549);hint=a.Kind=="learn"&&simulation.Special.Learning.FirstOrDefault(j=>j.Building==viewedBuilding&&j.Spell==a.Value) is {} study?simulation.Spell(a.Value).Name+(study.Finished?" - aprendido":" - aprendendo: "+study.Progress/simulation.Spell(a.Value).Learning+"%"):a.Hint;}
  }
  }
  NativeNumber(simulation.World.Resources[8],504,557);NativeNumber(simulation.World.Resources[0],552,557);NativeNumber(simulation.World.Resources[4],600,557);
  if(ViewedBuilding is {} b){if(simulation.Inhabited(b))NativeSprite(0x29b9+b.Type,326,549);else if(sprites.TryGetValue(0x29b9+b.Type,out var portrait))batch.Draw(GrayTexture(0x29b9+b.Type,portrait),NativeRect(326,549,portrait.Width,portrait.Height),Color.White);hint=pointer.Y<NativeTop?(b.Type==8?"Bolsa: "+simulation.ExchangeOccupants(b.Slot).Count()+" aguardando | Vagas: "+Math.Max(0,simulation.ResidentCapacity(b)-simulation.Occupants(b.Slot)-simulation.ExchangeReserved(b.Slot)):b.Type==3?WarehouseHint():b.Type==4?simulation.CropStatus(b):simulation.BuildName(b.Type)):hint;}
  else if(!ClassSelectionBar&&selected is {Type:17})DrawBalloonPortrait(326,549);else if(!ClassSelectionBar&&selected!=null&&selected.Type<19)NativeSprite(UnitPortrait(selected.Type),326,549);
  if(selected is {Type:17} selectedBalloon&&pointer.Y<NativeTop)hint=simulation.BalloonStatus(selectedBalloon);
  if(hint.Length>0){float scale=Math.Min(.72f,(GraphicsDevice.Viewport.Width-40)/Math.Max(1,font.MeasureString(hint).X));int width=Math.Min(GraphicsDevice.Viewport.Width-24,(int)(font.MeasureString(hint).X*scale)+16);batch.Draw(white,new Rectangle(12,NativeTop-30,width,26),new Color(0,0,0,190));Text(hint,20,NativeTop-27,Color.Wheat,scale);}
  if(showMini){UpdateMiniFog();batch.Draw(mini,MiniRect,Color.White);Outline(MiniRect,gold);DrawMinimapBuildings();var w=simulation.World;var vr=new Rectangle(MiniRect.X+(int)(camera.X/(w.Width*32)*MiniRect.Width),MiniRect.Y+(int)(camera.Y/(w.Height*32)*MiniRect.Height),Math.Min(MiniRect.Width,(int)(ViewWidth/zoom/(w.Width*32)*MiniRect.Width)),Math.Min(MiniRect.Height,(int)(ViewHeight/zoom/(w.Height*32)*MiniRect.Height)));Outline(vr,Color.LightGoldenrodYellow);foreach(var u in w.Units.Where(u=>VisualCatalog.Visible(u)&&SeenUnit(u)))batch.Draw(white,new Rectangle(MiniRect.X+(int)(u.X/1024f/w.Width*MiniRect.Width),MiniRect.Y+(int)(u.Y/1024f/w.Height*MiniRect.Height),2,2),MinimapColors.ForOwner(u.Owner));}
 }
}

