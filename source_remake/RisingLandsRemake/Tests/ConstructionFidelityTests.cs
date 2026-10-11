namespace RisingLands;
public static class ConstructionFidelityTests {
 public static void Run(string root){
  void Check(bool ok,string text){if(!ok)throw new Exception("FAIL: "+text);Console.WriteLine("PASS: "+text);}
  var s=MissionOneTests.Load(root);var u=s.World.Units.First(a=>a.Type==7&&a.Owner==0);
  int grass=Enumerable.Range(0,65536).First(c=>s.PlacementError(u,3,c)==null);int corner=grass+4+4*256;
  Check(Enumerable.Range(0,25).All(i=>s.ConstructionGrass(grass+i%5+i/5*256)),"warehouse footprint fits entirely on original grass terrain");
  ushort old=s.World.Tiles[corner];s.World.Tiles[corner]=1;string before=s.Serialize();
  Check(s.PlacementError(u,3,grass)=="Construa inteiramente sobre o terreno verde."&&!s.StartConstruction(u,3,grass)&&s.Serialize()==before,"one non-green corner rejects whole building without spending or changing orders");s.World.Tiles[corner]=old;
  int dry=Enumerable.Range(0,65536).First(c=>s.Passable(c&255,c>>8)&&!s.ConstructionGrass(c));
  Check(s.PlacementError(u,3,dry)!=null,"walkable non-green ground cannot receive a building");
  Check(s.StartConstruction(u,3,grass),"original green footprint accepts warehouse");var site=s.Constructions.Last();var v=new VisualCatalog();
  Check(v.ConstructionVisuals.Buildings.Count==22&&v.ConstructionVisuals.Buildings.All(b=>b.Stages.Count==101),"all building types have native visual stages from zero to one hundred percent");
  var stages=v.ConstructionVisuals.Buildings.First(b=>b.Type==3).Stages;
  Check(stages.Select(b=>System.Text.Json.JsonSerializer.Serialize(b.Objects)).Distinct().Count()>50&&stages.SelectMany(b=>b.Objects).SelectMany(o=>o.Commands).Any(c=>c.Clip!=null),"warehouse native reveal changes with progress and retains original clipping");
  for(int i=0;i<10000&&site.Progress==0;i++)s.Step();
  Check(site.Progress>0&&v.Frame(u,s).SequenceEqual(v.BuilderVisual.Frames.First(f=>f.Direction==u.Direction&&f.Phase==site.AnimationTick/v.BuilderVisual.FrameDelay%v.BuilderVisual.FrameCount).Commands),"working builder uses native construction animation");
  var copy=MissionOneTests.Load(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"construction restores with original progress and visual stage");
  s.Stop(u);int stopped=site.Progress;for(int i=0;i<20;i++)s.Step();Check(site.Progress==stopped,"paused construction freezes progress and building appearance");
  Check(s.ResumeConstruction(u,site),"builder resumes unfinished green-ground construction");for(int i=0;i<10000&&!s.Complete(site);i++)s.Step();
  Check(s.Complete(site)&&s.World.Buildings.Any(b=>b.Slot==site.Id),"native staged warehouse becomes functional when finished");
 }
}
