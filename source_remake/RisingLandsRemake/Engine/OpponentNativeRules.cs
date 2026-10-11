using System.Text.Json;
namespace RisingLands;
public sealed record OpponentInitialTechnology(int Mission,int[][] Players);
public sealed record OpponentTechnologyCatalog(List<OpponentInitialTechnology> Missions);
public sealed record NativeCityTarget(int Type,int Count);
public sealed record NativeCityMode(int Mode,string Address,List<NativeCityTarget> Targets);
public sealed record NativeCityCase(int Mode,int[] Counts,int[] Allowed,int[] Requests);
public sealed record NativeCityCatalog(List<NativeCityMode> Modes,List<NativeCityCase> Cases);
public sealed partial class Simulation {
 public static readonly OpponentTechnologyCatalog OpponentTechnologies=JsonSerializer.Deserialize<OpponentTechnologyCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/opponent_technology.json")))!;
 public static readonly NativeCityCatalog NativeCities=JsonSerializer.Deserialize<NativeCityCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/opponent_city.json")))!;
 public int OpponentLevel(int owner,int branch)=>Math.Max(Math.Min(EnemyResearchPolicy.Maximum(branch),OpponentTechnologies.Missions.First(m=>m.Mission==MissionNumber).Players[owner][branch]+(owner==3?2:0)),Campaign.EnemyLevels[owner][branch]);
 public bool OpponentBuildingUnlocked(int owner,int type)=>owner is >0 and <4&&(InitialBuildings.Any(b=>b.Owner==owner&&b.Type==type)||UnlockData.Technologies.Any(t=>t.Index<OpponentLevel(owner,t.Branch)&&t.Buildings.Contains(type)));
 public bool OpponentUnitUnlocked(int owner,int type)=>owner is >0 and <4&&UnlockData.Technologies.Any(t=>t.Index<OpponentLevel(owner,t.Branch)&&ProductionUnlocks.GetValueOrDefault(t.Key,Array.Empty<int>()).Contains(type));
 public static int NativeCityDemand(int mode,int[] existing,int[] pending,Func<int,bool> available){foreach(int tier in Enumerable.Range(1,10))foreach(var t in NativeCities.Modes.First(m=>m.Mode==mode).Targets)if(t.Count>=tier&&existing[t.Type]+pending[t.Type]<tier&&available(t.Type))return t.Type;return -1;}
 int OpponentCityDemand(int owner){var counts=new int[22];var pending=new int[22];foreach(var b in World.Buildings.Where(b=>b.Owner==owner))counts[b.Type]++;foreach(var c in Constructions.Where(c=>c.Owner==owner&&!Complete(c)))pending[c.Type]++;
  // Native survival exit requires its warehouse and cultivation service flags.
  // Until full resource-group adapters exist, inhabited buildings supply this bridge.
  int mode=World.Buildings.Any(b=>b.Owner==owner&&b.Type==3&&Inhabited(b))&&World.Buildings.Any(b=>b.Owner==owner&&b.Type==4&&Inhabited(b))?1:0;
  return NativeCityDemand(mode,counts,pending,type=>type!=12&&OpponentBuildingUnlocked(owner,type)&&Rule(type).Objects.Count>0);
 }
}
