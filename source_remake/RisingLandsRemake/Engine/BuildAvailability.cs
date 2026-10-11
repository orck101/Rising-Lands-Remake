using System.Text.Json;
using System.Text.RegularExpressions;
namespace RisingLands;
public record Invention(string Key,int Branch,int Index,int Cost,int Appearance,int Mission,int Effect,int Param1,int Param2,int Param3,int[] Buildings);
public record InitialTechnology(int Mission,int[] Levels,int[] Buildings);
public record UnlockCatalog(List<Invention> Technologies,List<InitialTechnology> Missions);
public sealed partial class Simulation {
    public static readonly UnlockCatalog UnlockData=JsonSerializer.Deserialize<UnlockCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/build_unlocks.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true,PropertyNamingPolicy=JsonNamingPolicy.SnakeCaseLower})!;
    public int MissionNumber=>int.TryParse(Regex.Match(World.MapName,@"\d+").Value,out int n)?Math.Clamp(n,1,25):1;
    public bool BuildingUnlocked(int type)=>UnlockData.Technologies.Any(t=>t.Index<Campaign.StartingLevels[t.Branch]&&t.Buildings.Contains(type))||UnlockData.Technologies.Any(t=>(TechnologyApplied(t))&&t.Buildings.Contains(type));
    public string BuildingRequirement(int type){
        if(BuildingUnlocked(type))return "Tecnologia disponivel";
        var t=UnlockData.Technologies.FirstOrDefault(t=>t.Buildings.Contains(type));
        if(t==null)return "Requisito ainda em estudo";
        if(t.Mission>MissionNumber)return "Requer missao "+t.Mission;
        return new[]{"Agricultura","Militar","Engenharia","Religiao"}[t.Branch]+" - pesquisa "+(t.Index+1);
    }
    public string? BuildChoiceError(int type){
        if(!BuildingUnlocked(type))return BuildingRequirement(type);
        var r=Rule(type);if(r.Objects.Count==0)return "Construcao ainda indisponivel";
        if(World.Resources[0]<r.Food||World.Resources[4]<r.Rock||World.Resources[8]<r.Parts)return "Recursos insuficientes";
        return null;
    }
}
