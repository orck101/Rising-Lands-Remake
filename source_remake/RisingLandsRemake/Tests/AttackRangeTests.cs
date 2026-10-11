namespace RisingLands;
public static class AttackRangeTests {
 static void Check(bool value,string name){if(!value)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
 public static void Run(string root){
  foreach(string map in new[]{"MONDE01.ETA","MONDE02.ETA"}){
   var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD",map));new VisualCatalog().Apply(w);var s=new Simulation(w);var unit=w.Units.First(u=>u.Owner==0&&VisualCatalog.Visible(u));
   unit.Type=0;Check(s.AttackRangeSelection(new[]{unit}).SingleOrDefault()==unit,map+": selecting melee soldier displays attack range");
   unit.Type=8;Check(s.AttackRangeSelection(new[]{unit}).SingleOrDefault()==unit&&s.CombatRule(unit.Type).Range==10,map+": selecting archer displays native ten-tile range");
   foreach(int type in new[]{0,1,2,3,4,8,11,12,14,15,16,18,19,20,21,22,23,24,25}){unit.Type=type;Check(s.AttackRangeSelection(new[]{unit}).SingleOrDefault()==unit,map+": combat type "+type+" has selection range");}
   foreach(int type in new[]{5,6,7,9,10,13,17}){unit.Type=type;Check(!s.AttackRangeSelection(new[]{unit}).Any(),map+": noncombat type "+type+" has no attack circle");}
   unit.Type=8;unit.Owner=1;Check(s.AttackRangeSelection(new[]{unit}).Any(),map+": selected enemy troop also displays range");
   unit.Health=0;Check(!s.AttackRangeSelection(new[]{unit}).Any(),map+": defeated troop has no range circle");
   unit.Health=1;unit.LegacyState=14;Check(!s.AttackRangeSelection(new[]{unit}).Any(),map+": resident has no range circle");
  }
 }
}
