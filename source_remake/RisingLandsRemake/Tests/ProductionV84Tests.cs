namespace RisingLands;
public static class ProductionV84Tests {
 public static void Run(string root){int count=0;void Check(bool ok,string text){if(!ok)throw new Exception("FAIL v84: "+text);count++;Console.WriteLine("PASS v84: "+text);}
  var s=OpponentV75Tests.Load(root);var archer=s.World.Units.Single(a=>a.Owner==1&&a.Type==8);int slot=archer.Slot;
  Check(s.OpponentLevel(1,1)==2,"mission four starts at military level two");
  Check(!s.OpponentUnitUnlocked(1,8),"seed archer does not bypass technology");
  Check(s.OpponentUnitUnlocked(1,0),"ordinary soldiers remain unlocked");
  Check(!s.OpponentUnitUnlocked(1,13),"seed food cart does not bypass technology either");
  for(int i=0;i<6*60*30;i++)s.Step();
  Check(!s.Recruitments.Any(j=>j.Owner==1&&j.Type==8),"actual enemy training never produces locked archers");
  Check(s.Recruitments.Any(j=>j.Owner==1&&j.Type==0&&j.Finished),"enemy still trains legal combat troops");
  Check(s.World.Units.Any(a=>a.Slot==slot&&a.Type==8),"original archer remains in the world");
  var copy=OpponentV75Tests.Load(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"stock and recruitment save reload unchanged");
  var boundary=OpponentV75Tests.Load(root);boundary.Campaign.EnemyLevels[1][1]=6;Check(!boundary.OpponentUnitUnlocked(1,8),"archer stays locked before discovery MILITAIRE6");
  boundary.Campaign.EnemyLevels[1][1]=7;Check(boundary.OpponentUnitUnlocked(1,8),"actual discovery unlocks archer at military level seven");
  var unlocks=System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,int[]>>(File.ReadAllText(PackageLayout.FromBase("Content/Data/production_unlocks.json")))!;
  foreach(int owner in Enumerable.Range(1,3))foreach(int type in Enumerable.Range(0,26)){
   bool expected=Simulation.UnlockData.Technologies.Any(t=>t.Index<boundary.OpponentLevel(owner,t.Branch)&&unlocks.GetValueOrDefault(t.Key,Array.Empty<int>()).Contains(type));
   Check(boundary.OpponentUnitUnlocked(owner,type)==expected,"technology-only production owner="+owner+" type="+type);
  }
  Console.WriteLine("PASS v84 total "+count);
 }
}
