using RisingLands;
public static class OpeningV88Tests {
 static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL v88 opening: "+name);Console.WriteLine("PASS v88 opening: "+name);}
 public static void Run(string root){
  var s=OpponentV75Tests.Load(root);var units=s.World.Units.Where(a=>a.Owner==1&&a.Health>0).ToArray();
  Check(units.Length==5&&units.Count(a=>a.Type==5)==1&&units.Count(a=>a.Type==7)==1&&units.Count(a=>a.Type==9)==3,"exactly one farmer, one builder, three colonists and no soldiers");
  Check(s.World.Resources[1]==15&&s.World.Resources[5]==15&&s.World.Resources[9]==0,"red begins with 15 food, 15 stone, zero gears");
  var home=s.World.Buildings.Single(b=>b.Owner==1);Check(home.Type==2&&s.Residents(home.Slot)==3,"only sanctuary with three colonists");
  var copy=OpponentV75Tests.Load(root);copy.Restore(s.Serialize());Check(s.Serialize()==copy.Serialize(),"new opening persists exactly");
  for(int i=0;i<15000&&s.Opponent.Deposited[1]==0;i++)s.Step();
  Check(s.Constructions.Any(c=>c.Owner==1&&c.Type==3&&s.Complete(c)),"red really constructs its first warehouse");
  Check(s.Opponent.Deposited[1]>0&&s.World.Buildings.Any(b=>b.Owner==1&&b.Type==3&&s.Inhabited(b)),"warehouse staffed and farmer physically deposits food");
  Check(s.Constructions.Where(c=>c.Owner==1).First().Type==3,"warehouse is the first construction");
  copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"resource conservation and jobs restore after opening development");
  s.Restart();Check(s.Tick==0&&s.World.Resources[1]==15&&s.World.Units.Count(a=>a.Owner==1&&a.Health>0)==5,"restart resets balanced opening");
  Console.WriteLine("ALL v88 OPENING TESTS PASSED");
 }
}
