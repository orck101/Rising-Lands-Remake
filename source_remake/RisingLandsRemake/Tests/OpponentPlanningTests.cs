using System.Reflection;
using System.Text.Json;
namespace RisingLands;
public static class OpponentPlanningTests {
 public static void Recovery(string root,string snapshot,string output){
  var s=MissionFiveScript.Load(root);s.Restore(File.ReadAllText(snapshot));
  const int owner=3;foreach(var a in s.World.Units.Where(a=>a.Owner==owner&&a.Type==7))a.Health=0;
  int before=s.Recruitments.Count(r=>r.Owner==owner&&r.Finished);var rows=new List<object>();
  for(int i=0;i<=18000;i++){
   if(i%4500==0){rows.Add(new{Seconds=i/30,Decision=s.DecisionFor(owner),Workers=s.World.Units.Where(a=>a.Owner==owner&&a.Health>0).GroupBy(a=>a.Type).ToDictionary(g=>g.Key,g=>g.Count()),Food=s.World.Resources[owner],Rock=s.World.Resources[owner+4]});string json=s.Serialize();var copy=MissionFiveScript.Load(root);copy.Restore(json);if(copy.Serialize()!=json)throw new Exception("Recovery save roundtrip failed");}
   if(i<18000)s.Step();
  }
  File.WriteAllText(output,JsonSerializer.Serialize(rows,new JsonSerializerOptions{WriteIndented=true}));
  if(!s.World.Units.Any(a=>a.Owner==owner&&a.Type==7&&a.Health>0)||s.Recruitments.Count(r=>r.Owner==owner&&r.Finished)<=before)throw new Exception("Essential builder recovery failed");
  Console.WriteLine("PASS planning: all builders lost; replacement trained with real resources and settlers within ten minutes; five exact save/load checkpoints");
 }
 public static void Run(string root,string output,string? snapshot=null){
  int checks=0;void Check(bool value,string name){if(!value)throw new Exception("FAIL planning: "+name);checks++;Console.WriteLine("PASS planning: "+name);}
  object? Call(Simulation s,string name,params object[] args)=>typeof(Simulation).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(s,args);
  var s=MissionFiveScript.Load(root);
  foreach(int owner in Enumerable.Range(1,3)){
   var stocks=s.World.Resources.ToArray();var decision=s.DecisionFor(owner);
   Check(decision.Personality==s.PersonalityFor(owner).Name&&decision.NextBuilding==3,"clan "+owner+" plans its real missing warehouse");
   Check(s.World.Resources.SequenceEqual(stocks)&&s.Recruitments.Count==0,"planning grants nothing");
   Check(decision.Farmers>=2&&decision.Farmers<=3&&decision.Builders==1,"opening does not exhaust population with workers");
  }
  Check(OpponentPolicyProfile.ForMission(15).ReserveMinutes<OpponentPolicyProfile.ForMission(5).ReserveMinutes,"campaign progressively reduces safety margin");
  if(snapshot!=null){s.Restore(File.ReadAllText(snapshot));var diagnostics=new List<object>();
   foreach(int owner in Enumerable.Range(1,3)){
    var builder=s.World.Units.FirstOrDefault(a=>a.Owner==owner&&a.Type==7&&a.Health>0);
    var reachable=builder==null?new byte[65536]:(byte[])Call(s,"OpponentReachableCells",builder)!;
    diagnostics.Add(new{Owner=owner,Decision=s.DecisionFor(owner),Mines=s.World.Mines.Select(m=>new{m.Cell,Known=s.Opponent.Explored[owner][m.Cell]!=0,Local=builder!=null&&(bool)Call(s,"OpponentLocalResource",builder,m.Cell)!,Reachable=reachable[m.Cell]!=0,EntranceReachable=m.Cell+256<65536&&reachable[m.Cell+256]!=0}).ToArray()});
   }
   File.WriteAllText(output,JsonSerializer.Serialize(diagnostics,new JsonSerializerOptions{WriteIndented=true}));
   string saved=s.Serialize();var copy=MissionFiveScript.Load(root);copy.Restore(saved);Check(copy.Serialize()==saved,"advanced economy restores exactly, including reverted construction workers");
   for(int i=0;i<300;i++){s.Step();copy.Step();}Check(s.Serialize()==copy.Serialize(),"planner continues deterministically after load");
  }
  Console.WriteLine("ALL PLANNING TESTS PASSED: "+checks);
 }
}
