namespace RisingLands;
public sealed partial class Simulation {
 double StructureFirepower(Actor a)=>Math.Max(0,CombatRule(a.Type).Damage[26]+MilitaryDamageBonus(a))*30.0/Math.Max(1,CombatRule(a.Type).Reload);
 Actor[] GreenAssaultWave(Actor[] candidates){
  int maximum=PolicyFor(3).MaximumWave;var chosen=new List<Actor>();
  void Add(Actor? actor){if(actor!=null&&chosen.Count<maximum&&!chosen.Contains(actor))chosen.Add(actor);}
  // Prefer support and demolition already available; never require locked classes.
  Add(candidates.Where(a=>a.Type==2).OrderByDescending(a=>a.Health).ThenBy(a=>a.Slot).FirstOrDefault());
  foreach(var a in candidates.Where(a=>a.Type!=2).OrderByDescending(StructureFirepower).ThenByDescending(DefensePower).ThenBy(a=>a.Slot).Take(2))Add(a);
  Add(candidates.Where(a=>CombatRule(a.Type).Range>1&&a.Type!=2&&!chosen.Contains(a)).OrderByDescending(DefensePower).ThenBy(a=>a.Slot).FirstOrDefault());
  Add(candidates.Where(a=>CombatRule(a.Type).Range<=1&&a.Type!=2&&!chosen.Contains(a)).OrderByDescending(DefensePower).ThenBy(a=>a.Slot).FirstOrDefault());
  foreach(var a in candidates.OrderByDescending(DefensePower).ThenBy(a=>a.Slot))Add(a);
  return chosen.ToArray();
 }
 bool GreenAssaultReady(Actor[] wave){
  int minimum=PolicyFor(3).MinimumWave;if(wave.Length<minimum)return false;
  double baseline=DefenseTypePower(0,TrainingRule(0).Health);
  if(wave.Sum(DefensePower)<baseline*minimum*.8||wave.Sum(StructureFirepower)<=0)return false;
  // Compare only currently observed defenders; knowledge never reveals hidden armies.
  var vision=OpponentVision(3);var defenders=World.Units.Where(a=>Hostile(3,a.Owner)&&FightingUnit(a)&&vision[ActorCell(a)]!=0).ToArray();
  var objective=World.Buildings.Where(b=>Hostile(3,b.Owner)&&vision[b.Cell]!=0).OrderBy(b=>CellDistance(b.Cell,ActorCell(wave[0]))).ThenBy(b=>b.Slot).FirstOrDefault();
  if(objective!=null){double opposition=defenders.Where(a=>a.Owner==objective.Owner&&CellDistance(ActorCell(a),objective.Cell)<=16).Sum(DefensePower);if(wave.Sum(DefensePower)<opposition*1.2)return false;}
  return true;
 }
 Building? GreenSiegeTarget(Actor a,byte[] vision)=>World.Buildings.Where(b=>Hostile(a.Owner,b.Owner)&&NearbyCells(b.Cell,Math.Max(Rule(b.Type).Width,Rule(b.Type).Height)).Any(c=>Inside(c,b.Cell,Rule(b.Type).Width,Rule(b.Type).Height)&&vision[c]!=0))
  .OrderBy(b=>CellDistance(ActorCell(a),b.Cell)/8).ThenBy(b=>b.Type is 1 or 5 or 18?0:b.Type is 2 or 3 or 4?1:2).ThenBy(b=>BuildingHealth(b)).ThenBy(b=>CellDistance(ActorCell(a),b.Cell)).ThenBy(b=>b.Slot).FirstOrDefault();
}
