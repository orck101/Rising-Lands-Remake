using System.Text.Json;
using System.Text.Json.Nodes;
namespace RisingLands;
public static class EnvironmentV62Tests {
 static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: v62 "+name);Console.WriteLine("PASS: v62 "+name);}
 public static void Run(string root){
  var s=MissionOneTests.Load(root);var builder=s.World.Units.First(a=>a.Owner==0&&a.Type==7);int site=Enumerable.Range(0,65536).First(c=>s.PlacementError(builder,3,c)==null);
  int builderX=builder.X,builderY=builder.Y;builder.X=(site&255)*1024;builder.Y=(site>>8)*1024;Check(s.PlacementError(builder,3,site)!=null,"occupied footprint rejects even own constructor under current placement rules");builder.X=builderX;builder.Y=builderY;
  ushort previous=s.World.Tiles[site];foreach(ushort tile in Enumerable.Range(16,15)){s.World.Tiles[site]=tile;Check(s.PlacementError(builder,3,site)==null,"green transition tile accepts building "+tile);}s.World.Tiles[site]=previous;
  var other=s.World.Units.First(a=>a.Owner==0&&a.Type==5);int oldX=other.X,oldY=other.Y;other.X=(site&255)*1024;other.Y=(site>>8)*1024;Check(s.PlacementError(builder,3,site)!=null,"another unit still blocks construction");other.X=oldX;other.Y=oldY;
  byte code=s.World.Objects[site];s.World.Objects[site]=1;Check(!s.Passable(site&255,site>>8)&&s.PlacementError(builder,3,site)!=null,"original tree obstruction applies to movement and building placement");s.World.Objects[site]=code;
  Check(EnvironmentPresentation.Data.Visuals.Where(v=>v.Code<=11).All(v=>v.Commands.Count>=2),"native trees contain original image and shadow in every extracted phase");
  var scene=new VisualCatalog().Scene(s.World);Check(scene.Objects.Any(o=>o.ObjectCode==67)&&EnvironmentPresentation.DynamicCode(67),"initial piles are identifiable and excluded from static scene even after load");
  foreach(int kind in new[]{0,1,2}){
   var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);int cell=Enumerable.Range(0,65536).First(c=>w.Objects[c]==0&&s.Passable(c&255,c>>8)&&s.BuildingAt(c)==null);w.Objects[cell]=(byte)(66+kind);w.Quantities[cell]=1;
   var unit=w.Units.First(a=>a.Owner==0&&a.Type==(kind==2?7:5));if(kind==0)unit.Type=6;var sim=new Simulation(w);unit.X=(cell&255)*1024;unit.Y=(cell>>8)*1024;Check(sim.Gather(unit,cell),"one-item ground pile receives worker order "+kind);for(int i=0;i<20&&unit.Food==0;i++)sim.Step();Check(unit.Food==1&&w.Objects[cell]==0&&!sim.IsResourcePile(cell),"last resource removes pile art and collection source "+kind);
  }
  var before=s.Serialize();var copyS=MissionOneTests.Load(root);copyS.Restore(before);Check(copyS.Serialize()==before,"environment state starts with exact save roundtrip");
  s.Step();int soil=Enumerable.Range(0,65536).First(c=>s.GrowingSoil(c)&&s.World.Objects[c]==0&&s.BuildingAt(c)==null&&(s.World.Collision[c]&1)==0);
  Check(s.SpawnPlant(soil,0)&&s.World.Objects[soil]==29&&s.EnvironmentState.GeneratedFood==1,"native food seed creates first visual stage and records new food");Check(!s.SpawnPlant(soil,1),"new food never overwrites an occupied cell");
  string planted=s.Serialize();copyS.Restore(planted);Check(copyS.Serialize()==planted,"new food and growth accounting survive save/load");
  long startClock=s.EnvironmentState.NativeTicks;for(int i=0;i<2000;i++){s.Step();copyS.Step();}Check(s.Serialize()==copyS.Serialize(),"food growth randomness resumes deterministically after save/load");Check(s.EnvironmentState.NativeTicks-startClock==2500&&s.EnvironmentState.GeneratedFood>0,"growth clock advances exactly 1.25 times at base game speed");
  var old=JsonNode.Parse(before)!;old["Version"]=24;old.AsObject().Remove("Environment");copyS.Restore(old.ToJsonString());Check(copyS.EnvironmentState.NativeTicks==0&&copyS.EnvironmentState.GeneratedFood==0,"v24 migration starts growth without fabricating prior food");
  string valid=copyS.Serialize();var bad=JsonNode.Parse(valid)!;bad["Environment"]!["Random"]=0;try{copyS.Restore(bad.ToJsonString());throw new Exception("Bad environment accepted");}catch(InvalidDataException){}Check(copyS.Serialize()==valid,"invalid growth save is rejected without changing current game");
  var construction=MissionOneTests.Load(root);var constructor=construction.World.Units.First(u=>u.Owner==0&&u.Type==7);int buildCell=Enumerable.Range(0,65536).First(c=>construction.PlacementError(constructor,3,c)==null);
  Check(construction.StartConstruction(constructor,3,buildCell),"builder reaches an available work point outside the footprint");for(int i=0;i<10000&&!construction.Complete(construction.Constructions.Single());i++)construction.Step();Check(construction.Complete(construction.Constructions.Single()),"construction completes with the builder outside its footprint");
  var nav=MissionOneTests.Load(root);var mover=nav.World.Units.First(u=>u.Owner==0&&u.Type==5);int goal=Enumerable.Range(0,65536).Where(c=>nav.Passable(c&255,c>>8)&&Simulation.CellDistance(c,mover.Y/1024*256+mover.X/1024)>6).First(c=>nav.Order(mover,c&255,c>>8));int blocked=mover.Route[mover.Route.Count/2];nav.World.Collision[blocked]=255;
  for(int i=0;i<10000&&mover.Route.Count>0;i++)nav.Step();Check(mover.Route.Count==0&&mover.X==(goal&255)*1024&&mover.Y==(goal>>8)*1024,"moving unit replans around a new obstruction and reaches original destination");
  var tech=Simulation.UnlockData.Technologies.First(t=>t.Key=="GENIECIVIL5");Check(!ResearchCompletionCatalog.Specific(tech)&&ResearchCompletionCatalog.For(tech)?.Name=="LABO2","missing original event 205 reuses original engineering lab animation");
  var research=ResearchV58Tests.Load(root);ResearchV58Tests.Staff(research);research.AgricultureResearch.Progress=0;var events=new List<Invention>();research.ResearchCompleted+=events.Add;ResearchV58Tests.Advance(research,14400);ResearchV58Tests.GatherFood(research);Check(research.PurchaseExtraResearch(2),"third discovery can choose initial engineering level without native event");ResearchV58Tests.Advance(research,7200);Check(events.Count==3&&events.Last().Key=="GENIECIVIL5"&&ResearchCompletionCatalog.For(events.Last())?.Name=="LABO2","third discovery emits exactly one available lab animation instead of disappearing");
  Console.WriteLine("ALL V62 ENVIRONMENT TESTS PASSED");
 }
}
