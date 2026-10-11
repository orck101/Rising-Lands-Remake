namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareConstructionCapture(string mode){
  SelectOnly(simulation.World.Units.First(u=>u.Type==7&&u.Owner==0));var u=selected!;
  int cell=Enumerable.Range(0,65536).Where(c=>(c&255)<simulation.World.Width&&(c>>8)<simulation.World.Height).OrderBy(c=>Math.Abs((c&255)-u.X/1024)+Math.Abs((c>>8)-u.Y/1024)).First(c=>simulation.StartConstruction(u,3,c));
  int percent=int.Parse(mode[5..]);var site=simulation.Constructions.Last();for(int i=0;i<10000&&site.Progress*100/simulation.Rule(3).Duration<percent;i++)simulation.Step();
  paused=true;Center();message="Construcao original: "+site.Progress*100/simulation.Rule(3).Duration+"%. Somente em terreno verde.";
 }
}
