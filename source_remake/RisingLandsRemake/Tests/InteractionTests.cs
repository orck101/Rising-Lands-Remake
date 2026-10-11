namespace RisingLands;
public static class InteractionTests {
 public static void Run(string root){
  void Check(bool ok,string text){if(!ok)throw new Exception("FAIL: "+text);Console.WriteLine("PASS: "+text);}
  var s=MissionOneTests.Load(root);var farmer=s.World.Units.First(u=>u.Type==5&&u.Owner==0);var gesture=new ResourceGesture();
  int ground=Enumerable.Range(0,65536).Where(c=>s.World.Objects[c]==0&&s.Passable(c&255,c>>8)&&s.BuildingAt(c)==null).OrderBy(c=>Math.Abs((c&255)-farmer.X/1024)+Math.Abs((c>>8)-farmer.Y/1024)).First();
  s.Order(farmer,ground&255,ground>>8);string before=s.Serialize();Check(gesture.Begin(s,new[]{farmer},5642)&&s.Serialize()==before,"first Ctrl click stages source without changing any simulation state");gesture.Cancel();Check(s.Serialize()==before&&!gesture.Pending,"Ctrl release cancels pending choice and preserves previous order");
  Check(gesture.Begin(s,new[]{farmer},5642),"valid food source restaged");Check(!gesture.Commit(s,5642)&&s.Serialize()==before&&gesture.Pending,"invalid destination preserves old order and pending source");
  Check(gesture.Commit(s,ground)&&!gesture.Pending&&farmer.FoodJob==1&&farmer.FoodDestinationKind==1,"second held-Ctrl click commits source and destination together");
  s=MissionOneTests.Load(root);int depot=ResearchTests.Build(s,3);farmer=s.World.Units.First(u=>u.Type==5&&u.Owner==0);farmer.RepeatFood=false;var building=s.World.Buildings.First(b=>b.Slot==depot);
  ground=Enumerable.Range(0,65536).Where(c=>s.World.Objects[c]==0&&s.Passable(c&255,c>>8)&&s.BuildingAt(c)==null).OrderBy(c=>Math.Abs((c&255)-farmer.X/1024)+Math.Abs((c>>8)-farmer.Y/1024)).First();
  var edge=s.OrderPreview();var edgeFarmer=edge.World.Units.First(u=>u.Slot==farmer.Slot);Check(edge.GatherWarehouse(edgeFarmer,depot),"warehouse pickup has reachable perimeter");Check(edge.SetFoodDestination(edgeFarmer,1,edgeFarmer.FoodSource),"warehouse cargo can be dropped on its free pickup perimeter cell");edge.Restore(edge.Serialize());
  int stock=s.World.Resources[0];Check(gesture.Begin(s,new[]{farmer},building.Cell)&&s.World.Resources[0]==stock,"warehouse source selection does not debit stock");
  Check(gesture.Commit(s,ground)&&s.World.Resources[0]==stock,"warehouse order confirmation does not debit stock before arrival");
  for(int i=0;i<10000&&farmer.Food==0;i++)s.Step();Check(farmer.Food==1&&s.World.Resources[0]==stock-1,"warehouse arrival moves exactly one food from global stock into cargo");
  var copy=MissionOneTests.Load(root);copy.Restore(s.Serialize());for(int i=0;i<10000&&farmer.Food>0;i++){s.Step();copy.Step();}
  Check(s.Serialize()==copy.Serialize()&&s.World.Objects[ground]==67&&s.World.Quantities[ground]==1,"warehouse cargo survives save/load and becomes a ground pile");
  foreach(int type in new[]{6,7}){
   var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);w.Resources[8]=4;var test=new Simulation(w);int dep=ResearchTests.Build(test,3);
   if(type==6){Check(test.Recruit(6),"sanctuary offers engineer with native configuration cost");for(int i=0;i<2000&&test.Recruitments.Any(j=>!j.Finished);i++)test.Step();}
   var worker=test.World.Units.First(u=>u.Owner==0&&u.Type==type);worker.RepeatFood=false;int index=Simulation.ResourceIndex(Simulation.WorkerResourceKind(worker));int previous=test.World.Resources[index];
   int target=Enumerable.Range(0,65536).First(c=>test.World.Objects[c]==0&&test.Passable(c&255,c>>8)&&test.BuildingAt(c)==null);
   Check(gesture.Begin(test,new[]{worker},test.World.Buildings.First(b=>b.Slot==dep).Cell)&&gesture.Commit(test,target),"role-specific warehouse order: "+type);
   for(int i=0;i<10000&&worker.Food==0;i++)test.Step();Check(worker.Food==1&&test.World.Resources[index]==previous-1&&worker.CargoKind==(type==6?0:2),"warehouse debits corresponding resource for worker "+type);
   test.Restore(test.Serialize());Check(true,"warehouse cargo validates conservation for worker "+type);
  }
  var barracks=MissionOneTests.Load(root);int barrack=ResearchTests.Build(barracks,1);
  Check(barracks.UnitUnlocked(0)&&!barracks.UnitUnlocked(8),"first mission barracks exposes swordsman and locks archer by technology");
  int home=barracks.World.Buildings.First(b=>b.Owner==0&&b.Type==2).Slot;int settlers=barracks.Settlers(home);
  Check(!barracks.Recruit(8,barrack)&&barracks.Recruit(0,barrack),"barracks rejects locked soldier and sends actual settler for available training");
  Check(barracks.Settlers(home)==settlers&&barracks.Recruitments.Last().SourceBuildingSlot==barrack,"barracks trains its own resident without silently draining sanctuary");
  var resumed=MissionOneTests.Load(root);resumed.Restore(barracks.Serialize());for(int i=0;i<10000&&barracks.Recruitments.Any(j=>!j.Finished);i++){barracks.Step();resumed.Step();}
  Check(barracks.Recruitments.All(j=>j.Finished)&&barracks.Serialize()==resumed.Serialize(),"barracks settler journey and soldier training survive save/load");
 }
}
