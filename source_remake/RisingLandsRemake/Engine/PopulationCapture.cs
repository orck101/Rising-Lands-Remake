using Microsoft.Xna.Framework;
namespace RisingLands;
public sealed partial class ResearchGame {
 void VerifyPopulationClicks(){
  var home=simulation.World.Buildings.First(b=>b.Type==2&&b.Owner==0);int before=simulation.Residents(home.Slot);var p=ToScreen((home.Cell&255)*32+16,(home.Cell>>8)*32+16);var pointer=new Point((int)p.X,(int)p.Y);
  lastBuildingClick=-1;if(!OpenBuildingAt(pointer)||!OpenBuildingAt(pointer)||selected!=null||selectedSlots.Count!=0||simulation.Residents(home.Slot)!=before-1)throw new InvalidDataException("Double-click must release real colonist");
  var settler=simulation.World.Units.Single(a=>a.Type==9&&a.Owner==0&&a.LegacyState==7);int slot=ResearchTests.Build(simulation,3,false);var depot=simulation.World.Buildings.First(b=>b.Slot==slot);if(simulation.Inhabited(depot))throw new InvalidDataException("New warehouse must be empty");
  SelectOnly(settler);p=ToScreen((depot.Cell&255)*32+16,(depot.Cell>>8)*32+16);pointer=new Point((int)p.X,(int)p.Y);lastBuildingClick=-1;
  if(!OpenBuildingAt(pointer)||!simulation.ResidentTrips.Any(j=>j.Slot==settler.Slot))throw new InvalidDataException("Building click must send selected colonist inside");
  for(int i=0;i<10000&&simulation.ResidentTrips.Count>0;i++)simulation.Step();if(!simulation.Inhabited(depot))throw new InvalidDataException("Resident arrival must activate warehouse");
  simulation.ReleaseResident(slot);viewedBuilding=slot;selected=null;selectedSlots.Clear();message="Armazem vazio: coloque um colono para ativar.";
 }
}
