namespace RisingLands;
public static class OpeningV79Tests {
 public static void Run(string root){
  int checks=0;void Check(bool ok,string name){if(!ok)throw new Exception("FAIL v79: "+name);checks++;Console.WriteLine("PASS v79: "+name);}
  void Clock(Simulation s,long tick)=>typeof(Simulation).GetProperty(nameof(Simulation.Tick))!.SetValue(s,tick);
  void Invoke(Simulation s,string method,params object[] args)=>typeof(Simulation).GetMethod(method,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(s,args);
  Simulation Map(){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE04.ETA"));new VisualCatalog().Apply(w);return new(w);}
  var s=Map();Check(s.OpponentEnabled,"real mission four opponent enabled");
  foreach(long tick in new long[]{0,30,5399}){Clock(s,tick);Check(s.OpponentWaveLimit==0&&!s.OpponentOffensiveTypeAllowed(0)&&!s.OpponentOffensiveTypeAllowed(8),"no offensive permission before three minutes at "+tick);Invoke(s,"PrepareOpponentWave",1);Check(s.Opponent.Sorties.Count==0,"no dispatched wave during peace");}
  Clock(s,5400);Check(s.OpponentWaveLimit==2&&s.OpponentOffensiveTypeAllowed(0)&&s.OpponentOffensiveTypeAllowed(1)&&!s.OpponentOffensiveTypeAllowed(8),"three-minute boundary permits only two swordsmen/scorchers");
  Clock(s,8999);Check(s.OpponentWaveLimit==2&&!s.OpponentOffensiveTypeAllowed(8),"ranged offensive locked until five minutes");
  Clock(s,9000);Check(s.OpponentWaveLimit==3&&s.OpponentOffensiveTypeAllowed(8),"five-minute boundary opens gradual three-unit wave");
  Clock(s,12600);Check(s.OpponentWaveLimit==4,"seven-minute cap increases gradually");
  s=Map();for(int i=0;i<300;i++)s.Step();Check(s.World.Units.Where(a=>a.Owner is >0 and <4).All(a=>a.AttackTarget<0)&&s.Opponent.Sieges.Count==0,"real opening does not attack or siege player");
  Clock(s,5400);Invoke(s,"PrepareOpponentWave",1);Check(s.Opponent.Sorties.Count<=2&&s.Opponent.Sorties.Keys.All(id=>s.World.Units.Single(a=>a.Slot==id).Type is 0 or 1),"real sortie never deploys early ranged troops or oversized group");
  int waveCount=s.Opponent.Sorties.Count;Check(waveCount>0,"real early sortie contains eligible troops");Invoke(s,"PrepareOpponentWave",1);Check(s.Opponent.Sorties.Count==waveCount,"repeated decision does not accumulate extra waves");
  var resumed=Map();resumed.Restore(s.Serialize());Check(resumed.Serialize()==s.Serialize(),"opening and wave state survive save/load");for(int i=0;i<30;i++){s.Step();resumed.Step();}Check(s.Serialize()==resumed.Serialize(),"opening continues deterministically after reload");
  Simulation Fixture(bool archer,int distance){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);w=Simulation.TrainingWorld(w,archer);Array.Fill(w.Collision,(byte)0);Array.Fill(w.Tiles,(ushort)349);Array.Clear(w.Objects);Array.Clear(w.Quantities);var ours=w.Units.First(a=>a.Owner==0&&a.Type==(archer?8:0));var enemy=w.Units.Single(a=>a.Owner==1);ours.X=20*1024;ours.Y=20*1024;enemy.X=(20+distance)*1024;enemy.Y=20*1024;var friend=w.Units.First(a=>a.Owner==0&&a.Type==5);friend.X=enemy.X;friend.Y=enemy.Y+1024;friend.LastAttacker=enemy.Slot;friend.LastHitTick=0;return new(w);}
  s=Fixture(true,1);var archer=s.World.Units.First(a=>a.Type==8&&a.Owner==0);int range=s.CombatRule(8).Range,perception=s.DefensePerception(archer);Check(perception==(range*3+1)/2,"ranged perception is 150 percent rounded up");
  s=Fixture(true,perception);archer=s.World.Units.First(a=>a.Type==8&&a.Owner==0);var foe=s.World.Units.Single(a=>a.Owner==1);Invoke(s,"AutomaticDefenseStep");Check(archer.AttackTarget==foe.Slot&&!s.InAttackRange(archer,foe),"ranged troop detects ally aggressor outside weapon range");int start=archer.X;for(int i=0;i<90;i++)s.Step();Check(archer.X!=start,"ranged troop walks to defend instead of extending weapon range");Check(s.CombatRule(8).Range==range,"weapon range unchanged");
  s=Fixture(true,perception+1);archer=s.World.Units.First(a=>a.Type==8&&a.Owner==0);Invoke(s,"AutomaticDefenseStep");Check(archer.AttackTarget<0,"aggressor outside expanded perception not acquired");
  s=Fixture(false,2);var melee=s.World.Units.First(a=>a.Type==0&&a.Owner==0);foe=s.World.Units.Single(a=>a.Owner==1);Check(s.DefensePerception(melee)==2*s.CombatRule(0).Range,"melee perception doubled");Invoke(s,"AutomaticDefenseStep");Check(melee.AttackTarget==foe.Slot&&!s.InAttackRange(melee,foe),"melee rescues at doubled perception without remote attack");
  Console.WriteLine("PASS v79 total "+checks);
 }
}
