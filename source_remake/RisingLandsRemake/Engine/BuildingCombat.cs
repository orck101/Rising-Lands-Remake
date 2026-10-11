namespace RisingLands;
public sealed class SiegeOrder {public int Building{get;set;}public int Windup{get;set;}}
public sealed partial class Simulation {
 // PE original table 0x495d90 + type*52; confirmed static building limits.
 static readonly int[] BuildingDamageLimits={200,400,600,100,100,300,500,400,500,30,50,100,70,350,350,400,300,300,300,300,400,300};
 public bool DestroyedBuilding(int slot)=>Opponent.BuildingDamage.TryGetValue(slot,out int damage)&&InitialBuildings.Concat(Constructions.Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).FirstOrDefault(b=>b.Slot==slot) is {} b&&damage>=BuildingDamageLimits[b.Type];
 readonly bool[] destroyedFootprint=new bool[65536];
 public bool DestroyedAt(int cell)=>cell>=0&&cell<65536&&destroyedFootprint[cell];
 void RefreshDestroyedFootprint(){Array.Clear(destroyedFootprint);foreach(var b in InitialBuildings.Concat(Constructions.Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).Where(b=>DestroyedBuilding(b.Slot))){var r=Rule(b.Type);for(int y=0;y<r.Height;y++)for(int x=0;x<r.Width;x++)destroyedFootprint[b.Cell+y*256+x]=true;}}
 public int BuildingMaxHealth(Building b)=>BuildingDamageLimits[b.Type];
 public int BuildingHealth(Building b)=>Math.Max(0,BuildingMaxHealth(b)-Opponent.BuildingDamage.GetValueOrDefault(b.Slot));
 public bool Sieging(Actor a)=>Opponent.Sieges.ContainsKey(a.Slot);
 bool BuildingReach(Actor a,Building b){var r=Rule(b.Type);int x=Math.Clamp(a.X/1024,b.Cell&255,(b.Cell&255)+r.Width-1),y=Math.Clamp(a.Y/1024,b.Cell>>8,(b.Cell>>8)+r.Height-1);int range=Math.Max(1,CombatRule(a.Type).Range);return ProjectileMath.WithinRange(a.X/1024-x,a.Y/1024-y,range*range);}
 public bool AttackBuilding(Actor a,int slot){var b=World.Buildings.FirstOrDefault(b=>b.Slot==slot);if(!World.Units.Contains(a)||!FightingUnit(a)||b==null||a.Owner==b.Owner||CombatRule(a.Type).Damage[26]<=0||a.Owner!=0&&!Hostile(a.Owner,b.Owner))return false;
  RegisterAllianceAttack(a.Owner,b.Owner);
  ClearOrders(a);Opponent.Sieges[a.Slot]=new(){Building=slot};return true;
 }
 void SiegeStep(){foreach(var pair in Opponent.Sieges.ToArray()){
  var a=World.Units.FirstOrDefault(a=>a.Slot==pair.Key);var b=World.Buildings.FirstOrDefault(b=>b.Slot==pair.Value.Building);
  if(a==null||b==null||!FightingUnit(a)||!Hostile(a.Owner,b.Owner)||a.AttackTarget>=0){Opponent.Sieges.Remove(pair.Key);continue;}if(Feeding(a))continue;
  var job=pair.Value;if(!BuildingReach(a,b)){job.Windup=0;if(a.Route.Count==0)ApproachStructure(a,b);continue;}
  a.Route.Clear();a.Direction=ConstructionFacing(a.Y/1024*256+a.X/1024,b.Cell,Rule(b.Type).Width,Rule(b.Type).Height);
  if(job.Windup==0){if(a.AttackCooldown>0)continue;a.AttackCooldown=CombatCycle(a);job.Windup=ReleaseTicks(a);}
  if(--job.Windup>0)continue;
  int damage=Math.Max(0,CombatMath.Damage(CombatRule(a.Type).Damage[26],(MilitaryDamageBonus(a)+SpiritualDamageBonus(a)),0,CaptainBonus(a),0,0));
  EmitUnitSound(a,7);
  if(AttackRuleFor(a)!.Projectile>=0)FireBuildingArrow(a,b,damage);else HitStructure(b,damage,a.Owner);
 }}
 bool ApproachStructure(Actor a,Building b){
  if(Tick<a.CombatRetryAt)return false;a.CombatRetryAt=Tick+30;
  var r=Rule(b.Type);int range=Math.Clamp(CombatRule(a.Type).Range,1,16);
  var cells=new List<int>();
  for(int y=Math.Max(0,(b.Cell>>8)-range);y<Math.Min(World.Height,(b.Cell>>8)+r.Height+range);y++)for(int x=Math.Max(0,(b.Cell&255)-range);x<Math.Min(World.Width,(b.Cell&255)+r.Width+range);x++){
   int cx=Math.Clamp(x,b.Cell&255,(b.Cell&255)+r.Width-1),cy=Math.Clamp(y,b.Cell>>8,(b.Cell>>8)+r.Height-1);
   if(!Inside(y*256+x,b.Cell,r.Width,r.Height)&&ProjectileMath.WithinRange(x-cx,y-cy,range*range)&&CanTraverse(a,x,y)&&!World.Units.Any(u=>u!=a&&VisualCatalog.Visible(u)&&!Airborne(u)&&u.X/1024==x&&u.Y/1024==y))cells.Add(y*256+x);
  }
  foreach(int cell in cells.OrderBy(c=>Math.Abs((c&255)-a.X/1024)+Math.Abs((c>>8)-a.Y/1024)).ThenBy(c=>c).Take(16))if(Order(a,cell&255,cell>>8,true))return true;return false;
 }
 void HitStructure(Building b,int damage,int attacker=-1){
  if(!World.Buildings.Contains(b)||damage<=0)return;
  var aggressor=World.Units.FirstOrDefault(a=>a.Owner==attacker&&Sieging(a)&&Opponent.Sieges[a.Slot].Building==b.Slot);if(aggressor!=null)ObserveBaseAttack(aggressor.Slot,b);
  int before=Opponent.BuildingDamage.GetValueOrDefault(b.Slot);Opponent.BuildingDamage[b.Slot]=Math.Min(BuildingMaxHealth(b),before+damage);RegisterStructureDamage(attacker,b,before,Opponent.BuildingDamage[b.Slot]);
  if(b.Owner==0)LastAttack.Record(unchecked((uint)Tick),b.Cell,b.Owner);
  if(DestroyedBuilding(b.Slot))DestroyStructure(b);
 }
 void FireBuildingArrow(Actor a,Building b,int damage){var r=Rule(b.Type);int x=Math.Clamp(a.X/1024,b.Cell&255,(b.Cell&255)+r.Width-1),y=Math.Clamp(a.Y/1024,b.Cell>>8,(b.Cell>>8)+r.Height-1);
  Arrows.Add(new Arrow{TargetBuilding=b.Slot,TargetSlot=-1,ProjectileType=AttackRuleFor(a)!.Projectile,Bonus=(MilitaryDamageBonus(a)+SpiritualDamageBonus(a)),Captain=CaptainBonus(a),StepPixels=NativeArrowSteps,SourceSlot=a.Slot,StartX=a.X/32+16,StartY=a.Y/32+16,EndX=x*32+16,EndY=y*32+16,Damage=damage,BornTick=Tick});
 }
 void DestroyStructure(Building b){
  Opponent.DestructionAt[b.Slot]=Tick;foreach(var resident in Special.ExchangeResidents.Where(r=>r.Building==b.Slot).ToArray()){var actor=World.Units.FirstOrDefault(a=>a.Slot==resident.Slot);if(actor!=null&&resident.Stored){actor.Health=0;actor.LegacyState=7;actor.Route.Clear();}Special.ExchangeResidents.Remove(resident);Special.ExchangeEntering.Remove(resident.Slot);}foreach(int id in Opponent.Repairs.Where(p=>p.Value.Building==b.Slot).Select(p=>p.Key).ToArray()){Opponent.Repairs.Remove(id);var worker=World.Units.FirstOrDefault(a=>a.Slot==id);if(worker!=null)worker.Route.Clear();}
  foreach(int id in World.BuildingResidents.GetValueOrDefault(b.Slot,new())){var resident=World.Units.FirstOrDefault(a=>a.Slot==id&&a.LegacyState==14);if(resident!=null){resident.Health=0;ClearOrders(resident);resident.AttackCooldown=0;}}
  if(b.Type==8)foreach(var n in Special.Negotiations.Where(n=>n.Stage=="Delivery"&&(b.Owner==0||n.Owner==b.Owner)).ToArray())CancelNegotiation(n,"Bolsa destruída; troca cancelada.");
  foreach(var trip in ResidentTrips.Where(t=>t.Building==b.Slot).ToArray())ResidentTrips.Remove(trip);
  foreach(int id in Opponent.Sieges.Where(p=>p.Value.Building==b.Slot).Select(p=>p.Key).ToArray())Opponent.Sieges.Remove(id);
  World.Buildings.Remove(b);RefreshDestroyedFootprint();doorCacheCount=-1;InvalidateSpatialIndex();EmitBuildingSound(b,0x23);
 }
 void ValidateSieges(Save s,OpponentState state){if(state.BuildingDamage==null||state.Sieges==null)throw new InvalidDataException("Missing siege state");var buildings=InitialBuildings.Concat(s.Constructions.Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).ToDictionary(b=>b.Slot);
  foreach(var p in state.BuildingDamage)if(!buildings.TryGetValue(p.Key,out var b)||p.Value<0||p.Value>BuildingDamageLimits[b.Type])throw new InvalidDataException("Invalid building damage");
  foreach(var p in state.Sieges)if(!buildings.TryGetValue(p.Value.Building,out var b)||!s.Units.Any(a=>a.Slot==p.Key&&a.Health>0&&a.Owner!=b.Owner&&SupportedFighter(a)&&CombatRule(a.Type).Damage[26]>0)||p.Value.Windup<0||p.Value.Windup>32||state.BuildingDamage.GetValueOrDefault(b.Slot)>=BuildingDamageLimits[b.Type])throw new InvalidDataException("Invalid siege order");
 }
}
