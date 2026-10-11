namespace RisingLands;
public static class OpponentInitialExploration {
 public static (int Before,int After) Triple(byte[] known,int width,int height,IEnumerable<int> baseCells){
  var cells=Enumerable.Range(0,height).SelectMany(y=>Enumerable.Range(0,width).Select(x=>y*256+x)).ToArray();int before=cells.Count(c=>known[c]!=0),target=Math.Min(cells.Length,before*3);
  var anchors=baseCells.Distinct().ToArray();if(before==0||anchors.Length==0)return(before,before);
  int Distance(int c)=>anchors.Min(a=>((c&255)-(a&255))*((c&255)-(a&255))+((c>>8)-(a>>8))*((c>>8)-(a>>8)));
  foreach(int cell in cells.Where(c=>known[c]==0).OrderBy(Distance).ThenBy(c=>c).Take(target-before))known[cell]=1;
  return(before,target);
 }
 public static double RadiusFactor=>Math.Sqrt(3);
}
public sealed partial class Simulation {
 void InitializeOpponentExploration(){
  if(!OpponentEnabled)return;
  InitializeTerrainKnowledge();foreach(int owner in Opponent.Explored.Keys.ToArray())OpponentVision(owner);
 }
 internal byte[] EnemyCurrentVisionForResearch(int owner)=>OpponentVision(owner);
}
