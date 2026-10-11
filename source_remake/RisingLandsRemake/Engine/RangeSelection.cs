namespace RisingLands;
public sealed partial class Simulation {
 public IEnumerable<Actor> AttackRangeSelection(IEnumerable<Actor> selected)=>selected.Where(u=>VisualCatalog.Visible(u)&&u.Type is not (5 or 6 or 7 or 9 or 10 or 13 or 17)&&CombatData.Rules.Any(r=>r.Type==u.Type&&r.Range>0)).DistinctBy(u=>u.Slot);
}
public sealed partial class ResearchGame {
 void PrepareRangeV50(){
  var own=simulation.World.Units.Where(u=>u.Owner==0&&VisualCatalog.Visible(u)).Take(2).ToArray();if(own.Length<2)throw new InvalidDataException("Range capture needs two visible units");
  own[0].Type=0;own[0].X=24*1024;own[0].Y=24*1024;own[1].Type=8;own[1].X=own[0].X+1024;own[1].Y=own[0].Y;
  SelectOnly(own[0]);selectedSlots.Add(own[1].Slot);
  if(simulation.AttackRangeSelection(SelectedUnits()).Count()!=2||simulation.CombatRule(8).Range!=10)throw new InvalidDataException("Mixed selection must show both native attack ranges");
  paused=true;Center();message="Alcance: arqueiro 10 quadrados; espadachim 1 quadrado.";
 }
 void PrepareSingleRangeV51(){
  PrepareRangeV50();var unit=simulation.World.Units.First(u=>selectedSlots.Contains(u.Slot)&&u.Type==(captureMenu=="range-v51-melee"?0:8));SelectOnly(unit);
  if(simulation.AttackRangeSelection(SelectedUnits()).Single()!=unit||selectedSlots.Count!=1)throw new InvalidDataException("Individual combat selection must display exactly one range");
  if(captureMenu=="range-v51-zoom")zoom=1.5f;Center();message=unit.Type==8?"Arqueiro selecionado: alcance de 10 quadrados.":"Espadachim selecionado: alcance de 1 quadrado.";
 }
 RangeDisk[] rangeDisks=Array.Empty<RangeDisk>();List<RangeArc> rangeArcs=new();
 void UpdateRangeOutline(){
  var disks=simulation.AttackRangeSelection(SelectedUnits()).OrderBy(u=>u.Slot).Select(u=>new RangeDisk(u.X/32.0+16,u.Y/32.0+16,simulation.CombatRule(u.Type).Range*32)).ToArray();
  if(!disks.SequenceEqual(rangeDisks)){rangeDisks=disks;rangeArcs=RangeOutline.Build(disks);}
 }
 void DrawAttackRanges(){
  UpdateRangeOutline();
  foreach(var arc in rangeArcs){
   double span=arc.End-arc.Start;
   int dots=Math.Max(span>=RangeOutline.Tau-1e-9?16:1,(int)Math.Ceiling(span*arc.Circle.Radius*zoom/8));
   for(int i=0;i<dots;i++){
    double angle=arc.Start+(i+.5)*span/dots;
    var p=ToScreen((float)(arc.Circle.X+Math.Cos(angle)*arc.Circle.Radius),(float)(arc.Circle.Y+Math.Sin(angle)*arc.Circle.Radius));
    if(p.X<0||p.X>=ViewWidth||p.Y<Top||p.Y>=Top+ViewHeight)continue;
    batch.Draw(white,new Microsoft.Xna.Framework.Rectangle((int)p.X-1,(int)p.Y-1,2,2),new Microsoft.Xna.Framework.Color(237,220,167,170));
   }
  }
 }
}
