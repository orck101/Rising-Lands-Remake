using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class Simulation {
 public bool IsSixthMission=>World.MapName.Equals("MONDE06.ETA",StringComparison.OrdinalIgnoreCase);
 // MONDE06.ETA: influence rectangle at 336680, (225,59)..(225,59).
 // Native evaluator 0x4586c7..0x4586ed requires current vision of its top-left cell.
 public const int SixthMissionTarget=59*256+225;
 static readonly (int X,int Y)[] SixthNavigationDirections={(0,-1),(-1,0),(1,0),(0,1),(-1,-1),(1,-1),(-1,1),(1,1)};
 // MONDE06 has natural passages connected diagonally; four-neighbour BFS isolates the target.
 bool SixthNavigationStep(Actor actor,int from,int to){
  if(!IsSixthMission||Airborne(actor)||Math.Abs((from&255)-(to&255))!=1||Math.Abs((from>>8)-(to>>8))!=1)return true;
  foreach(int side in new[]{(from&255)+(to>>8)*256,(to&255)+(from>>8)*256}){
   if(WallAt(side)||GateCell(side)&&!GatePassable(side)||BuildBlocks(side)||BuildingAt(side)!=null&&!Passable(side&255,side>>8)
    ||World.Collision[side]!=initialCollision[side]&&(World.Collision[side]&1)!=0)return false;
  }
  return true;
 }
 static bool SixthSurvivors(IEnumerable<Actor> actors)=>actors.Any(a=>a.Owner==0&&a.Health>0&&a.Type!=17&&a.LegacyState is not (0 or 4));
 public bool MissionLost=>IsFifthMission?FifthPlayerDefeatEnabled&&ClanEliminated(0):(IsSixthMission||IsSeventhMission||IsEighthMission||IsNinthMission)&&!SixthSurvivors(World.Units);
 bool SixthMissionSatisfied()=>SixthSurvivors(World.Units)&&InSight(SixthMissionTarget);
 bool SavedSixthMissionSatisfied(Save save,IEnumerable<Building> buildings){
  if(!SixthSurvivors(save.Units))return false;
  bool Sees(int x,int y,int radius){int dx=x-225,dy=y-59;return dx*dx+dy*dy<=radius*radius+(radius>=2?1:0);}
  return save.Units.Where(a=>a.Owner==0&&VisualCatalog.Visible(a)).Any(a=>Sees(a.X/1024,a.Y/1024,VisibilityData.Units.FirstOrDefault(r=>r.Type==a.Type)?.Radius??5))
   ||buildings.Where(b=>b.Owner==0).Any(b=>Sees((b.Cell&255)+Rule(b.Type).Width/2,(b.Cell>>8)+Rule(b.Type).Height/2,VisibilityData.Buildings.FirstOrDefault(r=>r.Type==b.Type)?.Radius??0))
   ||save.Special?.Zones.Any(z=>z.Owner==0&&z.Spell==0&&z.Until>save.Tick&&Sees(z.Cell&255,z.Cell>>8,Spell(0).Radius))==true;
 }
}
public sealed partial class ResearchGame {
 void UpdateMissionLost(KeyboardState k){CancelPointerNavigation();resourceGesture.Cancel();dragStart=null;if(Press(k,Keys.F9))OpenSlots(true);else if(Press(k,Keys.Escape)||Press(k,Keys.Enter))pauseMenu.Show(MenuPage.Main);}
 void DrawMissionLost(){if(!simulation.MissionLost||pauseMenu.Open)return;var r=VictoryPanel;batch.Draw(white,r,new Color(25,20,15,235));Outline(r,gold,2);Text($"MISSAO {simulation.MissionNumber} ENCERRADA",r.X+40,r.Y+45,gold,.95f);Text("Seu cla nao possui sobreviventes.",r.X+40,r.Y+105,Color.White,.7f);Text("Esc: menu para reiniciar ou carregar",r.X+40,r.Y+180,Color.Wheat,.65f);}
}
