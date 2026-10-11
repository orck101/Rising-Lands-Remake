using System.Text.Json;
namespace RisingLands;

public sealed record NativeGhostPlan(int Index,int Cell,uint ForbiddenTypes,int Offset) {
 public bool Allows(int type)=>Cell!=65535&&type is >=0 and <22&&(ForbiddenTypes&(1u<<type))==0;
}
public sealed record NativeOpponentMap(string MapName,string SourceHash,List<NativeGhostPlan> Plans);
public sealed record NativeOpponentCatalog(List<NativeOpponentMap> Maps);
public static class NativeOpponentPlans {
 static readonly Lazy<NativeOpponentCatalog> catalog=new(()=>JsonSerializer.Deserialize<NativeOpponentCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/opponent_plans.json")))??throw new InvalidDataException("Missing native opponent plans"));
 public static NativeOpponentMap? For(World world){var map=catalog.Value.Maps.FirstOrDefault(m=>m.MapName.Equals(world.MapName,StringComparison.OrdinalIgnoreCase));if(map!=null&&!map.SourceHash.Equals(world.SourceHash,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Opponent plans do not match original map");return map;}
 // Native GetNumIndiceTypeGhost: scan in file order; set bits forbid a type.
 public static int Find(NativeOpponentMap map,int start,int type)=>map.Plans.Where(p=>p.Index>=start&&p.Allows(type)).Select(p=>p.Index).FirstOrDefault(65535);
}
