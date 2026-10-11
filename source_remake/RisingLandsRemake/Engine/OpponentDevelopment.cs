namespace RisingLands;
public record OpponentDevelopmentPlan(string Phase,int Farmers,int CropSuppliers,int FoodReserve,int RockReserve,int PartsReserve,int ProductionResidents);
public sealed partial class Simulation {
 public OpponentDevelopmentPlan DevelopmentPlan(int owner){var e=EconomyFor(owner);var decision=DecisionFor(owner);return new(e.Phase,decision.Farmers,1,e.Reserve,decision.StoneTarget,decision.PartsTarget,3);}
 int DevelopmentConstruction(int owner)=>PolicyConstruction(owner);
 int DevelopmentMilitaryReserve(int owner)=>DefenseShortfall(owner)?DefenseFoodReserve(owner):MissionFourPressure(owner)?0:EconomyFor(owner).Reserve;
 int DevelopmentResidents(Building b,OpponentDevelopmentPlan plan,bool workersNeeded)=>workersNeeded?4:b.Type==7?2:b.Type is 3 or 4?1:plan.ProductionResidents;
 int OpponentFreeResidents(Building b)=>World.BuildingResidents.GetValueOrDefault(b.Slot,new()).Count(id=>World.Units.Any(a=>a.Slot==id&&a.Type==9&&a.Health>0&&a.LegacyState==14)&&!ResidentReserved(id));
 int OpponentWorkers(int owner,int type)=>World.Units.Count(a=>a.Owner==owner&&a.Type==type&&a.Health>0&&!ResidentTrips.Any(t=>t.Slot==a.Slot&&World.Buildings.Any(home=>home.Slot==t.Building&&home.Type==2)))+Recruitments.Count(j=>j.Owner==owner&&j.Type==type&&!j.Finished);
 bool TrainOpponent(int owner,Building b,int type){
  if(!OpeningTrainingAllowed(owner,type))return false;
  if(!OpponentUnitUnlocked(owner,type)||!Produces(b.Type,type)||!Inhabited(b)||Recruitments.Any(j=>j.BuildingSlot==b.Slot&&!j.Finished)||OpponentFreeResidents(b)<=1)return false;
  var r=TrainingRule(type);if(World.Resources[owner]<r.Food||World.Resources[4+owner]<r.Rock||World.Resources[8+owner]<r.Parts)return false;
  bool essential=type==5&&OpponentWorkers(owner,5)<2||type==7&&OpponentWorkers(owner,7)==0;
  if(type is 5 or 6 or 7&&!essential&&!(EconomicOpening(owner)&&type==5&&OwnService(owner,3))&&(CoreResidentDeficit(owner)>0||type==5&&OpponentWorkers(owner,5)>=2&&!PopulationEstablished(owner)))return false;
  bool pressureRecruit=MissionFourPressure(owner)&&OpponentOffensiveTypeAllowed(type);bool defensiveRecruit=AdvancedAttacks.Rules.Any(r=>r.Type==type)&&DefenseShortfall(owner);
  if(defensiveRecruit&&World.Resources[owner]<r.Food+DefenseFoodReserve(owner))return false;
  if(!AdvancedAttacks.Rules.Any(rule=>rule.Type==type)&&!pressureRecruit&&!defensiveRecruit&&!WarehouseEmergency(owner)&&!WarehouseBudgetAllows(owner,r.Food,r.Rock,r.Parts))return false;
  int operatorSlot=pressureRecruit||WarehouseEmergency(owner)&&type is 5 or 7?-1:WarehouseOperatorSlot(owner);
  int slot=World.BuildingResidents.GetValueOrDefault(b.Slot,new()).FirstOrDefault(id=>id!=operatorSlot&&World.Units.Any(a=>a.Slot==id&&a.Type==9&&a.Health>0&&a.LegacyState==14)&&!ResidentReserved(id),-1);if(slot<0||OpponentFreeResidents(b)-(operatorSlot>=0&&World.BuildingResidents.GetValueOrDefault(b.Slot,new()).Contains(operatorSlot)?1:0)<=1)return false;
  World.Resources[owner]-=(ushort)r.Food;World.Resources[4+owner]-=(ushort)r.Rock;World.Resources[8+owner]-=(ushort)r.Parts;
  bool worker=type is 5 or 6 or 7;
  Recruitments.Add(new(){Owner=owner,BuildingSlot=b.Slot,SettlerSlot=slot,Type=type,SourceBuildingSlot=worker?-1:b.Slot,TravelCell=worker?-1:b.Cell});return true;
 }
 void DevelopOpponent(int owner){
  PlanPopulation(owner);PlanRepairs(owner);
  StaffOpponentWarehouses(owner);
  // Recover indispensable workers before waiting for the first food deposit.
  int recovery=OpponentWorkers(owner,5)==0?5:OpponentWorkers(owner,7)==0?7:-1;
  if(recovery>=0)foreach(var sanctuary in World.Buildings.Where(b=>b.Owner==owner&&b.Type==2))if(TrainOpponent(owner,sanctuary,recovery))return;
  if(EconomicOpening(owner)){DevelopEconomicOpening(owner);return;}
  if(TradeOpening(owner)){StaffTradeOpening(owner);if(!World.Buildings.Any(b=>b.Owner==owner&&b.Type==2&&Inhabited(b)))return;}
  if(RedOpeningEconomyPending(owner)){
   var depot=World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==3&&!Inhabited(b));
   if(depot!=null&&!ResidentTrips.Any(t=>t.Building==depot.Slot)){var home=World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==2&&OpponentFreeResidents(b)>1);if(home!=null){var colonist=ReleaseResident(home.Slot,owner);if(colonist!=null)EnterBuilding(colonist,depot.Slot);}}
   return;
  }
  var plan=DevelopmentPlan(owner);int farmers=OpponentWorkers(owner,5);
  bool armyNeedsResidents=World.Buildings.Any(b=>b.Owner==owner&&b.Type==1&&OpponentFreeResidents(b)<=1);
  int army=OpponentArmyCount(owner);
  int desiredFarmers=Math.Max(ProductiveFarmerFloor(owner),army<PolicyGuards(owner)?Math.Min(4,plan.Farmers):plan.Farmers);
  var decision=DecisionFor(owner);
  int workerType=farmers<1?5:OpponentWorkers(owner,7)<1?7:OpponentWorkers(owner,7)<decision.Builders&&World.Resources[4+owner]<decision.StoneTarget?7:OpponentWorkers(owner,6)<decision.Engineers?6:farmers<desiredFarmers?5:OpponentWorkers(owner,7)<decision.Builders?7:-1;
  bool workersNeeded=EconomicWorkerRecovery(owner)|| (MissionFourPressure(owner)?farmers<1||OpponentWorkers(owner,7)<1:workerType>=0&&!armyNeedsResidents||workerType==7||farmers<1);
  foreach(var home in World.Buildings.Where(b=>b.Owner==owner&&b.Type==2).OrderBy(b=>b.Slot))if(workersNeeded&&TrainOpponent(owner,home,workerType))break;
 }
 void BalanceOpponentWorkers(int owner,Building depot,Building? crop){
  var plan=DevelopmentPlan(owner);var farmers=World.Units.Where(a=>a.Owner==owner&&a.Type==5&&a.Health>0&&VisualCatalog.Visible(a)).OrderByDescending(a=>a.Slot).ToArray();
  var suppliers=CropSupplierSlots(owner,crop);
  foreach(var a in World.Units.Where(a=>a.Owner==owner&&OriginalWorkerPolicy.Carrier(a.Type)&&VisualCatalog.Visible(a)&&!TradeDeliveryPending(a)&&!ResidentReserved(a.Slot)&&!Feeding(a))){
   if(Repairing(a)||Constructions.Any(c=>!Complete(c)&&HasBuilder(c,a.Slot)))continue;
   if(a.Type==5){var nearby=World.Buildings.Where(b=>b.Owner==owner&&b.Type==3&&Inhabited(b)).OrderBy(b=>Math.Abs((b.Cell&255)-((a.FoodField>=0?a.FoodField:a.Y/1024*256+a.X/1024)&255))+Math.Abs((b.Cell>>8)-((a.FoodField>=0?a.FoodField:a.Y/1024*256+a.X/1024)>>8))).FirstOrDefault()??depot;bool seedNeeded=OpponentSeedNeeded(owner);int target=crop!=null&&seedNeeded&&Residents(crop.Slot)<ResidentCapacity(crop)&&suppliers.Contains(a.Slot)?crop.Slot:nearby.Slot;
    if(a.FoodDestinationKind==2&&a.FoodDestination!=target)SetFoodDestination(a,2,target);
    // A depleted field must not pin the whole economy to an empty local patch.
    if(a.Food==0&&a.FoodJob==4&&Tick%(30*30)==0&&!HarvestCandidates(a,a.FoodField>=0?a.FoodField:a.Y/1024*256+a.X/1024,a.Y/1024*256+a.X/1024).Any())ClearOrders(a);
   }else{
    int kind=WorkerResourceKind(a),reserve=kind==2?plan.RockReserve:plan.PartsReserve;
    if(World.Resources[ResourceIndex(kind)+owner]>=reserve||WarehouseSpace(owner)<8&&World.Resources[owner]<plan.FoodReserve){
     a.RepeatFood=a.PersistentCollection=a.RepeatMine=false;
     if(a.Food==0&&a.Rock==0&&(a.FoodJob>0||a.MineJob>0))ClearOrders(a);
    }
   }
  }
 }
 bool OpponentResourceNeeded(int owner,int kind){var p=DevelopmentPlan(owner);return kind==1||World.Resources[ResourceIndex(kind)+owner]<(kind==2?p.RockReserve:p.PartsReserve)&&!(WarehouseSpace(owner)<8&&World.Resources[owner]<p.FoodReserve);}

 long opponentRegionTick=-1;List<Actor>? opponentRegionUnits;readonly Dictionary<(int Type,int Owner),List<byte[]>> opponentRegions=new();
 byte[] OpponentReachableCells(Actor a){
  if(opponentRegionTick!=Tick||opponentRegionUnits!=World.Units){opponentRegionTick=Tick;opponentRegionUnits=World.Units;opponentRegions.Clear();}
  int start=a.Y/1024*256+a.X/1024;
  var key=(a.Type,a.Owner);if(!opponentRegions.TryGetValue(key,out var regions))opponentRegions[key]=regions=new();
  var cached=regions.FirstOrDefault(r=>r[start]!=0);if(cached!=null)return cached;
  var danger=OpponentRouteDanger(a,false);var reachable=new byte[65536];var queue=navigationQueue;int head=0,tail=0;queue[tail++]=start;reachable[start]=1;
  while(head<tail){int cell=queue[head++];foreach(var (dx,dy) in NavigationDirections){int x=(cell&255)+dx,y=(cell>>8)+dy;if(x<0||y<0||x>=World.Width||y>=World.Height)continue;int next=y*256+x;if(reachable[next]==0&&CanTraverse(a,x,y)&&SafeRouteStep(danger,start,cell,next)){reachable[next]=1;queue[tail++]=next;}}}
  regions.Add(reachable);return reachable;
 }

 void ExploreOpponentResources(Actor a,int owner){
  var reachable=OpponentReachableCells(a);var vision=OpponentVision(owner);
  var baseBuilding=World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==3)??World.Buildings.FirstOrDefault(b=>b.Owner==owner);int anchor=baseBuilding?.Cell??(a.Y/1024*256+a.X/1024);
  // Explore sectors, rather than revealing every nearest tile before advancing.
  int radius=ResourceSearchRadius(a);var directions=new[]{(1,0),(1,1),(0,1),(-1,1),(-1,0),(-1,-1),(0,-1),(1,-1)};
  for(int ring=16;ring<=radius;ring+=16)for(int i=0;i<8;i++){
   var d=directions[(i+a.Slot)%8];int x=Math.Clamp((anchor&255)+d.Item1*ring,0,World.Width-1),y=Math.Clamp((anchor>>8)+d.Item2*ring,0,World.Height-1);
   foreach(int cell in NearbyCells(y*256+x,3).Where(c=>reachable[c]!=0&&vision[c]==0&&OpponentLocalResource(a,c)).OrderBy(c=>CellDistance(c,y*256+x)).ThenBy(c=>c))if(Order(a,cell&255,cell>>8,true))return;
  }
  foreach(int next in Enumerable.Range(0,65536).Where(c=>reachable[c]!=0&&vision[c]==0&&OpponentLocalResource(a,c)).OrderBy(c=>Math.Abs((c&255)-(anchor&255))+Math.Abs((c>>8)-(anchor>>8))).ThenBy(c=>Math.Abs((c&255)-a.X/1024)+Math.Abs((c>>8)-a.Y/1024)).ThenBy(c=>c).Take(24))if(Order(a,next&255,next>>8,true))return;
 }

 long opponentSeedTick=-1;readonly HashSet<int> opponentSeedPlants=new();
 bool OpponentPreservesPlant(Actor a,int cell){
  if(a.Owner is <1 or >3||World.Objects[cell] is <29 or >52||OriginalOpponentActive||MissionNumber<4)return false;
  if(opponentSeedTick!=Tick){opponentSeedTick=Tick;opponentSeedPlants.Clear();var patches=new HashSet<int>();
   for(int c=0;c<65536;c++){if(World.Objects[c] is <29 or >52)continue;int x=c&255,y=c>>8;
    bool fertile=new[]{-1,1,-256,256}.Any(d=>ValidMapCell(c+d)&&CellDistance(c,c+d)==1&&GrowingSoil(c+d)&&(World.Collision[c+d]&1)==0&&World.Objects[c+d] is not (>=1 and <=11)&&BuildingAt(c+d)==null);
    if(fertile&&patches.Add(y/3*86+x/3))opponentSeedPlants.Add(c);
   }
  }
  return opponentSeedPlants.Contains(cell);
 }
 bool OpponentEmergencyMeal(Actor a){
  if(!OpponentEnabled||a.Owner is <1 or >3||a.Type!=5||World.Resources[a.Owner]>0)return false;
  // Break a starvation deadlock using real harvested food, never free resources.
  if(a.CargoKind==1&&a.Food>0){a.Food--;Hunger.Consumed[a.Owner]++;a.Satiety=a.SatietyMaximum;a.FatigueClock=0;CancelFeeding(a);if(a.Food==0){a.OverflowCell=-1;a.Route.Clear();a.FoodJob=0;ContinueHarvest(a);}return true;}
  if(a.FoodJob is 1 or 2&&a.CargoKind==1){CancelFeeding(a);return true;}
  if(Tick%30!=0||!Opponent.Explored.TryGetValue(a.Owner,out var known))return false;
  var depot=World.Buildings.Where(b=>b.Owner==a.Owner&&b.Type==3&&Inhabited(b)).OrderBy(b=>Math.Abs((b.Cell&255)-a.X/1024)+Math.Abs((b.Cell>>8)-a.Y/1024)).FirstOrDefault();if(depot==null)return false;
  var reachable=OpponentReachableCells(a);
  int mealAnchor=a.Y/1024*256+a.X/1024;var sources=NearbyCells(mealAnchor,GroundMealRadius).Where(c=>MealNear(mealAnchor,c)&&known[c]!=0&&OpponentKnowsResource(a.Owner,c)&&IsFood(c)&&reachable[c]!=0&&OpponentLocalResource(a,c)).OrderBy(c=>Math.Abs((c&255)-a.X/1024)+Math.Abs((c>>8)-a.Y/1024)).ToArray();
  var local=sources.Where(c=>Math.Abs((c&255)-(depot.Cell&255))<=48&&Math.Abs((c>>8)-(depot.Cell>>8))<=48).ToArray();if(local.Length>0)sources=local;int offset=(int)((Tick/30*8)%Math.Max(1,sources.Length));
  foreach(int cell in sources.Take(24)){
   if(!Gather(a,cell,false))continue;a.RepeatFood=a.PersistentCollection=true;SetFoodDestination(a,2,depot.Slot);return true;
  }
  return false;
 }

}

