namespace RisingLands;
public sealed class BaseThreat {
 public int Slot{get;set;} public int Owner{get;set;} public int Type{get;set;} public int Cell{get;set;} public int Health{get;set;} public double Power{get;set;} public long Seen{get;set;} public bool AttackingBuilding{get;set;}public bool AttackingEconomy{get;set;}
}
public sealed class BaseGuardOrder {public int Anchor{get;set;} public int Post{get;set;} public int Target{get;set;}=-1;public int Sector{get;set;}public int BlockedTarget{get;set;}=-1;public long BlockedUntil{get;set;}public long MoveAfter{get;set;}}
public sealed class BaseDefenseState {
 public string Mode{get;set;}="Vigilancia";public long AlertUntil{get;set;}public long LastUpdate{get;set;}=-1;public int IncidentCell{get;set;}=-1;public long IncidentAt{get;set;}=-1;
 public int GarrisonTarget{get;set;}=3;public double ThreatPower{get;set;}public double GuardPower{get;set;}public double RequiredPower{get;set;}
 public List<BaseThreat> Threats{get;set;}=new();public Dictionary<int,BaseGuardOrder> Guards{get;set;}=new();
}
public sealed partial class Simulation {
 public BaseDefenseState BaseDefenseFor(int owner){if(!Opponent.BaseDefenses.TryGetValue(owner,out var d))Opponent.BaseDefenses[owner]=d=new();return d;}
 const int BaseThreatMemory=15*30;
 double DefenseMargin=>MissionNumber<=4?1.45:MissionNumber<=6?1.3:MissionNumber<=10?1.2:1.1;
 int ActorCell(Actor a)=>a.Y/1024*256+a.X/1024;
 List<Building> DefenseBases(int owner){var result=new List<Building>();foreach(var b in World.Buildings.Where(b=>b.Owner==owner&&b.Type is not (9 or 12)).OrderBy(b=>b.Type==2?0:b.Type==3?1:2).ThenBy(b=>b.Slot))if(result.All(h=>CellDistance(h.Cell,b.Cell)>24))result.Add(b);return result;}
 bool InsideDefenseTerritory(int owner,int cell,int radius=16)=>World.Buildings.Any(b=>b.Owner==owner&&CellDistance(cell,b.Cell)<=radius+Math.Max(Rule(b.Type).Width,Rule(b.Type).Height)/2);
 double DefenseTypePower(int type,int health){var r=CombatRule(type);double damage=r.Damage.Take(26).Where(x=>x>0).DefaultIfEmpty(1).Average();return Math.Max(.05,damage*30/Math.Max(1,r.Reload)*Math.Sqrt(Math.Max(1,health))*(1+r.Armor*.04)*(1+Math.Min(16,r.Range)*.04));}
 double DefensePower(Actor a)=>DefenseTypePower(a.Type,a.Health)*(1+MilitaryDamageBonus(a)/Math.Max(1,CombatRule(a.Type).Damage.Where(d=>d>0).DefaultIfEmpty(1).Average()))*(1+(a.Type==2?.12:CaptainBonus(a)>0?.08:0));
 bool DefenseEmergency(int owner)=>Opponent.BaseDefenses.TryGetValue(owner,out var d)&&d.Mode!="Vigilancia";
 bool DefenseShortfall(int owner)=>Opponent.BaseDefenses.TryGetValue(owner,out var d)&&(OpponentArmyCount(owner)+Recruitments.Count(j=>j.Owner==owner&&!j.Finished&&AdvancedAttacks.Rules.Any(r=>r.Type==j.Type))<d.GarrisonTarget||d.GuardPower<d.RequiredPower);
 int DefenseFoodReserve(int owner)=>Math.Max(OpeningFoodFloor(owner),(int)Math.Ceiling(PolicyMaintenance(owner)*2)+CultureFoodCost(OpponentLevel(owner,0)));
 int BaseGuardTarget(int owner){int sites=Math.Max(1,DefenseBases(owner).Count),buildings=World.Buildings.Count(b=>b.Owner==owner&&b.Type is not (9 or 12));return Math.Clamp(PolicyFor(owner).Guards+(sites-1)*2+Math.Max(0,buildings-5)/4,PolicyFor(owner).Guards,MissionNumber<=4?8:12);}
 void ObserveBaseAttack(int attackerSlot,Building building){if(!OpponentEnabled||building.Owner is <1 or >3)return;var a=World.Units.FirstOrDefault(u=>u.Slot==attackerSlot);if(a==null||!Hostile(building.Owner,a.Owner))return;var d=BaseDefenseFor(building.Owner);d.IncidentCell=building.Cell;d.IncidentAt=Tick;d.AlertUntil=Math.Max(d.AlertUntil,Tick+20*30);}
 void UpdateBaseDefense(int owner){
  var d=BaseDefenseFor(owner);if(d.LastUpdate==Tick)return;d.LastUpdate=Tick;var vision=OpponentVision(owner);
  var besiegers=Opponent.Sieges.Where(p=>World.Buildings.Any(b=>b.Slot==p.Value.Building&&b.Owner==owner)).Select(p=>p.Key).ToHashSet();
  foreach(var a in World.Units.Where(a=>Hostile(owner,a.Owner)&&FightingUnit(a)&&vision[ActorCell(a)]!=0&&InsideDefenseTerritory(owner,ActorCell(a),20))){
   var old=d.Threats.FirstOrDefault(t=>t.Slot==a.Slot);if(old==null){old=new(){Slot=a.Slot};d.Threats.Add(old);}old.Owner=a.Owner;old.Type=a.Type;old.Cell=ActorCell(a);old.Health=a.Health;old.Power=DefensePower(a);old.Seen=Tick;old.AttackingBuilding=besiegers.Contains(a.Slot);old.AttackingEconomy=World.Units.Any(v=>v.Slot==a.AttackTarget&&v.Owner==owner&&(v.Type==9||OriginalWorkerPolicy.Carrier(v.Type)));
  }
  // Discard only observed absence/death; hidden units retain their last observed snapshot.
  d.Threats.RemoveAll(t=>!Hostile(owner,t.Owner)||Tick-t.Seen>BaseThreatMemory||vision[t.Cell]!=0&&!World.Units.Any(a=>a.Slot==t.Slot&&FightingUnit(a)&&ActorCell(a)==t.Cell));
  bool invasion=d.Threats.Any(t=>t.AttackingBuilding||InsideDefenseTerritory(owner,t.Cell,8))||d.IncidentAt>=0&&Tick-d.IncidentAt<=10*30;
  if(d.Threats.Count>0)d.AlertUntil=Math.Max(d.AlertUntil,Tick+12*30);
  d.Mode=invasion?"Invasao":d.Threats.Count>0||Tick<d.AlertUntil?"Alerta":"Vigilancia";
  d.ThreatPower=d.Threats.Sum(t=>t.Power*(t.Seen==Tick?1:.75));d.RequiredPower=d.ThreatPower*DefenseMargin;
  int basis=BaseGuardTarget(owner);var army=World.Units.Where(a=>a.Owner==owner&&FightingUnit(a)&&!TradeDeliveryPending(a)).ToArray();
  double reference=army.Select(DefensePower).DefaultIfEmpty(DefenseTypePower(0,100)).Average();d.GarrisonTarget=Math.Clamp(Math.Max(basis,(int)Math.Ceiling(d.RequiredPower/Math.Max(1,reference))+1),basis,24);
  var eligible=army.Where(a=>DefenseAvailable(a)&&!ResidentReserved(a.Slot)).ToArray();
  var guards=eligible.OrderBy(a=>OpponentDeployed(a)?1:0).ThenBy(a=>InsideDefenseTerritory(owner,ActorCell(a),24)?0:1).ThenByDescending(a=>DefensePower(a)).ThenBy(a=>a.Slot).Take(Math.Min(d.GarrisonTarget,eligible.Length)).ToArray();
  if(d.Mode=="Vigilancia")guards=guards.Where(a=>!OpponentDeployed(a)).ToArray();
  foreach(int id in d.Guards.Keys.Where(id=>!guards.Any(a=>a.Slot==id)).ToArray())d.Guards.Remove(id);
  foreach(var a in guards){if(!d.Guards.ContainsKey(a.Slot))d.Guards[a.Slot]=new();if(d.Mode!="Vigilancia"){Opponent.Sorties.Remove(a.Slot);if(Opponent.MissionFourWaves.TryGetValue(owner,out var wave))wave.Members.Remove(a.Slot);}}
  d.GuardPower=guards.Sum(DefensePower);var homes=DefenseBases(owner);if(homes.Count==0){d.Guards.Clear();return;}
  var allocated=new HashSet<int>();int reserve=guards.Length>=3?Math.Max(1,guards.Length/4):0;int budget=d.Mode=="Invasao"&&d.ThreatPower>d.GuardPower*1.5?guards.Length:Math.Max(0,guards.Length-reserve);
  var clusters=d.Threats.GroupBy(t=>homes.OrderBy(h=>CellDistance(h.Cell,t.Cell)).First().Slot*4+DefenseSector(homes.OrderBy(h=>CellDistance(h.Cell,t.Cell)).First().Cell,t.Cell))
   .OrderByDescending(g=>g.Any(t=>t.AttackingBuilding)).ThenByDescending(g=>g.Sum(t=>t.Power)).ThenBy(g=>g.Key).ToArray();
  foreach(var cluster in clusters){int cell=cluster.OrderBy(t=>t.Slot).First().Cell;double need=cluster.Sum(t=>t.Power)*DefenseMargin,assigned=0;int count=0;int limit=cluster.Count()==1&&!cluster.Any(t=>t.AttackingBuilding)?Math.Min(2,budget):budget;
   foreach(var a in guards.Where(a=>!allocated.Contains(a.Slot)&&(a.Type!=2||guards.All(g=>g.Type==2)||d.ThreatPower>d.GuardPower*1.5)).OrderBy(a=>CellDistance(ActorCell(a),cell)).ThenByDescending(DefensePower).ThenBy(a=>a.Slot)){
    if(count>=limit||allocated.Count>=budget||assigned>=need)break;allocated.Add(a.Slot);count++;assigned+=DefensePower(a);
    var order=d.Guards[a.Slot];var home=homes.OrderBy(h=>CellDistance(h.Cell,cell)).First();order.Anchor=home.Cell;order.Sector=DefenseSector(home.Cell,cell);
    var targets=cluster.Where(t=>(d.Guards[a.Slot].BlockedTarget!=t.Slot||Tick>=d.Guards[a.Slot].BlockedUntil)&&World.Units.Any(b=>b.Slot==t.Slot&&Hostile(owner,b.Owner)&&FightingUnit(b)&&vision[ActorCell(b)]!=0&&CanAttack(a,b)))
     .OrderByDescending(t=>(t.AttackingBuilding?4:t.AttackingEconomy?3:1)*Math.Max(1,t.Power)*DefenseCounter(a.Type,t.Type)/(1+d.Guards.Count(p=>p.Key!=a.Slot&&p.Value.Target==t.Slot))).ThenBy(t=>CellDistance(ActorCell(a),t.Cell)).ThenBy(t=>t.Slot).ToArray();
    order.Target=targets.FirstOrDefault()?.Slot??-1;int tx=order.Target<0?cell:targets[0].Cell;order.Post=DefensePost(a,home,tx,order.Sector,count); 
   }
  }
  if(clusters.Length==0&&d.IncidentAt>=0&&Tick-d.IncidentAt<=10*30){foreach(var a in guards.OrderBy(a=>CellDistance(ActorCell(a),d.IncidentCell)).ThenBy(a=>a.Slot).Take(budget)){allocated.Add(a.Slot);var home=World.Buildings.Where(b=>b.Owner==owner).OrderBy(b=>CellDistance(b.Cell,d.IncidentCell)).First();var order=d.Guards[a.Slot];order.Anchor=home.Cell;order.Sector=DefenseSector(home.Cell,ActorCell(a));order.Target=-1;order.Post=DefensePost(a,home,d.IncidentCell,order.Sector,allocated.Count);}}
  int index=0;foreach(var a in guards.Where(a=>!allocated.Contains(a.Slot)).OrderBy(a=>a.Type==2?1:0).ThenBy(a=>a.Slot)){var home=homes[index%homes.Count];var order=d.Guards[a.Slot];order.Anchor=home.Cell;order.Sector=index/homes.Count%4;order.Target=-1;order.Post=DefensePost(a,home,-1,order.Sector,index++);}
 }
 int DefenseSector(int anchor,int cell){int dx=(cell&255)-(anchor&255),dy=(cell>>8)-(anchor>>8);return Math.Abs(dx)>=Math.Abs(dy)?dx>=0?0:2:dy>=0?1:3;}
 double DefenseCounter(int type,int enemy){var r=CombatRule(type);return Math.Max(0,r.Damage[enemy]-CombatRule(enemy).Armor)*30.0/Math.Max(1,r.Reload)*(1+Math.Min(16,r.Range)*.03);}
 int DefensePost(Actor a,Building home,int threat,int sector,int index){
  var r=Rule(home.Type);int x=(home.Cell&255)+r.Width/2,y=(home.Cell>>8)+r.Height/2;var dirs=new[]{(1,0),(0,1),(-1,0),(0,-1)};var (dx,dy)=dirs[sector];int range=CombatRule(a.Type).Range;
  int offset=threat>=0?Math.Max(3,Math.Max(r.Width,r.Height)/2+2):Math.Max(r.Width,r.Height)/2+(a.Owner==2?3:5);
  if(range>1||a.Type==2)offset=Math.Max(2,offset-2);int spread=index%3-1;x=Math.Clamp(x+dx*offset-dy*spread,0,World.Width-1);y=Math.Clamp(y+dy*offset+dx*spread,0,World.Height-1);
  var used=BaseDefenseFor(a.Owner).Guards.Where(p=>p.Key!=a.Slot).Select(p=>p.Value.Post).ToHashSet();
  return NearbyCells(y*256+x,3).Where(c=>CanTraverse(a,c&255,c>>8)&&!used.Contains(c)&&!World.Units.Any(u=>u!=a&&VisualCatalog.Visible(u)&&!Airborne(u)&&ActorCell(u)==c))
   .OrderBy(c=>CellDistance(c,y*256+x)+(threat<0&&a.Owner==2?FreeDefenseNeighbors(a,c):0)).ThenBy(c=>c).FirstOrDefault(ActorCell(a));
 }
 int FreeDefenseNeighbors(Actor a,int cell)=>NavigationDirections.Count(d=>ValidMapCell(cell+d.Item2*256+d.Item1)&&CanTraverse(a,(cell&255)+d.Item1,(cell>>8)+d.Item2));
 bool BaseDefenseControls(Actor a)=>OpponentEnabled&&a.Owner is >0 and <4&&Opponent.BaseDefenses.TryGetValue(a.Owner,out var d)&&d.Guards.ContainsKey(a.Slot);
 void PruneBaseGuardOwnership(){foreach(var pair in Opponent.BaseDefenses)foreach(int id in pair.Value.Guards.Keys.Where(id=>!World.Units.Any(a=>a.Slot==id&&a.Owner==pair.Key)).ToArray())pair.Value.Guards.Remove(id);}
 void ExecuteBaseDefense(int owner){var d=BaseDefenseFor(owner);foreach(var p in d.Guards.OrderBy(p=>p.Key)){
  var a=World.Units.FirstOrDefault(a=>a.Slot==p.Key);var order=p.Value;if(a==null||a.Owner!=owner||!DefenseAvailable(a))continue;
  var target=World.Units.FirstOrDefault(b=>b.Slot==order.Target);bool valid=target!=null&&CanAttack(a,target)&&Hostile(owner,target.Owner)&&InsideDefenseTerritory(owner,ActorCell(target),owner==2?12:18)&&d.Threats.Any(t=>t.Slot==target.Slot&&t.Seen>=Tick-30);
  if(valid&&CombatRule(a.Type).Range>1&&TileDistanceSquared(a,target!)<Math.Pow(Math.Max(1,CombatRule(a.Type).Range/3),2)&&ActorCell(a)!=order.Post){if(Tick>=order.MoveAfter){ClearOrders(a);order.MoveAfter=Tick+90;Order(a,order.Post&255,order.Post>>8,true);}if(a.Route.Count>0)continue;}
  if(valid){if(a.AttackTarget!=target!.Slot){ClearOrders(a);a.AttackTarget=target.Slot;a.AutomaticDefense=true;a.DefenseAnchor=order.Anchor;}continue;}
  if(a.AttackTarget>=0||Sieging(a)){ClearOrders(a);order.MoveAfter=Tick;}if(Tick<order.MoveAfter||ActorCell(a)==order.Post)continue;order.MoveAfter=Tick+60;
  if(a.Route.Count>0&&a.Route.Last()==order.Post)continue;Order(a,order.Post&255,order.Post>>8,true);
 }}
 void CoordinatedDefenseStep(){if(!OpponentEnabled)return;foreach(int owner in InitialActors.Values.Select(a=>a.Owner).Where(o=>o is >0 and <4).Distinct().Order()){if(IsFifthMission&&ClanEliminated(owner))continue;if(Tick%30==0)UpdateBaseDefense(owner);ExecuteBaseDefense(owner);if(Tick%30==0)DefensiveSupport(owner);}}
 void DefensiveSupport(int owner){
  var d=BaseDefenseFor(owner);if(Tick%150==0&&DefenseShortfall(owner)){
   var temple=World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==6&&Inhabited(b)&&!Special.Learning.Any(j=>j.Building==b.Slot&&!j.Finished));
   if(temple!=null)foreach(int id in new[]{3,1,4,2,0}){var rule=Spell(id);bool unlocked=UnlockData.Technologies.Any(t=>t.Index<OpponentLevel(owner,t.Branch)&&SpecialData.SpellUnlocks.GetValueOrDefault(t.Key,Array.Empty<int>()).Contains(id));if(!unlocked||Special.Learning.Any(j=>j.Owner==owner&&j.Spell==id)||World.Resources[owner]<rule.Food+DefenseFoodReserve(owner)||World.Resources[4+owner]<rule.Rock||World.Resources[8+owner]<rule.Parts)continue;World.Resources[owner]-=(ushort)rule.Food;World.Resources[4+owner]-=(ushort)rule.Rock;World.Resources[8+owner]-=(ushort)rule.Parts;Special.Learning.Add(new(){Owner=owner,Building=temple.Slot,Spell=id});break;}
  }
  if(d.Mode=="Vigilancia")return;var vision=OpponentVision(owner);
  foreach(var caster in World.Units.Where(a=>a.Owner==owner&&a.Type==4&&VisualCatalog.Visible(a)&&!Feeding(a)&&a.CastAnimationUntil<=Tick)){
   var enemy=d.Threats.Where(t=>t.Seen==Tick).OrderByDescending(t=>t.Power).Select(t=>World.Units.FirstOrDefault(a=>a.Slot==t.Slot)).FirstOrDefault(a=>a!=null&&a.PetrifiedUntil<=Tick&&CanCastSpell(caster,1,ActorCell(a),a));if(enemy!=null&&CastSpell(caster,1,ActorCell(enemy),enemy))continue;
   var guard=World.Units.Where(a=>a.Owner==owner&&FightingUnit(a)&&a.ArmoredUntil<=Tick&&a.AttackTarget>=0).OrderByDescending(DefensePower).FirstOrDefault(a=>CanCastSpell(caster,3,ActorCell(a),a));if(guard!=null&&CastSpell(caster,3,ActorCell(guard),guard))continue;
   var clustered=d.Threats.Where(t=>t.Seen==Tick&&d.Threats.Count(q=>q.Seen==Tick&&CellDistance(q.Cell,t.Cell)<=1)>=3&&!World.Units.Any(a=>a.Health>0&&!Hostile(owner,a.Owner)&&VisualCatalog.Visible(a)&&CellDistance(ActorCell(a),t.Cell)<=1)).OrderByDescending(t=>t.Power).FirstOrDefault(t=>CanCastSpell(caster,4,t.Cell));if(clustered!=null&&CastSpell(caster,4,clustered.Cell))continue;
   var mirror=World.Units.Where(a=>a.Owner==owner&&FightingUnit(a)&&a.Type!=4&&a.AttackTarget>=0).OrderByDescending(DefensePower).FirstOrDefault(a=>CanCastSpell(caster,2,ActorCell(a),a));if(mirror!=null&&Special.Mirrors.Count(m=>m.Seed.Owner==owner)<2)CastSpell(caster,2,ActorCell(mirror),mirror);
  }
  foreach(var drone in World.Units.Where(a=>a.Owner==owner&&a.Type==13&&VisualCatalog.Visible(a)&&a.RepairTarget<0&&!Feeding(a))){var injured=World.Units.Where(a=>a.Owner==owner&&a.Type is >=11 and <=17&&a.Health>0&&a.Health<MaxHealth(a)&&VisualCatalog.Visible(a)&&CellDistance(ActorCell(a),ActorCell(drone))<=12&&OpponentSafeCell(owner,ActorCell(a))).OrderBy(a=>(double)a.Health/MaxHealth(a)).FirstOrDefault();if(injured!=null){ClearOrders(drone);drone.RepairTarget=injured.Slot;}}
 }
 int[] DefensiveArmyCycle(int owner,int[] legal){if(!DefenseShortfall(owner))return legal;var d=BaseDefenseFor(owner);var army=World.Units.Where(a=>a.Owner==owner&&FightingUnit(a)).ToArray();
  double Value(int type){var r=CombatRule(type);double counter=d.Threats.Count==0?r.Damage.Where(x=>x>0).DefaultIfEmpty(1).Average():d.Threats.Sum(t=>DefenseCounter(type,t.Type)*t.Power)/Math.Max(1,d.ThreatPower);var cost=TrainingRule(type);double role=type==2&&!army.Any(a=>a.Type==2)&&army.Length>=5?1.3:r.Range>1&&!army.Any(a=>CombatRule(a.Type).Range>1)?1.25:1;
   return counter*role*(1+(PersonalityFor(owner).ArmyCycle.Contains(type)?.1:0))/Math.Max(1,cost.Food+cost.Rock*.5+cost.Parts);}
  return legal.OrderByDescending(Value).ThenBy(t=>t).ToArray();
 }
 bool TryDefenseFortification(int owner){
  if(owner!=2||EconomicOpening(owner)||DefenseEmergency(owner)||DefenseShortfall(owner)||!EconomyReady(owner)||Tick%450!=0||!OpponentBuildingUnlocked(owner,9))return false;
  var home=DefenseBases(owner).FirstOrDefault();if(home==null||World.Buildings.Count(b=>b.Owner==owner&&b.Type==9&&CellDistance(b.Cell,home.Cell)<18)>=6)return false;
  var worker=World.Units.FirstOrDefault(a=>a.Owner==owner&&a.Type==7&&VisualCatalog.Visible(a)&&!Feeding(a)&&!Repairing(a)&&!TradeDeliveryPending(a)&&a.Food==0&&a.Rock==0&&!Constructions.Any(c=>!Complete(c)&&HasBuilder(c,a.Slot)));if(worker==null)return false;
  var price=Rule(9);if(World.Resources[owner]<price.Food+EconomyFor(owner).Reserve||World.Resources[4+owner]<price.Rock+PolicyRockReserve(owner)||World.Resources[8+owner]<price.Parts)return false;
  // Short wings, never a sealed ring; retain a seven-cell central corridor.
  var guards=BaseDefenseFor(owner).Guards.Values.ToArray();if(guards.Length==0)return false;var dirs=new[]{(1,0),(0,1),(-1,0),(0,-1)};var rule=Rule(home.Type);
  foreach(int sector in Enumerable.Range(0,4).OrderByDescending(n=>guards.Count(g=>g.Sector==n)).ThenBy(n=>n)){var (dx,dy)=dirs[sector];int cx=(home.Cell&255)+rule.Width/2+dx*9,cy=(home.Cell>>8)+rule.Height/2+dy*9;
   foreach(int offset in new[]{-4,4,-5,5,-6,6}){int x=cx-dy*offset,y=cy+dx*offset;if(x<1||y<1||x>=World.Width-1||y>=World.Height-1)continue;int cell=y*256+x;if(!OpponentSafeCell(owner,cell)||DoorCorridor(cell)||FreeDefenseNeighbors(worker,cell)<3||World.Objects[cell]!=0)continue;if(StartConstruction(worker,9,cell))return true;}}
  return false;
 }
 void ValidateBaseDefenses(Save s,OpponentState state){if(state.BaseDefenses==null||state.BaseDefenses.Count>3)throw new InvalidDataException("Invalid base defenses");foreach(var p in state.BaseDefenses){var d=p.Value;if(p.Key is <1 or >3||d==null||d.Mode is not ("Vigilancia" or "Alerta" or "Invasao")||d.LastUpdate< -1||d.LastUpdate>s.Tick||d.AlertUntil<0||d.IncidentAt< -1||d.IncidentAt>s.Tick||d.IncidentCell!=-1&&!ValidMapCell(d.IncidentCell)||d.GarrisonTarget is <3 or >24||!double.IsFinite(d.ThreatPower)||!double.IsFinite(d.RequiredPower)||!double.IsFinite(d.GuardPower)||d.ThreatPower<0||d.RequiredPower<0||d.GuardPower<0||d.Threats==null||d.Guards==null||d.Threats.Count>1024||d.Threats.Select(t=>t.Slot).Distinct().Count()!=d.Threats.Count||d.Guards.Count>24)throw new InvalidDataException("Invalid base defense state");
  foreach(var t in d.Threats)if(!s.Units.Any(a=>a.Slot==t.Slot)||t.Owner is <0 or >3||t.Owner==p.Key||t.Type is <0 or >26||!ValidMapCell(t.Cell)||t.Seen<0||t.Seen>s.Tick||t.Health<0||!double.IsFinite(t.Power)||t.Power<0)throw new InvalidDataException("Invalid observed base threat");
  foreach(var g in d.Guards)if(!s.Units.Any(a=>a.Slot==g.Key&&a.Owner==p.Key)||!ValidMapCell(g.Value.Anchor)||!ValidMapCell(g.Value.Post)||g.Value.Sector is <0 or >3||g.Value.Target< -1||g.Value.Target>=0&&!s.Units.Any(a=>a.Slot==g.Value.Target)||g.Value.MoveAfter<0||g.Value.BlockedTarget< -1||g.Value.BlockedUntil<0)throw new InvalidDataException("Invalid base guard");
 }}
}
