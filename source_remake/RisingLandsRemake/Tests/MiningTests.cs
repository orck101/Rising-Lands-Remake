using System.Text.Json.Nodes;
namespace RisingLands;
public static class MiningTests {
    static void Check(bool v,string name){if(!v)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
    public static void Run(string root){
        Simulation Load(){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);return new(w);}
        Actor Builder(Simulation s)=>s.World.Units.First(u=>u.Type==7&&u.Owner==0);
        void Until(Simulation s,Func<bool> done,int limit=10000){for(int i=0;i<limit&&!done();i++)s.Step();Check(done(),"mining integration reaches expected state within bounded updates");}
        var sim=Load();var u=Builder(sim);
        Check(sim.World.Mines.Count==2&&sim.World.Mines[0].Cell==2091&&sim.World.Mines[0].Quantity==50&&sim.World.Mines[1].Quantity==200,"first mission imports original mine cells and reserves");
        Check(sim.MineAt(2091)==0&&sim.MineAt(2092)==0&&sim.MineAt(2347)==0&&sim.MineAt(2348)==0&&sim.MineAt(2093)==-1,"mine hit area matches original four-cell lookup");
        string before=sim.Serialize();Check(!sim.StartMining(sim.World.Units.First(a=>a.Type==5),0)&&!sim.StartMining(u,99)&&sim.Serialize()==before,"invalid miner and unknown mine leave world unchanged");
        Check(sim.StartMining(u,0),"builder routes to original mine perimeter");Until(sim,()=>u.Rock==1);
        Check(u.MineJob==0&&sim.World.Mines[0].Quantity==49&&sim.World.Resources[4]==30,"extraction depletes mine and holds rock until destination selected");
        before=sim.Serialize();Check(!sim.StartMining(u,1)&&sim.Serialize()==before,"full builder cannot discard cargo by changing mine");
        var home=sim.World.Buildings.First(b=>b.Type==2&&b.Owner==0);u.RepeatMine=false;Check(!sim.SetRockDestination(u,home.Slot),"native input rejects sanctuary as rock depot");
        Check(u.Rock==1&&sim.World.Resources[4]==30,"no warehouse holds pending rock without inventing stock");
        // Building with the carrier preserves its cargo and cancels its previous work.
        int depot=ResearchTests.Build(sim,3);Check(u.MineJob==0&&u.Rock==1,"construction cancels mining order but preserves carried rock");
        Check(sim.SetRockDestination(u,depot),"warehouse accepts explicit rock destination");Until(sim,()=>u.Rock==0);
        Check(sim.World.Resources[4]==25&&u.MineJob==0,"warehouse credits rock once after original six-rock building cost");
        u.RepeatMine=true;Check(sim.StartMining(u,0),"repeat mining resumes same original deposit");for(int i=0;i<200;i++)sim.Step();
        string saved=sim.Serialize();var restored=Load();restored.Restore(saved);for(int i=0;i<3000;i++){sim.Step();restored.Step();}
        Check(sim.Serialize()==restored.Serialize(),"rock reserve cargo route and delivery survive deterministic save/load");
        Until(sim,()=>sim.World.Mines[0].Quantity==0&&u.Rock==0&&u.MineJob==0,120000);
        Check(sim.World.Resources[4]==74&&sim.World.Mines[1].Quantity==200,"whole fifty-rock mine transferred once without harvesting another mine");
        Check(sim.World.Tiles[2091]==0x1db&&sim.World.Tiles[2092]==0x1dc,"depleted mine uses original exhausted terrain tiles");
        restored.Restore(sim.Serialize());Check(restored.World.Tiles[2091]==0x1db&&restored.Serialize()==sim.Serialize(),"loading depleted mine restores exhausted terrain");
        restored.Restart();Check(restored.World.Mines[0].Quantity==50&&restored.World.Tiles[2091]!=0x1db&&Builder(restored).Rock==0,"restart restores original mine terrain and reserve");
        var bad=JsonNode.Parse(saved)!;bad["Mines"]![0]!["Quantity"]=50;before=restored.Serialize();try{restored.Restore(bad.ToJsonString());throw new Exception("Duplicated rock accepted");}catch(InvalidDataException){}
        Check(restored.Serialize()==before,"save cannot replenish extracted rock without conservation");
        bad=JsonNode.Parse(saved)!;bad["Mines"]![0]!["Cell"]=1;try{restored.Restore(bad.ToJsonString());throw new Exception("Relocated mine accepted");}catch(InvalidDataException){}
        Check(restored.Serialize()==before,"save cannot relocate original mine");
        var stop=Load();var worker=Builder(stop);stop.StartMining(worker,0);stop.Stop(worker);for(int i=0;i<300;i++)stop.Step();Check(worker.MineJob==0&&stop.World.Mines[0].Quantity==50,"stop prevents mining from restarting on its own");
        var old=JsonNode.Parse(Load().Serialize())!;old["Version"]=10;old.AsObject().Remove("Mines");foreach(var a in old["Units"]!.AsArray())foreach(string key in new[]{"Rock","MineIndex","MineJob","MineTicks","MineWorkCell","RockDestination","RepeatMine"})a!.AsObject().Remove(key);restored.Restore(old.ToJsonString());
        Check(restored.World.Mines[0].Quantity==50&&restored.World.Units.All(a=>a.Rock==0&&a.MineJob==0),"v10 migration restores untouched native mines without inventing mining orders");
        var fullWorld=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(fullWorld);fullWorld.Mines[0].Quantity=200;for(int i=0;i<65536;i++)if(i!=5642&&Math.Abs((i&255)-10)+Math.Abs((i>>8)-22)<=6&&fullWorld.Objects[i] is 67 or >=29 and <=52){fullWorld.Objects[i]=0;fullWorld.Quantities[i]=0;}var full=new Simulation(fullWorld);int store=ResearchTests.Build(full,3);var miner=Builder(full);var farmer=full.World.Units.First(a=>a.Type==5&&a.Owner==0);
        full.SetFoodDestination(farmer,2,store);full.Gather(farmer,5642);Until(full,()=>full.World.Quantities[5642]==0&&farmer.Food==0&&farmer.FoodJob==0,30000);
        full.SetRockDestination(miner,store);full.StartMining(miner,0);Until(full,()=>full.WarehouseOverflowCells(full.World.Buildings.First(b=>b.Slot==store)).Any(c=>full.World.Objects[c]==68&&full.World.Quantities[c]>0),400000);
        Check(full.World.Resources[4]==66&&full.World.Resources[0]==34&&full.World.Mines[0].Quantity==157,"food plus rock reach exactly 100 and excess rock reaches nearby ground");
        var freshWorld=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(freshWorld);freshWorld.Mines[0].Quantity=200;for(int i=0;i<65536;i++)if(i!=5642&&Math.Abs((i&255)-10)+Math.Abs((i>>8)-22)<=6&&freshWorld.Objects[i] is 67 or >=29 and <=52){freshWorld.Objects[i]=0;freshWorld.Quantities[i]=0;}var fullCopy=new Simulation(freshWorld);fullCopy.Restore(full.Serialize());
        for(int i=0;i<120;i++){full.Step();fullCopy.Step();}Check(full.Serialize()==fullCopy.Serialize(),"full warehouse overflow route resumes deterministically");
        var oneWorld=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(oneWorld);oneWorld.Mines[0].Quantity=1;var contested=new Simulation(oneWorld);Check(contested.Recruit(7),"second miner recruited through actual sanctuary production");Until(contested,()=>contested.World.Units.Count(a=>a.Type==7&&a.Owner==0&&VisualCatalog.Visible(a))>=2);
        var miners=contested.World.Units.Where(a=>a.Type==7&&a.Owner==0&&VisualCatalog.Visible(a)).Take(2).ToArray();foreach(var m in miners)Check(contested.StartMining(m,0),"competing builder receives mine order");Until(contested,()=>contested.World.Mines[0].Quantity==0&&miners.All(m=>m.MineJob==0));
        Check(miners.Sum(m=>m.Rock)==1,"two builders cannot duplicate last unit in a mine");
    }
}
