namespace RisingLands;
public sealed partial class Simulation {
    public int MineAt(int cell)=>World.Mines.FindIndex(m=>Inside(cell,m.Cell,2,2));
    IEnumerable<int> MineWorkCells(Mine m){int x=m.Cell&255,y=m.Cell>>8;for(int dy=-1;dy<=2;dy++)for(int dx=-1;dx<=2;dx++)if((dx<0||dx>1||dy<0||dy>1)&&x+dx>=0&&y+dy>=0&&x+dx<World.Width&&y+dy<World.Height)yield return (y+dy)*256+x+dx;}
    public string MiningStatus(Actor u)=>u.MineJob switch{1=>"Indo para a jazida",2=>"Extraindo rocha",3=>"Entregando rocha",4=>"Aguardando acesso ou espaco",_=>u.Rock>0?"Rocha: escolha a entrega":"Aguardando ordem"};
    public bool StartMining(Actor u,int index,bool respond=true){
        if(u.Owner is <0 or >3||u.Type!=7||!VisualCatalog.Visible(u)||u.Rock+u.Food>=Capacity(u)||index<0||index>=World.Mines.Count||World.Mines[index].Quantity==0)return false;
        var m=World.Mines[index];if(!OpponentLocalResource(u,m.Cell))return false;
        foreach(int cell in new[]{m.Cell})if(Order(u,cell&255,cell>>8,true)){
            CancelFeeding(u);if(respond)EmitUnitSound(u,0xf,4);DetachBuilder(u.Slot);
            u.OverflowCell=-1;u.FoodJob=0;u.MineIndex=index;u.MineWorkCell=cell;u.MineJob=1;u.MineTicks=0;return true;
        }
        return false;
    }
    public bool MineNearest(Actor u){foreach(int i in Enumerable.Range(0,World.Mines.Count).OrderBy(i=>Math.Abs((World.Mines[i].Cell&255)-u.X/1024)+Math.Abs((World.Mines[i].Cell>>8)-u.Y/1024)))if(StartMining(u,i))return true;return false;}
    public bool SetRockDestination(Actor u,int slot){
        if(u.Type!=7||u.Owner is <0 or >3||u.Food>0||!VisualCatalog.Visible(u))return false;
        var b=World.Buildings.FirstOrDefault(b=>b.Slot==slot&&b.Owner==u.Owner&&AcceptsFood(b));if(b==null)return false;
        var route=u.Route;
        foreach(int cell in BuildingInteractionCells(b).OrderBy(c=>Math.Abs((c&255)-u.X/1024)+Math.Abs((c>>8)-u.Y/1024)))if(Order(u,cell&255,cell>>8,true)){
            CancelFeeding(u);u.OverflowCell=-1;u.RockDestination=slot;u.RockGround=-1;if(u.Rock>0){u.FoodJob=u.WorkTicks=0;DetachBuilder(u.Slot);u.MineWorkCell=cell;u.MineJob=3;u.MineTicks=0;}else u.Route=route;return true;
        }
        return false;
    }
    public bool RockGroundAllowed(int cell)=>cell>=0&&cell<65536&&Passable(cell&255,cell>>8)&&BuildingAt(cell)==null&&MineAt(cell)<0&&(World.Objects[cell]==0||World.Objects[cell]==68&&World.Quantities[cell]<31);
    public bool SetRockGround(Actor u,int cell){
        if(u.Type!=7||u.Owner is <0 or >3||u.Food>0||!VisualCatalog.Visible(u)||!RockGroundAllowed(cell))return false;
        var route=u.Route;if(!Order(u,cell&255,cell>>8,true))return false;
        CancelFeeding(u);u.OverflowCell=-1;u.RockGround=cell;u.RockDestination=-1;
        if(u.Rock>0){u.FoodJob=u.WorkTicks=0;DetachBuilder(u.Slot);u.MineWorkCell=cell;u.MineJob=3;u.MineTicks=0;}else u.Route=route;
        return true;
    }
    void DeliverRock(Actor u){u.OverflowCell=-1;var overflowDepot=World.Buildings.FirstOrDefault(b=>b.Slot==u.RockDestination&&b.Owner==u.Owner&&AcceptsFood(b)&&Inhabited(b));if(overflowDepot!=null&&WarehouseSpace(u.Owner)==0){RouteWarehouseOverflow(u,overflowDepot,true);return;}if(u.RockGround>=0){if(!SetRockGround(u,u.RockGround))u.MineJob=4;return;}if(u.RockDestination<0){u.MineJob=0;return;}if(!SetRockDestination(u,u.RockDestination))u.MineJob=4;}
    void DepleteMine(Mine mine){World.Tiles[mine.Cell]=0x1db;World.Tiles[mine.Cell+1]=0x1dc;}
    void MiningStep(Actor u){
        if(u.MineJob==0)return;
        if(u.MineJob==3&&u.OverflowCell>=0){OverflowArrival(u,true);return;}
        if(u.MineJob==4){if(Tick%30==0){if(u.Rock>0)DeliverRock(u);else ContinueMine(u);}return;}
        int arrivalX=(u.MineWorkCell&255)*1024,arrivalY=(u.MineWorkCell>>8)*1024;
        if(u.MineJob==3&&u.RockGround<0){var building=World.Buildings.FirstOrDefault(b=>b.Slot==u.RockDestination);if(building==null){u.MineJob=4;return;}if(!ReachBuildingInteraction(u,building,u.MineWorkCell))return;arrivalX=u.X;arrivalY=u.Y;}
        if(u.MineJob is 1 or 2&&u.MineWorkCell==World.Mines[u.MineIndex].Cell){
            int doorX=arrivalX+17*32,doorY=arrivalY+9*32;
            if(u.MineJob==1){u.X+=Math.Clamp(doorX-u.X,-LabStep,LabStep);u.Y+=Math.Clamp(doorY-u.Y,-LabStep,LabStep);u.Direction=5;}
            arrivalX=doorX;arrivalY=doorY;
        }
        if(u.X!=arrivalX||u.Y!=arrivalY){if(u.MineJob!=1)u.MineJob=4;return;}
        if(u.MineJob is 1 or 2){
            var mine=World.Mines[u.MineIndex];if(mine.Quantity==0){if(u.Rock>0)DeliverRock(u);else ContinueMine(u);return;}
            if(u.MineJob==1){u.MineJob=2;EmitUnitSound(u,0x10);u.MineTicks=WorkerCountdown(u);return;} // Native RECHARGE minus technology bonus; real-time cadence remains pending.
            if(--u.MineTicks>0)return;
            mine.Quantity--;u.Rock++;if(mine.Quantity==0)DepleteMine(mine);
            if(u.Rock<Capacity(u)){u.MineTicks=50;return;} // Native continuation reload at 0x44c175.
            DeliverRock(u);return;
        }
        if(u.RockGround>=0){
            int cell=u.RockGround;if(!RockGroundAllowed(cell)||cell!=u.MineWorkCell){u.MineJob=4;return;}
            int quantity=World.Objects[cell]==68?World.Quantities[cell]:0;
            int delivered=Math.Min(u.Rock,31-quantity);World.Objects[cell]=68;World.Quantities[cell]=(byte)(quantity+delivered);u.Rock-=delivered;
            u.MineJob=u.Rock>0?4:0;if(u.Rock==0&&u.RepeatMine)ContinueMine(u);return;
        }
        var depot=World.Buildings.FirstOrDefault(b=>b.Slot==u.RockDestination&&b.Owner==u.Owner&&AcceptsFood(b));
        if(depot==null||!Inhabited(depot)||!ValidBuildingArrivalCells(depot).Contains(u.MineWorkCell)){u.MineJob=4;return;}
        int amount=Math.Min(u.Rock,WarehouseSpace(u.Owner));World.Resources[4+u.Owner]+=(ushort)amount;if(u.Owner>0)Opponent.Deposited[4+u.Owner]+=amount;u.Rock-=amount;if(u.Rock>0){RouteWarehouseOverflow(u,depot,true);return;}u.MineJob=0;
        if(u.Rock==0&&u.RepeatMine)ContinueMine(u);
    }
    void ValidateMiningSave(Save s){
        if(s.Mines==null||s.Mines.Count!=InitialMines.Count)throw new InvalidDataException("Invalid mine collection");
        for(int i=0;i<s.Mines.Count;i++)if(s.Mines[i]==null||s.Mines[i].Cell!=InitialMines[i].Cell||s.Mines[i].Quantity<0||s.Mines[i].Quantity>InitialMines[i].Quantity)throw new InvalidDataException("Invalid mine reserve");
        var buildings=InitialBuildings.Concat(s.Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).ToArray();
        foreach(var u in s.Units){
            if(u.Rock<0||u.Rock>CargoCapacity(u,s.Research)||u.MineJob is <0 or >4||u.MineTicks is <0 or >255||u.MineIndex< -1||u.MineIndex>=InitialMines.Count||u.MineWorkCell< -1||u.MineWorkCell>=65536||(u.Rock>0||u.MineJob!=0||u.RockDestination!=-1||u.RockGround!=-1)&&(u.Owner is <0 or >3||u.Type!=7)||u.MineJob!=0&&(u.MineIndex<0||u.MineWorkCell<0)||u.RockDestination!= -1&&!buildings.Any(b=>b.Slot==u.RockDestination&&b.Owner==u.Owner&&AcceptsFood(b))||u.MineJob==3&&(u.Rock==0||u.RockDestination<0&&u.RockGround<0))throw new InvalidDataException("Invalid mining order");
            if(u.RockGround< -1||u.RockGround>=65536||u.RockGround>=0&&((u.RockGround&255)>=World.Width||(u.RockGround>>8)>=World.Height||u.RockDestination>=0)||u.MineJob==3&&u.RockGround>=0&&u.MineWorkCell!=u.RockGround)throw new InvalidDataException("Invalid rock ground destination");
            if(u.MineJob is 1 or 2&&u.Rock>=CargoCapacity(u,s.Research))throw new InvalidDataException("Mining carrier already full");
            if(u.MineJob is 1 or 2&&u.MineWorkCell!=InitialMines[u.MineIndex].Cell&&!MineWorkCells(InitialMines[u.MineIndex]).Contains(u.MineWorkCell))throw new InvalidDataException("Invalid mine working cell");
            if(u.MineJob!=0&&s.Constructions.Any(c=>HasBuilder(c,u.Slot)&&!Complete(c)))throw new InvalidDataException("Builder has conflicting work");
        }
        var spent=Spending(s.Constructions,s.Recruitments,s.Special??new(),s.TrainingQueue??new());for(int k=0;k<12;k++)spent[k]+=OpeningDebit(s,k);
        if(s.Mines.Sum(m=>(long)m.Quantity)+s.Units.Sum(u=>(long)u.Rock+(u.CargoKind==2?u.Food:0))+PileTotal(s.Objects,s.Quantities,68)+s.Resources.Skip(4).Take(4).Sum(v=>(long)v)+spent.Skip(4).Take(4).Sum()!=InitialMines.Sum(m=>(long)m.Quantity)+PileTotal(InitialObjects,InitialQuantities,68)+InitialResources.Skip(4).Take(4).Sum(v=>(long)v)+(s.Tests?.Rock??0))throw new InvalidDataException("Rock conservation failed");
        for(int i=5;i<8;i++)if(s.Resources[i]!=InitialResources[i]-spent[i]+TradeNet(s,1,i%4)+(s.Opponent?.Deposited[i]??0))throw new InvalidDataException("Unsupported foreign rock mutation");

    }
}
