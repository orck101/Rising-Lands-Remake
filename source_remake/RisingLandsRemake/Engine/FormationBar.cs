using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 FormationShape? selectedFormation;string formationGroup="";
 bool FormationBar=>ViewedBuilding==null&&SelectedUnits().Count(VisualCatalog.Visible)>1&&simulation.CanFormTroops(SelectedUnits());
 string FormationGroupKey()=>string.Join(",",SelectedUnits().Select(u=>u.Slot).Order());
 Rectangle FormationRect(int index)=>NativeRect(304+60*index,547,58,52);
 static string FormationName(FormationShape shape)=>shape switch{FormationShape.Line=>"Linha",FormationShape.Triangle=>"Triangulo",_=>"Quadrado"};
 bool UpdateFormationBar(MouseState mouse){if(!FormationBar)return false;for(int i=0;i<3;i++)if(FormationRect(i).Contains(mouse.Position)){
  var shape=(FormationShape)i;if(simulation.FormTroops(SelectedUnits(),shape)){selectedFormation=shape;formationGroup=FormationGroupKey();message="Formacao: "+FormationName(shape)+". Maior alcance no centro.";}else message="Sem espaco ou caminho para organizar todo o grupo.";return true;
 }return false;}
 string DrawFormationBar(Point pointer,string hint){if(!FormationBar)return hint;for(int i=0;i<3;i++){
  var r=FormationRect(i);NativeSprite(0x29b7,304+i*60,547);var positions=TroopFormations.Positions(9,(FormationShape)i);float step=i==1?2.5f:5f;
  foreach(var p in positions)batch.Draw(white,new Rectangle(r.Center.X+(int)(p.X*step*NativeScale)-2,r.Y+(int)(20*NativeScale)+(int)(p.Y*step*NativeScale),Math.Max(2,(int)(3*NativeScale)),Math.Max(2,(int)(3*NativeScale))),gold);
  Text(FormationName((FormationShape)i),r.X+3,r.Bottom-(int)(14*NativeScale),Color.Wheat,.39f*NativeScale);
  if(selectedFormation==(FormationShape)i&&formationGroup==FormationGroupKey())Outline(r,gold);
  if(r.Contains(pointer))hint="Organizar em "+FormationName((FormationShape)i)+": maior alcance no centro; menor alcance nas bordas.";
 }return hint;}
 int MoveSelectedTroops(int x,int y){if(FormationBar&&selectedFormation is {} shape&&formationGroup==FormationGroupKey())return simulation.FormTroops(SelectedUnits(),shape,y*256+x)?SelectedUnits().Count():0;return simulation.OrderGroup(SelectedUnits(),x,y);}
 void PrepareFormationCapture(){
  var units=simulation.World.Units.Where(u=>u.Owner==0&&VisualCatalog.Visible(u)).Take(9).ToArray();if(units.Length<4)throw new InvalidDataException("Formation capture requires troops");
  selectedSlots.Clear();for(int i=0;i<units.Length;i++){units[i].Type=i<2?8:0;selectedSlots.Add(units[i].Slot);}selected=units[0];viewedBuilding=-1;
  int index=captureMenu!.EndsWith("line")?1:captureMenu.EndsWith("triangle")?2:0;var r=FormationRect(index);oldMouse=default;
  var mouse=new MouseState(r.Center.X,r.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released);
  if(!UpdateNativeActions(default,mouse)||selectedFormation!=(FormationShape)index)throw new InvalidDataException("Formation UI click failed");
  for(int i=0;i<20000&&units.Any(u=>u.Route.Count>0);i++)simulation.Step();if(units.Any(u=>u.Route.Count>0))throw new InvalidDataException("Formation capture did not settle");paused=true;Center();
 }
}
