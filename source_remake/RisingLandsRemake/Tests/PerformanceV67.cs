using System.Diagnostics;
using System.Text.Json;
namespace RisingLands;
public static class PerformanceV67 {
 public static void Run(string root,string output){var results=new List<object>();foreach(int count in new[]{0,50,200}){
  var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE03.ETA"));new VisualCatalog().Apply(w);var s=new Simulation(w);for(int i=0;i<count;i++){int cell=(5+i%20*5)+256*(5+i/20*5);s.Constructions.Add(new(){Id=1000+i,Type=3,Cell=cell,Progress=1});w.Buildings.Add(new(1000+i,3,0,cell));}
  var seed=w.Units.First(a=>a.Type==0&&a.Owner==0&&VisualCatalog.Visible(a));for(int i=0;i<300;i++){var a=JsonSerializer.Deserialize<Actor>(JsonSerializer.Serialize(seed))!;a.Slot=5000+i;a.X=(5+i%25)*1024;a.Y=(5+i/25)*1024;w.Units.Add(a);}long checksum=0;var timer=Stopwatch.StartNew();for(int n=0;n<2;n++)for(int c=0;c<65536;c++){if(s.ConstructionCovers(c))checksum++;if(s.Passable(c&255,c>>8))checksum++;}timer.Stop();double queryMs=timer.Elapsed.TotalMilliseconds;timer.Restart();for(int i=0;i<20;i++)s.Step();timer.Stop();results.Add(new{buildings=count,units=w.Units.Count,queryMs,stepMs=timer.Elapsed.TotalMilliseconds/20,checksum});
 }Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);File.WriteAllText(output,JsonSerializer.Serialize(results,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(File.ReadAllText(output));}
}
