using System.Text.Json.Nodes;
namespace RisingLands;
public static class MissionTwoTests {
 public static Simulation Load(string root){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE02.ETA"));new VisualCatalog().Apply(w);return new(w);}
 static void Check(bool value,string name){if(!value)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
 static string WithoutHungerMeters(string save){var j=JsonNode.Parse(save)!;foreach(var a in j["Units"]!.AsArray()){a!.AsObject().Remove("Satiety");a.AsObject().Remove("SatietyMaximum");a.AsObject().Remove("FatigueClock");}return j.ToJsonString();}
 public static void PlayToVictory(Simulation s){
  var builder=s.World.Units.First(u=>u.Owner==0&&u.Type==7&&VisualCatalog.Visible(u));var farmer=s.World.Units.First(u=>u.Owner==0&&u.Type==5&&VisualCatalog.Visible(u));
  int Build(int type){foreach(int cell in Enumerable.Range(0,65536).Where(Simulation.InSecondMissionZone).OrderBy(c=>Math.Abs((c&255)-builder.X/1024)+Math.Abs((c>>8)-builder.Y/1024)))if(s.StartConstruction(builder,type,cell)){var site=s.Constructions.Last();for(int i=0;i<20000&&!s.Complete(site);i++)s.Step();if(!s.Complete(site))throw new Exception("Mission 2 build stalled");return site.Id;}throw new Exception("Mission 2 no valid construction site type "+type);}
  int depot=Build(3);var colono=s.World.Units.First(u=>u.Owner==0&&u.Type==9&&VisualCatalog.Visible(u));if(!s.EnterBuilding(colono,depot))throw new Exception("Mission 2 warehouse entry failed");for(int i=0;i<10000&&!s.Inhabited(s.World.Buildings.First(b=>b.Slot==depot));i++)s.Step();
  int delivery=s.World.Buildings.First(b=>b.Slot==depot).Cell;var orders=new WorkerResourceOrders();
  void Collect(int food,int rock){
   for(int tick=0;tick<400000&&(s.World.Resources[0]<food||s.World.Resources[4]<rock);tick++){
    if(s.World.Resources[0]<food&&farmer.Food==0&&farmer.FoodJob is 0 or 4&&tick%30==0){bool found=false;foreach(int cell in Enumerable.Range(0,65536).Where(s.IsFood).OrderBy(c=>Math.Abs((c&255)-farmer.X/1024)+Math.Abs((c>>8)-farmer.Y/1024)))if(orders.Source(s,farmer,cell)&&orders.Destination(s,farmer,delivery)){found=true;break;}if(!found)throw new Exception("Mission 2 food inaccessible");}
    if(s.World.Resources[4]<rock&&builder.Rock==0&&builder.Food==0&&(builder.MineJob is 0 or 4)&&(builder.FoodJob is 0 or 4)&&tick%30==0){bool found=false;foreach(int cell in s.World.Mines.Where(m=>m.Quantity>0).Select(m=>m.Cell).Concat(Enumerable.Range(0,65536).Where(c=>s.World.Objects[c]==68&&s.World.Quantities[c]>0)).OrderBy(c=>Math.Abs((c&255)-builder.X/1024)+Math.Abs((c>>8)-builder.Y/1024)))if(orders.Source(s,builder,cell)&&orders.Destination(s,builder,delivery)){found=true;break;}if(!found)throw new Exception("Mission 2 stone inaccessible");}
    if(s.World.Resources[0]>=food)s.Stop(farmer);if(s.World.Resources[4]>=rock)s.Stop(builder);s.Step();
   }
   s.Stop(farmer);s.Stop(builder);if(s.World.Resources[0]<food||s.World.Resources[4]<rock)throw new Exception("Mission 2 collection stalled");
  }
  Collect(50,30);int sanctuary=Build(2);Collect(16,14);int barracks=Build(1);Collect(50,0);
  for(int i=0;i<150;i++)s.Step();Check(!s.MissionWon&&s.MissionChecks==0,"finished empty objectives never trigger victory");
  string unfinished=s.Serialize();var premature=JsonNode.Parse(unfinished)!;premature["Version"]=20;premature["MissionChecks"]=100;premature["Campaign"]!["TransitionOpen"]=true;
  s.Restore(premature.ToJsonString());Check(!s.MissionWon&&!s.Campaign.TransitionOpen&&s.MissionChecks==0&&WithoutHungerMeters(s.Serialize())==WithoutHungerMeters(unfinished)&&s.World.Units.All(a=>a.Satiety==4&&a.FatigueClock==0),"old premature victory recovers without losing buildings or resources and initializes legacy hunger meters");
  foreach(int slot in new[]{sanctuary,barracks}){
   var settler=s.World.Units.First(u=>u.Owner==0&&u.Type==9&&VisualCatalog.Visible(u));
   if(!s.EnterBuilding(settler,slot))throw new Exception("Objective entry failed");
   Check(!s.MissionWon&&s.MissionChecks==0,"walking colonist does not qualify objective");
   for(int i=0;i<10000&&s.ResidentTrips.Any(t=>t.Slot==settler.Slot);i++)s.Step();
   Check(s.Occupants(slot)>0,"objective colonist actually enters building");
   if(slot==sanctuary){for(int i=0;i<150;i++)s.Step();Check(!s.MissionWon&&s.MissionChecks==0,"only one occupied objective cannot finish mission");}
  }
  Check(s.MissionChecks>0&&!s.MissionWon,"completion countdown starts only after both entries");
  var released=s.ReleaseResident(barracks)??throw new Exception("Cannot release objective resident");s.Step();
  Check(!s.MissionWon&&s.MissionChecks==0,"removing required colonist resets unfinished victory countdown");
  if(!s.EnterBuilding(released,barracks))throw new Exception("Objective re-entry failed");
  for(int i=0;i<10000&&s.ResidentTrips.Any(t=>t.Slot==released.Slot);i++)s.Step();
  for(int i=0;i<100&&!s.MissionWon;i++)s.Step();if(!s.MissionWon)throw new Exception("Mission 2 objective did not complete");
 }
 public static void Run(string root){
  var vectors=JsonNode.Parse(File.ReadAllText(Path.GetFullPath(PackageLayout.Asset(root,"../../reverse_engineering/mission_two_v55.json"))))!["cases"]!.AsArray();
  foreach(var v in vectors){var n=v!;int cell=n["zone"]!.GetValue<int>()==0?12*256+50:24*256+50;var buildings=new List<Building>();int owner=n["owner"]!.GetValue<int>();if(n["sanctuary"]!.GetValue<bool>())buildings.Add(new(1,2,owner,cell));if(n["barracks"]!.GetValue<bool>())buildings.Add(new(2,1,owner,cell));bool expected=n["result"]!["victory"]!.GetValue<int>()==1;
   var occupants=buildings.Select(b=>new Actor{Slot=100+b.Slot,Owner=owner,Type=9,Health=10,LegacyState=14}).ToArray();var residents=buildings.ToDictionary(b=>b.Slot,b=>new List<int>{100+b.Slot});
   Check(Simulation.SecondMissionSatisfied(occupants.Append(new Actor{Owner=0,Type=5,Health=10,LegacyState=7}),buildings,Array.Empty<Construction>(),n["food"]!.GetValue<int>(),residents)==expected,"original region/resource vector with required occupants");
  }
  Check(Simulation.InSecondMissionZone(36)&&Simulation.InSecondMissionZone(23*256+63)&&!Simulation.InSecondMissionZone(35)&&!Simulation.InSecondMissionZone(24*256+36),"mission 2 base region boundaries match map bytes");
  Check(!Simulation.SecondMissionSatisfied(new[]{new Actor{Owner=0,Type=5,Health=10,LegacyState=7}},Array.Empty<Building>(),new[]{new Construction{Type=1,Cell=50},new Construction{Type=2,Cell=55}},50),"started objectives never count as completed buildings");
  var fixtureActors=new[]{new Actor{Slot=11,Owner=0,Type=9,Health=10,LegacyState=14},new Actor{Slot=12,Owner=0,Type=9,Health=10,LegacyState=14}};
  var fixtureBuildings=new[]{new Building(1,2,0,50),new Building(2,1,0,55)};var fixtureResidents=new Dictionary<int,List<int>>{{1,new(){11}},{2,new(){12}}};
  Check(!Simulation.SecondMissionSatisfied(fixtureActors,fixtureBuildings,Array.Empty<Construction>(),50),"empty buildings reject victory even if colonists exist elsewhere");
  Check(!Simulation.SecondMissionSatisfied(fixtureActors,Array.Empty<Building>(),Array.Empty<Construction>(),50,fixtureResidents),"resident metadata without completed buildings cannot win");
  fixtureActors[1].LegacyState=7;Check(!Simulation.SecondMissionSatisfied(fixtureActors,fixtureBuildings,Array.Empty<Construction>(),50,fixtureResidents),"pending or visible colonist cannot count as an occupant");fixtureActors[1].LegacyState=14;
  Check(Simulation.SecondMissionSatisfied(fixtureActors,fixtureBuildings,Array.Empty<Construction>(),50,fixtureResidents),"both completed occupied objectives qualify");
  var s=Load(root);string initial=s.Serialize();s.RequestNextMission();Check(!s.MissionWon&&!s.Campaign.TransitionOpen,"mission 2 cannot advance before its objectives");
  PlayToVictory(s);Check(s.MissionWon&&s.World.Resources[0]>=50&&s.Constructions.Count(c=>c.Type is 1 or 2&&s.Complete(c))==2,"actual mission 2 construction collection and staffing finish objectives");
  var copy=Load(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize()&&copy.MissionWon,"mission 2 victory survives save/load");
  var oldValid=JsonNode.Parse(s.Serialize())!;oldValid["Version"]=20;copy.Restore(oldValid.ToJsonString());Check(copy.MissionWon&&WithoutHungerMeters(copy.Serialize())==WithoutHungerMeters(s.Serialize()),"occupied legitimate v20 victory remains valid after hunger initialization");
  string before=s.Serialize();var corrupt=JsonNode.Parse(initial)!;corrupt["MissionChecks"]=100;corrupt["Tick"]=100;try{s.Restore(corrupt.ToJsonString());throw new Exception("False victory accepted");}catch(InvalidDataException){}Check(s.Serialize()==before,"mission 2 false victory is rejected without mutating state");
  s.RequestNextMission();Check(s.Campaign.TransitionOpen,"mission 2 victory opens technology transition");
  foreach(int branch in new[]{0,1,2,3}){Check(s.ChooseNextResearch(branch),"mission 2 branch choice "+branch);var p=s.ProgressForNextMission();var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE03.ETA"));new VisualCatalog().Apply(w);var third=new Simulation(w,p);Check(third.MissionNumber==3&&third.Campaign.CompletedMission==2&&third.Campaign.SelectedBranch==branch&&!third.MissionWon,"mission 3 inherits campaign branch and completed mission "+branch);var saved=third.Serialize();third.Restore(saved);Check(third.Serialize()==saved,"mission 3 inherited state saves and loads "+branch);}
  copy.Restore(s.Serialize());Check(copy.Campaign.TransitionOpen&&copy.Serialize()==s.Serialize(),"mission 2 transition saves and loads");copy.Restart();Check(copy.Serialize()==initial&&!copy.MissionWon,"mission 2 restart resets victory and construction");Console.WriteLine("ALL MISSION TWO TESTS PASSED");
 }
}
