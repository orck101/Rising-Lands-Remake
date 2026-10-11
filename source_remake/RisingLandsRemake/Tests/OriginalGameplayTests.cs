namespace RisingLands;
public static class OriginalGameplayTests {
    static void Check(bool ok,string message){if(!ok)throw new Exception("FAIL: "+message);Console.WriteLine("PASS: "+message);}
    public static void Run(string root){
        Simulation Load(){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);return new(w);}
        foreach(int type in Enumerable.Range(0,26))foreach(int kind in Enumerable.Range(0,3))
            Check(OriginalWorkerPolicy.Collects(type,kind)==(kind==0&&type==6||kind==1&&type is 5 or 13||kind==2&&type==7),$"native resource permission unit {type} resource {kind}");
        Check(OriginalWorkerPolicy.Data.Deposits.Count==572&&OriginalWorkerPolicy.Data.Deposits.All(p=>p.Accepted==(p.Building==3&&p.Unit is 5 or 6 or 7 or 13||p.Building==4&&p.Unit==5)),"572 native building eligibility vectors loaded");
        var groupWorld=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(groupWorld);groupWorld.Objects[5642]=67;groupWorld.Quantities[5642]=3;for(int i=0;i<65536;i++)if(i!=5642&&Math.Abs((i&255)-10)+Math.Abs((i>>8)-22)<=6&&groupWorld.Objects[i] is 67 or >=29 and <=52){groupWorld.Objects[i]=0;groupWorld.Quantities[i]=0;}
        var groupSim=new Simulation(groupWorld);var farmers=groupWorld.Units.Where(u=>u.Type==5&&u.Owner==0).ToArray();var groupInput=new WorkerResourceOrders();
        var mixed=new[]{farmers[0],groupWorld.Units.First(u=>u.Type==7)};string unchanged=groupSim.Serialize();
        Check(groupInput.SourceGroup(groupSim,mixed,5642)==0&&groupSim.Serialize()==unchanged,"mixed farmer/builder group cannot issue incompatible food order");
        Check(groupInput.SourceGroup(groupSim,farmers,5642)==farmers.Length&&farmers.All(groupInput.WaitingFor),"all farmers in compatible selection await delivery after Ctrl source");
        int destination=Enumerable.Range(0,65536).Where(c=>groupWorld.Objects[c]==0&&groupSim.Passable(c&255,c>>8)&&groupSim.BuildingAt(c)==null).OrderBy(c=>Math.Abs((c&255)-10)+Math.Abs((c>>8)-22)).First(c=>groupInput.DestinationGroup(groupSim,farmers,c)==farmers.Length);
        int initialFood=groupWorld.Resources[0];for(int i=0;i<6000;i++)groupSim.Step();
        Check(groupWorld.Objects[5642]==0&&groupWorld.Objects[destination]==67&&groupWorld.Quantities[destination]==3&&groupWorld.Resources[0]==initialFood,"farmer group transfers final shared pile without duplicating resources");
        var timing=Load();var timedFarmer=timing.World.Units.First(a=>a.Type==5&&a.Owner==0);timedFarmer.RepeatFood=false;
        int plant=Enumerable.Range(0,65536).First(c=>timing.World.Objects[c] is >=29 and <=52&&timing.Gather(timedFarmer,c));
        for(int i=0;i<10000&&timedFarmer.FoodJob!=2;i++)timing.Step();
        Check(timedFarmer.FoodJob==2&&timedFarmer.WorkTicks==250,"farmer crop countdown uses original configured 250 updates");
        string duringWork=timing.Serialize();var resumed=Load();resumed.Restore(duringWork);Check(resumed.Serialize()==duringWork,"save accepts original crop countdown above old provisional limit");
        for(int i=0;i<249;i++)timing.Step();Check(timedFarmer.Food==0&&timedFarmer.WorkTicks==1,"crop is not credited before countdown completes");
        timing.Step();Check(timedFarmer.Food==1,"crop collected on native base countdown completion");
        var timedMine=Load();var timedBuilder=timedMine.World.Units.First(a=>a.Type==7&&a.Owner==0);timedMine.StartMining(timedBuilder,0);
        for(int i=0;i<10000&&timedBuilder.MineJob!=2;i++)timedMine.Step();
        Check(timedBuilder.MineJob==2&&timedBuilder.MineTicks==100,"first mining countdown uses original configured 100 updates");
        var sim=Load();var u=sim.World.Units.First(u=>u.Type==7&&u.Owner==0);u.RepeatMine=false;var orders=new WorkerResourceOrders();
        var home=sim.World.Buildings.First(b=>b.Type==2&&b.Owner==0);string before=sim.Serialize();
        Check(!sim.SetRockDestination(u,home.Slot)&&sim.Serialize()==before,"sanctuary rejected as manual rock depot without mutation");
        Check(orders.Source(sim,u,sim.World.Mines[0].Cell),"builder starts original mine through resource command");
        int ground=Enumerable.Range(0,65536).Where(c=>sim.RockGroundAllowed(c)).OrderBy(c=>Math.Abs((c&255)-u.X/1024)+Math.Abs((c>>8)-u.Y/1024)).First(c=>orders.Destination(sim,u,c));
        int stock=sim.World.Resources[4],reserve=sim.World.Mines[0].Quantity;
        for(int i=0;i<10000&&sim.World.Objects[ground]!=68;i++)sim.Step();
        Check(sim.World.Objects[ground]==68&&sim.World.Quantities[ground]==1&&sim.World.Resources[4]==stock&&sim.World.Mines[0].Quantity==reserve-1,"mine to chosen ground conserves rock without stock credit");
        var copy=Load();copy.Restore(sim.Serialize());Check(copy.Serialize()==sim.Serialize(),"save v14 retains mined rock ground destination and pile");
        var legacy=System.Text.Json.Nodes.JsonNode.Parse(copy.Serialize())!;legacy["Version"]=13;
        var legacyBuilder=legacy["Units"]!.AsArray().First(n=>(int)n!["Slot"]! ==u.Slot)!;
        legacyBuilder["RockDestination"]=home.Slot;legacyBuilder["RockGround"]=-1;
        copy.Restore(legacy.ToJsonString());
        Check(copy.World.Units.First(a=>a.Slot==u.Slot).RockDestination==-1&&copy.World.Resources.SequenceEqual(sim.World.Resources),"legacy sanctuary destination cleared without changing saved stock");
        var invalid=System.Text.Json.Nodes.JsonNode.Parse(sim.Serialize())!;
        invalid["Units"]!.AsArray().First(n=>(int)n!["Slot"]! ==u.Slot)!["RockGround"]=65536;
        string stable=sim.Serialize();bool rejected=false;try{sim.Restore(invalid.ToJsonString());}catch(InvalidDataException){rejected=true;}
        Check(rejected&&sim.Serialize()==stable,"invalid ground destination rejected atomically");
        int warehouse=ResearchTests.Build(sim,3);stock=sim.World.Resources[4];
        Check(orders.Source(sim,u,ground),"builder picks up mined rock pile");
        Check(orders.Destination(sim,u,sim.World.Buildings.First(b=>b.Slot==warehouse).Cell),"builder assigns warehouse after picking up ground rock");
        for(int i=0;i<10000&&sim.World.Resources[4]==stock;i++)sim.Step();
        Check(sim.World.Resources[4]==stock+1&&sim.World.Objects[ground]==0,"only final warehouse delivery credits mined rock stock");
    }
}
