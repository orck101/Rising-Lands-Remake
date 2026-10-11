using Microsoft.Xna.Framework;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareV72Capture(){
  var s=ImprovementsV67Tests.Fixture(root);for(int i=0;i<4;i++)while(s.GrantTestTechnology(i)){}for(int j=0;j<30;j++)foreach(int k in new[]{0,1,2})s.GrantTestResource(k);
  LoadMap(restored:s);paused=true;revealMap=true;showMini=true;pauseMenu.Show(MenuPage.Closed);researchMovies.Clear();CloseResearchMovie();
  var a=s.World.Units.First(a=>a.Type==7&&a.Owner==0);a.X=39*1024;a.Y=39*1024;int anchor=40+40*256;
  foreach(var (cell,delta) in new[]{(anchor,1),(anchor+8,256)}){
   if(s.PlanWall(a,cell-delta*2,cell+delta*2)!=5)throw new InvalidDataException("Capture wall");for(int i=0;i<12000&&s.Constructions.Any(c=>!s.Complete(c));i++)s.Step();
   if(!s.StartConstruction(a,12,cell))throw new InvalidDataException("Capture gate");for(int i=0;i<12000&&s.Constructions.Any(c=>!s.Complete(c));i++)s.Step();
  }
  if(captureMenu=="v72-gates-open"){
   var gate=s.Constructions.First(c=>c.Type==12);viewedBuilding=gate.Id;nativePage=0;oldMouse=default;var button=NativeRect(31,547,58,52);var mouse=new Microsoft.Xna.Framework.Input.MouseState(button.Center.X,button.Center.Y,0,Microsoft.Xna.Framework.Input.ButtonState.Pressed,Microsoft.Xna.Framework.Input.ButtonState.Released,Microsoft.Xna.Framework.Input.ButtonState.Released,Microsoft.Xna.Framework.Input.ButtonState.Released,Microsoft.Xna.Framework.Input.ButtonState.Released);
   if(!UpdateNativeActions(default,mouse)||!gate.GateOpenTarget)throw new InvalidDataException("Gate native button click");foreach(var other in s.Constructions.Where(c=>c.Type==12&&c.Id!=gate.Id))s.ToggleGate(other.Id);for(int i=0;i<20;i++)s.Step();
  }
  var troop=s.World.Units.First(a=>a.Type==0&&a.Owner==0);troop.X=43*1024;troop.Y=43*1024;troop.Route.Clear();troop.Satiety=1;SelectOnly(troop);
  if(captureMenu=="v72-gate-menu"){selected=null;selectedSlots.Clear();viewedBuilding=s.Constructions.First(c=>c.Type==12).Id;message="Selecione o portao e clique no icone original para abrir ou fechar.";}
  camera=new Vector2(44*32-ViewWidth/2f,41*32-ViewHeight/2f);ClampCamera();if(captureMenu!="v72-gate-menu")message="Portoes originais nas duas orientacoes; fome da tropa selecionada.";
 }
}
