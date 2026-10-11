using System.Text.Json;
namespace RisingLands;
public static class AttackAlertTests {
    static void Check(bool value,string label){if(!value)throw new Exception("FAIL: "+label);Console.WriteLine("PASS: "+label);}
    public static void Run(string root){
        using var data=JsonDocument.Parse(File.ReadAllText(PackageLayout.FromBase("Content/Data/attack_alert.json")));
        Check(data.RootElement.GetProperty("timing").EnumerateArray().All(v=>AttackAlert.CanRecord(v.GetProperty("now").GetUInt32(),v.GetProperty("last").GetUInt32(),v.GetProperty("owner").GetInt32())==v.GetProperty("expected").GetBoolean()),"16 native alert throttle and clock-reset cases match");
        Check(data.RootElement.GetProperty("camera").EnumerateArray().All(v=>{int cell=v.GetProperty("cell").GetInt32();var pos=AttackAlert.Recent((uint)(2000+v.GetProperty("age").GetInt32()),2000,cell)?AttackAlert.Camera(cell,v.GetProperty("width").GetInt32(),v.GetProperty("height").GetInt32(),256,256):(17,23);return pos==(v.GetProperty("x").GetInt32(),v.GetProperty("y").GetInt32());}),"60 native last-attack camera and expiry cases match");
        var a=new AttackAlert();a.Record(1,8224,1);Check(a.Cell==65535,"enemy damage does not create a player alert");a.Record(1,8224,0);Check(a.Cell==8224,"first modern-session attack is immediately navigable");a.Record(200,8225,0);Check(a.Cell==8224,"repeated damage cannot replace alert before 200 updates");a.Record(201,8225,0);Check(a.Cell==8225,"new attack replaces alert at cooldown boundary");
        var world=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));var visuals=new VisualCatalog();visuals.Apply(world);var sim=new Simulation(Simulation.TrainingWorld(world,true));var hero=sim.World.Units.First(u=>u.Owner==0&&u.Type==8);sim.Attack(hero,sim.World.Units.Last());for(int i=0;i<100;i++)sim.Step();Check(hero.Health<100&&sim.LastAttack.Cell==hero.Y/1024*256+hero.X/1024,"actual retaliation records player attack location");
        string save=sim.Serialize();sim.Restore(save);Check(sim.LastAttack.Cell==65535,"loading a save clears stale session attack location");
        Check(AttackAlert.Camera(8224,300,300,256,256)==(0,0),"modern oversized viewport clamps safely");
    }
}
