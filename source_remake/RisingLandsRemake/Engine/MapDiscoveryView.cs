using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 bool revealMap;Color[] miniTerrain=Array.Empty<Color>();Color[] miniFog=Array.Empty<Color>();
 Rectangle MapTestButton=>NativeRect(648,529,146,19);
 bool SeenCell(int cell)=>revealMap||simulation.InSight(cell);
 bool KnownCell(int cell)=>simulation.CanInteractAt(cell,revealMap);
 bool KnownBuilding(Building b)=>SeenBuilding(b)&&Enumerable.Range(0,simulation.Rule(b.Type).Height).Any(y=>Enumerable.Range(0,simulation.Rule(b.Type).Width).Any(x=>KnownCell(b.Cell+y*256+x)));
 bool SeenUnit(Actor u)=>revealMap||simulation.UnitInSight(u);
 bool SeenBuilding(Building b)=>revealMap||simulation.BuildingInSight(b);
 string? VisiblePlacementError(Actor? worker,int type,int cell){if(!simulation.CanPlaceInExploredArea(type,cell,revealMap))return "Explore o local com uma unidade antes de construir.";return simulation.PlacementError(worker,type,cell);}
 void ClearHiddenSelection(){selectedSlots.RemoveWhere(slot=>simulation.World.Units.FirstOrDefault(u=>u.Slot==slot) is {} u&&!SeenUnit(u));if(selected!=null&&!SeenUnit(selected))selected=SelectedUnits().FirstOrDefault();if(ViewedBuilding is {} b&&!SeenBuilding(b))viewedBuilding=-1;}
 bool UpdateMapTestButton(MouseState mouse)=>false;
 void ToggleTestMap(){
  revealMap=!revealMap;ClearHiddenSelection();message=revealMap?"Teste: mapa revelado, sem alterar a exploracao.":"Teste: descoberta e visao normais restauradas.";
 }
 void DrawMapTestButton(){}
 void DrawMapFog(int minX,int minY,int maxX,int maxY){
  if(revealMap)return;
  for(int y=minY;y<maxY;y++)for(int x=minX;x<maxX;x++){int cell=y*256+x;if(simulation.InSight(cell))continue;var p=ToScreen(x*32,y*32);batch.Draw(white,new Rectangle((int)p.X,(int)p.Y,(int)Math.Ceiling(32*zoom),(int)Math.Ceiling(32*zoom)),simulation.Discovered(cell)?new Color(0,0,0,150):Color.Black);}
 }
 void UpdateMiniFog(){var w=simulation.World;if(miniTerrain.Length!=w.Width*w.Height)return;if(miniFog.Length!=miniTerrain.Length)miniFog=new Color[miniTerrain.Length];
  for(int y=0;y<w.Height;y++)for(int x=0;x<w.Width;x++){int n=y*w.Width+x,cell=y*256+x;miniFog[n]=revealMap||simulation.InSight(cell)?miniTerrain[n]:simulation.Discovered(cell)?new Color((byte)(miniTerrain[n].R*.4f),(byte)(miniTerrain[n].G*.4f),(byte)(miniTerrain[n].B*.4f),(byte)255):Color.Black;}
  mini.SetData(miniFog);
 }
 void PrepareDiscoveryCapture(){
  showMini=true;simulation.UpdateVisibility();
  string before=simulation.Serialize();var old=oldMouse;oldMouse=default;var r=MapTestButton;var click=new MouseState(r.Center.X,r.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released);
  ToggleTestMap();if(!revealMap||simulation.Serialize()!=before)throw new InvalidDataException("Reveal test must not change exploration or simulation");
  ToggleTestMap();if(revealMap||simulation.Serialize()!=before)throw new InvalidDataException("Hide test must preserve exploration and simulation");oldMouse=old;
  if(captureMenu=="discovery-v60-reveal")ToggleTestMap();
  else if(captureMenu=="discovery-v60-explored"){
   // Reveal a scout position, then return it: area remains known without live vision.
   var scout=simulation.World.Units.First(u=>u.Owner==0&&VisualCatalog.Visible(u));int x=scout.X,y=scout.Y;
   scout.X=simulation.World.Width/2*1024;scout.Y=simulation.World.Height/2*1024;simulation.UpdateVisibility();scout.X=x;scout.Y=y;simulation.UpdateVisibility();
   camera=new Vector2(simulation.World.Width*16-ViewWidth/(2*zoom),simulation.World.Height*16-ViewHeight/(2*zoom));ClampCamera();
  }
  message="Teste de descoberta: preto = desconhecido; escuro = explorado; claro = visao atual.";
  UpdateMiniFog();if(miniFog.Any(c=>c.A!=255))throw new InvalidDataException("Minimap must remain opaque in every fog state");
 }
}
