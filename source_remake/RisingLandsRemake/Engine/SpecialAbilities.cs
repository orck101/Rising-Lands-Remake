using System.Text.Json;
namespace RisingLands;
public record SpellRule(int Id,string Name,int Radius,int Duration,int Learning,int Rock,int Food,int Parts,int Mana,int Range,int Sprite);
public record SpecialCatalog(List<SpellRule> Spells,Dictionary<string,int[]> SpellUnlocks,int[] Aerial,int[] Army,int[] ResourceValues,int BaseMana,int CaptainBonus);
public record AdvancedAttackRule(int Type,int ReleaseTicks,int Projectile,int Phase,int FrameCount,int FrameDelay);
public record AdvancedAttackCatalog(List<AdvancedAttackRule> Rules,List<UnitVisual> Types);
public sealed class SpellLearning {public int Owner{get;set;}public int Building{get;set;}public int Spell{get;set;}public int Progress{get;set;}=1;public bool Finished{get;set;}}
public sealed class SpellZone {public int Cell{get;set;}public int Spell{get;set;}public int Owner{get;set;}public long Until{get;set;}}
public sealed class MirrorSeed {public int Source{get;set;}public Actor Seed{get;set;}=new();}
public sealed class WallPlan {public int Builder{get;set;}public List<int> Sites{get;set;}=new();}
public sealed class SpecialState {
 public List<ExchangeResident> ExchangeResidents{get;set;}=new();public Dictionary<int,long> ExchangeEntering{get;set;}=new();
 public List<Negotiation> Negotiations{get;set;}=new();public List<UnitExchange> UnitExchanges{get;set;}=new();
 public List<NativeGateControl> NativeGates{get;set;}=new();
 public List<WallPlan> WallPlans{get;set;}=new();
 public List<SpellLearning> Learning{get;set;}=new();public List<SpellZone> Zones{get;set;}=new();public List<MirrorSeed> Mirrors{get;set;}=new();public List<TradeTransaction> Trades{get;set;}=new();public List<TradeContact> Contacts{get;set;}=new();
}
public sealed partial class Simulation {
 static readonly JsonSerializerOptions SpecialOptions=new(){PropertyNameCaseInsensitive=true,PropertyNamingPolicy=JsonNamingPolicy.SnakeCaseLower};
 public static readonly SpecialCatalog SpecialData=JsonSerializer.Deserialize<SpecialCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/special_rules.json")),SpecialOptions)!;
 public static readonly AdvancedAttackCatalog AdvancedAttacks=JsonSerializer.Deserialize<AdvancedAttackCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/advanced_attack.json")),SpecialOptions)!;
 public SpecialState Special{get;private set;}=new();
 void CancelSpecialOrder(Actor a){CancelExchangeEntry(a);a.RepairTarget=-1;Special.WallPlans.RemoveAll(p=>p.Builder==a.Slot);Special.Contacts.RemoveAll(c=>c.Messenger==a.Slot&&!c.Arrived);}
 public static bool Airborne(Actor u)=>SpecialData.Aerial.Contains(u.Type);
 public SpellRule Spell(int id)=>SpecialData.Spells.Single(s=>s.Id==id);
 bool SupportedSpiritualResearch(Invention t)=>t.Effect==0&&t.Param1==4&&SpecialData.SpellUnlocks.GetValueOrDefault(t.Key,Array.Empty<int>()).Length>0||t.Effect==1&&t.Param2==4&&t.Param1 is 9 or 10 or 12||t.Effect==0&&t.Param1==3&&t.Buildings.Length>0&&t.Buildings.All(type=>BuildData.Buildings.Any(b=>b.Type==type&&b.Objects.Count>0));
 public int SpiritualDamageBonus(Actor a)=>a.Owner==0?UnlockData.Technologies.Where(t=>TechnologyApplied(t)&&t.Effect==1&&t.Param1==10&&t.Param2==a.Type).Sum(t=>t.Param3):0;
 public bool SpellUnlocked(int id)=>UnlockData.Technologies.Any(t=>TechnologyApplied(t)&&SpecialData.SpellUnlocks.GetValueOrDefault(t.Key,Array.Empty<int>()).Contains(id));
 public bool SpellLearned(int id,int owner=0)=>Special.Learning.Any(j=>j.Owner==owner&&j.Spell==id&&j.Finished);
 public int ManaMaximum(Actor u)=>u.Type==4?SpecialData.BaseMana+(u.Owner==0?UnlockData.Technologies.Where(t=>TechnologyApplied(t)&&t.Effect==1&&t.Param1==12&&t.Param2==4).Sum(t=>t.Param3):0):0;
 public int ManaPeriod(Actor a){int bonus=a.Owner==0?UnlockData.Technologies.Where(t=>TechnologyApplied(t)&&t.Effect==1&&t.Param1==9&&t.Param2==4).Sum(t=>t.Param3):0;return bonus>0?Math.Max(1,600/bonus):60;}
 public bool CanLearnSpell(int id,int slot){var b=World.Buildings.FirstOrDefault(b=>b.Slot==slot);var r=Spell(id);return b is {Type:6,Owner:0}&&Inhabited(b)&&SpellUnlocked(id)&&!SpellLearned(id)&&!Special.Learning.Any(j=>j.Owner==0&&!j.Finished&&(j.Spell==id||j.Building==slot))&&World.Resources[0]>=r.Food&&World.Resources[4]>=r.Rock&&World.Resources[8]>=r.Parts;}
 public bool LearnSpell(int id,int slot){if(!CanLearnSpell(id,slot))return false;var r=Spell(id);World.Resources[0]-=(ushort)r.Food;World.Resources[4]-=(ushort)r.Rock;World.Resources[8]-=(ushort)r.Parts;Special.Learning.Add(new(){Spell=id,Building=slot});return true;}
 public bool CanCastSpell(Actor caster,int id,int cell,Actor? target=null){
  if(id is <0 or >4||caster.Type!=4||caster.Owner is <0 or >3||!World.Units.Contains(caster)||!VisualCatalog.Visible(caster)||caster.PetrifiedUntil>Tick||!SpellLearned(id,caster.Owner)||caster.Mana<Spell(id).Mana||!ValidMapCell(cell)||(caster.Owner==0?!InSight(cell):OpponentVision(caster.Owner)[cell]==0))return false;
  if(!ProjectileMath.WithinRange((cell&255)-caster.X/1024,(cell>>8)-caster.Y/1024,Spell(id).Range*Spell(id).Range))return false;
  if(id is 0 or 4)return true;
  if(target==null||!World.Units.Contains(target)||!VisualCatalog.Visible(target)||target.Y/1024*256+target.X/1024!=cell)return false;
  return id==1?(caster.Owner==0?target.Owner!=caster.Owner:Hostile(caster.Owner,target.Owner)):target.Owner==caster.Owner&&(id!=2||target.Type is not (5 or 6 or 7 or 9 or 10 or 13 or 17)&&!Special.Mirrors.Any(m=>m.Seed.Slot==target.Slot)&&Special.Mirrors.Count<1000&&World.Units.Count(a=>a.Health>0)<NativeEntityCapacity&&MirrorDestination(target)>=0);
 }
 int MirrorDestination(Actor target){for(int radius=1;radius<=3;radius++)for(int y=-radius;y<=radius;y++)for(int x=-radius;x<=radius;x++){if(Math.Max(Math.Abs(x),Math.Abs(y))!=radius)continue;int nx=target.X/1024+x,ny=target.Y/1024+y;if(CanTraverse(target,nx,ny)&&!World.Units.Any(a=>VisualCatalog.Visible(a)&&a.X/1024==nx&&a.Y/1024==ny))return ny*256+nx;}return -1;}
 public bool CastSpell(Actor caster,int id,int cell,Actor? target=null){
  if(!CanCastSpell(caster,id,cell,target))return false;Stop(caster);var rule=Spell(id);caster.Mana-=rule.Mana;caster.CastAnimationUntil=Tick+Math.Max(1,AdvancedAttacks.Rules.First(r=>r.Type==4).ReleaseTicks);
  if(id==0)Special.Zones.Add(new(){Spell=id,Cell=cell,Owner=caster.Owner,Until=Tick+rule.Duration});
  if(id==1){target!.PetrifiedUntil=Tick+rule.Duration;target.AttackWindup=0;}
  if(id==2){int dest=MirrorDestination(target!);var seed=CloneState(ExpectedActor(target!.Slot,Recruitments,Reversions));seed.Slot=40000+Special.Mirrors.Count;seed.Type=target.Type;seed.Owner=target.Owner;seed.LegacyState=7;seed.X=(dest&255)*1024;seed.Y=(dest>>8)*1024;seed.Health=target.Health;seed.Route.Clear();seed.AttackTarget=-1;seed.AttackCooldown=seed.AttackWindup=0;seed.Mana=0;seed.ArmoredUntil=seed.PetrifiedUntil=seed.CastAnimationUntil=0;Special.Mirrors.Add(new(){Source=target.Slot,Seed=CloneState(seed)});InitialActors.Add(seed.Slot,CloneState(seed));World.Units.Add(CloneState(seed));}
  if(id==3)target!.ArmoredUntil=Tick+rule.Duration;
  if(id==4){Special.Zones.Add(new(){Spell=id,Cell=cell,Owner=caster.Owner,Until=Tick+rule.Duration});foreach(var victim in World.Units.Where(a=>VisualCatalog.Visible(a)&&!Airborne(a)&&Math.Abs(a.X/1024-(cell&255))<=1&&Math.Abs(a.Y/1024-(cell>>8))<=1).ToArray())ApplyHit(caster,victim,Math.Max(0,90-(victim.ArmoredUntil>Tick?1:0)));}
  UpdateVisibility();return true;
 }
 public bool Repair(Actor drone,Actor target){if(drone.Type!=13||drone.Owner!=0||target.Owner!=drone.Owner||target.Type is <11 or >17||target.Health<=0||target.Health>=MaxHealth(target)||!VisualCatalog.Visible(drone)||!VisualCatalog.Visible(target))return false;Stop(drone);drone.RepairTarget=target.Slot;return true;}
 int CaptainBonus(Actor u)=>World.Units.Any(a=>a.Type==2&&a.Owner==u.Owner&&VisualCatalog.Visible(a)&&ProjectileMath.WithinRange(a.X/1024-u.X/1024,a.Y/1024-u.Y/1024,100))?SpecialData.CaptainBonus:0;
 void SpecialStep(){
  foreach(var job in Special.Learning.Where(j=>!j.Finished)){var b=World.Buildings.FirstOrDefault(b=>b.Slot==job.Building);if(b==null||!Inhabited(b))continue;job.Progress=Math.Min(Spell(job.Spell).Learning*100,job.Progress+100+Residents(b.Slot)*100/Math.Max(1,ResidentCapacity(b)));job.Finished=job.Progress==Spell(job.Spell).Learning*100;if(job.Finished)EmitBuildingSound(b,0x23);}
  Special.Zones.RemoveAll(z=>z.Until<=Tick);
  foreach(var a in World.Units.ToArray()){
   if(a.PetrifiedUntil<=Tick)a.PetrifiedUntil=0;if(a.ArmoredUntil<=Tick)a.ArmoredUntil=0;if(a.CastAnimationUntil<=Tick)a.CastAnimationUntil=0;
   if(a.Type==4&&a.Health>0&&Tick%ManaPeriod(a)==0&&World.Buildings.Any(b=>b.Type==17&&b.Owner==a.Owner&&ProjectileMath.WithinRange(a.X/1024-(b.Cell&255),a.Y/1024-(b.Cell>>8),16)))a.Mana=Math.Min(ManaMaximum(a),a.Mana+1);
   if(Special.Mirrors.Any(m=>m.Seed.Slot==a.Slot)&&a.Health>0&&Tick%Spell(2).Duration==0){a.Health--;if(a.Health==0)ClearOrders(a);}
   if(a.RepairTarget<0||a.Health<=0||a.PetrifiedUntil>Tick)continue;var t=World.Units.FirstOrDefault(t=>t.Slot==a.RepairTarget);if(t==null||t.Owner!=a.Owner||t.Health<=0||t.Health>=MaxHealth(t)){a.RepairTarget=-1;continue;}
   if(!MeleeReach(a,t)){if(a.Route.Count==0&&Tick%20==0)Order(a,t.X/1024,t.Y/1024,true);continue;}a.Route.Clear();if(Tick%20==0)t.Health=Math.Min(MaxHealth(t),t.Health+2);
  }
  TradeContactStep();ExchangeStorageStep();NegotiationStep();
 }
 void RestoreMirrorSeeds(Save s){var state=s.Special??new();if(state.Mirrors==null||state.Mirrors.Count>1000)throw new InvalidDataException("Invalid mirrors");int slot=40000;foreach(var m in state.Mirrors){if(m.Seed==null||m.Seed.Slot!=slot++||!s.Units.Any(a=>a.Slot==m.Source)||m.Seed.Type is 5 or 6 or 7 or 9 or 10 or 13 or 17||!RecruitData.Rules.Any(r=>r.Type==m.Seed.Type)||m.Seed.Health<=0||m.Seed.Health>TrainingRule(m.Seed.Type).Health||m.Seed.LegacyState!=7)throw new InvalidDataException("Invalid mirror seed");InitialActors.Add(m.Seed.Slot,CloneState(m.Seed));}}
 void ValidateSpecial(Save s){
  if(s.Version>=28&&s.Special==null)throw new InvalidDataException("Missing special state");var state=s.Special??new();if(state.Learning==null||state.Zones==null||state.Trades==null||state.Contacts==null||state.Learning.Count>20||state.Zones.Count>1000)throw new InvalidDataException("Invalid special state");
  var learned=new HashSet<(int,int)>();foreach(var j in state.Learning)if(j.Owner is <0 or >3||j.Spell is <0 or >4||!learned.Add((j.Owner,j.Spell))||!InitialBuildings.Concat(s.Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).Any(b=>b.Slot==j.Building&&b.Type==6&&b.Owner==j.Owner)||j.Progress<1||j.Progress>Spell(j.Spell).Learning*100||j.Finished!=(j.Progress==Spell(j.Spell).Learning*100))throw new InvalidDataException("Invalid spell learning");
  foreach(var z in state.Zones)if(z.Spell is not (0 or 4)||!ValidMapCell(z.Cell)||z.Owner is <0 or >3||z.Until<=s.Tick||z.Until>s.Tick+Spell(z.Spell).Duration)throw new InvalidDataException("Invalid spell zone");
  foreach(var a in s.Units)if(a.Mana<0||a.Mana>(a.Type==4?255:0)||a.PetrifiedUntil<0||a.PetrifiedUntil>s.Tick+Spell(1).Duration||a.ArmoredUntil<0||a.ArmoredUntil>s.Tick+Spell(3).Duration||a.CastAnimationUntil<0||a.CastAnimationUntil>s.Tick+32||a.RepairTarget< -1||a.RepairTarget>=0&&(a.Type!=13||!s.Units.Any(t=>t.Slot==a.RepairTarget&&t.Owner==a.Owner&&t.Type is >=11 and <=17)))throw new InvalidDataException("Invalid special actor");
  ValidateTrades(s,state);ValidateNegotiations(s,state);
 }
}
