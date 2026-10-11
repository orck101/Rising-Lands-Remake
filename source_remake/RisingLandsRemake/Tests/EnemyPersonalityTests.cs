namespace RisingLands;
public static class EnemyPersonalityTests {
 public static void Run(string root){int checks=0;void Check(bool yes,string name){if(!yes)throw new Exception("FAIL personality: "+name);checks++;Console.WriteLine("PASS personality: "+name);}
  Check(EnemyPersonalities.For(1).Name=="Exército sustentado"&&EnemyPersonalities.For(1).Branches.SequenceEqual(new[]{0,1}),"red uses agriculture/military");
  Check(EnemyPersonalities.For(2).Name=="Fortaleza produtiva"&&EnemyPersonalities.For(2).Branches.SequenceEqual(new[]{0,2}),"yellow uses agriculture/engineering");
  Check(EnemyPersonalities.For(3).Name=="Indústria militar"&&EnemyPersonalities.For(3).Branches.SequenceEqual(new[]{2,1}),"green uses engineering/military");
  var colors=LegacyAssets.Palette(PackageLayout.Asset(root,"PLATGAME.PIE"));Check(colors[144].R>colors[144].G&&colors[136].R>colors[136].B&&colors[136].G>colors[136].B&&colors[48].G>colors[48].R,"owner colors match original red/yellow/green palette");
  for(int owner=1;owner<=3;owner++){
   var p=EnemyPersonalities.For(owner);foreach(int branch in p.Branches){var levels=Enumerable.Range(0,4).Select(EnemyResearchPolicy.Maximum).ToArray();levels[branch]=0;Check(EnemyPersonalities.ChooseResearch(owner,34,levels)==branch,"clan "+owner+" researches available specialty "+branch);}
   var capped=Enumerable.Range(0,4).Select(EnemyResearchPolicy.Maximum).ToArray();Check(EnemyPersonalities.ChooseResearch(owner,34,capped)==-1,"maxed clan never invents an unavailable level");
   var cycle=EnemyPersonalities.ProductionCycle(owner,new[]{0,1,8});Check(cycle.Length>0&&cycle.All(t=>t is 0 or 1 or 8),"locked aircraft and advanced troops absent from cycle");
   Check(EnemyPersonalities.ProductionCycle(owner,new[]{25}).All(t=>t==25),"legal fallback prevents a specialty from blocking another available troop");
  }
  Check(EnemyPersonalities.ProductionCycle(3,new[]{0,8,11,12,16})[0]==16,"industry prefers its unlocked aircraft");
  Check(EnemyPersonalities.For(2).Buildings.ToList().IndexOf(13)<EnemyPersonalities.For(3).Buildings.ToList().IndexOf(13),"fortress prioritizes circus before industrial profile");
  Check(EnemyPersonalities.For(3).Buildings.Take(3).SequenceEqual(new[]{7,5,18}),"industry prioritizes laboratory/workshop/heliport after basic services");
  foreach(int mission in new[]{4,5,7}){
   Simulation Load(){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD","WORLD",$"MONDE{mission:00}.ETA"));new VisualCatalog().Apply(w);return new(w);}
   var s=Load();var stocks=s.World.Resources.ToArray();var levels=Enumerable.Range(1,3).Select(o=>Enumerable.Range(0,4).Select(b=>s.OpponentLevel(o,b)).ToArray()).ToArray();
   foreach(int owner in s.World.Units.Where(a=>a.Owner is >0 and <4).Select(a=>a.Owner).Distinct()){
    Check(s.Campaign.EnemyBranches[owner]==EnemyPersonalities.ChooseResearch(owner,mission,levels[owner-1]),"mission "+mission+" initializes configured branch for clan "+owner);
    var profile=s.PolicyFor(owner);Check(owner==3?profile.ReserveMinutes==OpponentPolicyProfile.ForMission(mission).ReserveMinutes&&profile.Guards==OpponentPolicyProfile.ForMission(mission).Guards&&profile.MinimumWave>=5&&profile.ArmyTarget>=profile.MaximumWave+profile.Guards:profile==OpponentPolicyProfile.ForMission(mission),"survival unchanged and green prepares larger assaults");
   }
   Check(s.World.Resources.SequenceEqual(stocks)&&s.Constructions.Count==0&&s.Recruitments.Count==0,"configuration does not grant resources/buildings/troops");
   for(int i=0;i<300;i++)s.Step();string saved=s.Serialize();var copy=Load();copy.Restore(saved);Check(copy.Serialize()==saved,"mission "+mission+" profiles coexist with exact save/load");for(int i=0;i<300;i++){s.Step();copy.Step();}Check(s.Serialize()==copy.Serialize(),"mission "+mission+" deterministic continuation");
  }
  string snapshot=PackageLayout.Asset(root,"..","..","experiments","simulacao-missao05-20261004","snapshots","baseline-600s.json");
  if(File.Exists(snapshot)){var s=MissionFiveScript.Load(root);string saved=File.ReadAllText(snapshot);s.Restore(saved);var old=System.Text.Json.Nodes.JsonNode.Parse(saved)!;var current=System.Text.Json.Nodes.JsonNode.Parse(s.Serialize())!;Check(System.Text.Json.Nodes.JsonNode.DeepEquals(old["Resources"],current["Resources"])&&System.Text.Json.Nodes.JsonNode.DeepEquals(old["Campaign"]!["EnemyLevels"],current["Campaign"]!["EnemyLevels"])&&System.Text.Json.Nodes.JsonNode.DeepEquals(old["Campaign"]!["EnemyBranches"],current["Campaign"]!["EnemyBranches"]),"v103 snapshot migrates without changing current branch, levels or resources");}
  Console.WriteLine("ALL PERSONALITY TESTS PASSED: "+checks);
 }
}
