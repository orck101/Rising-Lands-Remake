namespace RisingLands;
public sealed class ExchangeResident {
 public int Slot{get;set;}public int Building{get;set;}public int Cell{get;set;}public bool Stored{get;set;}
}
public sealed partial class Simulation {
 public IEnumerable<Actor> ExchangeOccupants(int building)=>Special.ExchangeResidents.Where(r=>r.Building==building&&r.Stored).Select(r=>World.Units.First(a=>a.Slot==r.Slot));
 public int ExchangeReserved(int building)=>Special.ExchangeResidents.Count(r=>r.Building==building);
 public bool CanStoreTradeUnit(Actor actor,Building bag){
  if(!World.Buildings.Contains(bag)||bag.Type!=8||bag.Owner!=actor.Owner||actor.Owner is <0 or >3||!World.Units.Contains(actor)||actor.Health<=0||!UnitExchangeable(actor.Type)||!VisualCatalog.Visible(actor)||Special.Mirrors.Any(m=>m.Seed.Slot==actor.Slot)||ResidentReserved(actor.Slot)||Special.ExchangeResidents.Any(r=>r.Slot==actor.Slot)||Special.Negotiations.Any(n=>n.Active&&n.Messenger==actor.Slot)||Occupants(bag.Slot)+ExchangeReserved(bag.Slot)>=ResidentCapacity(bag))return false;
  var goals=BuildingInteractionCells(bag).Where(c=>CanTraverse(actor,c&255,c>>8)).ToHashSet();
  if(Airborne(actor))return goals.Count>0;
  int start=actor.Y/1024*256+actor.X/1024;var seen=new HashSet<int>{start};var queue=new Queue<int>();queue.Enqueue(start);
  while(queue.TryDequeue(out int c)){if(goals.Contains(c))return true;foreach(var (dx,dy) in NavigationDirections){int x=(c&255)+dx,y=(c>>8)+dy;if(CanTraverse(actor,x,y)&&seen.Add(y*256+x))queue.Enqueue(y*256+x);}}return false;
 }
 public bool StoreTradeUnit(Actor actor,Building bag){
  if(actor.Type==9)return EnterBuilding(actor,bag.Slot);if(!CanStoreTradeUnit(actor,bag))return false;
  foreach(int cell in BuildingInteractionCells(bag).OrderBy(c=>Math.Abs((c&255)-actor.X/1024)+Math.Abs((c>>8)-actor.Y/1024))){
   if(!Order(actor,cell&255,cell>>8,true))continue;
   StopExceptTravel(actor);foreach(var ids in World.BuildingResidents.Values)ids.Remove(actor.Slot);Special.ExchangeResidents.Add(new(){Slot=actor.Slot,Building=bag.Slot,Cell=cell});return true;
  }return false;
 }
 public Actor? ReleaseExchangeUnit(int building,int type=-1,int owner=0){
  var bag=World.Buildings.FirstOrDefault(b=>b.Slot==building&&b.Owner==owner&&b.Type==8);if(bag==null)return null;
  var resident=Special.ExchangeResidents.FirstOrDefault(r=>r.Building==building&&r.Stored&&World.Units.Any(a=>a.Slot==r.Slot&&a.Health>0&&(type<0||a.Type==type)));if(resident==null)return null;
  int exit=BuildingExit(bag);if(exit<0)return null;var actor=World.Units.First(a=>a.Slot==resident.Slot);
  foreach(var n in Special.Negotiations.Where(n=>n.Active)){n.Stored.Remove(actor.Slot);n.DeliveryCells.Remove(actor.Slot);n.Entering.Remove(actor.Slot);}
  Special.ExchangeResidents.Remove(resident);Special.ExchangeEntering.Remove(actor.Slot);actor.LegacyState=7;actor.Route.Clear();PlaceAtExit(actor,bag,exit);return actor;
 }
 void CancelExchangeEntry(Actor actor){
  Special.ExchangeResidents.RemoveAll(r=>r.Slot==actor.Slot&&!r.Stored);Special.ExchangeEntering.Remove(actor.Slot);
  foreach(var n in Special.Negotiations.Where(n=>n.Active&&!n.Stored.Contains(actor.Slot))){n.DeliveryCells.Remove(actor.Slot);n.Entering.Remove(actor.Slot);}
 }
 void ExchangeStorageStep(){
  foreach(var resident in Special.ExchangeResidents.ToArray()){
   var actor=World.Units.FirstOrDefault(a=>a.Slot==resident.Slot);var bag=World.Buildings.FirstOrDefault(b=>b.Slot==resident.Building&&b.Type==8);
   if(actor==null||actor.Health<=0||bag==null||actor.Owner!=bag.Owner){Special.ExchangeResidents.Remove(resident);Special.ExchangeEntering.Remove(resident.Slot);if(actor!=null&&resident.Stored){actor.LegacyState=7;if(bag==null)actor.Health=0;}continue;}
   if(resident.Stored||!WalkIntoTradeBuilding(Special.ExchangeEntering,actor,bag,resident.Cell))continue;
   actor.LegacyState=12;actor.Route.Clear();resident.Stored=true;EmitBuildingSound(bag,0x27);
  }
 }
 bool ResolveExchangeTroops(Negotiation n){
  var selected=new HashSet<int>();
  List<int>? Match(int owner,List<int> types){
   var bag=Exchange(owner);if(bag==null)return null;
   var available=ExchangeOccupants(bag.Slot).Concat(World.Units.Where(a=>n.Stored.Contains(a.Slot)&&a.Owner==owner&&a.Health>0)).DistinctBy(a=>a.Slot).Where(a=>!Special.Negotiations.Any(other=>other.Id!=n.Id&&other.Active&&(other.Stored.Contains(a.Slot)||other.GiveUnits.Contains(a.Slot)||other.TakeUnits.Contains(a.Slot)))).OrderBy(a=>a.Slot).ToArray();
   var ids=new List<int>();foreach(int type in types){var actor=available.FirstOrDefault(a=>a.Type==type&&selected.Add(a.Slot));if(actor==null)return null;ids.Add(actor.Slot);}return ids;
  }
  var give=Match(0,n.GiveTypes);if(give==null)return false;var take=Match(n.Owner,n.TakeTypes);if(take==null)return false;
  n.GiveUnits=give;n.TakeUnits=take;n.Stored.Clear();foreach(int id in give.Concat(take))n.Stored.Add(id);return true;
 }
 public int ExchangeDelivered(Negotiation n,int owner){
  var types=owner==0?n.GiveTypes:n.TakeTypes;var bag=Exchange(owner);if(bag==null)return 0;
  var available=ExchangeOccupants(bag.Slot).Concat(World.Units.Where(a=>n.Stored.Contains(a.Slot)&&a.Owner==owner&&a.Health>0)).DistinctBy(a=>a.Slot).ToArray();
  return types.GroupBy(t=>t).Sum(g=>Math.Min(g.Count(),available.Count(a=>a.Type==g.Key)));
 }
 void PrepareEnemyExchange(Negotiation n){
  var bag=Exchange(n.Owner);if(bag==null)return;
  foreach(var required in n.TakeTypes.GroupBy(t=>t)){
   int queued=Special.ExchangeResidents.Count(r=>r.Building==bag.Slot&&World.Units.Any(a=>a.Slot==r.Slot&&a.Type==required.Key&&a.Health>0));
   queued+=n.Stored.Count(id=>!Special.ExchangeResidents.Any(r=>r.Slot==id)&&World.Units.Any(a=>a.Slot==id&&a.Type==required.Key&&a.Owner==n.Owner&&a.Health>0));
   foreach(var actor in TradeUnits(n.Owner,n.Id).Where(a=>a.Type==required.Key&&VisualCatalog.Visible(a)&&!TradeDeliveryPending(a)).OrderByDescending(a=>n.TakeUnits.Contains(a.Slot)).ThenBy(a=>a.Slot).ToArray()){
    if(queued>=required.Count())break;if(StoreTradeUnit(actor,bag))queued++;
   }
  }
 }
 void ValidateExchangeStorage(Save save,SpecialState state){
  if(state.ExchangeResidents==null||state.ExchangeEntering==null||state.ExchangeResidents.Count>save.Units.Count)throw new InvalidDataException("Invalid Exchange storage");
  var buildings=InitialBuildings.Concat(save.Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).ToDictionary(b=>b.Slot);var ids=new HashSet<int>();
  foreach(var r in state.ExchangeResidents){
   var a=save.Units.FirstOrDefault(a=>a.Slot==r.Slot);
   if(!ids.Add(r.Slot)||a==null||a.Health<=0||!SavedExchangeable(a.Type,save)||!buildings.TryGetValue(r.Building,out var bag)||bag.Type!=8||bag.Owner!=a.Owner||(save.Opponent?.BuildingDamage.GetValueOrDefault(bag.Slot)??0)>=BuildingDamageLimits[8]||!ValidBuildingArrivalCells(bag).Contains(r.Cell)||a.LegacyState!=(r.Stored?12:7)||r.Stored&&a.Route.Count>0||save.Residents!.Values.Any(list=>list.Contains(r.Slot))||save.Trips!.Any(t=>t.Slot==r.Slot)||state.Negotiations.Any(n=>n.Active&&n.Messenger==r.Slot)||state.Mirrors.Any(m=>m.Seed.Slot==r.Slot))throw new InvalidDataException("Invalid Exchange resident");
  }
  foreach(var group in state.ExchangeResidents.GroupBy(r=>r.Building))if(group.Count()+save.Residents!.GetValueOrDefault(group.Key,new()).Count(id=>save.Units.Any(a=>a.Slot==id&&a.Health>0&&a.LegacyState==14))>ResidentCapacity(buildings[group.Key]))throw new InvalidDataException("Exchange over capacity");
  foreach(var p in state.ExchangeEntering)if(p.Value<0||p.Value>save.Tick||!state.ExchangeResidents.Any(r=>r.Slot==p.Key&&!r.Stored)||!save.Units.Any(a=>a.Slot==p.Key&&a.Route.Count==0))throw new InvalidDataException("Invalid Exchange entrance");
 }
 void RestoreExchangeColonists(){
  foreach(var n in Special.Negotiations.Where(n=>n.Active&&(n.GiveTypes.Contains(9)||n.TakeTypes.Contains(9))).ToArray()){CancelNegotiation(n,"Troca cancelada: colonos somente ocupam construções.");n.GiveUnits.RemoveAll(id=>World.Units.First(a=>a.Slot==id).Type==9);n.TakeUnits.RemoveAll(id=>World.Units.First(a=>a.Slot==id).Type==9);n.GiveTypes.RemoveAll(t=>t==9);n.TakeTypes.RemoveAll(t=>t==9);}
  foreach(var r in Special.ExchangeResidents.Where(r=>World.Units.Any(a=>a.Slot==r.Slot&&a.Type==9)).ToArray()){
   var a=World.Units.First(a=>a.Slot==r.Slot);Special.ExchangeResidents.Remove(r);Special.ExchangeEntering.Remove(r.Slot);
   if(r.Stored){a.LegacyState=14;if(!World.BuildingResidents.TryGetValue(r.Building,out var ids))World.BuildingResidents[r.Building]=ids=new();ids.Add(a.Slot);}
   else ResidentTrips.Add(new(){Slot=a.Slot,Building=r.Building,Cell=r.Cell});
  }
 }
 void MigrateExchangeAgreements(Save save,SpecialState state){
  if(save.Version>=34)return;
  foreach(var n in state.Negotiations){n.GiveTypes=n.GiveUnits.Select(id=>save.Units.First(a=>a.Slot==id).Type).ToList();n.TakeTypes=n.TakeUnits.Select(id=>save.Units.First(a=>a.Slot==id).Type).ToList();if(n.Stage=="Delivery")n.Deadline=0;}
 }
}



