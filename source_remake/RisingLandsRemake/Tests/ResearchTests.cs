using System.Text.Json.Nodes;
namespace RisingLands;
public static class ResearchTests {
    static void Check(bool value,string name){if(!value)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
    public static int Build(Simulation sim,int type,bool staff=true){if(sim.BuildChoiceError(type) is {} error)throw new Exception("Construction fixture: "+error);var worker=sim.World.Units.First(u=>u.Type==7&&u.Owner==0&&VisualCatalog.Visible(u));
        foreach(int cell in Enumerable.Range(0,65536).Where(c=>(c&255)<sim.World.Width&&(c>>8)<sim.World.Height).OrderBy(c=>Math.Abs((c&255)-worker.X/1024)+Math.Abs((c>>8)-worker.Y/1024)))if(sim.StartConstruction(worker,type,cell)){
            var site=sim.Constructions.Last();for(int i=0;i<10000&&!sim.Complete(site);i++)sim.Step();if(!sim.Complete(site))throw new Exception("Construction timed out");if(staff&&type is 3 or 1){var settler=sim.ReleaseResident(sim.World.Buildings.First(b=>b.Type==2&&b.Owner==0).Slot)??throw new Exception("Fixture needs warehouse/barracks resident");if(!sim.EnterBuilding(settler,site.Id))throw new Exception("Fixture resident entry");for(int j=0;j<10000&&sim.ResidentTrips.Count>0;j++)sim.Step();}return site.Id;
        }
        throw new Exception("No reachable construction site for research fixture "+type);
    }
    public static int PrepareLaboratory(Simulation sim){
        int depot=Build(sim,3);var farmer=sim.World.Units.First(u=>u.Type==5&&u.Owner==0);
        if(!sim.SetFoodDestination(farmer,2,depot))throw new Exception("Research fixture warehouse unreachable");
        for(int i=0;i<50000&&sim.World.Resources[0]<30;i++){
            if(farmer.FoodJob==0&&!sim.GatherNearest(farmer))throw new Exception("Research fixture cannot gather enough food");sim.Step();
        }
        if(sim.World.Resources[0]<30)throw new Exception("Research fixture collection timed out");sim.Stop(farmer);
        return Build(sim,7);
    }
    public static void Run(string root){
        Simulation Load(string name="MONDE03.ETA"){var world=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD",name));new VisualCatalog().Apply(world);return new(world);}
        var first=Load("MONDE01.ETA");Check(!first.BuildingUnlocked(7)&&!first.ToggleResearch(),"mission one cannot invent technology without a laboratory");
        var sim=Load();Check(sim.BuildingUnlocked(7)&&!sim.BuildingUnlocked(8)&&sim.CurrentResearch?.Key=="GENIECIVIL5","mission three begins with laboratory unlocked and exchange awaiting research");
        int lab=PrepareLaboratory(sim);sim.ToggleResearch();int food=sim.World.Resources[0],rock=sim.World.Resources[4];Check(sim.Scientists()==0,"warehouse and actual food collection fund an empty laboratory without injected resources");
        Check(sim.AssignLaboratoryWorker(lab)&&sim.AssignLaboratoryWorker(lab)&&!sim.AssignLaboratoryWorker(lab),"laboratory sends actual settlers and respects original capacity of two");
        var slots=sim.LaboratoryWorkers.Select(j=>j.SettlerSlot).ToArray();Check(slots.Distinct().Count()==2&&sim.World.Units.Where(u=>slots.Contains(u.Slot)).All(VisualCatalog.Visible),"two distinct settlers visibly travel to their laboratory");
        var reserved=Load();reserved.Restore(sim.Serialize());Check(reserved.Recruit(5)&&!slots.Contains(reserved.Recruitments.Last().SettlerSlot),"training cannot reuse a settler already traveling to laboratory");
        var copy=Load();copy.Restore(sim.Serialize());for(int i=0;i<2000;i++){sim.Step();copy.Step();}
        Check(sim.Serialize()==copy.Serialize()&&sim.Scientists()==2,"laboratory journeys and arrival survive deterministic save/load");
        Check(sim.LaboratoryWorkers.All(j=>j.Stage==1)&&sim.World.Units.Where(u=>slots.Contains(u.Slot)).All(u=>!VisualCatalog.Visible(u)),"arrived scientists reside inside laboratory");
        Check(sim.Occupants(lab)==2,"occupancy indicator includes both researching colonists");
        
        var resident=sim.World.Units.First(u=>u.Slot==slots[0]);Check(!sim.Order(resident,resident.X/1024,resident.Y/1024)&&sim.Scientists()==2,"hidden laboratory resident cannot receive a movement order while researching");
        Check(sim.ToggleResearch(),"player starts engineering research");sim.Step();Check(Math.Abs(sim.Research.Progress-sim.CurrentResearch!.Cost/7200.0)<1e-9,"two scientists advance research at four-minute research rate");
        sim.ToggleResearch();double paused=sim.Research.Progress;for(int i=0;i<30;i++)sim.Step();Check(sim.Research.Progress==paused,"player can pause research without losing progress");sim.ToggleResearch();
        string working=sim.Serialize();copy.Restore(working);for(int i=0;i<14400;i++){sim.Step();copy.Step();}
        Check(sim.Serialize()==copy.Serialize(),"research and building unlocks restore deterministically");
        Check(sim.Research.Completed.SequenceEqual(new[]{"GENIECIVIL5","GENIECIVIL7"})&&sim.BuildingUnlocked(8)&&sim.BuildingUnlocked(9)&&!sim.BuildingUnlocked(16),"engineering unlocks exchange and wall while correctly skipping future mission market");
        Check(sim.Research.Completed.Count==2&&sim.Research.Progress==0&&sim.World.Resources[0]==food&&sim.World.Resources[4]==rock,"native two-discovery limit stops research without charging food or rock");
        int wall=Build(sim,9);Check(sim.World.Buildings.Any(b=>b.Slot==wall&&b.Type==9),"newly researched wall becomes constructible in the same game");
        Check(sim.ReturnLaboratoryWorker(lab)&&sim.Scientists()==1,"returning scientist immediately stops contributing");string returning=sim.Serialize();copy.Restore(returning);for(int i=0;i<2000;i++){sim.Step();copy.Step();}
        Check(sim.Serialize()==copy.Serialize()&&sim.LaboratoryWorkers.Count==1&&sim.World.Units.First(u=>u.Slot==slots[0]).LegacyState==14,"scientist returns to sanctuary and save/load preserves return journey");
        Check(sim.Recruit(5)&&sim.Recruitments.Last().SettlerSlot==slots[0],"returned settler can be trained without reusing active scientist");
        var invalid=JsonNode.Parse(sim.Serialize())!;invalid["Research"]!["Completed"]!.AsArray().Add("GENIECIVIL13");string before=sim.Serialize();try{sim.Restore(invalid.ToJsonString());throw new Exception("Research cheat accepted");}catch(InvalidDataException){}Check(sim.Serialize()==before,"invalid research completion list rejected atomically");
        invalid=JsonNode.Parse(sim.Serialize())!;invalid["LaboratoryWorkers"]![0]!["LaboratorySlot"]=999999;try{sim.Restore(invalid.ToJsonString());throw new Exception("Invalid lab accepted");}catch(InvalidDataException){}Check(sim.Serialize()==before,"invalid scientist destination rejected atomically");
        var between=Load();var pausedAtGate=JsonNode.Parse(between.Serialize())!;pausedAtGate["Research"]!["Index"]=6;pausedAtGate["Research"]!["Completed"]!.AsArray().Add("GENIECIVIL5");between.Restore(pausedAtGate.ToJsonString());Check(!between.BuildingUnlocked(16),"skipped future mission invention does not implicitly unlock market");
        var legacy=Load();var old=JsonNode.Parse(legacy.Serialize())!;old["Version"]=8;old.AsObject().Remove("Research");old.AsObject().Remove("LaboratoryWorkers");legacy.Restore(old.ToJsonString());Check(legacy.Research.Index==5&&legacy.Research.Completed.Count==0&&legacy.LaboratoryWorkers.Count==0,"v8 saves migrate without inventing discoveries or colonists");
        sim.Restart();Check(sim.Research.Completed.Count==0&&sim.LaboratoryWorkers.Count==0&&!sim.BuildingUnlocked(8),"restart resets discoveries and laboratory assignments");
        var stopped=Load();int stoppedLab=PrepareLaboratory(stopped);stopped.AssignLaboratoryWorker(stoppedLab);var traveler=stopped.World.Units.First(u=>u.Slot==stopped.LaboratoryWorkers[0].SettlerSlot);stopped.Stop(traveler);for(int i=0;i<2000;i++)stopped.Step();Check(stopped.LaboratoryWorkers[0].Stage==3&&stopped.Scientists()==0&&VisualCatalog.Visible(traveler),"stop interrupts laboratory trip without creating a hidden scientist");
    }
}
