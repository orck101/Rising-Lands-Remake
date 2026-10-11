namespace RisingLands;
public sealed partial class Simulation {
 bool DefenseAvailable(Actor a)=>a.Owner is >=0 and <=3&&FightingUnit(a)&&!TradeDeliveryPending(a)&&a.PetrifiedUntil<=Tick&&!Feeding(a)&&a.FoodJob==0&&a.MineJob==0&&!ResidentTrips.Any(t=>t.Slot==a.Slot)&&!Sieging(a)&&!Constructions.Any(c=>!Complete(c)&&HasBuilder(c,a.Slot));
 bool DefenseVisible(int owner,Actor target,Dictionary<int,byte[]> visions)=>owner==0?UnitInSight(target):visions.TryGetValue(owner,out var vision)&&vision[target.Y/1024*256+target.X/1024]!=0;
 void AutomaticDefenseStep(){
  var soldiers=World.Units.Where(a=>DefenseAvailable(a)&&!BaseDefenseControls(a)).ToArray();var visions=soldiers.Where(a=>a.Owner>0).Select(a=>a.Owner).Distinct().ToDictionary(o=>o,OpponentVision);
  var threats=new Dictionary<int,List<Actor>>();void Threat(int attacker,Actor friend){if(attacker<0)return;if(!threats.TryGetValue(attacker,out var friends))threats[attacker]=friends=new();friends.Add(friend);}
  foreach(var friend in World.Units.Where(a=>a.Health>0)){if(Tick-friend.LastHitTick<=150)Threat(friend.LastAttacker,friend);}
  var actors=World.Units.ToDictionary(a=>a.Slot);foreach(var enemy in World.Units.Where(a=>a.Health>0&&a.AttackTarget>=0))if(actors.TryGetValue(enemy.AttackTarget,out var victim))Threat(enemy.Slot,victim);
  var contacts=soldiers.Select(a=>a.Owner).Distinct().ToDictionary(owner=>owner,owner=>World.Units.Where(b=>b.Health>0&&b.Type!=10&&Hostile(owner,b.Owner)&&VisualCatalog.Visible(b)&&DefenseVisible(owner,b,visions)).ToArray());
  foreach(var a in soldiers){
   if(a.AutomaticDefense){var current=World.Units.FirstOrDefault(b=>b.Slot==a.AttackTarget);int radius=Math.Max(DefensePerception(a),VisibilityData.Units.FirstOrDefault(r=>r.Type==a.Type)?.Radius??5);
    if(current==null||!CanAttack(a,current)||!Hostile(a.Owner,current.Owner)||!DefenseVisible(a.Owner,current,visions)||a.DefenseAnchor<0||Math.Abs(current.X/1024-(a.DefenseAnchor&255))>radius+2||Math.Abs(current.Y/1024-(a.DefenseAnchor>>8))>radius+2){a.AttackTarget=-1;a.AttackWindup=0;a.Route.Clear();a.AutomaticDefense=false;a.DefenseAnchor=-1;}
   }
   if(a.AttackTarget>=0||a.Route.Count>0||Tick<a.DefenseAcquireAfter)continue;
   int sight=VisibilityData.Units.FirstOrDefault(r=>r.Type==a.Type)?.Radius??5;
   int perception=DefensePerception(a);
   bool Threatens(Actor b)=>a.LastAttacker==b.Slot&&Tick-a.LastHitTick<=150||threats.TryGetValue(b.Slot,out var friends)&&friends.Any(friend=>friend.Owner==a.Owner||!Hostile(a.Owner,friend.Owner));
   var targets=contacts[a.Owner].Where(b=>CanAttack(a,b)&&TileDistanceSquared(a,b)<=(Threatens(b)?(long)perception*perception:(long)sight*sight)&&(!(a.Owner>0&&OpponentEnabled&&!OpponentDeployed(a))||Threatens(b)));
   var target=targets.OrderBy(b=>Threatens(b)?0:FightingUnit(b)?1:2).ThenBy(b=>TileDistanceSquared(a,b)).ThenBy(b=>b.Slot).FirstOrDefault();if(target==null)continue;
   a.AttackTarget=target.Slot;a.AutomaticDefense=true;a.DefenseAnchor=a.Y/1024*256+a.X/1024;a.CombatRetryAt=0;
  }
 }
 bool CombatApproach(Actor a,Actor target){
  if(Tick<a.CombatRetryAt)return false;a.CombatRetryAt=Tick+16;
  int range=Math.Clamp(CombatRule(a.Type).Range,1,16),cx=target.X/1024,cy=target.Y/1024;
  var candidates=NearbyCells(cy*256+cx,range).Where(cell=>cell!=cy*256+cx&&ProjectileMath.WithinRange((cell&255)-cx,(cell>>8)-cy,range*range)&&CanTraverse(a,cell&255,cell>>8)&&!World.Units.Any(u=>u!=a&&VisualCatalog.Visible(u)&&!Airborne(u)&&u.X/1024==(cell&255)&&u.Y/1024==(cell>>8))).OrderBy(cell=>Math.Abs((cell&255)-a.X/1024)+Math.Abs((cell>>8)-a.Y/1024)).ThenBy(cell=>cell);
  foreach(int cell in candidates.Take(8))if(Order(a,cell&255,cell>>8,true))return true;if(BaseDefenseControls(a)){var guard=BaseDefenseFor(a.Owner).Guards[a.Slot];guard.BlockedTarget=target.Slot;guard.BlockedUntil=Tick+300;guard.Target=-1;a.AttackTarget=-1;a.AutomaticDefense=false;}return false;
 }
}
