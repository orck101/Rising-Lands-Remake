namespace RisingLands;
public sealed class BuildingRepairOrder : ConstructionWorker {public int Building{get;set;}}
public sealed partial class Simulation {
 internal void DamageBuildingForResearch(Building b,int damage)=>HitStructure(b,damage);
 public bool Repairing(Actor a)=>Opponent.Repairs.ContainsKey(a.Slot);
 public bool CanRepair(Actor a,Building b)=>a.Type==7&&VisualCatalog.Visible(a)&&a.Owner==b.Owner&&World.Buildings.Contains(b)&&BuildingHealth(b)>0&&BuildingHealth(b)<BuildingMaxHealth(b);
 public bool RepairBuilding(Actor a,int slot){
  var b=World.Buildings.FirstOrDefault(b=>b.Slot==slot);if(b==null||!CanRepair(a,b))return false;
  if(Opponent.Repairs.TryGetValue(a.Slot,out var existing)&&existing.Building==slot)return true;
  var taken=Opponent.Repairs.Where(p=>p.Key!=a.Slot).Select(p=>p.Value.WorkCell).ToHashSet();
  foreach(int cell in WorkCells(b.Cell,Rule(b.Type)).Where(c=>!taken.Contains(c)).OrderBy(c=>CellDistance(c,a.Y/1024*256+a.X/1024))){
   if(World.Units.Any(u=>u!=a&&VisualCatalog.Visible(u)&&u.X/1024==(cell&255)&&u.Y/1024==(cell>>8)))continue;
   if(!Order(a,cell&255,cell>>8,true))continue;var route=a.Route;ClearOrders(a);a.Route=route;
   Opponent.Repairs[a.Slot]=new(){Building=slot,BuilderSlot=a.Slot,WorkCell=cell,StrikeCooldown=50};return true;
  }return false;
 }
 ConstructionWorker? WorkingRepair(Actor a)=>!Feeding(a)&&a.Route.Count==0&&Opponent.Repairs.TryGetValue(a.Slot,out var job)&&a.X==(job.WorkCell&255)*1024&&a.Y==(job.WorkCell>>8)*1024?job:null;
 void RepairStep(){foreach(var pair in Opponent.Repairs.ToArray()){
  var a=World.Units.FirstOrDefault(u=>u.Slot==pair.Key);var b=World.Buildings.FirstOrDefault(b=>b.Slot==pair.Value.Building);var job=pair.Value;
  if(a==null||b==null||!CanRepair(a,b)){Opponent.Repairs.Remove(pair.Key);continue;}
  if(Feeding(a)||a.Satiety<=0||a.Route.Count>0)continue;
  if(a.X!=(job.WorkCell&255)*1024||a.Y!=(job.WorkCell>>8)*1024){if(Tick%30==0)Order(a,job.WorkCell&255,job.WorkCell>>8,true);continue;}
  a.Direction=ConstructionFacing(job.WorkCell,b.Cell,Rule(b.Type).Width,Rule(b.Type).Height);job.AnimationTick=(job.AnimationTick+1)%16;
  // CalculReparer 0x44b991 calls BatimentRepare: damage -= 2, cooldown 0x14.
  if(--job.StrikeCooldown>0)continue;job.StrikeCooldown=20;Opponent.BuildingDamage[b.Slot]=Math.Max(0,Opponent.BuildingDamage.GetValueOrDefault(b.Slot)-2);
  EmitUnitSound(a,0x25);if(BuildingHealth(b)==BuildingMaxHealth(b))Opponent.Repairs.Remove(pair.Key);
 }}
 void ValidateRepairs(Save s,OpponentState state){
  if(state.Repairs==null||state.DestructionAt==null)throw new InvalidDataException("Missing repair state");
  var buildings=InitialBuildings.Concat(s.Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).ToDictionary(b=>b.Slot);var cells=new HashSet<int>();
  foreach(var p in state.Repairs){var j=p.Value;if(j==null||!buildings.TryGetValue(j.Building,out var b)||!s.Units.Any(a=>a.Slot==p.Key&&a.Slot==j.BuilderSlot&&a.Health>0&&a.Type==7&&a.Owner==b.Owner)||j.AnimationTick is <0 or >=16||j.StrikeCooldown is <0 or >50||!WorkCells(b.Cell,Rule(b.Type)).Contains(j.WorkCell)||!cells.Add(j.WorkCell)||state.BuildingDamage.GetValueOrDefault(b.Slot)<=0||state.BuildingDamage.GetValueOrDefault(b.Slot)>=BuildingMaxHealth(b))throw new InvalidDataException("Invalid building repair");}
  foreach(var p in state.DestructionAt)if(!buildings.TryGetValue(p.Key,out var b)||p.Value<0||p.Value>s.Tick||state.BuildingDamage.GetValueOrDefault(b.Slot)<BuildingMaxHealth(b))throw new InvalidDataException("Invalid destruction animation");
 }
 public IEnumerable<Building> CollapsingBuildings()=>InitialBuildings.Concat(Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).Where(b=>Opponent.DestructionAt.TryGetValue(b.Slot,out long at)&&Tick-at<200);
}
