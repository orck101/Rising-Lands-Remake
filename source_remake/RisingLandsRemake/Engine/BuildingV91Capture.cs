using Microsoft.Xna.Framework;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareBuildingV91Capture(){
  var b=simulation.World.Buildings.First(b=>b.Owner==0&&b.Type==2);simulation.Opponent.BuildingDamage[b.Slot]=simulation.BuildingMaxHealth(b)/2;revealMap=true;
  if(captureMenu=="repair-v91"){
   var a=simulation.World.Units.First(a=>a.Owner==0&&a.Type==7&&VisualCatalog.Visible(a));if(!simulation.RepairBuilding(a,b.Slot))throw new InvalidDataException("Repair capture inaccessible");
   for(int i=0;i<5000&&simulation.WorkingBuilder(a)==null;i++)simulation.Step();if(simulation.WorkingBuilder(a)==null)throw new InvalidDataException("Repair capture arrival failed");SelectOnly(a);viewedBuilding=-1;
  }else{simulation.DamageBuildingForResearch(b,simulation.BuildingMaxHealth(b));for(int i=0;i<64;i++)simulation.Step();viewedBuilding=-1;}
  camera=new Vector2((b.Cell&255)*32-ViewWidth/2f,(b.Cell>>8)*32-ViewHeight/2f);ClampCamera();paused=true;pauseMenu.Show(MenuPage.Closed);message=captureMenu=="repair-v91"?"Construtor reparando prédio danificado.":"Desmoronamento original: captura de teste.";
 }
}
