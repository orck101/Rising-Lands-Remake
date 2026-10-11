using Microsoft.Xna.Framework.Input;
using System.Text.Json;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareBalloonCapture(){
  pauseMenu.Show(MenuPage.Closed);paused=true;revealMap=false;
  var col=simulation.World.Units.First(a=>a.Owner==0&&a.Type==9&&VisualCatalog.Visible(a));var port=simulation.World.Buildings.Where(b=>b.Type==18).OrderBy(b=>Math.Abs((b.Cell&255)-col.X/1024)+Math.Abs((b.Cell>>8)-col.Y/1024)).First();if(!simulation.EnterBuilding(col,port.Slot))throw new InvalidDataException("Capture: heliport entry failed");for(int i=0;i<1500&&!simulation.Ocean.Balloons.Any(a=>a.Owner==0);i++)simulation.Step();
  var balloon=simulation.World.Units.First(a=>a.Owner==0&&a.Type==17);var builder=simulation.World.Units.First(a=>a.Owner==0&&a.Type==7);SelectOnly(builder);Center();if(!HandleBalloonPointer(ActorBounds(balloon).Center)||!simulation.WaitingForFlight(builder))throw new InvalidDataException("UI boarding click failed");for(int i=0;i<2000&&simulation.BalloonPassengers(balloon).Count==0;i++)simulation.Step();if(simulation.BalloonPassengers(balloon).Count!=1)throw new InvalidDataException("UI passenger did not board");SelectOnly(balloon);var actions=NativeActions();if(actions.Length!=1||actions[0].Value!=7||!actions[0].Enabled)throw new InvalidDataException("Passenger portrait action incorrect");
  var boarded=simulation.Serialize();var box=NativeRect(31,547,58,52);oldMouse=new MouseState();var click=new MouseState(box.Center.X,box.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released);if(!UpdateNativeActions(default,click)||simulation.BalloonPassengers(balloon).Count!=0)throw new InvalidDataException("Passenger portrait click did not unload");
  simulation.Restore(boarded);balloon=simulation.World.Units.First(a=>a.Owner==0&&a.Type==17);SelectOnly(balloon);Center();oldMouse=new();message=simulation.BalloonStatus(balloon)+". Clique no terreno para transportar; retrato para desembarcar.";
  if(capture!=null)File.WriteAllText(Path.ChangeExtension(capture,".json"),JsonSerializer.Serialize(new{Passed=true,BoardingPointerVerified=true,PassengerPortraitVerified=true,UnloadClickVerified=true,Limit="Game input handlers invoked for graphical capture; not a manually played campaign."},new JsonSerializerOptions{WriteIndented=true}));
 }
}
