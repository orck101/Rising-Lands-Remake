using System.Reflection;
using System.Text.Json;
namespace RisingLands;
public static class HungerContinuesTests {
 public static void Run(string root,string snapshot,string output){int checks=0;void Check(bool ok,string label){if(!ok)throw new Exception(label);checks++;Console.WriteLine("PASS "+label);}
 Simulation Load(int mission=5){var w=LegacyAssets.Map(PackageLayout.Asset(root,$"CD/WORLD/MONDE{mission:00}.ETA"));new VisualCatalog().Apply(w);var s=new Simulation(w);if(mission==5)s.Restore(File.ReadAllText(snapshot));return s;}
 void Hunger(Simulation s)=>typeof(Simulation).GetMethod("HungerStep",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(s,null);
 void Clock(Simulation s)=>typeof(Simulation).GetField("<Tick>k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(s,6000L);
 void NoMeals(Simulation s){for(int i=0;i<65536;i++)if(s.World.Objects[i]==67){s.World.Objects[i]=0;s.World.Quantities[i]=0;}Array.Clear(s.World.Resources);s.Hunger.Trips.Clear();foreach(var u in s.World.Units)u.Satiety=u.SatietyMaximum;typeof(Simulation).GetField("mealCacheTick",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(s,-1L);Clock(s);}
 foreach(int owner in Enumerable.Range(0,4))foreach(int type in new[]{5,7}){
  var s=Load();NoMeals(s);var a=s.World.Units.First(u=>u.Owner==owner&&u.Type==type&&u.Health>0&&VisualCatalog.Visible(u));a.Satiety=0;a.FatigueClock=0;int cell=a.Y/1024*256+a.X/1024;
  int goal=Enumerable.Range(0,65536).First(c=>Math.Abs((c&255)-(cell&255))+Math.Abs((c>>8)-(cell>>8))>=3&&Math.Abs((c&255)-(cell&255))+Math.Abs((c>>8)-(cell>>8))<=5&&s.Order(a,c&255,c>>8,true));var route=a.Route.ToArray();a.Food=2;a.CargoKind=1;a.FoodJob=3;a.DeliveryCell=goal;
  Hunger(s);Check(!s.Feeding(a)&&a.Route.SequenceEqual(route)&&a.Food==2&&a.FoodJob==3,$"no-food task/cargo continues owner {owner} type {type}");
  int health=a.Health;a.FatigueClock=s.FatiguePeriod(a)-1;Hunger(s);Check(a.Health<health&&!s.Feeding(a),$"no-food starvation retained owner {owner} type {type}");
 }
 var p=Load(1);NoMeals(p);var worker=p.World.Units.First(u=>u.Owner==0&&u.Type==5&&VisualCatalog.Visible(u));worker.Satiety=0;worker.FoodJob=2;worker.WorkTicks=7;worker.FoodSource=worker.Y/1024*256+worker.X/1024;Hunger(p);Check(worker.FoodJob==2&&worker.WorkTicks==7&&!p.Feeding(worker),"mission one stationary harvesting not blocked by hunger");
 var s2=Load();NoMeals(s2);var unit=s2.World.Units.First(u=>u.Owner==0&&u.Type==0);unit.Satiety=0;int anchor=100*256+100;unit.X=100*1024;unit.Y=100*1024;
 for(int y=99;y<=101;y++)for(int x=99;x<=140;x++){int c=y*256+x;s2.World.Collision[c]=0;s2.World.Tiles[c]=0;s2.World.Objects[c]=0;s2.World.Quantities[c]=0;}
 int food=anchor+10;s2.World.Objects[food]=67;s2.World.Quantities[food]=2;unit.Route=new(){anchor+1,anchor+2};Hunger(s2);Check(s2.Feeding(unit)&&s2.Hunger.Trips.Single(t=>t.Slot==unit.Slot).Ground==food,"food within20 radius triggers meal");Check(s2.Hunger.Trips.Single(t=>t.Slot==unit.Slot).PausedRoute.SequenceEqual(new[]{anchor+1,anchor+2}),"work route preserved during meal");
 for(int i=0;i<700&&unit.Satiety==0;i++)s2.Step();Check(unit.Satiety>0&&s2.World.Quantities[food]==1,"meal consumed");Check(unit.Route.LastOrDefault()==anchor+2,"paused goal resumes after eating");
 var s3=Load();NoMeals(s3);var a3=s3.World.Units.First(u=>u.Owner==0&&u.Type==0);s3.Campaign.Alliances[1]=true;s3.World.Resources[0]=s3.World.Resources[1]=10;var ally=s3.World.Buildings.First(b=>b.Owner==1&&b.Type==3&&s3.Inhabited(b));var own=s3.World.Buildings.First(b=>b.Owner==0&&b.Type==3);var ownResident=s3.World.Units.First(u=>u.Owner==0&&u.Type==9&&u.Health>0);ownResident.LegacyState=14;s3.World.BuildingResidents[own.Slot]=new(){ownResident.Slot};var arrival=s3.BuildingInteractionCells(ally).First(c=>s3.Passable(c&255,c>>8));Array.Clear(s3.World.Collision);Array.Clear(s3.World.Tiles);Array.Clear(s3.World.Objects);int cx=((own.Cell&255)*2+(ally.Cell&255)*3)/5,cy=((own.Cell>>8)*2+(ally.Cell>>8)*3)/5;a3.X=(arrival&255)*1024;a3.Y=(arrival>>8)*1024;var f=new FeedingTrip{Slot=a3.Slot,AnchorCell=arrival};typeof(Simulation).GetMethod("FindMeal",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(s3,new object[]{a3,f});Check(s3.World.Buildings.Any(b=>b.Slot==f.Warehouse&&b.Owner==1),"closer allied warehouse selected without ownership priority");
 var native=Load();string state=native.Serialize();var copy=Load();copy.Restore(state);Check(copy.Serialize()==state,"real save normalized roundtrip");for(int i=0;i<120;i++){native.Step();copy.Step();}Check(copy.Serialize()==native.Serialize(),"deterministic save continuation");
 File.WriteAllText(output,JsonSerializer.Serialize(new{Checks=checks,Passed=true,Radius=Simulation.MealSearchRadius}));
 }
}




