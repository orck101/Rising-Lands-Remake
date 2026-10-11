namespace RisingLands;
public sealed partial class Simulation {
 // This is the requested remake policy, not a recovered original AI rule.
 bool WarehousePressure(int owner){var stock=WarehouseSummary(owner);return stock.Capacity>0&&(long)stock.Occupancy*10>=stock.Capacity*7L;}
 int WarehouseSurvivalReserve(int owner)=>Math.Max(6,(int)Math.Ceiling(PolicyMaintenance(owner)*2));
 bool WarehouseEmergency(int owner)=>BaseInvaded(owner)||World.Resources[owner]<WarehouseSurvivalReserve(owner)||EconomicWorkerRecovery(owner);
 bool WarehouseActivationPending(int owner)=>World.Buildings.Any(b=>b.Owner==owner&&b.Type==3&&!Inhabited(b));
 bool WarehouseExpansionNeeded(int owner){
  if(WarehouseActivationPending(owner)||Constructions.Any(c=>c.Owner==owner&&c.Type==3&&!Complete(c))){Opponent.WarehouseExpansionRequested[owner]=false;return false;}
  if(WarehousePressure(owner))Opponent.WarehouseExpansionRequested[owner]=true;
  return Opponent.WarehouseExpansionRequested[owner]&&OpponentBuildingUnlocked(owner,3)&&!WarehouseEmergency(owner);
 }
 bool WarehouseOperatorNeeded(int owner)=>WarehouseActivationPending(owner)||Constructions.Any(c=>c.Owner==owner&&c.Type==3&&!Complete(c))||WarehouseExpansionNeeded(owner);
 // Recompute the reservation from ordinary saved actors/trips: no new save schema.
 int WarehouseOperatorSlot(int owner){
  if(!WarehouseOperatorNeeded(owner))return -1;
  if(World.Buildings.Any(b=>b.Owner==owner&&b.Type==3&&!Inhabited(b)&&ResidentTrips.Any(t=>t.Building==b.Slot)))return -1;
  var free=World.Units.FirstOrDefault(a=>a.Owner==owner&&a.Type==9&&a.Health>0&&VisualCatalog.Visible(a)&&a.Route.Count==0&&!ResidentReserved(a.Slot)&&!TradeDeliveryPending(a));if(free!=null)return free.Slot;
  foreach(var home in World.Buildings.Where(b=>b.Owner==owner&&b.Type is 2 or 4&&OpponentFreeResidents(b)>1).OrderBy(b=>b.Type==4?0:1).ThenBy(b=>b.Slot)){
   var a=World.Units.FirstOrDefault(a=>a.Owner==owner&&a.Type==9&&a.Health>0&&a.LegacyState==14&&World.BuildingResidents.GetValueOrDefault(home.Slot,new()).Contains(a.Slot)&&!ResidentReserved(a.Slot)&&!Recruitments.Any(j=>j.SettlerSlot==a.Slot&&!j.Finished));if(a!=null)return a.Slot;
  }return -1;
 }
 bool WarehouseBudgetAllows(int owner,int food,int rock,int parts){if(!WarehouseExpansionNeeded(owner)||!OwnService(owner,4)||WarehouseOperatorSlot(owner)<0)return true;var cost=Rule(3);return World.Resources[owner]>=food+cost.Food+WarehouseSurvivalReserve(owner)&&World.Resources[4+owner]>=rock+cost.Rock&&World.Resources[8+owner]>=parts+cost.Parts;}
 void StaffOpponentWarehouses(int owner){
  foreach(var depot in World.Buildings.Where(b=>b.Owner==owner&&b.Type==3&&!Inhabited(b)).OrderBy(b=>b.Slot)){
   if(ResidentTrips.Any(t=>t.Building==depot.Slot))continue;int slot=WarehouseOperatorSlot(owner);var a=World.Units.FirstOrDefault(a=>a.Slot==slot);if(a==null)return;
   if(!VisualCatalog.Visible(a)){var home=World.Buildings.FirstOrDefault(b=>b.Owner==owner&&World.BuildingResidents.GetValueOrDefault(b.Slot,new()).Contains(slot));if(home==null||OpponentFreeResidents(home)<=1)return;a=ReleaseResident(home.Slot,owner,slot);}
   if(a!=null&&EnterBuilding(a,depot.Slot))return;
  }
 }
 bool SafeWarehouseSite(int owner,Actor builder,int cell){
  var rule=Rule(3);var known=Opponent.Explored.GetValueOrDefault(owner);if(known==null)return false;
  for(int y=0;y<rule.Height;y++)for(int x=0;x<rule.Width;x++){int c=cell+y*256+x;if(!ValidMapCell(c)||known[c]==0||!OpponentSafeCell(owner,c))return false;}
  if(PlacementError(builder,3,cell)!=null)return false;
  var reachable=OpponentReachableCells(builder);return WorkCells(cell,rule).Any(c=>reachable[c]!=0&&OpponentSafeCell(owner,c));
 }
 int WarehouseSiteDistance(int owner,int cell)=>World.Units.Where(a=>a.Owner==owner&&OriginalWorkerPolicy.Carrier(a.Type)&&a.Health>0&&VisualCatalog.Visible(a)).Select(a=>CellDistance(cell,a.FoodField>=0?a.FoodField:a.Y/1024*256+a.X/1024)).DefaultIfEmpty(0).Sum();
}

