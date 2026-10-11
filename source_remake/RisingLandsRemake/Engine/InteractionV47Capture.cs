namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareV47Capture(string mode){
  int slot=ResearchTests.Build(simulation,3);var b=simulation.World.Buildings.First(b=>b.Slot==slot);
  int type=mode=="arrival-stone"?7:mode=="arrival-engineer"?6:5;
  Actor actor;if(type==6){actor=simulation.World.Units.Where(a=>a.Type==5&&a.Owner==0).Last();actor.Type=6;}else actor=simulation.World.Units.First(a=>a.Type==type&&a.Owner==0);
  actor.RepeatFood=false;actor.RepeatMine=false;
  if(type==7)simulation.StartMining(actor,0);else if(type==6){actor.Food=1;actor.CargoKind=0;}else simulation.Gather(actor,5642);
  for(int i=0;i<20000&&actor.Food+actor.Rock==0;i++)simulation.Step();
  if(!(type==7?simulation.SetRockDestination(actor,slot):simulation.SetFoodDestination(actor,2,slot)))throw new InvalidDataException("Visible delivery capture route");
  int cell=type==7?actor.MineWorkCell:actor.DeliveryCell;var point=simulation.ExitPosition(b,cell);
  for(int i=0;i<20000;i++){if(actor.Route.Count==0&&Math.Abs(actor.X-point.X)<=64&&Math.Abs(actor.Y-point.Y)<=64)break;simulation.Step();}
  SelectOnly(actor);paused=true;Center();message="Entrega junto a arte do armazem; carga ainda visivel.";
 }
}
