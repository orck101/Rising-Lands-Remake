using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 int lastBuildingClick=-1;long lastBuildingTick=-100;int viewedBuilding=-1;
 Building? ViewedBuilding=>simulation.World.Buildings.FirstOrDefault(b=>b.Slot==viewedBuilding);
 Building? BuildingHit(Point pointer){
  var pos=new Vector2(pointer.X,pointer.Y-Top)/zoom+camera;var direct=simulation.BuildingAt((int)(pos.Y/32)*256+(int)(pos.X/32));if(direct!=null&&KnownBuilding(direct))return direct;
  foreach(var b in simulation.World.Buildings.Where(KnownBuilding).OrderByDescending(b=>b.Cell>>8)){
   var rule=simulation.Rule(b.Type);var site=simulation.Constructions.FirstOrDefault(c=>c.Id==b.Slot);
   var commands=site==null?scene.Objects.Where(o=>Simulation.Inside(o.Cell,b.Cell,rule.Width,rule.Height)).SelectMany(o=>o.Commands):rule.Objects.SelectMany(o=>o.Commands).Select(c=>c with{X=c.X+(b.Cell&255)*32,Y=c.Y+(b.Cell>>8)*32});
   if(commands.Any(c=>sprites.TryGetValue(c.Sprite,out var texture)&&new Rectangle(c.X,c.Y,texture.Width,texture.Height).Contains((int)pos.X,(int)pos.Y)))return b;
  }return null;
 }
 Construction? ConstructionHit(Point pointer){
  var pos=new Vector2(pointer.X,pointer.Y-Top)/zoom+camera;int cell=(int)(pos.Y/32)*256+(int)(pos.X/32);
  var site=simulation.Constructions.Where(c=>!simulation.Complete(c)&&KnownCell(c.Cell)).OrderByDescending(c=>c.Cell>>8).FirstOrDefault(c=>Simulation.Inside(cell,c.Cell,simulation.Rule(c.Type).Width,simulation.Rule(c.Type).Height)||visuals.ConstructionFrame(c,simulation).SelectMany(o=>o.Commands).Any(d=>sprites.TryGetValue(d.Sprite,out var t)&&new Rectangle(d.X+(c.Cell&255)*32,d.Y+(c.Cell>>8)*32,t.Width,t.Height).Contains((int)pos.X,(int)pos.Y)));
  return site;
 }
 bool JoinConstructionAt(Point pointer){var site=ConstructionHit(pointer);if(site==null)return false;var builders=SelectedUnits().Where(u=>u.Owner==0&&u.Type==7).ToArray();if(builders.Length==0)return false;
  int joined=builders.Count(u=>simulation.ResumeConstruction(u,site));message=joined>0?"Construtores na obra: "+Simulation.Workers(site).Count()+". Cada trabalhador acelera a construcao.":"Sem acesso livre a esta obra.";return true;
 }
 bool OpenBuildingAt(Point pointer){var b=BuildingHit(pointer);if(b==null)return false;if(b.Owner!=0){int attackers=SelectedUnits().Count(a=>a.Owner==0&&simulation.AttackBuilding(a,b.Slot));if(attackers>0){message=attackers+" tropas atacando a construcao.";return true;}}
  if(b.Owner==0){int repairs=SelectedUnits().Count(u=>simulation.RepairBuilding(u,b.Slot));if(repairs>0){message=repairs+" construtor(es) reparando o prédio.";return true;}}
  var colonists=SelectedUnits().Where(u=>(u.Type==9||b.Type==2)&&u.Owner==0&&VisualCatalog.Visible(u)).ToArray();
  if(colonists.Length>0){int entered=simulation.EnterBuildingGroup(colonists,b.Slot);lastBuildingClick=-1;message=entered>0?entered+" personagens a caminho da construcao."+(entered<colonists.Length?" Os demais estao sem vaga ou acesso livre.":""):"Sem vaga ou acesso para os colonos selecionados.";return true;}
  if(lastBuildingClick==b.Slot&&draws-lastBuildingTick<=12){lastBuildingClick=-1;var colonist=b.Type==8?simulation.ReleaseExchangeUnit(b.Slot)??simulation.ReleaseResident(b.Slot):simulation.ReleaseResident(b.Slot);if(colonist!=null){message="Personagem retirado. Selecione-o para dar ordens.";}else message="Sem colono disponivel ou saida livre.";return true;}lastBuildingClick=b.Slot;lastBuildingTick=draws;
  resourceGesture.Cancel();resourceOrders.Cancel();viewedBuilding=b.Slot;BuildingSound(b,8);nativePage=0;selected=null;selectedSlots.Clear();dragStart=null;return true;}
 int[] BuildingRecruits(Building b)=>Simulation.ProductionTypes(b.Type);
 string RecruitName(int type)=>type switch{5=>"Fazendeiro",7=>"Construtor",6=>"Engenheiro",0=>"Espadachim",1=>"Incendiario",8=>"Arqueiro",_=>visuals.Name(new Actor{Type=type})};
 string RecruitCost(int type){var r=simulation.TrainingRule(type);return r.Food+" alimento"+(r.Rock>0?", "+r.Rock+" rocha":"")+(r.Parts>0?", "+r.Parts+" engr.":"");}

}
public sealed record WarehouseNetwork(int Count,int Food,int Rock,int Parts,int StoredFood,int StoredRock,int Capacity,int ActiveCount=0){
 public int Occupancy=>StoredFood+StoredRock;
 public int Free=>Math.Max(0,Capacity-Occupancy);
 public string Status=>Count==0?"Sem armazem":ActiveCount==0?"Inativo":Occupancy>Capacity?"Superlotado":Free==0?"Lotado":"Espaco livre";
}
public sealed partial class Simulation {
 public WarehouseNetwork WarehouseSummary(int owner=0){
  if(owner is <0 or >3)throw new ArgumentOutOfRangeException(nameof(owner));
  int count=World.Buildings.Count(b=>b.Owner==owner&&b.Type==3);int active=World.Buildings.Count(b=>b.Owner==owner&&b.Type==3&&Inhabited(b));
  // Use the same spendable food/stone totals as the top bar and warehouse withdrawals.
  return new(count,World.Resources[owner],World.Resources[4+owner],World.Resources[8+owner],World.Resources[owner],World.Resources[4+owner],100*active,active);
 }
 public int WarehouseOccupancy()=>WarehouseSummary().Occupancy;
 public int WarehouseSpace(int owner=0)=>WarehouseSummary(owner).Free;
}
