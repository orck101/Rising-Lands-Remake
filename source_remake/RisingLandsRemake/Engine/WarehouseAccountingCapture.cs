namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareWarehouseAccountingCapture(){
  int slot=ResearchTests.Build(simulation,3);simulation.World.Resources[0]=110;simulation.World.Resources[4]=50;
  var summary=simulation.WarehouseSummary();if(summary.Occupancy!=160||summary.Capacity!=100||summary.Free!=0||summary.Status!="Superlotado")throw new InvalidDataException("Warehouse capture accounting does not match top bar");
  SelectOnly(null);viewedBuilding=slot;var b=simulation.World.Buildings.First(b=>b.Slot==slot);camera=new((b.Cell&255)*32-ViewWidth/2,(b.Cell>>8)*32-ViewHeight/2);ClampCamera();paused=true;
 }
}
