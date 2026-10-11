namespace RisingLands;
public sealed partial class Simulation {
 readonly Building?[] cellBuildings=new Building?[65536];readonly Construction?[] cellSites=new Construction?[65536];
 int indexedBuildings=-1,indexedSites=-1;
 static readonly Dictionary<int,BuildingRule> BuildRuleLookup=BuildData.Buildings.ToDictionary(r=>r.Type);
 static readonly Dictionary<int,HashSet<int>> BuildCollisionLookup=BuildData.Buildings.ToDictionary(r=>r.Type,r=>r.Collision.ToHashSet());
 void EnsureSpatialIndex(){
  if(indexedBuildings!=World.Buildings.Count){Array.Clear(cellBuildings);foreach(var b in World.Buildings){var r=Rule(b.Type);for(int y=0;y<r.Height;y++)for(int x=0;x<r.Width;x++){int cell=b.Cell+y*256+x;if(cell>=0&&cell<65536)cellBuildings[cell]??=b;}}indexedBuildings=World.Buildings.Count;}
  if(indexedSites!=Constructions.Count){Array.Clear(cellSites);foreach(var c in Constructions.Where(c=>!DestroyedBuilding(c.Id)).OrderByDescending(c=>c.Type==12).Where(c=>!ReplacedWall(c.Id))){var r=Rule(c.Type);for(int y=0;y<r.Height;y++)for(int x=0;x<r.Width;x++){int cell=c.Cell+y*256+x;if(cell>=0&&cell<65536)cellSites[cell]??=c;}}indexedSites=Constructions.Count;}
 }
 void InvalidateSpatialIndex(){indexedBuildings=indexedSites=-1;}
 Construction? SiteAt(int cell){EnsureSpatialIndex();return cell>=0&&cell<65536?cellSites[cell]:null;}
}
