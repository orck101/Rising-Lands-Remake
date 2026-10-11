namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareMissionEightCapture(){
  if(!simulation.IsEighthMission)throw new InvalidDataException("Mission-eight capture requires MONDE08.ETA");pauseMenu.Show(MenuPage.Closed);paused=true;revealMap=false;
  string folder=Path.GetFullPath(PackageLayout.Asset(root,"../../experiments/mission08-v123"));
  if(captureMenu=="v123-eight-victory"){simulation.Restore(File.ReadAllText(Path.Combine(folder,"natural-victory.snapshot.json")));return;}
  var col=simulation.World.Units.First(a=>a.Owner==0&&a.Type==9&&VisualCatalog.Visible(a));var port=simulation.World.Buildings.Where(b=>b.Type==18).OrderBy(b=>Math.Abs((b.Cell&255)-col.X/1024)+Math.Abs((b.Cell>>8)-col.Y/1024)).First();if(!simulation.EnterBuilding(col,port.Slot))throw new InvalidDataException("Heliport entry unavailable");for(int i=0;i<1500&&!simulation.Ocean.Balloons.Any(a=>a.Owner==0);i++)simulation.Step();var builder=simulation.World.Units.First(a=>a.Owner==0&&a.Type==7);int goal=Enumerable.Range(12,35).SelectMany(y=>Enumerable.Range(108,16).Select(x=>y*256+x)).First(c=>simulation.Passable(c&255,c>>8)&&simulation.ConstructionGrass(c));if(!simulation.Order(builder,goal&255,goal>>8))throw new InvalidDataException("Flight unavailable");for(int i=0;i<1000&&!simulation.Ocean.Jobs.Any(j=>j.Actor==builder.Slot&&j.Stage==2);i++)simulation.Step();for(int i=0;i<400;i++)simulation.Step();var balloon=simulation.World.Units.First(a=>a.Type==17&&a.Owner==0);SelectOnly(balloon);Center();message="Travessia real: construtor embarcado no balao, sem recursos concedidos.";
 }
}
