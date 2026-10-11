using Microsoft.Xna.Framework;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareV71Capture(){
  var s=SpecialV66Tests.Load(root,captureMenu!="v71-walls");LoadMap(restored:s);paused=true;revealMap=true;showMini=true;pauseMenu.Show(MenuPage.Closed);researchMovies.Clear();CloseResearchMovie();
  if(captureMenu=="v71-walls"){
   var a=s.World.Units.First(a=>a.Type==7&&a.Owner==0);
   int cell=Enumerable.Range(0,65536).Where(c=>(c&255)+5<s.World.Width&&(c>>8)+4<s.World.Height).OrderBy(c=>Math.Abs((c&255)-a.X/1024)+Math.Abs((c>>8)-a.Y/1024)).First(c=>Enumerable.Range(0,5).Select(i=>c+i).Concat(Enumerable.Range(1,4).Select(i=>c+4+i*256)).All(v=>s.PlacementError(a,9,v)==null));
   if(s.PlanWall(a,cell,cell+4)!=5)throw new InvalidDataException("Wall capture queue");for(int i=0;i<12000&&s.Constructions.Any(c=>!s.Complete(c));i++)s.Step();
   if(s.PlanWall(a,cell+4+256,cell+4+4*256)!=4)throw new InvalidDataException("Wall capture corner");for(int i=0;i<12000&&s.Constructions.Any(c=>!s.Complete(c));i++)s.Step();
   SelectOnly(a);camera=new Vector2((cell&255)*32-ViewWidth/2f+80,(cell>>8)*32-ViewHeight/2f+90);ClampCamera();message="Muralha continua: conexoes horizontais, verticais e canto originais.";
  }else{
   int home=ProductionV65Tests.Staff(s,16);var a=SpecialV66Tests.Recruit(s,10);var other=s.World.Buildings.First(b=>b.Type==16&&b.Owner==1);if(!s.SendMessenger(a,other.Slot))throw new InvalidDataException("Trade capture departure");for(int i=0;i<12000&&s.NegotiationFor(1)?.Stage!="Ready";i++)s.Step();var n=s.NegotiationFor(1)??throw new InvalidDataException("Trade capture arrival");camera=new Vector2((other.Cell&255)*32-ViewWidth/2f,(other.Cell>>8)*32-ViewHeight/2f);ClampCamera();
   if(captureMenu=="v71-market"){OpenNegotiation(n);marketGive[0]=10;marketTake[1]=8;var r=MarketRect(245,289,25,26);oldMouse=default;UpdateMarket(default,new Microsoft.Xna.Framework.Input.MouseState(r.Center.X,r.Center.Y,0,Microsoft.Xna.Framework.Input.ButtonState.Pressed,Microsoft.Xna.Framework.Input.ButtonState.Released,Microsoft.Xna.Framework.Input.ButtonState.Released,Microsoft.Xna.Framework.Input.ButtonState.Released,Microsoft.Xna.Framework.Input.ButtonState.Released));if(n.Stage!="Evaluating")throw new InvalidDataException("Trade confirm click failed");for(int i=0;i<100;i++)s.Step();}else message="Mensageiro chegou: clique na cartola animada para abrir o comercio.";
  }
 }
}
