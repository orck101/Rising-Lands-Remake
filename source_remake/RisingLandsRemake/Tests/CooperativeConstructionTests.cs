namespace RisingLands;
public static class CooperativeConstructionTests {
 public static void Run(string root){
  void Check(bool ok,string text){if(!ok)throw new Exception("FAIL: "+text);Console.WriteLine("PASS: "+text);}
  var s=MissionOneTests.Load(root);Check(s.Recruit(7),"recruit second real builder for shared construction");for(int i=0;i<2000&&s.Recruitments.Any(j=>!j.Finished);i++)s.Step();
  var builders=s.World.Units.Where(u=>u.Owner==0&&u.Type==7).ToArray();Check(builders.Length==2,"two builders available");
  int cell=Enumerable.Range(0,65536).First(c=>s.PlacementError(builders[0],3,c)==null);Check(s.StartConstruction(builders[0],3,cell),"shared site starts");var site=s.Constructions.Single();int food=s.World.Resources[0],rock=s.World.Resources[4];
  Check(s.ResumeConstruction(builders[1],site)&&Simulation.Workers(site).Count()==2,"second builder joins without replacing first");
  Check(s.ResumeConstruction(builders[1],site)&&Simulation.Workers(site).Count()==2&&s.World.Resources[0]==food&&s.World.Resources[4]==rock,"repeat join neither duplicates worker nor charges construction");
  Check(Simulation.Workers(site).Select(w=>w.WorkCell).Distinct().Count()==2,"builders reserve different work positions");
  for(int i=0;i<10000&&builders.Any(u=>u.Route.Count>0);i++)s.Step();Check(builders.All(u=>u.Route.Count==0),"both builders reach shared site");
  site.Progress=0;foreach(var w in Simulation.Workers(site)){w.AnimationTick=0;w.StrikeCooldown=20;}
  var single=MissionOneTests.Load(root);single.Restore(s.Serialize());single.Stop(single.World.Units.First(u=>u.Slot==builders[1].Slot));
  for(int i=0;i<160;i++){s.Step();single.Step();}
  Check(site.Progress==single.Constructions[0].Progress*2&&site.Progress>0,"two builders contribute exactly twice the work over matched hammer cycles");
  var copy=MissionOneTests.Load(root);copy.Restore(s.Serialize());for(int i=0;i<80;i++){s.Step();copy.Step();}Check(s.Serialize()==copy.Serialize(),"shared builder positions and independent hammer clocks survive save/load");
  s.Stop(builders[0]);int before=site.Progress;for(int i=0;i<64;i++)s.Step();Check(site.Progress>before&&Simulation.Workers(site).Single().BuilderSlot==builders[1].Slot,"stopping original builder leaves helper working");
  Check(s.ResumeConstruction(builders[0],site),"original builder rejoins active helper without displacement");
  var invalid=System.Text.Json.Nodes.JsonNode.Parse(s.Serialize())!;invalid["Constructions"]![0]!["Helpers"]![0]!["BuilderSlot"]=site.BuilderSlot;
  string good=s.Serialize();try{s.Restore(invalid.ToJsonString());throw new Exception("duplicate accepted");}catch(InvalidDataException){}Check(s.Serialize()==good,"duplicate construction crew rejected atomically");
  var legacy=System.Text.Json.Nodes.JsonNode.Parse(single.Serialize())!;legacy["Version"]=14;legacy["Constructions"]![0]!.AsObject().Remove("Helpers");var migrated=MissionOneTests.Load(root);migrated.Restore(legacy.ToJsonString());Check(Simulation.Workers(migrated.Constructions[0]).Count()==1,"v14 construction save migrates with original single worker");
  var traveling=MissionOneTests.Load(root);traveling.Restore(s.Serialize());var travelSite=traveling.Constructions[0];var travelWorker=traveling.World.Units.First(u=>u.Slot==builders[0].Slot);Check(Enumerable.Range(0,65536).Where(c=>Math.Abs((c&255)-travelWorker.X/1024)+Math.Abs((c>>8)-travelWorker.Y/1024)>10&&traveling.Passable(c&255,c>>8)).Any(c=>traveling.Order(travelWorker,c&255,c>>8,true)),"travel fixture has an actual reachable path");int animation=travelSite.AnimationTick;traveling.Step();Check(travelSite.AnimationTick==animation,"worker still travelling contributes no hammer cycle");
  for(int i=0;i<3000&&!s.Complete(site);i++)s.Step();Check(s.Complete(site)&&s.World.Buildings.Count(b=>b.Slot==site.Id)==1,"shared completion creates exactly one building");
 }
}
