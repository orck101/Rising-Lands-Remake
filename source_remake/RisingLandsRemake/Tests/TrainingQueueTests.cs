using System.Text.Json;
using System.Text.Json.Nodes;
using System.Reflection;
namespace RisingLands;
public static class TrainingQueueTests {
 public static void Run(string root,string output){
  var assertions=new List<string>();void Check(bool ok,string label){if(!ok)throw new Exception(label);assertions.Add(label);Console.WriteLine("PASS "+label);}
  var s=ProductionV65Tests.Funded(root);int barracks=ProductionV65Tests.Staff(s,1);var home=s.World.Buildings.First(b=>b.Owner==0&&b.Type==2);
  int[] sequence={0,0,0,1,1,1,0,0,0};int food=s.World.Resources[0],rock=s.World.Resources[4],parts=s.World.Resources[8];
  foreach(int type in sequence)Check(s.QueueRecruit(type,barracks),"enqueue mixed troop "+s.TrainingCount(barracks,type));
  Check(s.Recruitments.Count(j=>!j.Finished&&j.BuildingSlot==barracks)==1&&s.PendingTraining(barracks).Select(q=>q.Type).SequenceEqual(sequence.Skip(1)),"one active job and exact FIFO pending sequence");
  Check(s.World.Resources[0]==food-sequence.Sum(t=>s.TrainingRule(t).Food)&&s.World.Resources[4]==rock-sequence.Sum(t=>s.TrainingRule(t).Rock)&&s.World.Resources[8]==parts-sequence.Sum(t=>s.TrainingRule(t).Parts),"all resource costs paid on every enqueue");
  string json=s.Serialize();var copy=ResearchV58Tests.Load(root);copy.Restore(json);Check(copy.Serialize()==json,"mixed paid queue roundtrip save");
  Check(s.CancelQueuedRecruit(0,barracks)&&s.PendingTraining(barracks).Select(q=>q.Type).SequenceEqual(new[]{0,0,1,1,1,0,0}),"cancel removes last pending soldier without reordering");
  Check(s.World.Resources[0]==food-sequence.Sum(t=>s.TrainingRule(t).Food)+s.TrainingRule(0).Food,"cancellation refunds once");
  while(s.CancelQueuedRecruit(0,barracks)){}while(s.CancelQueuedRecruit(1,barracks)){}
  Check(!s.CancelQueuedRecruit(0,barracks)&&s.Recruitments.Any(j=>!j.Finished&&j.Type==0),"active training cannot be cancelled or refunded");
  Check(s.World.Resources[0]==food-s.TrainingRule(0).Food&&s.World.Resources[4]==rock-s.TrainingRule(0).Rock&&s.World.Resources[8]==parts-s.TrainingRule(0).Parts,"all pending costs returned exactly");
  json=s.Serialize();copy.Restore(json);Check(copy.Serialize()==json,"refund ledger roundtrip");
  foreach(int type in sequence.Skip(1))s.QueueRecruit(type,barracks);
  for(int index=0;index<sequence.Length;index++){
   for(int i=0;i<10000&&s.Recruitments.Count(j=>j.BuildingSlot==barracks&&j.Finished)<index+1;i++)s.Step();
   var done=s.Recruitments.Where(j=>j.BuildingSlot==barracks&&j.Finished).ToArray();Check(done.Length==index+1&&done[index].Type==sequence[index],"actual training completes FIFO position "+index);
   if(index==sequence.Length-1)break;
   // Supply the next colonist using normal entrance/reconversion commands.
   var colonist=s.ReleaseResident(home.Slot);
   if(colonist==null){var previous=s.World.Units.First(a=>a.Slot==done[index].SettlerSlot);Check(s.EnterBuilding(previous,home.Slot),"return soldier to sanctuary");for(int i=0;i<10000&&s.ResidentTrips.Count>0;i++)s.Step();colonist=s.ReleaseResident(home.Slot);}
   Check(colonist!=null&&s.EnterBuilding(colonist,barracks),"new colonist supplies queued producer");for(int i=0;i<10000&&s.ResidentTrips.Count>0;i++)s.Step();
  }
  Check(!s.PendingTraining(barracks).Any(),"mixed sequence fully drains");
  // Independent producers and worker cancellation, including no-colonist waiting.
  s=ProductionV65Tests.Funded(root);barracks=ProductionV65Tests.Staff(s,1);home=s.World.Buildings.First(b=>b.Owner==0&&b.Type==2);
  Check(s.QueueRecruit(5,home.Slot)&&s.QueueRecruit(7,home.Slot)&&s.QueueRecruit(6,home.Slot)&&s.QueueRecruit(0,barracks),"workers and separate barracks queue together");
  Check(s.Recruitments.Count(j=>!j.Finished)==2,"two buildings train independently");
  Check(s.CancelQueuedRecruit(6,home.Slot)&&s.PendingTraining(home.Slot).Single().Type==7,"worker right cancellation preserves builder position");
  int occupied=s.World.Resources[0];Check(!s.QueueRecruit(0,home.Slot)&&s.World.Resources[0]==occupied,"wrong producer cannot charge");
  json=s.Serialize();var bad=JsonNode.Parse(json)!;bad["TrainingQueue"]![0]!["Food"]=999;try{copy.Restore(bad.ToJsonString());throw new Exception("forged price accepted");}catch(InvalidDataException){Check(true,"forged queue costs rejected");}
  copy=ResearchV58Tests.Load(root);copy.Restore(json);for(int i=0;i<300;i++){s.Step();copy.Step();}Check(s.Serialize()==copy.Serialize(),"save continuation deterministic with queues");
  var empty=ProductionV65Tests.Funded(root);int producer=ProductionV65Tests.Staff(empty,1);empty.QueueRecruit(0,producer);empty.QueueRecruit(1,producer);var job=empty.Recruitments.Single(j=>!j.Finished);for(int i=0;i<10000&&!job.Finished;i++)empty.Step();for(int i=0;i<60;i++)empty.Step();Check(empty.PendingTraining(producer).Count()==1&&!empty.Recruitments.Any(j=>!j.Finished),"paid order waits without colonist and is preserved");
  int pendingCost=empty.TrainingRule(1).Food;int old=empty.World.Resources[0];var target=empty.World.Buildings.First(b=>b.Slot==producer);typeof(Simulation).GetMethod("HitStructure",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(empty,new object[]{target,empty.BuildingMaxHealth(target),-1});empty.Step();Check(!empty.PendingTraining(producer).Any()&&empty.World.Resources[0]>=old+pendingCost,"destroyed producer refunds pending orders");
  s=ProductionV65Tests.Funded(root);producer=ProductionV65Tests.Staff(s,1);s.QueueRecruit(0,producer);s.QueueRecruit(1,producer);job=s.Recruitments.Single(j=>!j.Finished);s.World.Units.First(a=>a.Slot==job.SettlerSlot).Health=0;
  var replacement=s.ReleaseResident(s.World.Buildings.First(b=>b.Owner==0&&b.Type==2).Slot)!;Check(s.EnterBuilding(replacement,producer),"replacement colonist can enter after trainee dies");for(int i=0;i<10000&&s.ResidentTrips.Count>0;i++)s.Step();s.Step();Check(s.Recruitments.Any(j=>j.Type==1&&!j.Finished&&j.SettlerSlot==replacement.Slot),"dead trainee does not permanently block queue");json=s.Serialize();copy=ResearchV58Tests.Load(root);copy.Restore(json);Check(copy.Serialize()==json,"dead trainee history and replacement preserve resource ledger");
  s=ProductionV65Tests.Funded(root);int workshop=ProductionV65Tests.Staff(s,5);s.QueueRecruit(11,workshop);food=s.World.Resources[0];rock=s.World.Resources[4];parts=s.World.Resources[8];Check(s.QueueRecruit(13,workshop)&&s.World.Resources[8]==parts-s.TrainingRule(13).Parts,"vehicle enqueue immediately pays gears");Check(s.CancelQueuedRecruit(13,workshop)&&s.World.Resources[0]==food&&s.World.Resources[4]==rock&&s.World.Resources[8]==parts,"vehicle cancellation fully restores all three resource balances");
  s.QueueRecruit(13,workshop);string isolated=Path.ChangeExtension(output,"save.json");s.SaveTo(isolated);copy=ResearchV58Tests.Load(root);copy.Restore(File.ReadAllText(isolated));Check(s.Serialize()==copy.Serialize(),"paid queue survives actual isolated disk save and reopen");
  s.World.Resources[0]=0;json=s.Serialize();Check(!s.QueueRecruit(13,workshop)&&s.Serialize()==json,"insufficient food rejects without adding or charging");
  File.WriteAllText(output,JsonSerializer.Serialize(new{Passed=true,Checks=assertions.Count,Assertions=assertions,Fixture="Resource/technology grants only in isolated queue fixtures; FIFO exercised with ordinary training and colonist entrance commands."},new JsonSerializerOptions{WriteIndented=true}));
 }
}
