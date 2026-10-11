namespace RisingLands;
public enum LeftAction { Select, Order }
public static class MouseCommands {
    // Reconstructed left-button command model; special original contexts remain partial.
    public static LeftAction Left(bool dragged,bool additive,bool hasPlayerSelection,int? hoveredOwner)=>!dragged&&!additive&&hasPlayerSelection&&hoveredOwner!=0?LeftAction.Order:LeftAction.Select;
}
