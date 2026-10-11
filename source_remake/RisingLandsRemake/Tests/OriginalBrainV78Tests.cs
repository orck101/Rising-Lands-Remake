using System.Text.Json;
namespace RisingLands;
public static class OriginalBrainV78Tests {
 public static void Run(string root){
  foreach(string name in new[]{"MONDE04","MONDE05"}){
   World World(){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD",name+".ETA"));new VisualCatalog().Apply(w);return w;}
   using var brain=new OriginalOpponentBrain(World());
   for(int i=0;i<60;i++)brain.Step(i);
   if(brain.Callbacks.Count<20)throw new Exception("Insufficient native AI execution");
   var state=brain.Save();using var loaded=new OriginalOpponentBrain(World());loaded.Restore(state);
   brain.Commands.Clear();loaded.Commands.Clear();
   for(int i=60;i<120;i++){brain.Step(i);loaded.Step(i);if(!brain.Save().Memory.SequenceEqual(loaded.Save().Memory))throw new Exception("Native brain diverged after reload");}
   if(!brain.Commands.SequenceEqual(loaded.Commands))throw new Exception("Original orders diverged after reload");
   Console.WriteLine("PASS v78 native brain "+name+": "+brain.Callbacks.Count+" callback routines, "+brain.Commands.Count+" orders, 60 deterministic reloads; "+state.Memory.Length+" saved bytes");
   string report=PackageLayout.Asset(root,"..","..","reports","native-brain-v78-"+name+".json");File.WriteAllText(report,JsonSerializer.Serialize(new{Map=name,Callbacks=brain.Callbacks.ToDictionary(p=>p.Key.ToString("x"),p=>p.Value),Commands=brain.Commands,Limits="Original decisions execute; commands captured, physics not exercised by this fixture. IA configuration uses original default values."},new JsonSerializerOptions{WriteIndented=true}));
  }
 }
}
