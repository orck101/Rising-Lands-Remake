namespace RisingLands;
public static class ViewportZoomTests {
 public static void Run(string root){
  int checks=0;
  foreach(var path in Directory.GetFiles(PackageLayout.Asset(root,"CD/WORLD"),"*.ETA")){
   var w=LegacyAssets.Map(path);
   foreach(var size in new[]{(800,600),(1280,800),(1920,1080),(2560,1440),(3440,1440),(5120,2880)}){
    int height=size.Item2-(int)Math.Ceiling(54*size.Item1/800f);
    foreach(float wanted in new[]{.01f,.5f,.75f,1f,1.5f,2f}){
     float zoom=ViewportZoom.Clamp(wanted,w.Width,w.Height,size.Item1,height);
     if(w.Width*32*zoom+0.001f<size.Item1||w.Height*32*zoom+0.001f<height)throw new Exception("FAIL: "+w.MapName+" zoom exposes blank area at "+size+" requested "+wanted);
     checks++;
    }
   }
  }
  float tiny=ViewportZoom.Clamp(.01f,8,8,5120,2800);if(tiny<20)throw new Exception("FAIL: tiny map must cover screen even above regular maximum zoom");
  Console.WriteLine("PASS: "+checks+" map/window/zoom combinations fill entire game viewport, including ultrawide and 5K");
  Console.WriteLine("PASS: exceptionally small maps raise both zoom limits to prevent empty space");
 }
}
