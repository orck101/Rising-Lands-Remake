using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace RisingLands;
public static class MissionNineTests {
 static readonly List<string> checks=new();static readonly List<object> runs=new();static string folder=".";
 static void Check(bool ok,string label){if(!ok)throw new InvalidDataException("FAIL M9: "+label);checks.Add(label);Console.WriteLine("PASS M9: "+label);}
 static Simulation Load(string root,char variant,int branch=2){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE09"+variant+".ETA"));new VisualCatalog().Apply(w);return new(w,new(){StartingLevels=Simulation.UnlockData.Missions.First(m=>m.Mission==9).Levels.ToArray(),SelectedBranch=branch});}
 static void Advance(Simulation s,int ticks){for(int i=0;i<ticks;i++)s.Step();}
 static void Roundtrip(string root,Simulation s,string label){var copy=Load(root,s.World.MapName[7],s.Research.InitialBranch);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),label);}
 static void Until(Simulation s,Func<bool> done,int ticks,string label){for(int i=0;i<ticks&&!done()&&!s.MissionLost&&!s.MissionWon;i++)s.Step();if(!done())File.WriteAllText(Path.Combine(folder,"failed-"+s.World.MapName+"-"+s.ActiveResearchBranch+".json"),s.Serialize());Check(done(),label+" tick="+s.Tick);}
 static int Prepare(string root,Simulation s,int workers){
  int store=ResearchTests.Build(s,3);var home=s.World.Buildings.Single(b=>b.Owner==0&&b.Type==2);
  var col=s.ReleaseResident(home.Slot);Check(col!=null&&s.EnterBuilding(col,store),"occupy new warehouse "+s.World.MapName);Until(s,()=>s.Occupants(store)==1,3000,"warehouse operator reaches actual entrance");
  var farmer=s.World.Units.First(a=>a.Owner==0&&a.Type==5);Check(s.SetFoodDestination(farmer,2,store)&&s.GatherNearest(farmer),"ordinary food loop from reachable native food");Until(s,()=>s.World.Resources[0]>=24,6000,"collect real food to fund laboratory");
  int lab=ResearchTests.Build(s,7,false);Check(!s.MissionWon&&s.MissionResearchCount==0,"building laboratory does not win");
  double before=s.ResearchState(s.ActiveResearchBranch).Progress;Advance(s,120);Check(s.ResearchState(s.ActiveResearchBranch).Progress==before,"empty laboratory never advances research");Roundtrip(root,s,"empty laboratory save");
  for(int i=0;i<workers;i++){col=s.ReleaseResident(home.Slot);Check(col!=null&&s.EnterBuilding(col,lab),"ordinary colonist walks into laboratory");}
  Until(s,()=>s.Scientists()==workers,3000,"researchers reach laboratory");return lab;
 }
 public static void Run(string root,string output){folder=Path.GetDirectoryName(Path.GetFullPath(output))!;Directory.CreateDirectory(folder);checks.Clear();runs.Clear();
  foreach(char variant in "ABCD"){
   var s=Load(root,variant);string initial=s.Serialize();Check(s.IsNinthMission&&s.HasMissionEnding&&!s.MissionWon&&!s.MissionLost,"unfinished active objective "+variant);
   Check(s.World.Width==128&&s.World.Height==128&&s.World.Buildings.Count==4&&s.World.Buildings.All(b=>b.Type==2),"native map and four sanctuaries "+variant);
   Check(s.World.Resources[0]==new[]{50,40,35,30}[variant-'A']&&s.World.Resources[4]==new[]{50,40,30,20}[variant-'A']&&s.World.Resources[8]==new[]{6,4,2,0}[variant-'A'],"native opening stock "+variant);
   Check(s.EffectiveLevels.SequenceEqual(new[]{2,2,7,4}),"mission-nine direct-entry technology floor "+variant);
   for(int owner=1;owner<=3;owner++)for(int b=0;b<4;b++)Check(s.OpponentLevel(owner,b)>=Simulation.OpponentTechnologies.Missions.First(m=>m.Mission==9).Players[owner][b]+(owner==3?2:0),"enemy technology floor/profile owner="+owner+" branch="+b+" variant="+variant);
   Roundtrip(root,s,"opening exact save "+variant);var enemy=Enumerable.Range(1,3).Select(o=>Enumerable.Range(0,4).Select(b=>s.OpponentLevel(o,b)).ToArray()).ToArray();s.Restore(initial);Check(enemy.SelectMany(a=>a).SequenceEqual(Enumerable.Range(1,3).SelectMany(o=>Enumerable.Range(0,4).Select(b=>s.OpponentLevel(o,b)))),"load does not stack green bonus "+variant);
   Advance(s,150);Check(!s.MissionWon&&s.MissionResearchCount==0,"waiting does not count as discovery "+variant);s.Restart();Check(s.Serialize()==initial,"restart restores native opening "+variant);
   var forged=JsonNode.Parse(initial)!;forged["Tick"]=100;forged["MissionChecks"]=100;bool rejected=false;try{s.Restore(forged.ToJsonString());}catch(InvalidDataException){rejected=true;}Check(rejected&&s.Serialize()==initial,"false victory rejected atomically "+variant);
   var objective=new ObjectiveReader();objective.Open(s.World.MapName,false);objective.Move(1);Check(objective.Lines.Any(l=>l.Contains("duas descobertas",StringComparison.OrdinalIgnoreCase)),"Portuguese objective alias "+variant);
   // Reachability audit: successful orders use ordinary ground pathfinding, then are cancelled.
   var builder=s.World.Units.First(a=>a.Owner==0&&a.Type==7);var farmer=s.World.Units.First(a=>a.Owner==0&&a.Type==5);
   var cells=Enumerable.Range(0,65536).Where(c=>(c&255)<128&&(c>>8)<128&&s.Passable(c&255,c>>8)).ToArray();int reachable=0;foreach(int c in cells.Where((c,i)=>i%Math.Max(1,cells.Length/100)==0))if(s.Order(builder,c&255,c>>8,true))reachable++;builder.Route.Clear();Check(reachable>0,"ground navigation has reachable native regions "+variant);
   var mine=s.World.Mines.OrderBy(m=>Math.Abs((m.Cell&255)-builder.X/1024)+Math.Abs((m.Cell>>8)-builder.Y/1024)).First();Check(s.Order(builder,mine.Cell&255,(mine.Cell>>8)+1,true),"nearest native mine entrance reachable "+variant);builder.Route.Clear();s.Restart();
  }
  foreach(var r in new[]{new[]{0,-50,0,50},new[]{0,50,-50,0},new[]{0,50,0,-50},new[]{0,0,0,0},new[]{0,50,50,-50},new[]{0,0,-50,-50}}){string map=Simulation.SelectTenthMap(r);Check(map=="MONDE10"+(char)('A'+Enumerable.Range(1,3).OrderBy(i=>r[i]).ThenBy(i=>i).First()-1)+".ETA","native least-goodwill next variant "+string.Join(',',r));}
  // One natural route for each variant and each branch, alternating staffing speeds.
  foreach(char variant in "ABCD")foreach(int branch in Enumerable.Range(0,4)){
   var s=Load(root,variant,branch);int staff=(variant-'A'+branch)%2+1;Prepare(root,s,staff);long start=s.Tick;int notices=0;s.ResearchCompleted+=t=>notices++;var times=new List<double>();bool one=false,mid=false;
   Check(s.ToggleResearch(branch),"manual pause accepted");double progress=s.ResearchState(branch).Progress;Advance(s,90);Check(s.ResearchState(branch).Progress==progress,"manual pause freezes progress");Check(s.ToggleResearch(branch),"resume research");
   for(int i=0;i<33000&&!s.MissionWon&&!s.MissionLost;i++){
    long t=Stopwatch.GetTimestamp();s.Step();times.Add(Stopwatch.GetElapsedTime(t).TotalMilliseconds);
    if(!mid&&s.ResearchPercent>=25){Roundtrip(root,s,"mid-research save "+variant+branch);var clone=Load(root,variant,branch);clone.Restore(s.Serialize());Advance(s,30);Advance(clone,30);Check(s.Serialize()==clone.Serialize(),"deterministic resumed research "+variant+branch);mid=true;}
    if(!one&&s.MissionResearchCount==1){Check(!s.MissionWon,"one discovery never wins "+variant+branch);Roundtrip(root,s,"one-discovery save "+variant+branch);one=true;}
   }
   if(!s.MissionWon)File.WriteAllText(Path.Combine(folder,"blocked-"+variant+branch+".json"),s.Serialize());
   if(!s.MissionWon){
    runs.Add(new{Variant=variant,Branch=branch,Scientists=staff,Result=s.MissionLost?"Defeated":"Blocked",Tick=s.Tick,Discoveries=s.MissionResearchCount,Food=s.World.Resources[0],Stone=s.World.Resources[4],BuildingDamage=s.Opponent.BuildingDamage,MeanMs=times.Average(),P95Ms=times.Order().ElementAt((int)(times.Count*.95)),MaxMs=times.Max()});
    File.WriteAllText(Path.Combine(folder,"routes-progress.json"),JsonSerializer.Serialize(runs,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine("ROUTE M9 "+variant+branch+" result="+(s.MissionLost?"Defeated":"Blocked")+" discoveries="+s.MissionResearchCount);continue;
   }
   Check(!s.MissionLost&&s.MissionResearchCount==2&&notices==2,"natural completion "+variant+" branch="+branch+" workers="+staff);
   Check(!s.ExtraResearchOffered&&!s.Research.ExtraPurchased&&s.Tests.Technologies.Count==0&&!s.Tests.Victory,"two discoveries without debug grants/third charge");Roundtrip(root,s,"victory save "+variant+branch);
   s.RequestNextMission();Check(s.ChooseNextResearch(branch),"next research choice available after victory");Roundtrip(root,s,"transition saves with selected branch");var nextWorld=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/"+s.NextMissionMap));new VisualCatalog().Apply(nextWorld);var next=new Simulation(nextWorld,s.ProgressForNextMission());Check(next.MissionNumber==10&&next.World.MapName==s.NinthNextMap&&next.Campaign.CompletedMission==9,"natural transition to actual mission-ten file");Check(next.EffectiveLevels.Zip(s.EffectiveLevels).All(p=>p.First>=p.Second),"next mission preserves discoveries");
   runs.Add(new{Variant=variant,Branch=branch,Scientists=staff,Result="Victory",Tick=s.Tick,Seconds=(s.Tick-start)/30.0,NextMap=s.NextMissionMap,Food=s.World.Resources[0],Stone=s.World.Resources[4],Units=s.World.Units.Count,MeanMs=times.Average(),P95Ms=times.Order().ElementAt((int)(times.Count*.95)),MaxMs=times.Max(),Enemies=Enumerable.Range(1,3).Select(o=>new{Owner=o,Units=s.World.Units.Count(a=>a.Owner==o&&a.Health>0),Buildings=s.World.Buildings.Count(b=>b.Owner==o)}).ToArray()});
   File.WriteAllText(Path.Combine(folder,"routes-progress.json"),JsonSerializer.Serialize(runs,new JsonSerializerOptions{WriteIndented=true}));if(variant=='D'&&branch==2)File.WriteAllText(Path.Combine(folder,"natural-victory.snapshot.json"),s.Serialize());Console.WriteLine("ROUTE M9 "+variant+branch+" ticks="+s.Tick+" mean="+times.Average());
  }
  var dead=Load(root,'A');foreach(var a in dead.World.Units.Where(a=>a.Owner==0))a.Health=0;Check(dead.MissionLost&&!dead.MissionWon,"isolated total-loss fixture defeats player");long halted=dead.Tick;dead.Step();Check(dead.Tick==halted,"defeat stops simulation");dead.Restart();Check(!dead.MissionLost,"restart recovers from defeat");
  File.WriteAllText(output,JsonSerializer.Serialize(new{Passed=true,Checks=checks.Count,Assertions=checks,Runs=runs,Limits="Sixteen natural headless routes with native resources and ordinary commands. Navigation/forged-save/defeat cases are separate fixtures; no manual UI campaign or enemy rebalance."},new JsonSerializerOptions{WriteIndented=true}));
 }
}
