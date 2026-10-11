using System.Text.Json;
namespace RisingLands;
public record ProductionBuilding(int Building,int[] Troops);
public record ProductionCatalog(List<ProductionBuilding> Buildings);
public sealed partial class Simulation {
 public static readonly ProductionCatalog ProductionData=JsonSerializer.Deserialize<ProductionCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/production_buildings.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 static readonly Dictionary<string,int[]> ProductionUnlocks=JsonSerializer.Deserialize<Dictionary<string,int[]>>(File.ReadAllText(PackageLayout.FromBase("Content/Data/production_unlocks.json")))!;
 int TrainingPopulation(int slot)=>World.BuildingResidents.GetValueOrDefault(slot,new()).Count(id=>World.Units.Any(a=>a.Slot==id&&a.Type==9&&a.Health>0&&a.LegacyState==14)&&!ResearchReserved(id));
 public static int[] ProductionTypes(int building)=>ProductionData.Buildings.FirstOrDefault(b=>b.Building==building)?.Troops??Array.Empty<int>();
 public static bool Produces(int building,int troop)=>ProductionTypes(building).Contains(troop);
 void MigrateProduction(Save save){if(save.Version<=26)foreach(var job in save.Recruitments){var b=InitialBuildings.Concat(save.Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).FirstOrDefault(b=>b.Slot==job.BuildingSlot);job.LegacyBarracks=b?.Type==1&&job.Type is 11 or 12 or 14;}}
 bool ValidProductionBuilding(Recruitment job,Building b)=>Produces(b.Type,job.Type)||job.LegacyBarracks&&b.Type==1&&job.Type is 11 or 12 or 14;
}
