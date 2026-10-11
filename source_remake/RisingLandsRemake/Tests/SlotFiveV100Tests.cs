using System.Security.Cryptography;
namespace RisingLands;
public static class SlotFiveV100Tests {
 public static void Run(string root,string path){
  OverflowAccounting(root);
  byte[] bytes=File.ReadAllBytes(path);string hash=Convert.ToHexString(SHA256.HashData(bytes));string json=System.Text.Encoding.UTF8.GetString(bytes);
  var s=OpponentV75Tests.Load(root);s.Restore(json);var e=s.EconomyFor(1);long start=s.Tick;bool attack=false,ready=false;int attackUnits=0;long attackTick=-1;
  Console.WriteLine($"BEFORE tick={s.Tick} stockFood={s.World.Resources[1]} stone={s.World.Resources[5]} warehouses={s.WarehouseSummary(1).Capacity} free={s.WarehouseSpace(1)} income={e.Income:F2} maintenance={e.Maintenance:F2} reserve={e.Reserve} knownFood={e.KnownFood} ready={s.EconomyReady(1)} phase={e.Phase}");
  for(int i=0;i<18000;i++){
   s.Step();ready|=s.EconomyReady(1);
   int wave=s.World.Units.Count(a=>a.Owner==1&&s.Opponent.Sorties.ContainsKey(a.Slot));if(wave>0&&!attack){attack=true;attackUnits=wave;attackTick=s.Tick;}
   if(i%1800==1799)Console.WriteLine($"STATE elapsed={s.Tick-start} food={s.World.Resources[1]} income={e.Income:F2} upkeep={e.Maintenance:F2} phase={e.Phase} stable={e.StableTicks} wave={wave}");
   if(attack)break;
  }
  if(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))!=hash)throw new Exception("Snapshot was modified");
  Console.WriteLine($"RESULT ready={ready} attack={attack} wave={attackUnits} seconds={(attackTick-start)/30.0} sourceHash={hash}");
  if(!ready||!attack)throw new Exception("FAIL v100: full warehouses must not permanently block a supplied enemy offensive");
  if(attackUnits>s.PolicyFor(1).MaximumWave)throw new Exception("Oversized wave");
  var copy=OpponentV75Tests.Load(root);copy.Restore(s.Serialize());if(copy.Serialize()!=s.Serialize())throw new Exception("Post-recovery state failed save/load");
  Console.WriteLine("ALL SLOT FIVE v100 TESTS PASSED");
 }
 static void OverflowAccounting(string root){
  var s=OpponentV75Tests.Load(root);var worker=s.World.Units.First(a=>a.Owner==1&&a.Type==5&&a.Health>0);var bag=s.World.Buildings.First(b=>b.Owner==1&&b.Type==3);int cell=s.WarehouseOverflowCells(bag).First(c=>s.GroundResourceSpace(c,1));
  var method=typeof(Simulation).GetMethod("OverflowArrival",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!;var stocks=s.World.Resources.ToArray();
  void Drop(bool planted,bool withdrawal){worker.X=(cell&255)*1024;worker.Y=(cell>>8)*1024;worker.Food=1;worker.CargoKind=1;worker.FoodJob=3;worker.FoodDestinationKind=2;worker.FoodDestination=bag.Slot;worker.DeliveryCell=worker.OverflowCell=cell;worker.FoodPlantsOnly=planted;worker.FoodSourceBuilding=withdrawal?bag.Slot:-1;worker.RepeatFood=false;method.Invoke(s,new object[]{worker,false});}
  Drop(true,false);if(s.Opponent.OverflowFood[1]!=1)throw new Exception("Fresh harvest missing from income");
  Drop(false,false);Drop(true,true);if(s.Opponent.OverflowFood[1]!=1)throw new Exception("Recycled piles or withdrawals became artificial income");
  if(!stocks.SequenceEqual(s.World.Resources)||s.World.Quantities[cell]!=3)throw new Exception("Overflow accounting changed stocks or lost actual delivered cargo");
  Console.WriteLine("PASS v100: fresh harvest counted; moving existing food and withdrawals excluded; stocks unchanged");
 }
}
