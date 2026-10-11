using System.Reflection;using System.Text.Json;
namespace RisingLands;
public static class RecoveryV118Tests {
 static object? Call(Simulation s,string name,params object[] args)=>typeof(Simulation).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(s,args);
 public static void Run(string root,string save,string output){
  int count=0;void Check(bool ok,string label){if(!ok)throw new Exception(label);count++;Console.WriteLine("PASS "+label);}
  Simulation Snapshot(){var s=BaseDefenseTests.Load(root);s.Restore(File.ReadAllText(save));return s;}
  foreach(int mission in new[]{4,5,7,15,25}){
   var s=BaseDefenseTests.Load(root,mission);var row=Simulation.OpponentTechnologies.Missions.First(m=>m.Mission==mission);
   for(int owner=1;owner<=3;owner++)for(int branch=0;branch<4;branch++)Check(s.OpponentLevel(owner,branch)==Math.Min(EnemyResearchPolicy.Maximum(branch),row.Players[owner][branch]+(owner==3?2:0)),"initial mission-based technology "+mission+"/"+owner+"/"+branch);
   string json=s.Serialize();s.Restore(json);Check(s.Serialize()==json,"technology bonus save/load is idempotent "+mission);s.Restart();for(int branch=0;branch<4;branch++)Check(s.OpponentLevel(3,branch)==Math.Min(EnemyResearchPolicy.Maximum(branch),row.Players[3][branch]+2),"restart never stacks bonus "+mission+"/"+branch);
  }
  var red=Snapshot();Check(red.OpponentArmyTarget(1)>=red.BaseDefenseFor(1).GarrisonTarget+red.PolicyFor(1).MaximumWave,"red production includes defense plus offense");
  var yellow=Snapshot();var army=yellow.World.Units.Where(a=>a.Owner==2&&a.Health>0&&VisualCatalog.Visible(a)&&Simulation.AdvancedAttacks.Rules.Any(r=>r.Type==a.Type)).ToArray();
  var guards=(HashSet<int>)Call(yellow,"OffensiveGuardReservation",2,army)!;Check(guards.Count==yellow.BaseDefenseFor(2).Guards.Count,"no second independent garrison reservation");Check(yellow.OpponentArmyTarget(2)>army.Length,"wounded yellow does not cap legal replacement production");
  int recruitment=red.Recruitments.Count;int food=red.World.Resources[1];Call(red,"OpponentRecruit",1);Check(red.Recruitments.Count>recruitment,"saved red resumes recruitment past old military cap");Check(red.World.Resources[1]<food,"resumed military training pays food");
  var green=Snapshot();Check(green.Opponent.WarehouseExpansionRequested[3],"snapshot retains historical storage request");Check((bool)Call(green,"WarehouseEmergency",3)!,"productive recovery defers storage spending");Check(!(bool)Call(green,"WarehouseExpansionNeeded",3)!,"no warehouse reservation during recovery");Check(green.Opponent.WarehouseExpansionRequested[3],"recovery does not erase queued warehouse request");
  var crop=green.World.Buildings.First(b=>b.Owner==3&&b.Type==4);Check(((HashSet<int>)Call(green,"CropSupplierSlots",3,crop)!).Count==1,"single farmer may seed population recovery");Call(green,"PlanPopulation",3);var home=green.World.Buildings.First(b=>b.Owner==3&&b.Type==2);Check(green.ResidentTrips.Any(t=>t.Building==home.Slot),"spare secondary staff moves to train recovered farmer");Check(green.World.Buildings.Where(b=>b.Owner==3&&b.Type is 3 or 4).All(green.Inhabited),"recovery preserves storage and cultivation operators");
  var seed=Snapshot();var donor=seed.World.Buildings.First(b=>b.Owner==3&&b.Type==1);int spare=seed.World.BuildingResidents[donor.Slot].First(id=>seed.World.Units.Any(a=>a.Slot==id&&a.Type==9&&a.Health>0&&a.LegacyState==14));seed.World.BuildingResidents[donor.Slot].Remove(spare);seed.World.BuildingResidents[home.Slot].Add(spare);Check(((HashSet<int>)Call(seed,"CropSupplierSlots",3,seed.World.Buildings.First(b=>b.Owner==3&&b.Type==4))!).Count==0,"recovery banks food for farmer training once spare colonist exists");
  Check((bool)Call(seed,"RecoverWoundedSurplus",3)!,"starved green reconverts wounded offensive surplus through real sanctuary");Check(seed.ResidentTrips.Where(t=>t.Building==home.Slot).All(t=>!seed.BaseDefenseFor(3).Guards.ContainsKey(t.Slot)),"economic reconversion never removes assigned defenders");
  foreach(int owner in new[]{1,2}){var storage=Snapshot();int capacity=storage.WarehouseSummary(owner).Capacity;storage.World.Resources[owner]=(ushort)(capacity*7/10-1);storage.World.Resources[4+owner]=0;storage.Opponent.WarehouseExpansionRequested[owner]=false;Check(!(bool)Call(storage,"WarehouseExpansionNeeded",owner)!,"stable storage below 70 percent clan "+owner);storage.World.Resources[owner]++;Check((int)Call(storage,"PolicyConstruction",owner)! ==3,"stable storage at 70 percent resumes priority clan "+owner);}
  var projected=BaseDefenseTests.Load(root,5,3);var productive=projected.World.Units.First(a=>a.Owner==3&&a.Type==5&&a.Health>0);int workers=(int)Call(projected,"OpponentWorkers",3,5)!;projected.ResidentTrips.Add(new(){Slot=productive.Slot,Building=projected.World.Buildings.First(b=>b.Owner==3&&b.Type==2).Slot});Check((int)Call(projected,"OpponentWorkers",3,5)! ==workers-1,"pending reversions stop counting as productive farmers");
  var noEnemies=BaseDefenseTests.Load(root,6);for(int i=0;i<60;i++)noEnemies.Step();Check(!noEnemies.OpponentEnabled&&noEnemies.Opponent.Sorties.Count==0,"mission6 still has no enemies");
  var stable=Snapshot();for(int i=0;i<90;i++)stable.Step();string stored=stable.Serialize();var restored=Snapshot();restored.Restore(stored);Check(restored.Serialize()==stored,"legacy slot imports and persists current rules");for(int i=0;i<90;i++){stable.Step();restored.Step();}Check(stable.Serialize()==restored.Serialize(),"recovered legacy continuation deterministic");
  File.WriteAllText(output,JsonSerializer.Serialize(new{Passed=true,Checks=count,Limit="Controlled probes and legacy continuation; not manual gameplay"},new JsonSerializerOptions{WriteIndented=true}));
 }
}


