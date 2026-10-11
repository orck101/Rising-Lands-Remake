using System.Text.Json;
namespace RisingLands;
public record HungerRule(int Type,int Period,bool Fatigues);
public record HungerCatalog(List<HungerRule> Rules,int StarvationDamage);
public record FatigueBonus(int Agriculture,int Type,int Bonus);
public record FatigueTechnology(List<FatigueBonus> Rows);
public sealed class FeedingTrip {
 public int Slot{get;set;}public int Ground{get;set;}=-1;public int Warehouse{get;set;}=-1;public int Arrival{get;set;}=-1;
 public int ReturnCell{get;set;}=-1;public int AnchorCell{get;set;}=-1;
 public List<int> PausedRoute{get;set;}=new();
}
public sealed class HungerState {public long[] Consumed{get;set;}=new long[4];public long[] SharedFoodNet{get;set;}=new long[4];public List<FeedingTrip> Trips{get;set;}=new();}
public sealed partial class Simulation {
 public static readonly HungerCatalog HungerData=JsonSerializer.Deserialize<HungerCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/hunger_rules.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true,PropertyNamingPolicy=JsonNamingPolicy.SnakeCaseLower})!;
 static readonly Dictionary<int,HungerRule> HungerRules=HungerData.Rules.ToDictionary(r=>r.Type);
 static readonly Dictionary<(int,int),int> FatigueBonuses=JsonSerializer.Deserialize<FatigueTechnology>(File.ReadAllText(PackageLayout.FromBase("Content/Data/fatigue_technology.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!.Rows.ToDictionary(r=>(r.Agriculture,r.Type),r=>r.Bonus);
 public int FatiguePeriod(Actor a)=>HungerRules[a.Type].Period+FatigueBonuses.GetValueOrDefault((a.Owner==0?AgricultureResearch.Index:a.Owner is >0 and <4?OpponentLevel(a.Owner,0):0,a.Type));
 public HungerState Hunger{get;private set;}=new();
 public bool Feeding(Actor a)=>Hunger.Trips.Any(t=>t.Slot==a.Slot&&t.Arrival>=0);
 public const int GroundMealRadius=20,WarehouseMealRadius=20,MealSearchRadius=GroundMealRadius,MealMaximumPath=40;
 static bool MealNear(int anchor,int cell,int radius=GroundMealRadius){if(anchor<0||cell<0)return false;int dx=(anchor&255)-(cell&255),dy=(anchor>>8)-(cell>>8);return dx*dx+dy*dy<=radius*radius;}
 bool MealRoute(Actor a,int cell,int radius=GroundMealRadius){if(!Order(a,cell&255,cell>>8,true,true))return false;if(a.Route.Count<=radius*2)return true;a.Route.Clear();return false;}
 public bool NeedsFood(Actor a)=>a.Owner!=4&&HungerRules.GetValueOrDefault(a.Type)?.Fatigues==true;
 public void CancelFeeding(Actor a)=>Hunger.Trips.RemoveAll(t=>t.Slot==a.Slot);
 int[] nearbyMeals=Array.Empty<int>();long mealCacheTick=-1;
 public bool LivingCombatTroop(Actor a)=>a.Type is 0 or 1 or 2 or 3 or 4 or 8 or 25;
 bool ActiveForFatigue(Actor a)=>a.Satiety==0||LivingCombatTroop(a)||a.Route.Count>0||a.FoodJob!=0||a.MineJob!=0||a.AttackTarget>=0||Feeding(a)||Repairing(a)||a.Type==7&&Constructions.Any(c=>!Complete(c)&&HasBuilder(c,a.Slot));
 void HungerStep(){
  foreach(var a in World.Units){
   if(a.Health<=0){CancelFeeding(a);continue;}if(a.PetrifiedUntil>Tick||WaitingForFlight(a)||TradeEntering(a)||!VisualCatalog.Visible(a)||!NeedsFood(a))continue;
   int period=FatiguePeriod(a);
   if(ActiveForFatigue(a)&&++a.FatigueClock>=period){
    if(a.Satiety>0){a.Satiety--;a.FatigueClock=0;}
    else{a.Health=Math.Max(0,a.Health-HungerData.StarvationDamage);a.FatigueClock=a.Health==0?period:HungerRules[a.Type].Period/2;EmitUnitSound(a,a.Health==0?5:0xc,a.Health==0?2:4);if(a.Health==0){CancelFeeding(a);ClearOrders(a);continue;}}
   }
   if(a.Satiety>0)continue;
   
   var trip=Hunger.Trips.FirstOrDefault(t=>t.Slot==a.Slot);
   if(trip!=null&&trip.AnchorCell<0)trip.AnchorCell=a.Y/1024*256+a.X/1024;
   if(trip!=null&&(!MealStillValid(a,trip)||a.Route.Count>MealMaximumPath||!OpponentSafeCell(a.Owner,trip.Arrival))){Hunger.Trips.Remove(trip);ResumeMealTask(a,trip);trip=null;}
   if(trip==null){
    if(Tick%60!=0)continue;
    var pending=new FeedingTrip{Slot=a.Slot,AnchorCell=a.Y/1024*256+a.X/1024,PausedRoute=new(a.Route),ReturnCell=LivingCombatTroop(a)&&a.Route.Count==0&&a.AttackTarget<0?a.Y/1024*256+a.X/1024:-1};
    FindMeal(a,pending);
    if(pending.Arrival<0){a.Route=pending.PausedRoute;continue;}
    trip=pending;Hunger.Trips.Add(trip);a.AttackWindup=0;EmitUnitSound(a,0xc,4);
   }
   bool arrived=trip.Arrival>=0&&a.Route.Count==0;
   if(arrived&&trip.Ground>=0&&a.X==(trip.Arrival&255)*1024&&a.Y==(trip.Arrival>>8)*1024&&World.Objects[trip.Ground]==67&&World.Quantities[trip.Ground]>0){
    if(--World.Quantities[trip.Ground]==0)World.Objects[trip.Ground]=0;FinishMeal(a,trip);continue;
   }
   if(arrived&&trip.Warehouse>=0){var b=World.Buildings.FirstOrDefault(b=>b.Slot==trip.Warehouse&&b.Type==3);if(b!=null&&!Hostile(a.Owner,b.Owner)&&Inhabited(b)&&World.Resources[b.Owner]>0){if(ReachBuildingInteraction(a,b,trip.Arrival)){World.Resources[b.Owner]--;if(b.Owner!=a.Owner){Hunger.SharedFoodNet[a.Owner]++;Hunger.SharedFoodNet[b.Owner]--;}FinishMeal(a,trip);}continue;}}
   if(a.Route.Count==0){Hunger.Trips.Remove(trip);ResumeMealTask(a,trip);}
  }
 }
 void FinishMeal(Actor a,FeedingTrip trip){
  Hunger.Consumed[a.Owner]++;a.Satiety=a.SatietyMaximum;a.FatigueClock=0;Hunger.Trips.Remove(trip);ResumeMealTask(a,trip);
 }
 void ResumeMealTask(Actor a,FeedingTrip trip){a.Route.Clear();
  if(trip.PausedRoute.Count>0){int goal=trip.PausedRoute[^1];Order(a,goal&255,goal>>8,true);}
  // Stationary work has retained its source/destination, cadence and cargo. Return there before resuming it.
  else if(trip.ReturnCell>=0)Order(a,trip.ReturnCell&255,trip.ReturnCell>>8,true);
  else if(a.FoodJob is 1 or 2&&a.FoodSource>=0&&a.FoodSourceBuilding<0)Order(a,a.FoodSource&255,a.FoodSource>>8,true);
  else if(a.FoodJob==3&&a.DeliveryCell>=0)Order(a,a.DeliveryCell&255,a.DeliveryCell>>8,true);
  else if(a.MineJob!=0&&a.MineWorkCell>=0){if(a.MineJob==2)a.MineJob=1;Order(a,a.MineWorkCell&255,a.MineWorkCell>>8,true);}
  else if(Constructions.FirstOrDefault(c=>!Complete(c)&&HasBuilder(c,a.Slot)) is {} site){var worker=Workers(site).First(w=>w.BuilderSlot==a.Slot);Order(a,worker.WorkCell&255,worker.WorkCell>>8,true);}
 }
 void FindMeal(Actor a,FeedingTrip trip){
  if(mealCacheTick<0||Tick-mealCacheTick>=60){nearbyMeals=Enumerable.Range(0,65536).Where(c=>World.Objects[c]==67&&World.Quantities[c]>0).ToArray();mealCacheTick=Tick;}
  if(a.Owner is <0 or >3)return;
  var candidates=new List<(int Cell,int Warehouse)>();
  candidates.AddRange(nearbyMeals.Where(c=>MealNear(trip.AnchorCell,c)&&World.Objects[c]==67&&World.Quantities[c]>0&&(!OpponentEnabled||a.Owner==0||OpponentKnowsResource(a.Owner,c))).Select(c=>(c,-1)));
  foreach(var b in World.Buildings.Where(b=>b.Type==3&&!Hostile(a.Owner,b.Owner)&&Inhabited(b)&&World.Resources[b.Owner]>0))
   candidates.AddRange(BuildingInteractionCells(b).Where(c=>MealNear(trip.AnchorCell,c)).Select(c=>(c,b.Slot)));
  int Distance(int cell){int dx=(cell&255)-(trip.AnchorCell&255),dy=(cell>>8)-(trip.AnchorCell>>8);return dx*dx+dy*dy;}
  foreach(var candidate in candidates.OrderBy(c=>Distance(c.Cell)).ThenBy(c=>c.Cell).ThenBy(c=>c.Warehouse)){
   if(!MealRoute(a,candidate.Cell))continue;
   trip.Arrival=candidate.Cell;trip.Warehouse=candidate.Warehouse;trip.Ground=candidate.Warehouse<0?candidate.Cell:-1;return;
  }
 }
 bool MealStillValid(Actor a,FeedingTrip trip){
  if(!MealNear(trip.AnchorCell,trip.Arrival))return false;
  if(trip.Ground>=0)return World.Objects[trip.Ground]==67&&World.Quantities[trip.Ground]>0;
  return World.Buildings.Any(b=>b.Slot==trip.Warehouse&&b.Type==3&&!Hostile(a.Owner,b.Owner)&&Inhabited(b)&&World.Resources[b.Owner]>0);
 }
 void RestoreLocalHunger(int version){
  foreach(var trip in Hunger.Trips.ToArray()){var a=World.Units.First(u=>u.Slot==trip.Slot);if(version<36||trip.AnchorCell<0){trip.AnchorCell=a.Y/1024*256+a.X/1024;if(trip.ReturnCell>=0&&!MealNear(trip.AnchorCell,trip.ReturnCell))trip.ReturnCell=-1;}
   if(trip.Arrival<0){Hunger.Trips.Remove(trip);ResumeMealTask(a,trip);continue;}
   if(trip.Arrival>=0&&(!MealStillValid(a,trip)||a.Route.Count>MealMaximumPath)){Hunger.Trips.Remove(trip);ResumeMealTask(a,trip);}
  }
 }
 void ValidateHunger(Save s){
  if(s.Version<29){foreach(var a in s.Units){a.Satiety=4;a.SatietyMaximum=4;a.FatigueClock=0;}return;}
  var h=s.Hunger??throw new InvalidDataException("Missing hunger state");
  
  if(h.Consumed==null||h.Consumed.Length!=4||h.Consumed.Any(n=>n<0||n>100000000)||h.Trips==null||h.Trips.Count>s.Units.Count||h.Trips.Select(t=>t.Slot).Distinct().Count()!=h.Trips.Count)throw new InvalidDataException("Invalid hunger ledger");
  if(h.SharedFoodNet==null||h.SharedFoodNet.Length!=4||h.SharedFoodNet.Any(n=>n< -100000000||n>100000000)||h.SharedFoodNet.Sum()!=0||h.SharedFoodNet.Where(n=>n>0).Sum()>h.Consumed.Sum())throw new InvalidDataException("Invalid shared food ledger");
  foreach(var a in s.Units)if(a.SatietyMaximum is <1 or >30||a.Satiety<0||a.Satiety>a.SatietyMaximum||a.FatigueClock is <0 or >65535)throw new InvalidDataException("Invalid hunger reserve");
  foreach(var t in h.Trips){var a=s.Units.FirstOrDefault(a=>a.Slot==t.Slot);if(a==null||a.Health<=0||a.Satiety!=0||!NeedsFood(a)||t.AnchorCell< -1||t.AnchorCell>=65536||t.AnchorCell>=0&&!ValidMapCell(t.AnchorCell)||t.ReturnCell< -1||t.ReturnCell>=65536||t.ReturnCell>=0&&((t.ReturnCell&255)>=World.Width||(t.ReturnCell>>8)>=World.Height||!LivingCombatTroop(a))||t.Ground< -1||t.Ground>=65536||t.Arrival< -1||t.Arrival>=65536||t.Warehouse< -1||t.Ground>=0&&t.Warehouse>=0||t.Ground>=0&&t.Arrival!=t.Ground||t.PausedRoute==null||t.PausedRoute.Count>65536||t.PausedRoute.Any(c=>c<0||c>=65536||(c&255)>=World.Width||(c>>8)>=World.Height))throw new InvalidDataException("Invalid feeding trip");if(t.Warehouse>=0&&!InitialBuildings.Concat(s.Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).Any(b=>b.Slot==t.Warehouse&&b.Type==3&&!Hostile(a.Owner,b.Owner)&&ValidBuildingArrivalCells(b).Contains(t.Arrival)))throw new InvalidDataException("Invalid feeding warehouse");}
  mealCacheTick=-1;
 }
}
public sealed partial class ResearchGame {
 void DrawHungerMeters(Actor? hovered,bool all){foreach(var a in simulation.World.Units.Where(a=>VisualCatalog.Visible(a)&&SeenUnit(a)&&!VisualCatalog.HiddenByWork(a)&&simulation.NeedsFood(a)&&UnitInfoRules.Show(a.Owner,selectedSlots.Contains(a.Slot),a==hovered,all)))Command(new DrawCommand("player",-8,-2,0x1e2b+Math.Clamp(a.Satiety,0,30)),a.X/32f,a.Y/32f);}
}



