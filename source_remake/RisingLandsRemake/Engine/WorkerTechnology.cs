using System.Text.Json;
namespace RisingLands;
public record WorkerTechnologyRule(int Agriculture,int Civil,int Type,int Carry,int Reload);
public record WorkerTechnologyCatalog(List<WorkerTechnologyRule> Rows);
public sealed partial class Simulation {
    public static readonly WorkerTechnologyCatalog WorkerTechnologies=JsonSerializer.Deserialize<WorkerTechnologyCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/worker_technology.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
    static readonly Dictionary<(int,int,int),WorkerTechnologyRule> WorkerTechnologyLookup=WorkerTechnologies.Rows.ToDictionary(r=>(r.Agriculture,r.Civil,r.Type));
    public static WorkerTechnologyRule TechnologyFor(int agriculture,int civil,int type)=>WorkerTechnologyLookup.TryGetValue((agriculture,civil,type),out var rule)?rule:throw new InvalidDataException("Unsupported worker technology levels");
    WorkerTechnologyRule? WorkerTechnology(Actor u,EngineeringResearch? research=null)=>!OriginalWorkerPolicy.Carrier(u.Type)?null:u.Owner==0?TechnologyFor((research??Research).Agriculture!.Index,(research??Research).Index,u.Type):u.Owner is >0 and <4?TechnologyFor(OpponentLevel(u.Owner,0),OpponentLevel(u.Owner,2),u.Type):null;
    public int WorkerCountdown(Actor u)=>Math.Max(1,(CombatRule(u.Type).Reload-(WorkerTechnology(u)?.Reload??0))&255);
    int CargoCapacity(Actor u,EngineeringResearch? research=null)=>FoodData.Capacities[u.Type]+(WorkerTechnology(u,research)?.Carry??0);
}
