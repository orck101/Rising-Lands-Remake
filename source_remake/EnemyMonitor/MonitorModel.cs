using System.Text.Json;
namespace RisingLands.Monitoring;
public sealed record TechnologySnapshot(int Branch,string Name,int Level,int Maximum,string[] Developed);
public sealed record MilitaryUnitSnapshot(int Type,string Name,int Count);
public sealed record EnemySnapshot(int Owner,string Name,bool Present,bool Defeated,int Food,int Stone,int Gears,int Units,int Buildings,int Scientists,int ResearchBranch,double ResearchProgress,int Discoveries,string ResearchStatus,TechnologySnapshot[] Technologies){ public string? Relation {get;init;} public int? Sympathy {get;init;} public MilitaryUnitSnapshot[]? Military {get;init;} }
public sealed record MonitorSnapshot(int Schema,int ProcessId,string Session,string Map,int Mission,long Tick,int Speed,string Status,bool Running,DateTimeOffset UpdatedUtc,EnemySnapshot[] Enemies);
public static class MonitorFeed {
 public static void Write(string directory,MonitorSnapshot value){
  Directory.CreateDirectory(directory);string target=Path.Combine(directory,"game-"+value.ProcessId+".json"),temp=target+".tmp";
  try{File.WriteAllText(temp,JsonSerializer.Serialize(value));File.Move(temp,target,true);}finally{if(File.Exists(temp))File.Delete(temp);}
 }
 public static MonitorSnapshot Read(string path){using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);var value=JsonSerializer.Deserialize<MonitorSnapshot>(stream)??throw new InvalidDataException("Dados vazios");
  if(value.Schema!=1||value.ProcessId<=0||value.Enemies==null||value.Enemies.Length!=3||value.Enemies.Any(e=>e==null))throw new InvalidDataException("Formato de monitor incompatível");
  if(!value.Enemies.Select(e=>e.Owner).SequenceEqual(new[]{1,2,3}))throw new InvalidDataException("Identidade de inimigo inválida");
  foreach(var e in value.Enemies){
   if(e.Food<0||e.Stone<0||e.Gears<0||e.Technologies==null||e.Technologies.Length!=4||e.ResearchBranch is < -1 or >3||!double.IsFinite(e.ResearchProgress)||e.ResearchProgress is <0 or >1||e.Technologies.Any(t=>t==null||t.Developed==null||t.Level<0||t.Level>t.Maximum))throw new InvalidDataException("Dados de economia inválidos");
   if(e.Military is {} military && (military.Any(m=>m==null||m.Type<0||string.IsNullOrWhiteSpace(m.Name)||m.Count<0)||military.Select(m=>m.Type).Distinct().Count()!=military.Length))throw new InvalidDataException("Dados militares inválidos");
   if(!e.Technologies.Select(t=>t.Branch).Order().SequenceEqual(new[]{0,1,2,3}))throw new InvalidDataException("Ramos de tecnologia inválidos");
  }return value;
 }
}
