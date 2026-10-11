namespace RisingLands;

// User-selected remake personalities, not recovered rules of the original AI.
// Branch indices: agriculture 0, military 1, engineering 2, spirituality 3.
public sealed record EnemyPersonality(string Color,string Name,int[] Branches,int[] Targets,int[] Buildings,int[] ArmyCycle);
public static class EnemyPersonalities {
 static readonly EnemyPersonality Red=new("Vermelho","Exército sustentado",new[]{0,1},new[]{11,20},new[]{7,5,13,18,10,11,6,15,17},new[]{8,0,1,8,2,0,8});
 static readonly EnemyPersonality Yellow=new("Amarelo","Fortaleza produtiva",new[]{0,2},new[]{15,16},new[]{7,13,18,5,10,11,6,15,17},new[]{3,25,8,3,16,0,1,2});
 static readonly EnemyPersonality Green=new("Verde","Indústria militar",new[]{2,1},new[]{18,18},new[]{7,5,18,14,10,11,13,6,15,17},new[]{16,11,8,12,16,1,2,0});
 public static EnemyPersonality For(int owner)=>owner switch{1=>Red,2=>Yellow,3=>Green,_=>throw new ArgumentOutOfRangeException(nameof(owner))};
 public static int ChooseResearch(int owner,int mission,int[] levels){
  var profile=For(owner);
  bool Available(int branch)=>Simulation.UnlockData.Technologies.Any(t=>t.Branch==branch&&t.Index==levels[branch]&&t.Mission<=mission);
  // Secure the ordinary research infrastructure first; this does not grant a level.
  if(levels[2]<5&&Available(2))return 2;
  var preferred=Enumerable.Range(0,profile.Branches.Length).Where(i=>Available(profile.Branches[i])).OrderBy(i=>(double)levels[profile.Branches[i]]/profile.Targets[i]).ThenBy(i=>i).ToArray();
  if(preferred.Length>0)return profile.Branches[preferred[0]];
  // A capped or mission-locked specialty must not stall otherwise legal research.
  return new[]{2,0,1,3}.FirstOrDefault(b=>(b!=3||mission>=7)&&Available(b),-1);
 }
 public static int[] ProductionCycle(int owner,IEnumerable<int> legal){var available=legal.Distinct().Order().ToArray();var cycle=For(owner).ArmyCycle.Where(available.Contains).ToList();cycle.AddRange(available.Where(t=>!cycle.Contains(t)));return cycle.ToArray();}
}
public sealed partial class Simulation {
 public EnemyPersonality PersonalityFor(int owner)=>EnemyPersonalities.For(owner);
}
