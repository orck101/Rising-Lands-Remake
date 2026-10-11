using System.Reflection;
using System.Text.Json;
namespace RisingLands;
public static class DiplomacyResearchV126Tests {
 public static Simulation LoadCopy(string root,string path){string json=File.ReadAllText(path);using var d=JsonDocument.Parse(json);var c=JsonSerializer.Deserialize<CampaignProgress>(d.RootElement.GetProperty("Campaign").GetRawText())!;c.TransitionOpen=false;var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD",d.RootElement.GetProperty("MapName").GetString()!));new VisualCatalog().Apply(w);var s=new Simulation(w,c);s.Restore(json);return s;}
 public static void Run(string root,string path,string output){
  var checks=new List<string>();void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);Console.WriteLine("PASS "+label);}
  void Invoke(Simulation s,string name,params object[] a)=>typeof(Simulation).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(s,a);
  var s=LoadCopy(root,path);var diagnosis=new{Map=s.World.MapName,s.Tick,Branch=s.ActiveResearchBranch,Next=s.ResearchInvention(s.ActiveResearchBranch),Scientists=s.Scientists(),Status=s.ResearchStatus(s.ActiveResearchBranch),s.MissionResearchCount,s.Research.ManualPaused};Console.WriteLine(JsonSerializer.Serialize(diagnosis));
  Check(s.ResearchInvention(0)?.Key=="AGRICULTURE6","slot1 blocked research reproduced at Rhino invention");
  Check(s.ResearchAvailable(0),"existing Rhino unlock accepted by agriculture research");
  var replay=LoadCopy(root,path);for(int i=0;i<300;i++)replay.Step();Check(replay.AgricultureResearch.Progress>0,"actual slot1 simulation resumes research without player intervention");
  var empty=LoadCopy(root,path);foreach(var b in empty.World.Buildings.Where(b=>b.Owner==0&&b.Type==7))empty.World.BuildingResidents[b.Slot].Clear();Invoke(empty,"AdvanceMissionResearch");Check(empty.AgricultureResearch.Progress==0&&!empty.AgricultureResearch.Enabled,"empty lab still cannot research");
  // Controlled staffing isolates research progression from the saved player's missing operators.
  var lab=s.World.Buildings.First(b=>b.Owner==0&&b.Type==7);var residents=s.World.BuildingResidents.GetValueOrDefault(lab.Slot,new());
  if(s.Scientists()==0){var colonist=s.World.Units.First(a=>a.Owner==0&&a.Type==9&&a.Health>0);foreach(var ids in s.World.BuildingResidents.Values)ids.Remove(colonist.Slot);colonist.LegacyState=14;s.World.BuildingResidents[lab.Slot]=new(){colonist.Slot};}
  for(int i=0;i<25200&&s.MissionResearchCount<2;i++)Invoke(s,"AdvanceMissionResearch");
  Check(s.MissionResearchCount==2&&s.UnitUnlocked(25)&&s.UnitUnlocked(13),"Rhino and food collector discoveries finish and unlock units");
  string json=s.Serialize();var copy=LoadCopy(root,path);copy.Restore(json);Check(copy.Serialize()==json,"new agriculture discoveries roundtrip save");
  var arena=TradeV97Tests.Fixture(root);var a=arena.World.Units.First(a=>a.Owner==0&&a.Type==0&&VisualCatalog.Visible(a));var enemy=arena.World.Units.First(a=>a.Owner==1&&VisualCatalog.Visible(a));arena.Campaign.Goodwill=new[]{0,20,30,40};
  Invoke(arena,"ApplyHit",a,enemy,1);Check(arena.Campaign.Goodwill.SequenceEqual(new[]{0,20,30,40}),"hostile unit hit preserves all opinions");
  var building=arena.World.Buildings.First(b=>b.Owner==1);Invoke(arena,"HitStructure",building,10,0);Check(arena.Campaign.Goodwill.SequenceEqual(new[]{0,20,30,40}),"hostile structure damage preserves all opinions");
  arena.Campaign.Alliances[1]=true;Check(arena.Attack(a,enemy)&&!arena.Allied(1)&&arena.Goodwill(1)==-15,"betrayal still breaks alliance and penalizes only affected ally");
  Invoke(arena,"ApplyHit",a,enemy,1);Check(arena.Goodwill(1)==-15,"hostile hits after betrayal add no penalty");
  Simulation campaign;
  // Use a mission-one fixture for the existing natural completion helper.
  var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);campaign=new Simulation(w);MissionOneTests.PlayToVictory(campaign);campaign.RequestNextMission();campaign.ChooseNextResearch(0);campaign.Campaign.Goodwill=new[]{0,37,-23,50};campaign.Campaign.Alliances[1]=true;
  var progress=campaign.ProgressForNextMission();w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE02.ETA"));new VisualCatalog().Apply(w);var next=new Simulation(w,progress);Check(next.Campaign.Goodwill.SequenceEqual(new[]{0,37,-23,50}),"mission transition preserves all goodwill values");json=next.Serialize();next.Restore(json);Check(next.Campaign.Goodwill.SequenceEqual(new[]{0,37,-23,50}),"new mission save/load preserves goodwill");
  File.WriteAllText(output,JsonSerializer.Serialize(new{Passed=true,Checks=checks.Count,Diagnosis=diagnosis,Assertions=checks},new JsonSerializerOptions{WriteIndented=true}));
 }
}
