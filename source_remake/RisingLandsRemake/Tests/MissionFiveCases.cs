using System.Text.Json;
namespace RisingLands;
public static class MissionFiveCases {
 public static void Run(string root,string output){
  Directory.CreateDirectory(output);var results=new List<object>();
  foreach(bool allied in new[]{false,true}){
   var s=MissionFiveScript.Load(root);s.Campaign.Alliances[1]=allied;
   var a=s.World.Units.First(u=>u.Owner==1&&u.Type==0&&VisualCatalog.Visible(u));var target=s.World.Units.First(u=>u.Owner==2&&u.Type==0&&VisualCatalog.Visible(u));
   int cell=Enumerable.Range(0,65536).Where(c=>(c&255)<s.World.Width&&(c>>8)<s.World.Height&&Math.Abs((c&255)-a.X/1024)+Math.Abs((c>>8)-a.Y/1024)<=3&&c!=a.Y/1024*256+a.X/1024&&s.Passable(c&255,c>>8)).First();
   target.X=(cell&255)*1024+512;target.Y=(cell>>8)*1024+512;target.Route.Clear();a.AttackTarget=target.Slot;var rows=new List<object>();
   for(int i=0;i<=60;i++){if(i is 0 or 29 or 30 or 31 or 60)rows.Add(new{s.Tick,a.AttackTarget,Health=target.Health,Automatic=a.AutomaticDefense,Windup=a.AttackWindup});if(i<60)s.Step();}
   string json=s.Serialize();var copy=MissionFiveScript.Load(root);copy.Restore(json);results.Add(new{Case="controlled-third-clan-defense",ArtificialSetup=true,AlliedWithPlayer=allied,Attacker=a.Slot,Target=target.Slot,Rows=rows,Roundtrip=copy.Serialize()==json});File.WriteAllText(Path.Combine(output,$"defense-allied-{allied}.json"),json);
  }
  var ending=MissionFiveScript.Load(root);for(int owner=1;owner<=3;owner++)ending.Campaign.Alliances[owner]=true;for(int i=0;i<200;i++)ending.Step();ending.RequestNextMission();results.Add(new{Case="controlled-all-allied-ending",ArtificialSetup=true,ending.HasMissionEnding,ending.MissionWon,ending.MissionChecks,ending.Campaign.TransitionOpen});
  string baseline=Path.Combine(Path.GetDirectoryName(Path.GetFullPath(output))!,"snapshots","baseline-3600s.json");
  if(File.Exists(baseline)){
   var s=MissionFiveScript.Load(root);s.Restore(File.ReadAllText(baseline));object? Call(string name,params object[] args)=>typeof(Simulation).GetMethod(name,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.Invoke(s,args);
   var owners=Enumerable.Range(1,3).Select(owner=>new{Owner=owner,Plan=s.DevelopmentPlan(owner),Warehouse=s.WarehouseSummary(owner),Construction=Call("PolicyConstruction",owner),BasicServices=Call("BasicServices",owner),WarehouseOperator=Call("WarehouseOperatorSlot",owner),WarehouseRequested=s.Opponent.WarehouseExpansionRequested[owner],RockReserve=Call("PolicyRockReserve",owner),KnownMines=s.World.Mines.Count(m=>s.Opponent.Explored[owner][m.Cell]!=0),Builders=s.World.Units.Where(a=>a.Owner==owner&&a.Type==7&&a.Health>0).Select(a=>new{a.Slot,a.X,a.Y,a.Rock,a.MineJob,a.MineIndex,a.RockDestination,a.FoodJob,a.Food,Route=a.Route.Count,KnownSafeMines=s.World.Mines.Count(m=>s.Opponent.Explored[owner][m.Cell]!=0&&(bool)Call("OpponentLocalResource",a,m.Cell)!)}).ToArray(),Buildings=s.World.Buildings.Where(b=>b.Owner==owner).Select(b=>new{b.Slot,b.Type,Residents=s.Residents(b.Slot),Capacity=s.ResidentCapacity(b),b.Cell}).ToArray()}).ToArray();
   results.Add(new{Case="natural-baseline-60min-diagnosis",ArtificialSetup=false,Owners=owners});
   foreach(int type in new[]{5,7}){
    var recovery=MissionFiveScript.Load(root);recovery.Restore(File.ReadAllText(baseline));int trained=recovery.Recruitments.Count(j=>j.Owner==1&&j.Finished);int removed=0;
    foreach(var a in recovery.World.Units.Where(a=>a.Owner==1&&a.Type==type&&a.Health>0)){a.Health=0;removed++;}
    for(int i=0;i<10*60*30;i++)recovery.Step();string json=recovery.Serialize();var copy=MissionFiveScript.Load(root);copy.Restore(json);
    results.Add(new{Case="controlled-loss-recovery",ArtificialSetup=true,Owner=1,RemovedType=type,RemovedCount=removed,Minutes=10,RemainingWorkers=recovery.World.Units.Count(a=>a.Owner==1&&a.Type==type&&a.Health>0),NewTraining=recovery.Recruitments.Count(j=>j.Owner==1&&j.Finished)-trained,Food=recovery.World.Resources[1],Rock=recovery.World.Resources[5],Economy=recovery.DevelopmentPlan(1),Roundtrip=copy.Serialize()==json});File.WriteAllText(Path.Combine(output,$"recovery-loss-{type}.json"),json);
   }
  }
  File.WriteAllText(Path.Combine(output,"controlled-cases.json"),JsonSerializer.Serialize(results,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(results));
 }
}

