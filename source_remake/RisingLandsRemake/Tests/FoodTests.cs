namespace RisingLands;
public static class FoodTests {
    static void Check(bool value,string name){if(!value)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
    static void RunUntilIdle(Simulation s,Actor u){for(int i=0;i<20000&&(u.FoodJob is 1 or 2 or 3||u.Route.Count>0);i++)s.Step();}
    public static void Run(string root){
        string map=PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA");var visuals=new VisualCatalog();World Load(){var w=LegacyAssets.Map(map);visuals.Apply(w);for(int i=0;i<65536;i++)if(i!=5642&&Math.Abs((i&255)-10)+Math.Abs((i>>8)-22)<=6&&w.Objects[i] is 67 or >=29 and <=52){w.Objects[i]=0;w.Quantities[i]=0;}return w;}
        var sim=new Simulation(Load());var farmer=sim.World.Units.First(u=>u.Type==5);const int source=5642;
        int drop=Enumerable.Range(0,65536).Where(i=>sim.World.Objects[i]==0&&sim.Passable(i&255,i>>8)&&sim.BuildingAt(i)==null&&i!=source).OrderBy(i=>Math.Abs((i&255)-(source&255))+Math.Abs((i>>8)-(source>>8))).First();
        Check(sim.World.Objects[source]==67&&sim.World.Quantities[source]==14,"original first-mission food pile contains 14");
        Check(sim.Capacity(farmer)==1,"original farmer carries one food");Check(!sim.Gather(sim.World.Units.First(u=>u.Type==0),source),"soldier cannot run farmer food job");
        int stock=sim.World.Resources[0];Check(sim.Gather(farmer,source),"farmer can reach original food pile");RunUntilIdle(sim,farmer);
        Check(farmer.Food==1&&farmer.FoodJob==0&&sim.World.Quantities[source]==13&&sim.World.Resources[0]==stock,"harvesting waits with cargo until player chooses destination");
        string full=sim.Serialize();Check(!sim.Gather(farmer,source)&&sim.Serialize()==full,"full carrier cannot silently choose nearest depot");
        Check(!sim.SetFoodDestination(farmer,1,source)&&sim.Serialize()==full,"reject source as destination without changing orders");
        Check(sim.SetFoodDestination(farmer,1,drop),"player selects a ground destination");
        string saved=sim.Serialize();var restored=new Simulation(Load());restored.Restore(saved);for(int i=0;i<400;i++){sim.Step();restored.Step();}
        Check(sim.Serialize()==restored.Serialize(),"explicit destination and delivery resume deterministically after save/load");RunUntilIdle(sim,farmer);
        Check(sim.World.Objects[source]==0&&sim.World.Quantities[drop]==14&&farmer.Food==0&&sim.World.Resources[0]==stock,"repeated ground transfer conserves all fourteen items without crediting stock");
        var stockWorld=Load();stockWorld.Buildings.Add(new Building(999,3,0,3345));PopulationFixture.Inhabit(stockWorld,999);var stockSim=new Simulation(stockWorld);var worker=stockWorld.Units.First(u=>u.Type==5);
        Check(stockSim.SetFoodDestination(worker,2,999)&&worker.Route.Count==0,"empty farmer remembers player-selected warehouse without moving");
        Check(stockSim.Gather(worker,source),"warehouse collection begins after explicit destination");RunUntilIdle(stockSim,worker);
        Check(stockWorld.Resources[0]==stock+14&&worker.Food==0,"warehouse receives all fourteen items exactly once");
        var concurrent=Load();concurrent.Quantities[source]=1;var both=new Simulation(concurrent);var farmers=concurrent.Units.Where(u=>u.Type==5).ToArray();foreach(var u in farmers)both.Gather(u,source);for(int i=0;i<2000;i++)both.Step();
        Check(concurrent.Quantities[source]==0&&farmers.Sum(u=>u.Food)==1,"two farmers cannot duplicate final food item");
        var blocked=new Simulation(Load());var carrier=blocked.World.Units.First(u=>u.Type==5);blocked.Gather(carrier,source);RunUntilIdle(blocked,carrier);var sanctuary=blocked.World.Buildings.First(b=>b.Type==2&&b.Owner==0);
        Check(!blocked.SetFoodDestination(carrier,2,sanctuary.Slot),"native input rejects sanctuary as manual food depot");RunUntilIdle(blocked,carrier);
        Check(carrier.Food==1&&carrier.FoodJob==0&&blocked.World.Resources[0]==stock,"invalid depot preserves carried food without stock credit");
        carrier.RepeatFood=false;Check(blocked.SetFoodDestination(carrier,1,drop),"blocked cargo can be redirected to ground");RunUntilIdle(blocked,carrier);
        Check(carrier.Food==0&&carrier.FoodJob==0&&blocked.World.Quantities[drop]==1&&blocked.World.Quantities[source]==13,"repeat off delivers one load and stops");
        var occupied=Load();occupied.Objects[drop]=67;occupied.Quantities[drop]=30;var race=new Simulation(occupied);var rworker=occupied.Units.First(u=>u.Type==5);race.Gather(rworker,source);RunUntilIdle(race,rworker);Check(race.SetFoodDestination(rworker,1,drop),"partially full ground pile accepts order");occupied.Quantities[drop]=31;rworker.RepeatFood=false;RunUntilIdle(race,rworker);
        Check(rworker.Food==0&&rworker.FoodJob==0&&occupied.Quantities[drop]==31&&Enumerable.Range(0,65536).Any(c=>c!=drop&&Simulation.CellDistance(c,drop)<=3&&occupied.Objects[c]==67&&occupied.Quantities[c]==1),"pile filling in transit redirects cargo nearby without exceeding pile capacity");
        string before=restored.Serialize();try{restored.Restore(saved.Replace("\"Food\":1","\"Food\":2"));throw new Exception("invalid cargo accepted");}catch(InvalidDataException){}Check(restored.Serialize()==before,"invalid cargo save rejected atomically");
        try{restored.Restore(saved.Replace("\"FoodDestination\":"+drop,"\"FoodDestination\":999999"));throw new Exception("invalid destination accepted");}catch(InvalidDataException){}Check(restored.Serialize()==before,"invalid destination save rejected atomically");
        var stopped=new Simulation(Load());var stopWorker=stopped.World.Units.First(u=>u.Type==5);stopped.Gather(stopWorker,source);RunUntilIdle(stopped,stopWorker);stopped.SetFoodDestination(stopWorker,1,drop);stopped.Stop(stopWorker);for(int i=0;i<600;i++)stopped.Step();Check(stopWorker.Food==1&&stopWorker.FoodJob==0,"stop cancels delivery without discarding cargo");
    }
}
