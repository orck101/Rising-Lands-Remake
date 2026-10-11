using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public static class SelectionClasses {
 public static int[] Types(IEnumerable<Actor> units)=>units.Where(VisualCatalog.Visible).Select(u=>u.Type).Distinct().Order().ToArray();
 public static Actor[] Filter(IEnumerable<Actor> units,int type)=>units.Where(u=>u.Type==type&&VisualCatalog.Visible(u)).DistinctBy(u=>u.Slot).ToArray();
}
public sealed partial class ResearchGame {
 bool ClassSelectionBar=>ViewedBuilding==null&&(FormationBar||SelectedUnits().Count(VisualCatalog.Visible)>1);
 Rectangle ClassRect(int index,int count){int rows=count>8?2:1,columns=(count+rows-1)/rows;int width=Math.Min(58,(FormationBar?292:480)/Math.Max(1,columns));int height=rows==1?50:25;return NativeRect(8+(index%columns)*width,547+(index/columns)*26,width-2,height);}
 bool UpdateSelectionClasses(MouseState mouse){
  if(!ClassSelectionBar)return false;var units=SelectedUnits().ToArray();var types=SelectionClasses.Types(units);
  for(int i=0;i<types.Length;i++)if(ClassRect(i,types.Length).Contains(mouse.Position)){
   var kept=SelectionClasses.Filter(units,types[i]);selectedSlots.Clear();foreach(var u in kept)selectedSlots.Add(u.Slot);selected=kept.FirstOrDefault();nativePage=0;resourceGesture.Cancel();resourceOrders.Cancel();dragStart=null;buildMode=false;constructionMenu=false;foodTargetMode=0;
   message=kept.Length+" "+visuals.Name(kept[0])+" selecionado(s).";return true;
  }return true;
 }
 string DrawSelectionClasses(Point pointer,string hint){
  var units=SelectedUnits().Where(VisualCatalog.Visible).ToArray();var types=SelectionClasses.Types(units);
  for(int i=0;i<types.Length;i++){
   var cell=ClassRect(i,types.Length);int type=types[i];int sprite=UnitPortrait(type);
   if(sprites.TryGetValue(0x29b7,out var frame))batch.Draw(frame,cell,Color.White);
   if(sprites.TryGetValue(sprite,out var portrait)){float scale=Math.Min((cell.Width-4)/(float)portrait.Width,(cell.Height-4)/(float)portrait.Height);int width=(int)(portrait.Width*scale),height=(int)(portrait.Height*scale);batch.Draw(portrait,new Rectangle(cell.Center.X-width/2,cell.Center.Y-height/2,width,height),Color.White);}
   if(cell.Contains(pointer)){Outline(cell,gold);hint=visuals.Name(units.First(u=>u.Type==type))+": "+units.Count(u=>u.Type==type)+" selecionado(s). Clique para selecionar somente esta classe.";}
  }return hint;
 }
 void PrepareClassSelectionCapture(){
  for(int count=1;count<=26;count++){
   var cells=Enumerable.Range(0,count).Select(i=>ClassRect(i,count)).ToArray();var area=NativeRect(8,547,480,52);
   if(cells.Any(r=>!area.Contains(r))||cells.Where((r,i)=>cells.Skip(i+1).Any(r.Intersects)).Any())throw new InvalidDataException("Class portraits must fit without overlap or covering resource counters");
  }
  var units=simulation.World.Units.Where(u=>u.Owner==0&&VisualCatalog.Visible(u)).Take(6).ToArray();if(units.Length!=6)throw new InvalidDataException("Class capture requires six visible units");
  int[] types={5,5,7,8,6,9};SelectOnly(null);for(int i=0;i<units.Length;i++){units[i].Type=types[i];selectedSlots.Add(units[i].Slot);}selected=units[0];
  if(SelectionClasses.Types(SelectedUnits()).Length!=5)throw new InvalidDataException("Mixed selection must show five distinct class portraits");
  string before=simulation.Serialize();var farmer=Array.IndexOf(SelectionClasses.Types(SelectedUnits()),5);var rect=ClassRect(farmer,5);oldMouse=default;
  if(!UpdateNativeActions(new KeyboardState(),new MouseState(rect.Center.X,rect.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released))||SelectedUnits().Count()!=2||SelectedUnits().Any(u=>u.Type!=5)||simulation.Serialize()!=before)throw new InvalidDataException("Class portrait click must filter only existing farmers without changing world orders");
  if(captureMenu=="classes-v53"){selectedSlots.Clear();foreach(var u in units)selectedSlots.Add(u.Slot);selected=units[0];}
  oldMouse=default;paused=true;Center();message=captureMenu=="classes-v53"?"Grupo: fazendeiros, construtor, engenheiro, arqueiro e colono.":"Clique no retrato: somente os dois fazendeiros do grupo permanecem selecionados.";
 }
}
