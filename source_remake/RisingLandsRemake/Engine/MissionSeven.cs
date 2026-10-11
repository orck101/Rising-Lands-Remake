using System.Text.Json;
namespace RisingLands;
public record CreatureEntry(int Index,int Cell,int Mask,int PeriodTicks);
public record CreatureTemplate(int Type,int Health,int AnimationCount,int AnimationDelay);
public record CreatureCatalog(List<CreatureEntry> Entries,List<int[]> Groups,List<CreatureTemplate> Templates);
public sealed class CreatureArrival {public int Entry{get;set;}public long Tick{get;set;}public List<Actor> Seeds{get;set;}=new();}
public sealed class CreatureState {public List<CreatureArrival> Arrivals{get;set;}=new();}
public sealed partial class Simulation {
 public bool IsSeventhMission=>World.MapName.Equals("MONDE07.ETA",StringComparison.OrdinalIgnoreCase);
 public static readonly CreatureCatalog SeventhCreatures=JsonSerializer.Deserialize<CreatureCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/mission_seven_creatures.json")))!;
 public static readonly AdvancedAttackCatalog CreatureAttacks=JsonSerializer.Deserialize<AdvancedAttackCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/creature_attack.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true,PropertyNamingPolicy=JsonNamingPolicy.SnakeCaseLower})!;
 public CreatureState Creatures{get;private set;}=new();
 bool CreatureOwner(int owner)=>IsSeventhMission&&owner==4;
 bool CombatOwner(int owner)=>owner is >=0 and <=3||CreatureOwner(owner);
 AdvancedAttackRule? AttackRuleFor(Actor a)=>AdvancedAttacks.Rules.FirstOrDefault(r=>r.Type==a.Type)??(CreatureOwner(a.Owner)?CreatureAttacks.Rules.FirstOrDefault(r=>r.Type==a.Type):null);
 bool SupportedFighter(Actor a)=>AttackRuleFor(a)!=null;
 bool SeventhHostile(int first,int second)=>IsSeventhMission&&(first==0&&second==4||first==4&&second==0);
 static bool SeventhMissionSatisfied(IEnumerable<Actor> actors,EngineeringResearch? research)=>SixthSurvivors(actors)&&research!=null&&research.Completed.Count+(research.Agriculture?.Completed.Count??0)+(research.Military?.Completed.Count??0)+(research.Spiritual?.Completed.Count??0)>=2;
 void InitializeSeventhCreatures(){if(IsSeventhMission)foreach(var a in World.Units.Where(a=>a.Type is >=19 and <=24))a.Owner=4;}
 int[] SeventhGroup(CreatureEntry entry,long tick){var allowed=Enumerable.Range(0,16).Where(g=>(entry.Mask&(1<<g))!=0).ToArray();return allowed.Length==0?Array.Empty<int>():SeventhCreatures.Groups[allowed[(int)((tick/entry.PeriodTicks-1)%allowed.Length)]];}
 static Actor CreatureSeed(int slot,int type,int cell){var t=SeventhCreatures.Templates.Single(t=>t.Type==type);return new(){Slot=slot,Type=type,Owner=4,LegacyState=7,Health=t.Health,AnimationCount=t.AnimationCount,AnimationDelay=t.AnimationDelay,X=(cell&255)*1024,Y=(cell>>8)*1024};}
 void CreatureStep(){
  if(!IsSeventhMission)return;
  // ETA delays *30; deterministic cycle replaces the original randomized first group.
  foreach(var entry in SeventhCreatures.Entries.Where(e=>e.PeriodTicks>0&&Tick%e.PeriodTicks==0)){
   var group=SeventhGroup(entry,Tick);if(group.Length==0||World.Units.Count(a=>a.Owner==4&&a.Health>0)+group.Length>50||World.Units.Count(a=>a.Health>0)+group.Length>NativeEntityCapacity||Creatures.Arrivals.Sum(a=>a.Seeds.Count)+group.Length>4096)continue;
   var arrival=new CreatureArrival{Entry=entry.Index,Tick=Tick};var cells=new HashSet<int>();
   foreach(int type in group){var probe=CreatureSeed(0,type,entry.Cell);int cell=NearbyCells(entry.Cell,6).Where(c=>CanTraverse(probe,c&255,c>>8)&&!cells.Contains(c)&&!World.Units.Any(a=>a.Health>0&&ActorCell(a)==c)).OrderBy(c=>Math.Abs((c&255)-(entry.Cell&255))+Math.Abs((c>>8)-(entry.Cell>>8))).ThenBy(c=>c).FirstOrDefault(-1);if(cell<0){arrival.Seeds.Clear();break;}cells.Add(cell);arrival.Seeds.Add(CreatureSeed(30000+Creatures.Arrivals.Sum(a=>a.Seeds.Count)+arrival.Seeds.Count,type,cell));}
   if(arrival.Seeds.Count!=group.Length)continue;Creatures.Arrivals.Add(arrival);foreach(var seed in arrival.Seeds){InitialActors.Add(seed.Slot,CloneState(seed));World.Units.Add(CloneState(seed));}
  }
  if(Tick%30!=0)return;
  foreach(var a in World.Units.Where(a=>a.Owner==4&&a.Health>0&&a.PetrifiedUntil<=Tick)){
   if(a.AttackTarget>=0&&World.Units.Any(b=>b.Slot==a.AttackTarget&&CanAttack(a,b)))continue;
   // Local interception; the fixed native player base supplies raid destinations.
   var target=World.Units.Where(b=>b.Owner==0&&CanAttack(a,b)&&TileDistanceSquared(a,b)<=18*18).OrderBy(b=>FightingUnit(b)?0:1).ThenBy(b=>TileDistanceSquared(a,b)).ThenBy(b=>b.Slot).FirstOrDefault();
   if(target!=null){ClearOrders(a);a.AttackTarget=target.Slot;continue;}
   if(Sieging(a))continue;
   var building=World.Buildings.Where(b=>b.Owner==0).OrderBy(b=>Math.Abs((b.Cell&255)-a.X/1024)+Math.Abs((b.Cell>>8)-a.Y/1024)).ThenBy(b=>b.Slot).FirstOrDefault();
   if(building!=null){AttackBuilding(a,building.Slot);continue;}
   if(a.Route.Count==0){var last=World.Units.Where(b=>b.Owner==0&&CanAttack(a,b)).OrderBy(b=>TileDistanceSquared(a,b)).FirstOrDefault();if(last!=null){a.AttackTarget=last.Slot;}}
  }
 }
 Save NormalizeSeventhSave(Save s){if(!IsSeventhMission||s.Version>=42)return s;foreach(var a in s.Units.Where(a=>a.Type is >=19 and <=24))a.Owner=4;return s with{Creatures=new()};}
 void RestoreCreatureSeeds(Save s){
  var state=s.Creatures;if(!IsSeventhMission){if(state?.Arrivals.Count>0)throw new InvalidDataException("Creatures outside mission seven");return;}
  if(state?.Arrivals==null||state.Arrivals.Count>4096)throw new InvalidDataException("Missing creature state");var seen=new HashSet<(int,long)>();int slot=30000;
  foreach(var arrival in state.Arrivals){var entry=SeventhCreatures.Entries.FirstOrDefault(e=>e.Index==arrival.Entry&&e.PeriodTicks>0);if(entry==null||arrival.Tick<=0||arrival.Tick>s.Tick||arrival.Tick%entry.PeriodTicks!=0||!seen.Add((arrival.Entry,arrival.Tick))||arrival.Seeds==null)throw new InvalidDataException("Invalid creature arrival");var group=SeventhGroup(entry,arrival.Tick);if(group.Length!=arrival.Seeds.Count)throw new InvalidDataException("Invalid creature group");
   for(int i=0;i<group.Length;i++){var seed=arrival.Seeds[i];int cell=ActorCell(seed);if(seed.Slot!=slot++||!ValidMapCell(cell)||Math.Abs((cell&255)-(entry.Cell&255))>6||Math.Abs((cell>>8)-(entry.Cell>>8))>6||JsonSerializer.Serialize(seed)!=JsonSerializer.Serialize(CreatureSeed(seed.Slot,group[i],cell)))throw new InvalidDataException("Invalid creature seed");InitialActors.Add(seed.Slot,CloneState(seed));}
  }if(slot>34096)throw new InvalidDataException("Too many creature seeds");
 }
}
