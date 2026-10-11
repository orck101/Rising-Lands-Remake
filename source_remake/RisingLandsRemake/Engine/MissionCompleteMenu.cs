using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
    Rectangle VictoryPanel=>new((GraphicsDevice.Viewport.Width-600)/2,(GraphicsDevice.Viewport.Height-300)/2,600,300);
    Rectangle NextMissionButton=>MenuRow(2);
    void UpdateMissionComplete(KeyboardState k,MouseState m){
        if(Press(k,Keys.F5)){OpenSlots(false);return;}
        if(Press(k,Keys.F9)){OpenSlots(true);return;}
        if(!simulation.Campaign.TransitionOpen)simulation.RequestNextMission();
        UpdateCampaignChoice(k,m);return;

        // All map orders, map switching and simulation updates stay blocked on this screen.
    }
    void VerifyNextMissionClick(){
        var previous=oldMouse;oldMouse=default;string before=simulation.Serialize();var r=NextMissionButton;
        UpdateMissionComplete(default,default);
        oldMouse=previous;if(!simulation.Campaign.TransitionOpen||!simulation.MissionWon)throw new InvalidDataException("Next mission must open original research transition");
    }
    void DrawMissionComplete(){
        if(!simulation.MissionWon||pauseMenu.Open)return;
        if(!simulation.Campaign.TransitionOpen)simulation.RequestNextMission();
        DrawCampaignChoice();return;

    }
    void DrawFirstMissionProgress(){
        if(!simulation.IsFirstMission||simulation.MissionWon)return;
        string status=$"MISSAO 1   Armazem na base: {(simulation.FirstMissionWarehouse?"OK":"pendente")}   Alimento: {simulation.World.Resources[0]}/50   Rocha: {simulation.World.Resources[4]}/50";
        var r=new Rectangle(12,Top+10,640,30);batch.Draw(white,r,new Color(18,27,24,215));Text(status,r.X+10,r.Y+5,gold,.65f);
    }
}
