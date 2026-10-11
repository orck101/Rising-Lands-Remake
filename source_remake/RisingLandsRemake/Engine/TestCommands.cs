namespace RisingLands;
// Explicit test grants are tracked separately from normal discoveries and resource production.
public sealed class TestState {
 public long Food{get;set;} public long Rock{get;set;} public long Parts{get;set;}
 public List<string> Technologies{get;set;}=new();
 public bool Victory{get;set;}
}
public sealed partial class Simulation {
 public TestState Tests{get;private set;}=new();
 public bool CanTestResource(int kind)=>kind is >=0 and <=2&&World.Resources[ResourceIndex(kind)]<=ushort.MaxValue-50&&(kind==1?Tests.Food:kind==2?Tests.Rock:Tests.Parts)<=1_000_000_000-50&&!MissionWon;
 public bool GrantTestResource(int kind){
  if(!CanTestResource(kind))return false;
  World.Resources[ResourceIndex(kind)]+=50;
  if(kind==1)Tests.Food+=50;else if(kind==2)Tests.Rock+=50;else Tests.Parts+=50;
  return true;
 }
 public bool CanTestTechnology(int branch)=>branch is >=0 and <=3&&ResearchInvention(branch)!=null&&!MissionWon;
 public bool GrantTestTechnology(int branch){
  if(!CanTestTechnology(branch))return false;
  var r=ResearchState(branch);var t=ResearchInvention(branch)!;
  Tests.Technologies.Add(t.Key);r.Index++;r.Progress=0;RefreshResearchEnabled();return true;
 }
 public bool TechnologyApplied(Invention t)=>t.Index<Campaign.StartingLevels[t.Branch]||ResearchState(t.Branch).Completed.Contains(t.Key)||Tests.Technologies.Contains(t.Key);
 public bool CompleteTestMission(){if(MissionWon)return false;Tests.Victory=true;MissionChecks=100;return true;}
 void ValidateTestState(Save s){
  if(s.Version>=26&&s.Tests==null)throw new InvalidDataException("Missing test state");
  var t=s.Tests??new TestState();
  if(new[]{t.Food,t.Rock,t.Parts}.Any(v=>v<0||v>1_000_000_000||v%50!=0)||t.Technologies==null||t.Technologies.Count>100||t.Technologies.Distinct().Count()!=t.Technologies.Count||t.Technologies.Any(k=>!UnlockData.Technologies.Any(i=>i.Key==k))||t.Victory&&s.MissionChecks!=100)throw new InvalidDataException("Invalid test grants");
  Tests=t;
 }
}
