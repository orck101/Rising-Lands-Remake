using System.Reflection;
using System.Text.Json;
namespace RisingLands;
public static class NearestMealTests {
 public static void Run(string root,string snapshot,string output){
  int checks=0;void Check(bool ok,string label){if(!ok)throw new Exception(label);checks++;Console.WriteLine("PASS meal20: "+label);}
  Simulation Load(){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE05.ETA"));new VisualCatalog().Apply(w);var s=new Simulation(w);s.Restore(File.ReadAllText(snapshot));return s;}
  void Reset(Simulation s){Array.Clear(s.World.Objects);Array.Clear(s.World.Quantities);Array.Clear(s.World.Collision);Array.Clear(s.World.Tiles);Array.Clear(s.World.Resources);s.Hunger.Trips.Clear();foreach(var u in s.World.Units)u.Satiety=u.SatietyMaximum;typeof(Simulation).GetField("mealCacheTick",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(s,-1L);}
  FeedingTrip Find(Simulation s,Actor a){var trip=new FeedingTrip{Slot=a.Slot,AnchorCell=a.Y/1024*256+a.X/1024};typeof(Simulation).GetMethod("FindMeal",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(s,new object[]{a,trip});return trip;}
  Check(Simulation.GroundMealRadius==20&&Simulation.WarehouseMealRadius==20,"all source radii are20");
  for(int owner=0;owner<4;owner++){
   var s=Load();Reset(s);var a=s.World.Units.First(u=>u.Owner==owner&&u.Health>0&&VisualCatalog.Visible(u)&&s.NeedsFood(u));
   var own=s.World.Buildings.First(b=>b.Owner==owner&&b.Type==3&&s.Inhabited(b));int front=s.BuildingInteractionCells(own).First();
   a.X=(front&255)*1024;a.Y=(front>>8)*1024;s.World.Resources[owner]=10;int ground=front+3;s.World.Objects[ground]=67;s.World.Quantities[ground]=2;
   Check(Find(s,a).Warehouse==own.Slot,"near own warehouse beats farther ground owner"+owner);
   a.X=((front&255)+4)*1024;ground=front+5;s.World.Objects[front+3]=0;s.World.Objects[ground]=67;s.World.Quantities[ground]=2;if(owner>0)s.Opponent.KnownResources[owner][ground]=67;typeof(Simulation).GetField("mealCacheTick",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(s,-1L);
   Check(Find(s,a).Ground==ground,"near ground beats farther own warehouse owner"+owner);
   Array.Clear(s.World.Objects);int allyOwner=owner==0?3:0;s.Campaign.Alliances[owner==0?3:owner]=true;
   var ally=s.World.Buildings.First(b=>b.Owner==allyOwner&&b.Type==3&&s.Inhabited(b));var moved=ally with{Cell=own.Cell+8};s.World.Buildings[s.World.Buildings.IndexOf(ally)]=moved;int allyFront=s.BuildingInteractionCells(moved).First();a.X=(allyFront&255)*1024;a.Y=(allyFront>>8)*1024;s.World.Resources[allyOwner]=10;
   Check(Find(s,a).Warehouse==ally.Slot,"closer ally beats own warehouse owner"+owner);
   s.Campaign.Alliances[owner==0?3:owner]=false;
   Check(Find(s,a).Warehouse!=ally.Slot,"hostile warehouse never selected owner"+owner);
   Array.Clear(s.World.Resources);a.X=100*1024;a.Y=100*1024;int anchor=100*256+100;
   foreach(int distance in new[]{20,21}){Array.Clear(s.World.Objects);Array.Clear(s.World.Quantities);int cell=anchor+distance;s.World.Objects[cell]=67;s.World.Quantities[cell]=2;if(owner>0)s.Opponent.KnownResources[owner][cell]=67;typeof(Simulation).GetField("mealCacheTick",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(s,-1L);var t=Find(s,a);Check(distance==20?t.Ground==cell:t.Arrival<0,"boundary"+distance+" owner"+owner);}
  }
  var loaded=Load();Check(!loaded.Hunger.Trips.Any(t=>t.Slot is 118 or 122),"old long trips in slot1 are canceled on load");
  var copy=Load();copy.Restore(loaded.Serialize());Check(copy.Serialize()==loaded.Serialize(),"normalized slot1 roundtrip exact");
  for(int i=0;i<120;i++){loaded.Step();copy.Step();}Check(copy.Serialize()==loaded.Serialize(),"normalized slot1 deterministic continuation");
  var lost=Load();Reset(lost);var worker=lost.World.Units.First(a=>a.Owner==0&&a.Type==5&&a.Health>0);int from=worker.Y/1024*256+worker.X/1024;worker.Satiety=0;worker.Food=2;worker.FoodJob=3;worker.DeliveryCell=from+2;worker.Route=new(){from+1};lost.Hunger.Trips.Add(new(){Slot=worker.Slot,Ground=from+1,Arrival=from+1,AnchorCell=from,PausedRoute=new(){from+2}});
  typeof(Simulation).GetField("<Tick>k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(lost,6001L);
  typeof(Simulation).GetMethod("HungerStep",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(lost,null);
  Check(!lost.Feeding(worker)&&worker.Food==2&&worker.FoodJob==3&&worker.Route.LastOrDefault()==from+2,"vanished pile cancels trip and preserves cargo/task");
  File.WriteAllText(output,JsonSerializer.Serialize(new{Checks=checks,Passed=true,Radius=20,Policy="Nearest reachable valid source without category preference",Limit="Controlled simulations, no manual gameplay"}));
 }
}
