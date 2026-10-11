using System.Text.Json;using System.Text.Json.Nodes;using System.Reflection;
namespace RisingLands;
public static class LocalHungerDevelopmentTests {
 static Simulation Load(string root,bool stock=false){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE05.ETA"));new VisualCatalog().Apply(w);if(stock){w.Resources[0]=1000;w.Resources[4]=1000;}return new(w);}
 public static void Run(string root,string output,string snapshot){int checks=0;void Check(bool ok,string label){if(!ok)throw new Exception("FAIL: "+label);checks++;Console.WriteLine("PASS: "+label);}
 var native=Load(root);native.Restore(File.ReadAllText(snapshot));var home=native.World.Buildings.First(b=>b.Owner==1&&b.Type==2);Check(native.ReleaseResident(home.Slot,1,15)!=null,"historically trained/reconverted colonist exits sanctuary");
 var s=Load(root,true);int depot=ResearchTests.Build(s,3);foreach(var a in s.World.Units.Where(a=>a.Owner>0))a.Health=0;var soldier=s.World.Units.First(a=>a.Owner==0&&a.Type==0&&VisualCatalog.Visible(a));s.Stop(soldier);
 void ClearMeals(){for(int i=0;i<65536;i++)if(s.World.Objects[i]==67){s.World.Objects[i]=0;s.World.Quantities[i]=0;}s.Hunger.Trips.Clear();typeof(Simulation).GetField("mealCacheTick",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(s,-1L);soldier.Route.Clear();soldier.Satiety=0;}
 int anchor=soldier.Y/1024*256+soldier.X/1024;
 int Cell(int min,int max)=>Enumerable.Range(0,65536).Where(c=>(c&255)<s.World.Width&&(c>>8)<s.World.Height&&s.Passable(c&255,c>>8)&&Math.Abs((c&255)-(anchor&255))+Math.Abs((c>>8)-(anchor>>8))>=min&&Math.Abs((c&255)-(anchor&255))+Math.Abs((c>>8)-(anchor>>8))<=max).First(c=>s.Order(soldier,c&255,c>>8,true));
 int near=Cell(1,2),far=Cell(50,60);soldier.Route.Clear();
 void Find(FeedingTrip t)=>typeof(Simulation).GetMethod("FindMeal",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(s,new object[]{soldier,t});
 ClearMeals();s.World.Resources[0]=0;s.World.Objects[far]=67;s.World.Quantities[far]=5;var trip=new FeedingTrip{Slot=soldier.Slot,AnchorCell=anchor,ReturnCell=anchor};s.Hunger.Trips.Add(trip);Find(trip);Check(trip.Arrival<0&&soldier.Route.Count==0,"distant food does not cause wandering");
 soldier.FatigueClock=s.FatiguePeriod(soldier)-1;int health=soldier.Health;s.Step();Check(soldier.Health<health&&soldier.Satiety==0,"starvation remains without a local meal");
 ClearMeals();s.World.Objects[near]=67;s.World.Quantities[near]=5;trip=new(){Slot=soldier.Slot,AnchorCell=anchor,ReturnCell=anchor};s.Hunger.Trips.Add(trip);Find(trip);Check(trip.Ground==near&&soldier.Route.Count<=Simulation.MealMaximumPath,"near ground food selected with bounded route");
 for(int i=0;i<500&&soldier.Satiety==0;i++)s.Step();Check(soldier.Satiety>0&&s.World.Quantities[near]==4,"local food consumed and hunger restored");
 ClearMeals();s.World.Resources[0]=20;var b=s.World.Buildings.First(b=>b.Slot==depot);int arrival=s.BuildingInteractionCells(b).First(c=>s.Passable(c&255,c>>8));soldier.X=(arrival&255)*1024;soldier.Y=(arrival>>8)*1024;anchor=arrival;
 near=Cell(1,2);ClearMeals();s.World.Objects[near]=67;s.World.Quantities[near]=2;trip=new(){Slot=soldier.Slot,AnchorCell=anchor};Find(trip);Check(trip.Ground==near&&trip.Warehouse<0,"ground food prioritized over nearby warehouse");
 ClearMeals();trip=new(){Slot=soldier.Slot,AnchorCell=anchor};Find(trip);Check(trip.Warehouse==depot,"own occupied nearby warehouse usable");
 soldier.X=0;soldier.Y=0;anchor=0;ClearMeals();trip=new(){Slot=soldier.Slot,AnchorCell=anchor};Find(trip);Check(trip.Arrival<0,"far warehouse does not cause roaming");
 soldier.X=20*1024;soldier.Y=20*1024;anchor=20*256+20;ClearMeals();trip=new(){Slot=soldier.Slot,AnchorCell=anchor,Ground=far,Arrival=far};s.Hunger.Trips.Add(trip);
 var legacy=Load(root);var legacySoldier=legacy.World.Units.First(a=>a.Owner==0&&a.Type==0);legacySoldier.Satiety=0;legacySoldier.X=20*1024;legacySoldier.Y=20*1024;legacy.Hunger.Trips.Add(new(){Slot=legacySoldier.Slot,Ground=far,Arrival=far,AnchorCell=anchor});var old=JsonNode.Parse(legacy.Serialize())!;old["Version"]=35;old["Hunger"]!["Trips"]![0]!.AsObject().Remove("AnchorCell");var loaded=Load(root);loaded.Restore(old.ToJsonString());Check(loaded.Hunger.Trips[0].Arrival<0&&loaded.World.Units.First(a=>a.Slot==legacySoldier.Slot).Route.Count==0,"old distant feeding order repaired on import");
 string normalized=loaded.Serialize();var c=Load(root);c.Restore(normalized);Check(c.Serialize()==normalized,"new local hunger save/load exact");Check(s.NeedsFood(soldier)&&s.NeedsFood(s.World.Units.First(a=>a.Type==5)),"military and worker hunger retained");
 File.WriteAllText(output,JsonSerializer.Serialize(new{Checks=checks,Passed=true,Radius=Simulation.MealSearchRadius,PathLimit=Simulation.MealMaximumPath}));
 }
 public static void Replay(string root,string snapshot,string output){var s=Load(root);s.Restore(File.ReadAllText(snapshot));var records=new List<object>();for(int checkpoint=0;checkpoint<=6;checkpoint++){
 records.Add(new{s.Tick,Clans=Enumerable.Range(1,3).Select(o=>new{Owner=o,Food=s.World.Resources[o],Rock=s.World.Resources[4+o],Trained=s.Recruitments.Count(j=>j.Owner==o&&j.Finished&&Simulation.AdvancedAttacks.Rules.Any(a=>a.Type==j.Type)),Army=s.World.Units.Count(a=>a.Owner==o&&a.Health>0&&Simulation.AdvancedAttacks.Rules.Any(r=>r.Type==a.Type)),Discoveries=s.Opponent.ResearchCompleted[o],Levels=Enumerable.Range(0,4).Select(b=>s.OpponentLevel(o,b)).ToArray(),Buildings=s.World.Buildings.Where(b=>b.Owner==o).Select(b=>new{b.Type,Residents=s.Occupants(b.Slot)}).ToArray(),Plan=s.DecisionFor(o)}).ToArray()});
 var c=Load(root);c.Restore(s.Serialize());if(c.Serialize()!=s.Serialize())throw new Exception("Replay roundtrip failed");if(checkpoint==6||s.MissionWon||s.MissionLost)break;for(int i=0;i<3000&&!s.MissionLost&&!s.MissionWon;i++)s.Step();
 }
 File.WriteAllText(output,JsonSerializer.Serialize(records,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine("REPLAY completed "+s.Tick);
 }
}




