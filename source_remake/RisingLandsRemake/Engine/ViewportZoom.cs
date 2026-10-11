namespace RisingLands;
public static class ViewportZoom {
 public static float Clamp(float requested,int mapWidth,int mapHeight,int viewWidth,int viewHeight){
  float fit=MathF.BitIncrement(Math.Max(viewWidth/(mapWidth*32f),viewHeight/(mapHeight*32f)));
  float minimum=Math.Max(.5f,fit);return Math.Clamp(requested,minimum,Math.Max(2f,minimum));
 }
}
public sealed partial class ResearchGame {
 void ChangeZoom(float change,Microsoft.Xna.Framework.Point anchor){float previous=zoom;zoom=ViewportZoom.Clamp(zoom+change,simulation.World.Width,simulation.World.Height,ViewWidth,ViewHeight);camera+=new Microsoft.Xna.Framework.Vector2(anchor.X,anchor.Y-Top)*(1/previous-1/zoom);ClampCamera();}
 void PrepareZoomCapture(){
  for(int i=0;i<100;i++)ChangeZoom(-.25f,View.Center);
  camera=captureMenu=="zoom-v54-right"?new Microsoft.Xna.Framework.Vector2(float.MaxValue,float.MaxValue):Microsoft.Xna.Framework.Vector2.Zero;ClampCamera();
  if(simulation.World.Width*32*zoom<ViewWidth||simulation.World.Height*32*zoom<ViewHeight)throw new InvalidDataException("Minimum zoom exposes empty area");
  paused=true;SelectOnly(null);message="Zoom minimo ajustado ao mapa: o cenario preenche toda a area de jogo.";
 }
}
