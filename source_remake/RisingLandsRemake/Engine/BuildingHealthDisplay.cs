using Microsoft.Xna.Framework;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareHealthV88Capture(){
  pauseMenu.Show(MenuPage.Closed);var b=simulation.World.Buildings.First(b=>b.Owner==0&&b.Type==2);
  simulation.Opponent.BuildingDamage[b.Slot]=simulation.BuildingMaxHealth(b)/2;
  viewedBuilding=b.Slot;camera=new Vector2((b.Cell&255)*32-ViewWidth/2f,(b.Cell>>8)*32-ViewHeight/2f);ClampCamera();paused=true;message="Teste visual: estrutura com metade da vida.";
 }
 void DrawBuildingHealth(Point pointer,bool showAll){
  var hover=View.Contains(pointer)?BuildingHit(pointer):null;
  foreach(var b in simulation.World.Buildings.Where(b=>SeenBuilding(b)&&(showAll||simulation.BuildingHealth(b)<simulation.BuildingMaxHealth(b)||b.Slot==viewedBuilding||b==hover||simulation.World.Units.Any(a=>a.Owner==0&&simulation.Opponent.Sieges.TryGetValue(a.Slot,out var job)&&job.Building==b.Slot)))){
   var r=simulation.Rule(b.Type);var site=simulation.Constructions.FirstOrDefault(c=>c.Id==b.Slot);
   var commands=site==null?scene.Objects.Where(o=>Simulation.Inside(o.Cell,b.Cell,r.Width,r.Height)).SelectMany(o=>o.Commands):r.Objects.SelectMany(o=>o.Commands).Select(c=>c with{X=c.X+(b.Cell&255)*32,Y=c.Y+(b.Cell>>8)*32});
   var art=commands.Where(c=>c.Kind!="shadow"&&sprites.ContainsKey(c.Sprite)).ToArray();
   float left=art.Length>0?art.Min(c=>c.X):(b.Cell&255)*32,top=art.Length>0?art.Min(c=>c.Y):(b.Cell>>8)*32;
   float right=art.Length>0?art.Max(c=>c.X+sprites[c.Sprite].Width):left+r.Width*32;
   var pos=ToScreen((left+right)/2,top);int width=54,hp=simulation.BuildingHealth(b),max=simulation.BuildingMaxHealth(b);
   batch.Draw(white,new Rectangle((int)pos.X-width/2,(int)pos.Y-9,width,5),new Color(35,35,35));
   batch.Draw(white,new Rectangle((int)pos.X-width/2,(int)pos.Y-9,hp*width/Math.Max(1,max),5),b.Owner==0?Color.LightGreen:Color.IndianRed);
  }
 }
}
