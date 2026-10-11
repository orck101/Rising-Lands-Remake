using System.Text.Json;
namespace RisingLands;
public static class ImprovementsV91Tests {
 static int checks;static void Check(bool ok,string label){if(!ok)throw new Exception("FAIL v91: "+label);checks++;Console.WriteLine("PASS v91: "+label);}
 static void Step(Simulation s,int n){for(int i=0;i<n;i++)s.Step();}
 public static void Run(string root){
  var s=SpecialV66Tests.Load(root);var builder=SpecialV66Tests.Recruit(s,7);int depot=ProductionV65Tests.Staff(s,3);var b=s.World.Buildings.First(b=>b.Slot==depot);s.Opponent.BuildingDamage[b.Slot]=40;
  Check(s.RepairBuilding(builder,b.Slot),"builder accepts damaged owned building");Check(!s.RepairBuilding(s.World.Units.First(a=>a.Type==0),b.Slot),"farmer cannot repair");
  for(int i=0;i<3000&&s.Opponent.BuildingDamage[b.Slot]==40;i++)s.Step();Check(s.BuildingHealth(b)>s.BuildingMaxHealth(b)-40,"walk then restore health with original two-point strikes");
  Check(s.WorkingBuilder(builder)!=null,"original hammer animation active for repair");var copy=SpecialV66Tests.Load(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"active repair survives save/load");Step(s,30);Step(copy,30);Check(copy.Serialize()==s.Serialize(),"repair resumes deterministically");
  s.Stop(builder);Check(!s.Repairing(builder),"stop cancels repair");Check(s.RepairBuilding(builder,b.Slot),"repair restarts");Step(s,600);Check(s.BuildingHealth(b)==s.BuildingMaxHealth(b)&&!s.Repairing(builder),"repair finishes without overhealing");Check(!s.RepairBuilding(builder,b.Slot),"healthy building rejected");
  var helper=SpecialV66Tests.Recruit(s,7);s.Opponent.BuildingDamage[b.Slot]=40;Check(s.RepairBuilding(builder,b.Slot)&&s.RepairBuilding(helper,b.Slot),"two builders can repair one building at separate positions");
  for(int i=0;i<4000&&(s.WorkingBuilder(builder)==null||s.WorkingBuilder(helper)==null);i++)s.Step();Check(s.WorkingBuilder(builder)!=null&&s.WorkingBuilder(helper)!=null,"both repair workers reach work positions");copy.Restore(s.Serialize());copy.Stop(copy.World.Units.First(a=>a.Slot==helper.Slot));Step(s,80);Step(copy,80);Check(s.BuildingHealth(b)>copy.BuildingHealth(copy.World.Buildings.First(x=>x.Slot==b.Slot)),"additional builder accelerates repair");
  var catalog=JsonSerializer.Deserialize<BuildingDamageCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/building_damage_visuals.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;var warehouse=catalog.Buildings.First(v=>v.Type==3);
  Check(warehouse.Stages.Where(f=>f.State==5&&f.Damage==50).Select(f=>JsonSerializer.Serialize(f.Objects)).Distinct().Count()==32,"32 original damage smoke/fire animation frames");Check(warehouse.Stages.Where(f=>f.State==3).Select(f=>JsonSerializer.Serialize(f.Objects)).Distinct().Count()==26,"26 original clipped collapse frames");
  foreach(bool ranged in new[]{false,true})foreach(int owner in new[]{0,1}){
   var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);w=Simulation.TrainingWorld(w,ranged);Array.Fill(w.Collision,(byte)0);Array.Fill(w.Tiles,(ushort)349);Array.Clear(w.Objects);Array.Clear(w.Quantities);
   var defender=w.Units.First(a=>a.Owner==0&&a.Type==(ranged?8:0));var foe=w.Units.Single(a=>a.Owner==1);var friend=w.Units.First(a=>a.Owner==0&&a.Type==5);if(owner==1){defender.Owner=friend.Owner=1;foe.Owner=0;}
   defender.X=20*1024;defender.Y=20*1024;int radius=Math.Max(1,Simulation.CombatData.Rules.First(r=>r.Type==defender.Type).Range);int distance=ranged?(int)Math.Ceiling(radius*1.5):radius*3;foe.X=(20+distance)*1024;foe.Y=20*1024;friend.X=foe.X-1024;friend.Y=foe.Y;foe.AttackTarget=friend.Slot;
   var defense=new Simulation(w);defense.UpdateVisibility();Step(defense,10);Check(defender.AttackTarget==foe.Slot&&defender.AutomaticDefense,$"owner {owner} protects ally at {(ranged?"1.5x ranged":"3x melee")} perception");
  }
  Simulation Fifth(){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE05.ETA"));new VisualCatalog().Apply(w);return new(w);}
  var outer=Fifth();var guard=outer.World.Units.First(a=>a.Owner==1&&a.Type==0&&a.Health>0);var victim=outer.World.Units.First(a=>a.Owner==1&&a.Type==5);var attacker=outer.World.Units.First(a=>a.Owner==0&&a.Type==0);guard.X=20*1024;guard.Y=20*1024;victim.X=22*1024;victim.Y=guard.Y;attacker.X=23*1024;attacker.Y=guard.Y;attacker.AttackTarget=victim.Slot;outer.UpdateVisibility();Step(outer,30);Check(guard.AttackTarget==attacker.Slot&&guard.AutomaticDefense,"active enemy AI defends allied worker outside home base during peace period");
  var fifth=Fifth();int initialArchers=fifth.World.Units.Count(a=>a.Owner==0&&a.Type==8&&a.Health>0);Check(initialArchers>0,"mission five retains player's native archers");
  var owners=fifth.World.Units.Where(a=>a.Owner is >0 and <4).Select(a=>a.Owner).Distinct().ToArray();Check(owners.Length>0,"native enemies found in mission five");
  foreach(int owner in owners){Check(fifth.World.Resources[owner]==15&&fifth.World.Resources[4+owner]==10,"mission five owner "+owner+" starts 15 food / 10 stone");Check(!fifth.World.Units.Any(a=>a.Owner==owner&&a.Type==8&&a.Health>0),"no enemy starting archers owner "+owner);}
  var fifthCopy=Fifth();fifthCopy.Restore(fifth.Serialize());Check(fifthCopy.Serialize()==fifth.Serialize(),"mission five reduced opening saves");Step(fifth,18000);
  foreach(int owner in owners){Console.WriteLine($"GROWTH owner={owner} food={fifth.World.Resources[owner]} stone={fifth.World.Resources[4+owner]} deposited={fifth.Opponent.Deposited[owner]} recruits={fifth.Recruitments.Count(j=>j.Owner==owner&&j.Finished)} buildings={fifth.World.Buildings.Count(b=>b.Owner==owner)}");Check(fifth.Opponent.Deposited[owner]>0,"real gathering owner "+owner);Check(fifth.Recruitments.Any(j=>j.Owner==owner&&j.Finished)||fifth.Constructions.Any(c=>c.Owner==owner&&fifth.Complete(c)),"real expansion/training owner "+owner);}
  fifthCopy.Restore(fifth.Serialize());Check(fifthCopy.Serialize()==fifth.Serialize(),"all clan economies remain conserved and loadable");Console.WriteLine("ALL v91 IMPROVEMENTS PASSED: "+checks);
 }
}
