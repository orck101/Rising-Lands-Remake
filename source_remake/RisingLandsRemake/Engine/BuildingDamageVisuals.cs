using System.Text.Json;
namespace RisingLands;
public record BuildingDamageFrame(int State,int Damage,int Phase,List<SceneObject> Objects);
public record BuildingDamageVisual(int Type,int Limit,List<BuildingDamageFrame> Stages);
public record BuildingDamageCatalog(List<BuildingDamageVisual> Buildings);
public sealed partial class ResearchGame {
 readonly BuildingDamageCatalog buildingDamageVisuals=JsonSerializer.Deserialize<BuildingDamageCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/building_damage_visuals.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 List<SceneObject> DamageFrame(Building b,bool collapse=false){
  var visual=buildingDamageVisuals.Buildings.FirstOrDefault(v=>v.Type==b.Type);if(visual==null)return new();
  int damage=simulation.Opponent.BuildingDamage.GetValueOrDefault(b.Slot),phase=collapse?Math.Clamp((int)((simulation.Tick-simulation.Opponent.DestructionAt[b.Slot])/8),0,25):(int)(simulation.Tick/2%32);
  var frames=visual.Stages.Where(f=>f.State==(collapse?3:5)&&f.Phase==phase).ToArray();
  return (collapse?frames.FirstOrDefault():frames.Where(f=>f.Damage<=damage).OrderByDescending(f=>f.Damage).FirstOrDefault())?.Objects??new();
 }
 void AddDamagedBuildings(List<(float depth,SceneObject? obj,Actor? actor)> renderables){
  foreach(var b in simulation.World.Buildings.Where(b=>b.Type is not (9 or 12)&&simulation.BuildingHealth(b)<simulation.BuildingMaxHealth(b)).Concat(simulation.CollapsingBuildings()).Where(SeenBuilding)){
   int ox=(b.Cell&255)*32,oy=(b.Cell>>8)*32;foreach(var o in DamageFrame(b,simulation.DestroyedBuilding(b.Slot)))renderables.Add((((b.Cell+o.Cell)>>8)*32+16,new SceneObject(b.Cell+o.Cell,o.Commands.Where(c=>c.Kind!="gray").Select(c=>c with{X=c.X+ox,Y=c.Y+oy,Clip=c.Clip is {} clip?new DrawClip(clip.Left+ox,clip.Top+oy,clip.Right+ox,clip.Bottom+oy):null}).ToList()),null));
  }
 }
}
