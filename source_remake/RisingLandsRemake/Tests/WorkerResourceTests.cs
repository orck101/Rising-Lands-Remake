namespace RisingLands;
public static class WorkerResourceTests {
    static void Check(bool v,string n){if(!v)throw new Exception("FAIL: "+n);Console.WriteLine("PASS: "+n);}
    public static void Run(string root){
        World Load(){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);return w;}
        const int source=5642;
        foreach(int kind in new[]{0,1,2})foreach(int type in new[]{5,6,7,13}){
            Simulation Create(){var w=Load();w.Objects[source]=(byte)(66+kind);w.Quantities[source]=3;if(kind==1)for(int i=0;i<65536;i++)if(i!=source&&Math.Abs((i&255)-10)+Math.Abs((i>>8)-22)<=6&&w.Objects[i] is 67 or >=29 and <=52){w.Objects[i]=0;w.Quantities[i]=0;}if(type is 6 or 13)w.Units.First(u=>u.Type==7&&u.Owner==0).Type=type;return new(w);}
            var sim=Create();var worker=sim.World.Units.First(u=>u.Owner==0&&u.Type==type);var orders=new WorkerResourceOrders();
            int drop=Enumerable.Range(0,65536).Where(i=>sim.World.Objects[i]==0&&sim.Passable(i&255,i>>8)&&sim.BuildingAt(i)==null).OrderBy(i=>Math.Abs((i&255)-(source&255))+Math.Abs((i>>8)-(source>>8))).First();
            string before=sim.Serialize();Check(!orders.Source(sim,worker,drop)&&orders.WorkerSlot==-1&&sim.Serialize()==before,$"resource {kind} worker {type}: empty source neither starts work nor moves unit");
            if(!OriginalWorkerPolicy.Collects(type,kind)){
                Check(!WorkerResourceOrders.SourceAvailable(sim,worker,source)&&!orders.Source(sim,worker,source)&&!sim.Gather(worker,source)&&sim.Serialize()==before,$"worker {type} rejects incompatible resource {kind} in cursor and commands");
                continue;
            }
            int initialStock=sim.World.Resources[0];
            Check(orders.Source(sim,worker,source)&&orders.WorkerSlot==worker.Slot&&worker.CargoKind==kind&&worker.FoodDestinationKind==0,$"resource {kind} worker {type}: Ctrl source selects exact pile and awaits destination");
            var pendingCopy=Create();pendingCopy.Restore(sim.Serialize());var pendingWorker=pendingCopy.World.Units.First(u=>u.Slot==worker.Slot);var pendingInput=new WorkerResourceOrders();
            Check(pendingInput.WaitingFor(pendingWorker)&&pendingInput.Destination(pendingCopy,pendingWorker,drop),$"resource {kind} worker {type}: loading between source and destination keeps mouse-only workflow");
            before=sim.Serialize();Check(!orders.Destination(sim,worker,source)&&orders.WorkerSlot==worker.Slot&&sim.Serialize()==before,$"resource {kind} worker {type}: invalid destination preserves source and order");
            Check(orders.Destination(sim,worker,drop)&&orders.WorkerSlot==-1,$"resource {kind} worker {type}: second click assigns ground delivery without keyboard shortcut");
            for(int i=0;i<100;i++)sim.Step();string saved=sim.Serialize();var copy=Create();copy.Restore(saved);
            for(int i=0;i<5000;i++){sim.Step();copy.Step();}
            Check(sim.World.Resources[0]==initialStock,$"resource {kind} worker {type}: ground transfer does not credit food stock");
            Check(sim.Serialize()==copy.Serialize()&&sim.World.Objects[source]==0&&sim.World.Objects[drop]==66+kind&&sim.World.Quantities[drop]==3&&worker.Food==0,$"resource {kind} worker {type}: transfer and save/load conserve pile type and quantity");
        }
        var harvest=new Simulation(Load());var harvester=harvest.World.Units.First(u=>u.Owner==0&&u.Type==5);harvester.RepeatFood=false;
        var foodOrders=new WorkerResourceOrders();
        int crop=Enumerable.Range(0,65536).Where(i=>harvest.World.Objects[i] is >=29 and <=52).OrderBy(i=>Math.Abs((i&255)-harvester.X/1024)+Math.Abs((i>>8)-harvester.Y/1024)).First(i=>foodOrders.Source(harvest,harvester,i));
        int ground=Enumerable.Range(0,65536).Where(i=>harvest.World.Objects[i]==0&&harvest.Passable(i&255,i>>8)&&harvest.BuildingAt(i)==null).OrderBy(i=>Math.Abs((i&255)-(crop&255))+Math.Abs((i>>8)-(crop>>8))).First(i=>foodOrders.Destination(harvest,harvester,i));
        int stock=harvest.World.Resources[0];
        for(int i=0;i<10000&&harvest.World.Objects[ground]!=67;i++)harvest.Step();
        Check(harvest.World.Objects[ground]==67&&harvest.World.Quantities[ground]==1&&harvest.World.Resources[0]==stock,"farmer harvests planted food onto chosen ground without stock credit");
        Check(foodOrders.Source(harvest,harvester,ground),"farmer can pick up the pile at his previous delivery destination");
        harvester.RepeatFood=false; // This scenario moves one load; continuous collection is tested separately.
        int second=Enumerable.Range(0,65536).Where(i=>i!=ground&&harvest.World.Objects[i]==0&&harvest.Passable(i&255,i>>8)&&harvest.BuildingAt(i)==null).OrderBy(i=>Math.Abs((i&255)-(ground&255))+Math.Abs((i>>8)-(ground>>8))).First(i=>foodOrders.Destination(harvest,harvester,i));
        for(int i=0;i<10000&&harvest.World.Objects[second]!=67;i++)harvest.Step();
        Check(harvest.World.Objects[ground]==0&&harvest.World.Objects[second]==67&&harvest.World.Quantities[second]==1&&harvest.World.Resources[0]==stock,"farmer moves harvested food from ground to another chosen ground point");
        int warehouse=ResearchTests.Build(harvest,3);stock=harvest.World.Resources[0];
        Check(foodOrders.Source(harvest,harvester,second)&&foodOrders.Destination(harvest,harvester,harvest.World.Buildings.First(b=>b.Slot==warehouse).Cell),"farmer redirects the moved food pile into warehouse");
        for(int i=0;i<10000&&harvest.World.Resources[0]==stock;i++)harvest.Step();
        Check(harvest.World.Resources[0]==stock+1&&harvest.World.Objects[second]==0,"only warehouse delivery credits the transported food to spendable stock");
        var real=new Simulation(Load());int depot=ResearchTests.Build(real,3);var farmer=real.World.Units.First(u=>u.Type==5&&u.Owner==0);var builder=real.World.Units.First(u=>u.Type==7&&u.Owner==0);var input=new WorkerResourceOrders();
        Check(input.Source(real,farmer,source)&&input.Destination(real,farmer,real.World.Buildings.First(b=>b.Slot==depot).Cell),"farmer source then warehouse click works without C or E");
        Check(input.Source(real,builder,real.World.Mines[0].Cell)&&input.Destination(real,builder,real.World.Buildings.First(b=>b.Slot==depot).Cell),"builder source then warehouse click works without M or E");
        for(int i=0;i<10000&&(real.World.Resources[0]<34||real.World.Resources[4]<25);i++)real.Step();
        Check(real.World.Resources[0]==34&&real.World.Resources[4]>=25,"Ctrl workflow delivers actual first-mission food and mined rock to paid warehouse");
        var w=Load();w.Objects[source]=66;w.Quantities[source]=1;w.Units.First(u=>u.Type==5&&u.Owner==0).Type=6;var gears=new Simulation(w);int gearDepot=ResearchTests.Build(gears,3);var carrier=gears.World.Units.First(u=>u.Type==6&&u.Owner==0);var gearInput=new WorkerResourceOrders();int parts=gears.World.Resources[8];
        Check(gearInput.Source(gears,carrier,source)&&gearInput.Destination(gears,carrier,gears.World.Buildings.First(b=>b.Slot==gearDepot).Cell),"craftsman can designate gear pile and warehouse destination");
        for(int i=0;i<10000&&gears.World.Resources[8]==parts;i++)gears.Step();Check(gears.World.Resources[8]==parts+1&&gears.World.Resources[0]==20&&gears.World.Resources[4]==24,"gear delivery credits only parts stock");
        var restoredWorld=Load();restoredWorld.Objects[source]=66;restoredWorld.Quantities[source]=1;restoredWorld.Units.First(u=>u.Type==5&&u.Owner==0).Type=6;var restored=new Simulation(restoredWorld);restored.Restore(gears.Serialize());Check(restored.Serialize()==gears.Serialize(),"delivered gear survives save/load with parts conservation");
    }
}
