using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 Rectangle ExtraResearchShortcut=>NativeRect(648,490,146,33);
 bool ExtraOfferReady=>simulation.ExtraResearchOffered&&simulation.World.Resources[0]>=Simulation.ResearchExtraFood;
 void BeginExtraResearchChoice(){OpenResearch();extraResearchChoosing=true;pendingResearchBranch=-1;message="Escolha um ramo nas quatro colunas. Custo: 50 alimentos.";}
 bool UpdateExtraResearchShortcut(MouseState mouse){
  if(!simulation.ExtraResearchOffered||mouse.LeftButton!=ButtonState.Pressed||oldMouse.LeftButton!=ButtonState.Released||!ExtraResearchShortcut.Contains(mouse.Position))return false;
  if(ExtraOfferReady)BeginExtraResearchChoice();else message="Pesquisa extra: faltam "+(50-simulation.World.Resources[0])+" alimentos.";
  return true;
 }
 void DrawExtraResearchShortcut(){
  if(pauseMenu.Open||researchMenu||constructionMenu||testMenu||simulation.MissionWon||!simulation.ExtraResearchOffered)return;
  var r=ExtraResearchShortcut;batch.Draw(white,r,ExtraOfferReady?new Color(72,57,29):new Color(42,39,35));Outline(r,ExtraOfferReady?gold:muted,2);
  Text("Pesquisa extra",r.X+6,r.Y+4,ExtraOfferReady?Color.Wheat:muted,.42f*NativeScale);
  Text("50 de alimento",r.X+6,r.Y+(int)(17*NativeScale),ExtraOfferReady?gold:muted,.34f*NativeScale);
 }
 void DrawExtraResearchOffer(){
  var r=ExtraResearchButton;bool ready=ExtraOfferReady;
  batch.Draw(white,r,ready?new Color(75,57,31):new Color(44,37,30));Outline(r,ready?gold:muted,2);
  ResearchText(simulation.Research.ExtraPurchased?"Pesquisa extra adquirida":"PESQUISA EXTRA - 50 alimentos",38,461,ready?Color.Wheat:muted,.6f);
  string state=simulation.Research.ExtraPurchased?(simulation.MissionResearchCount>=3?"Utilizada nesta missao":"Terceira pesquisa em andamento"):simulation.MissionResearchCount<2?"Conclua as duas pesquisas obrigatorias":ready?"Clique para escolher a terceira descoberta":"Faltam "+Math.Max(0,50-simulation.World.Resources[0])+" alimentos";
  ResearchText(state,38,489,ready?gold:muted,.5f);
 }
}
