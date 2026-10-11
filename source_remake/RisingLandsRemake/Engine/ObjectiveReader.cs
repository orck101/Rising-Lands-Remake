using System.Text.Json;
namespace RisingLands;

public sealed record MissionPages(string Map,string[][] Pages);
public sealed class ObjectiveReader {
    readonly Dictionary<string,MissionPages> missions;
    readonly Dictionary<string,string> aliases;
    string[][] pages=Array.Empty<string[]>();
    public int Page {get;private set;}
    public int Count=>pages.Length;
    public IReadOnlyList<string> Lines=>Count==0?Array.Empty<string>():pages[Page];
    public string Notice {get;private set;}="";
    public ObjectiveReader(){var entries=JsonSerializer.Deserialize<MissionPages[]>(File.ReadAllText(PackageLayout.FromBase("Content/Data/objective_pages_ptbr.json")))!;missions=entries.ToDictionary(m=>m.Map,StringComparer.OrdinalIgnoreCase);aliases=new(JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(PackageLayout.FromBase("Content/Data/objective_aliases.json")))!,StringComparer.OrdinalIgnoreCase);}
    public void Open(string map,bool training){Page=0;pages=!training&&missions.TryGetValue(aliases.GetValueOrDefault(map,map),out var mission)?mission.Pages:Array.Empty<string[]>();Notice=training?"Treino livre: sem objetivo de campanha.":Count==0?"Objetivo indisponivel para este mapa.":"Objetivos traduzidos para portugues.";}
    public void Move(int delta){if(Count>0)Page=(int)Math.Clamp((long)Page+delta,0,Count-1);}
}
