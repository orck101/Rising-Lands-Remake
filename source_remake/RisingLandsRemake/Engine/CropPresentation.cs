using System.Text.Json;
namespace RisingLands;
public record CropVisualRow(int Discount,int Residents,int Food,List<DrawCommand> Commands);
public record CropVisualData(ResidentAnchor Anchor,List<CropVisualRow> Rows);
public record CropPrice(int Level,int Discount,int Food);
public record CropNativeRules(List<CropPrice> Prices,int FoodCapacity,int Delay);
public static class CropPresentation {
 static readonly JsonSerializerOptions Options=new(){PropertyNameCaseInsensitive=true,PropertyNamingPolicy=JsonNamingPolicy.SnakeCaseLower};
 public static readonly CropVisualData Data=JsonSerializer.Deserialize<CropVisualData>(File.ReadAllText(PackageLayout.FromBase("Content/Data/crop_visuals.json")),Options)!;
 public static readonly CropNativeRules Rules=JsonSerializer.Deserialize<CropNativeRules>(File.ReadAllText(PackageLayout.FromBase("Content/Data/culture_native.json")),Options)!;
 public static int FoodCost(int agricultureLevel)=>Rules.Prices[Math.Clamp(agricultureLevel,0,Rules.Prices.Count-1)].Food;
 public static IReadOnlyList<DrawCommand> Commands(Building building,int residents,int food,int agricultureLevel){
  int discount=5-FoodCost(agricultureLevel);var row=Data.Rows.First(r=>r.Discount==discount&&r.Residents==Math.Clamp(residents,0,5)&&r.Food==Math.Clamp(food,0,5));
  int x=(building.Cell&255)*32+Data.Anchor.X,y=(building.Cell>>8)*32+Data.Anchor.Y;
  return row.Commands.Select(c=>c with{X=c.X+x,Y=c.Y+y}).ToArray();
 }
}
public sealed partial class Simulation {
 // EnginDisponible 0x457e0c checks the first 0x320 original entity records.
 public const int NativeEntityCapacity=800;
 public int CropFood(int slot)=>Cultures.FirstOrDefault(c=>c.Building==slot)?.Food??0;
 public string CropStatus(Building b){
  if(!Inhabited(b))return "Coloque um colono no cultivo para gerar novos colonos.";
  if(Occupants(b.Slot)>=ResidentCapacity(b))return "Cultivo lotado. A proxima entrega libera um colono pela frente.";
  if(World.Units.Count(a=>a.Health>0)>=NativeEntityCapacity)return "Sem espaco para novas unidades. Alimento preservado no cultivo.";
  return "Cultivo ativo. Entregue "+CultureFoodCost()+" alimentos para gerar um colono.";
 }
 public bool DepositCropFood(Actor worker,Building crop){
  if(crop.Type!=4||crop.Owner!=worker.Owner||worker.Type!=5||worker.Owner is <0 or >3||worker.CargoKind!=1||worker.Food<=0)return false;
  // BatimentAjouteMatiere 0x4235a9: capacity is tested before food.
  if(Residents(crop.Slot)>=ResidentCapacity(crop)){
   int drop=GroundDeliveryCells(crop.Cell+512,1).FirstOrDefault(-1);
   if(drop<0||ReleaseResident(crop.Slot,crop.Owner)==null)return false;
   int previous=World.Objects[drop]==67?World.Quantities[drop]:0;World.Objects[drop]=67;World.Quantities[drop]=(byte)(previous+1);
   if(!worker.GroundDrops.Contains(drop))worker.GroundDrops.Add(drop);
  }else{
   var culture=Culture(crop.Slot);if(culture.Food<CropPresentation.Rules.FoodCapacity)culture.Food++;else culture.Consumed++;
  }
  worker.Food--;return true;
 }
}
