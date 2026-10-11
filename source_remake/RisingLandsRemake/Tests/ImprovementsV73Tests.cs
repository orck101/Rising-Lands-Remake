namespace RisingLands;
public static class ImprovementsV73Tests {
 static int checks;
 static void Check(bool ok,string label){checks++;if(!ok)throw new Exception("FAIL v73: "+label);Console.WriteLine("PASS v73: "+label);}
 static Simulation Fixture(string root,int type){var baseSim=ImprovementsV67Tests.Fixture(root);var w=baseSim.World;var actor=w.Units.First(a=>a.Owner==0&&a.Type==0);actor.Type=type;actor.X=39*1024;actor.Y=40*1024;actor.Route.Clear();for(int y=0;y<w.Height;y++)w.Buildings.Add(new(1000+y,y==40?12:9,0,40+y*256));return new(w,baseSim.Campaign);}
 public static void Run(string root){foreach(int type in new[]{16,17}){
  var s=Fixture(root,type);var a=s.World.Units.First(a=>a.Type==type&&a.Owner==0);Check(Simulation.Airborne(a),"native aerial class "+type);Check(!s.Passable(40,40)&&!s.Passable(40,41),"ground wall and closed gate remain blocked "+type);
  a.Y=41*1024;Check(s.Order(a,41,41)&&a.Route.Contains(40+41*256),"direct aerial route crosses wall "+type);for(int i=0;i<40&&a.Route.Count>0;i++)s.Step();Check(a.Route.Count==0&&a.X==41*1024&&a.Y==41*1024,"aircraft completes wall crossing "+type);
  a.X=39*1024;a.Y=40*1024;Check(s.Order(a,41,40)&&a.Route.Contains(40+40*256),"direct aerial route crosses closed gate "+type);s.Step();var copy=Fixture(root,type);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"air route reloads "+type);for(int i=0;i<40;i++){s.Step();copy.Step();}Check(a.Route.Count==0&&a.X==41*1024&&s.Serialize()==copy.Serialize(),"saved aerial crossing continues deterministically "+type);
  Check(!s.Order(a,-1,40)&&!s.Order(a,x:s.World.Width,y:40),"aircraft remains inside map "+type);
  Check(s.ToggleGate(1040),"open gate "+type);for(int i=0;i<20;i++)s.Step();a.X=40*1024;a.Y=40*1024;Check(s.ToggleGate(1040),"aircraft overhead does not prevent gate closing "+type);
 }
 var ground=Fixture(root,0);var soldier=ground.World.Units.First(a=>a.Type==0&&a.Owner==0);Check(!ground.Order(soldier,41,40),"ground army cannot cross complete wall partition");Check(ground.ToggleGate(1040),"ground gate opens");for(int i=0;i<20;i++)ground.Step();Check(ground.Order(soldier,41,40)&&soldier.Route.Contains(40+40*256),"ground army still crosses open gate only");Console.WriteLine("ALL v73 TESTS PASSED: "+checks);
 }
}

