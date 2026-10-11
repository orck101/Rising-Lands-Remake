using System.Text.Json;
namespace RisingLands;
public static class MissionNineProgressionTests {
 public static void Run(string root,string output){var checks=new List<string>();void Check(bool ok,string label){if(!ok)throw new InvalidDataException(label);checks.Add(label);}
  Simulation Load(string name,int[] levels,int branch){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/"+name));new VisualCatalog().Apply(w);return new(w,new(){StartingLevels=levels,SelectedBranch=branch});}
  var s=Load("MONDE08.ETA",new[]{2,2,7,4},2);
  for(int b=0;b<4;b++)Check(s.NinthResearchChoiceError(b)==null,"direct baseline permits two discoveries in branch "+b);
  foreach(char v in "ABCD"){
   var advanced=Load("MONDE09"+v+".ETA",new[]{4,4,9,6},0);Check(advanced.EffectiveLevels.SequenceEqual(new[]{4,4,9,6}),"higher inherited levels preserved "+v);Check(advanced.MilitaryDamageBonus(new Actor{Owner=0,Type=0})==1,"inherited military damage still applies "+v);Check(advanced.SpiritualDamageBonus(new Actor{Owner=0,Type=4})==2,"inherited spiritual damage still applies "+v);
   Check(advanced.NinthResearchChoiceError(0)==null,"inherited agriculture remains a valid two-discovery choice "+v);
   foreach(int b in new[]{1,2,3})Check(advanced.NinthResearchChoiceError(b)!=null,"unimplemented later effect identified before entry "+v+b);
   string before=advanced.Serialize();advanced.Restore(before);Check(advanced.Serialize()==before&&advanced.MissionResearchCount==0,"inherited levels do not count as discoveries "+v);
  }
  // Isolated M8 building fixtures verify all rival subsets and duplicate sanctuaries.
  for(int mask=0;mask<8;mask++){
   s=Load("MONDE08.ETA",new[]{2,2,7,4},2);for(int o=1;o<=3;o++)if((mask&(1<<(o-1)))!=0){s.World.Buildings.Add(new Building(900+o,2,o,12*256+108));s.World.Buildings.Add(new Building(910+o,2,o,90*256+108));}
   string expected="MONDE09"+(char)('A'+System.Numerics.BitOperations.PopCount((uint)mask))+".ETA";Check(s.NextMissionMap==expected,"eight-to-nine subset "+mask+" counts clans, not buildings");
   var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/"+expected));new VisualCatalog().Apply(w);var next=new Simulation(w,new(){StartingLevels=new[]{4,2,9,4},SelectedBranch=0,CompletedMission=8});Check(next.IsNinthMission&&next.EffectiveLevels.SequenceEqual(new[]{4,2,9,4}),"all four native selected files accept inherited campaign "+mask);
  }
  foreach(char v in "ABC"){var next=Load("MONDE10"+v+".ETA",new[]{4,4,9,6},0);string before=next.Serialize();next.Restore(before);Check(next.MissionNumber==10&&next.Serialize()==before,"all mission-ten variants load and roundtrip "+v);}
  File.WriteAllText(output,JsonSerializer.Serialize(new{Passed=true,Checks=checks.Count,Assertions=checks,Limit="Selection and inherited-level fixtures; no fake research completions. Advanced unsupported research effects remain a documented implementation gap."},new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine("PASS M9 progression "+checks.Count);
 }
}
