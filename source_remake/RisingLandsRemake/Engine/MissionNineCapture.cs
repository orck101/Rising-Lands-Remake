using Microsoft.Xna.Framework.Input;
using System.Text.Json;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareMissionNineCapture(){
  if(!simulation.IsNinthMission)throw new InvalidDataException("Mission-nine capture requires MONDE09A-D.ETA");
  pauseMenu.Show(MenuPage.Closed);paused=true;revealMap=false;
  simulation.Restore(File.ReadAllText(Path.GetFullPath(PackageLayout.Asset(root,"../../experiments/mission09-v125/natural-victory.snapshot.json"))));
  if(captureMenu=="v125-nine-victory"){simulation.Campaign.TransitionOpen=false;return;}
  if(captureMenu=="v125-nine-next"){
   string expected=simulation.NextMissionMap;oldKeys=default;oldMouse=default;UpdateCampaignChoice(new KeyboardState(Keys.Enter),default);
   if(simulation.World.MapName!=expected||simulation.MissionNumber!=10||simulation.Campaign.CompletedMission!=9||selected!=null||selectedSlots.Count!=0)throw new InvalidDataException("Mission-nine campaign UI failed to open correct next map");
   File.WriteAllText(Path.ChangeExtension(capture!,".json"),JsonSerializer.Serialize(new{Passed=true,Handler="UpdateCampaignChoice Enter",NextMap=expected,simulation.Campaign.CompletedMission,SelectionEmpty=true}));paused=true;
  }
 }
}
