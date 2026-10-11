namespace RisingLands;
public sealed partial class Simulation {
 public const int OpponentGarrisonSize=3,OpponentEconomyRadius=48;
 long safetyTick=-1;List<Actor>? safetyUnits;readonly Dictionary<int,byte[]> opponentDanger=new();
 bool OpponentCivilian(Actor a)=>a.Type==9||OriginalWorkerPolicy.Carrier(a.Type);
 byte[] OpponentDanger(int owner){
  if(safetyTick/30!=Tick/30||safetyUnits!=World.Units||safetyTick<0){safetyTick=Tick;safetyUnits=World.Units;opponentDanger.Clear();}
  if(opponentDanger.TryGetValue(owner,out var cached))return cached;
  var danger=new byte[65536];opponentDanger[owner]=danger;var vision=OpponentVision(owner);var known=Opponent.Explored[owner];
  void Mark(int cell,int radius){foreach(int c in NearbyCells(cell,radius)){int distance=Math.Max(Math.Abs((c&255)-(cell&255)),Math.Abs((c>>8)-(cell>>8)));danger[c]=Math.Max(danger[c],(byte)(radius-distance+1));}}
  // Remember discovered buildings; moving troops are threats only while visible.
  foreach(var memory in Opponent.KnownStructures.GetValueOrDefault(owner,new()).Values.Where(m=>Hostile(owner,m.Building.Owner))){var b=memory.Building;var r=Rule(b.Type);Mark(b.Cell+r.Height/2*256+r.Width/2,12+Math.Max(r.Width,r.Height)/2);}
  foreach(var a in World.Units.Where(a=>Hostile(owner,a.Owner)&&FightingUnit(a)&&vision[a.Y/1024*256+a.X/1024]!=0))Mark(a.Y/1024*256+a.X/1024,Math.Max(5,CombatRule(a.Type).Range+4));
  return danger;
 }
 internal bool OpponentSafeCell(int owner,int cell)=>ValidMapCell(cell)&&(!OpponentEnabled||owner is <1 or >3||OpponentDanger(owner)[cell]==0);
 long resourceRadiusTick=-1;readonly Dictionary<int,int> resourceRadii=new();
 int ResourceSearchRadius(Actor a){
  if(a.Type!=7||Tick<300*30||World.Resources[4+a.Owner]>=PolicyRockReserve(a.Owner))return OpponentEconomyRadius;
  if(resourceRadiusTick!=Tick){resourceRadiusTick=Tick;resourceRadii.Clear();}
  if(resourceRadii.TryGetValue(a.Slot,out int radius))return radius;
  var known=Opponent.Explored[a.Owner];var reachable=OpponentReachableCells(a);
  bool Nearby(int cell)=>World.Buildings.Any(b=>b.Owner==a.Owner&&b.Type is 2 or 3 or 4&&CellDistance(cell,b.Cell)<=OpponentEconomyRadius);
  bool stone=World.Mines.Any(m=>known[m.Cell]!=0&&reachable[m.Cell]!=0&&Nearby(m.Cell)&&OpponentSafeCell(a.Owner,m.Cell));
  return resourceRadii[a.Slot]=stone?OpponentEconomyRadius:OpponentEconomyRadius*2;
 }
 bool OpponentLocalResource(Actor a,int cell)=>!OpponentEnabled||a.Owner is <1 or >3||OpponentSafeCell(a.Owner,cell)&&World.Buildings.Any(b=>b.Owner==a.Owner&&b.Type is 2 or 3 or 4&&Math.Max(Math.Abs((b.Cell&255)-(cell&255)),Math.Abs((b.Cell>>8)-(cell>>8)))<=ResourceSearchRadius(a));
 byte[]? OpponentRouteDanger(Actor a,bool meal)=>OpponentEnabled&&a.Owner is >0 and <4&&(OpponentCivilian(a)||meal)?OpponentDanger(a.Owner):null;
 static bool SafeRouteStep(byte[]? danger,int start,int from,int next)=>danger==null||danger[next]==0||danger[start]>0&&danger[next]<=danger[from];
 void ReturnOpponentToSafety(Actor a){
  var homes=World.Buildings.Where(b=>b.Owner==a.Owner&&b.Type is 2 or 3 or 4).OrderBy(b=>CellDistance(b.Cell,a.Y/1024*256+a.X/1024)).ToArray();
  foreach(var home in homes)foreach(int cell in NearbyCells(home.Cell,8).Where(c=>OpponentSafeCell(a.Owner,c)).OrderBy(c=>CellDistance(c,home.Cell)).ThenBy(c=>CellDistance(c,a.Y/1024*256+a.X/1024)))if(Order(a,cell&255,cell>>8,true,true))return;
 }
 void ProtectOpponentEconomy(int owner){
  foreach(var a in World.Units.Where(a=>a.Owner==owner&&VisualCatalog.Visible(a)&&!TradeDeliveryPending(a)&&OpponentCivilian(a)).OrderBy(a=>a.Slot)){
   bool sourceUnsafe=a.FoodJob is 1 or 2&&a.FoodSource>=0&&!OpponentLocalResource(a,a.FoodSource)||a.MineJob is 1 or 2&&a.MineIndex>=0&&!OpponentLocalResource(a,World.Mines[a.MineIndex].Cell);
   if(OpponentSafeCell(owner,a.Y/1024*256+a.X/1024)&&!sourceUnsafe&&a.Route.Take(8).All(c=>OpponentSafeCell(owner,c)))continue;
   CancelFeeding(a);ClearOrders(a);ResidentTrips.RemoveAll(t=>t.Slot==a.Slot);a.DefenseAcquireAfter=Tick+60;ReturnOpponentToSafety(a);
  }
 }
 double OpponentStrength(Actor a){var rule=CombatRule(a.Type);return Math.Sqrt(Math.Max(1,a.Health))*rule.Damage.Where(d=>d>0).DefaultIfEmpty(1).Average()/Math.Max(20,CombatCycle(a))*(1+Math.Min(10,rule.Range)*.06);}
 void AssessOpponentRetreat(int owner){
  if(MissionFourPressure(owner))return;
  var deployed=World.Units.Where(a=>a.Owner==owner&&FightingUnit(a)&&!TradeDeliveryPending(a)&&OpponentDeployed(a)).ToArray();if(deployed.Length==0)return;
  // Refresh observation before comparing a newly encountered force.
  var visible=OpponentVision(owner);
  var danger=World.Units.Where(a=>Hostile(owner,a.Owner)&&FightingUnit(a)&&visible[a.Y/1024*256+a.X/1024]!=0&&deployed.Any(d=>TileDistanceSquared(a,d)<=144)).ToArray();
  bool invasion=World.Units.Any(a=>Hostile(owner,a.Owner)&&FightingUnit(a)&&visible[a.Y/1024*256+a.X/1024]!=0&&NearOwnedBase(a,owner));
  bool outmatched=danger.Length>0&&danger.Sum(OpponentStrength)>deployed.Sum(OpponentStrength)*1.4;
  foreach(var a in deployed.Where(a=>invasion||outmatched||a.Health*4<=MaxHealth(a))){Opponent.Sorties.Remove(a.Slot);ClearOrders(a);a.DefenseAcquireAfter=Tick+60;ReturnOpponentToSafety(a);}
  if(invasion||outmatched)Opponent.NextWaveAt[owner]=Math.Max(Opponent.NextWaveAt[owner],Tick+60*30);
 }
 int OpponentPopulationTarget(int owner)=>Math.Max(8,2+World.Buildings.Where(b=>b.Owner==owner&&ResidentCapacity(b)>0).Sum(b=>Math.Min(ResidentCapacity(b),b.Type==2?3:b.Type==7?2:RecruitData.Rules.Any(r=>AdvancedAttacks.Rules.Any(a=>a.Type==r.Type)&&Produces(b.Type,r.Type))?3:1)));
 bool OpponentSeedNeeded(int owner)=>EconomicOpening(owner)?OpeningSeedNeeded(owner):!TradeOpening(owner)||World.Resources[owner]>=6&&World.Units.Count(u=>u.Owner==owner&&u.Type==9&&u.Health>0)<OpponentPopulationTarget(owner);
 internal int OpponentArmyCount(int owner)=>World.Units.Count(a=>a.Owner==owner&&FightingUnit(a));
}
