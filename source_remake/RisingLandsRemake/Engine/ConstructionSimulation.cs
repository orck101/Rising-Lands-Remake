using System.Text.Json;
namespace RisingLands;
public record BuildingRule(int Type,int Width,int Height,int Food,int Rock,int Parts,int Duration,List<SceneObject> Objects,List<TileChange> Tiles,List<int> Collision);
public record ConstructionCatalog(List<BuildingRule> Buildings);
public class ConstructionWorker {public int BuilderSlot{get;set;}=-1;public int WorkCell{get;set;}public int AnimationTick{get;set;}public int StrikeCooldown{get;set;}=20;}
public sealed class Construction : ConstructionWorker {public int Owner{get;set;}public int Id{get;set;}public int Type{get;set;}public int Cell{get;set;}public int ReplacesWall{get;set;}=-1;public int GateOrientation{get;set;}public int GateFrame{get;set;}=15;public bool GateOpenTarget{get;set;}
 public int Progress{get;set;}public List<ConstructionWorker> Helpers{get;set;}=new();}

public sealed partial class Simulation {
    public static readonly ConstructionCatalog BuildData=JsonSerializer.Deserialize<ConstructionCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/construction_rules.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true,PropertyNamingPolicy=JsonNamingPolicy.SnakeCaseLower})!;
    public List<Construction> Constructions{get;private set;}=new();
    public event Action<Construction>? ConstructionStruck;
    public static int ConstructionFacing(int workCell,int anchor,int width,int height){
        int x=workCell&255,y=workCell>>8,dx=Math.Sign(Math.Clamp(x,anchor&255,(anchor&255)+width-1)-x),dy=Math.Sign(Math.Clamp(y,anchor>>8,(anchor>>8)+height-1)-y);
        return dx<0?(dy<0?3:dy>0?15:1):dx>0?(dy<0?7:dy>0?11:9):dy<0?5:13;
    }
    List<Building> InitialBuildings;
    public BuildingRule Rule(int type)=>BuildRuleLookup[type];
    public bool Complete(Construction c)=>c.Progress>=Rule(c.Type).Duration;
    public string BuildName(int type)=>new[]{"Farol","Quartel","Santuario","Armazem","Cultivo","Oficina","Biblioteca","Laboratorio","Bolsa de trocas","Muralha","Torre de catapulta","Torre de canhao","Portao","Circo","Garagem","Templo","Mercado","Cristal","Heliporto","Hospital","Templo de fusao","Tenda de campanha"}[type];
    public static bool Inside(int cell,int anchor,int width,int height)=>(cell&255)>=(anchor&255)&&(cell&255)<(anchor&255)+width&&(cell>>8)>=(anchor>>8)&&(cell>>8)<(anchor>>8)+height;
    bool BuildBlocks(int cell)=>SiteAt(cell) is {} c&&(!Complete(c)||BuildCollisionLookup[c.Type].Contains(cell-c.Cell));
    int[] Spending(IEnumerable<Construction> sites,IEnumerable<Recruitment>? recruits=null,SpecialState? specials=null,IEnumerable<TrainingOrder>? queue=null){var costs=new int[12];foreach(var c in sites){var rule=Rule(c.Type);costs[c.Owner]+=rule.Food;costs[4+c.Owner]+=rule.Rock;costs[8+c.Owner]+=rule.Parts;}foreach(var t in recruits??Recruitments){var rule=TrainingRule(t.Type);costs[t.Owner]+=rule.Food;costs[4+t.Owner]+=rule.Rock;costs[8+t.Owner]+=rule.Parts;}foreach(var q in queue??TrainingQueue){costs[0]+=q.Food;costs[4]+=q.Rock;costs[8]+=q.Parts;}foreach(var j in (specials??Special).Learning){var spell=Spell(j.Spell);costs[j.Owner]+=spell.Food;costs[4+j.Owner]+=spell.Rock;costs[8+j.Owner]+=spell.Parts;}return costs;}
    // TerrainEstHerbe, original 0x476296: [0x15d,0x1a5).
    public bool ConstructionGrass(int cell)=>cell>=0&&cell<65536&&World.Tiles[cell] is >=349 and <421 or >=16 and <31;
    public bool ConstructionTerrainAllowed(int type,int cell)=>cell>=0&&cell<65536&&(type is 9 or 12||ConstructionGrass(cell));
    public bool ConstructionSurface(int cell)=>World.Objects[cell]==0||World.Objects[cell] is >=12 and <=28;
    public bool ConstructionCovers(int cell)=>SiteAt(cell)!=null;
    public string? PlacementError(Actor? builder,int type,int cell){if(builder==null||builder.Owner is <0 or >3||builder.Type!=7||!VisualCatalog.Visible(builder))return "Selecione um construtor.";if(builder.Owner==0){if(BuildChoiceError(type) is {} unavailable)return unavailable;}else if(!OpponentEnabled||!OpponentBuildingUnlocked(builder.Owner,type)||type==12)return "Construcao nao liberada para este cla.";var r=Rule(type);int x=cell&255,y=cell>>8;if(cell<0||x+r.Width>World.Width||y+r.Height>World.Height)return "Area fora do mapa.";
        if(type==12)return GatePlacementError(cell);
        for(int by=0;by<r.Height;by++)for(int bx=0;bx<r.Width;bx++){int i=cell+by*256+bx;if(!ConstructionTerrainAllowed(type,i))return "Construa inteiramente sobre o terreno verde.";if(BuildingAt(i)!=null||!Passable(x+bx,y+by)||!ConstructionSurface(i)||World.Units.Any(u=>VisualCatalog.Visible(u)&&Inside((u.Y/1024)*256+u.X/1024,i,1,1))||Constructions.Any(c=>Inside(i,c.Cell,Rule(c.Type).Width,Rule(c.Type).Height)))return "Area ocupada ou terreno bloqueado.";}
        if(World.Resources[builder.Owner]<r.Food||World.Resources[4+builder.Owner]<r.Rock||World.Resources[8+builder.Owner]<r.Parts)return "Recursos insuficientes.";return null;
    }
    IEnumerable<int> WorkCells(int anchor,BuildingRule r){int x=anchor&255,y=anchor>>8;for(int dy=-1;dy<=r.Height;dy++)for(int dx=-1;dx<=r.Width;dx++){if(dx>=0&&dx<r.Width&&dy>=0&&dy<r.Height)continue;int nx=x+dx,ny=y+dy;if(nx>=0&&ny>=0&&nx<World.Width&&ny<World.Height)yield return ny*256+nx;}}
    public bool StartConstruction(Actor builder,int type,int cell){if(PlacementError(builder,type,cell)!=null)return false;var r=Rule(type);var site=new Construction{Id=InitialBuildings.Select(b=>b.Slot).DefaultIfEmpty(-1).Max()+1+Constructions.Count,Owner=builder.Owner,Type=type,Cell=cell,BuilderSlot=builder.Slot,ReplacesWall=type==12?BuildingAt(cell)!.Slot:-1,GateOrientation=type==12?GateOrientationAt(cell):0};Constructions.Add(site);
        bool found=false;foreach(int target in WorkCells(cell,r).OrderBy(i=>Math.Abs((i&255)-builder.X/1024)+Math.Abs((i>>8)-builder.Y/1024))){if(Order(builder,target&255,target>>8,true)){site.WorkCell=target;found=true;break;}}
        if(!found){Constructions.Remove(site);return false;}DetachBuilder(builder.Slot,site);builder.MineJob=0;builder.MineTicks=0;builder.FoodJob=0;builder.WorkTicks=0;
        CancelFeeding(builder);builder.OverflowCell=-1;World.Resources[builder.Owner]-=(ushort)r.Food;World.Resources[4+builder.Owner]-=(ushort)r.Rock;World.Resources[8+builder.Owner]-=(ushort)r.Parts;return true;
    }
    public static IEnumerable<ConstructionWorker> Workers(Construction c){if(c.BuilderSlot>=0)yield return c;foreach(var w in c.Helpers)yield return w;}
    public static bool HasBuilder(Construction c,int slot)=>c.BuilderSlot==slot||c.Helpers.Any(w=>w.BuilderSlot==slot);
    void DetachBuilder(int slot,Construction? except=null){Opponent.Repairs.Remove(slot);foreach(var c in Constructions.Where(c=>c!=except&&!Complete(c))){if(c.BuilderSlot==slot)c.BuilderSlot=-1;c.Helpers.RemoveAll(w=>w.BuilderSlot==slot);}}
    public ConstructionWorker? WorkingBuilder(Actor u)=>Feeding(u)||u.Route.Count!=0?null:Constructions.Where(c=>!Complete(c)).SelectMany(Workers).FirstOrDefault(w=>w.BuilderSlot==u.Slot&&u.X==(w.WorkCell&255)*1024&&u.Y==(w.WorkCell>>8)*1024)??WorkingRepair(u);
    public bool ResumeConstruction(Actor builder,Construction site){
        if(!Constructions.Contains(site)||!VisualCatalog.Visible(builder)||builder.Type!=7||builder.Owner!=site.Owner||Complete(site))return false;
        if(HasBuilder(site,builder.Slot))return true;
        var taken=Workers(site).Select(w=>w.WorkCell).ToHashSet();int target=-1;
        foreach(int cell in WorkCells(site.Cell,Rule(site.Type)).Where(c=>!taken.Contains(c)).OrderBy(c=>Math.Abs((c&255)-builder.X/1024)+Math.Abs((c>>8)-builder.Y/1024))){
            if(World.Units.Any(u=>u!=builder&&VisualCatalog.Visible(u)&&u.X/1024==(cell&255)&&u.Y/1024==(cell>>8)))continue;
            if(Order(builder,cell&255,cell>>8,true)){target=cell;break;}}
        if(target<0)return false;DetachBuilder(builder.Slot,site);
        if(site.BuilderSlot<0){site.BuilderSlot=builder.Slot;site.WorkCell=target;}else site.Helpers.Add(new ConstructionWorker{BuilderSlot=builder.Slot,WorkCell=target});
        CancelFeeding(builder);builder.OverflowCell=-1;builder.FoodJob=0;builder.WorkTicks=0;builder.MineJob=0;builder.MineTicks=0;return true;
    }
    void BuildStep(){foreach(var c in Constructions.Where(c=>!Complete(c))){
        if(IsFifthMission&&ClanEliminated(c.Owner))continue;
        foreach(var w in Workers(c)){
            var u=World.Units.First(a=>a.Slot==w.BuilderSlot);if(Feeding(u)||!VisualCatalog.Visible(u)||u.Route.Count!=0||u.X!=(w.WorkCell&255)*1024||u.Y!=(w.WorkCell>>8)*1024)continue;
            var rule=Rule(c.Type);u.Direction=ConstructionFacing(w.WorkCell,c.Cell,rule.Width,rule.Height);
            // Each present worker commits its own native hammer cadence; no contribution while travelling.
            w.AnimationTick=(w.AnimationTick+1)%16;if(w.StrikeCooldown>0)w.StrikeCooldown--;
            if(w.StrikeCooldown!=0||w.AnimationTick/2!=2)continue;
            w.StrikeCooldown=20;c.Progress=Math.Min(rule.Duration,c.Progress+ConstructionStrikeProgressFor(c.Owner));ConstructionStruck?.Invoke(c);
            if(Complete(c)){var completed=new Building(c.Id,c.Type,c.Owner,c.Cell);if(c.Type==12){World.Buildings.RemoveAll(b=>b.Slot==c.ReplacesWall);InvalidateSpatialIndex();doorCacheCount=-1;}World.Buildings.Add(completed);EmitBuildingSound(completed,0x23);break;}
        }
    }}
    void ValidateBuildSave(Save s){if(s.Constructions==null||s.Constructions.Count>1000)throw new InvalidDataException("Invalid sites");ValidateGates(s);var activeWorkers=new HashSet<int>();var ids=new HashSet<int>();foreach(var c in s.Constructions){if(c.Owner is <0 or >3||!BuildData.Buildings.Any(r=>r.Type==c.Type)||!ids.Add(c.Id)||InitialBuildings.Any(b=>b.Slot==c.Id)||c.Id<0)throw new InvalidDataException("Invalid construction identity");var r=Rule(c.Type);if(c.Cell<0||(c.Cell&255)+r.Width>World.Width||(c.Cell>>8)+r.Height>World.Height||c.Progress<0||c.Progress>r.Duration||c.AnimationTick is <0 or >=16||c.StrikeCooldown is <0 or >20||!WorkCells(c.Cell,r).Contains(c.WorkCell)||c.BuilderSlot!= -1&&!s.Units.Any(u=>u.Slot==c.BuilderSlot&&(Complete(c)||u.Owner==c.Owner&&u.Type==7)))throw new InvalidDataException($"Invalid construction state (site {c.Id}, builder {c.BuilderSlot})");
            if(c.Helpers==null||c.Helpers.Any(w=>w==null)||c.Helpers.Count>s.Units.Count)throw new InvalidDataException("Invalid construction helpers");
            var cells=new HashSet<int>();var workers=new HashSet<int>();
            foreach(var w in Workers(c))if(!workers.Add(w.BuilderSlot)||!cells.Add(w.WorkCell)||!WorkCells(c.Cell,r).Contains(w.WorkCell)||w.AnimationTick is <0 or >=16||w.StrikeCooldown is <0 or >20||!s.Units.Any(u=>u.Slot==w.BuilderSlot&&(Complete(c)||u.Owner==c.Owner&&u.Type==7))||!Complete(c)&&!activeWorkers.Add(w.BuilderSlot))throw new InvalidDataException("Invalid construction crew");
        }
        foreach(var c in s.Constructions)foreach(var d in s.Constructions.Where(d=>d.Id>c.Id)){var r=Rule(c.Type);var t=Rule(d.Type);if((c.Cell&255)<(d.Cell&255)+t.Width&&(d.Cell&255)<(c.Cell&255)+r.Width&&(c.Cell>>8)<(d.Cell>>8)+t.Height&&(d.Cell>>8)<(c.Cell>>8)+r.Height){if(!(d.Type==12&&d.ReplacesWall==c.Id&&c.Type==9&&Complete(c)&&c.Cell==d.Cell))throw new InvalidDataException("Overlapping sites");}}
    }
}
