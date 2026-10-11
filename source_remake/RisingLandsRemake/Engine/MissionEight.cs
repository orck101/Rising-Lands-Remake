using System.Text.Json;
namespace RisingLands;
public sealed class FlightJob {public int Actor{get;set;}public int Balloon{get;set;}public int Destination{get;set;}public int Pickup{get;set;}=-1;public int Stage{get;set;}public bool Manual{get;set;}}
public sealed class OceanState {public Dictionary<int,int> Claims{get;set;}=new();public List<Actor> Balloons{get;set;}=new();public List<FlightJob> Jobs{get;set;}=new();}
public sealed partial class Simulation {
 public bool IsEighthMission=>World.MapName.Equals("MONDE08.ETA",StringComparison.OrdinalIgnoreCase);
 public OceanState Ocean{get;private set;}=new();List<Building> oceanOriginalBuildings=new();
 public static bool EighthCoast(int cell)=>cell>=0&&(cell&255)>=106&&(cell&255)<=127&&((cell>>8) is >=9 and <=55 or >=81 and <=127);
 static bool EighthSatisfied(int owner,IEnumerable<Actor> actors,IEnumerable<Building> buildings)=>actors.Any(a=>a.Owner==owner&&a.Type!=17&&a.Health>0&&a.LegacyState is not (0 or 4))&&buildings.Any(b=>b.Owner==owner&&b.Type==2&&EighthCoast(b.Cell));
 public string EighthNextMap=>"MONDE09"+(char)('A'+Enumerable.Range(1,3).Count(o=>World.Buildings.Any(b=>b.Owner==o&&b.Type==2&&EighthCoast(b.Cell))))+".ETA";
 void InitializeOcean(){oceanOriginalBuildings=InitialBuildings.ToList();}
 bool FlightHidden(int slot,OceanState? state)=>state?.Jobs.Any(j=>j.Actor==slot&&j.Stage==2)==true;
 public bool WaitingForFlight(Actor a)=>Ocean.Jobs.Any(j=>j.Actor==a.Slot);
 public bool CanClaimHeliport(Actor a,int slot)=>IsEighthMission&&a.Type==9&&a.Owner is >=0 and <=3&&World.Buildings.Any(b=>b.Slot==slot&&b.Type==18&&b.Owner==200)&&!ResidentTrips.Any(t=>t.Building==slot&&t.Slot!=a.Slot);
 void ClaimHeliport(Actor a,Building b){if(!CanClaimHeliport(a,b.Slot))return;Ocean.Claims[b.Slot]=a.Owner;ReplaceOceanOwner(b.Slot,a.Owner);}
 void ReplaceOceanOwner(int slot,int owner){for(int i=0;i<InitialBuildings.Count;i++)if(InitialBuildings[i].Slot==slot)InitialBuildings[i]=InitialBuildings[i] with{Owner=owner};for(int i=0;i<World.Buildings.Count;i++)if(World.Buildings[i].Slot==slot)World.Buildings[i]=World.Buildings[i] with{Owner=owner};InvalidateSpatialIndex();doorCacheCount=-1;}
 static readonly Lazy<UnitVisual> oceanBalloonVisual=new(()=>JsonSerializer.Deserialize<CatalogFile>(File.ReadAllText(PackageLayout.FromBase("Content/Data/unit_rendering.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true,PropertyNamingPolicy=JsonNamingPolicy.SnakeCaseLower})!.Types.First(t=>t.Type==17));
 Actor BalloonSeed(int owner,Building b){var frames=oceanBalloonVisual.Value;return new(){Slot=50000+owner,Type=17,Owner=owner,LegacyState=7,Health=1,X=((b.Cell&255)+2)*1024,Y=((b.Cell>>8)+2)*1024,AnimationCount=frames.FrameCount,AnimationDelay=frames.FrameDelay};}
 void EnsureBalloons(){foreach(var b in World.Buildings.Where(b=>b.Type==18&&b.Owner is >=0 and <=3&&Inhabited(b)).ToArray())if(!World.Units.Any(a=>a.Type==17&&a.Owner==b.Owner)&&World.Units.Count<NativeEntityCapacity){var seed=BalloonSeed(b.Owner,b);Ocean.Balloons.Add(CloneState(seed));InitialActors.Add(seed.Slot,CloneState(seed));World.Units.Add(CloneState(seed));}}
 public bool RequestFlight(Actor a,int cell,bool keepJob=false){
  if(!IsEighthMission||a.Type is <0 or >10 or 3 or 8||a.Health<=0||a.Owner is <0 or >3||!VisualCatalog.Visible(a)||!ValidMapCell(cell)||!CanTraverse(a,cell&255,cell>>8)||WaitingForFlight(a))return false;
  var balloon=World.Units.FirstOrDefault(b=>b.Type==17&&b.Owner==a.Owner&&b.Health>0);if(balloon==null)return false;
  if(!keepJob)ClearOrders(a);CancelFeeding(a);a.Route.Clear();Ocean.Jobs.Add(new(){Actor=a.Slot,Balloon=balloon.Slot,Destination=cell});return true;
 }
 int FlightLanding(Actor a,int cell)=>NearbyCells(cell,5).Where(c=>CanTraverse(a,c&255,c>>8)&&!World.Units.Any(u=>u!=a&&VisualCatalog.Visible(u)&&!Airborne(u)&&ActorCell(u)==c)).OrderBy(c=>TileSquare(c,cell)).ThenBy(c=>c).FirstOrDefault(-1);
 static int TileSquare(int a,int b){int dx=(a&255)-(b&255),dy=(a>>8)-(b>>8);return dx*dx+dy*dy;}
 void OceanStep(){
  EnsureBalloons();
  foreach(var j in Ocean.Jobs.ToArray()){var a=World.Units.FirstOrDefault(a=>a.Slot==j.Actor);if(a==null||a.Health<=0)Ocean.Jobs.Remove(j);}
  foreach(var balloon in World.Units.Where(a=>a.Type==17&&a.Health>0).ToArray()){
   var active=Ocean.Jobs.Where(j=>j.Balloon==balloon.Slot&&j.Stage>0).ToList();
   if(active.Count==0){var next=Ocean.Jobs.FirstOrDefault(j=>j.Balloon==balloon.Slot);if(next==null)continue;var a=World.Units.First(a=>a.Slot==next.Actor);int pickup=FlightLanding(a,next.Manual?ActorCell(balloon):ActorCell(a));if(pickup<0)continue;foreach(var j in Ocean.Jobs.Where(j=>j.Balloon==balloon.Slot&&j.Stage==0&&j.Manual==next.Manual&&(next.Manual||TileSquare(j.Destination,next.Destination)<=25)&&(next.Manual||TileSquare(ActorCell(World.Units.First(a=>a.Slot==j.Actor)),pickup)<=36)).Take(5)){j.Pickup=pickup;j.Stage=1;}Order(balloon,pickup&255,pickup>>8,true);continue;}
   if(active.All(j=>j.Stage==2)){
    if(balloon.Route.Count>0||active.Any(j=>j.Destination<0))continue;
    if(TileSquare(ActorCell(balloon),active[0].Destination)>25){var traveler=World.Units.First(a=>a.Slot==active[0].Actor);int target=FlightLanding(traveler,active[0].Destination);if(target>=0)Order(balloon,target&255,target>>8,true);continue;}
    foreach(var j in active){var a=World.Units.First(a=>a.Slot==j.Actor);int landing=FlightLanding(a,ActorCell(balloon));if(landing<0)continue;a.LegacyState=7;a.X=(landing&255)*1024;a.Y=(landing>>8)*1024;Ocean.Jobs.Remove(j);Order(a,j.Destination&255,j.Destination>>8,true);}continue;
   }
   if(balloon.Route.Count>0)continue;
   foreach(var j in active.Where(j=>j.Stage==1)){var a=World.Units.First(a=>a.Slot==j.Actor);if(TileSquare(ActorCell(a),j.Pickup)>2){if(a.Route.Count==0)Order(a,j.Pickup&255,j.Pickup>>8,true);continue;}CancelFeeding(a);a.Route.Clear();a.LegacyState=12;j.Stage=2;}
   if(active.All(j=>j.Stage==2)&&active[0].Destination>=0){var a=World.Units.First(a=>a.Slot==active[0].Actor);int landing=FlightLanding(a,active[0].Destination);if(landing>=0)Order(balloon,landing&255,landing>>8,true);}
  }
  foreach(var j in Ocean.Jobs.Where(j=>j.Stage==2)){var a=World.Units.First(a=>a.Slot==j.Actor);var b=World.Units.First(a=>a.Slot==j.Balloon);a.X=b.X;a.Y=b.Y;}
 }
 void OceanOpponentStep(){if(Tick%60!=0)return;foreach(int owner in Enumerable.Range(1,3)){
  var neutral=World.Buildings.Where(b=>b.Type==18&&b.Owner==200).OrderBy(b=>World.Units.Where(a=>a.Owner==owner).Select(a=>TileSquare(ActorCell(a),b.Cell)).DefaultIfEmpty(int.MaxValue).Min()).FirstOrDefault();
  if(!Ocean.Balloons.Any(a=>a.Owner==owner)){var col=World.Units.FirstOrDefault(a=>a.Owner==owner&&a.Type==9&&VisualCatalog.Visible(a)&&!ResidentReserved(a.Slot));if(col!=null&&neutral!=null)EnterBuilding(col,neutral.Slot);continue;}
  if(World.Buildings.Any(b=>b.Owner==owner&&b.Type==2&&EighthCoast(b.Cell))||Constructions.Any(c=>c.Owner==owner&&c.Type==2))continue;
  // Scout the far shore before committing the clan's only builder to the crossing.
  var scout=World.Units.FirstOrDefault(a=>a.Owner==owner&&a.Type==9&&VisualCatalog.Visible(a)&&!ResidentReserved(a.Slot));
  if(!World.Units.Any(a=>a.Owner==owner&&a.Type==9&&VisualCatalog.Visible(a)&&a.X/1024>=106)){
   if(scout!=null&&!WaitingForFlight(scout)){int y=owner==2?85:owner==1?44:109;int target=FlightLanding(scout,y*256+110);if(target>=0)Order(scout,target&255,target>>8);}continue;
  }
  // Inspect the whole planned footprint before taking the builder away from home.
  int scoutY=(owner==2?85:owner==1?44:109)+3;
  scout=World.Units.First(a=>a.Owner==owner&&a.Type==9&&VisualCatalog.Visible(a)&&a.X/1024>=106);
  int inspect=FlightLanding(scout,scoutY*256+119);
  if(inspect>=0&&TileSquare(ActorCell(scout),inspect)>2){if(scout.Route.Count==0)Order(scout,inspect&255,inspect>>8);continue;}
  var builder=World.Units.FirstOrDefault(a=>a.Owner==owner&&a.Type==7&&VisualCatalog.Visible(a)&&!WaitingForFlight(a));if(builder==null)continue;
  // The coastal ETA plans supply each clan's destination; production still pays real costs.
  int cy=owner==2?85:owner==1?44:109;
  var cell=Enumerable.Range(cy-5,11).SelectMany(y=>Enumerable.Range(108,12).Select(x=>y*256+x)).Where(c=>PlacementError(builder,2,c)==null).OrderBy(c=>TileSquare(c,cy*256+(owner==2?119:116))).FirstOrDefault(-1);
  if(cell>=0)StartConstruction(builder,2,cell);
 }}
 void RestoreOcean(Save s){
  if(!IsEighthMission&&s.Ocean?.Claims.Count>0)throw new InvalidDataException("Heliport claims outside mission eight");
  var o=s.Ocean??new();if(o.Claims==null||o.Balloons==null||o.Jobs==null||o.Claims.Count>4||o.Balloons.Count>4||o.Jobs.Count>800)throw new InvalidDataException("Invalid ocean state");
  InitialBuildings=oceanOriginalBuildings.ToList();foreach(var pair in o.Claims){var b=InitialBuildings.FirstOrDefault(b=>b.Slot==pair.Key&&b.Type==18&&b.Owner==200);if(b==null||pair.Value is <0 or >3)throw new InvalidDataException("Invalid heliport claim");for(int i=0;i<InitialBuildings.Count;i++)if(InitialBuildings[i].Slot==pair.Key)InitialBuildings[i]=b with{Owner=pair.Value};}
  var owners=new HashSet<int>();foreach(var seed in o.Balloons){bool validSeed=InitialBuildings.Concat(s.Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).Where(b=>b.Type==18&&b.Owner==seed.Owner).Any(b=>JsonSerializer.Serialize(seed)==JsonSerializer.Serialize(BalloonSeed(seed.Owner,b)));if(!validSeed||!owners.Add(seed.Owner))throw new InvalidDataException("Invalid balloon seed");InitialActors.Add(seed.Slot,CloneState(seed));}
  var actors=new HashSet<int>();foreach(var j in o.Jobs){var a=s.Units.FirstOrDefault(a=>a.Slot==j.Actor);var b=s.Units.FirstOrDefault(a=>a.Slot==j.Balloon);if(a==null||b==null||b.Type!=17||a.Owner!=b.Owner||a.Type is <0 or >10 or 3 or 8||a.Health<=0||j.Stage is <0 or >2||!actors.Add(j.Actor)||!(j.Manual&&j.Destination==-1||!j.Manual&&ValidMapCell(j.Destination))||j.Stage>0&&!ValidMapCell(j.Pickup)||j.Stage==2&&(a.LegacyState!=12||a.Route.Count!=0)||j.Stage<2&&a.LegacyState!=7)throw new InvalidDataException("Invalid flight job");}
  if(o.Jobs.Where(j=>j.Stage>0).GroupBy(j=>j.Balloon).Any(g=>g.Count()>5))throw new InvalidDataException("Overloaded balloon");
 }
}
