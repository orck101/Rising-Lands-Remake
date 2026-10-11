using System.Text.Json;
using Microsoft.Xna.Framework;
namespace RisingLands;
public record ResidentVisual(int Type,int Capacity,int Occupied,List<DrawCommand> Commands);
public record ResidentAnchor(int X,int Y);
public sealed record ResidentVisualData(Dictionary<string,ResidentAnchor> Anchors,List<ResidentVisual> Rows);
public static class BuildingResidentVisuals {
 static readonly ResidentVisualData Data=JsonSerializer.Deserialize<ResidentVisualData>(File.ReadAllText(PackageLayout.FromBase("Content/Data/building_resident_visuals.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static IReadOnlyList<DrawCommand> Commands(Building building,int occupants){
  var row=Data.Rows.FirstOrDefault(r=>r.Type==building.Type&&r.Occupied==0);
  if(row==null||!Data.Anchors.TryGetValue(building.Type.ToString(),out var anchor))return Array.Empty<DrawCommand>();
  int x=(building.Cell&255)*32+anchor.X,y=(building.Cell>>8)*32+anchor.Y;
  return row.Commands.Select((c,i)=>c with{X=c.X+x,Y=c.Y+y,Sprite=i<occupants?0x2a3c:0x2a3b,Kind=i<occupants?"player":"sprite"}).ToArray();
 }
}
public sealed partial class ResearchGame {
 void PrepareResidentCapture(){
  bool hoverOnly=captureMenu=="residents-hover";int occupied=hoverOnly?1:int.Parse(captureMenu![10..]);var home=simulation.World.Buildings.First(b=>b.Type==2&&b.Owner==0);int slot=ResearchTests.Build(simulation,3,false);var depot=simulation.World.Buildings.First(b=>b.Slot==slot);
  for(int i=0;i<occupied;i++){var settler=simulation.ReleaseResident(home.Slot)!;if(!simulation.EnterBuilding(settler,slot))throw new InvalidDataException("Resident capture entry");for(int j=0;j<10000&&simulation.ResidentTrips.Count>0;j++)simulation.Step();}
  viewedBuilding=slot;selected=null;selectedSlots.Clear();camera=new Vector2((depot.Cell&255)*32-ViewWidth/(2*zoom),(depot.Cell>>8)*32-ViewHeight/(2*zoom));ClampCamera();message="";if(hoverOnly){var p=ToScreen((depot.Cell&255)*32+16,(depot.Cell>>8)*32+16);interactionCapturePointer=new Point((int)p.X,(int)p.Y);viewedBuilding=-1;}
 }
 void DrawBuildingResidents(Point pointer){
  if(capture!=null&&interactionCapturePointer is {} capturePointer)pointer=capturePointer;
  Building? hover=(IsActive||capture!=null&&interactionCapturePointer!=null)&&View.Contains(pointer)?BuildingHit(pointer):null;
  foreach(var b in simulation.World.Buildings.Where(b=>b.Owner==0&&(b.Slot==viewedBuilding||b.Slot==hover?.Slot)))
   foreach(var command in b.Type==4?CropPresentation.Commands(b,simulation.Occupants(b.Slot),simulation.CropFood(b.Slot),simulation.AgricultureResearch.Index):BuildingResidentVisuals.Commands(b,simulation.Occupants(b.Slot)))Command(command);
 }
}
