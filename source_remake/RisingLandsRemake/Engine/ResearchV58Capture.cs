using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareResearchV58Capture(){
  if(captureMenu=="research-v58-load"){VerifyUserSaveLoad();return;}
  if(captureMenu?.StartsWith("research-v58-ui")==true){VerifyExtraResearchInput();return;}
  var campaign=new CampaignProgress{StartingLevels=Simulation.UnlockData.Missions.First(m=>m.Mission==3).Levels.ToArray(),SelectedBranch=0};LoadMap(campaign);
  ResearchV58Tests.Staff(simulation);simulation.AgricultureResearch.Progress=0;
  if(captureMenu is "research-v58-active" or "research-v58-paused"){ResearchV58Tests.Advance(simulation,3600);OpenResearch();VerifyResearchMenuClicks();if(captureMenu=="research-v58-paused")simulation.ToggleResearch(0);}
  else{
   ResearchV58Tests.Advance(simulation,14400);ResearchV58Tests.GatherFood(simulation);OpenResearch();
   if(captureMenu=="research-v58-offer"){paused=true;return;}
   void Click(Rectangle rect){oldMouse=default;UpdateResearchMenu(default,new MouseState(rect.Center.X,rect.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released));}
   int food=simulation.World.Resources[0];Click(ExtraResearchButton);Click(ExtraResearchCancel);if(simulation.World.Resources[0]!=food||extraResearchChoosing)throw new InvalidDataException("Canceled research purchase spent food");
   Click(ExtraResearchButton);Click(ResearchRect(611,125,78,420));if(pendingResearchBranch!=1||simulation.World.Resources[0]!=food)throw new InvalidDataException("Extra selection must preview military without charge");
   if(captureMenu!="research-v58-choice"){
    Click(ExtraResearchConfirm);if(simulation.World.Resources[0]!=food-50||simulation.ActiveResearchBranch!=1)throw new InvalidDataException("Extra confirmation did not charge once");
    ResearchV58Tests.Advance(simulation,captureMenu=="research-v58-complete"?7200:3600);
   }
  }
  paused=true;researchMovies.Clear();CloseResearchMovie();
 }
}
