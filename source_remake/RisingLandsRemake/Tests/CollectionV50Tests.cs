namespace RisingLands;
public static class CollectionV50Tests {
 static void Check(bool value,string name){if(!value)throw new Exception("FAIL: v50 "+name);Console.WriteLine("PASS: v50 "+name);}
 static void Until(Simulation s,Func<bool> done){for(int i=0;i<60000&&!done();i++)s.Step();Check(done(),"collection reaches expected state");}
 public static void Run(string root){
  Simulation Create(int kind,bool full=false){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);
   w.Objects[5642]=(byte)(66+kind);w.Quantities[5642]=200;
   for(int i=0;i<65536;i++)if(i!=5642&&Math.Abs((i&255)-10)+Math.Abs((i>>8)-22)<=6&&w.Objects[i] is 67 or >=29 and <=52){w.Objects[i]=0;w.Quantities[i]=0;}
   if(kind==0)w.Units.First(u=>u.Type==5&&u.Owner==0).Type=6;
   var s=new Simulation(w);ResearchTests.Build(s,3);if(full){s.World.Resources[Simulation.ResourceIndex(kind)]+=100;s.World.Quantities[5642]-=100;}return s;
  }
  foreach(int kind in new[]{1,2,0}){
   var s=Create(kind);var u=s.World.Units.First(a=>a.Owner==0&&a.Type==(kind==1?5:kind==2?7:6));var b=s.World.Buildings.First(a=>a.Type==3&&a.Owner==0);var input=new WorkerResourceOrders();
   // Move real fixture stock into the warehouse, keeping resource conservation.
   int index=Simulation.ResourceIndex(kind);s.World.Resources[index]+=5;s.World.Quantities[5642]-=5;
   int drop=Enumerable.Range(0,65536).Where(c=>s.GroundResourceSpace(c,kind)&&s.World.Objects[c]==0).OrderBy(c=>Math.Abs((c&255)-(b.Cell&255))+Math.Abs((c>>8)-(b.Cell>>8))).First();
   int stock=s.World.Resources[index];u.RepeatFood=u.RepeatMine=false;
   Check(input.Source(s,u,b.Cell),$"resource {kind}: warehouse source accessible (inhabited={s.Inhabited(b)}, available={s.WarehouseSourceAvailable(u,b)}, visible={VisualCatalog.Visible(u)}, state={u.LegacyState}, position={u.X/1024},{u.Y/1024})");
   Check(input.Destination(s,u,drop),$"resource {kind}: ground destination accessible at {drop}");
   Check(u.RepeatFood&&u.PersistentCollection,$"resource {kind}: fresh order reactivates repetition");
   Until(s,()=>s.World.Objects[drop]==66+kind&&s.World.Quantities[drop]>=3);
   Check(s.World.Resources[index]<=stock-3,$"resource {kind}: repeated withdrawal debits stock on pickup");
   var copy=s.OrderPreview();for(int i=0;i<200;i++){s.Step();copy.Step();}Check(s.Serialize()==copy.Serialize(),$"resource {kind}: repeated withdrawal survives save/load");
   Until(s,()=>s.World.Resources[index]==0&&u.Food==0&&u.FoodJob==4);
   int quantity=s.World.Quantities[drop];for(int i=0;i<90;i++)s.Step();Check(u.FoodJob==4&&s.World.Quantities[drop]==quantity,$"resource {kind}: empty warehouse preserves waiting order");
   s.World.Resources[index]+=2;s.World.Quantities[5642]-=2;
   Until(s,()=>s.World.Quantities[drop]==quantity+2&&u.Food==0);
   Check(s.World.Resources[index]==0,$"resource {kind}: replenishment resumes withdrawal without another command");
   s.Stop(u);int stopped=s.World.Resources[index],cargo=u.Food+u.Rock;for(int i=0;i<200;i++)s.Step();Check(s.World.Resources[index]==stopped&&u.Food+u.Rock==cargo&&u.FoodJob==0,$"resource {kind}: stop preserves load and cancels repetition");
  }
  var full=Create(2,true);var depot=full.World.Buildings.First(b=>b.Type==3&&b.Owner==0);var miner=full.World.Units.First(u=>u.Type==7&&u.Owner==0);var orders=new WorkerResourceOrders();
  var ring=full.WarehouseOverflowCells(depot).ToArray();var rule=full.Rule(depot.Type);int x=depot.Cell&255,y=depot.Cell>>8;
  Check(ring.Contains((y-3)*256+x-3)&&ring.Contains((y+rule.Height+2)*256+x+rule.Width+2)&&!ring.Contains(depot.Cell)&&!ring.Contains((y-4)*256+x),"overflow covers all sides and diagonal corners within three tiles");
  int stockBefore=full.World.Resources[4];Check(orders.Source(full,miner,5642)&&orders.Destination(full,miner,depot.Cell),"full warehouse accepts persistent stone collection order");
  Until(full,()=>miner.OverflowCell>=0);Check(full.World.Resources[4]==stockBefore&&miner.Food>0,"overflow does not credit warehouse before ground delivery");
  var moving=full.OrderPreview();for(int i=0;i<100;i++){full.Step();moving.Step();}Check(full.Serialize()==moving.Serialize(),"overflow route survives save/load");
  Until(full,()=>ring.Any(c=>full.World.Objects[c]==68&&full.World.Quantities[c]>=3));Check(miner.FoodDestination==depot.Slot&&full.World.Resources[4]==stockBefore,"multiple overflow trips preserve original warehouse destination and stock");
  // Block the complete surrounding area with non-resource scenery.
  var blocked=Create(2,true);var builder=blocked.World.Units.First(u=>u.Type==7&&u.Owner==0);var store=blocked.World.Buildings.First(b=>b.Type==3&&b.Owner==0);var cells=blocked.WarehouseOverflowCells(store).ToArray();
  // Small foliage occupies resource slots but remains walkable; trees now block navigation.
  foreach(int c in cells){blocked.World.Objects[c]=12;blocked.World.Quantities[c]=0;}
  Check(orders.Source(blocked,builder,5642)&&orders.Destination(blocked,builder,store.Cell),"blocked warehouse still accepts collection order");Until(blocked,()=>builder.Food>0&&builder.FoodJob==4);
  int load=builder.Food;for(int i=0;i<100;i++)blocked.Step();Check(builder.Food==load&&builder.FoodDestination==store.Slot,"fully blocked perimeter waits with cargo and destination preserved");
  int free=cells.Where(c=>blocked.Passable(c&255,c>>8)&&blocked.BuildingAt(c)==null&&blocked.MineAt(c)<0).OrderBy(c=>Math.Abs((c&255)-builder.X/1024)+Math.Abs((c>>8)-builder.Y/1024)).First();blocked.World.Objects[free]=0;
  Until(blocked,()=>blocked.World.Objects[free]==68);Check(blocked.World.Quantities[free]>0,"waiting collector resumes when a surrounding point becomes available");
  var partial=Create(2,true);var carrier=partial.World.Units.First(u=>u.Type==7&&u.Owner==0);var warehouse=partial.World.Buildings.First(b=>b.Type==3&&b.Owner==0);
  int pile=partial.WarehouseOverflowCandidates(warehouse,2,5642).First();
  partial.World.Objects[pile]=68;partial.World.Quantities[pile]=30;partial.World.Quantities[5642]-=30;
  Check(orders.Source(partial,carrier,5642)&&orders.Destination(partial,carrier,warehouse.Cell),"existing partial pile accepts warehouse overflow order");
  Until(partial,()=>partial.World.Quantities[pile]==31);
  Until(partial,()=>partial.WarehouseOverflowCells(warehouse).Any(c=>c!=pile&&partial.World.Objects[c]==68&&partial.World.Quantities[c]>0));
  Check(partial.World.Quantities[pile]==31,"partial pile is reused and capped before another surrounding point is used");
  _=partial.OrderPreview();Check(true,"partial overflow conserves resources in a validated save");
  var native=MissionOneTests.Load(root);var loaded=native.World.Units.First(u=>u.Type==7&&u.Owner==0);native.StartMining(loaded,0);Until(native,()=>loaded.Rock>0);native.Stop(loaded);
  _=native.OrderPreview();Check(loaded.Rock>0&&!loaded.RepeatMine,"stopping a loaded miner preserves valid cargo without restarting mining");
 }
}
