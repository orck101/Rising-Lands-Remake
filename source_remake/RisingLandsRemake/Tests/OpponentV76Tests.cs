namespace RisingLands;
public static class OpponentV76Tests {
 public static void Run(string root){
  int count=0;
  void Check(bool condition,string message){if(!condition)throw new Exception(message);count++;Console.WriteLine("PASS v76: "+message);}
  for(int number=1;number<=25;number++){
   string name=$"MONDE{number:00}.ETA",path=PackageLayout.Asset(root,"CD/WORLD",name);if(!File.Exists(path))continue;
   var world=LegacyAssets.Map(path);new VisualCatalog().Apply(world);var s=new Simulation(world);var native=NativeOpponentPlans.For(world);Check(native!=null,name+" original plans match map hash");
   foreach(int type in Enumerable.Range(0,22)){
    int expected=native!.Plans.Where(p=>p.Cell!=65535&&(p.ForbiddenTypes&(1u<<type))==0).Select(p=>p.Index).FirstOrDefault(65535);
    Check(NativeOpponentPlans.Find(native,0,type)==expected,name+" type lookup "+type);
   }
   if(number<4){Check(!s.OpponentEnabled,name+" tutorial keeps previous behavior");continue;}
   Check(s.OpponentEnabled==world.Units.Any(a=>a.Owner is >0 and <4),name+" opponent activates from native population");
   for(int i=0;i<60;i++)s.Step();
   var copyWorld=LegacyAssets.Map(path);new VisualCatalog().Apply(copyWorld);var copy=new Simulation(copyWorld);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),name+" opponent jobs and economy restore");
   for(int i=0;i<30;i++){s.Step();copy.Step();}Check(s.Serialize()==copy.Serialize(),name+" deterministic continuation");
  }
  Check(!new NativeGhostPlan(0,100,1u<<3,0).Allows(3),"set mask bit forbids warehouse");
  Check(new NativeGhostPlan(0,100,0,0).Allows(3),"clear mask bit allows warehouse");
  Check(!new NativeGhostPlan(0,65535,0,0).Allows(3),"sentinel plan never selected");
  Console.WriteLine($"PASS v76 total {count}");
 }
}
