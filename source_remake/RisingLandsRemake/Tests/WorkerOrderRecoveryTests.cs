using System.Text.Json;
namespace RisingLands;
public static class WorkerOrderRecoveryTests {
    public static void Run(string root,string snapshots,string output){
        int checks=0;void Check(bool ok,string name){if(!ok)throw new Exception(name);checks++;Console.WriteLine("PASS "+name);}
        Simulation Load(string path){string json=File.ReadAllText(path);using var doc=JsonDocument.Parse(json);var campaign=JsonSerializer.Deserialize<CampaignProgress>(doc.RootElement.GetProperty("Campaign").GetRawText())!;var world=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD",doc.RootElement.GetProperty("MapName").GetString()!));new VisualCatalog().Apply(world);var s=new Simulation(world,campaign);s.Restore(json);return s;}
        foreach(var path in Directory.GetFiles(snapshots,"*.snapshot.json")){
            var s=Load(path);Check(s.World.MapName=="MONDE03.ETA","actual crash snapshot recovered");
            using var original=JsonDocument.Parse(File.ReadAllText(path));
            Check(s.World.Resources.SequenceEqual(original.RootElement.GetProperty("Resources").Deserialize<ushort[]>()!),"recovery preserves resources");
            Check(s.World.Units.All(u=>u.FoodJob==0||u.MineJob==0),"exclusive worker jobs after migration");
            Check(s.OrderPreview().Serialize()==s.Serialize(),"actual snapshot preview roundtrip");
            for(int i=0;i<1800;i++){s.Step();if(i%120==0)s.OrderPreview();}
            Check(s.OrderPreview().Serialize()==s.Serialize(),"continued actual snapshot remains valid");
        }
        var loaded=Load(Directory.GetFiles(snapshots,"*6284.snapshot.json").Single());
        var a=loaded.World.Units.First(u=>u.Type==7&&u.Food>0);int cargo=a.Food;
        var gesture=new ResourceGesture();var depot=loaded.World.Buildings.First(b=>b.Owner==0&&b.Type==3&&loaded.Inhabited(b));
        Check(gesture.Begin(loaded,new[]{a},loaded.World.Mines[a.MineIndex].Cell),"loaded stone carrier can choose mine");
        Check(gesture.Commit(loaded,depot.Cell),"loaded stone carrier can commit delivery");
        Check(a.Food==cargo&&a.FoodJob==3&&a.MineJob==0,"new gesture preserves cargo and suspends new mine");
        loaded.OrderPreview();Check(true,"preview after loaded gesture does not crash");
        for(int i=0;i<3000;i++){loaded.Step();if(i%120==0)loaded.OrderPreview();}
        Check(a.Food==0&&a.MineJob!=0,"mine resumes after preserved cargo delivered");
        var overflow=Load(Directory.GetFiles(snapshots,"*21512.snapshot.json").Single());a=overflow.World.Units.First(u=>u.Slot==7);
        a.OverflowCell=17478;
        int spot=Enumerable.Range(0,65536).First(c=>overflow.PlacementError(a,3,c)==null);
        Check(overflow.StartConstruction(a,3,spot),"overflow carrier starts construction");
        Check(a.OverflowCell<0&&a.Rock==1,"starting construction removes obsolete marker and preserves cargo");
        overflow.OrderPreview();Check(true,"preview after starting construction is valid");
        var site=overflow.Constructions.Last();overflow.Stop(a);a.OverflowCell=17478;
        Check(overflow.ResumeConstruction(a,site),"overflow carrier joins construction");
        Check(a.OverflowCell<0&&a.Rock==1,"joining construction removes obsolete marker and preserves cargo");
        overflow.OrderPreview();Check(true,"preview after joining construction is valid");
        File.WriteAllText(output,JsonSerializer.Serialize(new{Passed=true,Checks=checks}));
    }
}
