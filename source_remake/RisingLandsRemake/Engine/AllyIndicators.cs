using Microsoft.Xna.Framework;
namespace RisingLands;
public sealed partial class ResearchGame {
 readonly Dictionary<int,Vector2> allyBuildingAnchors=new();Simulation? allyAnchorSimulation;
 void DrawAllyIndicators(Actor? hovered){
  if(allyAnchorSimulation!=simulation){allyBuildingAnchors.Clear();allyAnchorSimulation=simulation;}
  foreach(var u in simulation.World.Units.Where(u=>u.Owner is >0 and <4&&simulation.Allied(u.Owner)&&VisualCatalog.Visible(u)&&SeenUnit(u)&&!VisualCatalog.HiddenByWork(u)))
   {var p=ToScreen(u.X/32f,u.Y/32f-20);if(u==hovered)AllyLabel(p,.38f);else{Outline(new Rectangle((int)p.X-5,(int)p.Y,6,5),Color.LightGreen,1);Outline(new Rectangle((int)p.X-1,(int)p.Y+2,6,5),gold,1);}}
  foreach(var b in simulation.World.Buildings.Where(b=>b.Owner is >0 and <4&&simulation.Allied(b.Owner)&&SeenBuilding(b))){
   if(!allyBuildingAnchors.TryGetValue(b.Slot,out var anchor)){
   var r=simulation.Rule(b.Type);var site=simulation.Constructions.FirstOrDefault(c=>c.Id==b.Slot);
   var commands=site==null?scene.Objects.Where(o=>Simulation.Inside(o.Cell,b.Cell,r.Width,r.Height)).SelectMany(o=>o.Commands):r.Objects.SelectMany(o=>o.Commands).Select(c=>c with{X=c.X+(b.Cell&255)*32,Y=c.Y+(b.Cell>>8)*32});
   var art=commands.Where(c=>c.Kind!="shadow"&&sprites.ContainsKey(c.Sprite)).ToArray();
   float left=art.Length>0?art.Min(c=>c.X):(b.Cell&255)*32,top=art.Length>0?art.Min(c=>c.Y):(b.Cell>>8)*32;
   float right=art.Length>0?art.Max(c=>c.X+sprites[c.Sprite].Width):left+r.Width*32;
   anchor=new Vector2((left+right)/2,top-18);allyBuildingAnchors[b.Slot]=anchor;}
   AllyLabel(ToScreen(anchor.X,anchor.Y),.45f);
  }
 }
 void AllyLabel(Vector2 p,float scale){string label="ALIADO";int width=(int)(font.MeasureString(label).X*scale);if(p.X+width/2<0||p.X-width/2>ViewWidth||p.Y<0||p.Y>ViewHeight)return;batch.Draw(white,new Rectangle((int)p.X-width/2-3,(int)p.Y-2,width+6,(int)(30*scale)+4),new Color(15,28,18,200));Text(label,(int)p.X-width/2,(int)p.Y,Color.LightGreen,scale);}
}
