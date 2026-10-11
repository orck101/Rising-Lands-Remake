using Microsoft.Xna.Framework;
namespace RisingLands;
public sealed partial class ResearchGame {
 Point? interactionCapturePointer;
 void PrepareInteractionCapture(){
  var home=simulation.World.Buildings.First(b=>b.Type==2&&b.Owner==0);int depot=ResearchTests.Build(simulation,3);var building=simulation.World.Buildings.First(b=>b.Slot==depot);
  if(captureMenu=="entry-hover"){
   var colonist=simulation.ReleaseResident(home.Slot)??throw new InvalidDataException("No capture colonist");SelectOnly(colonist);if(!simulation.CanEnterBuilding(colonist,depot))throw new InvalidDataException("No valid entry cursor");
  }else{
   var farmer=simulation.World.Units.First(a=>a.Type==5&&a.Owner==0);SelectOnly(farmer);simulation.GatherNearest(farmer);for(int i=0;i<10000&&farmer.Food==0;i++)simulation.Step();if(farmer.Food==0)throw new InvalidDataException("No loaded farmer");
   var pointer=ToScreen((building.Cell&255)*32+16,(building.Cell>>8)*32+16);int cargo=farmer.Food;
   if(!HandleResourceClick(new Point((int)pointer.X,(int)pointer.Y),true)||farmer.FoodDestination!=depot||farmer.Food!=cargo)throw new InvalidDataException("Loaded warehouse UI redirect failed");
  }
  camera=new Vector2((building.Cell&255)*32-ViewWidth/(2*zoom),(building.Cell>>8)*32-ViewHeight/(2*zoom));ClampCamera();var p=ToScreen((building.Cell&255)*32+16,(building.Cell>>8)*32+16);interactionCapturePointer=new((int)p.X,(int)p.Y);
 }
}
