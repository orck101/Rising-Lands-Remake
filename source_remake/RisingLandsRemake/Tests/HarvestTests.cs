using System.Text.Json.Nodes;
namespace RisingLands;
public static class HarvestTests {
    static void Check(bool value,string name){if(!value)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
    static void Finish(Simulation s,Actor u){for(int i=0;i<20000&&(u.FoodJob is 1 or 2 or 3||u.Route.Count>0);i++)s.Step();}
    public static void Run(string root){
        World Load(){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);return w;}
        const int source=5642;var probe=new Simulation(Load());
        int Distance(int a,int b)=>Math.Abs((a&255)-(b&255))+Math.Abs((a>>8)-(b>>8));
        var free=Enumerable.Range(0,65536).Where(i=>probe.Passable(i&255,i>>8)&&probe.BuildingAt(i)==null).OrderBy(i=>Distance(i,source)).ThenBy(i=>i).ToArray();
        int second=free.First(i=>Distance(i,source)==1),drop=free.First(i=>i!=source&&i!=second&&Distance(i,source)<=2),far=free.First(i=>Distance(i,source)==Simulation.FoodFieldRadius+1);
        Simulation Field(){var w=Load();for(int i=0;i<65536;i++)if(w.Objects[i] is >=29 and <=52 or 67){w.Objects[i]=0;w.Quantities[i]=0;}w.Objects[source]=29;w.Objects[second]=51;w.Objects[far]=29;return new(w);}
        Actor Farmer(Simulation s)=>s.World.Units.First(u=>u.Type==5&&u.Owner==0);
        var sim=Field();var u=Farmer(sim);int stock=sim.World.Resources[0];
        Check(sim.SetFoodDestination(u,1,drop)&&sim.Gather(u,source),"crop-area order keeps explicit ground destination");
        for(int i=0;i<2000&&u.Food==0;i++)sim.Step();
        Check(u.Food==1&&u.FoodField==source,"crop anchor survives harvesting the first plant");
        string saved=sim.Serialize();var copy=Field();copy.Restore(saved);
        for(int i=0;i<3000;i++){sim.Step();copy.Step();}
        Check(sim.Serialize()==copy.Serialize(),"crop search and deliveries remain deterministic across save/load");
        Check(sim.World.Objects[source]==0&&sim.World.Objects[second]==0&&sim.World.Quantities[drop]==3&&u.Food==0&&u.FoodJob==0,"repeat harvest collects adjacent two-yield plant and delivers all three foods");
        Check(sim.World.Objects[far] is >=29 and <=40&&u.FoodField==source&&(u.FoodSource<0||Distance(u.FoodSource,source)<=Simulation.FoodFieldRadius),"fixed field boundary prevents collection drifting while distant food grows naturally");
        Check(sim.World.Resources[0]==stock&&u.FoodDestinationKind==1&&u.FoodDestination==drop,"field harvest neither consumes destination pile nor redirects stock");
        var off=Field();var single=Farmer(off);single.RepeatFood=false;off.SetFoodDestination(single,1,drop);off.Gather(single,source);Finish(off,single);
        Check(off.World.Quantities[drop]==1&&off.World.Objects[second]==51&&single.FoodJob==0,"repeat disabled leaves adjacent crops untouched");
        var cancelled=Field();var stopped=Farmer(cancelled);cancelled.SetFoodDestination(stopped,1,drop);cancelled.Gather(stopped,source);cancelled.Stop(stopped);for(int i=0;i<100;i++)cancelled.Step();
        Check(stopped.FoodJob==0&&cancelled.World.Objects[source]==29,"stop prevents automatic resumption of crop work");
        var reroute=Field();var traveller=Farmer(reroute);reroute.SetFoodDestination(traveller,1,drop);reroute.Gather(traveller,source);
        int obstacle=traveller.Route.First();byte originalCollision=reroute.World.Collision[obstacle];reroute.World.Collision[obstacle]|=1;reroute.Step();
        Check(traveller.FoodJob is 1 or 4&&traveller.Route.Count>0&&traveller.Food==0&&reroute.World.Objects[source]==29,"new route obstruction preserves destination without harvesting food remotely");
        reroute.World.Collision[obstacle]=originalCollision;for(int i=0;i<4000;i++)reroute.Step();
        Check(reroute.World.Quantities[drop]==3&&traveller.FoodJob==0,"cleared obstruction resumes harvest without another player order");
        var blocked=new Simulation(Load());var waiting=Farmer(blocked);waiting.RepeatFood=false;blocked.Gather(waiting,source);Finish(blocked,waiting);
        var home=blocked.World.Buildings.First(b=>b.Type==2&&b.Owner==0);Check(!blocked.SetFoodDestination(waiting,2,home.Slot),"sanctuary is not a valid manual food depot");Finish(blocked,waiting);
        Check(waiting.FoodJob==0&&waiting.Food==1,"rejected sanctuary destination preserves food awaiting player order");
        string pending=blocked.Serialize();var pendingCopy=new Simulation(Load());pendingCopy.Restore(pending);for(int i=0;i<120;i++){blocked.Step();pendingCopy.Step();}
        Check(blocked.Serialize()==pendingCopy.Serialize()&&waiting.Food==1,"unassigned cargo survives save/load without resource loss");
        int newDepot=ResearchTests.Build(blocked,3);Check(waiting.Food==1&&waiting.FoodDestinationKind==0,"new warehouse does not silently choose delivery destination");Check(blocked.SetFoodDestination(waiting,2,newDepot),"player explicitly redirects held food to new warehouse");for(int i=0;i<2000;i++)blocked.Step();
        Check(waiting.Food==0&&waiting.FoodJob==0&&blocked.World.Resources[0]==stock-10+1&&waiting.FoodDestination==newDepot,"explicit warehouse delivery credits held food exactly once");
        var contention=Field();var a=Farmer(contention);var b=contention.World.Units.First(x=>x.Type==5&&x.Owner==0&&x.Slot!=a.Slot);
        foreach(var worker in new[]{a,b}){contention.SetFoodDestination(worker,1,drop);contention.Gather(worker,source);}for(int i=0;i<4000;i++)contention.Step();
        Check(contention.World.Quantities[drop]==3&&a.Food+b.Food==0&&contention.World.Objects[second]==0,"competing farmers continue to adjacent crops without duplicating food");
        var corrupted=JsonNode.Parse(saved)!;corrupted["Units"]!.AsArray().First(n=>n!["Slot"]!.GetValue<int>()==u.Slot)!["FoodField"]=999999;
        string before=copy.Serialize();try{copy.Restore(corrupted.ToJsonString());throw new Exception("invalid crop area accepted");}catch(InvalidDataException){}
        Check(copy.Serialize()==before,"invalid field anchor rejected without changing current game");
        var legacy=JsonNode.Parse(saved)!;legacy["Version"]=9;foreach(var unit in legacy["Units"]!.AsArray())unit!.AsObject().Remove("FoodField");var migrated=Field();migrated.Restore(legacy.ToJsonString());var old=Farmer(migrated);Finish(migrated,old);
        Check(old.FoodField==-1&&migrated.World.Quantities[drop]==1&&migrated.World.Objects[second]==51,"v9 save migration preserves original single-source order");
    }
}
