using System.Text.Json;
namespace RisingLands;

public record SpecialVisual(int Type,int Phase,List<DrawCommand> Commands);
public record SpecialVisualCatalog(List<SpecialVisual> Visuals);
public record DrawClip(int Left,int Top,int Right,int Bottom);
public record DrawCommand(string Kind,int X,int Y,int Sprite,DrawClip? Clip=null);
public record ConstructionStage(int Progress,List<SceneObject> Objects);
public record ConstructionVisual(int Type,List<ConstructionStage> Stages);
public record ConstructionVisuals(List<ConstructionVisual> Buildings);
public record UnitFrame(int Direction,int Phase,List<DrawCommand> Commands);
public record UnitVisual(int Type,string Name,int FrameCount,List<UnitFrame> Frames,int FrameDelay=2);
public record WorkerVisualCatalog(List<UnitVisual> Types);
public record SpriteBank(string Name,int Base,int Count);
public record CatalogFile(List<SpriteBank> Banks,List<UnitVisual> Types);
public record ProjectileVisual(int Type,List<DrawCommand> Commands);
public record ProjectileVisualCatalog(List<ProjectileVisual> Visuals);
public record SceneObject(int Cell,List<DrawCommand> Commands,int ObjectCode=0);
public record TileChange(int Cell,int Tile);
public record CollisionChange(int Cell,int Value);
public record SceneFile(string Map,string SourceSha256,List<SceneObject> Objects,List<TileChange> TileChanges,List<CollisionChange> CollisionChanges);
public sealed class VisualCatalog {
    static readonly JsonSerializerOptions Options=new(){PropertyNameCaseInsensitive=true,PropertyNamingPolicy=JsonNamingPolicy.SnakeCaseLower};
    public CatalogFile Catalog {get;}
    public UnitVisual AttackVisual {get;}=JsonSerializer.Deserialize<UnitVisual>(File.ReadAllText(PackageLayout.FromBase("Content/Data/swordsman_attack.json")),Options)!;
    public UnitVisual ArcherVisual {get;}=JsonSerializer.Deserialize<UnitVisual>(File.ReadAllText(PackageLayout.FromBase("Content/Data/archer_attack.json")),Options)!;
    public List<DrawCommand> ArrowCommands {get;}=new(){new("shadow",-1,-1,10896),new("sprite",-1,-1,10896)}; // Native AffProjectile type 9 trace.
    public ProjectileVisualCatalog ProjectileVisuals {get;}=JsonSerializer.Deserialize<ProjectileVisualCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/projectile_schedule.json")),Options)!;
    public ConstructionVisuals ConstructionVisuals {get;}=JsonSerializer.Deserialize<ConstructionVisuals>(File.ReadAllText(PackageLayout.FromBase("Content/Data/construction_visuals.json")),Options)!;
    public UnitVisual BuilderVisual {get;}=JsonSerializer.Deserialize<UnitVisual>(File.ReadAllText(PackageLayout.FromBase("Content/Data/builder_construction.json")),Options)!;
    public List<SceneObject> ConstructionFrame(Construction site,Simulation sim)=>ConstructionVisuals.Buildings.First(b=>b.Type==site.Type).Stages[Math.Clamp(site.Progress*100/sim.Rule(site.Type).Duration,0,100)].Objects;
    public WorkerVisualCatalog WorkerVisuals {get;}=JsonSerializer.Deserialize<WorkerVisualCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/worker_visuals.json")),Options)!;
    readonly Dictionary<(int,string,int,int),List<DrawCommand>> workerFrames=new();
    public static bool HiddenByWork(Actor u)=>u.Type==7&&u.MineJob==2&&u.Route.Count==0&&u.Satiety>0;
    public static bool Hoeing(Actor u,Simulation? sim)=>u.Satiety>0&&u.Type==5&&u.FoodJob==2&&u.Route.Count==0&&sim!=null&&u.FoodSource>=0&&sim.World.Objects[u.FoodSource] is >=29 and <=52;
    readonly Dictionary<(int,int,int),List<DrawCommand>> frames=new();
    readonly Dictionary<int,string> names=new();
    public VisualCatalog(){foreach(var type in WorkerVisuals.Types)foreach(var f in type.Frames)workerFrames[(type.Type,type.Name,f.Direction,f.Phase)]=f.Commands;Catalog=JsonSerializer.Deserialize<CatalogFile>(File.ReadAllText(PackageLayout.FromBase("Content/Data/unit_rendering.json")),Options)!;foreach(var type in Catalog.Types)foreach(var f in type.Frames)frames[(type.Type,f.Direction,f.Phase)]=f.Commands;using var d=JsonDocument.Parse(File.ReadAllText(PackageLayout.FromBase("Content/Data/units.json")));foreach(var x in d.RootElement.GetProperty("items").EnumerateArray())names[x.GetProperty("id").GetInt32()]=x.GetProperty("display_name").GetString()??x.GetProperty("internal_name").GetString()!;}
    public string Name(Actor u)=>u.Type==17?"Balao":names.GetValueOrDefault(u.Type,"Unidade");
    public List<DrawCommand> Frame(Actor u,Simulation? sim=null){
        if((u.AttackTarget>=0||sim?.Sieging(u)==true)&&u.Route.Count==0&&u.AttackCooldown>0&&(Simulation.AdvancedAttacks.Types.FirstOrDefault(v=>v.Type==u.Type)??(sim?.IsSeventhMission==true&&u.Owner==4?Simulation.CreatureAttacks.Types.FirstOrDefault(v=>v.Type==u.Type):null)) is {} advanced){int phase=Math.Clamp(((sim?.CombatCycle(u)??Math.Max(Simulation.CombatData.Rules.First(r=>r.Type==u.Type).Reload,Simulation.AdvancedAttacks.Rules.First(r=>r.Type==u.Type).ReleaseTicks))-u.AttackCooldown+1)/Math.Max(1,advanced.FrameDelay),0,advanced.FrameCount-1);return advanced.Frames.First(f=>f.Direction==Math.Clamp(u.Direction,1,16)&&f.Phase==phase).Commands;}
        if(u.Type==4&&sim!=null&&u.CastAnimationUntil>sim.Tick){var cast=Simulation.AdvancedAttacks.Types.First(v=>v.Type==4);int phase=Math.Clamp((int)(32-(u.CastAnimationUntil-sim.Tick))/cast.FrameDelay,0,cast.FrameCount-1);return cast.Frames.First(f=>f.Direction==Math.Clamp(u.Direction,1,16)&&f.Phase==phase).Commands;}
        if(u.Type==7&&sim?.WorkingBuilder(u) is {} buildWork){int phase=buildWork.AnimationTick/Math.Max(1,BuilderVisual.FrameDelay)%BuilderVisual.FrameCount;return BuilderVisual.Frames.First(f=>f.Direction==Math.Clamp(u.Direction,1,16)&&f.Phase==phase).Commands;}
        if(HiddenByWork(u))return workerFrames[(7,"mining",Math.Clamp(u.Direction,1,16),0)];
        string? work=Hoeing(u,sim)?"harvest":u.Type is 5 or 6 or 7 or 13&&(u.Food>0||u.Rock>0)?"loaded":null;
        if(u.Type==13&&sim!=null&&u.FoodJob==2&&u.Route.Count==0)work="harvest";
          if(work!=null){var visual=WorkerVisuals.Types.First(v=>v.Type==u.Type&&v.Name==work);int phase=work=="harvest"?Math.Max(0,sim!.WorkerCountdown(u)-u.WorkTicks)/Math.Max(1,visual.FrameDelay):u.AnimationFrame;return workerFrames[(u.Type,work,Math.Clamp(u.Direction,1,16),phase%visual.FrameCount)];}
        if(u.Type is 0 or 8&&(u.AttackTarget>=0||sim?.Sieging(u)==true)&&u.Route.Count==0&&u.AttackCooldown>0){var attack=u.Type==8?ArcherVisual:AttackVisual;int phase=Math.Clamp(((sim?.CombatCycle(u)??Math.Max(Simulation.CombatData.Rules.First(r=>r.Type==u.Type).Reload,Simulation.AdvancedAttacks.Rules.First(r=>r.Type==u.Type).ReleaseTicks))-u.AttackCooldown+1)/Math.Max(1,attack.FrameDelay),0,attack.FrameCount-1);return attack.Frames.First(f=>f.Direction==Math.Clamp(u.Direction,1,16)&&f.Phase==phase).Commands;}return frames.GetValueOrDefault((u.Type,Math.Clamp(u.Direction,1,16),u.AnimationFrame),new());}
    public List<DrawCommand> ProjectileCommands(int type,int phase)=>ProjectileVisuals.Visuals.FirstOrDefault(v=>v.Type==type)?.Commands??CreatureProjectileVisuals.Visuals.FirstOrDefault(v=>v.Type==type&&v.Phase==phase%16)?.Commands??SpecialVisuals.Visuals.FirstOrDefault(v=>v.Type==type&&v.Phase==phase%16)?.Commands??new();
    public SpecialVisualCatalog CreatureProjectileVisuals {get;}=JsonSerializer.Deserialize<SpecialVisualCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/creature_projectiles.json")),Options)!;
    public SpecialVisualCatalog SpecialVisuals {get;}=JsonSerializer.Deserialize<SpecialVisualCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/special_visuals.json")),Options)!;
    public SceneFile Scene(World w){string file=PackageLayout.FromBase("Content/Data/Scenes",Path.GetFileNameWithoutExtension(w.MapName)+".json");var scene=JsonSerializer.Deserialize<SceneFile>(File.ReadAllText(file),Options)??throw new InvalidDataException("Scene missing");if(!scene.SourceSha256.Equals(w.SourceHash,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Scene does not match map SHA256");return scene;}
    public void Apply(World w){var s=Scene(w);foreach(var c in s.TileChanges)w.Tiles[c.Cell]=(ushort)c.Tile;foreach(var c in s.CollisionChanges)w.Collision[c.Cell]=(byte)c.Value;}
    public static bool Visible(Actor u)=>u.Health>0&&u.LegacyState is not (0 or 10 or 12 or 14);
}
