using System.Text.Json;
namespace RisingLands;
public static class ControlGroupTests {
    static void Check(bool ok,string label){if(!ok)throw new Exception("FAIL: "+label);Console.WriteLine("PASS: "+label);}
    public static void Run(){
        using var data=JsonDocument.Parse(File.ReadAllText(PackageLayout.FromBase("Content/Data/control_groups.json")));
        Check(data.RootElement.GetProperty("bindings").EnumerateArray().All(b=>ControlGroups.Binding(b.GetProperty("ctrl").GetBoolean(),b.GetProperty("shift").GetBoolean())==(b.GetProperty("command").GetInt32(),b.GetProperty("center").GetBoolean())),"36 original numeric-group modifier bindings match");
        Actor Unit(int id,int owner=0)=>new(){Slot=id,Owner=owner,Health=100,LegacyState=7};
        int cases=0;
        foreach(var row in data.RootElement.GetProperty("assignments").EnumerateArray()){
            var g=new ControlGroups();var before=row.GetProperty("before").EnumerateArray().ToArray();var units=before.Select(b=>Unit(b.GetProperty("slot").GetInt32(),b.GetProperty("owner").GetInt32())).ToArray();
            foreach(var b in before.Where(b=>b.GetProperty("owner").GetInt32()==0))g.Store(b.GetProperty("group").GetInt32(),units.Where(u=>u.Slot==b.GetProperty("slot").GetInt32()),new[]{b.GetProperty("slot").GetInt32()});
            g.Store(row.GetProperty("group").GetInt32(),units,before.Where(b=>b.GetProperty("selected").GetBoolean()).Select(b=>b.GetProperty("slot").GetInt32()));
            var expected=row.GetProperty("after").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
            if(units.Where(u=>u.Owner==0).Any(u=>g.GroupOf(u.Slot)!=expected[u.Slot]))throw new Exception("Native group mismatch");cases++;
        }
        Check(cases==72,"72 native exclusive group assignments match player units");
        var groups=new ControlGroups();var army=new[]{Unit(1),Unit(2),Unit(3,1)};groups.Store(1,army,new[]{1,2,3});groups.Store(2,army,new[]{1});
        Check(groups.Recall(1,army).Select(u=>u.Slot).SequenceEqual(new[]{2})&&groups.Recall(2,army).Select(u=>u.Slot).SequenceEqual(new[]{1}),"assigning another group transfers membership instead of duplicating it");
        army[1].Health=0;Check(groups.Recall(1,army).Length==0,"recall omits defeated units");
        groups.Store(2,army,Array.Empty<int>());Check(groups.Recall(2,army).Length==0,"storing empty selection clears that group");
        groups.Store(3,army,new[]{1});groups.Clear();Check(groups.Recall(3,army).Length==0,"map or save replacement clears session groups");
    }
}
