namespace RisingLands;
public static class WarehouseNetworkTests {
 static void Check(bool value,string name){if(!value)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
 public static void Run(string root){
  var s=MissionOneTests.Load(root);var initial=s.WarehouseSummary();Check(initial.Count==0&&initial.Occupancy==initial.Food+initial.Rock&&initial.Free==0&&initial.Status=="Sem armazem","initial spendable resources remain visible without a warehouse");
  ResearchTests.Build(s,3);var baseline=s.WarehouseSummary();s.World.Resources[0]+=(ushort)(99-baseline.Occupancy);
  var partial=s.WarehouseSummary();Check(partial.Occupancy==99&&partial.Free==1&&partial.Status=="Espaco livre","all spendable food and stone share warehouse capacity");
  s.World.Resources[8]+=500;Check(s.WarehouseSummary().Occupancy==99,"parts do not change native food-stone occupancy");
  s.World.Resources[0]++;Check(s.WarehouseSpace()==0&&s.WarehouseSummary().Status=="Lotado","exact capacity is reported as full");
  s.World.Resources[4]++;Check(s.WarehouseSpace()==0&&s.WarehouseSummary().Status=="Superlotado","overcapacity never yields negative free space");
  s.World.Resources[0]-=3;Check(s.WarehouseSpace()==2,"withdrawal immediately releases shared capacity");
  s.World.Resources[8]-=500;s.World.Resources[0]=(ushort)baseline.Food;s.World.Resources[4]=(ushort)baseline.Rock;
  ResearchTests.Build(s,3);Check(s.WarehouseSummary().Count==2&&s.WarehouseSummary().Capacity==200,"second finished warehouse expands network capacity");
 }
}
