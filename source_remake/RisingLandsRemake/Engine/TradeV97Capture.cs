using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareTradeV97Capture(){
  var s=TradeV97Tests.Fixture(root);LoadMap(restored:s);paused=true;revealMap=true;pauseMenu.Show(MenuPage.Closed);researchMovies.Clear();CloseResearchMovie();
  var actor=SpecialV66Tests.Recruit(s,10);var b=s.World.Buildings.First(b=>b.Owner==1&&b.Type==16);SelectOnly(actor);
  // Synthetic test building: supply its real native art to the scene hit tester.
  var commands=s.Rule(b.Type).Objects.SelectMany(o=>o.Commands).Select(c=>c with{X=c.X+(b.Cell&255)*32,Y=c.Y+(b.Cell>>8)*32}).ToList();scene.Objects.Add(new(b.Cell,commands));
  camera=new Vector2((b.Cell&255)*32-ViewWidth/2f,(b.Cell>>8)*32-ViewHeight/2f);ClampCamera();
  Point? click=null;
  foreach(var c in commands){if(!sprites.TryGetValue(c.Sprite,out var t))continue;for(int y=c.Y;y<c.Y+t.Height;y+=4)for(int x=c.X;x<c.X+t.Width;x+=4){if(s.BuildingAt(y/32*256+x/32)?.Slot==b.Slot)continue;var p=ToScreen(x,y);var pointer=new Point((int)p.X,(int)p.Y);if(View.Contains(pointer)&&BuildingHit(pointer)?.Slot==b.Slot){click=pointer;break;}}if(click!=null)break;}
  if(click==null)throw new InvalidDataException("No native market art outside logical footprint for pointer regression");
  if(!HandleSpecialPointer(click.Value)||s.NegotiationFor(1)==null||actor.Route.Count==0)throw new InvalidDataException("Clicking native market art must preserve messenger route");
  for(int i=0;i<20000&&s.NegotiationFor(1)?.Stage!="Ready";i++)s.Step();var n=s.NegotiationFor(1)!;if(n.Stage!="Ready")throw new InvalidDataException("Market door arrival failed");
  OpenNegotiation(n);marketGive[0]=10;marketTake[1]=8;var confirm=MarketRect(245,289,25,26);oldMouse=default;UpdateMarket(default,new MouseState(confirm.Center.X,confirm.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released));
  if(n.Stage!="Evaluating")throw new InvalidDataException("Offer confirmation failed");long tick=s.Tick;paused=false;AdvanceSimulation();paused=true;if(s.Tick<=tick)throw new InvalidDataException("Evaluation must advance while market panel is open");
  Console.WriteLine("PASS v97 UI: sprite click outside footprint, real arrival, confirm button and live evaluation");
 }
}
