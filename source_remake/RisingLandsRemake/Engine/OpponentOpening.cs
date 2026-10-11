namespace RisingLands;
public sealed partial class Simulation {
 public const long OpponentPeaceTicks=3*60*30,OpponentRestrictedTicks=5*60*30;
 public int DefensePerception(Actor a){int range=Math.Max(1,CombatRule(a.Type).Range);return range>1?(range*3+1)/2:range*3;}
 public int OpponentWaveLimit=>PolicyFor(1).MaximumWave;
 public bool OpponentOffensiveTypeAllowed(int type)=>AdvancedAttacks.Rules.Any(r=>r.Type==type);
 bool NearOpponentBase(Actor a)=>NearOwnedBase(a,a.Owner);
 bool NearOwnedBase(Actor a,int owner)=>World.Buildings.Any(b=>b.Owner==owner&&Math.Abs(a.X/1024-((b.Cell&255)+Rule(b.Type).Width/2))<=8&&Math.Abs(a.Y/1024-((b.Cell>>8)+Rule(b.Type).Height/2))<=8);
 bool OpponentDeployed(Actor a)=>Opponent.Sorties.TryGetValue(a.Slot,out var until)&&until>Tick&&OpponentOffensiveTypeAllowed(a.Type);
 void PrepareOpponentWave(int owner){
  if(EconomicOpening(owner)){foreach(int slot in Opponent.Sorties.Keys.Where(slot=>World.Units.Any(a=>a.Slot==slot&&a.Owner==owner)).ToArray()){Opponent.Sorties.Remove(slot);var a=World.Units.First(a=>a.Slot==slot);ClearOrders(a);ReturnOpponentToSafety(a);}return;}
  if(MissionFourPressure(owner)){PrepareMissionFourWave(owner);return;}
  foreach(int slot in Opponent.Sorties.Keys.Where(slot=>!World.Units.Any(a=>a.Slot==slot&&a.Health>0)||Opponent.Sorties[slot]<=Tick).ToArray())Opponent.Sorties.Remove(slot);
  if(Tick<Opponent.NextWaveAt[owner]||Allied(owner)||!EconomyReady(owner)||DefenseEmergency(owner)||DefenseShortfall(owner))return;
  // One sortie at a time. Green prepares a larger combined assault.
  if(World.Units.Any(a=>a.Owner==owner&&OpponentDeployed(a)))return;
  var army=World.Units.Where(a=>a.Owner==owner&&FightingUnit(a)&&!TradeDeliveryPending(a)).ToArray();
  var guards=OffensiveGuardReservation(owner,army);
  var candidates=army.Where(a=>!guards.Contains(a.Slot)&&DefenseAvailable(a)&&a.Satiety>0&&a.Health*2>=MaxHealth(a)&&OpponentOffensiveTypeAllowed(a.Type)).OrderBy(a=>a.Slot).ToArray();
  var wave=owner==3?GreenAssaultWave(candidates):candidates.Take(PolicyFor(owner).MaximumWave).ToArray();
  var vision=OpponentVision(owner);var invaders=World.Units.Where(a=>Hostile(owner,a.Owner)&&FightingUnit(a)&&vision[a.Y/1024*256+a.X/1024]!=0&&NearOwnedBase(a,owner)).ToArray();
  if(invaders.Length>0||wave.Length<PolicyFor(owner).MinimumWave)return;
  if(wave.Length==0||owner==3&&!GreenAssaultReady(wave))return;
  int duration=owner==3?180*30:90*30;
  foreach(var a in wave)Opponent.Sorties[a.Slot]=Tick+duration;
  Opponent.NextWaveAt[owner]=Tick+duration;
 }
 void HoldOpponentAtBase(Actor a){
  if(a.AutomaticDefense)return;
  if(a.AttackTarget>=0||Sieging(a)||a.Route.Count>0)ClearOrders(a);
  if(NearOpponentBase(a))return;
  var home=World.Buildings.Where(b=>b.Owner==a.Owner).OrderBy(b=>Math.Abs(a.X/1024-(b.Cell&255))+Math.Abs(a.Y/1024-(b.Cell>>8))).FirstOrDefault();
  if(home==null)return;
  foreach(int cell in NearbyCells(home.Cell,6).OrderBy(c=>Math.Abs((c&255)-a.X/1024)+Math.Abs((c>>8)-a.Y/1024)))if(Order(a,cell&255,cell>>8,true))break;
 }
}
