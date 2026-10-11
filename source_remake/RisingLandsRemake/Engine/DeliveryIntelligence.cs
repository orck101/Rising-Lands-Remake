namespace RisingLands;
public sealed partial class Simulation {
 public const int GroundDeliveryRadius=3;
 public static int CellDistance(int a,int b)=>Math.Max(Math.Abs((a&255)-(b&255)),Math.Abs((a>>8)-(b>>8)));
 public IEnumerable<int> GroundDeliveryCells(int anchor,int kind,int source=-1){
  int x=anchor&255,y=anchor>>8;
  var cells=new List<int>();for(int dy=-GroundDeliveryRadius;dy<=GroundDeliveryRadius;dy++)for(int dx=-GroundDeliveryRadius;dx<=GroundDeliveryRadius;dx++){
   int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=World.Width||ny>=World.Height)continue;int c=ny*256+nx;
   if(c!=source&&GroundResourceSpace(c,kind))cells.Add(c);
  }
  return cells.OrderBy(c=>CellDistance(c,anchor)).ThenBy(c=>World.Objects[c]==66+kind?0:1).ThenBy(c=>c);
 }
 public int WarehouseContourDistance(Building b,int cell){var r=Rule(b.Type);int x=cell&255,y=cell>>8,bx=b.Cell&255,by=b.Cell>>8;return Math.Max(Math.Max(bx-x,Math.Max(0,x-(bx+r.Width-1))),Math.Max(by-y,Math.Max(0,y-(by+r.Height-1))));}
 public IEnumerable<int> WarehouseOverflowCandidates(Building b,int kind,int origin)=>WarehouseOverflowCells(b).Where(c=>GroundResourceSpace(c,kind)).OrderBy(c=>WarehouseContourDistance(b,c)).ThenBy(c=>World.Objects[c]==66+kind?0:1).ThenBy(c=>Math.Abs((c&255)-(origin&255))+Math.Abs((c>>8)-(origin>>8))).ThenBy(c=>c);
 bool ContinueMine(Actor u){
  if(!u.RepeatMine)return false;if(StartMining(u,u.MineIndex,false))return true;
  if(u.MineIndex<0||u.MineIndex>=World.Mines.Count)return false;
  int anchor=World.Mines[u.MineIndex].Cell;u.CargoKind=2;
  foreach(int cell in Enumerable.Range(0,65536).Where(c=>CellDistance(c,anchor)<=FoodFieldRadius&&World.Objects[c]==68&&World.Quantities[c]>0&&c!=u.RockGround&&!u.GroundDrops.Contains(c)).OrderBy(c=>CellDistance(c,anchor)).ThenBy(c=>c)){
   var drops=u.GroundDrops.ToList();if(!Gather(u,cell,false))continue;u.GroundDrops=drops;
   u.FoodDestinationKind=u.RockDestination>=0?2:u.RockGround>=0?1:0;u.FoodDestination=u.RockDestination>=0?u.RockDestination:u.RockGround;u.RockDestination=u.RockGround=-1;u.RepeatFood=true;u.FoodField=anchor;return true;
  }
  u.MineJob=u.PersistentCollection?4:0;return false;
 }
}
