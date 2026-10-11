using System.Text.Json;
using System.Text.Json.Nodes;
namespace RisingLands;
public static class DiscoveryV60Tests {
 static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: v60 "+name);Console.WriteLine("PASS: v60 "+name);}
 public static void Run(string root){
  var s=MissionOneTests.Load(root);Check(s.Exploration.Explored.Any(b=>b==0)&&s.Exploration.Explored.Any(b=>b==1),"mission begins with unknown terrain and locally discovered territory");
  var original=s.World.Units.Where(u=>u.Owner==0).Select(u=>(u,u.X,u.Y,u.Health)).ToArray();var homes=s.World.Buildings.Where(b=>b.Owner==0).ToArray();
  foreach(var h in homes)s.World.Buildings.Remove(h);foreach(var (u,x,y,h) in original)u.Health=0;
  var scout=original.First(v=>v.u.Type==5).u;scout.Health=100;scout.X=32*1024;scout.Y=32*1024;s.UpdateVisibility();
  // Isolated native probes are an independent x86 oracle, not expected values copied from C#.
  using var native=JsonDocument.Parse(File.ReadAllText(Path.GetFullPath(PackageLayout.Asset(root,"../../reverse_engineering/visibility_v60.json"))));
  var vision=native.RootElement.GetProperty("probes").EnumerateArray().First(p=>p.GetProperty("entry").GetString()=="0x476bdf"&&p.GetProperty("radius").GetInt32()==5&&p.GetProperty("center")[0].GetInt32()==32&&p.GetProperty("center")[1].GetInt32()==32).GetProperty("cells").EnumerateArray().Select(c=>c.GetInt32()).ToHashSet();
  Check(Enumerable.Range(0,65536).Where(s.InSight).ToHashSet().SetEquals(vision.Where(c=>(c&255)<s.World.Width&&(c>>8)<s.World.Height)),"farmer vision exactly matches original x86 radius-five footprint");
  int center=32*256+32;Check(s.InSight(center)&&s.Discovered(center)&&s.Discovered(center+7)&&!s.InSight(center+7),"native discovery fringe extends beyond current vision");
  s.World.Objects[center]=66;s.World.Quantities[center]=9;s.UpdateVisibility();scout.X=2*1024;scout.Y=2*1024;s.UpdateVisibility();s.World.Objects[center]=67;s.World.Quantities[center]=3;s.UpdateVisibility();
  Check(s.Discovered(center)&&!s.InSight(center)&&s.Exploration.Objects[center]==66&&s.Exploration.Quantities[center]==9,"explored territory remembers last seen resources and hides later changes");
  var enemy=new Actor{Owner=1,X=32*1024,Y=32*1024};Check(!s.UnitInSight(enemy),"enemy outside current vision is hidden even on explored ground");enemy.X=scout.X;enemy.Y=scout.Y;Check(s.UnitInSight(enemy),"enemy inside current vision appears");
  foreach(var (u,x,y,h) in original){u.X=x;u.Y=y;u.Health=h;}s.World.Buildings.AddRange(homes);s.World.Objects[center]=0;s.World.Quantities[center]=0;s.UpdateVisibility();
  string saved=s.Serialize();var copy=MissionOneTests.Load(root);copy.Restore(saved);Check(copy.Serialize()==saved&&copy.Discovered(center),"explored state and last seen resources survive exact save/load roundtrip");
  var invalid=JsonNode.Parse(saved)!;invalid["Exploration"]!["Explored"]=Convert.ToBase64String(new byte[2]);try{copy.Restore(invalid.ToJsonString());throw new Exception("Invalid exploration accepted");}catch(InvalidDataException){}Check(copy.Serialize()==saved,"invalid fog save rejected without changing current game");
  var legacy=JsonNode.Parse(saved)!;legacy["Version"]=23;legacy.AsObject().Remove("Exploration");copy.Restore(legacy.ToJsonString());Check(copy.Exploration.Explored.Any(b=>b==1)&&!copy.Discovered(center),"v23 saves migrate with current local vision and no fabricated history");
  copy.Restart();Check(!copy.Discovered(center),"restart clears remote explored history");
  var research=ResearchV58Tests.Load(root);ResearchV58Tests.Staff(research);research.AgricultureResearch.Progress=0;var completed=new List<Invention>();research.ResearchCompleted+=completed.Add;
  ResearchV58Tests.Advance(research,14400);Check(completed.Count==2&&completed.All(t=>ResearchCompletionCatalog.For(t)!=null),"both mandatory discoveries emit one mapped original animation event");
  ResearchV58Tests.GatherFood(research);Check(research.PurchaseExtraResearch(1),"extra research purchased for animation fixture");ResearchV58Tests.Advance(research,7200);Check(completed.Count==3&&completed.Last().Branch==1&&ResearchCompletionCatalog.For(completed.Last())?.Name=="LABO3","third discovery emits the original military movie");
  research.Restore(research.Serialize());ResearchV58Tests.Advance(research,30);Check(completed.Count==3,"loading completed research does not replay completion events");
  foreach(var movie in ResearchCompletionCatalog.Data.Movies.Values){string folder=PackageLayout.FromBase("Content/ResearchCompletion",movie.Name);Check(movie.Frames>1&&movie.DurationMs>0&&Directory.GetFiles(folder,"*.png").Length==movie.Frames,"complete original movie frames: "+movie.Name);}
  Console.WriteLine("ALL V60 DISCOVERY TESTS PASSED");
 }
}
