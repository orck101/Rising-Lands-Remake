namespace RisingLands;
public record OpponentPolicyProfile(int ReserveMinutes,int StableSeconds,int Guards,int MinimumWave,int MaximumWave,int ArmyTarget,int MaximumFarmers){
 public static OpponentPolicyProfile ForMission(int mission)=>mission<=4?new(10,180,3,2,3,8,8):mission<=6?new(7,120,3,2,5,12,10):mission<=10?new(4,90,4,3,7,16,12):new(2,60,4,3,10,20,14);
}
public sealed class EconomyObservation {public long Tick{get;set;}public long Production{get;set;}}
public sealed class OpponentEconomyPolicy {
 public bool Initialized{get;set;}public long LastSample{get;set;}public long StableTicks{get;set;}public bool Recovering{get;set;}
 public int LastWorkers{get;set;}public int LastBuildings{get;set;}public int KnownFood{get;set;}public double Income{get;set;}public double Maintenance{get;set;}
 public int Reserve{get;set;}public int FarmerTarget{get;set;}=2;public string Phase{get;set;}="Consolidacao";
 public List<EconomyObservation> Samples{get;set;}=new();
}
public sealed partial class Simulation {
 // Common baseline for every clan. This is the extension point for later personalities.
 public OpponentPolicyProfile PolicyFor(int owner){var policy=OpponentPolicyProfile.ForMission(MissionNumber);if(owner!=3)return policy;int minimum=MissionNumber<=6?5:MissionNumber<=10?8:10;int maximum=MissionNumber<=6?8:MissionNumber<=10?12:16;return policy with{MinimumWave=minimum,MaximumWave=maximum,ArmyTarget=Math.Max(policy.ArmyTarget,maximum+policy.Guards+2)};}
 public OpponentEconomyPolicy EconomyFor(int owner){if(!Opponent.Economies.TryGetValue(owner,out var policy))Opponent.Economies[owner]=policy=new();return policy;}
 int PolicyGuards(int owner)=>Opponent.BaseDefenses.TryGetValue(owner,out var d)?d.GarrisonTarget:PolicyFor(owner).Guards;
 long FoodProduction(int owner)=>Opponent.Deposited[owner]+Opponent.OverflowFood[owner]+Cultures.Where(c=>InitialBuildings.Any(b=>b.Slot==c.Building&&b.Owner==owner)||Constructions.Any(b=>b.Id==c.Building&&b.Owner==owner)).Sum(c=>(long)c.Food+c.Consumed);
 int SafeKnownFood(int owner){if(!Opponent.Explored.TryGetValue(owner,out var known))return 0;var worker=World.Units.FirstOrDefault(a=>a.Owner==owner&&a.Type==5&&a.Health>0&&VisualCatalog.Visible(a));if(worker==null)return 0;var reachable=OpponentReachableCells(worker);int total=0;for(int cell=0;cell<65536;cell++){if(known[cell]==0||!OpponentKnowsResource(owner,cell)||reachable[cell]==0||!IsFood(cell)||!OpponentLocalResource(worker,cell))continue;total+=World.Objects[cell]==67?World.Quantities[cell]:World.Objects[cell] is 51 or 52?2:1;}return total;}
 double PolicyMaintenance(int owner)=>World.Units.Where(a=>a.Owner==owner&&a.Health>0&&VisualCatalog.Visible(a)&&NeedsFood(a)).Sum(a=>1800.0/(Math.Max(1,FatiguePeriod(a))*a.SatietyMaximum));
 bool BasicServices(int owner)=>new[]{2,3,4}.All(t=>World.Buildings.Any(b=>b.Owner==owner&&b.Type==t&&Inhabited(b)))&&OpponentWorkers(owner,5)>0&&OpponentWorkers(owner,7)>0;
 bool BaseInvaded(int owner){var visible=OpponentVision(owner);return World.Units.Any(a=>Hostile(owner,a.Owner)&&FightingUnit(a)&&visible[a.Y/1024*256+a.X/1024]!=0&&NearOwnedBase(a,owner));}
 void UpdateEconomyPolicy(int owner){
  var e=EconomyFor(owner);var profile=PolicyFor(owner);int workers=World.Units.Count(a=>a.Owner==owner&&a.Health>0&&OriginalWorkerPolicy.Carrier(a.Type));int buildings=World.Buildings.Count(b=>b.Owner==owner);
  if(e.Initialized&&(workers<e.LastWorkers||buildings<e.LastBuildings)){e.Recovering=true;e.StableTicks=0;}
  e.LastWorkers=workers;e.LastBuildings=buildings;e.Initialized=true;e.Maintenance=PolicyMaintenance(owner);
  _=WarehouseExpansionNeeded(owner);
  int replacement=CultureFoodCost(OpponentLevel(owner,0))+Math.Max(TrainingRule(5).Food,TrainingRule(7).Food);
  e.Reserve=Math.Max(MissionNumber<=4?20:12,(int)Math.Ceiling(e.Maintenance*profile.ReserveMinutes)+replacement);
  if(e.Samples.Count==0||Tick-e.LastSample>=300){e.LastSample=Tick;e.KnownFood=SafeKnownFood(owner);e.Samples.Add(new(){Tick=Tick,Production=Math.Max(e.Samples.LastOrDefault()?.Production??0,FoodProduction(owner))});e.Samples.RemoveAll(s=>Tick-s.Tick>180*30);if(e.Samples.Count>19)e.Samples.RemoveAt(0);var first=e.Samples[0];e.Income=Tick>first.Tick?Math.Max(0,(e.Samples[^1].Production-first.Production)*1800.0/(Tick-first.Tick)):0;}
  int farmers=Math.Max(1,OpponentWorkers(owner,5));double perFarmer=e.Samples.Count<3?2.5:Math.Max(.5,e.Income/farmers);double trainingBudget=MissionNumber<=4?3:MissionNumber<=6?5:MissionNumber<=10?7:10;
  int affordableFields=e.KnownFood<8?2:e.KnownFood<20?4:profile.MaximumFarmers;
  e.FarmerTarget=Math.Clamp((int)Math.Ceiling((e.Maintenance+trainingBudget+1)/Math.Max(.35,perFarmer-.15)),2,Math.Min(profile.MaximumFarmers,affordableFields));
  bool solvent=BasicServices(owner)&&World.Resources[owner]>=e.Reserve&&e.Income>=e.Maintenance+1&&e.KnownFood>=Math.Max(5,(int)Math.Ceiling(e.Maintenance*2));
  if(solvent&&!BaseInvaded(owner))e.StableTicks=Math.Min(36000,e.StableTicks+30);else{e.StableTicks=0;if(e.Samples.Count>2||e.Recovering)e.Recovering=true;}
  if(e.StableTicks>=profile.StableSeconds*30)e.Recovering=false;
  e.Phase=e.Recovering?"Recuperacao":e.StableTicks<profile.StableSeconds*30?"Consolidacao":OpponentArmyCount(owner)<profile.Guards?"Defesa":"Equilibrada";
  if(!MissionFourPressure(owner)&&!EconomyReady(owner))foreach(var a in World.Units.Where(a=>a.Owner==owner&&Opponent.Sorties.ContainsKey(a.Slot)).ToArray()){Opponent.Sorties.Remove(a.Slot);ClearOrders(a);ReturnOpponentToSafety(a);}
 }
 public bool EconomyReady(int owner){var e=EconomyFor(owner);return e.Initialized&&!e.Recovering&&e.StableTicks>=PolicyFor(owner).StableSeconds*30&&BasicServices(owner)&&World.Resources[owner]>=e.Reserve&&e.Income>e.Maintenance&&e.KnownFood>=5;}
 int PrimaryBaseCell(int owner)=>InitialBuildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==3)?.Cell??InitialBuildings.First(b=>b.Owner==owner).Cell;
 Building? SecondaryBase(int owner)=>World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==2&&CellDistance(b.Cell,PrimaryBaseCell(owner))>=28);
 int SecondarySupport(int owner){var home=SecondaryBase(owner);if(home==null)return -1;foreach(int type in new[]{3,4,1})if(!World.Buildings.Any(b=>b.Owner==owner&&b.Type==type&&CellDistance(b.Cell,home.Cell)<=12)&&!Constructions.Any(c=>c.Owner==owner&&c.Type==type&&CellDistance(c.Cell,home.Cell)<=12))return type;return -1;}
 bool ExpansionReady(int owner)=>EconomyReady(owner)&&OpponentArmyCount(owner)>=PolicyGuards(owner)+PolicyFor(owner).MinimumWave&&World.Resources[owner]>=EconomyFor(owner).Reserve+Rule(2).Food+Rule(3).Food+Rule(4).Food&&World.Resources[4+owner]>=Rule(2).Rock+Rule(3).Rock+Rule(4).Rock&&World.Units.Count(a=>a.Owner==owner&&a.Type==9&&a.Health>0)>=6;
 int PolicyRockReserve(int owner)=>SecondaryBase(owner)==null&&EconomyReady(owner)&&OpponentArmyCount(owner)>=PolicyGuards(owner)+PolicyFor(owner).MinimumWave&&World.Units.Count(a=>a.Owner==owner&&a.Type==9&&a.Health>0)>=6?new[]{2,3,4,1}.Sum(t=>Rule(t).Rock):Math.Max(30,Rule(1).Rock);
 int FindSecondarySite(int owner){
  if(!ExpansionReady(owner)||SecondaryBase(owner)!=null||Constructions.Any(c=>c.Owner==owner&&c.Type==2&&!Complete(c)))return -1;
  var builder=World.Units.FirstOrDefault(a=>a.Owner==owner&&a.Type==7&&a.Health>0&&VisualCatalog.Visible(a)&&!TradeDeliveryPending(a));if(builder==null)return -1;
  var known=Opponent.Explored[owner];var reachable=OpponentReachableCells(builder);var rule=Rule(2);var food=Enumerable.Range(0,65536).Where(c=>known[c]!=0&&OpponentKnowsResource(owner,c)&&IsFood(c)&&OpponentSafeCell(owner,c)).ToArray();
  foreach(int cell in Enumerable.Range(0,65536).Where(c=>known[c]!=0&&reachable[c]!=0&&CellDistance(c,PrimaryBaseCell(owner))>=28&&CellDistance(c,PrimaryBaseCell(owner))<=OpponentEconomyRadius&&World.Buildings.Where(b=>b.Owner==owner).All(b=>CellDistance(c,b.Cell)>=20)).OrderBy(c=>CellDistance(c,PrimaryBaseCell(owner))).ThenBy(c=>c)){
   if(!food.Any(c=>CellDistance(c,cell)<=12)||PlacementError(builder,2,cell)!=null)continue;
   bool safe=true;for(int y=0;y<rule.Height;y++)for(int x=0;x<rule.Width;x++)if(!ValidMapCell(cell+y*256+x)||known[cell+y*256+x]==0||!OpponentSafeCell(owner,cell+y*256+x))safe=false;
   if(safe&&BuildingInteractionCells(new(-1,2,owner,cell)).Any(c=>reachable[c]!=0&&OpponentSafeCell(owner,c)))return cell;
  }return -1;
 }
 int PolicyConstruction(int owner){
  if(EconomicOpening(owner))return OpeningBuilding(owner);
  int planned=PlannedBuilding(owner);if(planned>=0)return planned;
  int trade=TradePriorityConstruction(owner);if(trade>=0)return trade;
  foreach(int type in new[]{2,3,4,1})if(!World.Buildings.Any(b=>b.Owner==owner&&b.Type==type)&&OpponentBuildingUnlocked(owner,type))return type;
  int support=SecondarySupport(owner);if(support>=0)return World.Resources[owner]>=EconomyFor(owner).Reserve+Rule(support).Food&&World.Resources[4+owner]>=Rule(support).Rock?support:-1;
  if(!MissionFourPressure(owner)&&(EconomyFor(owner).Recovering||OpponentArmyCount(owner)<PolicyGuards(owner)))return -1;
  foreach(int type in PersonalityFor(owner).Buildings)if(OpponentBuildingUnlocked(owner,type)&&!World.Buildings.Any(b=>b.Owner==owner&&b.Type==type)&&World.Resources[owner]>=DevelopmentMilitaryReserve(owner)+Rule(type).Food&&World.Resources[4+owner]>=Rule(type).Rock&&World.Resources[8+owner]>=Rule(type).Parts)return type;
  return FindSecondarySite(owner)>=0?2:-1;
 }
 void ValidateEconomyPolicies(Save s,OpponentState state){if(state.Economies==null||state.Economies.Count>3)throw new InvalidDataException("Invalid economic policy");foreach(var p in state.Economies){var e=p.Value;if(p.Key is <1 or >3||e==null||e.Samples==null||e.Samples.Count>19||e.LastSample<0||e.LastSample>s.Tick||e.StableTicks<0||e.StableTicks>36000||e.LastWorkers<0||e.LastBuildings<0||e.KnownFood<0||e.KnownFood>2000000||!double.IsFinite(e.Income)||e.Income<0||!double.IsFinite(e.Maintenance)||e.Maintenance<0||e.Reserve<0||e.FarmerTarget is <2 or >14||e.Samples.Any(x=>x.Tick<0||x.Tick>s.Tick||x.Production<0)||e.Samples.Zip(e.Samples.Skip(1)).Any(x=>x.First.Tick>=x.Second.Tick||x.First.Production>x.Second.Production)||e.Phase is not ("Consolidacao" or "Recuperacao" or "Defesa" or "Equilibrada"))throw new InvalidDataException("Invalid clan economic history");}}
}
