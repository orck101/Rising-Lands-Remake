namespace RisingLands;
public sealed partial class Simulation {
 public bool IsFifthMission=>World.MapName.Equals("MONDE05.ETA",StringComparison.OrdinalIgnoreCase);
 // Temporary user-requested observation mode: extinction does not end the
 // player's mission5 session. Enemy elimination and normal victory are intact.
 public const bool FifthPlayerDefeatEnabled=false;
 // Approved remake rule, not a claimed reconstruction of native elimination.
 public static bool ClanEliminated(int owner,IEnumerable<Actor> actors,IEnumerable<Building> buildings,IReadOnlyDictionary<int,List<int>>? residents,IEnumerable<LaboratoryWorker>? workers=null,IEnumerable<Recruitment>? recruits=null){
  var living=actors.Where(a=>a.Owner==owner&&a.Health>0&&a.LegacyState is not (0 or 4)).ToArray();
  if(living.Any(a=>a.LegacyState!=14||a.Type!=9||recruits?.Any(j=>!j.Finished&&j.SettlerSlot==a.Slot&&buildings.Any(b=>b.Slot==j.BuildingSlot&&b.Owner==owner))==true))return false; // Includes mine/negotiation/Exchange units.
  return !buildings.Any(b=>b.Owner==owner&&living.Any(a=>a.Type==9&&a.LegacyState==14&&(residents!=null&&residents.TryGetValue(b.Slot,out var ids)&&ids.Contains(a.Slot)||workers?.Any(j=>j.Stage==1&&j.LaboratorySlot==b.Slot&&j.SettlerSlot==a.Slot)==true)));
 }
 public bool ClanEliminated(int owner)=>ClanEliminated(owner,World.Units.Where(a=>!Special.Mirrors.Any(m=>m.Seed.Slot==a.Slot)),World.Buildings,World.BuildingResidents,LaboratoryWorkers,Recruitments);
 public int[] FifthRequiredClans=>Enumerable.Range(1,3).Where(o=>!Allied(o)&&!ClanEliminated(o)).ToArray();
 bool FifthMissionSatisfied(IEnumerable<Actor> actors,IEnumerable<Building> buildings,IReadOnlyDictionary<int,List<int>>? residents,CampaignProgress campaign,ExplorationState? exploration,IEnumerable<LaboratoryWorker>? workers,IEnumerable<Recruitment>? recruits)=>
  !ClanEliminated(0,actors,buildings,residents,workers,recruits)&&Enumerable.Range(1,3).All(o=>exploration?.ClansFound[o]==true&&(campaign.Alliances[o]||ClanEliminated(o,actors,buildings,residents,workers,recruits)));
 void DiscoverFifthClans(){
  if(!IsFifthMission)return;
  foreach(int owner in Enumerable.Range(1,3))if(!Exploration.ClansFound[owner]&&(World.Units.Any(a=>a.Owner==owner&&VisualCatalog.Visible(a)&&InSight(a.Y/1024*256+a.X/1024))||World.Buildings.Any(b=>b.Owner==owner&&BuildingInSight(b))))Exploration.ClansFound[owner]=true;
 }
 public string FifthClanStatus(int owner)=>$"{new[]{"Jogador","Vermelho","Amarelo","Verde"}[owner]}: "+(!Exploration.ClansFound[owner]?"nao encontrado":Allied(owner)?"aliado":ClanEliminated(owner)?"eliminado":"eliminar");
}
