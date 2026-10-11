using System.Text.Json;
namespace RisingLands;
public static class MissionFiveEndingTests {
 public static void Run(string root,string output){
  int checks=0;void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: "+name);checks++;Console.WriteLine("PASS: "+name);}
  Simulation Load(int mission=5,CampaignProgress? campaign=null){var w=LegacyAssets.Map(PackageLayout.Asset(root,$"CD/WORLD/MONDE{mission:00}.ETA"));new VisualCatalog().Apply(w);return new(w,campaign);}
  void Tick(Simulation s,int n=110){for(int i=0;i<n;i++)s.Step();}
  var s=Load();Check(s.HasMissionEnding&&!s.MissionWon&&!s.MissionLost,"initial phase active with ending");
  Check(s.FifthRequiredClans.SequenceEqual(new[]{1,2,3}),"three hostile clans required");
  foreach(var o in Enumerable.Range(1,3)){s.Campaign.Alliances[o]=true;Check(!s.FifthRequiredClans.Contains(o),"alliance removes "+o);}
  Tick(s,1);Check(s.Exploration.ClansFound.Skip(1).All(v=>v)&&!s.MissionWon,"shared allied vision discovers clans before victory confirmation");
  foreach(int o in Enumerable.Range(1,3)){
   var scout=s.World.Units.First(a=>a.Owner==0&&VisualCatalog.Visible(a));var target=s.World.Buildings.First(b=>b.Owner==o);scout.X=(target.Cell&255)*1024;scout.Y=(target.Cell>>8)*1024;s.UpdateVisibility();Check(s.Exploration.ClansFound[o],"visible building discovers "+o);
  }
  s.Step();Check(s.MissionChecks>0,"confirmation starts");
  string partial=s.Serialize();var resumed=Load();resumed.Restore(partial);Check(resumed.Serialize()==partial,"partial discovery/progress exact save/load");
  s.Campaign.Alliances[2]=false;s.Step();Check(s.MissionChecks==0&&s.FifthRequiredClans.Contains(2),"broken alliance resets victory");
  Tick(resumed);Check(resumed.MissionWon,"diplomatic route wins");
  string win=resumed.Serialize();var loaded=Load();loaded.Restore(win);Check(loaded.Serialize()==win&&loaded.MissionWon,"victory save/load");
  long t=loaded.Tick;loaded.Step();Check(loaded.Tick==t,"victory no duplicate");loaded.RequestNextMission();Check(loaded.ChooseNextResearch(3),"next phase branch choice");var next=Load(6,loaded.ProgressForNextMission());Check(next.MissionNumber==6&&!next.OpponentEnabled,"campaign enters exploration phase six");
  foreach(int state in new[]{7,10,12}){var a=new Actor{Owner=1,Type=7,Health=10,LegacyState=state};Check(!Simulation.ClanEliminated(1,new[]{a},Array.Empty<Building>(),null),"living independent state "+state);}
  var resident=new Actor{Slot=100,Owner=1,Type=9,Health=10,LegacyState=14};var b=new Building(200,2,1,0);var ids=new Dictionary<int,List<int>>{{200,new(){100}}};
  Check(!Simulation.ClanEliminated(1,new[]{resident},new[]{b},ids),"resident protects occupied sanctuary");
  Check(Simulation.ClanEliminated(1,new[]{resident},new[]{b},new Dictionary<int,List<int>>()),"unused hidden seed does not protect empty building");
  Check(!Simulation.ClanEliminated(1,new[]{resident},new[]{b},null,new[]{new LaboratoryWorker{SettlerSlot=100,LaboratorySlot=200,Stage=1}}),"laboratory assignment counts without home residents");
  Check(!Simulation.ClanEliminated(1,new[]{resident},new[]{b},null,null,new[]{new Recruitment{Owner=1,SettlerSlot=100,BuildingSlot=200}}),"living trainee in valid production prevents elimination");
  resident.Health=0;Check(Simulation.ClanEliminated(1,new[]{resident},new[]{b},ids),"dead resident does not count");
  var military=Load();foreach(int o in Enumerable.Range(1,3)){military.Exploration.ClansFound[o]=true;foreach(var a in military.World.Units.Where(a=>a.Owner==o))a.Health=0;}
  Check(military.World.Buildings.Any(b=>b.Owner>0)&&military.FifthRequiredClans.Length==0,"empty enemy buildings do not block");Tick(military);Check(military.MissionWon,"military route wins with empty ruins intact");
  var mixed=Load();mixed.Campaign.Alliances[1]=true;foreach(int o in Enumerable.Range(1,3)){mixed.Exploration.ClansFound[o]=true;if(o!=1)foreach(var a in mixed.World.Units.Where(a=>a.Owner==o))a.Health=0;}
  Tick(mixed);Check(mixed.MissionWon,"mixed route wins");
  var dead=Load();foreach(var a in dead.World.Units.Where(a=>a.Owner==0))a.Health=0;Check(dead.ClanEliminated(0)&&!dead.MissionLost&&!dead.MissionWon,"player extinction remains classified but temporary defeat disabled");t=dead.Tick;for(int i=0;i<300;i++)dead.Step();Check(dead.Tick==t+300&&dead.Opponent.Economies.Values.Any(e=>e.LastSample>t),"enemy economy and simulation continue without player survivors");var deadCopy=Load();deadCopy.Restore(dead.Serialize());Check(!deadCopy.MissionLost&&deadCopy.ClanEliminated(0),"observation after extinction survives save/load");for(int i=0;i<30;i++){dead.Step();deadCopy.Step();}Check(dead.Serialize()==deadCopy.Serialize(),"extinct-player observation continues deterministically");dead.Restart();Check(!dead.MissionLost&&!dead.Exploration.ClansFound.Any(v=>v),"restart clears discovery and defeat");
  var legacy=System.Text.Json.Nodes.JsonNode.Parse(partial)!;legacy["Version"]=34;legacy["Exploration"]!.AsObject().Remove("ClansFound");var old=Load();old.Restore(legacy.ToJsonString());Check(old.Exploration.ClansFound.Skip(1).All(v=>v),"legacy discovery reconstructed from explored base footprints");
  var forged=System.Text.Json.Nodes.JsonNode.Parse(win)!;forged["Exploration"]!["ClansFound"]![1]=false;try{Load().Restore(forged.ToJsonString());throw new Exception("forged victory accepted");}catch(InvalidDataException){Check(true,"victory missing discovery rejected");}
  Check(!Load(4).IsFifthMission&&Load(6).HasMissionEnding,"other mission boundaries");
  File.WriteAllText(output,JsonSerializer.Serialize(new{Checks=checks,Passed=true,Rule="User approved remake policy",Limit="Engine simulations, no manual UI playthrough"}));
 }
}
