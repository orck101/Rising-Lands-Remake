using System.Text.Json;
namespace RisingLands;
public sealed class SaveSlots {
 public const int Count=5;
 readonly string directory,map;readonly string[] legacyDirectories;readonly bool shared;
 public SaveSlots(string directory,string map,IEnumerable<string>? legacyDirectories=null,bool shared=false){this.shared=shared;this.directory=directory;this.map=Path.GetFileName(map);this.legacyDirectories=legacyDirectories?.ToArray()??Array.Empty<string>();}
 public string PathFor(int slot){if(slot<1||slot>Count)throw new ArgumentOutOfRangeException(nameof(slot));return Path.Combine(directory,(shared?"campaign":map)+".slot-"+slot+".research-v33.json");}
 public string? Existing(int slot){var path=PathFor(slot);if(File.Exists(path))return path;
  if(shared){foreach(int v in new[]{32,31,30,29,28,27,26,25,24,23,22,21,20,19,18,17}){var previous=path.Replace("research-v33.json","research-v"+v+".json");if(File.Exists(previous))return previous;}var candidates=new List<(string Path,int Rank,int Slot,string Map)>();int rank=0;
   foreach(var dir in new[]{directory}.Concat(legacyDirectories).Distinct()){
    if(Directory.Exists(dir))foreach(var file in Directory.GetFiles(dir,"MONDE*.ETA*.research-v*.json")){
     var match=System.Text.RegularExpressions.Regex.Match(Path.GetFileName(file),@"^(MONDE\d+\.ETA)(?:\.slot-(?<slot>[1-5]))?\.research-v\d+\.json$",System.Text.RegularExpressions.RegexOptions.IgnoreCase);
     if(match.Success)candidates.Add((file,rank,match.Groups["slot"].Success?int.Parse(match.Groups["slot"].Value):1,match.Groups[1].Value.ToUpperInvariant()));
    }rank++;
   }
   var latest=candidates.GroupBy(c=>(c.Map,c.Slot)).Select(g=>g.OrderBy(c=>c.Rank).ThenByDescending(c=>File.GetLastWriteTimeUtc(c.Path)).First()).OrderBy(c=>c.Rank).ThenByDescending(c=>File.GetLastWriteTimeUtc(c.Path)).ThenBy(c=>c.Path);
   var assigned=new Dictionary<int,string>();foreach(var old in latest){int target=!assigned.ContainsKey(old.Slot)?old.Slot:Enumerable.Range(1,Count).FirstOrDefault(i=>!assigned.ContainsKey(i));if(target!=0)assigned[target]=old.Path;}
   return assigned.GetValueOrDefault(slot);
  }
  foreach(int v in new[]{32,31,30,29,28,27,26,25,24,23,22,21,20,19,18,17}){var previous=path.Replace("research-v33.json","research-v"+v+".json");if(File.Exists(previous))return previous;}if(slot!=1)return null;return legacyDirectories.SelectMany(d=>Enumerable.Range(4,29).Reverse().Select(v=>Path.Combine(d,map+".research-v"+v+".json"))).FirstOrDefault(File.Exists);}
 public string Description(int slot){var path=Existing(slot);if(path==null)return "Slot "+slot+" - Vazio";try{using var doc=JsonDocument.Parse(File.ReadAllText(path));if(!shared&&doc.RootElement.GetProperty("MapName").GetString()!=map)return "Slot "+slot+" - Incompativel";return "Slot "+slot+" - "+(shared?doc.RootElement.GetProperty("MapName").GetString()!.Replace("MONDE","Missao ").Replace(".ETA","")+" - ":"")+File.GetLastWriteTime(path).ToString("dd/MM/yyyy HH:mm");}catch(Exception e)when(e is IOException or UnauthorizedAccessException or JsonException or KeyNotFoundException or InvalidOperationException){return "Slot "+slot+" - Arquivo invalido";}}
 public void Save(Simulation simulation,int slot){if(!shared&&simulation.World.MapName!=map)throw new InvalidDataException("Save slot map mismatch");simulation.SaveTo(PathFor(slot));}
 public string Read(int slot){var path=Existing(slot)??throw new FileNotFoundException("Slot vazio");string json=File.ReadAllText(path);try{using var doc=JsonDocument.Parse(json);return json;}catch(JsonException)when(File.Exists(path+".bak")){return File.ReadAllText(path+".bak");}}
 public void Load(Simulation simulation,int slot)=>simulation.Restore(Read(slot));
}
