namespace RisingLands;
public sealed partial class Simulation {
 bool RecruitSoldier(int type,int buildingSlot){
  if(!CanRecruit(type,buildingSlot))return false;var rule=TrainingRule(type);int slot=World.BuildingResidents[buildingSlot].First(id=>World.Units.Any(a=>a.Slot==id&&a.Type==9&&a.LegacyState==14&&a.Health>0)&&!ResidentReserved(id));
  World.Resources[0]-=(ushort)rule.Food;World.Resources[4]-=(ushort)rule.Rock;World.Resources[8]-=(ushort)rule.Parts;
  Recruitments.Add(new(){Type=type,SettlerSlot=slot,BuildingSlot=buildingSlot,SourceBuildingSlot=buildingSlot,TravelCell=World.Buildings.First(b=>b.Slot==buildingSlot).Cell,Arrived=true});return true;
 }
}
