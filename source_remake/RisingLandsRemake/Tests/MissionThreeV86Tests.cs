using System.Text.Json.Nodes;
namespace RisingLands;
public static class MissionThreeV86Tests {
 public static void Run(string root){int count=0;void Check(bool ok,string name){if(!ok)throw new Exception("FAIL v86: "+name);count++;Console.WriteLine("PASS v86: "+name);}
  var s=ResearchV58Tests.Load(root);Check(s.IsThirdMission&&s.HasMissionEnding&&!s.MissionWon,"mission three has a real ending and starts incomplete");
  ResearchV58Tests.Staff(s);s.AgricultureResearch.Progress=0;
  Check(!s.MissionWon&&s.MissionResearchCount==0,"building and staffing laboratory alone do not win");
  ResearchV58Tests.Advance(s,7200);Check(s.MissionResearchCount==1&&!s.MissionWon,"one completed discovery does not win");
  ResearchV58Tests.Advance(s,7200);Check(s.MissionResearchCount==2&&s.ExtraResearchOffered&&!s.MissionWon,"two normal discoveries unlock extra but do not win");
  ResearchV58Tests.GatherFood(s);int food=s.World.Resources[0];Check(s.PurchaseExtraResearch(2)&&s.World.Resources[0]==food-50,"extra research purchase pays exactly fifty food");
  Check(!s.MissionWon&&s.MissionResearchCount==2,"paying alone does not win");
  var copy=ResearchV58Tests.Load(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"paid incomplete objective saves and loads");
  var forged=JsonNode.Parse(s.Serialize())!;forged["MissionChecks"]=100;bool rejected=false;try{copy.Restore(forged.ToJsonString());}catch(InvalidDataException){rejected=true;}Check(rejected,"unfinished third research cannot forge a saved victory");
  ResearchV58Tests.Advance(s,7199);Check(s.MissionResearchCount==2&&!s.MissionWon,"partial third discovery remains incomplete");
  s.Step();Check(s.MissionResearchCount==3&&s.MissionChecks==1,"third completion begins victory evaluation");
  // Simulate loading a previously stuck save whose research was already finished.
  var old=JsonNode.Parse(s.Serialize())!;old["MissionChecks"]=0;copy.Restore(old.ToJsonString());ResearchV58Tests.Advance(copy,100);Check(copy.MissionWon,"already researched older save now reaches victory");
  ResearchV58Tests.Advance(s,99);Check(s.MissionWon,"three real discoveries complete mission three");
  copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize()&&copy.MissionWon,"victory state saves and loads");
  s.RequestNextMission();Check(s.ChooseNextResearch(1),"completed mission opens next branch selection");
  var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE04.ETA"));new VisualCatalog().Apply(w);var next=new Simulation(w,s.ProgressForNextMission());
  Check(next.MissionNumber==4&&next.Campaign.CompletedMission==3&&next.Campaign.SelectedBranch==1,"campaign advances to mission four with selected branch");
  Check(!next.World.Units.Any(a=>a.Owner>0&&a.Type==13&&a.Health>0),"next mission preserves removed enemy starting cart");
  Console.WriteLine("PASS v86 total "+count);
 }
}
