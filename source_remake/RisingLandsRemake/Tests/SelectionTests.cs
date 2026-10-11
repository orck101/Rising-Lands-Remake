namespace RisingLands;
public static class SelectionTests {
    static void Check(bool value,string label){if(!value)throw new Exception("FAIL: "+label);Console.WriteLine("PASS: "+label);}
    public static void Run(){
        Check(SelectionRules.Catalog.Vectors.Count==1430&&SelectionRules.Catalog.Vectors.All(v=>SelectionRules.InZone(v.Type,32,32,v.Left,v.Top,v.Right,v.Bottom)==v.Expected),"1430 native drag-selection zones match all 26 types");
        Check(SelectionRules.Catalog.Vectors.All(v=>SelectionRules.InZone(v.Type,32,32,v.Right,v.Bottom,v.Left,v.Top)==v.Expected),"reversed drag keeps native inclusive zone geometry");
        Actor Unit(int slot,int owner=0)=>new(){Slot=slot,Owner=owner,Health=100,LegacyState=7};
        var own=Unit(1);var ally=Unit(2);var enemy=Unit(3,1);
        Check(SelectionRules.Combine(new[]{own},new[]{enemy},true,false).SequenceEqual(new[]{1}),"additive enemy click cannot mix inspection with player commands");
        Check(SelectionRules.Combine(new[]{enemy},new[]{own},true,false).SequenceEqual(new[]{1}),"additive player selection clears an inspected enemy");
        Check(SelectionRules.Combine(new[]{own},new[]{own,ally},true,false).SequenceEqual(new[]{1,2}),"additive selection preserves units without duplicates");
        Check(SelectionRules.Combine(new[]{own},new[]{enemy},false,false).SequenceEqual(new[]{3}),"single enemy inspection remains available");
        Check(SelectionRules.Combine(Array.Empty<Actor>(),new[]{enemy,own},false,true).SequenceEqual(new[]{1}),"box selection includes only player units");
        Check(SelectionRules.Combine(Array.Empty<Actor>(),Enumerable.Range(0,300).Select(i=>Unit(i)),false,true).Length==255,"selection obeys original 255-unit bound");
        own.Health=0;Check(SelectionRules.Combine(new[]{own},new[]{ally},true,false).SequenceEqual(new[]{2}),"dead units removed during additive selection");
    }
}
