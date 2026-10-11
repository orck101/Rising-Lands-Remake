namespace RisingLands;
public sealed partial class Simulation {
 public bool TradeEntering(Actor actor)=>Special.ExchangeEntering.ContainsKey(actor.Slot)||Special.Negotiations.Any(n=>n.Active&&n.Entering.ContainsKey(actor.Slot));
 bool WalkIntoTradeBuilding(Negotiation n,Actor actor,Building b,int cell){
  return WalkIntoTradeBuilding(n.Entering,actor,b,cell);
 }
 bool WalkIntoTradeBuilding(Dictionary<int,long> entering,Actor actor,Building b,int cell){
  if(actor.PetrifiedUntil>Tick||Feeding(actor)||actor.Route.Count>0)return false;
  if(!entering.ContainsKey(actor.Slot)){
   if(!ReachBuildingInteraction(actor,b,cell))return false;
   entering[actor.Slot]=Tick;return false;
  }
  // Native CalculEntreBatiment selects an entry cell; AffEngin uses walking
  // frames for state 21 and draws nothing for state 12. The final 24px approach
  // aligns the existing calibrated front foot point to the door artwork.
  var door=DoorOrigin(b);int x=door.X,y=Math.Max(0,door.Y-24*32);
  int dx=Math.Sign(x-actor.X),dy=Math.Sign(y-actor.Y);
  actor.Direction=dx<0?(dy<0?3:dy>0?15:1):dx>0?(dy<0?7:dy>0?11:9):dy<0?5:13;
  actor.X+=Math.Clamp(x-actor.X,-LabStep,LabStep);actor.Y+=Math.Clamp(y-actor.Y,-LabStep,LabStep);
  actor.AnimationCounter++;if(actor.AnimationCounter>=actor.AnimationDelay){actor.AnimationCounter=0;actor.AnimationFrame=(actor.AnimationFrame+1)%Math.Max(1,actor.AnimationCount);}
  if(actor.X!=x||actor.Y!=y)return false;
  entering.Remove(actor.Slot);return true;
 }
}
