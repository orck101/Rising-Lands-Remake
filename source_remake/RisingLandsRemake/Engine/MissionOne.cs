namespace RisingLands;
public sealed partial class Simulation {
    public bool IsFirstMission=>World.MapName.Equals("MONDE01.ETA",StringComparison.OrdinalIgnoreCase);
    public int MissionChecks{get;private set;}
    public bool IsSecondMission=>World.MapName.Equals("MONDE02.ETA",StringComparison.OrdinalIgnoreCase);
    public bool IsThirdMission=>World.MapName.Equals("MONDE03.ETA",StringComparison.OrdinalIgnoreCase);
    public bool IsFourthMission=>World.MapName.Equals("MONDE04.ETA",StringComparison.OrdinalIgnoreCase);
    public bool HasMissionEnding=>IsFirstMission||IsSecondMission||IsThirdMission||IsFourthMission||IsFifthMission||IsSixthMission||IsSeventhMission||IsEighthMission||IsNinthMission||Tests.Victory;
    public bool MissionWon=>HasMissionEnding&&MissionChecks==100;
    public static bool InSecondMissionZone(int cell)=>cell>=0&&(cell&255)>=36&&(cell&255)<=63&&(cell>>8)<=23;
    public static bool SecondMissionSatisfied(IEnumerable<Actor> actors,IEnumerable<Building> buildings,IEnumerable<Construction> sites,int food,IReadOnlyDictionary<int,List<int>>? residents=null)=>
        actors.Any(u=>u.Owner==0&&u.Type!=17&&u.LegacyState is not (0 or 4)&&u.Health>0)&&food>=50&&
        new[]{1,2}.All(type=>buildings.Any(b=>b.Owner==0&&b.Type==type&&InSecondMissionZone(b.Cell)&&
            residents!=null&&residents.TryGetValue(b.Slot,out var ids)&&actors.Any(u=>ids.Contains(u.Slot)&&u.Owner==0&&u.Type==9&&u.Health>0&&u.LegacyState==14)));
    bool FourthMissionSatisfied(IEnumerable<Actor> actors,IEnumerable<Building> buildings,IReadOnlyDictionary<int,List<int>>? residents,bool allied)=>
        actors.Any(a=>a.Owner==0&&a.Health>0)&&buildings.Any(b=>b.Owner==0&&b.Type==16&&residents!=null&&residents.TryGetValue(b.Slot,out var ids)&&actors.Any(a=>ids.Contains(a.Slot)&&a.Type==9&&a.Health>0&&a.LegacyState==14))&&
        (allied||!buildings.Any(b=>b.Owner==1)&&!actors.Any(a=>a.Owner==1&&VisualCatalog.Visible(a)));
    static bool ThirdMissionSatisfied(IEnumerable<Actor> actors,IEnumerable<Building> buildings,EngineeringResearch? research)=>
        actors.Any(a=>a.Owner==0&&a.Health>0)&&buildings.Any(b=>b.Owner==0&&b.Type==7)&&research is {ExtraPurchased:true}&&
        research.Completed.Count+(research.Agriculture?.Completed.Count??0)+(research.Military?.Completed.Count??0)+(research.Spiritual?.Completed.Count??0)>=3;
    bool MissionSatisfied(IEnumerable<Actor> actors,IEnumerable<Building> buildings,IEnumerable<Construction> sites,int food,int rock,IReadOnlyDictionary<int,List<int>>? residents=null)=>IsFirstMission?FirstMissionSatisfied(actors,buildings,sites,food,rock):IsSecondMission?SecondMissionSatisfied(actors,buildings,sites,food,residents):IsThirdMission?ThirdMissionSatisfied(actors,buildings,Research):IsFifthMission?FifthMissionSatisfied(actors.Where(a=>!Special.Mirrors.Any(m=>m.Seed.Slot==a.Slot)),buildings,residents,Campaign,Exploration,LaboratoryWorkers,Recruitments):IsNinthMission?NinthMissionSatisfied(actors,Research):IsEighthMission?EighthSatisfied(0,actors,buildings):IsSeventhMission?SeventhMissionSatisfied(actors,Research):IsSixthMission?SixthMissionSatisfied():IsFourthMission&&FourthMissionSatisfied(actors,buildings,residents,Allied(1));
    bool SavedMissionSatisfied(Save s){var buildings=InitialBuildings.Concat(s.Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).Where(b=>(s.Opponent?.BuildingDamage.GetValueOrDefault(b.Slot)??0)<BuildingDamageLimits[b.Type]);return IsNinthMission?NinthMissionSatisfied(s.Units,s.Research):IsEighthMission?EighthSatisfied(0,s.Units,buildings):IsSeventhMission?SeventhMissionSatisfied(s.Units,s.Research):IsFifthMission?FifthMissionSatisfied(s.Units.Where(a=>s.Special?.Mirrors.Any(m=>m.Seed.Slot==a.Slot)!=true),buildings,s.Residents,s.Campaign!,s.Exploration,s.LaboratoryWorkers,s.Recruitments):IsSixthMission?SavedSixthMissionSatisfied(s,buildings):IsThirdMission?ThirdMissionSatisfied(s.Units,buildings,s.Research):IsFourthMission?FourthMissionSatisfied(s.Units,buildings,s.Residents,s.Campaign?.Alliances[1]==true):MissionSatisfied(s.Units,buildings,s.Constructions,s.Resources[0],s.Resources[4],s.Residents);}
    public static bool InFirstMissionZone(int cell)=>cell>=0&&(cell&255)<=28&&(cell>>8)<=25;
    public static bool FirstMissionSatisfied(IEnumerable<Actor> actors,IEnumerable<Building> buildings,IEnumerable<Construction> sites,int food,int rock)=>
        actors.Any(u=>u.Owner==0&&u.Type!=17&&u.LegacyState is not (0 or 4)&&u.Health>0)&&food>=50&&rock>=50&&
        (buildings.Any(b=>b.Owner==0&&b.Type==3&&InFirstMissionZone(b.Cell))||sites.Any(c=>c.Type==3&&InFirstMissionZone(c.Cell)));
    public bool FirstMissionWarehouse=>World.Buildings.Any(b=>b.Owner==0&&b.Type==3&&InFirstMissionZone(b.Cell))||Constructions.Any(c=>c.Type==3&&InFirstMissionZone(c.Cell));
    void MissionStep(){
        // Mission 2 requires completed, currently occupied objectives for consecutive evaluations.
        // One evaluation per modern tick is provisional, not an original wall-clock timing claim.
        if(!HasMissionEnding||MissionChecks>=100)return;
        if(MissionSatisfied(World.Units,World.Buildings,Constructions,World.Resources[0],World.Resources[4],World.BuildingResidents)){MissionChecks++;if(MissionChecks==100)SoundIssued?.Invoke(new("A",0x40100,3,0,0,-1));}
        else if(IsSecondMission||IsThirdMission||IsFourthMission||IsFifthMission||IsSixthMission||IsSeventhMission||IsEighthMission||IsNinthMission)MissionChecks=0;
    }
    public void RequestNextMission(){if(MissionWon)Campaign.TransitionOpen=true;}
    void ValidateMissionSave(Save s){
        if(s.MissionChecks<0||s.MissionChecks>100||s.MissionChecks>s.Tick&&!Tests.Victory||!HasMissionEnding&&s.MissionChecks!=0)throw new InvalidDataException("Invalid mission progress");
        if(s.MissionChecks==100&&!Tests.Victory&&!SavedMissionSatisfied(s))throw new InvalidDataException("Victory requirements not met");
    }
}
