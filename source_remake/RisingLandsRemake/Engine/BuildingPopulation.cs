using System.Text.Json;
namespace RisingLands;
public sealed class ResidentTrip {public int Slot{get;set;}public int Building{get;set;}public int Cell{get;set;}}
public sealed class CultureState {public int Building{get;set;}public int Food{get;set;}public int Progress{get;set;}public int Born{get;set;}public int Consumed{get;set;}}
public sealed partial class Simulation {
 Dictionary<int,Actor> OriginalActorSeeds=new();Dictionary<int,List<int>> OriginalResidents=new();
 public List<Actor> BornActors{get;private set;}=new();public List<ResidentTrip> ResidentTrips{get;private set;}=new();public List<CultureState> Cultures{get;private set;}=new();
 static T CloneState<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
 void InitializePopulation(){OriginalActorSeeds=CloneState(InitialActors);OriginalResidents=CloneState(World.BuildingResidents);}
 Dictionary<int,Actor> residentActorIndex=new();List<Actor>? residentActorList;int residentActorCount=-1;
 Actor? ResidentActor(int id){if(residentActorList!=World.Units||residentActorCount!=World.Units.Count){residentActorIndex=World.Units.ToDictionary(a=>a.Slot);residentActorList=World.Units;residentActorCount=World.Units.Count;}return residentActorIndex.GetValueOrDefault(id);}
 public int Residents(int slot)=>World.BuildingResidents.TryGetValue(slot,out var ids)?ids.Count(id=>ResidentActor(id) is {Type:9,Health:>0,LegacyState:14}&&!ResearchReserved(id)):0;
 public int Occupants(int slot){var ids=World.BuildingResidents.GetValueOrDefault(slot);int count=ids?.Count(id=>ResidentActor(id) is {Type:9,Health:>0,LegacyState:14})??0;return count+LaboratoryWorkers.Count(j=>j.LaboratorySlot==slot&&j.Stage==1&&(ids==null||!ids.Contains(j.SettlerSlot))&&ResidentActor(j.SettlerSlot) is {Type:9,Health:>0,LegacyState:14});}
 public bool Inhabited(Building b)=>Residents(b.Slot)>0||LaboratoryWorkers.Any(j=>j.LaboratorySlot==b.Slot&&j.Stage==1);
 static readonly Dictionary<string,int> NativeResidentCapacities=JsonSerializer.Deserialize<Dictionary<string,int>>(File.ReadAllText(PackageLayout.FromBase("Content/Data/resident_capacities.json")))!;
 public int ResidentCapacity(Building b)=>NativeResidentCapacities.GetValueOrDefault(b.Type.ToString(),0);
 bool ResidentReserved(int slot)=>ResearchReserved(slot)||Recruitments.Any(j=>j.SettlerSlot==slot&&!j.Finished)||ResidentTrips.Any(j=>j.Slot==slot);
 public Actor? ReleaseResident(int slot,int owner=0,int actorSlot=-1){
  var b=World.Buildings.FirstOrDefault(b=>b.Slot==slot&&b.Owner==owner);if(b==null)return null;
  var actor=World.Units.FirstOrDefault(a=>(actorSlot<0||a.Slot==actorSlot)&&a.Type==9&&a.Health>0&&a.LegacyState==14&&(World.BuildingResidents.GetValueOrDefault(slot,new()).Contains(a.Slot)||LaboratoryWorkers.Any(j=>j.LaboratorySlot==slot&&j.Stage==1&&j.SettlerSlot==a.Slot))&&!Recruitments.Any(j=>j.SettlerSlot==a.Slot&&!j.Finished)&&!ResidentTrips.Any(j=>j.Slot==a.Slot));if(actor==null)return null;
  int exit=BuildingExit(b);if(exit<0)return null;
  foreach(var ids in World.BuildingResidents.Values)ids.Remove(actor.Slot);LaboratoryWorkers.RemoveAll(j=>j.SettlerSlot==actor.Slot);actor.LegacyState=7;PlaceAtExit(actor,b,exit);actor.Route.Clear();actor.Direction=13;EmitBuildingSound(b,0x26);return actor;
 }
 public bool CanEnterBuilding(Actor actor,int slot){
  if(World.Buildings.FirstOrDefault(b=>b.Slot==slot&&b.Type==8) is {} bag&&actor.Type!=9)return CanStoreTradeUnit(actor,bag);
  var b=World.Buildings.FirstOrDefault(b=>b.Slot==slot&&(b.Owner==actor.Owner||CanClaimHeliport(actor,slot)));if(Special.Mirrors.Any(m=>m.Seed.Slot==actor.Slot)||b==null||actor.Owner is <0 or >3||(actor.Type!=9&&b.Type!=2)||actor.Health<=0||!VisualCatalog.Visible(actor)||ResidentReserved(actor.Slot)||Occupants(slot)+ExchangeReserved(slot)+ResidentTrips.Count(j=>j.Building==slot)+LaboratoryWorkers.Count(j=>j.LaboratorySlot==slot&&j.Stage==0)>=ResidentCapacity(b))return false;
  var goals=BuildingInteractionCells(b).Where(c=>Passable(c&255,c>>8)).ToHashSet();if(goals.Count==0)return false;
  int start=(actor.Y/1024)*256+actor.X/1024;var seen=new HashSet<int>{start};var queue=new Queue<int>();queue.Enqueue(start);
  while(queue.TryDequeue(out int cell)){if(goals.Contains(cell))return true;foreach(var (dx,dy) in NavigationDirections){int x=(cell&255)+dx,y=(cell>>8)+dy;if(CanTraverse(actor,x,y)&&seen.Add(y*256+x))queue.Enqueue(y*256+x);}}return false;
 }
 public bool EnterBuilding(Actor actor,int slot){
  if(World.Buildings.FirstOrDefault(b=>b.Slot==slot&&b.Type==8) is {} bag&&actor.Type!=9)return StoreTradeUnit(actor,bag);
  if(!CanEnterBuilding(actor,slot))return false;var b=World.Buildings.First(b=>b.Slot==slot);
  foreach(int cell in BuildingInteractionCells(b).OrderBy(c=>Math.Abs((c&255)-actor.X/1024)+Math.Abs((c>>8)-actor.Y/1024)))if(Order(actor,cell&255,cell>>8)){
   foreach(var ids in World.BuildingResidents.Values)ids.Remove(actor.Slot);ResidentTrips.Add(new(){Slot=actor.Slot,Building=slot,Cell=cell});EmitUnitSound(actor,0xa,4);return true;
  }return false;
 }
 public CultureState Culture(int slot){var state=Cultures.FirstOrDefault(c=>c.Building==slot);if(state==null){state=new(){Building=slot};Cultures.Add(state);}return state;}
 // Original BatimentNextEtape crop branch 0x426ca8: 5 minus price bonus, no timer.
 public static int CultureFoodCost(int agricultureLevel)=>CropPresentation.FoodCost(agricultureLevel);
 public int CultureFoodCost()=>CultureFoodCost(AgricultureResearch.Index);
 void PopulationStep(){
  foreach(var trip in ResidentTrips.ToArray()){
   var actor=World.Units.First(a=>a.Slot==trip.Slot);if(actor.Health<=0){ResidentTrips.Remove(trip);continue;}
   var building=World.Buildings.FirstOrDefault(b=>b.Slot==trip.Building);if(Feeding(actor)||building==null||!ReachBuildingInteraction(actor,building,trip.Cell))continue;
   if(CanClaimHeliport(actor,building.Slot)){ClaimHeliport(actor,building);building=World.Buildings.First(b=>b.Slot==building.Slot);}
   if(building.Type==2&&!RevertAtSanctuary(actor,building))continue;
   actor.LegacyState=14;if(!World.BuildingResidents.TryGetValue(trip.Building,out var ids))World.BuildingResidents[trip.Building]=ids=new();ids.Add(actor.Slot);EmitBuildingSound(building,0x27);ResidentTrips.Remove(trip);
  }
  foreach(var culture in Cultures){var b=World.Buildings.FirstOrDefault(b=>b.Slot==culture.Building&&b.Type==4);if(b==null||!Inhabited(b)||culture.Food<CultureFoodCost(b.Owner==0?AgricultureResearch.Index:OpponentLevel(b.Owner,0))||Residents(b.Slot)>=ResidentCapacity(b))continue;
   if(BornActors.Count>=1000||World.Units.Count(a=>a.Health>0)>=NativeEntityCapacity)continue;
   var template=OriginalActorSeeds.Values.First(a=>a.Type==9&&a.Owner==b.Owner);var seed=new Actor{Slot=OriginalActorSeeds.Keys.Max()+BornActors.Count+1,Type=9,Owner=b.Owner,LegacyState=14,Health=template.Health,AnimationCount=template.AnimationCount,AnimationDelay=template.AnimationDelay,X=(b.Cell&255)*1024,Y=(b.Cell>>8)*1024};
   BornActors.Add(CloneState(seed));InitialActors.Add(seed.Slot,CloneState(seed));World.Units.Add(seed);if(!World.BuildingResidents.TryGetValue(b.Slot,out var ids))World.BuildingResidents[b.Slot]=ids=new();ids.Add(seed.Slot);culture.Consumed+=culture.Food;culture.Food=0;culture.Progress=0;culture.Born++;
  }
 }
 public bool CanRecruit(int type,int slot){
  if(!UnitUnlocked(type)||!RecruitData.Rules.Any(r=>r.Type==type)||!World.Buildings.Any(b=>b.Slot==slot&&b.Owner==0&&Produces(b.Type,type))||Recruitments.Any(j=>j.BuildingSlot==slot&&!j.Finished))return false;
  var r=TrainingRule(type);return World.Resources[0]>=r.Food&&World.Resources[4]>=r.Rock&&World.Resources[8]>=r.Parts&&World.BuildingResidents.GetValueOrDefault(slot,new()).Any(id=>World.Units.Any(a=>a.Slot==id&&a.Owner==0&&a.Type==9&&a.Health>0&&a.LegacyState==14)&&!ResidentReserved(id));
 }
 void ValidatePopulation(Save s){
  if(s.Residents==null||s.Trips==null||s.Cultures==null||s.BornActors==null||s.BornActors.Count>1000)throw new InvalidDataException("Invalid population state");
  var buildings=InitialBuildings.Concat(s.Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).ToDictionary(b=>b.Slot);var assigned=new HashSet<int>();
  foreach(var entry in s.Residents){if(!buildings.ContainsKey(entry.Key)||entry.Value==null)throw new InvalidDataException("Invalid resident building");foreach(int id in entry.Value)if(!assigned.Add(id)||!s.Units.Any(a=>a.Slot==id))throw new InvalidDataException("Invalid resident identity");}
  foreach(var trip in s.Trips){if(!buildings.TryGetValue(trip.Building,out var b)||!assigned.Add(trip.Slot)||!s.Units.Any(a=>a.Slot==trip.Slot&&(a.Owner==b.Owner||IsEighthMission&&b.Type==18&&b.Owner==200)&&a.LegacyState==7&&(a.Type==9||b.Type==2))||!ValidBuildingArrivalCells(b).Contains(trip.Cell))throw new InvalidDataException("Invalid resident trip");}
  if(s.Cultures.Select(c=>c.Building).Distinct().Count()!=s.Cultures.Count||s.Cultures.Any(c=>!buildings.TryGetValue(c.Building,out var b)||b.Type!=4||c.Food is <0 or >100||c.Progress is <0 or >299||c.Born<0||c.Consumed<0)||s.Cultures.Sum(c=>c.Born)!=s.BornActors.Count)throw new InvalidDataException("Invalid culture production");
 }
 public void Restore(string json){
  var s=JsonSerializer.Deserialize<Save>(json)??throw new InvalidDataException("Empty save");s=NormalizeSeventhSave(s);json=JsonSerializer.Serialize(s);var oldOcean=Ocean;var oldBuildings=InitialBuildings;var oldCreatures=Creatures;var oldTests=Tests;var oldCampaign=Campaign;var oldSeeds=InitialActors;var oldResidents=CloneState(World.BuildingResidents);
  try{
   InitialActors=CloneState(OriginalActorSeeds);var seeds=s.BornActors??new();int next=OriginalActorSeeds.Keys.Max()+1;var template=OriginalActorSeeds.Values.FirstOrDefault(a=>a.Owner==0&&a.Type==9);
   foreach(var a in seeds){template=OriginalActorSeeds.Values.FirstOrDefault(t=>t.Owner==a.Owner&&t.Type==9);if(template==null||a.Slot!=next++||a.Type!=9||a.Owner is <0 or >3||a.LegacyState!=14||a.Health!=template.Health||a.AnimationCount!=template.AnimationCount||a.AnimationDelay!=template.AnimationDelay)throw new InvalidDataException("Invalid born colonist seed");InitialActors.Add(a.Slot,CloneState(a));}
   World.BuildingResidents.Clear();foreach(var pair in s.Residents??OriginalResidents)World.BuildingResidents[pair.Key]=new(pair.Value);
   RestoreOcean(s);RestoreCreatureSeeds(s);RestoreMirrorSeeds(s);RestoreCore(json);Ocean=s.Ocean??new();Creatures=s.Creatures??new();
   Special=s.Special??new();MigrateExchangeAgreements(s,Special);
   Reversions=s.Reversions??new();BornActors=CloneState(seeds);ResidentTrips=s.Trips??new();Cultures=s.Cultures??new();if(s.Version<=16)foreach(var c in Cultures){c.Consumed=c.Born*10;c.Progress=0;}RetireHistoricalLockedEnemyRecruitments();RestoreExploration(s);RestoreLocalHunger(s.Version);RestoreExchangeColonists();
  }catch{Ocean=oldOcean;InitialBuildings=oldBuildings;Creatures=oldCreatures;Tests=oldTests;Campaign=oldCampaign;InitialActors=oldSeeds;World.BuildingResidents.Clear();foreach(var pair in oldResidents)World.BuildingResidents[pair.Key]=pair.Value;throw;}
 }
}


