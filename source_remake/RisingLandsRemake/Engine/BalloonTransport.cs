namespace RisingLands;
public sealed partial class Simulation {
 public static bool BalloonPassengerType(int type)=>type is >=0 and <=10 and not (3 or 8);
 public List<Actor> BalloonPassengers(Actor balloon)=>Ocean.Jobs.Where(j=>j.Balloon==balloon.Slot&&j.Stage==2).Select(j=>World.Units.First(a=>a.Slot==j.Actor)).ToList();
 public string BalloonStatus(Actor balloon)=>"Balao: "+BalloonPassengers(balloon).Count+" / 5 a bordo | "+Ocean.Jobs.Count(j=>j.Balloon==balloon.Slot&&j.Stage<2)+" aguardando";
 public bool CanBoardBalloon(Actor a,Actor balloon){
  if(!BalloonPassengerType(a.Type)||!VisualCatalog.Visible(a)||a.Health<=0||a.PetrifiedUntil>Tick||balloon.Type!=17||balloon.Health<=0||a.Owner!=balloon.Owner||a.Owner is <0 or >3||WaitingForFlight(a)||!World.Units.Contains(a)||!World.Units.Contains(balloon))return false;
  var jobs=Ocean.Jobs.Where(j=>j.Balloon==balloon.Slot).ToList();if(jobs.Count>=5||jobs.Any(j=>!j.Manual))return false;
  int cell=FlightLanding(a,ActorCell(balloon));return cell>=0&&OpponentReachableCells(a)[cell]!=0;
 }
 public bool BoardBalloon(Actor a,Actor balloon){if(!CanBoardBalloon(a,balloon))return false;ClearOrders(a);CancelFeeding(a);a.Route.Clear();Ocean.Jobs.Add(new(){Actor=a.Slot,Balloon=balloon.Slot,Destination=-1,Manual=true});return true;}
 bool RedirectLoadedBalloon(Actor balloon,int destination){
  var jobs=Ocean.Jobs.Where(j=>j.Balloon==balloon.Slot&&j.Stage>0).ToList();if(jobs.Count==0||jobs.Any(j=>!j.Manual||j.Stage!=2))return false;
  var first=World.Units.First(a=>a.Slot==jobs[0].Actor);if(!ValidMapCell(destination))return false;if(!CanTraverse(first,destination&255,destination>>8))return Order(balloon,destination&255,destination>>8,true);
  int landing=FlightLanding(first,destination);if(landing<0||!Order(balloon,landing&255,landing>>8,true))return false;
  foreach(var job in jobs){job.Manual=false;job.Destination=destination;}return true;
 }
 public bool CanUnloadBalloon(Actor balloon,int type=-1)=>balloon.Type==17&&balloon.Route.Count==0&&BalloonPassengers(balloon).Any(a=>(type<0||a.Type==type)&&CanTraverse(a,balloon.X/1024,balloon.Y/1024)&&FlightLanding(a,ActorCell(balloon))>=0);
 public int UnloadBalloon(Actor balloon,int type=-1){
  if(!CanUnloadBalloon(balloon,type))return 0;int count=0;
  foreach(var j in Ocean.Jobs.Where(j=>j.Balloon==balloon.Slot&&j.Stage==2).ToArray()){var a=World.Units.First(a=>a.Slot==j.Actor);if(type>=0&&a.Type!=type||!CanTraverse(a,balloon.X/1024,balloon.Y/1024))continue;int cell=FlightLanding(a,ActorCell(balloon));if(cell<0)continue;a.LegacyState=7;a.X=(cell&255)*1024;a.Y=(cell>>8)*1024;a.Route.Clear();Ocean.Jobs.Remove(j);count++;}return count;
 }
}
