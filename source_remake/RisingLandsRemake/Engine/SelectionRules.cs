using System.Text.Json;
namespace RisingLands;

public record SelectionOffset(int X,int Y);
public record SelectionFootprint(int Type,List<SelectionOffset> Offsets);
public record SelectionVector(int Type,int Left,int Top,int Right,int Bottom,bool Expected);
public record SelectionCatalog(List<SelectionFootprint> Rules,List<SelectionVector> Vectors);
public static class SelectionRules {
    public const int Maximum=255; // Native SelectionAdd/EnginAddSelection guard.
    public static readonly SelectionCatalog Catalog=JsonSerializer.Deserialize<SelectionCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/selection_rules.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
    public static bool InZone(int type,int cellX,int cellY,int left,int top,int right,int bottom){
        var rule=Catalog.Rules.FirstOrDefault(r=>r.Type==type);
        return rule!=null&&rule.Offsets.Any(o=>cellX+o.X>=Math.Min(left,right)&&cellX+o.X<=Math.Max(left,right)&&cellY+o.Y>=Math.Min(top,bottom)&&cellY+o.Y<=Math.Max(top,bottom));
    }
    // Inspection of an enemy is allowed alone; additive command selection is player-owned.
    public static int[] Combine(IEnumerable<Actor> current,IEnumerable<Actor> hits,bool additive,bool box){
        var result=additive?current.Where(u=>u.Owner==0&&VisualCatalog.Visible(u)).ToList():new List<Actor>();
        foreach(var u in hits.Where(VisualCatalog.Visible).OrderBy(u=>u.Slot)){
            if((additive||box)&&u.Owner!=0)continue;
            if(result.Count==Maximum)break;
            if(result.All(a=>a.Slot!=u.Slot))result.Add(u);
            if(!additive&&!box)break;
        }
        return result.Select(u=>u.Slot).ToArray();
    }
}
