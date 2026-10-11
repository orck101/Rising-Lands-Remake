using System.Text.Json.Nodes;
namespace RisingLands;
public static class MissionOneTests {
    static void Check(bool v,string name){if(!v)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
    public static Simulation Load(string root){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);return new(w);}
    public static void PlayToVictory(Simulation sim){
        int depot=ResearchTests.Build(sim,3);
        if(!sim.FirstMissionWarehouse)throw new Exception("Playthrough warehouse outside original zone");
        var farmer=sim.World.Units.First(u=>u.Type==5&&u.Owner==0);var builder=sim.World.Units.First(u=>u.Type==7&&u.Owner==0);
        var input=new WorkerResourceOrders();int delivery=sim.World.Buildings.First(b=>b.Slot==depot).Cell;
        if(!input.Source(sim,builder,sim.World.Mines[0].Cell)||!input.Destination(sim,builder,delivery))throw new Exception("Playthrough cannot designate mine and destination");
        bool FoodOrder(){foreach(int cell in Enumerable.Range(0,65536).Where(sim.IsFood).OrderBy(c=>Math.Abs((c&255)-farmer.X/1024)+Math.Abs((c>>8)-farmer.Y/1024)))if(input.Source(sim,farmer,cell))return input.Destination(sim,farmer,delivery);return false;}
        for(int i=0;i<400000&&!sim.MissionWon;i++){
            if(sim.World.Resources[0]<50&&farmer.FoodJob is 0 or 4&&farmer.Food==0&&sim.Tick%30==0&&!FoodOrder())throw new Exception("Playthrough cannot find food");
            if(sim.World.Resources[0]>=50)sim.Stop(farmer);
            if(sim.World.Resources[4]>=50)sim.Stop(builder);
            sim.Step();
        }
        if(!sim.MissionWon)throw new Exception($"Playthrough stalled: food={sim.World.Resources[0]}, rock={sim.World.Resources[4]}, checks={sim.MissionChecks}");
    }
    public static void Run(string root){
        SanctuaryTests.Run(root);CampaignTransitionTests.Run(root);
        InteractionV47Tests.Run(root);
        ImprovementV46Tests.Run(root);
        ResidentIndicatorTests.Run(root);
        InteractionV42Tests.Run(root);
        SaveSlotTests.Run(root);
        var native=JsonNode.Parse(File.ReadAllText(Path.GetFullPath(PackageLayout.Asset(root,"../../reverse_engineering/mission_one_vectors.json"))))!["conditions"]!.AsArray();
        foreach(var vector in native){
            var n=vector!;var units=n["alive"]!.GetValue<int>()==0?Array.Empty<Actor>():new[]{new Actor{Owner=0,Type=5,LegacyState=7,Health=100}};
            var buildings=n["building"] is {} bld?new[]{new Building(0,bld["type"]!.GetValue<int>(),bld["owner"]!.GetValue<int>(),bld["zone"]!.GetValue<int>()==0?0:29)}:Array.Empty<Building>();
            bool expected=n["result"]!["ended"]!.GetValue<int>()!=0&&n["result"]!["victory"]!.GetValue<int>()!=0;
            if(Simulation.FirstMissionSatisfied(units,buildings,Array.Empty<Construction>(),n["food"]!.GetValue<int>(),n["stone"]!.GetValue<int>())!=expected)throw new Exception("Native mission vector mismatch: "+n);
        }
        Check(native.Count==75,"all seventy-five native mission-condition vectors match modern victory predicate");
        var actor=new Actor{Owner=0,Type=9,LegacyState=14,Health=100};var building=new Building(0,3,0,0);
        bool S(int food,int rock,Building b)=>Simulation.FirstMissionSatisfied(new[]{actor},new[]{b},Array.Empty<Construction>(),food,rock);
        Check(!S(49,50,building)&&!S(50,49,building)&&S(50,50,building),"mission stock thresholds are inclusive at fifty");
        Check(S(50,50,building with{Cell=25*256+28})&&!S(50,50,building with{Cell=25*256+29})&&!S(50,50,building with{Cell=26*256+28}),"warehouse zone includes native boundary and excludes adjacent cells");
        Check(!S(50,50,building with{Owner=1})&&!S(50,50,building with{Type=1}),"enemy warehouse and wrong building do not satisfy mission");
        Check(S(50,50,building),"hidden sanctuary colonist prevents false absence of survivors");
        actor.Health=0;Check(!S(50,50,building),"dead modern actor cannot trigger victory");actor.Health=100;actor.Type=17;Check(!S(50,50,building),"original excluded unit type cannot alone trigger victory");actor.Type=9;
        Check(Simulation.FirstMissionSatisfied(new[]{actor},Array.Empty<Building>(),new[]{new Construction{Type=3,Cell=0,Progress=0}},50,50),"warehouse under construction counts as original evaluator requires");
        var sim=Load(root);Check(!sim.MissionWon&&sim.MissionChecks==0,"first mission starts without a victory");
        PlayToVictory(sim);
        Check(sim.MissionWon&&sim.World.Resources[0]>=50&&sim.World.Resources[4]>=50&&sim.Constructions.Any(sim.Complete),"complete original first mission through paid construction food and mining without resource injection");
        string saved=sim.Serialize();long tick=sim.Tick;for(int i=0;i<200;i++){sim.Step();}
        Check(sim.Serialize()==saved&&sim.Tick==tick&&sim.World.MapName=="MONDE01.ETA","victory freezes world until campaign confirmation");
        var restored=Load(root);restored.Restore(saved);Check(restored.MissionWon&&restored.Serialize()==saved,"saving and loading completed mission preserves completion screen");
        restored.Restart();Check(!restored.MissionWon&&restored.MissionChecks==0&&restored.World.Resources[0]==30&&restored.World.Resources[4]==30,"restart restores first mission initial resources and clears victory");
        var pending=JsonNode.Parse(saved)!;pending["MissionChecks"]=50;var a=Load(root);var b=Load(root);a.Restore(pending.ToJsonString());b.Restore(pending.ToJsonString());
        for(int i=0;i<49;i++){a.Step();b.Step();}Check(!a.MissionWon&&a.MissionChecks==99&&a.Serialize()==b.Serialize(),"pending native-style gate resumes deterministically through ninety-nine checks");a.Step();Check(a.MissionWon,"hundredth successful evaluation completes mission");
        var interrupted=Load(root);interrupted.Restore(pending.ToJsonString());interrupted.World.Resources[0]=49;interrupted.Step();Check(interrupted.MissionChecks==50,"false evaluation does not reset accumulated end gate");
        var invalid=JsonNode.Parse(saved)!;invalid["MissionChecks"]=101;string before=restored.Serialize();try{restored.Restore(invalid.ToJsonString());throw new Exception("Invalid victory accepted");}catch(InvalidDataException){}Check(restored.Serialize()==before,"invalid completion counter rejected atomically");
        var initial=JsonNode.Parse(restored.Serialize())!;initial["MissionChecks"]=100;initial["Tick"]=100;try{restored.Restore(initial.ToJsonString());throw new Exception("False victory accepted");}catch(InvalidDataException){}Check(restored.Serialize()==before,"save cannot claim victory without required building and stock");
        var old=JsonNode.Parse(saved)!;old["Version"]=11;old.AsObject().Remove("MissionChecks");restored.Restore(old.ToJsonString());Check(!restored.MissionWon&&restored.MissionChecks==0,"v11 save migration evaluates objectives anew without inventing completion");
        PopulationTests.Run(root);FoodTests.Run(root);MiningTests.Run(root);WorkerResourceTests.Run(root);OriginalGameplayTests.Run(root);WorkerTechnologyTests.Run(root);CollectionFlowTests.Run(root);WorkerVisualTests.Run(root);ConstructionFidelityTests.Run(root);ConstructionRhythmTests.Run(root);InteractionTests.Run(root);CooperativeConstructionTests.Run(root);WarehouseNetworkTests.Run(root);CollectionV50Tests.Run(root);
        AttackRangeTests.Run(root);WarehouseAccountingTests.Run(root);SelectionClassTests.Run();ViewportZoomTests.Run(root);Console.WriteLine("ALL MISSION ONE TESTS PASSED");
    }
}
