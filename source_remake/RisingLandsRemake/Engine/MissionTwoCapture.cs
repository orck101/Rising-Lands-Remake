using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareMissionTwoCapture(){
  if(!simulation.IsSecondMission)throw new InvalidDataException("Mission-two capture must start in MONDE02");MissionTwoTests.PlayToVictory(simulation);
  if(captureMenu=="mission2-v55-victory")return;
  VerifyNextMissionClick();if(!simulation.ChooseNextResearch(2))throw new InvalidDataException("Mission two branch selection failed");
  if(captureMenu=="mission2-v55-third"){
   oldMouse=default;var r=CampaignConfirm;UpdateCampaignChoice(default,new MouseState(r.Center.X,r.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released));oldMouse=default;
   if(simulation.World.MapName!="MONDE03.ETA"||simulation.Campaign.CompletedMission!=2||simulation.Campaign.SelectedBranch!=2||selected!=null||selectedSlots.Count!=0||pauseMenu.Open)throw new InvalidDataException("Confirm button must open mission three directly without unit selection");
  }
 }
}
