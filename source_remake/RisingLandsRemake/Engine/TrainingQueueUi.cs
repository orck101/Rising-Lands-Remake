using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 void DrawTrainingQueue(){
  if(ViewedBuilding is not {Owner:0} b)return;
  var active=simulation.Recruitments.FirstOrDefault(j=>j.BuildingSlot==b.Slot&&!j.Finished&&simulation.World.Units.Any(a=>a.Slot==j.SettlerSlot&&a.Health>0));
  var pending=simulation.PendingTraining(b.Slot).ToArray();if(active==null&&pending.Length==0)return;
  var items=(active==null?Array.Empty<int>():new[]{active.Type}).Concat(pending.Select(q=>q.Type)).ToArray();
  int size=28,spacing=34,y=NativeTop-72,width=Math.Min(12,items.Length)*spacing+74+(items.Length>12?52:0);
  batch.Draw(white,new Rectangle(12,y-5,width,36),new Color(20,20,20,225));
  Text("Fila",17,y+5,gold,.45f);
  for(int i=0;i<Math.Min(12,items.Length);i++){
   int x=62+i*spacing;if(sprites.TryGetValue(UnitPortrait(items[i]),out var portrait)){float scale=Math.Min(size/(float)portrait.Width,size/(float)portrait.Height);int w=(int)(portrait.Width*scale),h=(int)(portrait.Height*scale);batch.Draw(portrait,new Rectangle(x+(size-w)/2,y+(size-h)/2,w,h),Color.White);}
   if(i==0&&active!=null){Outline(new Rectangle(x,y,size,size),gold,2);Text((active.Progress*100/simulation.TrainingRule(active.Type).Duration)+"%",x,y-15,gold,.4f);}
   if(i<Math.Min(12,items.Length)-1)Text(">",x+size,y+6,Color.Wheat,.35f);
  }
  if(items.Length>12)Text("+"+(items.Length-12),62+12*spacing,y+6,gold,.45f);
 }
 void PrepareTrainingQueueCapture(){
  pauseMenu.Show(MenuPage.Closed);paused=true;viewedBuilding=simulation.World.Buildings.First(b=>b.Type==2&&b.Owner==0).Slot;selected=null;selectedSlots.Clear();
  foreach(int type in new[]{5,5,7,6,5}){
   var actions=NativeActions();int i=Array.FindIndex(actions,a=>a.Kind=="recruit"&&a.Value==type);nativePage=i/4;var r=NativeRect(31+60*(i%4),547,58,52);oldMouse=default;
   if(!UpdateNativeActions(default,new MouseState(r.Center.X,r.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released)))throw new InvalidDataException("Queue portrait click failed");
  }
  var before=simulation.PendingTraining(viewedBuilding).Count();var cancel=NativeRect(31,547,58,52);oldMouse=default;
  if(!UpdateNativeActions(default,new MouseState(cancel.Center.X,cancel.Center.Y,0,ButtonState.Released,ButtonState.Released,ButtonState.Pressed,ButtonState.Released,ButtonState.Released))||simulation.PendingTraining(viewedBuilding).Count()!=before-1)throw new InvalidDataException("Queue right-click cancellation failed");
  oldMouse=default;nativePage=0;message="Fila: clique esquerdo adiciona; direito cancela e devolve o custo.";
 }
}
