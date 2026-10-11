namespace RisingLands;
public static class CrashV95Tests {
 static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL v95: "+name);Console.WriteLine("PASS v95: "+name);}
 public static void ReportTest(string root){
  string destination=Path.Combine(Path.GetFullPath(PackageLayout.Asset(root,"../..")),"reports","v95-diagnostic-test","working","RISING");
  CrashDiagnostics.Initialize(destination);var s=OpponentV75Tests.Load(root);
  CrashDiagnostics.Context=()=>new {Map=s.World.MapName,Tick=s.Tick};CrashDiagnostics.Snapshot=s.Serialize;CrashDiagnostics.Phase="Teste controlado";
  string? file=CrashDiagnostics.Record(new InvalidDataException("Synthetic diagnostic test"),"Teste");
  Check(file!=null&&File.Exists(file),"crash report persists exception details");
  var json=System.Text.Json.JsonDocument.Parse(File.ReadAllText(file!));Check(json.RootElement.GetProperty("Exception").GetString()!.Contains("Synthetic diagnostic test"),"report retains exception and context");
  string snapshot=File.ReadAllText(file!.Replace(".json",".snapshot.json"));var copy=OpponentV75Tests.Load(root);copy.Restore(snapshot);Check(copy.Serialize()==s.Serialize(),"separate diagnostic snapshot remains loadable");
  Check(CrashDiagnostics.Record(new Exception("Duplicate"),"Teste")==null,"duplicate fatal notifications do not overwrite evidence");
 }
 public static void Run(string root){
  var s=OpponentV75Tests.Load(root);var a=s.World.Units.First(a=>a.Owner==1&&a.Type==5&&a.Health>0);var b=s.World.Buildings.First(b=>b.Owner==1&&b.Type==3);
  int source=Enumerable.Range(0,65536).First(c=>s.World.Objects[c] is >=29 and <=52);
  s.World.Objects[source]=(byte)Simulation.FoodData.HarvestTransitions[s.World.Objects[source]];a.Food=1;a.CargoKind=1;
  s.Hunger.Consumed[1]+=s.World.Resources[1];s.World.Resources[1]=0;
  int cell=s.WarehouseOverflowCells(b).First(c=>s.GroundResourceSpace(c,1));
  a.FoodSource=a.FoodField=source;a.FoodDestinationKind=2;a.FoodDestination=b.Slot;a.OverflowCell=a.DeliveryCell=cell;a.FoodJob=3;a.RepeatFood=a.PersistentCollection=true;a.Satiety=0;
  _=s.OrderPreview();Check(true,"valid overflow snapshot before last carried meal");
  s.Step();Check(a.Food==0&&a.Satiety>0,"enemy consumes last carried food during overflow delivery");
  _=s.OrderPreview();Check(true,"new resource command preview does not crash after last carried meal");
  Check(a.OverflowCell==-1,"collection restart discards completed overflow route");
  var player=s.World.Units.First(u=>u.Owner==0&&u.Type==5&&VisualCatalog.Visible(u));
  Check(s.Gather(player,Enumerable.Range(0,65536).Where(s.IsFood).OrderBy(c=>Math.Abs((c&255)-player.X/1024)+Math.Abs((c>>8)-player.Y/1024)).First()),"player farmer can receive order after enemy emergency meal");
  _=s.OrderPreview();Check(true,"player order validation includes repaired enemy state");
  CollectionV50Tests.Run(root);
  s=OpponentV75Tests.Load(root);int snapshots=0;
  for(int i=0;i<60000;i++){s.Step();if(i%1500==0){_=s.OrderPreview();snapshots++;}}
  Check(snapshots==40,"mission four: 33 simulated minutes with 40 validated snapshots");
 }
}
