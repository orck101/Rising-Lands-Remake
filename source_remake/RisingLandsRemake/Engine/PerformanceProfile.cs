using System.Diagnostics;
using System.Text.Json;
namespace RisingLands;
public sealed partial class ResearchGame {
 bool profileRun;readonly List<double> profileUpdates=new(),profileDraws=new();
 sealed class CpuSample:IDisposable {readonly long start=Stopwatch.GetTimestamp();readonly List<double> target;public CpuSample(List<double> target){this.target=target;}public void Dispose()=>target.Add(Stopwatch.GetElapsedTime(start).TotalMilliseconds);}
 IDisposable? ProfileSample(bool update)=>profileRun?new CpuSample(update?profileUpdates:profileDraws):null;
 void PrepareProfile(){
  if(simulation.IsSixthMission||simulation.IsSeventhMission||simulation.IsEighthMission||simulation.IsNinthMission){profileRun=true;gameSpeed=captureMenu!.EndsWith("5")?5:1;paused=false;revealMap=false;return;}
  var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE03.ETA"));visuals.Apply(w);var seed=w.Units.First(a=>a.Type==0&&a.Owner==0&&VisualCatalog.Visible(a));
  for(int i=0;i<300;i++){var a=JsonSerializer.Deserialize<Actor>(JsonSerializer.Serialize(seed))!;a.Slot=5000+i;a.X=(45+i%25)*1024;a.Y=(45+i/25)*1024;w.Units.Add(a);}
  var state=new Simulation(w);for(int i=0;i<200;i++)state.Constructions.Add(new(){Id=1000+i,Type=3,Cell=5+i%20*5+256*(5+i/20*5),Progress=1});
  LoadMap(restored:state);profileRun=true;gameSpeed=captureMenu!.EndsWith("5")?5:1;paused=false;revealMap=false;camera=new(30*32,30*32);ClampCamera();
 }
 void WriteProfile(){if(!profileRun||capture==null)return;
  static object Summary(List<double> values){var a=values.Skip(10).Order().ToArray();return new{samples=a.Length,meanMs=a.Average(),p95Ms=a[(int)((a.Length-1)*.95)],maxMs=a[^1]};}
  File.WriteAllText(Path.ChangeExtension(capture,".json"),JsonSerializer.Serialize(new{map=simulation.World.MapName,speed=gameSpeed,units=simulation.World.Units.Count,sites=simulation.Constructions.Count,ticks=simulation.Tick,update=Summary(profileUpdates),draw=Summary(profileDraws),limits=simulation.IsSixthMission||simulation.IsSeventhMission||simulation.IsEighthMission||simulation.IsNinthMission?"CPU timings from native mission opening, after ten warm-up frames. Excludes GPU latency and later large armies.":"CPU timings from same synthetic scene, after ten warm-up frames. Excludes GPU latency; does not represent every gameplay scenario."},new JsonSerializerOptions{WriteIndented=true}));
 }
}
