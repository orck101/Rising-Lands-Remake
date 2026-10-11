using System.Text.Json;
namespace RisingLands;
// Diagnostic player: ordinary engine commands, real costs, discovered sources only.
// This is not a human playthrough or a proof of optimal strategy.
public static class MissionFiveScript {
 public static Simulation Load(string root){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD","WORLD","MONDE05.ETA"));new VisualCatalog().Apply(w);return new(w);}
 static int Distance(int a,int b)=>Math.Abs((a&255)-(b&255))+Math.Abs((a>>8)-(b>>8));
 public static void Run(string root,string output){
  Directory.CreateDirectory(output);var s=Load(root);var events=new List<object>();var checkpoints=new List<object>();var timer=System.Diagnostics.Stopwatch.StartNew();
  void Event(string action,object details){events.Add(new{Tick=s.Tick,Action=action,Details=details});Console.WriteLine($"SCRIPT {s.Tick/30}s {action}: {JsonSerializer.Serialize(details)}");}
  void Save(){string json=s.Serialize();File.WriteAllText(Path.Combine(output,$"script-{s.Tick/30}s.json"),json);var copy=Load(root);copy.Restore(json);if(copy.Serialize()!=json)throw new InvalidDataException("Script roundtrip mismatch");}
  for(int i=0;i<=30*60*30;i++){
   if(i%150==0){
    var own=s.World.Buildings.Where(b=>b.Owner==0).ToArray();var depot=own.FirstOrDefault(b=>b.Type==3&&s.Inhabited(b));var crop=own.FirstOrDefault(b=>b.Type==4&&s.Inhabited(b));
    foreach(var b in own.Where(b=>b.Type!=2&&!s.Inhabited(b)&&!s.ResidentTrips.Any(t=>t.Building==b.Slot))){
     var settler=s.World.Units.FirstOrDefault(a=>a.Owner==0&&a.Type==9&&a.Health>0&&VisualCatalog.Visible(a)&&!s.ResidentTrips.Any(t=>t.Slot==a.Slot));
     if(settler==null){var home=own.FirstOrDefault(h=>h.Type==2&&s.Residents(h.Slot)>1);if(home!=null)settler=s.ReleaseResident(home.Slot);}
     if(settler!=null&&s.EnterBuilding(settler,b.Slot))Event("occupy",new{b.Type,b.Slot,Settler=settler.Slot});
    }
    var farmers=s.World.Units.Where(a=>a.Owner==0&&a.Type==5&&a.Health>0&&VisualCatalog.Visible(a)).OrderBy(a=>a.Slot).ToArray();
    for(int f=0;f<farmers.Length;f++){var a=farmers[f];int anchor=a.Y/1024*256+a.X/1024;
     int destination=crop!=null&&f==farmers.Length-1&&farmers.Length>1?crop.Slot:depot?.Slot??-1;
     if(destination>=0&&(a.FoodDestinationKind!=2||a.FoodDestination!=destination))s.SetFoodDestination(a,2,destination);
     if(a.FoodJob==0&&a.Food==0){foreach(int c in Enumerable.Range(0,65536).Where(c=>s.Discovered(c)&&s.IsFood(c)).OrderBy(c=>Distance(c,anchor)).Take(24))if(s.Gather(a,c,false)){if(destination<0)s.SetFoodDestination(a,1,anchor);Event("gather",new{a.Slot,Cell=c,Destination=destination});break;}}
    }
    if(i%900==0&&farmers.Length>0&&!Enumerable.Range(0,65536).Any(c=>s.Discovered(c)&&s.IsFood(c))){var scout=farmers[0];int anchor=scout.Y/1024*256+scout.X/1024;var home=own.First(b=>b.Type==2);
     if(scout.FoodJob==0&&scout.Route.Count==0)foreach(int c in Enumerable.Range(0,65536).Where(c=>(c&255)<s.World.Width&&(c>>8)<s.World.Height&&!s.Discovered(c)&&Distance(c,home.Cell)<=60&&new[]{-1,1,-256,256}.Any(d=>s.Discovered(c+d))).OrderBy(c=>Distance(c,anchor)).Take(24))if(s.Order(scout,c&255,c>>8,true)){Event("explore",new{scout.Slot,Cell=c});break;}
    }
    var builder=s.World.Units.FirstOrDefault(a=>a.Owner==0&&a.Type==7&&a.Health>0&&VisualCatalog.Visible(a));
    if(builder!=null&&!s.Constructions.Any(c=>!s.Complete(c)&&c.Owner==0)){
     int type=new[]{3,4,1,7,16,8}.FirstOrDefault(t=>s.BuildingUnlocked(t)&&!own.Any(b=>b.Type==t)&&!s.Constructions.Any(c=>c.Owner==0&&c.Type==t),-1);bool built=false;
     if(type>=0&&s.World.Resources[0]>=s.Rule(type).Food&&s.World.Resources[4]>=s.Rule(type).Rock){int anchor=builder.Y/1024*256+builder.X/1024;
      foreach(int c in Enumerable.Range(0,65536).Where(c=>s.Discovered(c)&&Distance(c,anchor)<=18&&s.CanPlaceInExploredArea(type,c)).OrderBy(c=>Distance(c,anchor)).ThenBy(c=>c))if(s.StartConstruction(builder,type,c)){Event("build",new{Type=type,Cell=c});built=true;break;}
     }
     if(!built&&depot!=null&&builder.MineJob==0&&builder.Rock==0&&s.World.Resources[4]<80){foreach(int m in Enumerable.Range(0,s.World.Mines.Count).Where(m=>s.Discovered(s.World.Mines[m].Cell)).OrderBy(m=>Distance(s.World.Mines[m].Cell,builder.Y/1024*256+builder.X/1024)))if(s.StartMining(builder,m,false)){s.SetRockDestination(builder,depot.Slot);builder.RepeatMine=true;Event("mine",new{builder.Slot,Mine=m});break;}}
    }
    if(depot!=null&&farmers.Length<3&&s.World.Resources[0]>=s.TrainingRule(5).Food+10&&s.Recruit(5))Event("train-farmer",new{Food=s.World.Resources[0]});
    var laboratory=own.FirstOrDefault(b=>b.Type==7);if(laboratory!=null&&s.Scientists()+s.LaboratoryWorkers.Count(j=>j.Stage==0)<2&&s.AssignLaboratoryWorker(laboratory.Slot))Event("researcher",new{laboratory.Slot});
    if(s.ExtraResearchOffered&&s.World.Resources[0]>=50&&s.PurchaseExtraResearch(s.ActiveResearchBranch))Event("extra-research",new{Branch=s.ActiveResearchBranch});
    if(i%900==0&&s.World.Resources[0]>80){var barracks=own.FirstOrDefault(b=>b.Type==1&&s.Inhabited(b));if(barracks!=null&&s.Recruit(0,barracks.Slot))Event("train-soldier",new{barracks.Slot});}
   }
   if(i%9000==0){Save();checkpoints.Add(new{s.Tick,Food=s.World.Resources[0],Rock=s.World.Resources[4],Parts=s.World.Resources[8],Units=s.World.Units.Where(a=>a.Owner==0&&a.Health>0).GroupBy(a=>a.Type).ToDictionary(g=>g.Key,g=>g.Count()),Buildings=s.World.Buildings.Where(b=>b.Owner==0).Select(b=>new{b.Type,b.Cell,Occupants=s.Occupants(b.Slot)}).ToArray(),Cultures=s.Cultures.Select(c=>new{c.Building,c.Food,c.Born,c.Consumed}).ToArray(),s.MissionResearchCount,s.HasMissionEnding,s.MissionWon,WallSeconds=timer.Elapsed.TotalSeconds});File.WriteAllText(Path.Combine(output,"script-results.json"),JsonSerializer.Serialize(new{Events=events,Checkpoints=checkpoints},new JsonSerializerOptions{WriteIndented=true}));}
   if(i<30*60*30)s.Step();
  }
  var resumed=Load(root);resumed.Restore(s.Serialize());for(int i=0;i<300;i++){s.Step();resumed.Step();}bool deterministic=s.Serialize()==resumed.Serialize();File.WriteAllText(Path.Combine(output,"continuation.json"),JsonSerializer.Serialize(new{Seconds=10,Deterministic=deterministic}));if(!deterministic)throw new InvalidDataException("Continuation differs");
 }
}
