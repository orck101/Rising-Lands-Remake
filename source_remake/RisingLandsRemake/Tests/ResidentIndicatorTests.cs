using System.Text.Json;
namespace RisingLands;
public static class ResidentIndicatorTests {
 static void Check(bool v,string n){if(!v)throw new Exception("FAIL: "+n);Console.WriteLine("PASS: "+n);}
 public static void Run(string root){
  using var doc=JsonDocument.Parse(File.ReadAllText(PackageLayout.FromBase("Content/Data/building_resident_visuals.json")));
  foreach(var row in doc.RootElement.GetProperty("rows").EnumerateArray()){
   int type=row.GetProperty("type").GetInt32(),occupied=row.GetProperty("occupied").GetInt32();var anchor=doc.RootElement.GetProperty("anchors").GetProperty(type.ToString());
   var actual=BuildingResidentVisuals.Commands(new Building(0,type,0,0),occupied);var expected=row.GetProperty("commands").EnumerateArray().ToArray();
   Check(actual.Count==expected.Length&&actual.Select((c,i)=>c.Sprite==expected[i].GetProperty("sprite").GetInt32()&&c.X==expected[i].GetProperty("x").GetInt32()+anchor.GetProperty("x").GetInt32()&&c.Y==expected[i].GetProperty("y").GetInt32()+anchor.GetProperty("y").GetInt32()).All(v=>v),"resident indicator matches native draw vector type "+type+" occupancy "+occupied);
  }
  var s=MissionOneTests.Load(root);var home=s.World.Buildings.First(b=>b.Type==2&&b.Owner==0);
  Check(s.Occupants(home.Slot)==8&&BuildingResidentVisuals.Commands(home,8).Count==16,"sanctuary shows eight occupied and eight empty original places");
  Check(s.Recruit(5)&&s.Occupants(home.Slot)==8,"training reservation keeps its physical colonist in occupancy indicator");
  var worker=s.ReleaseResident(home.Slot)!;Check(s.Occupants(home.Slot)==7,"colonist exit immediately updates physical occupancy");
  int slot=ResearchTests.Build(s,3,false);var depot=s.World.Buildings.First(b=>b.Slot==slot);
  Check(s.Occupants(slot)==0&&BuildingResidentVisuals.Commands(depot,0).All(c=>c.Sprite==0x2a3b),"new warehouse displays two empty original places");
  Check(s.EnterBuilding(worker,slot)&&s.Occupants(slot)==0,"walking colonist is not counted inside before arrival");
  for(int i=0;i<10000&&s.ResidentTrips.Count>0;i++)s.Step();Check(s.Occupants(slot)==1,"arrival changes warehouse to one occupied place");
  var second=s.ReleaseResident(home.Slot)!;Check(s.EnterBuilding(second,slot),"second colonist can enter warehouse");for(int i=0;i<10000&&s.ResidentTrips.Count>0;i++)s.Step();
  Check(s.Occupants(slot)==2&&BuildingResidentVisuals.Commands(depot,2).All(c=>c.Sprite==0x2a3c),"full warehouse displays two occupied original places");
  string saved=s.Serialize();var restored=MissionOneTests.Load(root);restored.Restore(saved);Check(restored.Occupants(slot)==2,"full occupancy survives save/load");
  s.ReleaseResident(slot);s.ReleaseResident(slot);Check(s.Occupants(slot)==0,"all colonists leaving restores empty indicator");
 }
}
