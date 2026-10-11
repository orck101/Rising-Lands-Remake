using System.Text.Json;
namespace RisingLands;
public sealed class NativeGateControl {public int Slot{get;set;}public int Frame{get;set;}=15;public bool Open{get;set;}}
public record GateVisual(int Orientation,int Frame,List<DrawCommand> Commands);
public record GateVisualData(List<GateVisual> Frames,List<GateVisual> Stages);
public static class GatePresentation {
 public static readonly GateVisualData Data=JsonSerializer.Deserialize<GateVisualData>(File.ReadAllText(PackageLayout.FromBase("Content/Data/gate_visuals_v72.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static List<DrawCommand> Commands(int orientation,int frame,bool building=false)=>(building?Data.Stages:Data.Frames).First(f=>f.Orientation==orientation&&f.Frame==frame).Commands;
}
public sealed partial class Simulation {
 public bool ReplacedWall(int slot)=>Constructions.Any(c=>c.Type==12&&c.ReplacesWall==slot&&Complete(c));
 public bool GateCell(int cell)=>SiteAt(cell)?.Type==12||BuildingAt(cell)?.Type==12;
 public bool GatePassable(int cell)=>SiteAt(cell) is {Type:12,GateOpenTarget:true,GateFrame:31} c&&Complete(c)||BuildingAt(cell) is {Type:12} b&&Special.NativeGates.Any(g=>g.Slot==b.Slot&&g.Open&&g.Frame==31);
 public int GateFrameAt(int cell)=>SiteAt(cell) is {Type:12} c?c.GateFrame:Special.NativeGates.FirstOrDefault(g=>g.Slot==BuildingAt(cell)?.Slot)?.Frame??15;
 bool GateNeighbour(int cell)=>ValidMapCell(cell)&&(BuildingAt(cell)?.Type is 9 or 12||SiteAt(cell)?.Type is 9 or 12);
 public int GateOrientationAt(int cell){int x=cell&255,y=cell>>8;
  if(x>0&&x+1<World.Width&&GateNeighbour(cell-1)&&GateNeighbour(cell+1))return 0;
  if(y>0&&y+1<World.Height&&GateNeighbour(cell-256)&&GateNeighbour(cell+256))return 1;
  return -1;
 }
 string? GatePlacementError(int cell){
  if(BuildingAt(cell) is not {Type:9,Owner:0} wall||SiteAt(cell) is {Type:12})return "Coloque o portao sobre uma muralha sua ja concluida.";
  if(GateOrientationAt(cell)<0)return "O portao precisa de muralha nos dois lados de um trecho reto.";
  if(World.Units.Any(u=>VisualCatalog.Visible(u)&&u.X/1024==(cell&255)&&u.Y/1024==(cell>>8)))return "Retire as unidades do trecho do portao.";
  var r=Rule(12);if(World.Resources[0]<r.Food||World.Resources[4]<r.Rock||World.Resources[8]<r.Parts)return "Recursos insuficientes.";
  return null;
 }
 public bool ToggleGate(int slot){var c=Constructions.FirstOrDefault(c=>c.Id==slot&&c.Type==12&&Complete(c));if(c==null){var b=InitialBuildings.FirstOrDefault(b=>b.Slot==slot&&b.Type==12&&b.Owner==0);if(b==null)return false;var g=Special.NativeGates.FirstOrDefault(g=>g.Slot==slot);if(g==null){g=new(){Slot=slot};Special.NativeGates.Add(g);}if(g.Frame is not (0 or 15 or 31)||g.Open&&World.Units.Any(u=>VisualCatalog.Visible(u)&&!Airborne(u)&&u.X/1024==(b.Cell&255)&&u.Y/1024==(b.Cell>>8)))return false;g.Open=!g.Open;g.Frame=g.Open?16:0;return true;}if(c.GateFrame is not (0 or 15 or 31))return false;
  bool open=!c.GateOpenTarget;if(!open&&World.Units.Any(u=>VisualCatalog.Visible(u)&&!Airborne(u)&&u.X/1024==(c.Cell&255)&&u.Y/1024==(c.Cell>>8)))return false;
  c.GateOpenTarget=open;c.GateFrame=open?16:0;return true;
 }
 void GateStep(){foreach(var c in Constructions.Where(c=>c.Type==12&&Complete(c)))if(c.GateOpenTarget&&c.GateFrame<31||!c.GateOpenTarget&&c.GateFrame<15)c.GateFrame++;foreach(var g in Special.NativeGates)if(g.Open&&g.Frame<31||!g.Open&&g.Frame<15)g.Frame++;}
 void ValidateGates(Save s){if(s.Special!=null&&s.Special.NativeGates==null)throw new InvalidDataException("Missing native gate controls");var native=s.Special?.NativeGates??new();if(native.Select(g=>g.Slot).Distinct().Count()!=native.Count||native.Any(g=>!InitialBuildings.Any(b=>b.Slot==g.Slot&&b.Type==12&&b.Owner==0)||g.Frame is <0 or >31||(g.Open?g.Frame<16:g.Frame>15)))throw new InvalidDataException("Invalid native gate control");var replacements=new HashSet<int>();foreach(var c in s.Constructions){
  if(c.Type!=12){if(c.ReplacesWall!=-1)throw new InvalidDataException("Unexpected wall replacement");continue;}
  var source=InitialBuildings.FirstOrDefault(b=>b.Slot==c.ReplacesWall)??s.Constructions.Where(d=>Complete(d)).Select(d=>new Building(d.Id,d.Type,0,d.Cell)).FirstOrDefault(b=>b.Slot==c.ReplacesWall);
  if(source is not {Type:9,Owner:0}||source.Cell!=c.Cell||c.ReplacesWall==c.Id||!replacements.Add(c.ReplacesWall)||c.GateOrientation is <0 or >1||c.GateFrame is <0 or >31||!Complete(c)&&(c.GateFrame!=15||c.GateOpenTarget)||Complete(c)&&(c.GateOpenTarget?c.GateFrame<16:c.GateFrame>15))throw new InvalidDataException("Invalid gate upgrade");
 }}
}
public sealed partial class ResearchGame {
 void DrawGates(List<(float depth,SceneObject? obj,Actor? actor)> renderables){
  var cells=simulation.World.Buildings.Where(b=>b.Type==12).Select(b=>b.Cell).Concat(simulation.Constructions.Where(c=>c.Type==12).Select(c=>c.Cell)).Distinct();
  foreach(int cell in cells.Where(cell=>revealMap||SeenCell(cell))){var site=simulation.Constructions.FirstOrDefault(c=>c.Type==12&&c.Cell==cell);bool unfinished=site!=null&&!simulation.Complete(site);int orientation=site?.GateOrientation??Math.Max(0,simulation.GateOrientationAt(cell));int frame=unfinished?Math.Clamp(site!.Progress*100/simulation.Rule(12).Duration,0,100):simulation.GateFrameAt(cell);
   var commands=GatePresentation.Commands(orientation,frame,unfinished).Select(c=>new DrawCommand(c.Kind,c.X+(cell&255)*32,c.Y+(cell>>8)*32,c.Sprite,c.Clip==null?null:new DrawClip(c.Clip.Left+(cell&255)*32,c.Clip.Top+(cell>>8)*32,c.Clip.Right+(cell&255)*32,c.Clip.Bottom+(cell>>8)*32))).ToList();
   renderables.Add(((cell>>8)*32+16,new SceneObject(cell,commands),null));
  }
 }
}
