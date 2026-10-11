using System.Text.Json;
namespace RisingLands;
public record VisibilityRadius(int Type,int Radius);
public record VisibilityCatalog(List<VisibilityRadius> Units,List<VisibilityRadius> Buildings,int UnitDiscoveryExtra,int BuildingDiscoveryExtra);
public sealed class ExplorationState {
 public Dictionary<int,StructureMemory> Structures{get;set;}=new();
 public bool[] ClansFound{get;set;}=new bool[4];
 public byte[] Explored{get;set;}=new byte[65536];
 public ushort[] Tiles{get;set;}=new ushort[65536];
 public byte[] Objects{get;set;}=new byte[65536];
 public byte[] Quantities{get;set;}=new byte[65536];
}
public sealed partial class Simulation {
 public static readonly VisibilityCatalog VisibilityData=JsonSerializer.Deserialize<VisibilityCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/visibility_native.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true,PropertyNamingPolicy=JsonNamingPolicy.SnakeCaseLower})!;
 public ExplorationState Exploration{get;private set;}=new();
 readonly byte[] currentVision=new byte[65536];
 readonly Dictionary<(int,bool),(int X,int Y)[]> visionDisks=new();
 public bool Discovered(int cell)=>ValidMapCell(cell)&&Exploration.Explored[cell]!=0;
 public bool CanInteractAt(int cell,bool reveal=false)=>ValidMapCell(cell)&&(reveal||Discovered(cell));
 public bool CanPlaceInExploredArea(int type,int cell,bool reveal=false){var r=Rule(type);return Enumerable.Range(0,r.Height).All(y=>Enumerable.Range(0,r.Width).All(x=>CanInteractAt(cell+y*256+x,reveal)));}
 public bool InSight(int cell)=>ValidMapCell(cell)&&currentVision[cell]!=0;
 bool ValidMapCell(int cell)=>cell>=0&&cell<65536&&(cell&255)<World.Width&&(cell>>8)<World.Height;
 public bool UnitInSight(Actor u)=>u.Owner==0||InSight(u.Y/1024*256+u.X/1024);
 public bool BuildingInSight(Building b)=>b.Owner==0||Enumerable.Range(0,Rule(b.Type).Height).Any(y=>Enumerable.Range(0,Rule(b.Type).Width).Any(x=>InSight(b.Cell+y*256+x)));
 (int X,int Y)[] VisionDisk(int radius,bool discovery){
  if(visionDisks.TryGetValue((radius,discovery),out var disk))return disk;
  int r=radius+(discovery?1:0),limit=r*r+(r>=2?1:0);var cells=new List<(int,int)>();
  for(int y=-r;y<=r;y++)for(int x=-r;x<=r;x++)if(x*x+y*y<=limit)cells.Add((x,y));
  return visionDisks[(radius,discovery)]=cells.ToArray();
 }
 void StampVision(int x,int y,int radius,int discoveryExtra,bool remember){
  if(remember)foreach(var d in VisionDisk(radius+discoveryExtra,true)){int xx=x+d.X,yy=y+d.Y;if(xx>=0&&yy>=0&&xx<World.Width&&yy<World.Height){int cell=yy*256+xx;if(Exploration.Explored[cell]==0)Exploration.Tiles[cell]=World.Tiles[cell];Exploration.Explored[cell]=1;}}
  foreach(var d in VisionDisk(radius,false)){int xx=x+d.X,yy=y+d.Y;if(xx>=0&&yy>=0&&xx<World.Width&&yy<World.Height)currentVision[yy*256+xx]=1;}
 }
 public void UpdateVisibility(bool remember=true){
  InvalidateSharedVision();Array.Clear(currentVision);
  foreach(var u in World.Units.Where(u=>SharesVision(0,u.Owner)&&u.Health>0&&VisualCatalog.Visible(u))){int radius=VisibilityData.Units.FirstOrDefault(r=>r.Type==u.Type)?.Radius??5;StampVision(u.X/1024,u.Y/1024,radius,VisibilityData.UnitDiscoveryExtra,remember);}
  foreach(var b in World.Buildings.Where(b=>SharesVision(0,b.Owner))){var rule=Rule(b.Type);int radius=VisibilityData.Buildings.FirstOrDefault(r=>r.Type==b.Type)?.Radius??0;StampVision((b.Cell&255)+rule.Width/2,(b.Cell>>8)+rule.Height/2,radius,VisibilityData.BuildingDiscoveryExtra,remember);}
  foreach(var z in Special.Zones.Where(z=>z.Spell==0&&SharesVision(0,z.Owner)&&z.Until>Tick))StampVision(z.Cell&255,z.Cell>>8,Spell(0).Radius,0,remember);
  // Store last observed terrain/resources; hidden changes must not leak through fog.
  if(remember){RememberStructures(Exploration.Structures,0,currentVision);DiscoverFifthClans();}
  if(remember)for(int i=0;i<65536;i++)if(currentVision[i]!=0){Exploration.Tiles[i]=World.Tiles[i];Exploration.Objects[i]=World.Objects[i];Exploration.Quantities[i]=World.Quantities[i];}
 }
 void ValidateExploration(Save save){
  if(save.Version<24&&save.Exploration==null)return;var e=save.Exploration??throw new InvalidDataException("Missing exploration");
  if(e.ClansFound==null||e.ClansFound.Length!=4||e.ClansFound[0])throw new InvalidDataException("Invalid discovered clans");
  if(e.Explored==null||e.Tiles==null||e.Objects==null||e.Quantities==null||e.Explored.Length!=65536||e.Tiles.Length!=65536||e.Objects.Length!=65536||e.Quantities.Length!=65536||e.Explored.Any(b=>b>1))throw new InvalidDataException("Invalid exploration planes");
  for(int i=0;i<65536;i++)if(!ValidMapCell(i)&&e.Explored[i]!=0||e.Explored[i]==0&&(e.Tiles[i]!=0||e.Objects[i]!=0||e.Quantities[i]!=0))throw new InvalidDataException("Invalid exploration cell");
  ValidateStructureMemories(e.Structures,save);MigrateClanDiscovery(save,e);

 }
 void MigrateClanDiscovery(Save save,ExplorationState e){
  if(IsFifthMission&&save.Version<35)foreach(var b in InitialBuildings.Where(b=>b.Owner is >0 and <4))if(Enumerable.Range(0,Rule(b.Type).Height).Any(y=>Enumerable.Range(0,Rule(b.Type).Width).Any(x=>e.Explored[b.Cell+y*256+x]!=0)))e.ClansFound[b.Owner]=true;
 }
 void RestoreExploration(Save save){Exploration=save.Exploration??new();MigrateClanDiscovery(save,Exploration);InitializeTerrainKnowledge();InvalidateSharedVision();UpdateVisibility(save.Exploration==null||save.Version<41);}
}
