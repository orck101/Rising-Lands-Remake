namespace RisingLands;
public static class WarehouseAccountingTests {
 static void Check(bool v,string name){if(!v)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
 public static void Run(string root){
  var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);
  w.Objects[5642]=67;w.Quantities[5642]=200;w.Buildings.Add(new Building(999,3,0,3345));PopulationFixture.Inhabit(w,999);
  var s=new Simulation(w);s.World.Resources[0]+=80;s.World.Quantities[5642]-=80;s.World.Resources[4]+=20;s.World.Mines[1].Quantity-=20;var info=s.WarehouseSummary();
  Check(info.Occupancy==160&&info.Food==110&&info.Rock==50,"screenshot stock of 110 food and 50 stone is reported as 160 occupied");
  Check(info.Capacity==100&&info.Free==0&&info.Status=="Superlotado","one active warehouse reports 160/100 without silently reducing stock");
  var copy=s.OrderPreview();Check(copy.Serialize()==s.Serialize()&&copy.WarehouseSummary()==info,"overcapacity save preserves all resources and displays actual occupancy");
  s.World.Resources[0]=50;s.World.Resources[4]=50;Check(s.WarehouseSpace()==0&&s.WarehouseSummary().Status=="Lotado","100 combined resources exactly fill one warehouse");
  s.World.Resources[0]--;Check(s.WarehouseSpace()==1&&s.WarehouseSummary().Occupancy==99,"one withdrawn resource releases exactly one storage place");
 }
}
