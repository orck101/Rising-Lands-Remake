using System.Reflection;
using System.Text.Json;
namespace RisingLands;
public static class MealRadiusTests {
 public static void Run(string root,string snapshot,string output){int checks=0;void Check(bool ok,string label){if(!ok)throw new Exception(label);checks++;Console.WriteLine("PASS "+label);}
 Simulation Load(){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE05.ETA"));new VisualCatalog().Apply(w);return new(w);}
 var s=Load();s.Restore(File.ReadAllText(snapshot));var a=s.World.Units.First(u=>u.Owner==0&&u.Type==0&&u.Health>0);var depot=s.World.Buildings.First(b=>b.Owner==1&&b.Type==3&&s.Inhabited(b));
 foreach(var other in s.World.Units)other.Satiety=other.SatietyMaximum;s.Hunger.Trips.Clear();s.Campaign.Alliances[1]=true;int arrival=s.BuildingInteractionCells(depot).First(c=>s.Passable(c&255,c>>8));a.X=(arrival&255)*1024;a.Y=(arrival>>8)*1024;a.Satiety=0;a.Route.Clear();
 var trip=new FeedingTrip{Slot=a.Slot,AnchorCell=arrival};s.Hunger.Trips.Add(trip);
 typeof(Simulation).GetMethod("FindMeal",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(s,new object[]{a,trip});
 // Remove ambiguity from surrounding piles without changing the conservation ledger:
 // test allied consumption directly at a valid depot interaction cell.
 trip.Ground=-1;trip.Warehouse=depot.Slot;trip.Arrival=arrival;a.Route.Clear();int before=s.World.Resources[1];
 for(int i=0;i<60&&a.Satiety==0;i++)typeof(Simulation).GetMethod("HungerStep",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(s,null);
 Check(a.Satiety>0&&s.World.Resources[1]==before-1,"allied meal deducted from warehouse owner");
 Check(s.Hunger.SharedFoodNet[0]==1&&s.Hunger.SharedFoodNet[1]==-1,"shared food accounting balances");
 var copy=Load();copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"allied meal save/load and conservation");
 for(int i=0;i<60;i++){s.Step();copy.Step();}Check(s.Serialize()==copy.Serialize(),"allied meal deterministic continuation");
 // Controlled open corridor tests the actual finder, including range boundaries.
 var t=Load();var unit=t.World.Units.First(u=>u.Owner==0&&u.Type==0);int anchor=100*256+100;unit.X=100*1024;unit.Y=100*1024;unit.Satiety=0;
 for(int y=95;y<=105;y++)for(int x=95;x<=128;x++){int c=y*256+x;t.World.Collision[c]=0;t.World.Tiles[c]=0;t.World.Objects[c]=0;t.World.Quantities[c]=0;}
 foreach(int cell in Enumerable.Range(0,65536).Where(c=>t.World.Objects[c]==67).ToArray()){t.World.Objects[cell]=0;t.World.Quantities[cell]=0;}
 foreach(int owner in Enumerable.Range(0,4))t.World.Resources[owner]=0;
 void Find(int offset){int food=anchor+offset;t.World.Objects[food]=67;t.World.Quantities[food]=2;typeof(Simulation).GetField("mealCacheTick",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(t,-1L);var f=new FeedingTrip{Slot=unit.Slot,AnchorCell=anchor};typeof(Simulation).GetMethod("FindMeal",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(t,new object[]{unit,f});Check(offset<=20?f.Ground==food&&unit.Route.Count==offset:f.Arrival<0,"ground boundary "+offset);t.World.Objects[food]=0;t.World.Quantities[food]=0;unit.Route.Clear();}
 Find(20);Find(21);
 // Warehouse radius is measured at its reachable interaction point.
 var near=typeof(Simulation).GetMethod("MealNear",BindingFlags.Static|BindingFlags.NonPublic)!;
 Check((bool)near.Invoke(null,new object[]{0,20,Simulation.WarehouseMealRadius})!,"warehouse includes20 tiles");Check(!(bool)near.Invoke(null,new object[]{0,21,Simulation.WarehouseMealRadius})!,"warehouse excludes21 tiles");
 s.Campaign.Alliances[1]=false;a.Satiety=0;a.Route.Clear();var denied=new FeedingTrip{Slot=a.Slot,AnchorCell=arrival};typeof(Simulation).GetMethod("FindMeal",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(s,new object[]{a,denied});Check(denied.Warehouse!=depot.Slot,"hostile warehouse rejected after alliance breaks");
 File.WriteAllText(output,JsonSerializer.Serialize(new{Checks=checks,Passed=true,GroundRadius=Simulation.GroundMealRadius,WarehouseRadius=Simulation.WarehouseMealRadius}));
 }
}




