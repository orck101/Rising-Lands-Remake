namespace RisingLands;
public sealed class OriginalIaRandom {
 public uint State{get;private set;}
 public OriginalIaRandom(uint seed){State=seed%2147483647;if(State==0)State=1;}
 public int Next(int maximum){if(maximum<=0)throw new ArgumentOutOfRangeException(nameof(maximum));long value=16807L*(State%127773)-2836L*(State/127773);if(value<=0)value+=2147483647;State=(uint)value;return (int)(State%(uint)maximum);}
}
public static class EnemyResearchPolicy {
 public static int Maximum(int branch)=>Simulation.UnlockData.Technologies.Where(t=>t.Branch==branch).Max(t=>t.Index)+1;
 public static int Choose(int owner,int mission,int[] levels,int[] thresholds,OriginalIaRandom random){
  if(thresholds.All(v=>v==0)){int favorite=owner==1?1:owner==3?0:random.Next(4),weight=30+random.Next(20),step=(100-weight)/3,cumulative=0;for(int b=0;b<3;b++){cumulative+=b==favorite?weight:step;thresholds[b]=cumulative;}thresholds[3]=100;}
  for(int b=0;b<4;b++)if(levels[b]>=Maximum(b))thresholds[b]=-1;
  if(thresholds.All(v=>v==-1))return -1;
  for(int attempt=0;attempt<1000;attempt++){
   int roll=random.Next(100),branch=0;while(branch<4&&roll>=thresholds[branch])branch++;
   if(branch>=4||thresholds[branch]==-1)branch=Array.FindIndex(thresholds,v=>v!=-1);
   if(branch==3&&mission<7)continue;return branch;
  }
  // An entirely unavailable early spiritual branch must not hang the remake.
  return Enumerable.Range(0,4).FirstOrDefault(b=>thresholds[b]!=-1&&(b!=3||mission>=7),-1);
 }
 public static int[] Adjust(int mission,int discoveries,int[] levels,int branch){var result=(int[])levels.Clone();if(mission>=3&&branch is >=0 and <4)result[branch]=Math.Min(Maximum(branch),result[branch]+discoveries);return result;}
}
public sealed partial class Simulation {
 void InitializeEnemyCampaign(){if(Campaign.EnemyMission==MissionNumber)return;
  var random=new OriginalIaRandom(Campaign.EnemyRandomSeed!=0?Campaign.EnemyRandomSeed:BitConverter.ToUInt32(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(World.SourceHash)),0)&0x7fffffff);
  foreach(int owner in Enumerable.Range(1,3)){var levels=Enumerable.Range(0,4).Select(b=>OpponentLevel(owner,b)).ToArray();Campaign.EnemyLevels[owner]=levels;if(!InitialActors.Values.Any(a=>a.Owner==owner))continue;Campaign.EnemyBranches[owner]=EnemyPersonalities.ChooseResearch(owner,MissionNumber,levels);}
  Campaign.EnemyRandomSeed=random.State;Campaign.EnemyMission=MissionNumber;
 }
 internal int EnemyScientists(int owner)=>World.Buildings.Where(b=>b.Owner==owner&&b.Type==7).Sum(b=>World.BuildingResidents.GetValueOrDefault(b.Slot,new()).Count(slot=>World.Units.Any(a=>a.Slot==slot&&a.Owner==owner&&a.Type==9&&a.Health>0&&a.LegacyState==14)&&!ResidentReserved(slot)));
 internal void OpponentResearchStep(){
  if(!OpponentEnabled)return;
  foreach(int owner in Enumerable.Range(1,3)){
   if(IsFifthMission&&ClanEliminated(owner))continue;
   int branch=Campaign.EnemyBranches[owner];if(branch<0||Opponent.ResearchCompleted[owner]>=2)continue;
   int level=OpponentLevel(owner,branch);var invention=UnlockData.Technologies.FirstOrDefault(t=>t.Branch==branch&&t.Index==level);
   if(invention==null){Campaign.EnemyBranches[owner]=(branch+1)%4;continue;}
   if(invention.Mission>MissionNumber){if(invention.Appearance!=100){Campaign.EnemyLevels[owner][branch]=level+1;Opponent.ResearchProgress[owner]=0;}continue;}
   int scientists=EnemyScientists(owner);if(scientists==0)continue;
   // User-requested timing applies to every player: 7 min with one, 4 min with two.
   Opponent.ResearchProgress[owner]+=1/(30.0*(scientists==1?420:240));
   if(Opponent.ResearchProgress[owner]+1e-10<1)continue;
   Campaign.EnemyLevels[owner][branch]=level+1;Opponent.ResearchCompleted[owner]++;Opponent.ResearchProgress[owner]=0;
  }
 }
 void ValidateEnemyCampaign(CampaignProgress campaign){if(campaign.EnemyLevels==null||campaign.EnemyThresholds==null||campaign.EnemyBranches==null||campaign.EnemyLevels.Length!=4||campaign.EnemyThresholds.Length!=4||campaign.EnemyBranches.Length!=4||campaign.EnemyBranches.Any(b=>b is < -1 or >3)||campaign.EnemyMission<0||campaign.EnemyMission>MissionNumber||campaign.EnemyRandomSeed>=2147483647)throw new InvalidDataException("Invalid enemy campaign research");
  for(int owner=0;owner<4;owner++){var levels=campaign.EnemyLevels[owner];var weights=campaign.EnemyThresholds[owner];if(levels==null||levels.Length!=4||weights==null||weights.Length!=4||weights.Any(w=>w is < -1 or >100))throw new InvalidDataException("Invalid enemy research fields");for(int branch=0;branch<4;branch++)if(levels[branch]<0||levels[branch]>EnemyResearchPolicy.Maximum(branch))throw new InvalidDataException("Invalid enemy inherited level");}
 }
}
