namespace RisingLands;
public static class SelfTests {
    static void Check(bool result,string name){if(!result)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
    public static void Run(string root){
        int count=0;foreach(string p in Directory.GetFiles(PackageLayout.Asset(root,"CD","WORLD"),"*.ETA")){var w=LegacyAssets.Map(p);Check(w.Tiles.Length==65536&&w.Units.Count>0,Path.GetFileName(p));count++;}Check(count==34,"all 34 maps");
        var palette=LegacyAssets.Palette(PackageLayout.Asset(root,"PLATGAME.PIE"));int sprites=0;
        foreach(string p in Directory.GetFiles(PackageLayout.Asset(root,"GRAPHICS"),"*.CMP")){var f=LegacyAssets.Sprites(p,palette);sprites+=f.Count;}Check(sprites==12268,"12268 CMP frames");
        var catalog=new VisualCatalog();
        Check(catalog.Catalog.Banks.Select(b=>b.Base).SequenceEqual(new[]{1,1346,4194,5109,7029,7640}),"original one-based sprite bank boundaries");
        bool ValidSprite(DrawCommand c)=>catalog.Catalog.Banks.Any(b=>c.Sprite>=b.Base&&c.Sprite<b.Base+b.Count);
        Check(catalog.Catalog.Types.All(t=>t.Frames.Count==16*t.FrameCount&&t.Frames.All(f=>f.Commands.All(ValidSprite))),"unit directions, phases and sprite bounds");
        foreach(string p in Directory.GetFiles(PackageLayout.Asset(root,"CD","WORLD"),"*.ETA")){var w=LegacyAssets.Map(p);var scene=catalog.Scene(w);Check(scene.Objects.All(o=>o.Cell>=0&&o.Cell<65536&&o.Commands.All(ValidSprite)),"scene hash and sprite bounds "+w.MapName);catalog.Apply(w);}
        var first=LegacyAssets.Map(PackageLayout.Asset(root,"CD","WORLD","MONDE01.ETA"));catalog.Apply(first);
        Check(first.Units.Count(u=>!VisualCatalog.Visible(u))==8,"eight original hidden occupants");
        var nav=new Simulation(first);var mover=first.Units.First(VisualCatalog.Visible);
        Check(Enumerable.Range(0,65536).Where(i=>(first.Collision[i]&1)!=0&&!nav.InteractionPassage(i)).All(i=>!nav.Passable(i&255,i>>8)),"collision mask blocks navigation");
        Check(first.Collision.Any(v=>(v&1)!=0),"collision test is nonempty");
        bool routed=false;for(int y=0;y<first.Height&&!routed;y++)for(int x=0;x<first.Width&&!routed;x++)if(Math.Abs(x-mover.X/1024)+Math.Abs(y-mover.Y/1024)>8)routed=nav.Order(mover,x,y);
        Check(routed&&mover.Route.All(i=>nav.Passable(i&255,i>>8)),"route avoids initialized scene obstacles");
        int oldFrame=mover.AnimationFrame;mover.AnimationCounter=0;mover.Route=new(){(mover.Y/1024)*256+mover.X/1024+1};for(int i=0;i<mover.AnimationDelay;i++)nav.Step();
        Check(mover.Direction==9&&mover.AnimationFrame==(oldFrame+1)%mover.AnimationCount,"east movement advances original animation sequence");
        string path=PackageLayout.Asset(root,"CD","WORLD","MONDE01.ETA");var sim=new Simulation(LegacyAssets.Map(path));var a=sim.World.Units[0];
        int startX=a.X,startY=a.Y;bool found=false;for(int y=0;y<sim.World.Height&&!found;y++)for(int x=0;x<sim.World.Width&&!found;x++)if(Math.Abs(x-a.X/1024)+Math.Abs(y-a.Y/1024)>5)found=sim.Order(a,x,y);
        Check(found,"reachable route");int destination=a.Route[^1];for(int i=0;i<20;i++)sim.Step();Check(a.X!=startX||a.Y!=startY,"unit position changes");string saved=sim.Serialize();var other=new Simulation(LegacyAssets.Map(path));other.Restore(saved);
        for(int i=0;i<100;i++){sim.Step();other.Step();}Check(sim.Serialize()==other.Serialize(),"save/load deterministic continuation");
        string before=other.Serialize();var invalidVersion=System.Text.Json.Nodes.JsonNode.Parse(saved)!;invalidVersion["Version"]=149;try{other.Restore(invalidVersion.ToJsonString());throw new Exception("Invalid save accepted");}catch(InvalidDataException){}Check(other.Serialize()==before,"invalid save leaves state intact");
        Check(!sim.Order(a,-1,0),"out of bounds order rejected");for(int i=0;i<10000&&a.Route.Count>0;i++)sim.Step();Check(a.Route.Count==0&&a.X==(destination&255)*1024&&a.Y==(destination>>8)*1024,"unit reaches requested destination");
        string disk=PackageLayout.FromBase("TestArtifacts","navigation.research.json");sim.SaveTo(disk);var reloaded=new Simulation(LegacyAssets.Map(path));reloaded.Restore(File.ReadAllText(disk));Check(reloaded.Serialize()==sim.Serialize(),"save file written and reloaded");FoodTests.Run(root);ConstructionTests.Run(root);RecruitmentTests.Run(root);CombatTests.Run(root);ArcherTests.Run(root);TimingTests.Run(root);SelectionTests.Run();ControlGroupTests.Run();AttackAlertTests.Run(root);UnitInfoTests.Run();MouseCommandTests.Run();PauseMenuTests.Run(root);ObjectiveTests.Run(root);ResearchTests.Run(root);HarvestTests.Run(root);MiningTests.Run(root);Console.WriteLine("ALL TESTS PASSED");
    }
}


