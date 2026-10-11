using System.Text.Json;
using Microsoft.Xna.Framework;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareRangeUnionCapture(){
  var seed=simulation.World.Units.First(u=>u.Owner==0&&VisualCatalog.Visible(u));
  selectedSlots.Clear();selected=null;viewedBuilding=-1;paused=true;revealMap=true;
  bool separated=captureMenu=="range-v64-separated",moving=captureMenu=="range-v64-moved";
  int count=separated?2:36;
  for(int i=0;i<count;i++){
   var troop=JsonSerializer.Deserialize<Actor>(JsonSerializer.Serialize(seed))!;troop.Slot=10000+i;troop.Type=!separated&&i%5==0?0:8;troop.LegacyState=7;troop.Health=100;troop.Route.Clear();
   troop.X=(int)((separated?24+i*25:24+i%6*1.5)*1024);troop.Y=(int)((separated?25:22+i/6*1.5)*1024);
   simulation.World.Units.Add(troop);selectedSlots.Add(troop.Slot);selected??=troop;
  }
  // Register synthetic screenshot actors in a fresh, isolated simulation seed.
  simulation=new Simulation(simulation.World);
  UpdateRangeOutline();var cached=rangeArcs;UpdateRangeOutline();if(!ReferenceEquals(cached,rangeArcs))throw new InvalidDataException("Stationary selection must reuse cached union geometry");
  if(moving){var shifted=simulation.World.Units.First(u=>u.Slot==10001);shifted.X+=28*1024;UpdateRangeOutline();if(ReferenceEquals(cached,rangeArcs)||!rangeDisks.Any(c=>c.X==shifted.X/32.0+16))throw new InvalidDataException("Moving troop must invalidate range geometry");}
  selectedSlots.Remove(10000);UpdateRangeOutline();if(rangeDisks.Length!=count-1)throw new InvalidDataException("Deselection must remove its range");selectedSlots.Add(10000);UpdateRangeOutline();
  var circles=simulation.AttackRangeSelection(SelectedUnits()).Select(u=>new RangeDisk(u.X/32.0+16,u.Y/32.0+16,simulation.CombatRule(u.Type).Range*32)).ToArray();
  var arcs=RangeOutline.Build(circles);double perimeter=arcs.Sum(a=>(a.End-a.Start)*a.Circle.Radius);
  if(!separated&&perimeter>=circles.Sum(c=>c.Radius*RangeOutline.Tau)/3)throw new InvalidDataException("Dense range capture must eliminate internal circles");
  if(separated&&arcs.Count!=2)throw new InvalidDataException("Separated troops must show two full contours");
  zoom=separated?.6f:.8f;ClampCamera();float cx=separated?36.5f:28,cy=26;
  camera=new Vector2(cx*32-ViewWidth/(2*zoom),cy*32-ViewHeight/(2*zoom));ClampCamera();
  message=separated?"Tropas separadas: dois alcances independentes.":"36 tropas: somente o contorno externo do alcance combinado.";
 }
}
