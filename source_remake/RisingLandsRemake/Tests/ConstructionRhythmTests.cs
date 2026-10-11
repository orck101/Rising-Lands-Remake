namespace RisingLands;
public static class ConstructionRhythmTests {
 public static void Run(string root){
  void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
  var s=MissionOneTests.Load(root);var u=s.World.Units.First(a=>a.Type==7&&a.Owner==0);
  int cell=Enumerable.Range(0,65536).First(c=>s.PlacementError(u,3,c)==null);
  byte original=s.World.Objects[cell];
  for(int code=12;code<=28;code++){s.World.Objects[cell]=(byte)code;Check(s.PlacementError(u,3,cell)==null,"decorative grass object does not block construction: "+code);}
  foreach(int code in new[]{1,11,29,52,66,67,68}){s.World.Objects[cell]=(byte)code;Check(s.PlacementError(u,3,cell)!=null,"obstacle or resource remains protected: "+code);}
  s.World.Objects[cell]=original;
  Check(s.StartConstruction(u,3,cell),"hammer cycle construction begins");var c=s.Constructions.Last();
  for(int i=0;i<10000&&u.Route.Count>0;i++)s.Step();
  bool still=true;int previous=c.Progress,hits=0,hold=0,events=0;s.ConstructionStruck+=site=>events++;var v=new VisualCatalog();
  for(int i=0;i<200;i++){
   int before=c.Progress;var frame=v.ConstructionFrame(c,s);s.Step();
   if(c.Progress!=before){hits++;Check(c.Progress-before==2&&c.AnimationTick/2==2,"construction advances only on hammer contact frame");}
   else{hold++;still&=ReferenceEquals(frame,v.ConstructionFrame(c,s));}
  }
  Check(events==hits,"exactly one sound event per construction impact");
  Check(u.Direction==Simulation.ConstructionFacing(c.WorkCell,c.Cell,5,5),"builder faces construction while working");
  Check(hits>=5&&hold>180&&still,"building strips remain fixed between spaced hammer strikes");
  var copy=MissionOneTests.Load(root);copy.Restore(s.Serialize());for(int i=0;i<100;i++){s.Step();copy.Step();}Check(s.Serialize()==copy.Serialize(),"save/load preserves countdown and hammer pose between strikes");
  s.Stop(u);int tick=c.AnimationTick,cooldown=c.StrikeCooldown,progress=c.Progress,pausedEvents=events;for(int i=0;i<40;i++)s.Step();
  Check(events==pausedEvents,"paused construction emits no hammer sounds");
  Check(c.AnimationTick==tick&&c.StrikeCooldown==cooldown&&c.Progress==progress,"interruption freezes hammer countdown and visual stage");
  Check(s.ResumeConstruction(u,c),"hammer cycle resumes without second payment");
  var invalid=System.Text.Json.Nodes.JsonNode.Parse(s.Serialize())!;invalid["Constructions"]![0]!["StrikeCooldown"]=-1;string beforeInvalid=s.Serialize();
  try{s.Restore(invalid.ToJsonString());throw new Exception("invalid countdown accepted");}catch(InvalidDataException){}
  Check(s.Serialize()==beforeInvalid,"invalid construction timer rejected atomically");
 }
}
