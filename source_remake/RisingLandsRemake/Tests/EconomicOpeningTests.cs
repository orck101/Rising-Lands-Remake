using System.Text.Json;
namespace RisingLands;
public static class EconomicOpeningTests {
 public static void Run(string root,string output){int checks=0;void Check(bool value,string label){if(!value)throw new Exception(label);checks++;Console.WriteLine("PASS: "+label);}
 Simulation Load(int mission=5){var w=LegacyAssets.Map(PackageLayout.Asset(root,$"CD/WORLD/MONDE{mission:00}.ETA"));new VisualCatalog().Apply(w);return new(w);}
 var s=Load();var samples=new List<object>();
 foreach(int until in new[]{0,90*30,3*60*30,5*60*30,8*60*30,10*60*30,12*60*30}){
 while(s.Tick<until&&!s.MissionLost&&!s.MissionWon)s.Step();
 samples.Add(new{s.Tick,Clans=Enumerable.Range(1,3).Select(o=>new{Owner=o,Food=s.World.Resources[o],Rock=s.World.Resources[4+o],Farmers=s.World.Units.Count(a=>a.Owner==o&&a.Type==5&&a.Health>0),Builders=s.World.Units.Count(a=>a.Owner==o&&a.Type==7&&a.Health>0),Colonists=s.World.Units.Count(a=>a.Owner==o&&a.Type==9&&a.Health>0),Decision=s.DecisionFor(o),Sites=s.Constructions.Where(c=>c.Owner==o).Select(c=>new{c.Type,c.Progress}).ToArray(),Buildings=s.World.Buildings.Where(b=>b.Owner==o).Select(b=>new{b.Type,Active=s.Inhabited(b)}).ToArray()}).ToArray()});
 foreach(int o in Enumerable.Range(1,3))Check(s.EconomicOpening(o)==(s.Tick<18000),"clock boundary "+o+" at "+s.Tick);
 if(s.Tick<18000){Check(!s.Opponent.Sorties.Any(),"no offensive sortie during opening");Check(!s.Constructions.Any(c=>c.Type is 7 or 8 or 16),"secondary buildings deferred");}
 var c=Load();c.Restore(s.Serialize());Check(c.Serialize()==s.Serialize(),"checkpoint save/load "+s.Tick);for(int i=0;i<30;i++){s.Step();c.Step();}Check(s.Serialize()==c.Serialize(),"deterministic continuation "+s.Tick);
 }
 Check(!Load(4).EconomicOpening(1)&&!Load(6).EconomicOpening(1),"phase isolation");
 Check(Enumerable.Range(1,3).All(o=>s.World.Units.Any(a=>a.Owner==o&&a.Type==5&&a.Health>0)),"all clans retain food workers");
 File.WriteAllText(output,JsonSerializer.Serialize(new{Checks=checks,Passed=true,Samples=samples},new JsonSerializerOptions{WriteIndented=true}));
 }
}
