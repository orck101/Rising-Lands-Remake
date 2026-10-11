namespace RisingLands;
public static class WorkerVisualTests {
 public static void Run(string root){
  void Check(bool ok,string text){if(!ok)throw new Exception("FAIL: "+text);Console.WriteLine("PASS: "+text);}
  var visuals=new VisualCatalog();
  foreach(int type in new[]{5,6,7}){
   var actor=new Actor{Type=type,Health=100,LegacyState=7};
   bool distinct=true;
   for(int direction=1;direction<=16;direction++)for(int phase=0;phase<8;phase++){
    actor.Direction=direction;actor.AnimationFrame=phase;actor.Food=0;actor.Rock=0;var empty=visuals.Frame(actor).ToArray();
    if(type is 5 or 6)actor.Food=1;else actor.Rock=1;
    var loaded=visuals.Frame(actor);distinct&=loaded.Count>0&&!loaded.SequenceEqual(empty);
   }
   Check(distinct,"original loaded sprites differ in all directions and eight movement frames: type "+type);
  }
  var s=MissionOneTests.Load(root);var farmer=s.World.Units.First(u=>u.Owner==0&&u.Type==5);farmer.RepeatFood=false;
  Check(Enumerable.Range(0,65536).Where(c=>s.World.Objects[c] is >=29 and <=52).OrderBy(c=>Math.Abs((c&255)-farmer.X/1024)+Math.Abs((c>>8)-farmer.Y/1024)).Any(c=>s.Gather(farmer,c)),"farmer accepts crop for visual cycle");
  for(int i=0;i<10000&&farmer.FoodJob!=2;i++)s.Step();
  Check(VisualCatalog.Hoeing(farmer,s)&&farmer.Food==0,"crop starts hoeing before food enters hands");
  var frames=new List<string>();
  for(int i=0;i<8;i++){frames.Add(string.Join(",",visuals.Frame(farmer,s).Select(c=>c.Sprite)));s.Step();}
  Check(frames.Distinct().Count()==4&&frames[0]==frames[1]&&frames[2]==frames[3],"hoe animation has four frames with two simulation ticks each");
  Check(frames[0]==string.Join(",",visuals.Frame(farmer,s).Select(c=>c.Sprite))&&farmer.Food==0,"hoe animation loops while harvest delay remains");
  var restored=MissionOneTests.Load(root);restored.Restore(s.Serialize());var resumed=restored.World.Units.First(u=>u.Slot==farmer.Slot);
  Check(visuals.Frame(resumed,restored).SequenceEqual(visuals.Frame(farmer,s)),"save restores exact hoe pose without new save fields");
  for(int i=0;i<1000&&farmer.Food==0;i++){s.Step();restored.Step();}
  Check(farmer.Food==1&&!VisualCatalog.Hoeing(farmer,s)&&s.Serialize()==restored.Serialize(),"harvest completes with loaded sprite and deterministic restoration");
  var pile=MissionOneTests.Load(root);var picker=pile.World.Units.First(u=>u.Owner==0&&u.Type==5);picker.RepeatFood=false;
  int cell=picker.Y/1024*256+picker.X/1024;pile.World.Objects[cell]=67;pile.World.Quantities[cell]=1;
  Check(pile.Gather(picker,cell),"loose food pickup starts");pile.Step();
  Check(picker.FoodJob==2&&!VisualCatalog.Hoeing(picker,pile)&&picker.WorkTicks==10,"loose food uses short pickup delay without hoeing");
  var mine=MissionOneTests.Load(root);var builder=mine.World.Units.First(u=>u.Type==7&&u.Owner==0);builder.RepeatMine=false;
  Check(mine.MineNearest(builder),"builder heads to mine");
  for(int i=0;i<10000&&builder.MineJob!=2;i++)mine.Step();
  Check(VisualCatalog.HiddenByWork(builder)&&visuals.Frame(builder,mine).Count==0&&builder.Rock==0,"builder disappears with shadow during extraction before gaining rock");
  for(int i=0;i<1000&&builder.Rock==0;i++)mine.Step();
  Check(builder.Rock==1&&!VisualCatalog.HiddenByWork(builder)&&visuals.Frame(builder,mine).Count>0,"builder reappears carrying sack after extraction");
  int ground=Enumerable.Range(0,65536).Where(mine.RockGroundAllowed).OrderBy(c=>Math.Abs((c&255)-builder.X/1024)+Math.Abs((c>>8)-builder.Y/1024)).First();
  Check(mine.SetRockGround(builder,ground),"loaded builder accepts nearby ground delivery");
  for(int i=0;i<1000&&builder.Rock>0;i++)mine.Step();
  var emptyBuilder=new Actor{Type=7,Direction=builder.Direction,AnimationFrame=builder.AnimationFrame};
  Check(builder.Rock==0&&visuals.Frame(builder,mine).SequenceEqual(visuals.Frame(emptyBuilder)),"builder returns to empty sprite after putting down sack");
 }
}
