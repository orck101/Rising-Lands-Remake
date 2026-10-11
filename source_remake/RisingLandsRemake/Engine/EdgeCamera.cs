using Microsoft.Xna.Framework;
namespace RisingLands;
public static class EdgeCamera {
 public static Vector2 Direction(Rectangle area,Point mouse,bool active,int margin=36){
  if(!active||!area.Contains(mouse))return Vector2.Zero;
  return new Vector2(mouse.X<area.Left+margin?-1:mouse.X>=area.Right-margin?1:0,mouse.Y<area.Top+margin?-1:mouse.Y>=area.Bottom-margin?1:0);
 }
}
public sealed partial class ResearchGame {
 Vector2 panVelocity;

 void PanCamera(Microsoft.Xna.Framework.Input.KeyboardState keys,Point mouse,float seconds){
  if(!IsActive||capture!=null){panVelocity=Vector2.Zero;return;}
  var direction=EdgeCamera.Direction(View,mouse,!showMini||!MiniRect.Contains(mouse));
  if(keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left)||keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))direction.X-=1;
  if(keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right)||keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))direction.X+=1;
  if(keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up)||keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))direction.Y-=1;
  if(keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down)||keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))direction.Y+=1;
  direction.X=Math.Clamp(direction.X,-1,1);direction.Y=Math.Clamp(direction.Y,-1,1);float edgeX=direction.X==0?0:Math.Clamp((36-Math.Min(mouse.X-View.Left,View.Right-1-mouse.X))/36f,0,1);
  float edgeY=direction.Y==0?0:Math.Clamp((36-Math.Min(mouse.Y-View.Top,View.Bottom-1-mouse.Y))/36f,0,1);
  bool horizontal=keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A)||keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D)||keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left)||keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right);
  bool vertical=keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W)||keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S)||keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up)||keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down);
  var target=new Vector2(direction.X*(horizontal?420:540*edgeX),direction.Y*(vertical?420:540*edgeY));
  float dt=Math.Clamp(seconds,0,.1f);panVelocity=Vector2.Lerp(panVelocity,target,1-MathF.Exp(-12*dt));camera+=panVelocity*(dt/zoom);ClampCamera();
 }
}
