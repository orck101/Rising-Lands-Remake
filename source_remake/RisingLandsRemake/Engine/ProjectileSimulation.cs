using System.Text.Json;
using System.Text.Json.Serialization;
namespace RisingLands;

public sealed class Arrow {
    public int ProjectileType{get;set;}=9;public int Bonus{get;set;} public int Captain{get;set;} public bool Armored{get;set;}
    public int StepPixels{get;set;}=8; // Missing in v6 saves: retain their existing flight schedule.
    public int TargetBuilding{get;set;}=-1;
    public int SourceSlot{get;set;}public int TargetSlot{get;set;}public int StartX{get;set;}public int StartY{get;set;}public int EndX{get;set;}public int EndY{get;set;}public int Progress{get;set;}public int Damage{get;set;}public long BornTick{get;set;}
    [JsonIgnore] public int Length=>Math.Max(Math.Abs(EndX-StartX),Math.Abs(EndY-StartY));
    [JsonIgnore] public (int X,int Y) Position=>ProjectileMath.Point(StartX,StartY,EndX,EndY,Progress);
    [JsonIgnore] public bool HasImpacted=>StepPixels==30?Progress>Length:Progress>=Length;
    [JsonIgnore] public bool Finished=>StepPixels==30?Progress>Length+(Length>0?8:0):Progress>=Length;
    public IEnumerable<(int Type,int X,int Y,int Remaining)> ActiveParts(){for(int part=0;part<(StepPixels==30&&ProjectileType==9&&Length>0?5:1);part++){int progress=Progress-part*2;if(progress<0||progress>Length)continue;var p=ProjectileMath.Point(StartX,StartY,EndX,EndY,progress);yield return(ProjectileType+part,p.X,p.Y,Length-progress);}}
}
public static class ProjectileMath {
    // Exact integer raster geometry from ProjectileLineaire/ProjectileCalculLineaire.
    public static (int X,int Y) Point(int x,int y,int endX,int endY,int progress){int dx=Math.Abs(endX-x),dy=Math.Abs(endY-y),sx=endX>=x?1:-1,sy=endY>=y?1:-1;int major=Math.Max(dx,dy),minor=Math.Min(dx,dy),error=2*minor-major;
        for(int i=0;i<Math.Min(progress,major);i++){bool diagonal=error>=0;if(dx>dy){x+=sx;if(diagonal)y+=sy;}else{y+=sy;if(diagonal)x+=sx;}error+=diagonal?2*(minor-major):2*minor;}return(x,y);
    }
    public static bool WithinRange(int dx,int dy,int squared)=>(long)dx*dx+(long)dy*dy<=squared;
}
public sealed partial class Simulation {
    public AttackAlert LastAttack{get;}=new();
    public List<Arrow> Arrows{get;private set;}=new();
    public const int NativeArrowSteps=30; // Verified per ProjectileCalcul call; global 30 Hz remains laboratory timing.
    public bool InAttackRange(Actor a,Actor b)=>CombatRule(a.Type).Range>1?ProjectileMath.WithinRange(a.X/1024-b.X/1024,a.Y/1024-b.Y/1024,CombatRule(a.Type).Range*CombatRule(a.Type).Range):MeleeReach(a,b);
    void FireArrow(Actor a,Actor b,int damage){Arrows.Add(new Arrow{ProjectileType=AttackRuleFor(a)!.Projectile,Bonus=(MilitaryDamageBonus(a)+SpiritualDamageBonus(a)),Captain=CaptainBonus(a),Armored=b.ArmoredUntil>Tick,StepPixels=NativeArrowSteps,SourceSlot=a.Slot,TargetSlot=b.Slot,StartX=a.X/32+16,StartY=a.Y/32+16,EndX=b.X/1024*32+16,EndY=b.Y/1024*32+16,Damage=damage,BornTick=Tick});}
    void ApplyHit(Actor source,Actor target,int damage){if(target.Health<=0)return;if(source.Owner!=target.Owner&&damage>0)LastAttack.Record(unchecked((uint)Tick),target.Y/1024*256+target.X/1024,target.Owner);if(damage>0){RegisterAllianceAttack(source.Owner,target.Owner);RegisterUnitDamage(source.Owner,target.Owner);target.LastAttacker=source.Slot;target.LastHitTick=Tick;EmitUnitSound(target,1);}target.Health=Math.Max(0,target.Health-damage);if(target.Health==0)EmitUnitSound(target,5);target.HitFlash=damage>0?6:0;
        if(target.Health==0){ClearOrders(target);target.AttackCooldown=0;}
        else if(damage>0&&source.Type!=10&&Hostile(target.Owner,source.Owner)&&target.AttackTarget<0&&target.Route.Count==0&&DefenseAvailable(target)&&(target.Owner==0?UnitInSight(source):!OpponentEnabled||OpponentVision(target.Owner)[ActorCell(source)]!=0)&&CanAttack(target,source)){target.AttackTarget=source.Slot;target.AutomaticDefense=true;target.DefenseAnchor=target.Y/1024*256+target.X/1024;target.CombatRetryAt=0;}
    }
    void ProjectileStep(){foreach(var arrow in Arrows.ToArray()){
        if(arrow.TargetBuilding>=0){var structure=World.Buildings.FirstOrDefault(b=>b.Slot==arrow.TargetBuilding);bool impacted=arrow.HasImpacted;arrow.Progress+=arrow.StepPixels;if(!impacted&&arrow.HasImpacted&&structure!=null){if(arrow.Damage>0)ObserveBaseAttack(arrow.SourceSlot,structure);HitStructure(structure,arrow.Damage,World.Units.FirstOrDefault(u=>u.Slot==arrow.SourceSlot)?.Owner??-1);}if(arrow.Finished)Arrows.Remove(arrow);continue;}
        var target=World.Units.First(u=>u.Slot==arrow.TargetSlot);if(arrow.StepPixels==8&&target.Health<=0){Arrows.Remove(arrow);continue;}
        bool alreadyImpacted=arrow.HasImpacted;arrow.Progress+=arrow.StepPixels;
        // Fixed aim point, no homing. Tile occupancy at arrival is provisional hit logic.
        if(!alreadyImpacted&&arrow.HasImpacted&&target.X/1024==arrow.EndX/32&&target.Y/1024==arrow.EndY/32&&VisualCatalog.Visible(target))ApplyHit(World.Units.First(u=>u.Slot==arrow.SourceSlot),target,arrow.Damage);
        if(arrow.Finished)Arrows.Remove(arrow);
    }}
    void ValidateProjectileSave(Save save){if(save.Arrows==null){if(save.Version>=6)throw new InvalidDataException("Missing arrows");return;}if(save.Arrows.Count>4096)throw new InvalidDataException("Too many arrows");var ids=new HashSet<(int,long)>();
        foreach(var arrow in save.Arrows){if(arrow==null)throw new InvalidDataException("Null arrow");var source=save.Units.FirstOrDefault(u=>u.Slot==arrow.SourceSlot);var target=save.Units.FirstOrDefault(u=>u.Slot==arrow.TargetSlot);
            var building=arrow.TargetBuilding>=0?InitialBuildings.Concat(save.Constructions.Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).FirstOrDefault(b=>b.Slot==arrow.TargetBuilding):null;
            bool structureTarget=arrow.TargetBuilding>=0;
            int targetOwner=structureTarget?building?.Owner??-1:target?.Owner??-1;
            if(source==null||targetOwner<0||(structureTarget?(building==null||arrow.TargetSlot!=-1||arrow.Armored||!Inside(arrow.EndY/32*256+arrow.EndX/32,building.Cell,Rule(building.Type).Width,Rule(building.Type).Height)):(target==null||target.Type is <0 or >26))||!(AttackRuleFor(source)?.Projectile==arrow.ProjectileType)||source.Owner==targetOwner||!CombatOwner(source.Owner)||!CombatOwner(targetOwner)||arrow.BornTick<0||arrow.BornTick>save.Tick||!ids.Add((arrow.SourceSlot,arrow.BornTick)))throw new InvalidDataException("Invalid arrow identity");
            if(arrow.StepPixels is not (8 or 30)||arrow.StartX<0||arrow.StartY<0||arrow.EndX<0||arrow.EndY<0||arrow.StartX>=World.Width*32||arrow.EndX>=World.Width*32||arrow.StartY>=World.Height*32||arrow.EndY>=World.Height*32||arrow.EndX%32!=16||arrow.EndY%32!=16||arrow.Progress<0||arrow.Length>CombatRule(source.Type).Range*32+32||arrow.Finished)throw new InvalidDataException("Invalid arrow trajectory");
            long age=save.Tick-arrow.BornTick;if(age>(arrow.Length+8)/arrow.StepPixels||age*arrow.StepPixels!=arrow.Progress)throw new InvalidDataException("Invalid arrow clock");
            int damage=CombatMath.Damage(CombatRule(source.Type).Damage[structureTarget?26:target!.Type],arrow.Bonus,0,arrow.Captain,structureTarget?0:CombatRule(target!.Type).Armor,0,arrow.Armored?1:0);if(arrow.Bonus<0||arrow.Bonus>255||arrow.Captain is not (0 or 2)||arrow.Damage!=damage)throw new InvalidDataException("Invalid arrow damage");
        }
    }
}
