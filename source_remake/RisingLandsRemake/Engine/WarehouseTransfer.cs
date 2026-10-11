using System.Text.Json;
namespace RisingLands;
public sealed partial class Simulation {
 public static int WorkerResourceKind(Actor u)=>u.Type==7?2:u.Type==6?0:1;
 public bool WarehouseSourceAvailable(Actor u,Building b)=>b.Owner==u.Owner&&b.Type==3&&Inhabited(b)&&u.Owner is >=0 and <=3&&OriginalWorkerPolicy.Carrier(u.Type)&&World.Resources[ResourceIndex(WorkerResourceKind(u))+u.Owner]>0;
 public bool GatherWarehouse(Actor u,int slot,bool respond=true){
  var b=World.Buildings.FirstOrDefault(b=>b.Slot==slot);if(b==null||!WarehouseSourceAvailable(u,b)||u.Food+u.Rock>=Capacity(u))return false;
  foreach(int cell in BuildingInteractionCells(b).OrderBy(c=>Math.Abs((c&255)-u.X/1024)+Math.Abs((c>>8)-u.Y/1024))){if(!Order(u,cell&255,cell>>8,true))continue;
   CancelFeeding(u);DetachBuilder(u.Slot);if(respond)EmitUnitSound(u,0xf,4);
   u.OverflowCell=-1;u.FoodPlantsOnly=false;u.CargoKind=WorkerResourceKind(u);u.MineJob=0;u.FoodField=-1;u.FoodSourceBuilding=slot;u.FoodSource=cell;u.FoodJob=1;u.WorkTicks=0;return true;
  }return false;
 }
 void WarehousePickupStep(Actor u){
  if(u.FoodJob==4){if(u.Food>0){if(Tick%30==0)Deliver(u);return;}if(Tick%30==0)GatherWarehouse(u,u.FoodSourceBuilding,false);return;}
  var source=World.Buildings.FirstOrDefault(b=>b.Slot==u.FoodSourceBuilding);if(source==null){u.FoodJob=4;return;}if(!ReachBuildingInteraction(u,source,u.FoodSource))return;
  if(u.FoodJob==1){u.FoodJob=2;u.WorkTicks=10;return;}if(--u.WorkTicks>0)return;
  var b=World.Buildings.FirstOrDefault(b=>b.Slot==u.FoodSourceBuilding);
  int index=ResourceIndex(u.CargoKind)+u.Owner;if(b==null||!WarehouseSourceAvailable(u,b)){u.FoodJob=4;u.WorkTicks=0;return;}
  int amount=Math.Min(Capacity(u)-u.Food,(int)World.Resources[index]);World.Resources[index]-=(ushort)amount;u.Food+=amount;u.WorkTicks=0;Deliver(u);
 }
 public Simulation OrderPreview(){
  var w=new World{MapName=World.MapName,SourceHash=World.SourceHash,Width=World.Width,Height=World.Height,Tiles=(ushort[])InitialTiles.Clone(),Collision=(byte[])World.Collision.Clone(),Objects=(byte[])InitialObjects.Clone(),Quantities=(byte[])InitialQuantities.Clone(),Resources=(ushort[])InitialResources.Clone(),Units=CloneState(OriginalActorSeeds.Values.ToList()),Buildings=InitialBuildings.ToList(),Mines=InitialMines.Select(m=>new Mine{Cell=m.Cell,Quantity=m.Quantity}).ToList(),BuildingResidents=CloneState(OriginalResidents)};
  var copy=new Simulation(w,Campaign);copy.Restore(Serialize());return copy;
 }
}
public sealed class ResourceGesture {
 public int SourceCell{get;private set;}=-1;int[] slots=Array.Empty<int>();public bool Pending=>SourceCell>=0;
 public void Cancel(){SourceCell=-1;slots=Array.Empty<int>();}
 public bool Begin(Simulation s,IReadOnlyList<Actor> workers,int cell){if(!WorkerResourceOrders.GroupSourceAvailable(s,workers,cell))return false;SourceCell=cell;slots=workers.Select(u=>u.Slot).ToArray();return true;}
 bool Apply(Simulation s,int destination){var orders=new WorkerResourceOrders();foreach(int slot in slots){var u=s.World.Units.First(a=>a.Slot==slot);int food=u.Food,rock=u.Rock,kind=u.CargoKind;u.Food=0;u.Rock=0;bool source=orders.Source(s,u,SourceCell);u.Food=food;u.Rock=rock;if(food+rock>0)u.CargoKind=kind;if(!source||!orders.Destination(s,u,destination))return false;}return true;}
 public bool RedirectAvailable(Simulation s,IReadOnlyList<Actor> workers,int cell)=>workers.Count>0&&workers.All(u=>u.Owner==0&&OriginalWorkerPolicy.Carrier(u.Type)&&VisualCatalog.Visible(u)&&u.Food+u.Rock>0&&new WorkerResourceOrders().DestinationAvailable(s,u,cell));
 public bool Redirect(Simulation s,IReadOnlyList<Actor> workers,int cell){if(!RedirectAvailable(s,workers,cell))return false;var copy=s.OrderPreview();foreach(var u in workers)if(!new WorkerResourceOrders().Destination(copy,copy.World.Units.First(a=>a.Slot==u.Slot),cell))return false;foreach(var u in workers){if(!new WorkerResourceOrders().Destination(s,u,cell))throw new InvalidOperationException("Redirect changed after validation");}Cancel();return true;}
 public bool Commit(Simulation s,int cell){if(!Pending||slots.Any(slot=>!DestinationAvailable(s,s.World.Units.First(a=>a.Slot==slot),cell)))return false;var copy=s.OrderPreview();if(!Apply(copy,cell))return false;if(!Apply(s,cell))throw new InvalidOperationException("Order changed after validation");Cancel();return true;}
 public bool DestinationAvailable(Simulation s,Actor u,int cell){if(!Pending||cell<0||cell>=65536)return false;var b=s.BuildingAt(cell);if(b!=null)return b.Owner==0&&b!=s.BuildingAt(SourceCell)&&(s.AcceptsFood(b)||b.Type==4&&u.Type==5);return cell!=SourceCell&&s.Passable(cell&255,cell>>8)&&(s.World.Objects[cell]==0||s.World.Objects[cell]==66+Simulation.WorkerResourceKind(u)&&s.World.Quantities[cell]<31);}
}
