namespace RisingLands;
public sealed partial class Simulation {
 public bool IsNinthMission=>new[]{"MONDE09A.ETA","MONDE09B.ETA","MONDE09C.ETA","MONDE09D.ETA"}.Contains(World.MapName,StringComparer.OrdinalIgnoreCase);
 static bool NinthMissionSatisfied(IEnumerable<Actor> actors,EngineeringResearch? research)=>SixthSurvivors(actors)&&research!=null&&research.Completed.Count+(research.Agriculture?.Completed.Count??0)+(research.Military?.Completed.Count??0)+(research.Spiritual?.Completed.Count??0)>=2;
 // MakeMissionName 0x46c04c: least sympathetic clan, first owner wins ties.
 public string NinthNextMap=>SelectTenthMap(Campaign.Goodwill);
 public static string SelectTenthMap(IReadOnlyList<int> goodwill){
  if(goodwill.Count!=4)throw new ArgumentException("Four clan relations required",nameof(goodwill));
  int owner=Enumerable.Range(1,3).OrderBy(i=>goodwill[i]).ThenBy(i=>i).First();
  return "MONDE10"+(char)('A'+owner-1)+".ETA";
 }
 public string NextMissionMap=>IsEighthMission?EighthNextMap:IsNinthMission?NinthNextMap:$"MONDE{MissionNumber+1:00}.ETA";
 public string? NinthResearchChoiceError(int branch){
  if(branch is <0 or >3)return "Escolha um ramo de tecnologia.";
  int index=Math.Max(EffectiveLevels[branch],UnlockData.Missions.First(m=>m.Mission==9).Levels[branch]);
  for(int discovery=0;discovery<2;discovery++){
   while(UnlockData.Technologies.FirstOrDefault(t=>t.Branch==branch&&t.Index==index) is {} gate&&gate.Mission>9&&gate.Appearance<100)index++;
   var t=UnlockData.Technologies.FirstOrDefault(t=>t.Branch==branch&&t.Index==index);
   if(t==null)return "Este ramo nao possui duas descobertas restantes. Escolha outro.";
   if(t.Mission>9)return "Este ramo exige uma missao posterior. Escolha outro.";
   if(!SupportedBranchResearch(t))return "Este ramo ainda nao permite duas descobertas no remake. Escolha outro.";
   index++;
  }
  return null;
 }
}
