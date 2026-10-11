namespace RisingLands;
public static class InteractionV47Tests {
 static void Check(bool ok,string text){if(!ok)throw new Exception("FAIL: "+text);Console.WriteLine("PASS: "+text);}
 public static void Run(string root){
  foreach(int type in new[]{5,6,7}){
   var world=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(world);
   if(type==6)world.Units.Where(a=>a.Type==5&&a.Owner==0).Last().Type=6;
   if(type==6){world.Objects[5642]=66;world.Quantities[5642]=1;}
   var s=new Simulation(world);int slot=ResearchTests.Build(s,3);var building=s.World.Buildings.First(b=>b.Slot==slot);var worker=world.Units.First(a=>a.Type==type&&a.Owner==0);worker.RepeatFood=false;worker.RepeatMine=false;
   Check(type==7?s.StartMining(worker,0):s.Gather(worker,5642),"worker "+type+" acquires actual cargo before delivery");
   for(int i=0;i<20000&&worker.Food+worker.Rock==0;i++)s.Step();Check(worker.Food+worker.Rock==1,"worker "+type+" has one cargo item");
   int resource=Simulation.ResourceIndex(Simulation.WorkerResourceKind(worker)),before=world.Resources[resource];
   Check(type==7?s.SetRockDestination(worker,slot):s.SetFoodDestination(worker,2,slot),"worker "+type+" routes to visible warehouse entrance");
   int arrival=type==7?worker.MineWorkCell:worker.DeliveryCell;Check(s.FrontCells(building).Contains(arrival),"delivery "+type+" uses art approach rather than footprint perimeter");
   for(int i=0;i<20000&&world.Resources[resource]==before;i++){s.Step();if(world.Resources[resource]!=before){var pos=s.ExitPosition(building,arrival);Check(worker.X==pos.X&&worker.Y==pos.Y,"worker "+type+" credits stock only at exact visible arrival point");}}
   Check(world.Resources[resource]==before+1&&worker.Food+worker.Rock==0,"worker "+type+" delivers cargo once");
   string saved=s.Serialize();s.Restore(saved);worker=world.Units.First(a=>a.Slot==worker.Slot);Check(s.Serialize()==saved,"visual approach state "+type+" survives save/load");
   Check(s.GatherWarehouse(worker,slot),"worker "+type+" withdraws from same visible warehouse entrance");
   int stock=world.Resources[resource];for(int i=0;i<20000&&world.Resources[resource]==stock;i++)s.Step();var pickup=s.ExitPosition(building,worker.FoodSource);
   Check(world.Resources[resource]==stock-1&&worker.X==pickup.X&&worker.Y==pickup.Y&&worker.Food==1,"warehouse debits worker "+type+" only after reaching visible entrance");
  }
  var first=MissionOneTests.Load(root);var home=first.World.Buildings.First(b=>b.Type==2&&b.Owner==0);var colono=first.ReleaseResident(home.Slot)!;
  Check(colono.Y/32+24==(home.Cell>>8)*32+78+24,"colonist feet are twenty-four pixels below native sanctuary doorway");
  Check(first.EnterBuilding(colono,home.Slot),"colonist can return through visible entrance");int goal=first.ResidentTrips.Single().Cell;
  for(int i=0;i<20000&&first.ResidentTrips.Count>0;i++)first.Step();var end=first.ExitPosition(home,goal);
  Check(colono.LegacyState==14&&colono.X==end.X&&colono.Y==end.Y,"colonist hides only after reaching building artwork");
 }
}
