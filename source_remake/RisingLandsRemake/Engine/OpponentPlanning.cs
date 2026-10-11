namespace RisingLands;

public sealed record OpponentDecision(int Owner,string Personality,string Priority,string Reason,int Farmers,int Builders,int Engineers,int Army,int FoodReserve,int StoneTarget,int PartsTarget,int NextBuilding,bool CanAttack);
public sealed partial class Simulation {
 HashSet<int> CropSupplierSlots(int owner,Building? crop){
  if(crop==null||!OpponentSeedNeeded(owner))return new();
  // Once there is a spare colonist, bank the next harvest for essential worker
  // training instead of consuming every food unit in further births.
  if(EconomicWorkerRecovery(owner)&&World.Buildings.Any(b=>b.Owner==owner&&b.Type==2&&OpponentFreeResidents(b)>1)&&World.Resources[owner]<TrainingRule(5).Food+OpeningFoodFloor(owner))return new();
  var farmers=World.Units.Where(a=>a.Owner==owner&&a.Type==5&&a.Health>0&&VisualCatalog.Visible(a)).OrderByDescending(a=>a.Slot).ToArray();
  return farmers.Length>0&&(farmers.Length>=2||EconomicWorkerRecovery(owner))?new(){farmers[0].Slot}:new();
 }
 bool OwnService(int owner,int type)=>World.Buildings.Any(b=>b.Owner==owner&&b.Type==type&&Inhabited(b));
 int CoreResidentDeficit(int owner)=>World.Buildings.Where(b=>b.Owner==owner&&b.Type is 2 or 3 or 4&&!Inhabited(b)).Count()+(!World.Buildings.Any(b=>b.Owner==owner&&b.Type==4)?1:0);
 bool PopulationEstablished(int owner)=>OwnService(owner,4)&&World.Units.Count(a=>a.Owner==owner&&a.Type==9&&a.Health>0)>=4;
 bool EnemyResearchAvailable(int owner){int branch=Campaign.EnemyBranches[owner];return branch>=0&&Opponent.ResearchCompleted[owner]<2&&UnlockData.Technologies.Any(t=>t.Branch==branch&&t.Index==OpponentLevel(owner,branch)&&t.Mission<=MissionNumber);}
 int PlannedBuilding(int owner){
  if(DefenseEmergency(owner)&&BasicServices(owner)&&!World.Buildings.Any(b=>b.Owner==owner&&b.Type==1)&&!Constructions.Any(c=>c.Owner==owner&&c.Type==1)&&OpponentBuildingUnlocked(owner,1))return 1;
  if(EconomicOpening(owner))return OpeningBuilding(owner);
  int opening=TradePriorityConstruction(owner);if(opening>=0)return opening;
  foreach(int type in new[]{2,3,4,1})if(!World.Buildings.Any(b=>b.Owner==owner&&b.Type==type)&&!Constructions.Any(c=>c.Owner==owner&&c.Type==type)&&OpponentBuildingUnlocked(owner,type))return type;
  if(WarehouseExpansionNeeded(owner)&&WarehouseOperatorSlot(owner)>=0)return 3;
  if(!BasicServices(owner))return -1;
  if(EnemyResearchAvailable(owner)&&!World.Buildings.Any(b=>b.Owner==owner&&b.Type==7)&&OpponentBuildingUnlocked(owner,7)&&World.Resources[owner]>=EconomyFor(owner).Reserve+Rule(7).Food)return 7;
  // Diplomacy is infrastructure, not a special cheat opening for mission five.
  if(OpponentArmyCount(owner)>=PolicyGuards(owner)&&PopulationEstablished(owner))foreach(int type in new[]{16,8})if(OpponentBuildingUnlocked(owner,type)&&!World.Buildings.Any(b=>b.Owner==owner&&b.Type==type)&&World.Resources[owner]>=EconomyFor(owner).Reserve+Rule(type).Food)return type;
  return -1;
 }
 public OpponentDecision DecisionFor(int owner){
  var e=EconomyFor(owner);if(EconomicOpening(owner))return new(owner,PersonalityFor(owner).Name,BaseInvaded(owner)?"Defender base":"Abertura economica","Armazem, comida, cultivo e colonos antes de ofensivas.",OpeningFarmers(owner),1,0,PolicyGuards(owner),OpeningFoodFloor(owner),Math.Max(30,OpeningBuilding(owner)>=0?Rule(OpeningBuilding(owner)).Rock:0),0,OpeningBuilding(owner),false);
  int wanted=PlannedBuilding(owner);int builders=BasicServices(owner)&&PopulationEstablished(owner)&&EconomyFor(owner).Income>e.Maintenance+2?2:1;
  int farmers=Math.Min(e.FarmerTarget,PopulationEstablished(owner)?PolicyFor(owner).MaximumFarmers:3);
  int stone=Math.Max(PolicyRockReserve(owner),wanted>=0?Rule(wanted).Rock:0);
  int parts=Math.Max(0,wanted>=0?Rule(wanted).Parts:0);
  foreach(int type in PersonalityFor(owner).ArmyCycle.Where(t=>OpponentUnitUnlocked(owner,t)&&RecruitData.Rules.Any(r=>r.Type==t)).Distinct())parts=Math.Max(parts,TrainingRule(type).Parts*2);
  int engineers=parts>World.Resources[8+owner]&&PopulationEstablished(owner)&&OpponentUnitUnlocked(owner,6)?1:0;
  string priority=OpponentWorkers(owner,5)==0||OpponentWorkers(owner,7)==0?"Recuperar trabalhadores":!OwnService(owner,3)?"Ativar armazenamento":!OwnService(owner,4)?"Gerar colonos":WarehouseExpansionNeeded(owner)?"Ampliar armazenamento":BaseInvaded(owner)?"Defender base":!EconomyReady(owner)?"Estabilizar economia":OpponentArmyCount(owner)<PolicyGuards(owner)+PolicyFor(owner).MinimumWave?"Formar defesa e excedente":EnemyResearchAvailable(owner)?"Pesquisar e desenvolver":"Preparar ofensiva";
  string reason=priority switch{"Recuperar trabalhadores"=>"Uma profissÃƒÂ£o essencial estÃƒÂ¡ sem trabalhador.","Ativar armazenamento"=>"Recursos precisam de depÃƒÂ³sito operado.","Gerar colonos"=>"Cultivo e operadores sÃƒÂ£o necessÃƒÂ¡rios antes de consumir mais mÃƒÂ£o de obra.","Ampliar armazenamento"=>WarehouseOperatorSlot(owner)<0?"Capacidade pressionada; reservar ou gerar operador antes de outra obra.":"Capacidade atingiu o limite de expansÃƒÂ£o.","Defender base"=>"AmeaÃƒÂ§a hostil observada perto de serviÃƒÂ§os prÃƒÂ³prios.","Estabilizar economia"=>"Renda, reserva ou serviÃƒÂ§os ainda insuficientes.",_=>"Usar excedente real conforme o perfil e a tecnologia disponÃƒÂ­vel."};
  return new(owner,PersonalityFor(owner).Name,priority,reason,Math.Clamp(farmers,2,14),builders,engineers,OpponentArmyTarget(owner),e.Reserve,stone,parts,wanted,EconomyReady(owner)&&!BaseInvaded(owner)&&!Allied(owner));
 }
 int BuildingStaffGoal(Building b){
  if(b.Type==1&&DefenseShortfall(b.Owner))return Math.Min(ResidentCapacity(b),4);
  if(b.Type is 3 or 4 or 16 or 8)return 1;
  if(b.Type==2)return PopulationEstablished(b.Owner)?3:2;
  if(b.Type==7)return EnemyResearchAvailable(b.Owner)?2:1;
  return RecruitData.Rules.Any(r=>Produces(b.Type,r.Type)&&OpponentUnitUnlocked(b.Owner,r.Type))?3:1;
 }
 bool MoveResident(int owner,Building target,bool allowSeed=false){
  if(Occupants(target.Slot)+ResidentTrips.Count(t=>t.Building==target.Slot)>=ResidentCapacity(target))return false;
  var free=World.Units.FirstOrDefault(a=>a.Owner==owner&&a.Type==9&&a.Health>0&&VisualCatalog.Visible(a)&&!ResidentReserved(a.Slot)&&!TradeDeliveryPending(a));if(free!=null)return EnterBuilding(free,target.Slot);
  var sources=World.Buildings.Where(b=>b.Owner==owner&&b.Slot!=target.Slot&&OpponentFreeResidents(b)>(allowSeed&&b.Type==2?0:b.Type==4?1:b.Type==2?PopulationEstablished(owner)?1:2:BuildingStaffGoal(b))).OrderBy(b=>b.Type==4?0:b.Type==2?1:2).ThenBy(b=>b.Slot);
  foreach(var source in sources){var candidate=World.Units.FirstOrDefault(a=>a.Owner==owner&&a.Type==9&&a.Health>0&&a.LegacyState==14&&World.BuildingResidents.GetValueOrDefault(source.Slot,new()).Contains(a.Slot)&&!ResidentReserved(a.Slot));if(candidate==null)continue;var released=ReleaseResident(source.Slot,owner,candidate.Slot);if(released!=null&&EnterBuilding(released,target.Slot))return true;}
  return false;
 }
 void PlanPopulation(int owner){
  if(RecoverProductionResident(owner))return;
  if(RecoverWoundedSurplus(owner))return;
  var crop=World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==4);
  if(crop!=null&&!Inhabited(crop)&&!ResidentTrips.Any(t=>t.Building==crop.Slot)){
   // Seeding cultivation may temporarily vacate the sanctuary; genuine births replace it.
   if(MoveResident(owner,crop,OpponentWorkers(owner,5)>0&&World.Resources[owner]>=CultureFoodCost(OpponentLevel(owner,0))))return;
  }
  var targets=World.Buildings.Where(b=>b.Owner==owner&&ResidentCapacity(b)>0&&Occupants(b.Slot)+ResidentTrips.Count(t=>t.Building==b.Slot)<BuildingStaffGoal(b))
   .OrderBy(b=>!Inhabited(b)?b.Type==3?0:b.Type==2?1:b.Type==4?2:3:b.Type==1&&DefenseShortfall(owner)?3:b.Type==2?4:b.Type==7&&EnemyResearchAvailable(owner)?5:b.Type==1?6:7).ThenBy(b=>b.Slot).ToArray();
  foreach(var target in targets)if(MoveResident(owner,target))return;
  var home=World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==2&&Occupants(b.Slot)+ResidentTrips.Count(t=>t.Building==b.Slot)<ResidentCapacity(b));if(home==null)return;
  bool essential=OpponentWorkers(owner,5)==0||OpponentWorkers(owner,7)==0;
  bool noReserve=(EconomicOpening(owner)?World.Buildings.Any(b=>b.Owner==owner&&b.Type is 2 or 3 or 4&&!Inhabited(b)):CoreResidentDeficit(owner)>0)||WarehouseOperatorNeeded(owner)&&WarehouseOperatorSlot(owner)<0;
  if(!essential&&!noReserve||ResidentTrips.Any(t=>t.Building==home.Slot))return;
  var donor=World.Units.Where(a=>a.Owner==owner&&a.Health>0&&VisualCatalog.Visible(a)&&!Feeding(a)&&!TradeDeliveryPending(a)&&!ResidentReserved(a.Slot)&&!Constructions.Any(c=>!Complete(c)&&HasBuilder(c,a.Slot)))
   .Where(a=>a.Type==5&&OpponentWorkers(owner,5)>ProductiveFarmerFloor(owner)||a.Type==7&&OpponentWorkers(owner,7)>1||essential&&FightingUnit(a))
   .OrderBy(a=>OriginalWorkerPolicy.Carrier(a.Type)?0:1).ThenBy(a=>a.Slot).FirstOrDefault();
  if(donor!=null)EnterBuilding(donor,home.Slot);
 }
 void PlanRepairs(int owner){
  var damaged=World.Buildings.Where(b=>b.Owner==owner&&BuildingHealth(b)>0&&BuildingHealth(b)<BuildingMaxHealth(b)&&OpponentSafeCell(owner,b.Cell)).OrderBy(b=>b.Type is 2 or 3 or 4?0:1).ThenBy(b=>(double)BuildingHealth(b)/BuildingMaxHealth(b)).FirstOrDefault();if(damaged==null)return;
  var builder=World.Units.FirstOrDefault(a=>a.Owner==owner&&a.Type==7&&a.Health>0&&VisualCatalog.Visible(a)&&!Feeding(a)&&!Repairing(a)&&!ResidentReserved(a.Slot)&&!TradeDeliveryPending(a)&&!Constructions.Any(c=>!Complete(c)&&HasBuilder(c,a.Slot))&&a.Food==0&&a.Rock==0);if(builder!=null)RepairBuilding(builder,damaged.Slot);
 }
 int[] PlannedArmyCycle(int owner,IEnumerable<int> legal){var cycle=EnemyPersonalities.ProductionCycle(owner,legal);var visible=OpponentVision(owner);
  bool airThreat=World.Units.Any(a=>a.Health>0&&Hostile(owner,a.Owner)&&Airborne(a)&&VisualCatalog.Visible(a)&&visible[a.Y/1024*256+a.X/1024]!=0);
  if(airThreat&&cycle.Contains(8)&&World.Units.Count(a=>a.Owner==owner&&a.Type==8&&a.Health>0)<3)return new[]{8}.Concat(cycle).ToArray();
  if(cycle.Contains(2)&&OpponentArmyCount(owner)>=6&&!World.Units.Any(a=>a.Owner==owner&&a.Type==2&&a.Health>0))return new[]{2}.Concat(cycle).ToArray();return cycle;
 }
}



