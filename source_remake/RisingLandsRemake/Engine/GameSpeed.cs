using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 const int MaximumGameSpeed=3;
 int gameSpeed=1;bool speedDragging;
 Rectangle SpeedTrack=>NativeRect(657,578,128,7);
 Rectangle SpeedHit=>NativeRect(648,549,146,48);
 void AdvanceSimulation(){gameSpeed=Math.Clamp(gameSpeed,1,MaximumGameSpeed);if(paused||marketMenu&&MarketSession?.Stage!="Evaluating"||ResearchMovieOpen||testMenu)return;for(int i=0;i<gameSpeed&&!simulation.MissionWon&&researchMovies.Count==0;i++)simulation.Step();ClearHiddenSelection();}
 bool UpdateSpeedSlider(MouseState m){
  if(m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released&&SpeedHit.Contains(m.Position))speedDragging=true;
  if(!speedDragging)return false;
  if(m.LeftButton==ButtonState.Released){speedDragging=false;return true;}
  gameSpeed=Math.Clamp(1+(int)Math.Round((MaximumGameSpeed-1.0)*(m.X-SpeedTrack.Left)/Math.Max(1,SpeedTrack.Width)),1,MaximumGameSpeed);return true;
 }
 void DrawSpeedSlider(){
  if(pauseMenu.Open||researchMenu||simulation.MissionWon)return;
  var track=SpeedTrack;Text("Velocidade: "+gameSpeed+"x",SpeedHit.X+4,NativeRect(650,550,1,1).Y,gold,.52f*NativeScale);
  batch.Draw(white,track,new Color(66,66,60));
  for(int i=0;i<MaximumGameSpeed;i++)batch.Draw(white,new Rectangle(track.Left+track.Width*i/(MaximumGameSpeed-1),track.Y-2,2,track.Height+4),muted);
  int x=track.Left+track.Width*(gameSpeed-1)/(MaximumGameSpeed-1);batch.Draw(white,new Rectangle(x-5,track.Y-5,10,track.Height+10),gold);
 }
 void VerifySpeedSlider(){
  var previous=oldMouse;string before=simulation.Serialize();var comparison=simulation.OrderPreview();
  void Drag(int x){oldMouse=default;var r=SpeedHit;UpdateSpeedSlider(new MouseState(r.Center.X,r.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released));UpdateSpeedSlider(new MouseState(x,r.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released));UpdateSpeedSlider(default);}
  Drag(SpeedTrack.Left);if(gameSpeed!=1)throw new InvalidDataException("Speed lower limit");
  Drag(SpeedTrack.Center.X);if(gameSpeed!=2)throw new InvalidDataException("Speed middle value");
  Drag(SpeedTrack.Right+100);if(gameSpeed!=MaximumGameSpeed)throw new InvalidDataException("Speed upper limit");
  AdvanceSimulation();for(int i=0;i<MaximumGameSpeed;i++)comparison.Step();if(simulation.Serialize()!=comparison.Serialize())throw new InvalidDataException("Speed3 must match three simulation steps");
  var snapshot=simulation.Serialize();paused=true;AdvanceSimulation();paused=false;if(snapshot!=simulation.Serialize())throw new InvalidDataException("Speed must respect pause");
  simulation.Restore(before);oldMouse=default;var removed=MapTestButton;if(UpdateMapTestButton(new MouseState(removed.Center.X,removed.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released))||testMenu)throw new InvalidDataException("Removed test button must not open menu");oldMouse=previous;gameSpeed=MaximumGameSpeed;
 }
}
