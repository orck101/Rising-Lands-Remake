namespace RisingLands;
public sealed partial class Simulation {
 bool TradeOpening(int owner)=>MissionNumber==4&&owner==1&&Opponent.TradeFirstOpening&&!MissionFourPressure(owner);
 int TradePriorityConstruction(int owner){if(!TradeOpening(owner))return -1;foreach(int type in new[]{16,8,4,2,1})if(!World.Buildings.Any(b=>b.Owner==owner&&b.Type==type)){if(type==2&&World.Buildings.Count(b=>b.Owner==owner&&b.Type==3)<2)return 3;return type;}return -1;}
 void StaffTradeOpening(int owner){
  StaffOpponentWarehouses(owner);int reserved=WarehouseOperatorSlot(owner);
  foreach(int type in new[]{16,8,3,4,2}){
   var target=World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==type&&!Inhabited(b));if(target==null||ResidentTrips.Any(t=>t.Building==target.Slot))continue;
   var settler=World.Units.FirstOrDefault(a=>a.Slot!=reserved&&a.Owner==owner&&a.Type==9&&VisualCatalog.Visible(a)&&a.Route.Count==0&&!ResidentReserved(a.Slot));
   if(settler==null){
    var source=World.Buildings.Where(b=>b.Owner==owner&&b.Slot!=target.Slot&&OpponentFreeResidents(b)>(World.BuildingResidents.GetValueOrDefault(b.Slot,new()).Contains(reserved)?2:1)).OrderBy(b=>b.Type==4?0:1).FirstOrDefault();
    // One operator must temporarily seed cultivation: its real births staff new buildings.
    if(source==null&&type==4)source=World.Buildings.FirstOrDefault(b=>b.Owner==owner&&b.Type==8&&OpponentFreeResidents(b)>0);
    if(source!=null){var candidate=World.Units.FirstOrDefault(a=>a.Slot!=reserved&&a.Owner==owner&&a.Type==9&&a.Health>0&&a.LegacyState==14&&World.BuildingResidents.GetValueOrDefault(source.Slot,new()).Contains(a.Slot)&&!ResidentReserved(a.Slot));if(candidate!=null)settler=ReleaseResident(source.Slot,owner,candidate.Slot);}
   }
   if(settler!=null&&EnterBuilding(settler,target.Slot))return;
  }
 }
}
