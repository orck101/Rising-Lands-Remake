namespace RisingLands;
public sealed partial class Simulation {
 public const long EconomicOpeningTicks=10*60*30;
 public bool EconomicOpening(int owner)=>IsFifthMission&&owner is >0 and <4&&Tick<EconomicOpeningTicks;
 int OpeningFoodFloor(int owner)=>Math.Max(4,(int)Math.Ceiling(PolicyMaintenance(owner)*2));
 int OpeningFarmers(int owner){var e=EconomyFor(owner);int capacity=e.KnownFood<8?2:e.KnownFood<20?4:Tick<5*60*30?5:6;return Math.Clamp(Math.Max(e.FarmerTarget,Tick<3*60*30?3:5),2,OwnService(owner,4)?capacity:Math.Min(3,capacity));}
 int OpeningBuilding(int owner){
  foreach(int type in new[]{2,3,4})if(!World.Buildings.Any(b=>b.Owner==owner&&b.Type==type)&&!Constructions.Any(c=>c.Owner==owner&&c.Type==type)&&OpponentBuildingUnlocked(owner,type))return type;
  if(!BasicServices(owner))return -1;
  if(WarehouseExpansionNeeded(owner)&&WarehouseOperatorSlot(owner)>=0)return 3;
  if(DefenseEmergency(owner)&&!World.Buildings.Any(b=>b.Owner==owner&&b.Type==1)&&!Constructions.Any(c=>c.Owner==owner&&c.Type==1)&&World.Resources[owner]>=Rule(1).Food+OpeningFoodFloor(owner))return 1;
  return -1;
 }
 bool OpeningSeedNeeded(int owner)=>World.Resources[owner]>=OpeningFoodFloor(owner)&&World.Units.Count(a=>a.Owner==owner&&a.Type==9&&a.Health>0)<OpeningFarmers(owner)+6;
 bool OpeningTrainingAllowed(int owner,int type){
  if(!EconomicOpening(owner))return true;
  if(type is not (5 or 7))return DefenseEmergency(owner)&&DefenseShortfall(owner)&&World.Resources[owner]>=TrainingRule(type).Food+OpeningFoodFloor(owner);
  if(OpponentWorkers(owner,type)==0)return true;
  if(type==7&&OpponentWorkers(owner,7)>=1)return false;
  return World.Resources[owner]>=TrainingRule(type).Food+OpeningFoodFloor(owner);
 }
 void DevelopEconomicOpening(int owner){
  int target=OpeningFarmers(owner);int type=OpponentWorkers(owner,5)<target?5:-1;
  if(type>=0)foreach(var home in World.Buildings.Where(b=>b.Owner==owner&&b.Type==2).OrderBy(b=>b.Slot))if(TrainOpponent(owner,home,type))break;
 }
}
