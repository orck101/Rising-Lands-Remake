using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareEnvironmentCapture(){
  if(captureMenu=="environment-v62-third"){
   var levels=Simulation.UnlockData.Missions.First(m=>m.Mission==3).Levels.ToArray();LoadMap(new CampaignProgress{StartingLevels=levels,SelectedBranch=0});ResearchV58Tests.Staff(simulation);simulation.AgricultureResearch.Progress=0;
   ResearchV58Tests.Advance(simulation,14400);while(researchMovies.Count>0){StartResearchMovie();UpdateResearchMovie(new GameTime(),new KeyboardState(Keys.Enter),default);}ResearchV58Tests.GatherFood(simulation);
   if(!simulation.PurchaseExtraResearch(2))throw new InvalidDataException("Extra engineering unavailable");ResearchV58Tests.Advance(simulation,7200);
   if(researchMovies.Count!=1||simulation.MissionResearchCount!=3)throw new InvalidDataException("Exactly one third movie must be queued");StartResearchMovie();
   if(playingInvention?.Key!="GENIECIVIL5"||playingResearchMovie?.Name!="LABO2")throw new InvalidDataException("Missing native event must reuse lab movie");
   string before=simulation.Serialize();AdvanceSimulation();if(simulation.Serialize()!=before)throw new InvalidDataException("Third movie must pause simulation");movieSeconds=2;paused=true;return;
  }
  int anchor=Enumerable.Range(0,65536).Where(c=>captureMenu=="environment-v62-trees"?simulation.World.Objects[c] is >=1 and <=11:simulation.World.Objects[c] is >=29 and <=52).OrderBy(c=>Simulation.CellDistance(c,22*256+10)).First();
  if(captureMenu=="environment-v62-grown")for(int i=0;i<12000;i++)simulation.Step();
  revealMap=true;paused=true;zoom=1.75f;camera=new Vector2((anchor&255)*32-ViewWidth/(2*zoom),(anchor>>8)*32-ViewHeight/(2*zoom));ClampCamera();
  message=captureMenu=="environment-v62-trees"?"Arvores e vegetacao originais do mapa.":"Alimento cresce e se espalha em solo apropriado - 1.25x.";
 }
}
