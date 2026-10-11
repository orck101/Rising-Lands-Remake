using System.Text.Json;
namespace RisingLands;
public static class UnitInfoTests {
    public static void Run(){
        using var data=JsonDocument.Parse(File.ReadAllText(PackageLayout.FromBase("Content/Data/unit_info_rules.json")));
        var vectors=data.RootElement.GetProperty("vectors").EnumerateArray().ToArray();
        if(vectors.Length!=64||vectors.Any(v=>UnitInfoRules.Show(v.GetProperty("owner").GetInt32(),v.GetProperty("selected").GetBoolean(),v.GetProperty("hovered").GetBoolean(),v.GetProperty("space").GetBoolean())!=v.GetProperty("expected").GetBoolean()))throw new Exception("FAIL: native information gate");
        Console.WriteLine("PASS: 64 native selection, hover and held-Space information gates match");
    }
}
