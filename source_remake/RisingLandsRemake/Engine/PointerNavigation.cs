using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 Point? miniPosition,miniDragOffset;Point? middleDrag;
 Rectangle PositionedMini {get {int size=Math.Min(200,Math.Min(View.Width,View.Height));var pos=miniPosition??new Point(View.Right-size-16,View.Top+16);return new(Math.Clamp(pos.X,View.Left,View.Right-size),Math.Clamp(pos.Y,View.Top,View.Bottom-size),size,size);}}
 void CancelPointerNavigation(){miniDragOffset=null;middleDrag=null;panVelocity=Vector2.Zero;}
 bool UpdatePointerNavigation(MouseState mouse){
  if(!IsActive){CancelPointerNavigation();return false;}
  if(showMini&&mouse.RightButton==ButtonState.Pressed&&oldMouse.RightButton==ButtonState.Released&&MiniRect.Contains(mouse.Position)){miniDragOffset=mouse.Position-MiniRect.Location;middleDrag=null;dragStart=null;}
  if(miniDragOffset is {} offset){if(mouse.RightButton==ButtonState.Released){miniDragOffset=null;return true;}miniPosition=mouse.Position-offset;miniPosition=PositionedMini.Location;panVelocity=Vector2.Zero;return true;}
  if(mouse.MiddleButton==ButtonState.Pressed){if(middleDrag==null){if(!View.Contains(mouse.Position)||showMini&&MiniRect.Contains(mouse.Position))return false;middleDrag=mouse.Position;dragStart=null;}else{camera+=(middleDrag.Value.ToVector2()-mouse.Position.ToVector2())/zoom;middleDrag=mouse.Position;ClampCamera();}panVelocity=Vector2.Zero;return true;}
  if(middleDrag!=null){middleDrag=null;panVelocity=Vector2.Zero;return true;}return false;
 }
}
