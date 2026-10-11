using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace RisingLands;
public static class MissionSixTests {
 static int checks;
 static void Check(bool ok,string label){if(!ok)throw new InvalidDataException("FAIL M6: "+label);checks++;Console.WriteLine("PASS M6: "+label);}
 static Simulation Load(string root,CampaignProgress? campaign=null){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE06.ETA"));new VisualCatalog().Apply(w);return new(w,campaign);}
 public static void Benchmark(string root,string output){
  var s=Load(root);for(int i=0;i<100;i++)s.Step();var times=new List<double>();
  for(int i=0;i<3000;i++){long start=Stopwatch.GetTimestamp();s.Step();times.Add(Stopwatch.GetElapsedTime(start).TotalMilliseconds);}
  var sorted=times.Order().ToArray();var result=new{Samples=times.Count,MeanMs=times.Average(),P95Ms=sorted[(int)(sorted.Length*.95)],MaxMs=sorted[^1],s.Tick,s.OpponentEnabled,Units=s.World.Units.Count,Limit="Simulation CPU only; not frame rate or GPU timing."};
  File.WriteAllText(output,JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(result));
  Check(result.MeanMs<3&&result.P95Ms<6,"simulation has CPU headroom for 150 ticks/second (5x)");
 }
 public static void Run(string root,string output){
  checks=0;var s=Load(root);string initial=s.Serialize();
  Check(s.HasMissionEnding&&s.IsSixthMission&&!s.MissionWon&&!s.MissionLost,"mission six has active exploration objective");
  Check(!s.OpponentEnabled&&s.World.Units.All(a=>a.Owner==0)&&s.World.Buildings.All(b=>b.Owner==0),"native map has no enemy units, buildings or AI");
  Check(s.World.Width==256&&s.World.Height==128&&s.World.Units.Count==11,"native dimensions and eleven actor records preserved");
  Check(s.World.Resources[0]==20&&s.World.Resources[4]==20&&s.World.Resources.Skip(1).Where((_,i)=>i!=3).All(v=>v==0),"native 20 food / 20 rock opening preserved");
  Check(s.World.Mines.Single(m=>m.Cell==Simulation.SixthMissionTarget-256).Quantity==0,"target is original exhausted mine, not a stone extraction objective");
  var reader=new ObjectiveReader();reader.Open("MONDE06.ETA",false);reader.Move(1);Check(reader.Lines.Any(t=>t.Contains("conhecimento espiritual")),"Portuguese objective explains spiritual exploration");
  Check(!s.InSight(Simulation.SixthMissionTarget)&&!s.Discovered(Simulation.SixthMissionTarget),"target begins under unexplored fog");
  for(int i=0;i<150;i++)s.Step();Check(!s.MissionWon&&s.MissionChecks==0,"waiting at base does not win or spawn enemies");
  s.Exploration.Explored[Simulation.SixthMissionTarget]=1;s.Exploration.Tiles[Simulation.SixthMissionTarget]=s.World.Tiles[Simulation.SixthMissionTarget];
  for(int i=0;i<110;i++)s.Step();Check(!s.MissionWon,"remembered terrain alone is not current sight");
  s.Restart();Check(s.Serialize()==initial,"restart restores native opening and fog");
  var scout=s.World.Units.First(a=>a.Owner==0&&a.Type==0&&VisualCatalog.Visible(a));
  Check(s.Order(scout,225,59),"normal movement finds a route from initial base to abandoned mine");
  Check(scout.Route.Count>100&&scout.Route.All(c=>s.Passable(c&255,c>>8)),"exploration route crosses real passable terrain");
  for(int i=0;i<300;i++)s.Step();string moving=s.Serialize();var copy=Load(root);copy.Restore(moving);Check(copy.Serialize()==moving,"mid-exploration save restores exact route and fog");
  for(int i=0;i<60;i++){s.Step();copy.Step();}Check(s.Serialize()==copy.Serialize(),"exploration continues deterministically after load");
  int checkpoints=0;for(int i=0;i<16000&&!s.MissionWon;i++){
   s.Step();if(i%1000==0){copy=Load(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"journey checkpoint "+(++checkpoints)+" roundtrip");}
  }
  Check(s.MissionWon&&s.InSight(Simulation.SixthMissionTarget),"mission completed by physical scouting without cheats or test victory");
  Check(s.World.Mines.Single(m=>m.Cell==Simulation.SixthMissionTarget-256).Quantity==0&&s.MissionResearchCount==0,"completion requires neither mining nor laboratory research");
  string victory=s.Serialize();copy=Load(root);copy.Restore(victory);Check(copy.Serialize()==victory&&copy.MissionWon,"victory is restored from saved sight, not destination object's current vision");
  var bad=JsonNode.Parse(victory)!;var moved=bad["Units"]!.AsArray().Single(a=>a!["Slot"]!.GetValue<int>()==scout.Slot)!;moved["X"]=4*1024;moved["Y"]=57*1024;
  try{copy.Restore(bad.ToJsonString());throw new Exception("Forged victory accepted");}catch(InvalidDataException e){Check(e.Message=="Victory requirements not met","forged victory rejected for missing sight specifically");}Check(copy.Serialize()==victory,"victory without genuine sight rejected atomically");
  var partial=Load(root);var observer=partial.World.Units.First(a=>a.Type==0&&VisualCatalog.Visible(a));observer.X=225*1024;observer.Y=59*1024;partial.UpdateVisibility();
  for(int i=0;i<12;i++)partial.Step();Check(partial.MissionChecks>0&&partial.MissionChecks<100,"sighting starts completion confirmation without immediate victory");
  var resumed=Load(root);resumed.Restore(partial.Serialize());Check(resumed.Serialize()==partial.Serialize(),"partial sighting confirmation survives save/load");
  observer.X=4*1024;observer.Y=57*1024;partial.UpdateVisibility();partial.Step();Check(partial.MissionChecks==0&&!partial.MissionWon,"losing current sight resets incomplete confirmation");
  for(int i=0;i<100&&!resumed.MissionWon;i++)resumed.Step();Check(resumed.MissionWon,"loaded partial confirmation completes under maintained sight");
  long stop=s.Tick;s.Step();Check(s.Tick==stop,"victory stops simulation and duplicate completion");
  s.RequestNextMission();Check(s.ChooseNextResearch(3),"spiritual branch may be selected for next mission");var progress=s.ProgressForNextMission();
  var nextWorld=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE07.ETA"));new VisualCatalog().Apply(nextWorld);var next=new Simulation(nextWorld,progress);
  Check(next.MissionNumber==7&&next.Campaign.CompletedMission==6&&next.EffectiveLevels[3]>=4,"mission seven inherits progression and native four spiritual unlocks");
  var lost=Load(root);foreach(var a in lost.World.Units)a.Health=0;Check(lost.MissionLost&&!lost.MissionWon,"no surviving clan members means defeat, never victory");
  var lostCopy=Load(root);lostCopy.Restore(lost.Serialize());Check(lostCopy.MissionLost,"defeat survives save/load");lost.Restart();Check(!lost.MissionLost&&lost.Serialize()==initial,"restart recovers from defeat");
  var fresh=Load(root);Check(fresh.Serialize()==initial,"separate mission-six session has no leaked progress");
  var corners=Load(root);var walker=corners.World.Units.First(a=>a.Type==0&&VisualCatalog.Visible(a));
  int corner=Enumerable.Range(0,65536).First(c=>(c&255)<255&&(c>>8)<127&&new[]{c,c+1,c+256,c+257}.All(v=>corners.Passable(v&255,v>>8)&&corners.BuildingAt(v)==null));
  walker.X=(corner&255)*1024;walker.Y=(corner>>8)*1024;
  Check(corners.Order(walker,(corner&255)+1,(corner>>8)+1)&&walker.Route.Count==1,"open diagonal produces one legal movement step");
  corners.World.Buildings.Add(new(9000,9,0,corner+1));corners.World.Buildings.Add(new(9001,9,0,corner+256));
  bool found=corners.Order(walker,(corner&255)+1,(corner>>8)+1);Check(!found||walker.Route.Count>1,"diagonal cannot cut between two wall corners");
  if(found){int before=walker.Y/1024*256+walker.X/1024;for(int i=0;i<500&&walker.Route.Count>0;i++){corners.Step();int now=walker.Y/1024*256+walker.X/1024;if(now!=before)Check(now!=corner+1&&now!=corner+256,"movement avoids wall cells");before=now;}}
  File.WriteAllText(output,JsonSerializer.Serialize(new{Checks=checks,Passed=true,JourneyTicks=s.Tick,JourneySeconds=s.Tick/30.0,Checkpoints=checkpoints,Limits="Scripted movement via gameplay commands; no manual interface playthrough."},new JsonSerializerOptions{WriteIndented=true}));
  Console.WriteLine("ALL MISSION SIX TESTS PASSED: "+checks);
 }
}
