using System.Text.Json.Nodes;
namespace RisingLands;
public static class CampaignTransitionTests {
 static void Check(bool value,string name){if(!value)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
 static Simulation Second(string root,CampaignProgress progress){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE02.ETA"));new VisualCatalog().Apply(w);return new(w,progress);}
 public static void Run(string root){
  var sim=MissionOneTests.Load(root);string initial=sim.Serialize();sim.RequestNextMission();Check(!sim.Campaign.TransitionOpen&&!sim.ChooseNextResearch(0)&&sim.Serialize()==initial,"campaign cannot advance before original mission victory");
  MissionOneTests.PlayToVictory(sim);sim.RequestNextMission();Check(sim.Campaign.TransitionOpen&&sim.Campaign.SelectedBranch==-1,"victory opens branch selection without automatic choice");
  try{sim.ProgressForMissionTwo();throw new Exception("Missing choice accepted");}catch(InvalidOperationException){}Check(!sim.ChooseNextResearch(-1)&&!sim.ChooseNextResearch(4),"campaign rejects missing and invalid choices");
  var levels=sim.EffectiveLevels;var resources=sim.World.Resources.ToArray();
  for(int branch=0;branch<4;branch++){
   Check(sim.ChooseNextResearch(branch)&&sim.EffectiveLevels.SequenceEqual(levels)&&sim.World.Resources.SequenceEqual(resources),"branch "+branch+" choice grants no instant technology or resources");
   string saved=sim.Serialize();var restored=MissionOneTests.Load(root);restored.Restore(saved);Check(restored.Serialize()==saved&&restored.Campaign.TransitionOpen&&restored.Campaign.SelectedBranch==branch,"branch "+branch+" transition round trip");
   var next=Second(root,restored.ProgressForMissionTwo());Check(next.World.MapName=="MONDE02.ETA"&&next.Campaign.CompletedMission==1&&next.Campaign.SelectedBranch==branch&&next.EffectiveLevels.SequenceEqual(levels)&&!next.Campaign.TransitionOpen,"branch "+branch+" opens original second map preserving levels");
   string second=next.Serialize();var loaded=Second(root,restored.ProgressForMissionTwo());loaded.Restore(second);Check(loaded.Serialize()==second,"branch "+branch+" second mission save round trip");
   next.Campaign.SelectedBranch=(branch+1)%4;next.Restart();Check(next.Serialize()==second,"branch "+branch+" restart retains campaign initial choice");
  }
  string before=sim.Serialize();var invalid=JsonNode.Parse(before)!;invalid["Campaign"]!["StartingLevels"]![0]=-1;try{sim.Restore(invalid.ToJsonString());throw new Exception("Invalid inheritance accepted");}catch(InvalidDataException){}Check(sim.Serialize()==before,"corrupt inherited level rejected without changing live state");
  var legacy=JsonNode.Parse(initial)!;legacy["Version"]=17;legacy.AsObject().Remove("Campaign");var migrated=MissionOneTests.Load(root);migrated.Restore(legacy.ToJsonString());Check(migrated.EffectiveLevels.SequenceEqual(new[]{2,2,4,0})&&migrated.Campaign.SelectedBranch==-1,"v17 migration preserves native baseline without inventing campaign choice");
  foreach(int branch in new[]{0,2}){var next=Second(root,new(){StartingLevels=new[]{4,2,6,0},SelectedBranch=branch,CompletedMission=1,CarriedProgress=10});Check(next.EffectiveLevels.SequenceEqual(new[]{4,2,6,0})&&next.ResearchState(branch).Progress==10,"inherited developed levels and accumulator branch "+branch);string saved=next.Serialize();next.Restore(saved);Check(next.Serialize()==saved,"inherited research round trip branch "+branch);}
 }
}
