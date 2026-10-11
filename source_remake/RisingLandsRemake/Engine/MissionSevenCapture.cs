using Microsoft.Xna.Framework;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareMissionSevenCapture(){
  if(!simulation.IsSeventhMission)throw new InvalidDataException("Mission-seven capture requires native map seven");
  pauseMenu.Show(MenuPage.Closed);paused=true;revealMap=false;
  if(captureMenu=="v122-seven-victory"){
   simulation.Restore(File.ReadAllText(Path.GetFullPath(PackageLayout.Asset(root,"../../experiments/mission07-v122/natural-victory.snapshot.json"))));return;
  }
  var archer=simulation.World.Units.First(a=>a.Owner==0&&a.Type==8);var raptor=simulation.World.Units.Single(a=>a.Type==21);
  raptor.X=archer.X+3*1024;raptor.Y=archer.Y;simulation.UpdateVisibility();simulation.Attack(archer,raptor);for(int i=0;i<17;i++)simulation.Step();SelectOnly(archer);Center();message="Ensaio visual isolado: ataque e projétil original do Raptor.";
 }
}
