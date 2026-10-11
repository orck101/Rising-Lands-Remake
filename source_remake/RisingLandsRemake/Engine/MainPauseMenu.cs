using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 static readonly int[] MainMenuOrder={0,1,2,3,4,7,5,6};
 Rectangle MainMenuButton(int index){
  int[] rows={230,301,335,369,436,530,530,470};
  return MenuNativeRect(index==6?410:200,rows[index],index is 5 or 6?190:400,30);
 }
 void MoveMainMenu(int direction){int current=Array.IndexOf(MainMenuOrder,pauseMenu.Choice);int next=MainMenuOrder[(current+direction+MainMenuOrder.Length)%MainMenuOrder.Length];pauseMenu.Move(next-pauseMenu.Choice,MenuLabels.Length);}
 void DrawMainPauseMenu(){
  batch.Draw(white,GraphicsDevice.Viewport.Bounds,Color.Black);batch.Draw(nativeMenu,NativeMenuBounds,Color.White);
  batch.Draw(white,MenuNativeRect(180,178,440,391),new Color(18,14,10,155));
  MenuText("JOGO PAUSADO",193,gold,.95f);
  MenuText("PARTIDA",278,muted,.52f);MenuText("AJUSTES",413,muted,.52f);
  foreach(int index in MainMenuOrder){var rect=MainMenuButton(index);bool active=index==pauseMenu.Choice||rect.Contains(Mouse.GetState().Position);
   batch.Draw(white,rect,active?new Color(74,60,36,215):new Color(24,20,15,155));
   if(active||index==0)Outline(rect,gold);
   string label=MenuLabels[index];float scale=MenuScale*.8f*(index is 5 or 6?.7f:.82f);var measured=font.MeasureString(label)*scale;
   Text(label,(int)(rect.Center.X-measured.X/2),(int)(rect.Center.Y-measured.Y/2),active?gold:Color.Wheat,scale);
  }
  MenuText("Esc: continuar   |   Setas e Enter: selecionar",579,muted,.5f);
 }
}
