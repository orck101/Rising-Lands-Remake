using System.Text.Json;
namespace RisingLands;
public static class MissionFiveAudit {
 public static void Run(string root,string output,int minutes=10,string? inputSnapshot=null){
  if(minutes<10||minutes>120)throw new ArgumentException("Audit duration must be 10..120 minutes");
  var timer=System.Diagnostics.Stopwatch.StartNew();
  var world=LegacyAssets.Map(PackageLayout.Asset(root,"CD","WORLD","MONDE05.ETA"));new VisualCatalog().Apply(world);
  var s=new Simulation(world);if(inputSnapshot!=null)s.Restore(File.ReadAllText(inputSnapshot));long startTick=s.Tick;var records=new List<object>();
  foreach(int until in new[]{0,5400,9000,18000}.Concat(Enumerable.Range(3,Math.Max(0,minutes/5-2)).Select(m=>m*9000)).Distinct().Where(t=>t>=startTick)){
   while(s.Tick<until)s.Step();
   var clans=Enumerable.Range(0,4).Select(owner=>new {
    Owner=owner,Food=s.World.Resources[owner],Rock=s.World.Resources[owner+4],Parts=s.World.Resources[owner+8],
    Units=s.World.Units.Where(a=>a.Owner==owner&&a.Health>0).GroupBy(a=>a.Type).ToDictionary(g=>g.Key,g=>g.Count()),
    Buildings=s.World.Buildings.Where(b=>b.Owner==owner).Select(b=>new{b.Slot,b.Type,b.Cell,Inhabited=s.Inhabited(b)}).ToArray(),
    Works=s.Constructions.Where(c=>c.Owner==owner).Select(c=>new{c.Type,c.Progress,Complete=s.Complete(c)}).ToArray(),
    Trained=s.Recruitments.Count(r=>r.Owner==owner&&r.Finished),FoodDeposited=s.Opponent.Deposited[owner],
    Sorties=s.World.Units.Count(a=>a.Owner==owner&&s.Opponent.Sorties.ContainsKey(a.Slot)),
    Levels=owner==0?s.EffectiveLevels:Enumerable.Range(0,4).Select(b=>s.OpponentLevel(owner,b)).ToArray(),
    ResearchBranch=owner==0?s.ActiveResearchBranch:s.Campaign.EnemyBranches[owner],
    Decision=owner==0?null:s.DecisionFor(owner),Economy=owner==0?null:JsonSerializer.Deserialize<OpponentEconomyPolicy>(JsonSerializer.Serialize(s.EconomyFor(owner))),Ready=owner>0&&s.EconomyReady(owner)
   }).ToArray();
   records.Add(new{s.Tick,Seconds=s.Tick/30,WallSeconds=timer.Elapsed.TotalSeconds,HasEnding=s.HasMissionEnding,s.MissionWon,Clans=clans});
   var copyWorld=LegacyAssets.Map(PackageLayout.Asset(root,"CD","WORLD","MONDE05.ETA"));new VisualCatalog().Apply(copyWorld);var copy=new Simulation(copyWorld);copy.Restore(s.Serialize());
   if(copy.Serialize()!=s.Serialize())throw new InvalidDataException("Mission five audit roundtrip failed");
   string snapshot=Path.Combine(Path.GetDirectoryName(Path.GetFullPath(output))!,"snapshots",$"baseline-{s.Tick/30}s.json");Directory.CreateDirectory(Path.GetDirectoryName(snapshot)!);File.WriteAllText(snapshot,s.Serialize());
   Console.WriteLine($"AUDIT checkpoint {s.Tick/30}s: save/load passed; "+string.Join("; ",Enumerable.Range(1,3).Select(o=>$"clan{o} food={s.World.Resources[o]}, rock={s.World.Resources[o+4]}, buildings={s.World.Buildings.Count(b=>b.Owner==o)}, trained={s.Recruitments.Count(r=>r.Owner==o&&r.Finished)}, phase={s.EconomyFor(o).Phase}")));
   Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);File.WriteAllText(output,JsonSerializer.Serialize(records,new JsonSerializerOptions{WriteIndented=true}));
  }
 }
}
