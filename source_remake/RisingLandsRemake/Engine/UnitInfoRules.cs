namespace RisingLands;
public static class UnitInfoRules {
    // AffEngin's information gate: selected local unit, hovered unit, or held Space.
    public static bool Show(int owner,bool selected,bool hovered,bool space)=>(owner==0&&selected)||hovered||space;
}
