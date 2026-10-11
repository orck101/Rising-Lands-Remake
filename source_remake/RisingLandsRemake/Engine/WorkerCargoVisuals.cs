namespace RisingLands;
public static class WorkerCargoVisuals {
 public static int Quantity(Actor a)=>a.Food+a.Rock;
 public static int Resource(Actor a)=>a.Rock>0?2:a.CargoKind;
 public static IReadOnlyList<DrawCommand> Commands(Actor a)=>a.Type is not (5 or 6 or 7 or 13)?Array.Empty<DrawCommand>():Enumerable.Range(0,Quantity(a)).Select(i=>new DrawCommand("sprite",0,10+i*8,0x2a3d)).ToArray();
}
public sealed partial class ResearchGame {
 void DrawWorkerCargo(Actor? hovered,bool all){foreach(var a in simulation.World.Units.Where(a=>VisualCatalog.Visible(a)&&SeenUnit(a)&&!VisualCatalog.HiddenByWork(a)&&UnitInfoRules.Show(a.Owner,selectedSlots.Contains(a.Slot),a==hovered,all)))foreach(var c in WorkerCargoVisuals.Commands(a))Command(c,a.X/32f,a.Y/32f);}
 void DrawCargoCounter(){var units=SelectedUnits().ToArray();if(ViewedBuilding!=null||units.Length!=1||units[0].Type is not (5 or 6 or 7 or 13))return;var a=units[0];NativeNumber(WorkerCargoVisuals.Quantity(a),390,557);}
 void PrepareV70Capture(){pauseMenu.Show(MenuPage.Closed);paused=true;revealMap=true;var a=simulation.World.Units.First(a=>a.Owner==0&&a.Type==5);simulation.Stop(a);a.Food=3;SelectOnly(a);Center();message="Carga do fazendeiro: 3 unidades (quadradinhos originais).";}
 void PrepareV70FogCapture(){
  var fixture=ImprovementsV67Tests.Fixture(root,false);int source=40+40*256,target=43+40*256;
  foreach(var b in fixture.World.Buildings.Where(b=>b.Slot!=900).ToArray()){fixture.World.Buildings.Remove(b);fixture.World.BuildingResidents.Remove(b.Slot);}foreach(var u in fixture.World.Units.Where(a=>a.Owner==0)){u.X=2*1024;u.Y=2*1024;}
  foreach(var u in fixture.World.Units.Where(a=>a.Type==9&&!fixture.World.BuildingResidents.Values.Any(ids=>ids.Contains(a.Slot))))u.LegacyState=7;fixture.World.Objects[source]=67;fixture.World.Quantities[source]=5;LoadMap(restored:new Simulation(fixture.World,fixture.Campaign));paused=true;revealMap=false;pauseMenu.Show(MenuPage.Closed);
  var farmer=simulation.World.Units.First(a=>a.Type==5&&a.Owner==0);SelectOnly(farmer);camera=new Microsoft.Xna.Framework.Vector2(40*32-ViewWidth/(2*zoom),40*32-ViewHeight/(2*zoom));ClampCamera();
  for(int y=38;y<=46;y++)for(int x=38;x<=46;x++){int c=y*256+x;simulation.Exploration.Explored[c]=1;simulation.Exploration.Tiles[c]=simulation.World.Tiles[c];}simulation.Exploration.Objects[source]=67;simulation.Exploration.Quantities[source]=5;
  Microsoft.Xna.Framework.Point PointFor(int cell){var p=ToScreen((cell&255)*32+16,(cell>>8)*32+16);return new((int)p.X,(int)p.Y);}
  if(simulation.InSight(source)||!HandleResourceClick(PointFor(source),true)||!resourceGesture.Pending||!HandleResourceClick(PointFor(target),true)||resourceGesture.Pending||farmer.FoodSource!=source||farmer.FoodDestination!=target)throw new InvalidDataException("Explored-fog two-click collection order failed");
  int unknown=50+50*256;simulation.Exploration.Explored[unknown]=0;string before=simulation.Serialize();if(!HandleResourceClick(PointFor(unknown),true)||resourceGesture.Pending||simulation.Serialize()!=before)throw new InvalidDataException("Unknown terrain accepted a resource interaction");
  IssuePointerOrder(PointFor(unknown));if(farmer.Route.Count==0||farmer.Route.Last()!=unknown||farmer.FoodJob!=0)throw new InvalidDataException("Unknown terrain did not become movement-only exploration");
  File.WriteAllText(Path.ChangeExtension(capture!,".json"),System.Text.Json.JsonSerializer.Serialize(new{passed=true,explored_fog_collection=true,unknown_resource_blocked=true,unknown_movement_allowed=true}));message="Area explorada aceita ordens; area desconhecida aceita apenas movimento.";
 }
}
