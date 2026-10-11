using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace RisingLands;
public static class MissionFourV102Tests {
 static int checks;
 static readonly Dictionary<Simulation,HashSet<int>> allocated=new();
 static void Check(bool ok,string label){if(!ok)throw new Exception("FAIL v102: "+label);checks++;Console.WriteLine("PASS v102: "+label);}
 static void Call(Simulation s,string method,int owner)=>typeof(Simulation).GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(s,new object[]{owner});
 static void Time(Simulation s,long tick)=>typeof(Simulation).GetProperty("Tick")!.SetValue(s,tick);
 static void Kill(Simulation s,int id){var a=s.World.Units.Single(a=>a.Slot==id);a.Health=0;s.Opponent.Sieges.Remove(id);s.CancelFeeding(a);typeof(Simulation).GetMethod("ClearOrders",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(s,new object[]{a});}
 static Actor Add(Simulation s,int type=0){
  if(!allocated.TryGetValue(s,out var used))allocated[s]=used=new();
  var a=s.World.Units.First(a=>a.Slot>=10000&&a.Type==type&&!used.Contains(a.Slot));
  used.Add(a.Slot);a.Health=type==9?100:s.TrainingRule(type).Health;a.Satiety=4;return a;
 }
 static Simulation Fixture(string root){
  var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE04.ETA"));new VisualCatalog().Apply(w);
  Array.Fill(w.Collision,(byte)0);Array.Fill(w.Tiles,(ushort)349);Array.Clear(w.Objects);Array.Clear(w.Quantities);
  var home=w.Buildings.First(b=>b.Owner==1);
  for(int i=0;i<302;i++){
   int type=i<200?0:i<300?8:i==300?5:9;
   var a=JsonSerializer.Deserialize<Actor>(JsonSerializer.Serialize(w.Units.First(u=>u.Type==type)))!;
   a.Slot=10000+i;a.Owner=1;a.LegacyState=7;a.X=((home.Cell&255)+6)*1024;a.Y=((home.Cell>>8)+6)*1024;
   a.Route.Clear();a.AttackTarget=-1;a.Satiety=4;
   if(type!=9){var r=Simulation.RecruitData.Rules.First(r=>r.Type==type);a.Health=r.Health;a.AnimationCount=r.AnimationCount;a.AnimationDelay=r.AnimationDelay;a.AnimationFrame=0;a.AnimationCounter=0;}
   w.Units.Add(a);
  }
  return new(w);
 }
 public static void Run(string root){
  var s=Fixture(root);Time(s,Simulation.MissionFourPressureTicks-1);
  Check(!s.MissionFourPressure(1)&&s.Opponent.MissionFourWaves.Count==0,"no pressure before 60 minutes");
  Time(s,Simulation.MissionFourPressureTicks);
  Check(s.MissionFourPressure(1),"pressure starts at exactly 108000 simulation ticks");
  for(int i=0;i<9;i++)Add(s,i%2==0?0:8);
  Add(s,5);Add(s,9);Call(s,"PrepareOpponentWave",1);
  Check(s.Opponent.Sorties.Count==0,"nine combat units plus civilians cannot launch");
  Add(s,0).Health=1;s.Opponent.NextWaveAt[1]=s.Tick+999999;Call(s,"PrepareOpponentWave",1);
  var wave=s.Opponent.MissionFourWaves[1];
  Check(wave.Members.Count==10&&wave.Members.All(id=>s.World.Units.Single(a=>a.Slot==id).Type is 0 or 8),"mixed first wave of ten ignores artificial cooldown and health threshold");
  Check(wave.Members.All(id=>s.Opponent.Sorties[id]==long.MaxValue),"no 90-second expiry");
  Check(wave.Members.All(id=>{var a=s.World.Units.Single(a=>a.Slot==id);return a.Route.Count>0||s.Opponent.Sieges.ContainsKey(id);}),"all attackers receive a direct player attack or movement order");
  Time(s,s.Tick+3000);Call(s,"AssessOpponentRetreat",1);Call(s,"UpdateEconomyPolicy",1);Call(s,"PrepareOpponentWave",1);
  Check(wave.Members.Count==10&&wave.Members.All(s.Opponent.Sorties.ContainsKey),"wounds and economic recovery do not recall the active wave");
  foreach(int id in wave.Members.Skip(1))Kill(s,id);
  for(int i=0;i<13;i++)Add(s);
  Call(s,"PrepareOpponentWave",1);Call(s,"OpponentRecruit",1);
  Check(wave.RequiredSize==10&&wave.Members.Count==10,"one surviving attacker prevents a new wave and escalation");
  var serialized=s.Serialize();var resumed=Fixture(root);resumed.Restore(serialized);
  Check(resumed.Serialize()==serialized,"active wave survives save/load exactly");
  var corrupt=JsonNode.Parse(serialized)!;corrupt["Opponent"]!["MissionFourWaves"]!["1"]!["RequiredSize"]=9;
  try{resumed.Restore(corrupt.ToJsonString());throw new Exception("Invalid wave accepted");}catch(InvalidDataException){}
  Check(resumed.Serialize()==serialized,"invalid wave state is rejected without changing the saved simulation");
  Kill(s,wave.Members[0]);Call(s,"PrepareOpponentWave",1);
  Check(wave.RequiredSize==13&&wave.Members.Count==13,"complete destruction launches thirteen immediately");
  foreach(int expected in new[]{17,22,28,35,44}){
   foreach(int id in wave.Members)Kill(s,id);
   Call(s,"PrepareOpponentWave",1);Check(wave.RequiredSize==expected&&wave.Members.Count==0,"ceil 25 percent escalation to "+expected);
   for(int i=0;i<expected;i++)Add(s);
   Call(s,"PrepareOpponentWave",1);Check(wave.Members.Count==expected,"full wave launches at "+expected);
  }
  foreach(int id in wave.Members)Kill(s,id);
  Call(s,"PrepareOpponentWave",1);s.World.Resources[1]=0;s.World.Resources[5]=0;s.World.Resources[9]=0;
  int jobs=s.Recruitments.Count;Call(s,"OpponentRecruit",1);
  Check(wave.Members.Count==0&&s.Recruitments.Count==jobs&&s.World.Resources[1]==0,"economic incapacity cannot create free troops or undersized attacks");
  var old=JsonNode.Parse(serialized)!;old["Opponent"]!.AsObject().Remove("MissionFourWaves");
  resumed=Fixture(root);resumed.Restore(old.ToJsonString());Call(resumed,"PrepareOpponentWave",1);
  Check(resumed.Opponent.MissionFourWaves[1].RequiredSize==10,"old saves initialize first wave at ten");
  resumed.Restart();Check(resumed.Tick==0&&resumed.Opponent.MissionFourWaves.Count==0,"restarting mission clears timer and wave progression");
  s=Fixture(root);Time(s,108000);for(int i=0;i<10;i++)Add(s);s.Campaign.Alliances[1]=true;Call(s,"PrepareOpponentWave",1);
  Check(s.Opponent.Sorties.Count==0,"allied enemy does not attack");
  s.Campaign.Alliances[1]=false;foreach(var a in s.World.Units.Where(a=>a.Owner==0))a.Health=0;Call(s,"PrepareOpponentWave",1);
  Check(s.Opponent.Sorties.Count==0,"player elimination stops waves");
  s=Fixture(root);Time(s,108000);for(int i=0;i<10;i++)Add(s);Call(s,"PrepareOpponentWave",1);s.CompleteTestMission();
  long finishedTick=s.Tick;s.Step();Call(s,"OpponentRecruit",1);
  Check(s.Tick==finishedTick&&s.Recruitments.Count==0,"mission completion stops simulation and offensive recruitment");
  s=StrategyV96Tests.Load(root,"MONDE05.ETA",true);Time(s,200000);Call(s,"PrepareOpponentWave",1);
  Check(!s.MissionFourPressure(1)&&s.Opponent.MissionFourWaves.Count==0&&s.Opponent.Sorties.Count==0,"mission five retains normal economy restrictions");
  Console.WriteLine("ALL v102 MISSION FOUR TESTS PASSED: "+checks);
 }
 public static void Replay(string root,string path){
  var hash=System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path));
  var s=OpponentV75Tests.Load(root);s.Restore(File.ReadAllText(path));
  long start=s.Tick;int initialArmy=s.World.Units.Count(a=>a.Owner==1&&a.Health>0&&Simulation.AdvancedAttacks.Rules.Any(r=>r.Type==a.Type));
  Check(s.MissionFourPressure(1),"real saved mission is past sixty minutes");
  Call(s,"PrepareOpponentWave",1);
  int food=s.World.Resources[1],rock=s.World.Resources[5],parts=s.World.Resources[9],previousJobs=s.Recruitments.Count;
  Call(s,"OpponentRecruit",1);
  Check(initialArmy==8&&s.Recruitments.Any(j=>j.Owner==1&&!j.Finished&&Simulation.AdvancedAttacks.Rules.Any(r=>r.Type==j.Type)),"real eight-unit army starts paid recruitment beyond old cap");
  Check(s.Recruitments.Where(j=>j.Owner==1&&!j.Finished).All(j=>s.OpponentUnitUnlocked(1,j.Type)),"recruits obey actual technological unlocks");
  var paid=s.Recruitments.Skip(previousJobs).Select(j=>s.TrainingRule(j.Type)).ToArray();
  Check(food-s.World.Resources[1]==paid.Sum(r=>r.Food)&&rock-s.World.Resources[5]==paid.Sum(r=>r.Rock)&&parts-s.World.Resources[9]==paid.Sum(r=>r.Parts),"offensive production debits the full real costs");
  for(int i=0;i<36000&&!s.Opponent.MissionFourWaves[1].Members.Any();i++)s.Step();
  var wave=s.Opponent.MissionFourWaves[1];
  Check(wave.Members.Count==10,"real snapshot produces and dispatches ten without resource grants");
  Console.WriteLine($"REPLAY launchSeconds={(s.Tick-start)/30.0:F1} food={s.World.Resources[1]} rock={s.World.Resources[5]} members={string.Join(',',wave.Members)}");
  var resumed=OpponentV75Tests.Load(root);resumed.Restore(s.Serialize());
  Check(resumed.Serialize()==s.Serialize(),"real active offensive survives save/load");
  for(int i=0;i<300;i++){s.Step();resumed.Step();}
  Check(resumed.Serialize()==s.Serialize(),"real offensive continues deterministically after reload");
  var playerBuildings=s.World.Buildings.Where(b=>b.Owner==0).Select(b=>b.Slot).ToArray();
  int PlayerDamage()=>playerBuildings.Sum(id=>s.Opponent.BuildingDamage.GetValueOrDefault(id));
  int health=s.World.Units.Where(a=>a.Owner==0).Sum(a=>a.Health),damage=PlayerDamage();
  for(int i=0;i<18000&&!s.MissionWon&&s.World.Units.Where(a=>a.Owner==0).Sum(a=>a.Health)>=health&&PlayerDamage()<=damage;i++){
   s.Step();
   if(i%1800==0)Console.WriteLine("CONTACT STATE "+string.Join(";",wave.Members.Select(id=>s.World.Units.Single(a=>a.Slot==id)).Select(a=>$"id={a.Slot} hp={a.Health} at={a.X/1024},{a.Y/1024} route={a.Route.Count} siege={s.Opponent.Sieges.GetValueOrDefault(a.Slot)?.Building} target={a.AttackTarget} satiety={a.Satiety} feeding={s.Feeding(a)}")));
  }
  Console.WriteLine($"REPLAY contactSeconds={(s.Tick-start)/30.0:F1} playerHealthLoss={health-s.World.Units.Where(a=>a.Owner==0).Sum(a=>a.Health)} buildingDamage={PlayerDamage()-damage} won={s.MissionWon} allied={s.Allied(1)}");
  Check(s.World.Units.Where(a=>a.Owner==0).Sum(a=>a.Health)<health||PlayerDamage()>damage,"real offensive reaches and damages the player");
  Check(hash.SequenceEqual(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path))),"reference snapshot preserved byte for byte");
  Console.WriteLine("ALL v102 REAL SNAPSHOT TESTS PASSED: "+checks);
 }
}
