namespace RisingLands;
public static class CollectionFlowTests {
 public static void Run(string root){
  int failures=0;void Check(bool ok,string text){Console.WriteLine((ok?"PASS: ":"FAIL: ")+text);if(!ok)failures++;}
  Simulation Load(){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);return new(w);}
  var s=Load();var u=s.World.Units.First(a=>a.Type==5&&a.Owner==0);u.RepeatFood=false;var orders=new WorkerResourceOrders();orders.Source(s,u,5642);
  for(int i=0;i<10000&&u.Food==0;i++)s.Step();
  var free=Enumerable.Range(0,65536).Where(c=>s.World.Objects[c]==0&&s.Passable(c&255,c>>8)&&s.BuildingAt(c)==null).OrderBy(c=>Math.Abs((c&255)-10)+Math.Abs((c>>8)-22)).Take(20).ToArray();
  Check(orders.Destination(s,u,free[0]),"loaded farmer accepts first destination");
  Check(orders.Destination(s,u,free[1])&&u.FoodDestination==free[1],"loaded farmer can redirect delivery with a second click");
  var group=Load();var farmers=group.World.Units.Where(a=>a.Type==5&&a.Owner==0).ToArray();
  var cells=Enumerable.Range(0,65536).Where(c=>group.World.Objects[c]==0&&group.Passable(c&255,c>>8)&&group.BuildingAt(c)==null&&Math.Abs((c&255)-10)+Math.Abs((c>>8)-22)<=4).Take(12).ToArray();
  foreach(int c in cells)group.World.Objects[c]=29;
  var go=new WorkerResourceOrders();Check(go.SourceGroup(group,farmers,cells[0])==farmers.Length,"group accepts chosen crop field");
  Check(farmers.Select(a=>a.FoodSource).Distinct().Count()==farmers.Length,"farmers spread across available plants rather than queueing on one");
  Check(farmers.All(a=>a.FoodField==cells[0]),"group retains player chosen field anchor");
  var large=Load();foreach(int c in cells)large.World.Objects[c]=29;var carrier=large.World.Units.First(a=>a.Type==5&&a.Owner==0);carrier.Type=13;
  Check(large.Capacity(carrier)>1&&large.Gather(carrier,cells[0]),"multi-item food carrier accepts a crop field");
  int first=carrier.FoodSource;for(int i=0;i<10000&&carrier.Food==0;i++)large.Step();
  Check(carrier.Food==1&&carrier.FoodJob==1&&carrier.FoodSource!=first,"partial load continues to nearby crop before returning");
  var nav=Load();var mover=nav.World.Units.First(a=>a.Type==5&&a.Owner==0);int destination=free.First(c=>nav.Order(mover,c&255,c>>8));var expected=mover.Route.ToArray();
  long allocatedBefore=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<10;i++)nav.Order(mover,destination&255,destination>>8);long allocated=GC.GetAllocatedBytesForCurrentThread()-allocatedBefore;
  Check(allocated<500000,"repeated short paths allocate less than 500 KB total: "+allocated+" bytes");
  Check(mover.Route.SequenceEqual(expected),"reused path buffers preserve deterministic route order");
  if(failures>0)throw new Exception(failures+" collection flow regressions");
 }
}
