namespace RisingLands;
public sealed class TrainingOrder {
 public int Building{get;set;}public int Type{get;set;}public int Food{get;set;}public int Rock{get;set;}public int Parts{get;set;}
}
public sealed partial class Simulation {
 public List<TrainingOrder> TrainingQueue{get;private set;}=new();
 public IEnumerable<TrainingOrder> PendingTraining(int building)=>TrainingQueue.Where(q=>q.Building==building);
 public int TrainingCount(int building,int type)=>PendingTraining(building).Count(q=>q.Type==type)+Recruitments.Count(j=>j.BuildingSlot==building&&j.Type==type&&!j.Finished&&World.Units.Any(a=>a.Slot==j.SettlerSlot&&a.Health>0));
 public bool CanQueueRecruit(int type,int building){
  var b=World.Buildings.FirstOrDefault(b=>b.Slot==building&&b.Owner==0&&Produces(b.Type,type));
  if(b==null||!Inhabited(b)||!UnitUnlocked(type)||!RecruitData.Rules.Any(r=>r.Type==type)||TrainingQueue.Count>=1000)return false;
  var r=TrainingRule(type);return World.Resources[0]>=r.Food&&World.Resources[4]>=r.Rock&&World.Resources[8]>=r.Parts;
 }
 public bool QueueRecruit(int type,int building){
  if(!CanQueueRecruit(type,building))return false;var r=TrainingRule(type);
  World.Resources[0]-=(ushort)r.Food;World.Resources[4]-=(ushort)r.Rock;World.Resources[8]-=(ushort)r.Parts;
  TrainingQueue.Add(new(){Building=building,Type=type,Food=r.Food,Rock=r.Rock,Parts=r.Parts});StartTrainingQueues();return true;
 }
 bool RefundTraining(TrainingOrder q){
  // Preserve the paid order when the native ushort stock counter cannot hold a full refund.
  if(World.Resources[0]+q.Food>ushort.MaxValue||World.Resources[4]+q.Rock>ushort.MaxValue||World.Resources[8]+q.Parts>ushort.MaxValue)return false;
  World.Resources[0]+=(ushort)q.Food;World.Resources[4]+=(ushort)q.Rock;World.Resources[8]+=(ushort)q.Parts;TrainingQueue.Remove(q);return true;
 }
 public bool CancelQueuedRecruit(int type,int building){var q=TrainingQueue.LastOrDefault(q=>q.Type==type&&q.Building==building);return q!=null&&RefundTraining(q);}
 void StartTrainingQueues(){
  foreach(int building in TrainingQueue.Select(q=>q.Building).Distinct().ToArray()){
   var b=World.Buildings.FirstOrDefault(b=>b.Owner==0&&b.Slot==building);
   if(b==null){foreach(var lost in PendingTraining(building).ToArray())RefundTraining(lost);continue;}
   if(Recruitments.Any(j=>j.BuildingSlot==building&&!j.Finished&&World.Units.Any(a=>a.Slot==j.SettlerSlot&&a.Health>0))||!Inhabited(b))continue;
   var q=PendingTraining(building).First();if(!UnitUnlocked(q.Type)||!Produces(b.Type,q.Type))continue;
   int slot=World.BuildingResidents.GetValueOrDefault(building,new()).FirstOrDefault(id=>World.Units.Any(a=>a.Slot==id&&a.Owner==0&&a.Type==9&&a.Health>0&&a.LegacyState==14)&&!ResidentReserved(id),-1);if(slot<0)continue;
   // Cost was paid on enqueue. Transferring to the recruitment ledger does not debit again.
   Recruitments.Add(new(){Type=q.Type,BuildingSlot=building,SettlerSlot=slot,SourceBuildingSlot=q.Type is 5 or 6 or 7?-1:building,TravelCell=q.Type is 5 or 6 or 7?-1:b.Cell,Arrived=true});TrainingQueue.Remove(q);
  }
 }
 void ValidateTrainingQueue(Save s){
  if(s.Version>=45&&s.TrainingQueue==null||s.TrainingQueue?.Count>1000)throw new InvalidDataException("Invalid training queue");
  var buildings=InitialBuildings.Concat(s.Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).ToDictionary(b=>b.Slot);
  foreach(var q in s.TrainingQueue??new()){
   if(!buildings.TryGetValue(q.Building,out var b)||b.Owner!=0||!Produces(b.Type,q.Type)||!RecruitData.Rules.Any(r=>r.Type==q.Type))throw new InvalidDataException("Invalid queued producer");
   var r=TrainingRule(q.Type);if(q.Food!=r.Food||q.Rock!=r.Rock||q.Parts!=r.Parts)throw new InvalidDataException("Invalid paid training cost");
   int Index(int branch)=>branch switch{0=>s.Research!.Agriculture!.Index,1=>s.Research!.Military!.Index,2=>s.Research!.Index,_=>s.Research!.Spiritual!.Index};
   if(!UnlockData.Technologies.Any(t=>t.Index<Index(t.Branch)&&ProductionUnlocks.GetValueOrDefault(t.Key,Array.Empty<int>()).Contains(q.Type)))throw new InvalidDataException("Locked queued troop");
  }
 }
}
