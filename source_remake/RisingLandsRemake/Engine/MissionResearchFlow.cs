namespace RisingLands;

public sealed partial class Simulation {
    public const int ResearchExtraFood=50;
    public int MissionResearchCount=>Enumerable.Range(0,4).Sum(b=>ResearchState(b).Completed.Count);
    public int ResearchLimit=>Research.ExtraPurchased?3:2;
    public int ActiveResearchBranch=>Research.ExtraPurchased?Research.ExtraBranch:Research.InitialBranch;
    public bool ExtraResearchOffered=>!Research.ExtraPurchased&&MissionResearchCount==2&&!MissionWon&&!IsSeventhMission&&!IsNinthMission;
    public int ResearchPercent=>MissionResearchCount>=ResearchLimit?100:ResearchInvention(ActiveResearchBranch) is {} t?(int)Math.Clamp(ResearchState(ActiveResearchBranch).Progress*100/Math.Max(1,t.Cost)+1e-7,0,100):0;
    public string ResearchStage=>MissionResearchCount>=3?"3/3 pesquisas concluidas":MissionResearchCount>=2&&!Research.ExtraPurchased?"2/2 pesquisas concluidas":Research.ExtraPurchased?"Pesquisa extra - 3 de 3":"Pesquisa "+(MissionResearchCount+1)+" de 2";
    public int? ResearchSecondsRemaining=>Scientists()==0||!ResearchAvailable(ActiveResearchBranch)||MissionResearchCount>=ResearchLimit?null:Math.Max(0,(int)Math.Ceiling((1-ResearchState(ActiveResearchBranch).Progress/Math.Max(1,ResearchInvention(ActiveResearchBranch)!.Cost))*(Scientists()==1?420:240)-1e-7));
    int SkipResearchGate(int branch,int index){while(UnlockData.Technologies.FirstOrDefault(t=>t.Branch==branch&&t.Index==index) is {} t&&t.Mission>MissionNumber&&t.Appearance<100)index++;return index;}
    bool SupportedBranchResearch(Invention t)=>t.Branch switch{
        0=>SupportedAgriculture(t),2=>SupportedResearch(t),3=>SupportedSpiritualResearch(t),
        _=>t.Effect==0&&t.Param1==1&&RecruitData.Rules.Any(r=>r.Type==t.Param2)||t.Effect==0&&t.Param1==3&&t.Buildings.Length>0&&t.Buildings.All(type=>BuildData.Buildings.Any(b=>b.Type==type&&b.Objects.Count>0))||t.Branch==1&&t.Effect==1&&t.Param1==5&&t.Param2==30
    };
    public bool ResearchAvailable(int branch)=>branch is >=0 and <=3&&ResearchInvention(branch) is {} t&&t.Mission<=MissionNumber&&SupportedBranchResearch(t);
    public string? ExtraResearchError(int branch){
        if(!ExtraResearchOffered)return "Conclua as duas pesquisas obrigatorias";
        if(branch is <0 or >3)return "Escolha um ramo";
        var t=ResearchInvention(branch);if(t==null)return "Ramo no nivel maximo";
        if(t.Mission>MissionNumber)return "Disponivel na missao "+t.Mission;
        if(!SupportedBranchResearch(t))return "Efeito ainda indisponivel no remake";
        if(World.Resources[0]<ResearchExtraFood)return "Alimento insuficiente: precisa de 50";
        return null;
    }
    public bool PurchaseExtraResearch(int branch){
        if(ExtraResearchError(branch)!=null)return false;
        World.Resources[0]-=ResearchExtraFood;Research.ExtraPurchased=true;Research.ExtraBranch=branch;
        // Buying does not cancel a pause explicitly requested by the player.
        RefreshResearchEnabled();return true;
    }
    string ResearchFlowStatus(int branch){
        if(branch!=ActiveResearchBranch)return "Ramo fixado para esta missao";
        if(MissionResearchCount>=ResearchLimit)return Research.ExtraPurchased?"Limite de pesquisas da missao atingido":(IsSeventhMission||IsNinthMission)?"Duas descobertas: objetivo cumprido":"Duas concluidas - pesquisa extra disponivel";
        var t=ResearchInvention(branch);if(t==null)return "Ramo no nivel maximo";
        if(t.Mission>MissionNumber)return "Disponivel na missao "+t.Mission;
        if(!SupportedBranchResearch(t))return "Efeito ainda indisponivel no remake";
        if(Research.ManualPaused)return "Pausada pelo jogador";
        if(!World.Buildings.Any(b=>b.Owner==0&&b.Type==7))return "Construa um laboratorio";
        if(Scientists()==0)return "Sem colonos no laboratorio";
        return "Pesquisando";
    }
    void RefreshResearchEnabled(){for(int b=0;b<4;b++)ResearchState(b).Enabled=b==ActiveResearchBranch&&!Research.ManualPaused&&MissionResearchCount<ResearchLimit&&ResearchAvailable(b)&&Scientists()>0;}
    void AdvanceMissionResearch(){
        if(Scientists()==0){RefreshResearchEnabled();return;}
        int branch=ActiveResearchBranch;var r=ResearchState(branch);
        if(MissionResearchCount<ResearchLimit){int next=SkipResearchGate(branch,r.Index);if(next!=r.Index){r.Index=next;r.Progress=0;}}
        RefreshResearchEnabled();if(!r.Enabled||ResearchInvention(branch) is not {} t)return;
        // 30 simulation updates/second; one researcher = 420 s, two or more = 240 s.
        r.Progress+=Math.Max(1,t.Cost)/(30.0*(Scientists()==1?420:240));
        if(r.Progress+1e-7<t.Cost)return;
        r.Completed.Add(t.Key);r.Index=SkipResearchGate(branch,r.Index+1);r.Progress=0;
        string benefit=t.Branch is 0 or 2?ResearchName(t):t.Effect==0&&t.Param1==1?"Nova unidade desbloqueada":t.Effect==0&&t.Param1==3?BuildName(t.Param2):t.Param1==4?"Novo poder espiritual":t.Param1==10?"Druida: dano +"+t.Param3:"Dano de combate +"+t.Param3;
        ResearchNotice="Pesquisa concluida: "+benefit+". "+MissionResearchCount+" descobertas nesta missao.";
        RefreshResearchEnabled();
        ResearchCompleted?.Invoke(t);
    }
    void MigrateResearchFlow(Save s){
        if(s.Version>=23)return;var r=s.Research;if(r==null)return;
        r.Military=new(){Index=Campaign.StartingLevels[1]};r.Spiritual=new(){Index=Campaign.StartingLevels[3]};
        r.InitialBranch=Campaign.SelectedBranch>=0?Campaign.SelectedBranch:r.Agriculture?.Enabled==true?0:2;
        r.ExtraBranch=-1;r.ExtraPurchased=false;r.ManualPaused=false;
        r.LegacyDiscoveries=r.Completed.Count+(r.Agriculture?.Completed.Count??0);
        foreach(var a in new[]{r,r.Agriculture})if(a!=null&&UnlockData.Technologies.FirstOrDefault(t=>t.Branch==(a==r?2:0)&&t.Index==a.Index) is {} t&&a.Progress>=t.Cost)a.Progress=t.Cost*(1-1e-9);
    }
    void ValidateResearchFlow(Save s){
        var root=s.Research??throw new InvalidDataException("Missing research");
        EngineeringResearch State(int b)=>b switch{0=>root.Agriculture!,1=>root.Military!,2=>root,3=>root.Spiritual!,_=>throw new InvalidDataException("Invalid branch")};
        if(root.InitialBranch is <0 or >3||root.ExtraBranch is < -1 or >3||root.LegacyDiscoveries is <0 or >4||root.ExtraPurchased!=(root.ExtraBranch>=0)||root.ExtraPurchased&&root.LegacyDiscoveries>2)throw new InvalidDataException("Invalid mission research choices");
        int total=0;
        for(int b=0;b<4;b++){
            var r=State(b);if(r==null||r.Completed==null||r.Completed.Count>3||!double.IsFinite(r.Progress)||r.Progress<0||b!=2&&(r.Agriculture!=null||r.Military!=null||r.Spiritual!=null||r.ExtraPurchased||r.ExtraBranch!=-1||r.InitialBranch!=-1||r.ManualPaused||r.LegacyDiscoveries!=0))throw new InvalidDataException("Invalid branch state");
            int cursor=Campaign.StartingLevels[b];
            var grants=Tests.Technologies.Select(k=>UnlockData.Technologies.First(t=>t.Key==k)).Where(t=>t.Branch==b).ToArray();
            var history=r.Completed.Select(k=>UnlockData.Technologies.FirstOrDefault(t=>t.Key==k&&t.Branch==b)??throw new InvalidDataException("Invalid research key")).Concat(grants).OrderBy(t=>t.Index).ToArray();
            if(history.Select(t=>t.Key).Distinct().Count()!=history.Length)throw new InvalidDataException("Duplicate technology discovery");
            foreach(var t in history){bool granted=Tests.Technologies.Contains(t.Key);if(!granted)cursor=SkipResearchGate(b,cursor);if(t.Index!=cursor||!granted&&(!SupportedBranchResearch(t)||t.Mission>MissionNumber))throw new InvalidDataException("Invalid research history");cursor++;}
            var next=UnlockData.Technologies.FirstOrDefault(t=>t.Branch==b&&t.Index==r.Index);
            bool inherited=r.Completed.Count==0&&r.Index==Campaign.StartingLevels[b]&&Campaign.SelectedBranch==b&&r.Progress==Campaign.CarriedProgress;
            if(r.Index!=cursor&&r.Index!=SkipResearchGate(b,cursor)||next!=null&&r.Progress>=Math.Max(1,next.Cost)&&!inherited)throw new InvalidDataException("Invalid research progress");
            total+=r.Completed.Count;
            if(root.LegacyDiscoveries==0&&r.Completed.Count>0&&b!=root.InitialBranch&&(!root.ExtraPurchased||b!=root.ExtraBranch||r.Completed.Count>1))throw new InvalidDataException("Research outside locked branch");
        }
        if(s.LaboratoryWorkers==null||total<root.LegacyDiscoveries||total>Math.Max(root.LegacyDiscoveries,root.ExtraPurchased?3:2)||root.ExtraPurchased&&total<2)throw new InvalidDataException("Invalid research quota");
        if(root.LegacyDiscoveries==0){int initialCount=State(root.InitialBranch).Completed.Count;int mandatory=initialCount-(root.ExtraPurchased&&root.ExtraBranch==root.InitialBranch&&total==3?1:0);if(root.ExtraPurchased&&mandatory!=2)throw new InvalidDataException("Extra research before mandatory discoveries");}
        if(!Campaign.TransitionOpen&&Campaign.SelectedBranch>=0&&root.InitialBranch!=Campaign.SelectedBranch)throw new InvalidDataException("Changed initial research branch");
    }
    public int MilitaryDamageBonus(Actor actor)=>actor.Owner==0?UnlockData.Technologies.Where(t=>t.Branch==1&&TechnologyApplied(t)&&t.Effect==1&&t.Param1==5&&t.Param2==30).Sum(t=>t.Param3):actor.Owner is >0 and <4?UnlockData.Technologies.Where(t=>t.Branch==1&&t.Index<OpponentLevel(actor.Owner,1)&&t.Effect==1&&t.Param1==5&&t.Param2==30).Sum(t=>t.Param3):0;
    public int ConstructionStrikeProgress=>2*UnlockData.Technologies.Where(t=>t.Branch==2&&t.Effect==1&&t.Param1==8&&t.Param2==7&&(t.Index<Campaign.StartingLevels[2]||TechnologyApplied(t))).Select(t=>t.Param3).DefaultIfEmpty(10).Max()/10;
    public int ConstructionStrikeProgressFor(int owner)=>owner==0?ConstructionStrikeProgress:2*UnlockData.Technologies.Where(t=>t.Branch==2&&t.Effect==1&&t.Param1==8&&t.Param2==7&&t.Index<OpponentLevel(owner,2)).Select(t=>t.Param3).DefaultIfEmpty(10).Max()/10;
}
