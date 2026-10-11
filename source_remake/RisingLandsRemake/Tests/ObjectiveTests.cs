namespace RisingLands;
public static class ObjectiveTests {
    static void Check(bool value,string label){if(!value)throw new Exception("FAIL: "+label);Console.WriteLine("PASS: "+label);}
    public static void Run(string root){
        var reader=new ObjectiveReader();reader.Open("MONDE01.ETA",false);Check(reader.Count==3&&reader.Lines[0]=="- PROLOGO -","Portuguese first mission opens at prologue across three pages");
        reader.Move(int.MaxValue);Check(reader.Page==2,"objective reader clamps last page");reader.Move(int.MinValue);Check(reader.Page==0,"objective reader clamps first page");
        int present=0,pages=0;foreach(var path in Directory.GetFiles(PackageLayout.Asset(root,"CD/WORLD"),"*.ETA")){reader.Open(Path.GetFileName(path),false);bool exists=File.Exists(Path.Combine(Path.GetDirectoryName(path)!,Path.GetFileName(path)[..7]+".TXT"));if((reader.Count>0)!=exists)throw new Exception("Text mapping mismatch "+path);if(exists){present++;pages+=reader.Count;for(int i=0;i<reader.Count;i++){if(reader.Lines.Count!=18)throw new Exception("Native page size mismatch");reader.Move(1);}}}
        Check(present==34&&pages==53,"all 34 map variants resolve to Portuguese mission texts");reader.Open("MONDE01.ETA",true);Check(reader.Count==0&&reader.Page==0,"training never presents campaign objective");reader.Open("missing.ETA",false);reader.Move(1);Check(reader.Count==0&&reader.Page==0,"missing objective remains safe to navigate");reader.Open("monde01.eta",false);Check(reader.Page==0&&reader.Count==3,"reopening objective resets pagination and matches case-insensitively");
        foreach(string variant in new[]{"MONDE09D.ETA","MONDE10B.ETA","MONDE12C.ETA","MONDE17A.ETA"}){reader.Open(variant,false);string[] first=reader.Lines.ToArray();reader.Open(variant[..7]+".ETA",false);Check(first.SequenceEqual(reader.Lines),"branch uses canonical mission text "+variant);}
        var menu=new PauseMenuState();menu.Show(MenuPage.Objectives);menu.Back();Check(menu.Page==MenuPage.Main,"objective return preserves paused menu");
    }
}
