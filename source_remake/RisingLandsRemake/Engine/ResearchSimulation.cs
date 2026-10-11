namespace RisingLands;

public sealed class EngineeringResearch {
    public int Index{get;set;}
    public double Progress{get;set;}
    public bool Enabled{get;set;}
    public List<string> Completed{get;set;}=new();
    public EngineeringResearch? Agriculture{get;set;}
    public EngineeringResearch? Military{get;set;}
    public EngineeringResearch? Spiritual{get;set;}
    public int InitialBranch{get;set;}=-1;
    public bool ManualPaused{get;set;}
    public int ExtraBranch{get;set;}=-1;
    public bool ExtraPurchased{get;set;}
    public int LegacyDiscoveries{get;set;}

}
public sealed class LaboratoryWorker {
    public int SettlerSlot{get;set;}
    public int HomeSlot{get;set;}
    public int LaboratorySlot{get;set;}
    public int ArrivalCell{get;set;}
    public int Stage{get;set;} // 0: outbound, 1: working inside, 2: returning, 3: interrupted.
}
public sealed partial class Simulation {
    public EngineeringResearch Research{get;private set;}=new();
    public List<LaboratoryWorker> LaboratoryWorkers{get;private set;}=new();
    public string ResearchNotice{get;private set;}="";
    EngineeringResearch InitialResearch()=>new(){InitialBranch=Campaign.SelectedBranch>=0?Campaign.SelectedBranch:2,Index=Campaign.StartingLevels[2],Progress=Campaign.SelectedBranch==2?Campaign.CarriedProgress:0,Agriculture=new(){Index=Campaign.StartingLevels[0],Progress=Campaign.SelectedBranch==0?Campaign.CarriedProgress:0},Military=new(){Index=Campaign.StartingLevels[1],Progress=Campaign.SelectedBranch==1?Campaign.CarriedProgress:0},Spiritual=new(){Index=Campaign.StartingLevels[3],Progress=Campaign.SelectedBranch==3?Campaign.CarriedProgress:0}};
    public Invention? CurrentResearch=>CivilResearch(Research.Index);
    static Invention? CivilResearch(int index)=>UnlockData.Technologies.FirstOrDefault(t=>t.Branch==2&&t.Index==index);
    // Building unlocks and demonstrated builder carry/reload bonuses are enabled.
    static bool SupportedResearch(Invention t)=>t.Effect==1&&t.Param1 is 1 or 3 or 8&&t.Param2==7||t.Effect==0&&t.Param1==3&&t.Buildings.Length>0&&t.Buildings.All(type=>BuildData.Buildings.Any(b=>b.Type==type&&b.Objects.Count>0));
    public EngineeringResearch AgricultureResearch=>Research.Agriculture!;
    public EngineeringResearch ResearchState(int branch)=>branch switch{0=>AgricultureResearch,1=>Research.Military!,2=>Research,3=>Research.Spiritual!,_=>throw new ArgumentOutOfRangeException(nameof(branch))};
    public Invention? ResearchInvention(int branch)=>UnlockData.Technologies.FirstOrDefault(t=>t.Branch==branch&&t.Index==ResearchState(branch).Index);
    static bool SupportedAgriculture(Invention t)=>t.Branch==0&&(t.Effect==0&&t.Param1==1&&RecruitData.Rules.Any(r=>r.Type==t.Param2)&&ProductionUnlocks.GetValueOrDefault(t.Key,Array.Empty<int>()).Contains(t.Param2)||t.Effect==1&&t.Param1==0||t.Effect==1&&t.Param2==5&&t.Param1 is 1 or 3||t.Effect==1&&t.Param1==2&&t.Param2==9||t.Effect==0&&t.Param1==3&&t.Buildings.Length>0&&t.Buildings.All(type=>BuildData.Buildings.Any(b=>b.Type==type&&b.Objects.Count>0)));
    public string ResearchName(Invention? t)=>t is {Effect:0,Param1:1}?t.Param2 switch{25=>"Stomper desbloqueado",13=>"Carro coletor desbloqueado",_=>"Nova unidade desbloqueada"}:t?.Branch==0?(t.Effect==1&&t.Param1==0?"Menor necessidade de alimento":t.Param1==2&&t.Param2==9?"Cultivo precisa de menos comida":t.Param1==3&&t.Effect==1?"Colheita mais rapida":t.Param1==1&&t.Effect==1?"Fazendeiro transporta mais comida":string.Join(" / ",t.Buildings.Select(BuildName))):t==null?"Engenharia concluida":t.Buildings.Length>0?string.Join(" / ",t.Buildings.Select(BuildName)):t.Effect==1&&t.Param1==1&&t.Param2==7?"Construtor transporta mais pedra":t.Effect==1&&t.Param1==3&&t.Param2==7?"Mineracao mais rapida":t.Key=="GENIECIVIL8"?"Construcao mais rapida":"Melhoria de engenharia";
    public int LaboratoryCapacity=>2; // RISING.INI LABORATOIRE LOGEMENT.
    public int Scientists(int? lab=null)=>World.Buildings.Where(b=>b.Owner==0&&b.Type==7&&(lab==null||b.Slot==lab)).Sum(b=>
        World.BuildingResidents.GetValueOrDefault(b.Slot,new()).Count(slot=>World.Units.Any(u=>u.Slot==slot&&u.Type==9&&u.Owner==0&&u.Health>0&&u.LegacyState==14)&&!LaboratoryWorkers.Any(j=>j.SettlerSlot==slot)&&!Recruitments.Any(j=>j.SettlerSlot==slot))
        +LaboratoryWorkers.Count(j=>j.LaboratorySlot==b.Slot&&j.Stage==1&&World.Units.Any(u=>u.Slot==j.SettlerSlot&&u.Type==9&&u.Owner==0&&u.Health>0&&u.LegacyState==14)));
    public string ResearchStatus(int branch=2)=>ResearchFlowStatus(branch);
    public bool ToggleResearch(int branch=2){if(branch!=ActiveResearchBranch||!ResearchAvailable(branch)||MissionResearchCount>=ResearchLimit||!World.Buildings.Any(b=>b.Owner==0&&b.Type==7))return false;Research.ManualPaused=!Research.ManualPaused;RefreshResearchEnabled();return true;}
    bool ResearchReserved(int slot)=>LaboratoryWorkers.Any(j=>j.SettlerSlot==slot);
    void CancelResearchTrip(Actor u){var j=LaboratoryWorkers.FirstOrDefault(j=>j.SettlerSlot==u.Slot&&j.Stage!=1);if(j!=null)j.Stage=3;}
    bool RouteResident(Actor u,Building from,Building to){
        int x=u.X,y=u.Y,state=u.LegacyState;var route=u.Route;
        var targets=BuildingInteractionCells(to).Where(i=>Passable(i&255,i>>8)).ToArray();
        var exits=VisualCatalog.Visible(u)?new[]{u.Y/1024*256+u.X/1024}:FrontCells(from).Where(TraversableBuildingExit).OrderBy(i=>Math.Abs((i&255)-(to.Cell&255))+Math.Abs((i>>8)-(to.Cell>>8))).ToArray();
        foreach(int exit in exits){if(state==14)PlaceAtExit(u,from,exit);else {u.X=x;u.Y=y;}u.LegacyState=7;
            foreach(int goal in targets.OrderBy(i=>Math.Abs((i&255)-(exit&255))+Math.Abs((i>>8)-(exit>>8))))if(Order(u,goal&255,goal>>8,true))return true;
        }
        u.X=x;u.Y=y;u.LegacyState=state;u.Route=route;return false;
    }
    public bool AssignLaboratoryWorker(int labSlot){
        var lab=World.Buildings.FirstOrDefault(b=>b.Slot==labSlot&&b.Owner==0&&b.Type==7);if(lab==null)return false;
        int reserved=Scientists(labSlot)+LaboratoryWorkers.Count(j=>j.LaboratorySlot==labSlot&&j.Stage!=1);if(reserved>=LaboratoryCapacity)return false;
        foreach(var home in World.Buildings.Where(b=>b.Owner==0&&b.Type==2).OrderBy(b=>Math.Abs((b.Cell&255)-(lab.Cell&255))+Math.Abs((b.Cell>>8)-(lab.Cell>>8)))){
            foreach(int slot in World.BuildingResidents.GetValueOrDefault(home.Slot,new())){
                var u=World.Units.FirstOrDefault(a=>a.Slot==slot&&a.Type==9&&a.Owner==0&&a.LegacyState==14&&a.Health>0);
                if(u==null||ResearchReserved(slot)||Recruitments.Any(j=>j.SettlerSlot==slot)||!RouteResident(u,home,lab))continue;
                LaboratoryWorkers.Add(new(){SettlerSlot=slot,HomeSlot=home.Slot,LaboratorySlot=labSlot,ArrivalCell=u.Route.LastOrDefault(u.Y/1024*256+u.X/1024)});return true;
            }
        }
        return false;
    }
    public bool ReturnLaboratoryWorker(int labSlot){
        foreach(var j in LaboratoryWorkers.Where(j=>j.LaboratorySlot==labSlot&&j.Stage!=2).OrderBy(j=>j.Stage==1?0:1)){
            var u=World.Units.First(a=>a.Slot==j.SettlerSlot);if(u.Health<=0)continue;
            var lab=World.Buildings.First(b=>b.Slot==j.LaboratorySlot);var home=World.Buildings.First(b=>b.Slot==j.HomeSlot);
            if(!RouteResident(u,lab,home))continue;j.ArrivalCell=u.Route.LastOrDefault(u.Y/1024*256+u.X/1024);j.Stage=2;return true;
        }
        return false;
    }
    void ResearchStep(){
        foreach(var j in LaboratoryWorkers.ToArray()){
            var u=World.Units.First(a=>a.Slot==j.SettlerSlot);if(u.Health<=0){if(j.Stage!=1)j.Stage=3;continue;}
            if(Feeding(u)||j.Stage is not (0 or 2))continue;var target=World.Buildings.First(b=>b.Slot==(j.Stage==2?j.HomeSlot:j.LaboratorySlot));if(!ReachBuildingInteraction(u,target,j.ArrivalCell))continue;
            u.LegacyState=14;if(j.Stage==2)LaboratoryWorkers.Remove(j);else j.Stage=1;
        }
        AdvanceMissionResearch();
    }
    void ValidateResearchSave(Save s){
        ValidateResearchFlow(s);
        var buildings=InitialBuildings.Concat(s.Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).ToArray();var slots=new HashSet<int>();
        foreach(var j in s.LaboratoryWorkers!){
            if(!slots.Add(j.SettlerSlot)||j.Stage is <0 or >3||!InitialActors.TryGetValue(j.SettlerSlot,out var a)||a.Type!=9||a.Owner!=0||a.LegacyState!=14||!World.BuildingResidents.GetValueOrDefault(j.HomeSlot,new()).Contains(j.SettlerSlot)||!buildings.Any(b=>b.Slot==j.HomeSlot&&b.Owner==0&&b.Type==2)||!buildings.Any(b=>b.Slot==j.LaboratorySlot&&b.Owner==0&&b.Type==7)||s.Recruitments.Any(t=>t.SettlerSlot==j.SettlerSlot))throw new InvalidDataException("Invalid laboratory assignment");
            var destination=buildings.First(b=>b.Slot==(j.Stage==2?j.HomeSlot:j.LaboratorySlot));
            if(j.Stage!=3&&!ValidBuildingArrivalCells(destination).Contains(j.ArrivalCell)||j.ArrivalCell<0||j.ArrivalCell>=65536)throw new InvalidDataException("Invalid laboratory route");
            var u=s.Units.First(u=>u.Slot==j.SettlerSlot);if(j.Stage==1&&(u.Route.Count!=0||!AtBuildingArrival(u,buildings.First(b=>b.Slot==j.LaboratorySlot),j.ArrivalCell)))throw new InvalidDataException("Invalid laboratory resident");
        }
        foreach(var b in buildings.Where(b=>b.Type==7))if(s.LaboratoryWorkers.Count(j=>j.LaboratorySlot==b.Slot)>LaboratoryCapacity)throw new InvalidDataException("Laboratory capacity exceeded");
    }
}
