using Microsoft.Xna.Framework;
namespace RisingLands;
public static class ImprovementV46Tests {
 static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
 public static void Run(string root){
  var s=MissionOneTests.Load(root);var miner=s.World.Units.First(a=>a.Type==7&&a.Owner==0);miner.RepeatMine=false;
  Check(s.StartMining(miner,0),"mine door reachable from first mission");
  bool visibleApproach=false;for(int i=0;i<10000&&miner.MineJob!=2;i++){s.Step();if(miner.Route.Count==0&&miner.MineJob==1)visibleApproach=true;}
  var mine=s.World.Mines[0];Check(visibleApproach&&miner.MineJob==2&&miner.X==(mine.Cell&255)*1024+17*32&&miner.Y==(mine.Cell>>8)*1024+9*32,"builder reaches visible mine doorway before disappearing");
  var copy=MissionOneTests.Load(root);copy.Restore(s.Serialize());for(int i=0;i<120;i++){s.Step();copy.Step();}Check(s.Serialize()==copy.Serialize()&&miner.Rock==1,"doorway extraction survives save/load and emerges carrying stone");
  var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);
  for(int i=0;i<65536;i++)if(i!=5642&&Math.Abs((i&255)-10)+Math.Abs((i>>8)-22)<=6&&w.Objects[i] is 67 or >=29 and <=52){w.Objects[i]=0;w.Quantities[i]=0;}
  w.Quantities[5642]=1;var probe=new Simulation(w);int next=new[]{5643,5898,5386,5641}.First(c=>probe.Passable(c&255,c>>8)&&probe.BuildingAt(c)==null);w.Objects[next]=67;w.Quantities[next]=2;
  var food=new Simulation(w);var farmer=w.Units.First(a=>a.Type==5&&a.Owner==0);int drop=Enumerable.Range(0,65536).Where(c=>w.Objects[c]==0&&food.Passable(c&255,c>>8)&&food.BuildingAt(c)==null).OrderBy(c=>Math.Abs((c&255)-10)+Math.Abs((c>>8)-22)).First();
  int stock=w.Resources[0];Check(food.SetFoodDestination(farmer,1,drop)&&food.Gather(farmer,5642),"food source and player destination accepted");
  bool found=false;for(int i=0;i<20000&&farmer.FoodJob!=0;i++){food.Step();found|=farmer.FoodSource==next;}
  Check(found&&w.Objects[5642]==0&&w.Objects[next]==0&&w.Quantities[drop]==3&&w.Resources[0]==stock&&farmer.Food==0,"depleted pile redirects to neighboring food and preserves ground destination without stock credit");
  string saved=food.Serialize();food.Restore(saved);Check(food.Serialize()==saved,"food pile search anchor remains valid in saved game");
  var area=new Rectangle(0,0,1280,712);Check(EdgeCamera.Direction(area,new Point(30,350),true).X==-1&&EdgeCamera.Direction(area,new Point(36,350),true)==Vector2.Zero,"wider edge activation leaves central play area stationary");
 }
}
