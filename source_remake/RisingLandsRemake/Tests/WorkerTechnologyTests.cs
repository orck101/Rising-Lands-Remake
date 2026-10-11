namespace RisingLands;
public static class WorkerTechnologyTests {
    static void Check(bool ok,string label){if(!ok)throw new Exception("FAIL: "+label);Console.WriteLine("PASS: "+label);}
    public static void Run(string root){
        Check(Simulation.WorkerTechnologies.Rows.Count==2000,"2000 original worker technology states imported");
        var rules=Simulation.WorkerTechnologies.Rows;
        Check(rules.All(r=>r.Carry>=0&&r.Reload>=0&&r.Reload<256),"native worker bonus values fit cargo and countdown representation");
        Simulation Load(){var world=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(world);return new(world);}
        var sim=Load();var u=sim.World.Units.First(u=>u.Type==7&&u.Owner==0);
        Check(sim.Capacity(u)==1&&sim.WorkerCountdown(u)==100,"first mission preserves base builder capacity and countdown");
        // Controlled research-level fixture, not a claim that mission one permits these discoveries.
        sim.Research.Index=12;Check(sim.Capacity(u)==2&&sim.WorkerCountdown(u)==100,"native civil level 12 increases builder cargo without changing mining time");
        sim.Research.Index=13;Check(sim.Capacity(u)==2&&sim.WorkerCountdown(u)==66,"native civil level 13 applies 34-count reload reduction");
        u.RepeatMine=false;Check(sim.StartMining(u,0),"upgraded builder begins mining");
        for(int i=0;i<10000&&u.MineJob!=2;i++)sim.Step();
        Check(u.MineTicks==66,"mining state machine uses researched initial countdown");
        for(int i=0;i<65;i++)sim.Step();Check(u.Rock==0,"upgraded mining does not credit rock before countdown");
        sim.Step();Check(u.Rock==1&&u.MineJob==2&&u.MineTicks==50,"first rock keeps upgraded carrier working with native continuation timer");
        for(int i=0;i<50;i++)sim.Step();Check(u.Rock==2&&u.MineJob==0,"upgraded builder fills two-rock cargo and awaits destination");
        Check(sim.World.Mines[0].Quantity==48&&sim.World.Resources[4]==30,"upgraded extraction preserves stock until delivery");
        sim.Restart();u=sim.World.Units.First(u=>u.Type==7&&u.Owner==0);Check(sim.Capacity(u)==1&&sim.WorkerCountdown(u)==100,"restart removes controlled technology upgrades");
        var baseFarmer=Simulation.TechnologyFor(2,4,5);var maxFarmer=Simulation.TechnologyFor(19,24,5);
        Check(baseFarmer.Carry==0&&baseFarmer.Reload==0&&maxFarmer.Carry>0&&maxFarmer.Reload>0,"native farmer agriculture bonuses differ from first mission baseline");
    }
}
