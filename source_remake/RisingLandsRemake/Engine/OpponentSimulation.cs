namespace RisingLands;

public sealed class OpponentState {
 public Dictionary<int,Dictionary<int,UnitMemory>> KnownUnits{get;set;}=new();public Dictionary<int,Dictionary<int,StructureMemory>> KnownStructures{get;set;}=new();public Dictionary<int,byte[]> KnownResources{get;set;}=new();
 public Dictionary<int,BaseDefenseState> BaseDefenses{get;set;}=new();
 public Dictionary<int,MissionFourWave> MissionFourWaves{get;set;}=new();
 public Dictionary<int,OpponentEconomyPolicy> Economies{get;set;}=new();
 public bool ReducedOpening{get;set;}
 public bool TradeFirstOpening{get;set;}
 public bool BalancedFifthOpening{get;set;}
 public long[] NextWaveAt{get;set;}=new long[4];
 public Dictionary<int,long> Sorties{get;set;}=new();
 public double[] ResearchProgress{get;set;}=new double[4];
 public int[] ResearchCompleted{get;set;}=new int[4];
 public Dictionary<int,int> BuildingDamage{get;set;}=new();
 public Dictionary<int,BuildingRepairOrder> Repairs{get;set;}=new();
 public Dictionary<int,long> DestructionAt{get;set;}=new();
 public Dictionary<int,SiegeOrder> Sieges{get;set;}=new();
 public long[] Deposited{get;set;}=new long[12];
 public long[] OverflowFood{get;set;}=new long[4];
 public bool[] WarehouseExpansionRequested{get;set;}=new bool[4];
 public Dictionary<int,byte[]> Explored{get;set;}=new();
 public Dictionary<int,int> Patrol{get;set;}=new();
 public Dictionary<int,int> TrainingSerial{get;set;}=new();
 public Dictionary<int,int> TargetsSeen{get;set;}=new();
}

public sealed partial class Simulation {
 public OpponentState Opponent{get;private set;}=new();
 public bool OpponentEnabled=>MissionNumber>=4&&NativeOpponentPlans.For(World)!=null&&InitialActors.Values.Any(a=>a.Owner is >0 and <4);
 // Native resources, actors, buildings, costs and combat tables; the decision policy
 // below is a deterministic remake policy, not a translation of the RULE interpreter.
 public bool Hostile(int first,int second)=>SeventhHostile(first,second)||first!=second&&first is >=0 and <=3&&second is >=0 and <=3&&!(first==0?Allied(second):second==0&&Allied(first));
 static long TileDistanceSquared(Actor a,Actor b){long x=(a.X-b.X)/1024,y=(a.Y-b.Y)/1024;return x*x+y*y;}
 bool FightingUnit(Actor a)=>a.Health>0&&VisualCatalog.Visible(a)&&a.Type!=10&&SupportedFighter(a);
 byte[] OpponentVision(int owner){
  var visible=SharedCurrentVision(owner);RefreshOpponentKnowledge(owner,visible);
  foreach(var a in World.Units.Where(a=>Hostile(owner,a.Owner)&&VisualCatalog.Visible(a)&&visible[a.Y/1024*256+a.X/1024]!=0))Opponent.TargetsSeen[a.Slot]=a.Y/1024*256+a.X/1024;
  return visible;
 }
 void OpponentStep(){
  if(IsEighthMission){OceanOpponentStep();return;}
  if(Tick%10==0){CoordinatedDefenseStep();AutomaticDefenseStep();}
  if(OriginalOpponentActive){OriginalOpponentStep();return;}
  if(!OpponentEnabled||Tick%30!=0)return;
  foreach(int owner in InitialActors.Values.Select(a=>a.Owner).Where(o=>o is >0 and <4).Distinct().Order()){
   if(IsFifthMission&&ClanEliminated(owner))continue;
   var vision=OpponentVision(owner);
   UpdateEconomyPolicy(owner);
   ProtectOpponentEconomy(owner);
   OpponentEconomy(owner,vision);
   if(MissionFourPressure(owner))PrepareMissionFourWave(owner);
   if(DefenseShortfall(owner)&&Tick%150==0)OpponentRecruit(owner);
   if(MissionFourPressure(owner)&&MissionFourCombatContinues(owner)){
    if(!MissionFourActiveWave(owner)){OpponentRecruit(owner);DevelopOpponent(owner);OpponentExpand(owner);}
    else if(Tick%300==0){DevelopOpponent(owner);OpponentExpand(owner);}
   }else if(!MissionFourPressure(owner)&&Tick%(MissionNumber==5?150:300)==0){DevelopOpponent(owner);OpponentRecruit(owner);OpponentExpand(owner);}
   AssessOpponentRetreat(owner);PrepareOpponentWave(owner);
   foreach(var a in World.Units.Where(a=>a.Owner==owner&&FightingUnit(a)&&!TradeDeliveryPending(a)&&!ResidentReserved(a.Slot)&&!Feeding(a)).OrderBy(a=>a.Slot)){
    if(BaseDefenseControls(a))continue;
    if(!OpponentDeployed(a)){HoldOpponentAtBase(a);continue;}
    var target=World.Units.Where(b=>b.Type!=10&&Hostile(owner,b.Owner)&&CanAttack(a,b)&&vision[b.Y/1024*256+b.X/1024]!=0).OrderBy(b=>FightingUnit(b)?0:1).ThenBy(b=>TileDistanceSquared(a,b)).ThenBy(b=>b.Slot).FirstOrDefault();
    if(target!=null){if(a.AttackTarget!=target.Slot){ClearOrders(a);a.AttackTarget=target.Slot;}continue;}
    if(a.AttackTarget>=0){a.AttackTarget=-1;a.AttackWindup=0;a.Route.Clear();}
    if(Opponent.Sieges.ContainsKey(a.Slot))continue;
    var structure=owner==3?GreenSiegeTarget(a,vision):World.Buildings.Where(b=>Hostile(owner,b.Owner)&&NearbyCells(b.Cell,Math.Max(Rule(b.Type).Width,Rule(b.Type).Height)).Any(c=>Inside(c,b.Cell,Rule(b.Type).Width,Rule(b.Type).Height)&&vision[c]!=0)).OrderBy(b=>Math.Abs((b.Cell&255)-a.X/1024)+Math.Abs((b.Cell>>8)-a.Y/1024)).FirstOrDefault();
    if(structure!=null&&AttackBuilding(a,structure.Slot))continue;
    if(a.Route.Count>0)continue;
    // Deployed soldiers all advance; garrison orders are handled separately.
    int known=Opponent.KnownUnits.GetValueOrDefault(owner,new()).Values.Where(m=>Hostile(owner,m.Owner)).OrderBy(m=>Math.Abs((m.Cell&255)-a.X/1024)+Math.Abs((m.Cell>>8)-a.Y/1024)).Select(m=>m.Cell).FirstOrDefault(-1);
    if(known>=0&&a.Y/1024*256+a.X/1024!=known){if(Order(a,known&255,known>>8,true))continue;}
    if(MissionFourPressure(owner)){AdvanceMissionFourAttacker(a);continue;}
    Scout(a,owner);
   }
  }
 }
 IEnumerable<int> NearbyCells(int center,int radius){for(int y=Math.Max(0,(center>>8)-radius);y<=Math.Min(World.Height-1,(center>>8)+radius);y++)for(int x=Math.Max(0,(center&255)-radius);x<=Math.Min(World.Width-1,(center&255)+radius);x++)yield return y*256+x;}
 void Scout(Actor a,int owner){
  int index=Opponent.Patrol.GetValueOrDefault(a.Slot);int columns=Math.Max(1,(World.Width-8)/12),rows=Math.Max(1,(World.Height-8)/12);
  for(int attempt=0;attempt<Math.Min(8,columns*rows);attempt++,index++){
   int n=index%(columns*rows),y=4+(n/columns)*12,x=4+((n/columns)%2==0?n%columns:columns-1-n%columns)*12,cell=y*256+x;
   Opponent.Patrol[a.Slot]=index+1;
   // Terrain knowledge is not current intelligence: patrol unseen sectors even
   // when their ground layout was supplied at the start of the mission.
   if(OpponentVision(owner)[cell]!=0)continue;
   Opponent.Patrol[a.Slot]=index+1;
   foreach(int c in NearbyCells(cell,2).OrderBy(c=>Math.Abs((c&255)-x)+Math.Abs((c>>8)-y)))if(Order(a,c&255,c>>8,true))return;
  }
 }
 void OpponentEconomy(int owner,byte[] vision){
  var depot=World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==3&&Inhabited(b));if(depot==null)return;
  var crop=World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==4&&Inhabited(b));BalanceOpponentWorkers(owner,depot,crop);
  var suppliers=CropSupplierSlots(owner,crop);
  foreach(var a in World.Units.Where(a=>a.Owner==owner&&OriginalWorkerPolicy.Carrier(a.Type)&&VisualCatalog.Visible(a)&&!TradeDeliveryPending(a)&&!ResidentReserved(a.Slot)&&!Feeding(a)&&a.FoodJob==0&&a.MineJob==0&&a.Route.Count==0&&!Repairing(a)&&!Constructions.Any(c=>!Complete(c)&&HasBuilder(c,a.Slot))).OrderBy(a=>a.Slot)){
   var workerDepot=World.Buildings.Where(b=>b.Owner==owner&&b.Type==3&&Inhabited(b)).OrderBy(b=>Math.Abs((b.Cell&255)-a.X/1024)+Math.Abs((b.Cell>>8)-a.Y/1024)).FirstOrDefault()??depot;
   int destination=a.Type==5&&suppliers.Contains(a.Slot)&&crop!=null&&OpponentSeedNeeded(owner)&&Residents(crop.Slot)<ResidentCapacity(crop)?crop.Slot:workerDepot.Slot;
   if(a.Food>0){SetFoodDestination(a,2,destination);continue;}if(a.Rock>0){SetRockDestination(a,depot.Slot);continue;}
   int kind=WorkerResourceKind(a);if(!OpponentResourceNeeded(owner,kind))continue;a.CargoKind=kind;
   int anchor=World.Buildings.First(b=>b.Slot==destination).Cell;
   var reachable=OpponentReachableCells(a);
   var sources=NearbyCells(anchor,Math.Max(World.Width,World.Height)).Where(c=>Opponent.Explored[owner][c]!=0&&IsCargoSource(a,c)&&reachable[c]!=0).OrderBy(c=>World.Units.Count(w=>w!=a&&w.Owner==owner&&w.FoodJob is 1 or 2&&w.FoodSource==c)).ThenBy(c=>Math.Abs((c&255)-(anchor&255))+Math.Abs((c>>8)-(anchor>>8))+Math.Abs((c&255)-a.X/1024)+Math.Abs((c>>8)-a.Y/1024)).ThenBy(c=>c);
   var allSources=sources.ToArray();var localSources=allSources.Where(c=>Math.Abs((c&255)-(anchor&255))<=48&&Math.Abs((c>>8)-(anchor>>8))<=48).ToArray();var sourceList=localSources.Length>0?localSources:allSources;int sourceOffset=(int)((Tick/30*8)%Math.Max(1,sourceList.Length));
   bool found=false;foreach(int c in sourceList.Take(24))if(Gather(a,c,false)){a.RepeatFood=a.PersistentCollection=true;SetFoodDestination(a,2,destination);found=true;break;}
   if(!found&&a.Type==7)foreach(int i in Enumerable.Range(0,World.Mines.Count).Where(i=>Opponent.Explored[owner][World.Mines[i].Cell]!=0&&OpponentLocalResource(a,World.Mines[i].Cell)).OrderBy(i=>Math.Abs((World.Mines[i].Cell&255)-a.X/1024)+Math.Abs((World.Mines[i].Cell>>8)-a.Y/1024)))if(StartMining(a,i,false)){a.RepeatMine=true;SetRockDestination(a,depot.Slot);found=true;break;}
   if(!found&&OpponentSafeCell(owner,a.Y/1024*256+a.X/1024))ExploreOpponentResources(a,owner);
  }

 }
 void OpponentRecruit(int owner){
  bool defensive=DefenseShortfall(owner);
  if(!DefenseEmergency(owner)&&EconomicWorkerRecovery(owner))return;
  if(EconomicOpening(owner)&&(!DefenseEmergency(owner)||!defensive))return;
  if(MissionFourPressure(owner)&&!defensive&&(!MissionFourCombatContinues(owner)||MissionFourActiveWave(owner)))return;
  if(!defensive&&(RedOpeningEconomyPending(owner)||TradeOpening(owner)&&TradePriorityConstruction(owner)>=0))return;
  // Native initial technologies authorize production, plus original mission troops.
  if(OpponentArmyCount(owner)+Recruitments.Count(j=>j.Owner==owner&&!j.Finished&&AdvancedAttacks.Rules.Any(r=>r.Type==j.Type))>=(MissionFourPressure(owner)?MissionFourRequiredSize(owner)+PolicyGuards(owner):OpponentArmyTarget(owner)))return;
  var available=PlannedArmyCycle(owner,AdvancedAttacks.Rules.Select(r=>r.Type).Where(type=>OpponentUnitUnlocked(owner,type)&&RecruitData.Rules.Any(r=>r.Type==type)));if(available.Length==0)return;available=DefensiveArmyCycle(owner,available);
  foreach(var b in World.Buildings.Where(b=>b.Owner==owner&&Inhabited(b)).OrderBy(b=>b.Slot)){
   if(Recruitments.Any(j=>j.BuildingSlot==b.Slot&&!j.Finished)||Residents(b.Slot)<=1)continue;
   int serial=Opponent.TrainingSerial.GetValueOrDefault(b.Slot);
   for(int i=0;i<available.Length;i++){int type=available[(defensive?i:serial+i)%available.Length];if(!Produces(b.Type,type))continue;var r=TrainingRule(type);if(World.Resources[owner]<r.Food+DevelopmentMilitaryReserve(owner))continue;
    if(TrainOpponent(owner,b,type)){Opponent.TrainingSerial[b.Slot]=serial+i+1;break;}
   }
  }
 }
 void OpponentExpand(int owner){
  StaffOpponentWarehouses(owner);
  // Native demand order and ghost positions; city-group assignment is a bridge.
  foreach(var b in World.Buildings.Where(b=>b.Owner==owner&&!Inhabited(b)&&ResidentCapacity(b)>0).OrderBy(b=>b.Type==3?0:1).ToArray()){
   if(ResidentTrips.Any(j=>j.Building==b.Slot))continue;
   int reserved=WarehouseOperatorSlot(owner);
   var settler=World.Units.FirstOrDefault(a=>a.Owner==owner&&a.Type==9&&a.Slot!=reserved&&VisualCatalog.Visible(a)&&!TradeDeliveryPending(a)&&a.Route.Count==0&&!ResidentReserved(a.Slot));
   if(settler==null){var home=World.Buildings.FirstOrDefault(h=>h.Owner==owner&&h.Type==2&&OpponentFreeResidents(h)>(World.BuildingResidents.GetValueOrDefault(h.Slot,new()).Contains(reserved)?2:1));if(home!=null){var candidate=World.Units.FirstOrDefault(a=>a.Slot!=reserved&&a.Owner==owner&&a.Type==9&&a.LegacyState==14&&a.Health>0&&World.BuildingResidents.GetValueOrDefault(home.Slot,new()).Contains(a.Slot)&&!ResidentReserved(a.Slot));if(candidate!=null)settler=ReleaseResident(home.Slot,owner,candidate.Slot);}}
   if(settler!=null)EnterBuilding(settler,b.Slot);
  }
  var pending=Constructions.FirstOrDefault(c=>c.Owner==owner&&!Complete(c));
  if(pending!=null){foreach(var helper in World.Units.Where(a=>a.Owner==owner&&a.Type==7&&VisualCatalog.Visible(a)&&!TradeDeliveryPending(a)&&!ResidentReserved(a.Slot)&&a.Food==0&&a.Rock==0&&!Feeding(a)&&!Constructions.Any(c=>!Complete(c)&&HasBuilder(c,a.Slot))))if(Workers(pending).Count()<2)ResumeConstruction(helper,pending);return;}
  if(TryDefenseFortification(owner))return;int wanted=DevelopmentConstruction(owner);if(wanted<0)return;
  if(EconomicOpening(owner)&&OwnService(owner,3)&&World.Resources[owner]<Rule(wanted).Food+OpeningFoodFloor(owner))return;
  if(wanted==3&&WarehouseSummary(owner).ActiveCount>0&&(WarehouseEmergency(owner)||World.Resources[owner]<Rule(3).Food+WarehouseSurvivalReserve(owner)||WarehouseOperatorSlot(owner)<0))return;
  if(wanted!=3&&!WarehouseEmergency(owner)&&!WarehouseBudgetAllows(owner,Rule(wanted).Food,Rule(wanted).Rock,Rule(wanted).Parts))return;
  var builder=World.Units.FirstOrDefault(a=>a.Owner==owner&&a.Type==7&&VisualCatalog.Visible(a)&&!TradeDeliveryPending(a)&&!ResidentReserved(a.Slot)&&a.Food==0&&a.Rock==0&&!Feeding(a)&&!Constructions.Any(c=>!Complete(c)&&HasBuilder(c,a.Slot)));
  var depot=World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==3)??World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==2);if(builder==null||depot==null)return;
  int anchor=SecondarySupport(owner)==wanted&&SecondaryBase(owner) is {} secondary?secondary.Cell:depot.Cell;
  if(wanted==2&&World.Buildings.Any(b=>b.Owner==owner&&b.Type==2)){int remote=FindSecondarySite(owner);if(remote>=0)StartConstruction(builder,2,remote);return;}
  var plans=NativeOpponentPlans.For(World);if(plans==null)return;
  // Native positions and type mask; proximity/city assignment remains provisional.
  foreach(var plan in plans.Plans.Where(p=>p.Allows(wanted)&&ValidMapCell(p.Cell)&&CellDistance(p.Cell,anchor)<=(SecondarySupport(owner)==wanted?12:24)).OrderBy(p=>wanted==3?World.Units.Where(a=>a.Owner==owner&&a.Type==5&&a.FoodField>=0).Select(a=>Math.Abs((p.Cell&255)-(a.FoodField&255))+Math.Abs((p.Cell>>8)-(a.FoodField>>8))).DefaultIfEmpty(1000).Sum():p.Index).ThenBy(p=>p.Index)){
   int cell=plan.Cell;if(World.Buildings.Any(b=>b.Cell==cell)||Constructions.Any(c=>c.Cell==cell&&!Complete(c)))continue;
   if(wanted==3&&!SafeWarehouseSite(owner,builder,cell))continue;
   if(StartConstruction(builder,wanted,cell))return;
  }
  // Native ghost positions can be occupied after the player's new opening.
  // Search valid nearby grass instead of permanently stalling the economy.
  var cost=Rule(wanted);if(World.Resources[owner]<cost.Food||World.Resources[4+owner]<cost.Rock||World.Resources[8+owner]<cost.Parts)return;
  foreach(int cell in NearbyCells(anchor,SecondarySupport(owner)==wanted?12:24).OrderBy(c=>wanted==3?WarehouseSiteDistance(owner,c):CellDistance(c,anchor)).ThenBy(c=>c)){if(wanted==3&&!SafeWarehouseSite(owner,builder,cell))continue;if(StartConstruction(builder,wanted,cell))return;}
 }
 void ValidateOpponent(Save s){var state=s.Opponent;if(s.Version>=32&&state==null)throw new InvalidDataException("Missing opponent state");if(state==null)return;
  ValidateBaseDefenses(s,state);ValidateOpponentKnowledge(s,state);ValidateMissionFourWaves(s,state);
  if(state.BalancedFifthOpening&&MissionNumber!=5)throw new InvalidDataException("Invalid mission five opening");
  if(state.TradeFirstOpening&&(!state.ReducedOpening||MissionNumber!=4))throw new InvalidDataException("Invalid trade-first opening");
  if(state.ReducedOpening&&MissionNumber!=4)throw new InvalidDataException("Invalid reduced opening mission");
  if(state.NextWaveAt==null||state.NextWaveAt.Length!=4||state.NextWaveAt.Any(t=>t<0)||state.Sorties==null||state.Sorties.Any(p=>p.Value<0||!s.Units.Any(a=>a.Slot==p.Key&&a.Owner is >0 and <4)))throw new InvalidDataException("Invalid opponent opening state");
  if(state.ResearchProgress==null||state.ResearchProgress.Length!=4||state.ResearchProgress.Any(p=>!double.IsFinite(p)||p<0||p>=1)||state.ResearchCompleted==null||state.ResearchCompleted.Length!=4||state.ResearchCompleted.Any(n=>n<0||n>2))throw new InvalidDataException("Invalid enemy laboratory research");
  ValidateSieges(s,state);ValidateRepairs(s,state);ValidateEconomyPolicies(s,state);
  if(state.OverflowFood==null||state.OverflowFood.Length!=4||state.OverflowFood.Any(v=>v<0))throw new InvalidDataException("Invalid harvested overflow history");
  if(state.WarehouseExpansionRequested==null||state.WarehouseExpansionRequested.Length!=4||state.WarehouseExpansionRequested[0])throw new InvalidDataException("Invalid warehouse expansion requests");
  if(state.Deposited==null||state.Deposited.Length!=12||state.Deposited.Any(v=>v<0)||state.Explored==null||state.Patrol==null||state.TrainingSerial==null||state.TargetsSeen==null||state.Explored.Any(p=>p.Key is <1 or >3||p.Value==null||p.Value.Length!=65536||p.Value.Any(v=>v>1))||state.Patrol.Any(p=>p.Value<0||!s.Units.Any(a=>a.Slot==p.Key&&a.Owner is >0 and <4))||state.TrainingSerial.Any(p=>p.Value<0||!InitialBuildings.Concat(s.Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).Any(b=>b.Slot==p.Key&&b.Owner is >0 and <4))||state.TargetsSeen.Any(p=>!ValidMapCell(p.Value)||!s.Units.Any(a=>a.Slot==p.Key)))throw new InvalidDataException("Invalid opponent state");
 }
}
