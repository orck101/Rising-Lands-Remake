using Microsoft.Xna.Framework;
namespace RisingLands;
public static class InteractionV42Tests {
 static void Check(bool value,string name){if(!value)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
 public static void Run(string root){
  var area=new Rectangle(0,0,1280,712);
  foreach(var row in new[]{(new Point(0,350),new Vector2(-1,0)),(new Point(1279,350),new Vector2(1,0)),(new Point(640,0),new Vector2(0,-1)),(new Point(640,711),new Vector2(0,1)),(new Point(0,0),new Vector2(-1,-1)),(new Point(1279,0),new Vector2(1,-1)),(new Point(0,711),new Vector2(-1,1)),(new Point(1279,711),new Vector2(1,1)),(new Point(640,350),Vector2.Zero),(new Point(640,750),Vector2.Zero),(new Point(-1,350),Vector2.Zero)})Check(EdgeCamera.Direction(area,row.Item1,true)==row.Item2,"edge camera context at "+row.Item1);
  Check(EdgeCamera.Direction(area,new Point(0,0),false)==Vector2.Zero,"inactive window cannot scroll by mouse");
  var s=MissionOneTests.Load(root);var home=s.World.Buildings.First(b=>b.Type==2&&b.Owner==0);var group=new List<Actor>();
  for(int i=0;i<3;i++){var a=s.ReleaseResident(home.Slot);Check(a!=null&&s.FrontCells(home).Contains(a.Y/1024*256+a.X/1024)&&s.Passable(a.X/1024,a.Y/1024),"released colonist appears on free front edge "+i);group.Add(a!);}
  Check(group.Select(a=>(a.X,a.Y)).Distinct().Count()==3,"door line fills free positions before allowing overlap");
  Check(s.Recruit(5),"training starts with colonists occupying the exit");for(int i=0;i<2000&&!s.Recruitments[0].Finished;i++)s.Step();var trained=s.World.Units.First(a=>a.Slot==s.Recruitments[0].SettlerSlot);
  Check(s.Recruitments[0].Finished&&group.Any(a=>a.X==trained.X&&a.Y==trained.Y),"trained unit exits over a standing colonist without getting stuck");
  int depot=ResearchTests.Build(s,3,false);Check(s.EnterBuildingGroup(group,depot)==2&&s.ResidentTrips.Count==2,"group entry reserves both available warehouse places without overbooking");
  var waiting=group.Single(a=>!s.ResidentTrips.Any(j=>j.Slot==a.Slot));Check(waiting.LegacyState==7,"excess selected colonist stays outside");
  for(int i=0;i<10000&&s.ResidentTrips.Count>0;i++)s.Step();Check(s.Occupants(depot)==2&&s.ResidentTrips.Count==0,"both group members finish their independent routes and enter");
  var b=s.World.Buildings.First(b=>b.Slot==depot);var exit=s.ReleaseResident(depot);Check(exit!=null&&s.FrontCells(b).Contains(exit.Y/1024*256+exit.X/1024),"warehouse withdrawal uses front edge");
  var saved=s.Serialize();var copy=MissionOneTests.Load(root);copy.Restore(saved);Check(copy.Serialize()==saved,"group arrival and front exit survive save/load");
  var blocked=MissionOneTests.Load(root);var h=blocked.World.Buildings.First(b=>b.Type==2&&b.Owner==0);var front=blocked.FrontCells(h).ToArray();var tiles=front.Select(c=>blocked.World.Collision[c]).ToArray();foreach(int c in front)blocked.World.Collision[c]=255;
  string before=blocked.Serialize();Check(blocked.ReleaseResident(h.Slot)==null&&blocked.Serialize()==before,"blocked front does not remove or teleport a colonist");
  Check(blocked.Recruit(5),"training can begin while front is temporarily blocked");for(int i=0;i<2000;i++)blocked.Step();Check(!blocked.Recruitments[0].Finished&&blocked.World.Units.First(a=>a.Slot==blocked.Recruitments[0].SettlerSlot).LegacyState==14,"trained troop waits inside when all front cells are blocked");
  for(int i=0;i<front.Length;i++)blocked.World.Collision[front[i]]=tiles[i];blocked.Step();var troop=blocked.World.Units.First(a=>a.Slot==blocked.Recruitments[0].SettlerSlot);Check(blocked.Recruitments[0].Finished&&blocked.FrontCells(h).Contains(troop.Y/1024*256+troop.X/1024),"training exits through front after obstruction clears");
 }
}
