using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 Rectangle CampaignConfirm=>ResearchRect(28,475,310,36);
 void UpdateCampaignChoice(KeyboardState k,MouseState m){
  if(Press(k,Keys.Escape)){pauseMenu.Show(MenuPage.Main);return;}
  bool click=m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released;
  for(int col=0;col<4;col++)if(Press(k,(Keys)((int)Keys.D1+col))||click&&ResearchRect(405+103*col,125,78,420).Contains(m.Position)){simulation.ChooseNextResearch(ResearchColumns[col]);return;}
  if(Press(k,Keys.Enter)||click&&CampaignConfirm.Contains(m.Position)){if(simulation.Campaign.SelectedBranch<0){message="Escolha um ramo de tecnologia.";return;}StartNextMission();}
 }
 void VerifyCampaignClicks(){
  var previous=oldMouse;oldMouse=default;
  MouseState Click(Rectangle r)=>new(r.Center.X,r.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released);
  UpdateCampaignChoice(default,Click(CampaignConfirm));if(!simulation.MissionWon||!simulation.Campaign.TransitionOpen)throw new InvalidDataException("Confirmation requires branch selection");
  for(int col=0;col<4;col++){UpdateCampaignChoice(default,Click(ResearchRect(405+103*col,125,78,420)));if(simulation.Campaign.SelectedBranch!=ResearchColumns[col])throw new InvalidDataException("Original branch column hitbox mismatch");}
  UpdateCampaignChoice(default,Click(ResearchRect(405,125,78,420)));UpdateCampaignChoice(default,Click(CampaignConfirm));oldMouse=previous;
  if(simulation.World.MapName!="MONDE02.ETA"||simulation.Campaign.SelectedBranch!=0||selected!=null||selectedSlots.Count!=0)throw new InvalidDataException("Campaign confirmation must open second map without automatic selection");
 }
 void StartNextMission(){
  if(simulation.IsEighthMission&&simulation.NinthResearchChoiceError(simulation.Campaign.SelectedBranch) is {} error){message=error;return;}
  int number=simulation.MissionNumber+1;string filename=simulation.NextMissionMap;int next=Array.FindIndex(maps,p=>Path.GetFileName(p).Equals(filename,StringComparison.OrdinalIgnoreCase));if(next<0){message="Arquivo da proxima missao indisponivel.";return;}var progress=simulation.ProgressForNextMission();
  training=false;mapIndex=next;paused=false;LoadMap(progress);researchBranch=progress.SelectedBranch;pauseMenu.Show(MenuPage.Closed);message="Missao "+number+". Ramo escolhido: "+ResearchBranches[progress.SelectedBranch]+".";
 }
 void DrawCampaignChoice(){
  batch.Draw(white,GraphicsDevice.Viewport.Bounds,Color.Black);batch.Draw(nativeResearch,NativeMenuBounds,Color.White);
  var levels=simulation.EffectiveLevels;int selected=simulation.Campaign.SelectedBranch;DrawInventionAnimation(selected);
  for(int col=0;col<4;col++){int branch=ResearchColumns[col];for(int row=0;row<Math.Clamp(levels[branch],0,20);row++)batch.Draw(white,ResearchRect(442+103*col,425-15*row,5,11),gold);ResearchText(levels[branch].ToString(),430+103*col,459,gold,.85f);if(branch==selected)Outline(ResearchRect(409+103*col,495,72,48),gold);}
  ResearchText("MISSAO "+simulation.MissionNumber+" CONCLUIDA",28,115,gold,.8f);ResearchText("Tecnologia da proxima missao",28,145,Color.White,.65f);if(selected<0)ResearchText("Escolha a tecnologia",28,191,Color.White,.7f);
  if(simulation.IsSixthMission)ResearchText("Conhecimento espiritual encontrado!",28,165,gold,.55f);
  DrawCampaignBenefits(selected);
  if(simulation.IsEighthMission&&selected>=0&&simulation.NinthResearchChoiceError(selected)!=null)ResearchText("Ramo indisponivel: escolha outro",28,402,Color.Orange,.55f);
  ResearchText("Agricultura / Engenharia",28,425,Color.White,.62f);ResearchText("Armamento / Espiritualidade",28,446,Color.White,.62f);
  batch.Draw(white,CampaignConfirm,selected>=0?new Color(45,52,34):new Color(35,35,35));Outline(CampaignConfirm,selected>=0?gold:Color.Gray);ResearchText("Comecar missao "+(simulation.MissionNumber+1)+"  [Enter]",40,484,selected>=0?gold:Color.Gray,.68f);ResearchText("F5: salvar | F9: carregar | Esc: menu",28,525,Color.White,.55f);
  if(selected>=0)Outline(CampaignConfirm,gold);
 }
}
