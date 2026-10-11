namespace RisingLands;
public static class ExplorationV93Tests {
 public static void Run(string root){int checks=0;void Check(bool ok,string name){if(!ok)throw new Exception("FAIL v93: "+name);checks++;Console.WriteLine("PASS v93: "+name);}
  Check(Math.Abs(OpponentInitialExploration.RadiusFactor*OpponentInitialExploration.RadiusFactor-3)<1e-12,"circular radius sqrt(3) gives three times area");
  var corner=new byte[65536];corner[0]=corner[1]=corner[256]=1;var result=OpponentInitialExploration.Triple(corner,8,8,new[]{0});Check(result==(3,9)&&corner.Count(v=>v!=0)==9,"exact three times cells at map edge");Check(corner[0]==1&&corner[1]==1&&corner[256]==1,"initial known cells preserved");
  var small=new byte[65536];for(int y=0;y<2;y++)for(int x=0;x<3;x++)small[y*256+x]=1;result=OpponentInitialExploration.Triple(small,4,4,new[]{0});Check(result==(6,16)&&small.Count(v=>v!=0)==16,"physical map limit respected");Check(small[4]==0&&small[1024]==0,"no cells outside map marked");
  foreach(string map in new[]{"MONDE04.ETA","MONDE05.ETA"}){
   Simulation Load(){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD",map));new VisualCatalog().Apply(w);return new(w);}
   var s=Load();var originalPlayer=s.Exploration.Explored.ToArray();string initial=s.Serialize();
   foreach(int owner in s.Opponent.Explored.Keys.ToArray()){
    var visible=s.EnemyCurrentVisionForResearch(owner);int before=visible.Count(v=>v!=0),after=s.Opponent.Explored[owner].Count(v=>v!=0);
    Console.WriteLine($"AREA map={map} owner={owner} baseline={before} known={after} factor={(double)after/before:0.000} foodSources={Enumerable.Range(0,65536).Count(c=>s.Opponent.Explored[owner][c]!=0&&s.IsFood(c))} mines={s.World.Mines.Count(m=>s.Opponent.Explored[owner][m.Cell]!=0)}");
    Check(after==Math.Min(before*3,s.World.Width*s.World.Height),"exact initial area tripled "+map+" owner "+owner);Check(after<s.World.Width*s.World.Height,"whole map remains undiscovered "+owner);Check(visible.Any(v=>v==0)&&visible.Where((v,c)=>s.Opponent.Explored[owner][c]!=0).Count(v=>v==0)>0,"known area does not grant current sight "+owner);
    int farther=Enumerable.Range(0,65536).First(c=>s.Opponent.Explored[owner][c]==0&&(c&255)<s.World.Width&&(c>>8)<s.World.Height);
    var enemy=s.World.Units.First(a=>a.Owner==owner&&a.Type==5&&a.Health>0);enemy.X=(farther&255)*1024;enemy.Y=(farther>>8)*1024;int previous=after;s.EnemyCurrentVisionForResearch(owner);Check(s.Opponent.Explored[owner].Count(v=>v!=0)<previous+before*3,"movement stamps normal vision rather than retripling "+owner);
   }
   Check(s.Exploration.Explored.SequenceEqual(originalPlayer),"player discovery unchanged "+map);
   var copy=Load();copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"expanded and subsequently explored knowledge saves "+map);
   s.Restart();Check(s.Serialize()==initial,"restart reproduces initial expanded area "+map);
  }
  Console.WriteLine("ALL v93 EXPLORATION PASSED: "+checks);
 }
}
