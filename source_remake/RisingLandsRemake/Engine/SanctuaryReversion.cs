namespace RisingLands;
public sealed partial class Simulation {
 public Dictionary<int,int> Reversions{get;private set;}=new();
 Actor ColonistTemplate(int slot){int owner=InitialActors[slot].Owner;var seed=OriginalActorSeeds.Values.First(a=>a.Type==9&&a.Owner==owner);return new(){Slot=slot,Type=9,Owner=owner,LegacyState=14,Health=seed.Health,AnimationCount=seed.AnimationCount,AnimationDelay=seed.AnimationDelay};}
 bool DepositBeforeReversion(Actor actor,Building building){
  int kind=actor.Rock>0?2:actor.CargoKind,amount=actor.Rock>0?actor.Rock:actor.Food;if(amount==0)return true;
  var cells=Enumerable.Range(-5,11).SelectMany(dy=>Enumerable.Range(-5,11).Select(dx=>(x:(building.Cell&255)+dx,y:(building.Cell>>8)+dy))).Where(p=>p.x>=0&&p.y>=0&&p.x<World.Width&&p.y<World.Height).Select(p=>p.y*256+p.x).Where(c=>Passable(c&255,c>>8)&&BuildingAt(c)==null&&MineAt(c)<0&&(World.Objects[c]==0||World.Objects[c]==66+kind&&World.Quantities[c]<31)).OrderBy(c=>Math.Abs((c&255)-actor.X/1024)+Math.Abs((c>>8)-actor.Y/1024)).ToArray();
  if(cells.Sum(c=>World.Objects[c]==0?31:31-World.Quantities[c])<amount)return false;
  foreach(int cell in cells){int existing=World.Objects[cell]==66+kind?World.Quantities[cell]:0;int drop=Math.Min(amount,31-existing);World.Objects[cell]=(byte)(66+kind);World.Quantities[cell]=(byte)(existing+drop);amount-=drop;if(amount==0)break;}
  actor.Rock=actor.Food=0;return true;
 }
 bool RevertAtSanctuary(Actor actor,Building building){if(actor.Type==9)return true;if(!DepositBeforeReversion(actor,building))return false;var template=ColonistTemplate(actor.Slot);Reversions[actor.Slot]=Recruitments.Count(j=>j.SettlerSlot==actor.Slot);actor.OverflowCell=-1;actor.PersistentCollection=actor.FoodPlantsOnly=false;actor.FoodField=actor.FoodSource=actor.FoodSourceBuilding=actor.DeliveryCell=actor.FoodDestination=actor.RockDestination=actor.RockGround=actor.MineIndex=actor.MineWorkCell=-1;actor.FoodDestinationKind=actor.FoodJob=actor.MineJob=actor.WorkTicks=actor.MineTicks=0;actor.RepeatFood=actor.RepeatMine=false;actor.Mana=0;actor.RepairTarget=-1;actor.PetrifiedUntil=actor.ArmoredUntil=actor.CastAnimationUntil=0;actor.Type=9;actor.Health=Math.Min(actor.Health,template.Health);actor.AnimationCount=template.AnimationCount;actor.AnimationDelay=template.AnimationDelay;actor.AnimationFrame=actor.AnimationCounter=0;return true;}
 void ValidateReversions(Save s){var records=s.Reversions??new();foreach(var pair in records)if(!InitialActors.TryGetValue(pair.Key,out var seed)||ExpectedTradeOwner(pair.Key,s.Special) is <0 or >3||pair.Value<0||pair.Value>s.Recruitments.Count(j=>j.SettlerSlot==pair.Key)||!s.Units.Any(u=>u.Slot==pair.Key))throw new InvalidDataException("Invalid sanctuary reversion");}
}
