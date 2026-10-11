namespace RisingLands;
public static class RampNavigationTests {
 static void Check(bool value,string name){if(!value)throw new InvalidDataException("FAIL: "+name);Console.WriteLine("PASS: "+name);}
 static Simulation Load(string root,string map){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD",map));new VisualCatalog().Apply(w);return new(w);}
 public static void Run(string root){
  var s=Load(root,"MONDE05.ETA");
  Check(s.Passable(14,77),"MONDE05 ramp tile 569, native collision 0, is traversable");
  Check(s.Passable(14,75),"MONDE05 plateau tile 599, native collision 0, is traversable");
  Check(!s.Passable(12,77)&&!s.Passable(16,78),"cliff walls flanking the ramp remain blocked");
  var mover=s.World.Units.First(a=>a.Owner==0&&VisualCatalog.Visible(a)&&!Simulation.Airborne(a));
  s.Stop(mover);mover.X=14*1024;mover.Y=80*1024;
  Check(s.Order(mover,14,75),"ground unit routes uphill through actual mission five ramp");
  Check(mover.Route.Any(c=>s.World.Tiles[c] is >=565 and <=598)&&mover.Route.All(c=>s.Passable(c&255,c>>8)),"route uses ramp and never crosses blocked cliff cells");
  string saved=s.Serialize();var copy=Load(root,"MONDE05.ETA");copy.Restore(saved);
  Check(copy.Serialize()==saved,"save restores in-progress uphill route exactly");
  for(int i=0;i<140;i++){s.Step();copy.Step();}
  Check(mover.X==14*1024&&mover.Y==75*1024&&mover.Route.Count==0,"unit physically reaches plateau");
  Check(s.Serialize()==copy.Serialize(),"movement across ramp remains deterministic after load");
  Check(s.Order(mover,14,80),"ground unit routes downhill");for(int i=0;i<140;i++)s.Step();
  Check(mover.X==14*1024&&mover.Y==80*1024,"unit physically descends ramp");
  foreach(int owner in Enumerable.Range(0,4)){
   var actor=s.World.Units.First(a=>a.Owner==owner&&VisualCatalog.Visible(a)&&!Simulation.Airborne(a));
   actor.X=181*1024;actor.Y=98*1024;
   Check(s.Order(actor,181,94,true),$"owner {owner} can route through red-side ramp to plateau");
  }
  int groundClasses=0;
  foreach(var actor in s.World.Units.Where(VisualCatalog.Visible).Where(a=>!Simulation.Airborne(a)).DistinctBy(a=>a.Type)){
   actor.X=14*1024;actor.Y=80*1024;
   Check(s.Order(actor,14,75,true),$"existing ground class {actor.Type} shares ramp navigation");groundClasses++;
  }
  int maps=0,open=0,blocked=0;
  foreach(var path in Directory.GetFiles(PackageLayout.Asset(root,"CD/WORLD"),"*.ETA")){
   var nav=Load(root,Path.GetFileName(path));int mapOpen=0,mapBlocked=0;
   for(int y=0;y<nav.World.Height;y++)for(int x=0;x<nav.World.Width;x++){
    int c=y*256+x,tile=nav.World.Tiles[c];if(tile is <477 or >608||nav.World.Objects[c] is >=1 and <=11||nav.BuildingAt(c)!=null||nav.InteractionPassage(c))continue;
    bool expected=(nav.World.Collision[c]&1)==0;
    if(nav.Passable(x,y)!=expected)throw new InvalidDataException($"Terrain mismatch: {nav.World.MapName} ({x},{y}), tile {tile}, collision {nav.World.Collision[c]}");
    if(expected)mapOpen++;else mapBlocked++;
   }
   open+=mapOpen;blocked+=mapBlocked;maps++;
   Check(true,$"{nav.World.MapName}: {mapOpen} native open mountain cells / {mapBlocked} blocked cells");
  }
  Check(maps==34&&open>0&&blocked>0&&groundClasses>0,"all 34 maps include open passages and blocked mountains");
 }
}
