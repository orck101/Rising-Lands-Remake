using System.Text.Json;
namespace RisingLands;
public static class SaveLoadProbe {
 public static void Run(string root,string path){
  string json=File.ReadAllText(path);using var doc=JsonDocument.Parse(json);
  var campaign=JsonSerializer.Deserialize<CampaignProgress>(doc.RootElement.GetProperty("Campaign").GetRawText())!;
  campaign.TransitionOpen=false;
  var world=LegacyAssets.Map(PackageLayout.Asset(root,"CD","WORLD",doc.RootElement.GetProperty("MapName").GetString()!));
  new VisualCatalog().Apply(world);
  var loaded=new Simulation(world,campaign);loaded.Restore(json);
  Console.WriteLine($"LOAD PASS: {path}, tick={loaded.Tick}, units={loaded.World.Units.Count}");
 }
}
