namespace RisingLands;
public sealed partial class Simulation {
 // Visually calibrated foot points in the native completed building sprites.
 static readonly (int X,int Y)[] DoorFeet={(29,38),(105,74),(112,78),(29,70),(46,47),(130,100),(105,120),(104,65),(95,95),(16,24),(30,48),(30,50),(16,24),(65,66),(60,84),(73,105),(57,80),(16,24),(58,72),(39,55),(73,105),(38,53)};
 public (int X,int Y) DoorOrigin(Building b){var foot=DoorFeet[b.Type];return ((b.Cell&255)*1024+(foot.X-12)*32,(b.Cell>>8)*1024+(foot.Y-24+24)*32);}
 public IEnumerable<int> BuildingInteractionCells(Building b)=>FrontCells(b);
 // Old saves may still carry a perimeter destination; complete those existing orders safely.
 IEnumerable<int> ValidBuildingArrivalCells(Building b)=>FrontCells(b).Concat(WorkCells(b.Cell,Rule(b.Type)));
 (int X,int Y) BuildingArrivalPosition(Building b,int cell)=>FrontCells(b).Contains(cell)?ExitPosition(b,cell):((cell&255)*1024,(cell>>8)*1024);
 bool AtBuildingArrival(Actor actor,Building b,int cell){var p=BuildingArrivalPosition(b,cell);return actor.X==p.X&&actor.Y==p.Y;}
 bool ReachBuildingInteraction(Actor actor,Building b,int cell){
  if(actor.Route.Count>0||!ValidBuildingArrivalCells(b).Contains(cell)||!Passable(cell&255,cell>>8))return false;
  var p=BuildingArrivalPosition(b,cell);if(actor.X==p.X&&actor.Y==p.Y)return true;
  actor.Direction=5;actor.X+=Math.Clamp(p.X-actor.X,-LabStep,LabStep);actor.Y+=Math.Clamp(p.Y-actor.Y,-LabStep,LabStep);
  actor.AnimationCounter++;if(actor.AnimationCounter>=actor.AnimationDelay){actor.AnimationCounter=0;actor.AnimationFrame=(actor.AnimationFrame+1)%Math.Max(1,actor.AnimationCount);}
  return actor.X==p.X&&actor.Y==p.Y;
 }
 public IEnumerable<int> FrontCells(Building b){
  var door=DoorOrigin(b);int x=door.X/1024,y=door.Y/1024;
  foreach(int dx in new[]{0,-1,1})if(x+dx>=0&&x+dx<World.Width&&y>=0&&y<World.Height)yield return y*256+x+dx;
 }
 readonly bool[] doorPassages=new bool[65536];int doorCacheCount=-1;
 bool DoorCorridor(int cell){
  if(doorCacheCount!=World.Buildings.Count){Array.Clear(doorPassages);foreach(var b in World.Buildings.Where(b=>b.Type is not (9 or 12))){var d=DoorOrigin(b);var rule=Rule(b.Type);for(int y=Math.Max(b.Cell>>8,d.Y/1024);y<(b.Cell>>8)+rule.Height&&y<World.Height;y++)for(int x=Math.Max(b.Cell&255,d.X/1024-1);x<Math.Min((b.Cell&255)+rule.Width,d.X/1024+2)&&x<World.Width;x++)doorPassages[y*256+x]=true;}doorCacheCount=World.Buildings.Count;}
  return doorPassages[cell];
 }
 public float ActorRenderDepth(Actor actor){int cell=actor.Y/1024*256+actor.X/1024;var building=(DoorCorridor(cell)?BuildingAt(cell):null);return building==null?actor.Y/32f+16:Math.Max(actor.Y/32f+16,((building.Cell>>8)+Rule(building.Type).Height)*32+16);}
 public bool TraversableBuildingExit(int cell)=>Passable(cell&255,cell>>8);
 public int BuildingExit(Building b){var cells=FrontCells(b).Where(TraversableBuildingExit).ToArray();return cells.FirstOrDefault(c=>!World.Units.Any(a=>VisualCatalog.Visible(a)&&Math.Abs(a.X-ExitPosition(b,c).X)<768&&Math.Abs(a.Y-ExitPosition(b,c).Y)<768),cells.FirstOrDefault(-1));}
 public (int X,int Y) ExitPosition(Building b,int cell){var d=DoorOrigin(b);return ((cell&255)*1024+d.X%1024,(cell>>8)*1024+d.Y%1024);}
 public void PlaceAtExit(Actor actor,Building b,int cell){var d=ExitPosition(b,cell);actor.X=d.X;actor.Y=d.Y;}
 public int EnterBuildingGroup(IEnumerable<Actor> actors,int slot){int count=0;foreach(var actor in actors.DistinctBy(a=>a.Slot).ToArray())if(EnterBuilding(actor,slot))count++;return count;}
}
