namespace RisingLands;
public static class ImprovementsV57Tests {
 static void Check(bool value,string name){if(!value)throw new Exception("FAIL: v57 "+name);Console.WriteLine("PASS: v57 "+name);}
 static void Until(Simulation s,Func<bool> predicate){for(int i=0;i<60000&&!predicate();i++)s.Step();Check(predicate(),"bounded resource cycle finishes");}
 public static void InspectPlayerSaves(string root){
  foreach(var path in Directory.GetFiles(Path.GetFullPath(PackageLayout.Asset(root,"../../UserSaves")),"*.json")){
   var bytes=File.ReadAllText(path);var save=System.Text.Json.JsonSerializer.Deserialize<Simulation.Save>(bytes)!;
   var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD",save.MapName));new VisualCatalog().Apply(w);var s=new Simulation(w);
   try{s.Restore(bytes);Console.WriteLine("PASS: player save loads: "+Path.GetFileName(path));}catch(Exception e){Console.WriteLine("ERROR: "+Path.GetFileName(path)+" "+e.Message);Environment.ExitCode=1;}
   Check(File.ReadAllText(path)==bytes,"read-only inspection preserves saved file");
  }
 }
 public static void Run(string root){
  Simulation Fixture(int type,int amount){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);w.Objects[5642]=(byte)(type==7?68:67);w.Quantities[5642]=(byte)amount;if(type==7){w.Objects[5643]=68;w.Quantities[5643]=6;}return new Simulation(w);}
  var s=Fixture(5,60);var farmer=s.World.Units.First(u=>u.Owner==0&&u.Type==5);int drop=Enumerable.Range(0,65536).Where(c=>s.GroundResourceSpace(c,1)&&s.World.Objects[c]==0&&Math.Abs((c&255)-10)+Math.Abs((c>>8)-22)>4).OrderBy(c=>Math.Abs((c&255)-10)+Math.Abs((c>>8)-22)).First();
  s.World.Objects[drop]=67;s.World.Quantities[drop]=30;s.World.Quantities[5642]-=30;var orders=new WorkerResourceOrders();Check(orders.Source(s,farmer,5642)&&orders.Destination(s,farmer,drop),"farmer receives ground delivery order");
  Until(s,()=>s.World.Quantities[drop]==31);Until(s,()=>Enumerable.Range(0,65536).Any(c=>c!=drop&&Math.Max(Math.Abs((c&255)-(drop&255)),Math.Abs((c>>8)-(drop>>8)))<=3&&s.World.Objects[c]==67&&s.World.Quantities[c]>0));
  Check(farmer.FoodDestination==drop,"ground spill preserves chosen delivery anchor");_=s.OrderPreview();
  var stone=Fixture(7,1);var builder=stone.World.Units.First(u=>u.Owner==0&&u.Type==7);int other=5643;
  int dest=Enumerable.Range(0,65536).Where(c=>stone.GroundResourceSpace(c,2)&&stone.World.Objects[c]==0&&Math.Abs((c&255)-10)+Math.Abs((c>>8)-22)>8).OrderBy(c=>Math.Abs((c&255)-10)+Math.Abs((c>>8)-22)).First();
  Check(orders.Source(stone,builder,5642)&&orders.Destination(stone,builder,dest),"builder receives loose stone cycle");Until(stone,()=>stone.World.Quantities[other]==0);Check(builder.FoodDestination==dest,"nearby stone search preserves delivery order");_=stone.OrderPreview();
  ExhaustedMine(root);SavePersistence(root);Formations(root);CultureDelivery(root);UnstaffedCrop(root);OverflowPriority(root);Console.WriteLine("ALL V57 TESTS PASSED");
 }
 static void ExhaustedMine(string root){
  var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);w.Mines[0].Quantity=1;var probe=new Simulation(w);int anchor=w.Mines[0].Cell;
  int stone=Enumerable.Range(0,65536).Where(c=>Simulation.CellDistance(c,anchor)<=6&&probe.GroundResourceSpace(c,2)&&w.Objects[c]==0).OrderBy(c=>Simulation.CellDistance(c,anchor)).First();w.Objects[stone]=68;w.Quantities[stone]=3;var s=new Simulation(w);var u=w.Units.First(a=>a.Owner==0&&a.Type==7);
  int drop=Enumerable.Range(0,65536).Where(c=>s.GroundResourceSpace(c,2)&&w.Objects[c]==0&&Simulation.CellDistance(c,anchor)>8).OrderBy(c=>Simulation.CellDistance(c,anchor)).First();var orders=new WorkerResourceOrders();Check(orders.Source(s,u,anchor)&&orders.Destination(s,u,drop),"one-stone mine receives persistent destination");Until(s,()=>w.Mines[0].Quantity==0&&w.Quantities[stone]==0);Check(u.FoodDestination==drop&&u.FoodField==anchor,"exhausted mine switches to nearby loose stone and preserves destination");_=s.OrderPreview();
 }
 static void Formations(string root){
  var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);var units=w.Units.Where(u=>u.Owner==0&&VisualCatalog.Visible(u)).ToArray();for(int i=0;i<units.Length;i++)units[i].Type=i<2?8:0;var sim=new Simulation(w);
  int anchor=Enumerable.Range(0,65536).Where(c=>(c&255)>6&&(c>>8)>6&&(c&255)<w.Width-6&&(c>>8)<w.Height-6).First(c=>Enumerable.Range(-5,11).All(dy=>Enumerable.Range(-5,11).All(dx=>sim.Passable((c&255)+dx,(c>>8)+dy))));
  var home=w.Buildings.First(b=>b.Owner==0&&b.Type==2);sim.ReleaseResident(home.Slot);Check(sim.EnterBuilding(units[0],home.Slot),"combat troop can have pending sanctuary entry before new order");
  foreach(var shape in Enum.GetValues<FormationShape>()){
   Check(sim.FormTroops(units,shape,anchor),"formation plans reachable positions for "+shape);Check(!sim.ResidentTrips.Any(t=>units.Any(u=>u.Slot==t.Slot)),"formation cancels previous sanctuary entry "+shape);var goals=units.ToDictionary(u=>u.Slot,u=>u.Route.Count>0?u.Route.Last():u.Y/1024*256+u.X/1024);Check(goals.Values.Distinct().Count()==units.Length,"formation reserves distinct destinations "+shape);
   double centerX=goals.Values.Average(c=>(double)(c&255)),centerY=goals.Values.Average(c=>(double)(c>>8));double Distance(int cell)=>Math.Pow((cell&255)-centerX,2)+Math.Pow((cell>>8)-centerY,2);
   Check(units.Where(u=>u.Type==8).Max(u=>Distance(goals[u.Slot]))<=units.Where(u=>u.Type==0).Min(u=>Distance(goals[u.Slot])),"greater range is central and melee is outside "+shape);
   Until(sim,()=>units.All(u=>u.Route.Count==0));Check(units.Select(u=>u.Y/1024*256+u.X/1024).Distinct().Count()==units.Length,"troops arrive without overlapping goals "+shape);_=sim.OrderPreview();
  }
  units[0].Owner=1;string before=sim.Serialize();Check(!sim.FormTroops(units,FormationShape.Line,anchor)&&sim.Serialize()==before,"enemy group rejection leaves orders unchanged");units[0].Owner=0;units[0].Type=5;Check(!sim.CanFormTroops(units),"worker in selection hides formation options");
  for(int count=1;count<=80;count++)foreach(var shape in Enum.GetValues<FormationShape>())Check(TroopFormations.Positions(count,shape).Distinct().Count()==count,"layout has unique positions "+shape+" "+count);
 }
 static void CultureDelivery(string root){
  var original=MissionOneTests.Load(root);var levels=original.Campaign.StartingLevels.ToArray();levels[0]=Enumerable.Range(0,20).First(l=>Simulation.TechnologyFor(l,levels[2],5).Carry>=1);var s=new Simulation(original.World,new CampaignProgress{StartingLevels=levels});int slot=ResearchTests.Build(s,4,false);var crop=s.World.Buildings.First(b=>b.Slot==slot);var home=s.World.Buildings.First(b=>b.Owner==0&&b.Type==2);var resident=s.ReleaseResident(home.Slot)!;Check(s.EnterBuilding(resident,slot),"cultivation explicitly receives colonist");Until(s,()=>s.ResidentTrips.Count==0);
  var f=s.World.Units.First(u=>u.Owner==0&&u.Type==5);var orders=new WorkerResourceOrders();int source=Enumerable.Range(0,65536).Where(s.IsFood).OrderBy(c=>Simulation.CellDistance(c,f.Y/1024*256+f.X/1024)).First();Check(orders.Source(s,f,source)&&orders.Destination(s,f,crop.Cell),"cultivation accepts source and delivery gesture");Until(s,()=>f.Food>0);s.Stop(f);
  // Use conserved existing stock to form a two-food delivery fixture.
  int transfer=2-f.Food;s.World.Resources[0]-=(ushort)transfer;f.Food=2;f.RepeatFood=false;Check(s.SetFoodDestination(f,2,slot),"loaded farmer targets crop");Until(s,()=>s.Culture(slot).Food==1);Check(f.Food==1,"first crop update unloads exactly one food");
  var copy=s.OrderPreview();for(int i=0;i<9;i++){s.Step();copy.Step();}Check(s.Culture(slot).Food==1&&f.Food==1,"crop unload waits between individual food items");s.Step();copy.Step();Check(s.Culture(slot).Food==2&&f.Food==0,"second crop item unloads after native ten-count reload");
  for(int i=0;i<60;i++){s.Step();copy.Step();}Check(s.Serialize()==copy.Serialize()&&s.Culture(slot).Food+s.Culture(slot).Consumed==2&&f.Food==0,"partial crop unloading survives save/load and conserves food");
 }
 static void UnstaffedCrop(string root){
  var s=MissionOneTests.Load(root);int slot=ResearchTests.Build(s,4,false);var crop=s.World.Buildings.First(b=>b.Slot==slot);var culture=s.Culture(slot);culture.Food=5;s.World.Resources[0]-=5;var farmer=s.World.Units.First(u=>u.Owner==0&&u.Type==5);Check(s.GatherNearest(farmer),"unstaffed crop receives real harvested food");Until(s,()=>farmer.Food>0);s.Stop(farmer);farmer.RepeatFood=false;Check(s.SetFoodDestination(farmer,2,slot),"unstaffed crop accepts food delivery as native code does");Until(s,()=>farmer.Food==0);Check(culture.Food==5&&culture.Consumed==1&&s.BornActors.Count==0,"native counter clamp consumes excess delivery without generating colonist in empty crop");_=s.OrderPreview();
 }
 static void OverflowPriority(string root){
  var s=MissionOneTests.Load(root);int slot=ResearchTests.Build(s,3);var depot=s.World.Buildings.First(b=>b.Slot==slot);var candidates=s.WarehouseOverflowCandidates(depot,2,0).ToArray();Check(candidates.Length>2,"warehouse has valid surrounding candidates");Check(candidates.Select(c=>s.WarehouseContourDistance(depot,c)).SequenceEqual(candidates.Select(c=>s.WarehouseContourDistance(depot,c)).Order()),"warehouse prioritizes contour distance over worker origin");
  int chosen=candidates.First();s.World.Objects[chosen]=68;s.World.Quantities[chosen]=30;var repeat=s.WarehouseOverflowCandidates(depot,2,65535).ToArray();Check(repeat.First()==chosen,"nearest-ring partial pile takes priority and is filled before another square");
 }
 static void SavePersistence(string root){var directory=Path.GetFullPath(PackageLayout.Asset(root,"../../reports/v57-save-tests/"+Guid.NewGuid().ToString("N")));var first=MissionOneTests.Load(root);var second=MissionTwoTests.Load(root);var a=new SaveSlots(directory,first.World.MapName,shared:true);a.Save(first,1);var b=new SaveSlots(directory,second.World.MapName,shared:true);b.Save(second,2);Check(File.Exists(a.PathFor(1))&&File.Exists(b.PathFor(2)),"saves remain on disk across missions");var restarted=new SaveSlots(directory,"MONDE03.ETA",shared:true);Check(restarted.Existing(1)!=null&&restarted.Existing(2)!=null&&restarted.Description(1).Contains("Missao 01")&&restarted.Description(2).Contains("Missao 02"),"global slots stay visible after restart in another mission");
  string before=restarted.Read(1);first.Step();a.Save(first,1);Check(File.ReadAllText(a.PathFor(1)+".bak")==before,"confirmed overwrite preserves previous save backup");File.WriteAllText(a.PathFor(1),"interrupted");Check(restarted.Read(1)==before,"backup recovers damaged current save");
  string legacyDir=directory+"-legacy";Directory.CreateDirectory(legacyDir);string old=Path.Combine(legacyDir,"MONDE03.ETA.slot-3.research-v21.json");second.SaveTo(old);var legacy=new SaveSlots(legacyDir,"MONDE01.ETA",shared:true);Check(legacy.Existing(3)==old&&File.ReadAllText(old)==second.Serialize(),"legacy per-mission slot is discovered without rewriting it");
  string collisionDir=directory+"-collision";Directory.CreateDirectory(collisionDir);first.SaveTo(Path.Combine(collisionDir,"MONDE01.ETA.slot-1.research-v21.json"));second.SaveTo(Path.Combine(collisionDir,"MONDE02.ETA.slot-1.research-v21.json"));var colliding=new SaveSlots(collisionDir,"MONDE03.ETA",shared:true);var mapped=Enumerable.Range(1,5).Select(colliding.Existing).Where(p=>p!=null).ToArray();Check(mapped.Length==2&&mapped.Distinct().Count()==2,"old slots with the same number in different missions both remain visible");}
}
