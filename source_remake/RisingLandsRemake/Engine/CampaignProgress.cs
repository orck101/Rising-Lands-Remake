namespace RisingLands;
public sealed class CampaignProgress {
 public int[][] EnemyLevels{get;set;}=Enumerable.Range(0,4).Select(_=>new int[4]).ToArray();
 public int[][] EnemyThresholds{get;set;}=Enumerable.Range(0,4).Select(_=>new int[4]).ToArray();
 public int[] EnemyBranches{get;set;}=Enumerable.Repeat(-1,4).ToArray();public uint EnemyRandomSeed{get;set;}public int EnemyMission{get;set;}
 public int[] Goodwill{get;set;}=new int[4];public bool[] Alliances{get;set;}=new bool[4];
 public int[] StartingLevels{get;set;}=Array.Empty<int>();
 public int SelectedBranch{get;set;}=-1;
 public int CompletedMission{get;set;}
 public bool TransitionOpen{get;set;}
 public double CarriedProgress{get;set;}
}
public sealed partial class Simulation {
 public CampaignProgress Campaign{get;private set;}=new();
 public int[] EffectiveLevels {get{var levels=Campaign.StartingLevels.ToArray();for(int b=0;b<4;b++)levels[b]=ResearchState(b).Index;return levels;}}
 CampaignProgress DefaultCampaign()=>new(){StartingLevels=UnlockData.Missions.First(m=>m.Mission==MissionNumber).Levels.ToArray()};
 void ValidateCampaign(CampaignProgress campaign){
  ValidateEnemyCampaign(campaign);
  if(campaign.Goodwill==null||campaign.Goodwill.Length!=4||campaign.Goodwill.Any(x=>x< -60||x>100)||campaign.Alliances==null||campaign.Alliances.Length!=4)throw new InvalidDataException("Invalid diplomacy");
  if(campaign.StartingLevels==null||campaign.StartingLevels.Length!=4||campaign.SelectedBranch is < -1 or >3||campaign.CompletedMission<0||campaign.CompletedMission>=MissionNumber||!double.IsFinite(campaign.CarriedProgress)||campaign.CarriedProgress<0||campaign.CarriedProgress>65000||campaign.TransitionOpen&&!HasMissionEnding)throw new InvalidDataException("Invalid campaign state");
  var baseline=UnlockData.Missions.First(m=>m.Mission==MissionNumber).Levels;
  for(int b=0;b<4;b++)if(campaign.StartingLevels[b]<baseline[b]||campaign.StartingLevels[b]>UnlockData.Technologies.Where(t=>t.Branch==b).Max(t=>t.Index)+1)throw new InvalidDataException("Invalid inherited technology level");
 }
 public Invention[] NextMissionTechnologies(int branch)=>branch is <0 or >3?Array.Empty<Invention>():UnlockData.Technologies.Where(t=>t.Branch==branch&&t.Index>=EffectiveLevels[branch]&&t.Mission<=MissionNumber+1).OrderBy(t=>t.Index).Take(2).ToArray();
 public bool ChooseNextResearch(int branch){if(!MissionWon||!Campaign.TransitionOpen||branch is <0 or >3)return false;Campaign.SelectedBranch=branch;return true;}
 public CampaignProgress ProgressForMissionTwo()=>ProgressForNextMission();
 public CampaignProgress ProgressForNextMission(){
  if(!MissionWon||!Campaign.TransitionOpen||Campaign.SelectedBranch<0)throw new InvalidOperationException("Choose a branch after victory");
  var levels=EffectiveLevels;var baseline=UnlockData.Missions.First(m=>m.Mission==MissionNumber+1).Levels;
  for(int i=0;i<4;i++)levels[i]=Math.Max(levels[i],baseline[i]);
  double progress=Campaign.SelectedBranch==ActiveResearchBranch?ResearchState(ActiveResearchBranch).Progress:0;
  var enemyLevels=Enumerable.Range(0,4).Select(owner=>Enumerable.Range(0,4).Select(branch=>owner==0?0:OpponentLevel(owner,branch)).ToArray()).ToArray();
  foreach(int owner in Enumerable.Range(1,3).Where(owner=>InitialActors.Values.Any(a=>a.Owner==owner)))enemyLevels[owner]=EnemyResearchPolicy.Adjust(MissionNumber,MissionResearchCount,enemyLevels[owner],Campaign.EnemyBranches[owner]);
  return new(){EnemyLevels=enemyLevels,EnemyBranches=(int[])Campaign.EnemyBranches.Clone(),EnemyThresholds=CloneState(Campaign.EnemyThresholds),EnemyRandomSeed=Campaign.EnemyRandomSeed,EnemyMission=Campaign.EnemyMission,StartingLevels=levels,Goodwill=(int[])Campaign.Goodwill.Clone(),Alliances=(bool[])Campaign.Alliances.Clone(),SelectedBranch=Campaign.SelectedBranch,CompletedMission=MissionNumber,CarriedProgress=progress};
 }
}
