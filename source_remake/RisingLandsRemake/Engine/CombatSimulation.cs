using System.Text.Json;
namespace RisingLands;

public record CombatRule(int Type,string Name,int[] Damage,int Armor,int Range,int Reload);
public record ReleaseRule(int Type,int ReleaseTicks);
public record ReleaseCatalog(List<ReleaseRule> Rules);
public record CombatCatalog(List<CombatRule> Rules);
public static class CombatMath {
    // EnginInfligeDegat: signed byte intermediate, negative -> 1, zero stays zero.
    public static int Damage(int basis,int attack,int bonus,int captain,int armor,int defense,int flag=0){int d=unchecked((sbyte)(basis+attack+bonus+captain-armor-defense));if(d<0)d=1;return unchecked((sbyte)(d-flag));}
}
public sealed partial class Simulation {
    public static readonly CombatCatalog CombatData=JsonSerializer.Deserialize<CombatCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/combat_rules.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
    public static readonly ReleaseCatalog Releases=JsonSerializer.Deserialize<ReleaseCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/attack_release.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true,PropertyNamingPolicy=JsonNamingPolicy.SnakeCaseLower})!;
    public int ReleaseTicks(Actor actor)=>AttackRuleFor(actor)!.ReleaseTicks;
    public CombatRule CombatRule(int type)=>CombatData.Rules.Single(r=>r.Type==type);
    public int CombatCycle(Actor actor)=>Math.Max(Math.Max(1,CombatRule(actor.Type).Reload),AttackRuleFor(actor)?.ReleaseTicks??1);
    public int MaxHealth(Actor actor)=>ExpectedActor(actor.Slot,Recruitments,Reversions).Health;
    // Combat follows native release moments and damage tables for supported troops.
    public bool CanAttack(Actor a,Actor b)=>World.Units.Contains(a)&&World.Units.Contains(b)&&a!=b&&(!OpponentEnabled||b.Type!=10)&&SupportedFighter(a)&&b.Type is >=0 and <=26&&b.Type!=17&&CombatRule(a.Type).Damage[b.Type]>0&&a.Owner!=b.Owner&&CombatOwner(a.Owner)&&CombatOwner(b.Owner)&&VisualCatalog.Visible(a)&&VisualCatalog.Visible(b);
    public bool Attack(Actor a,Actor b){if(a.Owner!=0||!CanAttack(a,b))return false;RegisterAllianceAttack(a.Owner,b.Owner);CancelFeeding(a);a.DefenseAcquireAfter=0;a.AutomaticDefense=false;a.DefenseAnchor=-1;a.CombatRetryAt=0;a.AttackWindup=0;a.Route.Clear();a.FoodJob=0;a.WorkTicks=0;a.AttackTarget=b.Slot;return true;}
    public static bool MeleeReach(Actor a,Actor b)=>Math.Abs(a.X-b.X)<=1024&&Math.Abs(a.Y-b.Y)<=1024;
    void ClearOrders(Actor u){u.AutomaticDefense=false;u.DefenseAnchor=-1;u.CombatRetryAt=0;Opponent.Sieges.Remove(u.Slot);Opponent.Repairs.Remove(u.Slot);CancelSpecialOrder(u);u.PersistentCollection=false;u.OverflowCell=-1;u.MineJob=0;u.MineTicks=0;CancelResearchTrip(u);u.AttackWindup=0;u.Route.Clear();u.FoodJob=0;u.WorkTicks=0;u.AttackTarget=-1;DetachBuilder(u.Slot);}
    void CombatStep(){ProjectileStep();foreach(var a in World.Units.OrderBy(u=>u.Slot)){
        if(a.HitFlash>0)a.HitFlash--;
        if(a.Health<=0){ClearOrders(a);a.AttackCooldown=0;continue;}
        if(a.AttackCooldown>0)a.AttackCooldown--;
        if(a.PetrifiedUntil>Tick||Feeding(a))continue;if(a.AttackTarget<0)continue;var b=World.Units.FirstOrDefault(u=>u.Slot==a.AttackTarget);
        if(b==null||!CanAttack(a,b)||OpponentEnabled&&(!Hostile(a.Owner,b.Owner)||a.Owner is >0 and <4&&OpponentVision(a.Owner)[ActorCell(b)]==0)){a.AttackWindup=0;a.AttackTarget=-1;a.AutomaticDefense=false;a.DefenseAnchor=-1;a.Route.Clear();continue;}
        if(!InAttackRange(a,b)){a.AttackWindup=0;
            // Replan at tile boundaries only. Route/clock are laboratory behavior.
            if(a.Route.Count==0||Tick%16==0)CombatApproach(a,b);
            continue;
        }
        a.Route.Clear();int vx=b.X-a.X,vy=b.Y-a.Y;a.Direction=Math.Abs(vx)>=Math.Abs(vy)?vx<0?1:9:vy<0?5:13;
        if(a.AttackWindup==0){if(a.AttackCooldown>0)continue;a.AttackCooldown=CombatCycle(a);a.AttackWindup=ReleaseTicks(a);}
        if(--a.AttackWindup>0)continue;
        EmitUnitSound(a,7);int damage=CombatMath.Damage(CombatRule(a.Type).Damage[b.Type],(MilitaryDamageBonus(a)+SpiritualDamageBonus(a)),0,CaptainBonus(a),CombatRule(b.Type).Armor,0,b.ArmoredUntil>Tick?1:0);
        if(AttackRuleFor(a)!.Projectile>=0)FireArrow(a,b,damage);else ApplyHit(a,b,damage);
    }foreach(var a in World.Units.Where(a=>a.AttackTarget>=0)){var target=World.Units.FirstOrDefault(b=>b.Slot==a.AttackTarget);if(target==null||!CanAttack(a,target)||OpponentEnabled&&!Hostile(a.Owner,target.Owner)){a.AttackWindup=0;a.AttackTarget=-1;a.AutomaticDefense=false;a.DefenseAnchor=-1;a.Route.Clear();}}}
    void ValidateCombatSave(Save s){foreach(var u in s.Units){if(u.AttackWindup<0||u.AttackWindup>32||u.AttackWindup>0&&(!SupportedFighter(u)||u.AttackTarget<0||u.Route.Count!=0||u.AttackCooldown<=0||u.AttackWindup!=ReleaseTicks(u)-(CombatCycle(u)-u.AttackCooldown+1)))throw new InvalidDataException("Invalid attack windup");if(u.AttackCooldown<0||u.AttackCooldown>CombatCycle(u)||u.HitFlash<0||u.HitFlash>6||u.AttackTarget< -1)throw new InvalidDataException("Invalid combat counters");if(u.Health==0&&(u.Route.Count!=0||u.FoodJob!=0||u.AttackTarget!=-1))throw new InvalidDataException("Dead actor has active orders");if(u.AttackTarget>=0){var target=s.Units.FirstOrDefault(t=>t.Slot==u.AttackTarget);if(target==null||!SupportedFighter(u)||(target.Type<0||target.Type>26||CombatRule(u.Type).Damage[target.Type]==0)||u.Owner==target.Owner||!CombatOwner(u.Owner)||!CombatOwner(target.Owner)||u.Slot==target.Slot||!VisualCatalog.Visible(u)||!VisualCatalog.Visible(target)||u.FoodJob!=0)throw new InvalidDataException("Invalid combat target");}}}
    public static World TrainingWorld(World source,bool ranged=false){var soldier=source.Units.First(u=>u.Type==(ranged?8:0)&&u.Owner==0&&VisualCatalog.Visible(u));var probe=new Simulation(source);int cell=-1;
        for(int y=Math.Max(1,soldier.Y/1024-6);y<Math.Min(source.Height-1,soldier.Y/1024+7)&&cell<0;y++)for(int x=Math.Max(1,soldier.X/1024-6);x<Math.Min(source.Width-4,soldier.X/1024+7)&&cell<0;x++)if(Enumerable.Range(0,5).All(dx=>probe.Passable(x+dx,y)&&source.Objects[y*256+x+dx]==0&&!source.Units.Any(u=>u!=soldier&&VisualCatalog.Visible(u)&&u.X/1024==x+dx&&u.Y/1024==y)))cell=y*256+x;
        if(cell<0)throw new InvalidDataException("No training arena available");soldier.X=(cell&255)*1024;soldier.Y=(cell>>8)*1024;
        var opponent=JsonSerializer.Deserialize<Actor>(JsonSerializer.Serialize(soldier))!;opponent.Slot=source.Units.Max(u=>u.Slot)+1;opponent.Owner=1;opponent.X+=4*1024;source.Units.Add(opponent);
        return new World{MapName=(ranged?"Arqueiro-":"Treino-")+source.MapName,SourceHash=source.SourceHash+(ranged?":archer-v1":":training-v1"),Width=source.Width,Height=source.Height,Tiles=source.Tiles,Collision=source.Collision,Objects=source.Objects,Quantities=source.Quantities,Resources=source.Resources,Units=source.Units,Buildings=source.Buildings,BuildingResidents=source.BuildingResidents};
    }
}
