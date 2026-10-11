using System.Text.Json;
namespace RisingLands;
public record ResourcePermission(int Kind,int[] Types);
public record DepositPermission(int Building,int Unit,bool Accepted);
public record WorkerPolicyData(List<ResourcePermission> Resources,List<DepositPermission> Deposits);
public static class OriginalWorkerPolicy {
    public static readonly WorkerPolicyData Data=JsonSerializer.Deserialize<WorkerPolicyData>(File.ReadAllText(PackageLayout.FromBase("Content/Data/worker_policy.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
    public static bool Carrier(int type)=>Data.Resources.Any(r=>r.Types.Contains(type));
    public static bool Collects(int type,int kind)=>Data.Resources.Any(r=>r.Kind==kind&&r.Types.Contains(type));
    public static bool Deposits(int type,int building)=>Data.Deposits.Any(r=>r.Unit==type&&r.Building==building&&r.Accepted);
}
