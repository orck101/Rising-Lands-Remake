using System.Text.Json;
namespace RisingLands;
public static class EnemyProductionV87Tests {
 public static void Run(string root){int count=0;void Check(bool ok,string name){if(!ok)throw new Exception("FAIL v87: "+name);count++;Console.WriteLine("PASS v87: "+name);}
  string json=File.ReadAllText(Path.GetFullPath(PackageLayout.Asset(root,"../../experiments/v87-existing-save.json")));var original=JsonSerializer.Deserialize<Simulation.Save>(json)!;
  Check(original.Recruitments.Count(j=>j.Owner==1&&j.Type==8&&j.Finished)==2,"actual old save reproduces two illegal trained archers");
  var s=OpponentV75Tests.Load(root);s.Restore(json);Check(s.OpponentLevel(1,1)==2,"red military level is still two");
  Check(s.World.Units.Count(a=>a.Owner==1&&a.Type==8&&a.Health>0)==1,"old illegal trained archers retired but native archer preserved");
  Check(s.World.Resources.SequenceEqual(original.Resources),"migration does not manufacture or debit resources");
  var copy=OpponentV75Tests.Load(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"corrected historical training remains loadable");
  for(int i=0;i<900;i++){s.Step();copy.Step();}Check(copy.Serialize()==s.Serialize(),"migration continues deterministically");
  Check(s.World.Units.Count(a=>a.Owner==1&&a.Type==8&&a.Health>0)<=1,"continued old save never recreates illegal archers");
  var pending=JsonSerializer.Deserialize<Simulation.Save>(json)!;var job=pending.Recruitments.First(j=>j.Owner==1&&j.Type==8);job.Finished=false;job.Progress=1;
  var seed=pending.BornActors!.FirstOrDefault(a=>a.Slot==job.SettlerSlot)??LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE04.ETA")).Units.First(a=>a.Slot==job.SettlerSlot);pending.Units[pending.Units.FindIndex(a=>a.Slot==job.SettlerSlot)]=JsonSerializer.Deserialize<Actor>(JsonSerializer.Serialize(seed))!;
  foreach(var residents in pending.Residents!.Values)residents.Remove(job.SettlerSlot);pending.Residents[job.BuildingSlot].Add(job.SettlerSlot);
  var queued=OpponentV75Tests.Load(root);queued.Restore(JsonSerializer.Serialize(pending));
  Check(queued.Recruitments.First(j=>j.SettlerSlot==job.SettlerSlot).Finished&&queued.World.Units.First(a=>a.Slot==job.SettlerSlot).Health==0,"illegal historical queued training cannot spawn a live archer or block barracks");
  copy=OpponentV75Tests.Load(root);copy.Restore(queued.Serialize());Check(copy.Serialize()==queued.Serialize(),"retired queued training and cost history reload exactly");
  Console.WriteLine("PASS v87 total "+count);
 }
}
