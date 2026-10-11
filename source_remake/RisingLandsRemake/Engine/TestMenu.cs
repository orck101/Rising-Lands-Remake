using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 bool testMenu;
 float TestScale=>Math.Min(GraphicsDevice.Viewport.Width/800f,GraphicsDevice.Viewport.Height/600f);
 Rectangle TestRect(int x,int y,int w,int h)=>new((GraphicsDevice.Viewport.Width-(int)(800*TestScale))/2+(int)(x*TestScale),(GraphicsDevice.Viewport.Height-(int)(600*TestScale))/2+(int)(y*TestScale),(int)Math.Ceiling(w*TestScale),(int)Math.Ceiling(h*TestScale));
 Rectangle TestPanel=>TestRect(206,75,388,424);
 Rectangle TestRow(int i)=>TestRect(220,124+i*32,360,27);
 Rectangle TestClose=>TestRect(549,87,30,26);
 static readonly int[] TestBranches={0,2,1,3};
 bool TestActionEnabled(int i)=>i<3?simulation.CanTestResource(i==0?1:i==1?2:0):i<7?simulation.CanTestTechnology(TestBranches[i-3]):true;
 void ActivateTestAction(int i){
  if(!TestActionEnabled(i))return;
  if(i<3){int kind=i==0?1:i==1?2:0;simulation.GrantTestResource(kind);message="Teste: +50 de "+(i==0?"comida":i==1?"pedra":"engrenagem")+".";}
  else if(i<7){int branch=TestBranches[i-3];simulation.GrantTestTechnology(branch);message="Teste: "+ResearchBranches[branch]+" agora no nivel "+simulation.ResearchState(branch).Index+".";}
  else if(i==7)ToggleTestMap();
  else if(i==8){simulation.CompleteTestMission();testMenu=false;buildMode=false;constructionMenu=false;researchMenu=false;resourceGesture.Cancel();resourceOrders.Cancel();message="Missao concluida pelo menu de testes.";}
 }
 void UpdateTestMenu(KeyboardState keys,MouseState mouse){
  if(Press(keys,Keys.Escape)){testMenu=false;return;}
  if(mouse.LeftButton!=ButtonState.Pressed||oldMouse.LeftButton!=ButtonState.Released)return;
  if(TestClose.Contains(mouse.Position)||MapTestButton.Contains(mouse.Position)||!TestPanel.Contains(mouse.Position)){testMenu=false;return;}
  for(int i=0;i<9;i++)if(TestRow(i).Contains(mouse.Position)){ActivateTestAction(i);return;}
 }
 void DrawTestMenu(){
  if(!testMenu)return;
  batch.Draw(white,GraphicsDevice.Viewport.Bounds,new Color(0,0,0,150));
  var p=TestPanel;batch.Draw(white,p,new Color(43,36,29));Outline(p,gold,2);
  Text("MENU DE TESTES",p.X+18,p.Y+14,gold,.62f*TestScale);
  var close=TestClose;Outline(close,gold);Text("X",close.X+9,close.Y+3,gold,.55f*TestScale);
  for(int i=0;i<9;i++){
   var r=TestRow(i);bool enabled=TestActionEnabled(i),hover=r.Contains(Mouse.GetState().Position);
   batch.Draw(white,r,hover&&enabled?new Color(80,66,41):new Color(52,46,37));Outline(r,enabled?gold:muted);
   string label=i switch{0=>"+50 comida",1=>"+50 pedra",2=>"+50 engrenagem",7=>revealMap?"Esconder mapa":"Mostrar mapa",8=>"Passar de fase",_=>"+1 nivel: "+ResearchBranches[TestBranches[i-3]]+" ("+simulation.ResearchState(TestBranches[i-3]).Index+")"};
   if(i>=3&&i<7&&!enabled)label+=" - maximo";
   Text(label,r.X+9,r.Y+4,enabled?Color.Wheat:muted,.46f*TestScale);
  }
  Text("Jogo pausado enquanto este menu esta aberto.",p.X+18,TestRect(220,429,1,1).Y,muted,.38f*TestScale);
  Text(message,p.X+18,TestRect(220,452,1,1).Y,gold,.36f*TestScale);
  Text("Esc ou X para fechar",p.X+18,TestRect(220,476,1,1).Y,muted,.4f*TestScale);
 }
 void PrepareTestMenuCapture(){
  var prior=oldMouse;oldMouse=default;
  MouseState Click(Rectangle r)=>new(r.Center.X,r.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released);
  if(!UpdateMapTestButton(Click(MapTestButton))||!testMenu)throw new InvalidDataException("Test button must open menu above speed slider");
  string before=simulation.Serialize();AdvanceSimulation();if(simulation.Serialize()!=before)throw new InvalidDataException("Test menu must pause simulation");
  for(int i=0;i<7;i++)UpdateTestMenu(default,Click(TestRow(i)));
  UpdateTestMenu(default,Click(TestRow(7)));if(!revealMap)throw new InvalidDataException("Test menu map reveal failed");
  UpdateTestMenu(default,Click(TestRow(7)));if(revealMap)throw new InvalidDataException("Test menu map hide failed");
  var restored=simulation.OrderPreview();if(restored.Serialize()!=simulation.Serialize())throw new InvalidDataException("Test actions must preserve save roundtrip");
  if(captureMenu is "test-v63-victory" or "test-v63-next"){
   UpdateTestMenu(default,Click(TestRow(8)));if(!simulation.MissionWon||testMenu||simulation.Campaign.TransitionOpen)throw new InvalidDataException("Test completion must show normal victory first");
   if(captureMenu=="test-v63-next"){int next=simulation.MissionNumber+1;VerifyNextMissionClick();simulation.ChooseNextResearch(0);StartNextMission();if(simulation.MissionNumber!=next||simulation.Tests.Victory||testMenu||pauseMenu.Open)throw new InvalidDataException("Debug victory must follow normal transition and reset test state in next map");}
  }else if(captureMenu=="test-v63-button")UpdateTestMenu(new KeyboardState(Keys.Escape),default);
  else message="Recursos e quatro niveis concedidos para teste.";
  oldMouse=prior;
 }
}
