namespace RisingLands;
public sealed partial class Simulation {
 public const int WarehouseOverflowRadius=3;
 public IEnumerable<int> WarehouseOverflowCells(Building warehouse){var r=Rule(warehouse.Type);int x=warehouse.Cell&255,y=warehouse.Cell>>8;
  for(int ny=Math.Max(0,y-3);ny<=Math.Min(World.Height-1,y+r.Height-1+3);ny++)for(int nx=Math.Max(0,x-3);nx<=Math.Min(World.Width-1,x+r.Width-1+3);nx++)if(nx<x||nx>=x+r.Width||ny<y||ny>=y+r.Height)yield return ny*256+nx;
 }
 public bool GroundResourceSpace(int cell,int kind)=>cell>=0&&cell<65536&&(cell&255)<World.Width&&(cell>>8)<World.Height&&Passable(cell&255,cell>>8)&&BuildingAt(cell)==null&&MineAt(cell)<0&&(World.Objects[cell]==0||World.Objects[cell]==66+kind&&World.Quantities[cell]<31);
 bool RouteWarehouseOverflow(Actor worker,Building depot,bool mined){int kind=mined?2:worker.CargoKind;
  foreach(int cell in WarehouseOverflowCandidates(depot,kind,worker.Y/1024*256+worker.X/1024)){
   if(!Order(worker,cell&255,cell>>8,true))continue;worker.OverflowCell=cell;if(mined){worker.MineWorkCell=cell;worker.MineJob=3;}else{worker.DeliveryCell=cell;worker.FoodJob=3;}return true;
  }
  worker.OverflowCell=-1;if(mined)worker.MineJob=4;else worker.FoodJob=4;return false;
 }
 bool OverflowArrival(Actor worker,bool mined){int cell=worker.OverflowCell;int slot=mined?worker.RockDestination:worker.FoodDestination;var depot=World.Buildings.FirstOrDefault(b=>b.Slot==slot&&b.Owner==worker.Owner&&AcceptsFood(b));
  if(depot==null||!GroundResourceSpace(cell,mined?2:worker.CargoKind)||!WarehouseOverflowCells(depot).Contains(cell)){worker.OverflowCell=-1;if(mined)worker.MineJob=4;else worker.FoodJob=4;return true;}
  if(worker.X!=(cell&255)*1024||worker.Y!=(cell>>8)*1024)return true;
  int kind=mined?2:worker.CargoKind;int previous=World.Objects[cell]==66+kind?World.Quantities[cell]:0;int amount=Math.Min(mined?worker.Rock:worker.Food,31-previous);World.Objects[cell]=(byte)(66+kind);World.Quantities[cell]=(byte)(previous+amount);if(mined)worker.Rock-=amount;else worker.Food-=amount;
  if(amount>0&&!worker.GroundDrops.Contains(cell))worker.GroundDrops.Add(cell);
  // Fresh plant harvest remains production when the warehouse is full. Moving
  // existing piles or withdrawing stocks does not create economic income.
  if(amount>0&&!mined&&kind==1&&worker.Owner is >0 and <4&&worker.FoodPlantsOnly&&worker.FoodSourceBuilding<0)Opponent.OverflowFood[worker.Owner]+=amount;
  worker.OverflowCell=-1;if((mined?worker.Rock:worker.Food)>0){RouteWarehouseOverflow(worker,depot,mined);return true;}
  if(mined){worker.MineJob=0;if(worker.RepeatMine)ContinueMine(worker);}else{worker.FoodJob=0;ContinueHarvest(worker);}return true;
 }
 void ValidateOverflow(Save s){var buildings=InitialBuildings.Concat(s.Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).ToArray();foreach(var worker in s.Units){if(worker.OverflowCell< -1||worker.OverflowCell>=65536)throw new InvalidDataException("Invalid overflow cell");if(worker.OverflowCell<0)continue;bool mined=worker.MineJob==3&&worker.Rock>0;int slot=mined?worker.RockDestination:worker.FoodDestination;var depot=buildings.FirstOrDefault(b=>b.Slot==slot&&b.Owner==worker.Owner&&b.Type==3);
  if(depot==null||!WarehouseOverflowCells(depot).Contains(worker.OverflowCell)||!mined&&(worker.FoodJob!=3||worker.Food<=0||worker.FoodDestinationKind!=2)||(mined?worker.MineWorkCell:worker.DeliveryCell)!=worker.OverflowCell)throw new InvalidDataException("Invalid warehouse overflow order");
 }}
}
