using System.Text.Json;
namespace RisingLands;
public static class CombatV88Tests {
 static int checks;
 static void Check(bool ok,string label){checks++;if(!ok)throw new Exception("FAIL v88: "+label);Console.WriteLine("PASS v88: "+label);}
 static void Steps(Simulation s,int n){for(int i=0;i<n;i++)s.Step();}
 public static void Run(string root){
  foreach(var rule in Simulation.AdvancedAttacks.Rules){
   var s=SpecialV66Tests.Load(root,true);var guard=s.World.Units.First(u=>u.Owner==1&&u.Type==0);guard.LegacyState=14;var a=SpecialV66Tests.Recruit(s,rule.Type);guard.LegacyState=7;
   guard.X=a.X+1024;guard.Y=a.Y;guard.Health=100;s.UpdateVisibility();
   if(s.CombatRule(a.Type).Damage[guard.Type]<=0){Check(!s.Attack(a,guard),"native zero damage rejects direct attack troop "+rule.Type);continue;}
   Check(s.Attack(a,guard),"attack order troop "+rule.Type);
   var visual=new VisualCatalog();a.AttackCooldown=s.CombatCycle(a);var first=visual.Frame(a,s).ToArray();a.AttackCooldown=1;var last=visual.Frame(a,s).ToArray();a.AttackCooldown=0;
   Check(rule.FrameCount==1?first.SequenceEqual(last):!first.SequenceEqual(last),"original attack poses and frame count troop "+rule.Type);
   Steps(s,s.ReleaseTicks(a)+8);Check(guard.Health<100,"attack hits guard troop "+rule.Type);
   var building=s.World.Buildings.First(b=>b.Owner==1&&b.Type==16);guard.Health=0;s.Stop(guard);a.AttackCooldown=0;
   var r=s.Rule(building.Type);int cell=Enumerable.Range(0,65536).Where(c=>(c&255)<s.World.Width&&(c>>8)<s.World.Height&&s.Passable(c&255,c>>8)&&!Simulation.Inside(c,building.Cell,r.Width,r.Height)).OrderBy(c=>Math.Abs((c&255)-(building.Cell&255))+Math.Abs((c>>8)-(building.Cell>>8))).First();a.X=(cell&255)*1024;a.Y=(cell>>8)*1024;s.UpdateVisibility();
   if(s.CombatRule(a.Type).Damage[26]<=0){Check(!s.AttackBuilding(a,building.Slot),"original class cannot damage structures "+rule.Type);continue;}
   Check(s.AttackBuilding(a,building.Slot),"building order without AI mission troop "+rule.Type);
   int health=s.BuildingHealth(building);bool flight=false;
   for(int i=0;i<400&&s.BuildingHealth(building)==health;i++){s.Step();flight|=s.Arrows.Any(p=>p.SourceSlot==a.Slot&&p.TargetBuilding==building.Slot);if(flight){var restored=SpecialV66Tests.Load(root,true);restored.Restore(s.Serialize());Check(restored.Serialize()==s.Serialize(),"building projectile saves troop "+rule.Type);flight=false;}}
   Check(s.BuildingHealth(building)<health&&s.BuildingHealth(building)>=0,"building loses real health troop "+rule.Type);
   s.Stop(a);var copy=SpecialV66Tests.Load(root,true);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"damaged building save/load troop "+rule.Type);
  }
  var duel=SpecialV66Tests.Load(root,true);var own=duel.World.Units.First(a=>a.Owner==0&&a.Type==0&&VisualCatalog.Visible(a));var enemy=duel.World.Units.First(a=>a.Owner==1&&a.Type==0&&VisualCatalog.Visible(a));enemy.X=own.X+1024;enemy.Y=own.Y;duel.UpdateVisibility();int hp=own.Health;duel.Attack(own,enemy);Steps(duel,50);Check(enemy.Health<100&&own.Health<hp,"guard against guard retaliates and both take damage");
  var demolition=SpecialV66Tests.Load(root,true);var soldier=demolition.World.Units.First(a=>a.Owner==0&&a.Type==0&&VisualCatalog.Visible(a));var market=demolition.World.Buildings.First(b=>b.Slot==700);var sr=demolition.Rule(market.Type);int ground=Enumerable.Range(0,65536).Where(c=>(c&255)<demolition.World.Width&&(c>>8)<demolition.World.Height&&demolition.Passable(c&255,c>>8)&&!Simulation.Inside(c,market.Cell,sr.Width,sr.Height)).OrderBy(c=>Math.Abs((c&255)-(market.Cell&255))+Math.Abs((c>>8)-(market.Cell>>8))).First();soldier.X=(ground&255)*1024;soldier.Y=(ground>>8)*1024;foreach(var u in demolition.World.Units.Where(u=>u.Owner==1&&VisualCatalog.Visible(u))){u.Health=0;demolition.Stop(u);}Check(demolition.AttackBuilding(soldier,700),"melee demolition accepted");for(int i=0;i<8000&&!demolition.DestroyedBuilding(700);i++)demolition.Step();Check(demolition.DestroyedBuilding(700)&&!demolition.World.Buildings.Contains(market)&&demolition.DestroyedAt(market.Cell),"building dies and removes collision and active attacks");var loaded=SpecialV66Tests.Load(root,true);loaded.Restore(demolition.Serialize());Check(loaded.Serialize()==demolition.Serialize(),"destroyed building persists");
  var ranged=SpecialV66Tests.Load(root,true);var foe=ranged.World.Units.First(a=>a.Owner==1&&a.Type==0);foe.LegacyState=14;var archer=SpecialV66Tests.Recruit(ranged,8);foe.LegacyState=7;foe.Health=0;ranged.Stop(foe);var target=ranged.World.Buildings.First(b=>b.Slot==700);var tr=ranged.Rule(target.Type);
  int far=Enumerable.Range(0,65536).Where(c=>(c&255)<ranged.World.Width&&(c>>8)<ranged.World.Height&&ranged.Passable(c&255,c>>8)&&Math.Abs((c&255)-(target.Cell&255))+Math.Abs((c>>8)-(target.Cell>>8))>=18).OrderBy(c=>Math.Abs((c&255)-(target.Cell&255))+Math.Abs((c>>8)-(target.Cell>>8))).First();archer.X=(far&255)*1024;archer.Y=(far>>8)*1024;ranged.UpdateVisibility();Check(ranged.AttackBuilding(archer,target.Slot),"ranged siege begins outside weapon range");for(int i=0;i<4000&&ranged.BuildingHealth(target)==ranged.BuildingMaxHealth(target);i++)ranged.Step();int dx=Math.Abs(archer.X/1024-Math.Clamp(archer.X/1024,target.Cell&255,(target.Cell&255)+tr.Width-1)),dy=Math.Abs(archer.Y/1024-Math.Clamp(archer.Y/1024,target.Cell>>8,(target.Cell>>8)+tr.Height-1));Check(ranged.BuildingHealth(target)<ranged.BuildingMaxHealth(target)&&dx*dx+dy*dy>4,"archer approaches a firing position instead of touching structure");
  Console.WriteLine("ALL v88 COMBAT TESTS PASSED: "+checks);
 }
}
