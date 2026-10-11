namespace RisingLands;
public sealed class WorkerResourceOrders {
    readonly Dictionary<int,bool> pending=new();
    public int WorkerSlot{get;private set;}=-1;
    public bool Mining{get;private set;}
    public bool WaitingFor(Actor u)=>u.Food>0||u.Rock>0||pending.ContainsKey(u.Slot)||u.FoodDestinationKind==0&&(u.FoodJob!=0||u.Food>0)||u.RockDestination<0&&u.RockGround<0&&(u.MineJob!=0||u.Rock>0);
    public void Cancel(){pending.Clear();WorkerSlot=-1;Mining=false;}
    public static bool SourceAvailable(Simulation sim,Actor u,int cell)=>cell>=0&&cell<65536&&(cell&255)<sim.World.Width&&(cell>>8)<sim.World.Height&&u.Owner==0&&OriginalWorkerPolicy.Carrier(u.Type)&&VisualCatalog.Visible(u)&&
        (sim.BuildingAt(cell) is {} depot&&sim.WarehouseSourceAvailable(u,depot)||sim.IsResourcePile(cell)&&OriginalWorkerPolicy.Collects(u.Type,sim.World.Objects[cell]-66)||OriginalWorkerPolicy.Collects(u.Type,1)&&sim.IsFood(cell)||u.Type==7&&sim.MineAt(cell) is int index&&index>=0&&sim.World.Mines[index].Quantity>0);
    public static bool GroupSourceAvailable(Simulation sim,IReadOnlyList<Actor> units,int cell)=>units.Count>0&&units.All(u=>SourceAvailable(sim,u,cell));
    public int SourceGroup(Simulation sim,IReadOnlyList<Actor> units,int cell){
        if(!GroupSourceAvailable(sim,units,cell))return 0;
        return units.Count(u=>Source(sim,u,cell));
    }
    public int DestinationGroup(Simulation sim,IReadOnlyList<Actor> units,int cell)=>units.Count(u=>WaitingFor(u)&&Destination(sim,u,cell));
    public bool Source(Simulation sim,Actor u,int cell){
        if(!SourceAvailable(sim,u,cell))return false;
        var warehouse=sim.BuildingAt(cell);bool fromWarehouse=warehouse!=null&&sim.WarehouseSourceAvailable(u,warehouse);
        bool mine=!fromWarehouse&&u.Type==7&&!sim.IsResourcePile(cell)&&sim.MineAt(cell)>=0;
        int previousKind=u.FoodDestinationKind,previousTarget=u.FoodDestination;
        if(!mine){u.FoodDestinationKind=0;u.FoodDestination=-1;}
        if(!(fromWarehouse?sim.GatherWarehouse(u,warehouse!.Slot):mine?sim.StartMining(u,sim.MineAt(cell)):sim.Gather(u,cell))){u.FoodDestinationKind=previousKind;u.FoodDestination=previousTarget;return false;}
        if(mine){u.RockDestination=-1;u.RockGround=-1;}else{u.FoodDestinationKind=0;u.FoodDestination=-1;}
        u.RepeatFood=u.RepeatMine=true;u.PersistentCollection=true;u.OverflowCell=-1;
        pending[u.Slot]=mine;WorkerSlot=u.Slot;Mining=mine;return true;
    }
    public bool DestinationAvailable(Simulation sim,Actor u,int cell){
        if(!WaitingFor(u))return false;
        var b=sim.BuildingAt(cell);bool mine=u.Rock>0||u.Food==0&&(pending.TryGetValue(u.Slot,out bool remembered)?remembered:u.MineJob!=0);
        return mine?(b!=null?sim.AcceptsFood(b):sim.RockGroundAllowed(cell)):sim.FoodDestinationError(u,b==null?1:2,b?.Slot??cell)==null;
    }
    public bool Destination(Simulation sim,Actor u,int cell){
        if(!WaitingFor(u))return false;
        var building=sim.BuildingAt(cell);
        bool mining=u.Rock>0||u.Food==0&&(pending.TryGetValue(u.Slot,out bool remembered)?remembered:u.MineJob!=0);
        bool ok=mining?(building!=null?sim.SetRockDestination(u,building.Slot):sim.SetRockGround(u,cell)):sim.SetFoodDestination(u,building==null?1:2,building?.Slot??cell);
        if(ok){pending.Remove(u.Slot);WorkerSlot=pending.Keys.LastOrDefault(-1);Mining=WorkerSlot>=0&&pending[WorkerSlot];}return ok;
    }
}
