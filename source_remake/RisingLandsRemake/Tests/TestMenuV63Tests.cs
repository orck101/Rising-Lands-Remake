using System.Text.Json.Nodes;
namespace RisingLands;
public static class TestMenuV63Tests {
 static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: v63 "+name);Console.WriteLine("PASS: v63 "+name);}
 static void Roundtrip(Simulation s,string root,string name){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD",s.World.MapName));new VisualCatalog().Apply(w);var copy=new Simulation(w);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),name);}
 public static void Run(string root){
  var s=MissionOneTests.Load(root);string initial=s.Serialize();
  foreach(int kind in new[]{1,2,0}){int index=kind==0?8:kind==1?0:4;int before=s.World.Resources[index];Check(s.GrantTestResource(kind)&&s.World.Resources[index]==before+50,"resource grant +50 "+kind);Check(s.GrantTestResource(kind)&&s.World.Resources[index]==before+100,"repeated resource grant "+kind);Roundtrip(s,root,"resource conservation and roundtrip "+kind);}
  Check(!s.GrantTestResource(-1)&&!s.GrantTestTechnology(4),"invalid commands do not change state");
  foreach(int branch in new[]{0,2,1,3}){int before=s.ResearchState(branch).Index,quota=s.MissionResearchCount;var invention=s.ResearchInvention(branch)!;Check(s.GrantTestTechnology(branch)&&s.ResearchState(branch).Index==before+1,"exactly one technology level "+branch);Check(s.TechnologyApplied(invention)&&s.MissionResearchCount==quota,"test technology grants effect without consuming research quota "+branch);Roundtrip(s,root,"technology save and load "+branch);}
  var research=ResearchV58Tests.Load(root);ResearchV58Tests.Staff(research);research.AgricultureResearch.Progress=0;ResearchV58Tests.Advance(research,7200);Check(research.MissionResearchCount==1,"normal discovery before test grant");Check(research.GrantTestTechnology(0),"grant between normal discoveries");Roundtrip(research,root,"normal and test research mixed history");ResearchV58Tests.Advance(research,7200);Roundtrip(research,root,"normal research after grant resumes and saves");
  foreach(int branch in new[]{0,1,2,3}){int grants=0;while(s.GrantTestTechnology(branch))grants++;int limit=Simulation.UnlockData.Technologies.Where(t=>t.Branch==branch).Max(t=>t.Index)+1;Check(s.ResearchState(branch).Index==limit&&!s.CanTestTechnology(branch),"technology max respected "+branch);Roundtrip(s,root,"full technology history roundtrip "+branch);}
  s.Step();Roundtrip(s,root,"simulation remains valid with all technology branches maxed");
  string valid=s.Serialize();var bad=JsonNode.Parse(valid)!;bad["Tests"]!["Food"]=1;try{s.Restore(bad.ToJsonString());throw new Exception("Invalid grant accepted");}catch(InvalidDataException){}Check(s.Serialize()==valid,"invalid grant rejected with state preserved");
  bad=JsonNode.Parse(valid)!;bad["Resources"]!.AsArray()[0]=s.World.Resources[0]+1;try{s.Restore(bad.ToJsonString());throw new Exception("Untracked food accepted");}catch(InvalidDataException){}Check(s.Serialize()==valid,"untracked resource increase rejected");
  s.Restart();Check(s.Serialize()==initial,"restart clears test grants and technologies");
  foreach(int mission in new[]{1,2,3}){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD",$"MONDE{mission:00}.ETA"));new VisualCatalog().Apply(w);var sim=new Simulation(w);Check(sim.CompleteTestMission()&&sim.MissionWon&&sim.MissionChecks==100&&!sim.Campaign.TransitionOpen,"test completion opens normal victory mission "+mission);Roundtrip(sim,root,"forced victory save at tick zero "+mission);long tick=sim.Tick;sim.Step();Check(sim.Tick==tick,"victory stops world updates "+mission);sim.RequestNextMission();Check(sim.Campaign.TransitionOpen&&sim.ChooseNextResearch(0),"normal next mission research transition "+mission);var next=sim.ProgressForNextMission();Check(next.CompletedMission==mission,"next mission progress preserved "+mission);Roundtrip(sim,root,"transition save with forced victory "+mission);sim.Restart();Check(!sim.MissionWon&&!sim.Tests.Victory,"restart clears forced victory "+mission);}
  var near=MissionOneTests.Load(root);while(near.GrantTestResource(1)){}Check(near.World.Resources[0]>ushort.MaxValue-50&&!near.GrantTestResource(1),"resource grant stops before integer overflow");Roundtrip(near,root,"large grants conservation");
  Console.WriteLine("ALL TEST MENU v63 CHECKS PASSED");
 }
}

