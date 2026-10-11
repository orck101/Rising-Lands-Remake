namespace RisingLands;
public static class OpponentV77Tests {
 public static void Run(string root){
  int checks=0;void Check(bool ok,string name){if(!ok)throw new Exception(name);checks++;Console.WriteLine("PASS v77 integration: "+name);}
  Simulation Create(int scientists){
   var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE04.ETA"));new VisualCatalog().Apply(w);
   int slot=w.Buildings.Max(b=>b.Slot)+1;w.Buildings.Add(new(slot,7,1,40*256+40));w.BuildingResidents[slot]=new();
   var settlers=w.Units.Where(a=>a.Owner==1&&a.Type==9).Take(scientists).ToArray();if(settlers.Length!=scientists)throw new Exception("Missing original enemy colonists");
   foreach(var a in settlers){foreach(var ids in w.BuildingResidents.Values)ids.Remove(a.Slot);a.LegacyState=14;w.BuildingResidents[slot].Add(a.Slot);}
   var s=new Simulation(w);s.Campaign.EnemyBranches[1]=0;return s;
  }
  var idle=Create(0);for(int i=0;i<15000;i++)idle.OpponentResearchStep();Check(idle.Opponent.ResearchProgress[1]==0&&idle.Opponent.ResearchCompleted[1]==0,"empty laboratory cannot research");
  foreach(int scientists in new[]{1,2}){
   var s=Create(scientists);int level=s.OpponentLevel(1,0),duration=30*(scientists==1?420:240);var resources=s.World.Resources.ToArray();
   Check(s.EnemyScientists(1)==scientists,"actual enemy residents counted: "+scientists);
   for(int i=0;i<duration/2;i++)s.OpponentResearchStep();var saved=Create(scientists);saved.Restore(s.Serialize());Check(saved.Serialize()==s.Serialize(),"partial enemy research saved: "+scientists);
   for(int i=duration/2;i<duration-1;i++){s.OpponentResearchStep();saved.OpponentResearchStep();}
   Check(s.OpponentLevel(1,0)==level&&s.Opponent.ResearchCompleted[1]==0,"research waits full duration: "+scientists);
   s.OpponentResearchStep();saved.OpponentResearchStep();Check(s.OpponentLevel(1,0)==level+1&&s.Opponent.ResearchCompleted[1]==1,"completion at exact requested duration: "+scientists);
   Check(saved.Serialize()==s.Serialize(),"continued research matches after reload: "+scientists);
   for(int i=0;i<duration*3;i++)s.OpponentResearchStep();Check(s.Opponent.ResearchCompleted[1]==2&&s.OpponentLevel(1,0)==level+2,"enemy quota stops at two discoveries: "+scientists);
   Check(s.World.Resources.SequenceEqual(resources),"research preserves actual stock: "+scientists);
  }
  var pause=Create(1);for(int i=0;i<100;i++)pause.OpponentResearchStep();double progress=pause.Opponent.ResearchProgress[1];var lab=pause.World.Buildings.Single(b=>b.Type==7&&b.Owner==1);pause.World.BuildingResidents[lab.Slot].Clear();for(int i=0;i<100;i++)pause.OpponentResearchStep();Check(pause.Opponent.ResearchProgress[1]==progress,"removing laboratory crew pauses without losing progress");
  Check(pause.ConstructionStrikeProgressFor(1)>0,"enemy construction uses its own technology");
  Console.WriteLine("PASS v77 integration total "+checks);
 }
}
