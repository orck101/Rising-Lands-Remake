using System.Reflection;
using System.Text.Json.Nodes;
namespace RisingLands;
public static class PolicyV98Tests {
 static int checks;static void Check(bool ok,string label){checks++;if(!ok)throw new Exception("FAIL v98: "+label);Console.WriteLine("PASS v98: "+label);}
 static object? Call(Simulation s,string method,int owner)=>typeof(Simulation).GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(s,new object[]{owner});
 static void Time(Simulation s,long tick)=>typeof(Simulation).GetProperty("Tick")!.SetValue(s,tick);
 static void Ready(Simulation s,int owner){foreach(int type in new[]{2,3,4}){var b=s.World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==type);if(b==null){var home=s.World.Buildings.First(b=>b.Owner==owner);b=new(900+owner*10+type,type,owner,home.Cell+8+256*8);s.World.Buildings.Add(b);}if(!s.Inhabited(b)){var a=new Actor{Slot=99000+s.World.Units.Count,Owner=owner,Type=9,Health=100,LegacyState=14};s.World.Units.Add(a);s.World.BuildingResidents[b.Slot]=new(){a.Slot};}}
  var e=s.EconomyFor(owner);e.Initialized=true;e.Recovering=false;e.StableTicks=s.PolicyFor(owner).StableSeconds*30;e.Income=10;e.Maintenance=1;e.Reserve=20;e.KnownFood=30;s.World.Resources[owner]=100;
 }
 public static void Run(string root){
  var s=StrategyV96Tests.Load(root,"MONDE05.ETA",true);Check(s.PolicyFor(1)==s.PolicyFor(2)&&s.PolicyFor(2)==s.PolicyFor(3),"all three clans share the same baseline");Check(!ReferenceEquals(s.EconomyFor(1),s.EconomyFor(2)),"clans retain independent economic histories");
  Check(OpponentPolicyProfile.ForMission(4).ReserveMinutes>OpponentPolicyProfile.ForMission(5).ReserveMinutes&&OpponentPolicyProfile.ForMission(5).ReserveMinutes>OpponentPolicyProfile.ForMission(11).ReserveMinutes,"safety margin decreases through the campaign");Check(OpponentPolicyProfile.ForMission(11).ArmyTarget>OpponentPolicyProfile.ForMission(4).ArmyTarget,"later missions target larger armies");
  Ready(s,1);Call(s,"PrepareOpponentWave",1);Check(s.Opponent.Sorties.Count>0,"financially prepared clan may attack without waiting for an absolute timer");Check(s.World.Units.Count(a=>a.Owner==1&&a.Health>0&&Simulation.AdvancedAttacks.Rules.Any(r=>r.Type==a.Type)&&!s.Opponent.Sorties.ContainsKey(a.Slot))>=s.PolicyFor(1).Guards,"sortie preserves its garrison");
  s=StrategyV96Tests.Load(root,"MONDE05.ETA",true);Ready(s,1);s.EconomyFor(1).Income=.1;Time(s,200000);Call(s,"PrepareOpponentWave",1);Check(s.Opponent.Sorties.Count==0,"elapsed time never overrides an unsustainable economy");
  s=StrategyV96Tests.Load(root,"MONDE05.ETA",true);Ready(s,1);s.EconomyFor(1).Recovering=true;Call(s,"PrepareOpponentWave",1);Check(s.Opponent.Sorties.Count==0,"recovering clan does not launch attacks");
  s=StrategyV96Tests.Load(root);Call(s,"UpdateEconomyPolicy",1);var farmer=s.World.Units.First(a=>a.Owner==1&&a.Type==5);farmer.Health=0;Time(s,30);Call(s,"UpdateEconomyPolicy",1);Check(s.EconomyFor(1).Recovering&&s.EconomyFor(1).StableTicks==0,"worker loss triggers recovery and resets stability");
  s=StrategyV96Tests.Load(root);Ready(s,1);Check((int)Call(s,"FindSecondarySite",1)! == -1,"no distant base without known resources, reserves and troops");
  s=StrategyV96Tests.Load(root,"MONDE05.ETA",true);Ready(s,1);Array.Fill(s.Opponent.Explored[1],(byte)1);s.World.Resources[1]=300;s.World.Resources[5]=100;
  for(int i=0;i<6;i++)s.World.Units.Add(new Actor{Slot=99100+i,Owner=1,Type=9,Health=100,LegacyState=14});
  Check(s.DevelopmentPlan(1).RockReserve>=new[]{2,3,4,1}.Sum(t=>s.Rule(t).Rock),"eligible expansion raises real stone collection target for its support buildings");
  int primary=(int)Call(s,"PrimaryBaseCell",1)!;
  foreach(int cell in Enumerable.Range(0,65536).Where(c=>c%17==0&&s.GroundResourceSpace(c,1)&&s.OpponentSafeCell(1,c))){s.World.Objects[cell]=67;s.World.Quantities[cell]=10;}
  int site=(int)Call(s,"FindSecondarySite",1)!;Check(site>=0&&Math.Abs((site&255)-(primary&255))+Math.Abs((site>>8)-(primary>>8))>=28,"stable funded clan finds a distant safe second-base site");
  var expansionBuilder=s.World.Units.First(a=>a.Owner==1&&a.Type==7&&a.Health>0&&VisualCatalog.Visible(a));int expansionFood=s.World.Resources[1],expansionStone=s.World.Resources[5];
  Check(s.StartConstruction(expansionBuilder,2,site)&&s.World.Resources[1]==expansionFood-s.Rule(2).Food&&s.World.Resources[5]==expansionStone-s.Rule(2).Rock,"second sanctuary uses actual builder and construction costs");
  s=OpponentV75Tests.Load(root);int firstReady=-1,firstWave=-1;for(int i=0;i<120000;i++){s.Step();if(firstReady<0&&s.EconomyReady(1))firstReady=(int)s.Tick;if(firstWave<0&&s.Opponent.Sorties.Count>0)firstWave=(int)s.Tick;if(i%18000==0){var e=s.EconomyFor(1);Console.WriteLine($"STATE tick={s.Tick} phase={e.Phase} food={s.World.Resources[1]} income={e.Income:F2} upkeep={e.Maintenance:F2} reserve={e.Reserve} farmers={s.World.Units.Count(a=>a.Owner==1&&a.Type==5&&a.Health>0)}/{e.FarmerTarget} army={s.World.Units.Count(a=>a.Owner==1&&a.Health>0&&Simulation.AdvancedAttacks.Rules.Any(r=>r.Type==a.Type))} knownFood={e.KnownFood}");}}
  Check(s.Opponent.Deposited[1]>0&&s.Opponent.Deposited[5]>0,"real mission four collects actual food and stone");Check(s.World.Buildings.Count(b=>b.Owner==1&&b.Type==2)==1||s.World.Buildings.Where(b=>b.Owner==1&&b.Type==2).All(b=>s.World.Buildings.Where(x=>x.Owner==1&&x.Type==2&&x.Slot!=b.Slot).All(x=>Math.Abs((x.Cell&255)-(b.Cell&255))+Math.Abs((x.Cell>>8)-(b.Cell>>8))>=28)),"sanctuaries are not duplicated beside the first base");
  var copy=OpponentV75Tests.Load(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"economic observations survive save/load");for(int i=0;i<300;i++){s.Step();copy.Step();}Check(copy.Serialize()==s.Serialize(),"economic policy resumes deterministically");
  var invalid=JsonNode.Parse(s.Serialize())!;invalid["Opponent"]!["Economies"]!["1"]!["Income"]=-1;string before=s.Serialize();try{s.Restore(invalid.ToJsonString());throw new Exception("corrupt history accepted");}catch(InvalidDataException){}Check(s.Serialize()==before,"corrupt economic history rejected atomically");
  Check(firstReady>=0,"economy can reach stable readiness through real collection");Check(firstWave>=0,"developed economy can fund an actual defended offensive");Console.WriteLine($"ALL v98 POLICY TESTS PASSED: {checks}; ready={firstReady}; attack={firstWave}");
 }
}
