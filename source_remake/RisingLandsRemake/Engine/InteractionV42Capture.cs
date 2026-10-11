using Microsoft.Xna.Framework;
namespace RisingLands;
public sealed partial class ResearchGame {
 void VerifyV42Capture(){
  var home=simulation.World.Buildings.First(b=>b.Owner==0&&b.Type==2);
  if(captureMenu=="warehouse-clean"){
   viewedBuilding=ResearchTests.Build(simulation,3);selected=null;selectedSlots.Clear();
   var actions=NativeActions();if(actions.Length!=1||actions[0].Sprite!=0x29bc||actions[0].Hint!=WarehouseHint())throw new InvalidDataException("Warehouse must show only network information action");message="";return;
  }
  var colonists=new List<Actor>();for(int i=0;i<3;i++)colonists.Add(simulation.ReleaseResident(home.Slot)??throw new InvalidDataException("Front capture release"));
  if(captureMenu=="group-entry"){
   selected=colonists[0];selectedSlots.Clear();foreach(var actor in colonists)selectedSlots.Add(actor.Slot);
   var p=ToScreen((home.Cell&255)*32+16,(home.Cell>>8)*32+16);if(!OpenBuildingAt(new Point((int)p.X,(int)p.Y))||simulation.ResidentTrips.Count!=3)throw new InvalidDataException("Single building click must send all selected colonists");
   for(int i=0;i<10000&&simulation.ResidentTrips.Count>0;i++)simulation.Step();if(simulation.Occupants(home.Slot)!=8)throw new InvalidDataException("Group capture arrival");
  }
  selected=null;selectedSlots.Clear();viewedBuilding=home.Slot;message=captureMenu=="group-entry"?"Os tres colonos entraram com um unico clique.":"Tres colonos retirados pela frente do santuario.";
 }
}
