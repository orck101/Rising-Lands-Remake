using System.Text.Json.Nodes;
namespace RisingLands;
public static class ProductionV65Tests {
 static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: v65 "+name);Console.WriteLine("PASS: v65 "+name);}
 public static Simulation Funded(string root){var s=ResearchV58Tests.Load(root);for(int i=0;i<24;i++)foreach(int k in new[]{0,1,2})s.GrantTestResource(k);for(int b=0;b<4;b++)while(s.GrantTestTechnology(b)){}return s;}
 public static int Staff(Simulation s,int type){int slot=ResearchTests.Build(s,type,false);var u=s.ReleaseResident(s.World.Buildings.First(b=>b.Owner==0&&b.Type==2).Slot)??throw new Exception("No colonist to staff production fixture");if(!s.EnterBuilding(u,slot))throw new Exception("No entry into production fixture");for(int i=0;i<10000&&s.ResidentTrips.Count>0;i++)s.Step();if(s.Occupants(slot)!=1)throw new Exception("Production fixture staffing failed");return slot;}
 public static void Run(string root){
  var expected=new Dictionary<int,int[]>{{1,new[]{0,1,2,8}},{2,new[]{5,6,7}},{5,new[]{11,12,13,14,15}},{13,new[]{3,25}},{15,new[]{4}},{16,new[]{10}},{18,new[]{16}},{20,new[]{4}}};
  foreach(int building in Enumerable.Range(0,22))Check(Simulation.ProductionTypes(building).SequenceEqual(expected.GetValueOrDefault(building,Array.Empty<int>())),"native production menu order "+building);
  var visuals=new VisualCatalog();foreach(var rule in Simulation.RecruitData.Rules)Check(visuals.Catalog.Types.Any(v=>v.Type==rule.Type&&v.Frames.Count==16*v.FrameCount),"produced troop has original directional frames "+rule.Type);
  foreach(var entry in expected){foreach(int troop in entry.Value){
   var s=Funded(root);int slot=entry.Key==2?s.World.Buildings.First(b=>b.Owner==0&&b.Type==2).Slot:Staff(s,entry.Key);
   int wrongSlot=entry.Key==2?Staff(s,1):s.World.Buildings.First(b=>b.Owner==0&&b.Type==2).Slot;var wrong=s.World.Buildings.First(b=>b.Slot==wrongSlot);string unchanged=s.Serialize();Check(!s.Recruit(troop,wrong.Slot)&&s.Serialize()==unchanged,"wrong producer rejects without cost "+entry.Key+"/"+troop);
   var price=s.TrainingRule(troop);int food=s.World.Resources[0],rock=s.World.Resources[4],parts=s.World.Resources[8],count=s.World.Units.Count(a=>a.Type!=17);
   Check(s.CanRecruit(troop,slot)&&s.Recruit(troop,slot),"correct staffed producer starts "+entry.Key+"/"+troop);
   Check(s.World.Resources[0]==food-price.Food&&s.World.Resources[4]==rock-price.Rock&&s.World.Resources[8]==parts-price.Parts,"original resource costs charged once "+troop);
   Check(!s.Recruit(troop,slot),"occupied producer rejects duplicate order "+troop);
   int housed=s.Occupants(slot),capacity=s.ResidentCapacity(s.World.Buildings.First(b=>b.Slot==slot));s.Step();Check(s.Recruitments.Last().Progress==101+housed*100/capacity,"training rate uses original housed population and producer capacity "+entry.Key+"/"+troop);
   string pending=s.Serialize();var copy=ResearchV58Tests.Load(root);copy.Restore(pending);Check(copy.Serialize()==pending,"pending production save roundtrip "+entry.Key+"/"+troop);
   for(int i=0;i<10000&&!s.Recruitments.Last().Finished;i++)s.Step();var job=s.Recruitments.Last();var actor=s.World.Units.First(u=>u.Slot==job.SettlerSlot);
   Check(job.Finished&&actor.Type==troop&&VisualCatalog.Visible(actor)&&s.World.Units.Count(a=>a.Type!=17)==count,"colono becomes troop and exits producer "+entry.Key+"/"+troop);
   Check(visuals.Frame(actor,s).Count>0,"produced troop is rendered "+troop);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"finished production roundtrip "+entry.Key+"/"+troop);
  }}
  var empty=Funded(root);int workshop=ResearchTests.Build(empty,5,false);Check(!empty.CanRecruit(11,workshop),"empty workshop disables production");
  var legacy=Funded(root);int barracks=Staff(legacy,1);Check(!legacy.CanRecruit(11,barracks),"workshop vehicle excluded from new barracks orders");
  // Construct an old-format legacy barracks order from a valid equivalent workshop order.
  int work=Staff(legacy,5);legacy.Recruit(11,work);var old=JsonNode.Parse(legacy.Serialize())!;old["Version"]=26;var j=old["Recruitments"]!.AsArray().Last()!;int resident=j["SettlerSlot"]!.GetValue<int>();j["BuildingSlot"]=barracks;j["SourceBuildingSlot"]=barracks;j["TravelCell"]=legacy.World.Buildings.First(b=>b.Slot==barracks).Cell;j.AsObject().Remove("LegacyBarracks");old["Residents"]![work.ToString()]!.AsArray().RemoveAt(0);old["Residents"]![barracks.ToString()]!.AsArray().Add(resident);
  var migrated=ResearchV58Tests.Load(root);migrated.Restore(old.ToJsonString());Check(migrated.Recruitments.Last().LegacyBarracks,"old incorrect barracks production is preserved explicitly during migration");var saved=ResearchV58Tests.Load(root);saved.Restore(migrated.Serialize());Check(saved.Serialize()==migrated.Serialize(),"migrated legacy order survives new-format save");
  var invalid=JsonNode.Parse(legacy.Serialize())!;invalid["Recruitments"]!.AsArray().Last()!["BuildingSlot"]=barracks;string prior=legacy.Serialize();try{legacy.Restore(invalid.ToJsonString());throw new Exception("Invalid producer accepted");}catch(InvalidDataException){}Check(legacy.Serialize()==prior,"new save cannot switch production to wrong building");
  Console.WriteLine("ALL PRODUCTION v65 TESTS PASSED");
 }
}
