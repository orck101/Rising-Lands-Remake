namespace RisingLands;
public sealed partial class Simulation {
 // Real production supports the garrison AND an offensive group. Injured units
 // may stay home; a bounded replacement allowance avoids an immortal army cap.
 public int OpponentArmyTarget(int owner){
  if(MissionFourPressure(owner))return (int)Math.Min(int.MaxValue,MissionFourRequiredSize(owner)+PolicyGuards(owner));
  var p=PolicyFor(owner);
  int wounded=World.Units.Count(a=>a.Owner==owner&&FightingUnit(a)&&a.Health*2<MaxHealth(a));
  return Math.Max(p.ArmyTarget,PolicyGuards(owner)+p.MaximumWave)+Math.Min(wounded,p.MaximumWave);
 }
 int ProductiveFarmerFloor(int owner){
  int farmers=Math.Max(1,OpponentWorkers(owner,5));var e=EconomyFor(owner);
  double productivity=e.Samples.Count<3?2.5:Math.Max(.5,e.Income/farmers);
  return Math.Clamp((int)Math.Ceiling((PolicyMaintenance(owner)+1)/productivity),2,PolicyFor(owner).MaximumFarmers);
 }
 bool EconomicWorkerRecovery(int owner)=>OpponentWorkers(owner,5)<ProductiveFarmerFloor(owner)||OpponentWorkers(owner,7)==0;
 HashSet<int> OffensiveGuardReservation(int owner,Actor[] army){
  var guards=army.Where(BaseDefenseControls).Select(a=>a.Slot).ToHashSet();
  foreach(var a in army.Where(a=>!guards.Contains(a.Slot)).OrderByDescending(NearOpponentBase).ThenByDescending(DefensePower).ThenBy(a=>a.Slot)){
   if(guards.Count>=PolicyGuards(owner))break;guards.Add(a.Slot);
  }
  return guards;
 }
 bool RecoverProductionResident(int owner){
  if(!EconomicWorkerRecovery(owner))return false;
  var home=World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==2&&OpponentFreeResidents(b)<=1&&Occupants(b.Slot)+ResidentTrips.Count(t=>t.Building==b.Slot)<ResidentCapacity(b));
  if(home==null||ResidentTrips.Any(t=>t.Building==home.Slot))return false;
  if(MoveResident(owner,home))return true;
  // Release spare staff from secondary services, never the last operator or a
  // reserved trainee. Food/storage/cultivation remain operated during recovery.
  foreach(var donor in World.Buildings.Where(b=>b.Owner==owner&&b.Type is not (2 or 3 or 4)&&OpponentFreeResidents(b)>1).OrderBy(b=>b.Type==7?1:0).ThenBy(b=>b.Slot)){
   var a=ReleaseResident(donor.Slot,owner);if(a!=null&&EnterBuilding(a,home.Slot))return true;
  }
  return false;
 }
 bool RecoverWoundedSurplus(int owner){
  if(DefenseEmergency(owner)||OpponentWorkers(owner,5)>=2||World.Resources[owner]>=TrainingRule(5).Food||!OwnService(owner,3)||!OwnService(owner,4)||OpponentArmyCount(owner)<=PolicyGuards(owner)+1)return false;
  var home=World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==2&&Occupants(b.Slot)+ResidentTrips.Count(t=>t.Building==b.Slot)<ResidentCapacity(b));
  if(home==null||ResidentTrips.Any(t=>t.Building==home.Slot&&World.Units.Any(a=>a.Slot==t.Slot&&FightingUnit(a))))return false;
  // Economically recover wounded surplus using the normal sanctuary conversion.
  // No healing, resource grant or deletion; keep the assigned defense intact.
  var surplus=World.Units.Where(a=>a.Owner==owner&&FightingUnit(a)&&a.Health*2<MaxHealth(a)&&!BaseDefenseControls(a)&&!OpponentDeployed(a)&&!ResidentReserved(a.Slot)&&!TradeDeliveryPending(a)&&!Feeding(a)&&a.AttackTarget<0&&!Sieging(a))
   .OrderBy(a=>a.Health).ThenBy(a=>a.Slot);
  foreach(var a in surplus)if(EnterBuilding(a,home.Slot))return true;
  return false;
 }
}
