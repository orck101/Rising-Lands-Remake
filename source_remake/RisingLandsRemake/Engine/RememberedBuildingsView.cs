using Microsoft.Xna.Framework;
namespace RisingLands;
public sealed partial class ResearchGame {
 readonly Dictionary<int,(int Owner,bool Gray)> rememberedObjectColors=new();
 IEnumerable<StructureMemory> HiddenBuildingMemories()=>simulation.Exploration.Structures.Values.Where(m=>!SeenBuilding(m.Building));
 void DrawRememberedBuildingTiles(){if(revealMap)return;foreach(var m in HiddenBuildingMemories())if(m.Progress>=0)foreach(var tile in simulation.Rule(m.Building.Type).Tiles){int cell=m.Building.Cell+tile.Cell;if(simulation.InSight(cell)||!sprites.TryGetValue(tile.Tile,out var texture))continue;var p=ToScreen((cell&255)*32,(cell>>8)*32);batch.Draw(texture,new Rectangle((int)p.X,(int)p.Y,(int)Math.Ceiling(32*zoom),(int)Math.Ceiling(32*zoom)),Color.White);}}
 void PrepareSharedVisionCapture(){paused=true;pauseMenu.Show(MenuPage.Closed);simulation.Campaign.Alliances[1]=true;simulation.UpdateVisibility();var b=simulation.World.Buildings.First(b=>b.Owner==1&&b.Type==2);camera=new Vector2((b.Cell&255)*32-ViewWidth/2f,(b.Cell>>8)*32-ViewHeight/2f);ClampCamera();if(captureMenu=="v117-memory"){simulation.Campaign.Alliances[1]=false;simulation.UpdateVisibility();}message=captureMenu=="v117-memory"?"Alianca desfeita: territorio lembrado, sem visao atual.":"Alianca ativa: visao compartilhada.";}
 void AddRememberedBuildings(List<(float depth,SceneObject? obj,Actor? actor)> renderables){
  rememberedObjectColors.Clear();if(revealMap)return;
  foreach(var m in HiddenBuildingMemories()){var b=m.Building;var rule=simulation.Rule(b.Type);int ox=(b.Cell&255)*32,oy=(b.Cell>>8)*32;List<SceneObject> objects;
   if(b.Type==9)objects=new(){new(0,new(){new("sprite",0,0,7360+Math.Max(0,m.WallMask-1))})};
   else if(b.Type==12)objects=new(){new(0,GatePresentation.Commands(m.GateOrientation,m.Progress>=0&&m.Progress<rule.Duration?m.Progress*100/rule.Duration:m.GateFrame,m.Progress>=0&&m.Progress<rule.Duration).ToList())};
   else if(m.Progress>=0&&m.Progress<rule.Duration)objects=visuals.ConstructionFrame(new(){Type=b.Type,Progress=m.Progress},simulation);
   else if(m.Health<simulation.BuildingMaxHealth(b)){var visual=buildingDamageVisuals.Buildings.FirstOrDefault(v=>v.Type==b.Type);objects=visual?.Stages.Where(f=>f.State==5&&f.Phase==m.Seen/2%32&&f.Damage<=simulation.BuildingMaxHealth(b)-m.Health).OrderByDescending(f=>f.Damage).FirstOrDefault()?.Objects??rule.Objects;}
   else objects=rule.Objects;
   foreach(var o in objects){int cell=b.Cell+o.Cell;rememberedObjectColors[cell]=(b.Owner,!m.Occupied&&b.Type is not (9 or 12));renderables.Add(((cell>>8)*32+16,new SceneObject(cell,o.Commands.Where(c=>c.Kind!="gray").Select(c=>c with{X=c.X+ox,Y=c.Y+oy,Clip=c.Clip is {} clip?new DrawClip(clip.Left+ox,clip.Top+oy,clip.Right+ox,clip.Bottom+oy):null}).ToList()),null));}
  }
 }
}
