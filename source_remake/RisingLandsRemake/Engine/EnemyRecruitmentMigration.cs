namespace RisingLands;
public sealed partial class Simulation {
 bool RetireLockedEnemyRecruitment(Recruitment job){
  if(job.Owner is not (>0 and <4)||OpponentUnitUnlocked(job.Owner,job.Type))return false;
  var actor=World.Units.FirstOrDefault(a=>a.Slot==job.SettlerSlot&&a.Owner==job.Owner);
  if(actor==null)return false;
  // Preserve the paid cost/history and save identities. A pending invalid order
  // must also stop reserving a production building, without refunding free stock.
  if(!job.Finished){var rule=TrainingRule(job.Type);job.Finished=true;job.Progress=rule.Duration;actor.Type=job.Type;actor.LegacyState=7;actor.AnimationCount=rule.AnimationCount;actor.AnimationDelay=rule.AnimationDelay;actor.AnimationFrame=actor.AnimationCounter=0;}
  if(actor.Type!=job.Type)return false;
  actor.Health=0;ClearOrders(actor);CancelFeeding(actor);Opponent.Sorties.Remove(actor.Slot);Opponent.Sieges.Remove(actor.Slot);return true;
 }
 void RetireHistoricalLockedEnemyRecruitments(){
  foreach(var group in Recruitments.GroupBy(j=>j.SettlerSlot)){
   if(Reversions.GetValueOrDefault(group.Key)>=group.Count()&&Reversions.ContainsKey(group.Key))continue;
   RetireLockedEnemyRecruitment(group.Last());
  }
 }
}
