namespace RisingLands;

public sealed class ControlGroups {
    readonly Dictionary<int,int> membership=new();
    public static (int Command,bool Center) Binding(bool ctrl,bool shift)=>(ctrl?11:12,!ctrl&&shift);
    public int GroupOf(int slot)=>membership.GetValueOrDefault(slot);
    public void Clear()=>membership.Clear();
    public void Store(int group,IEnumerable<Actor> world,IEnumerable<int> selected){
        if(group is <1 or >9)throw new ArgumentOutOfRangeException(nameof(group));
        var ids=selected.ToHashSet();
        foreach(var u in world.Where(u=>u.Owner==0&&u.Health>0&&u.LegacyState is not (0 or 4))){
            if(GroupOf(u.Slot)==group)membership.Remove(u.Slot);
            if(ids.Contains(u.Slot))membership[u.Slot]=group;
        }
    }
    public Actor[] Recall(int group,IEnumerable<Actor> world)=>world.Where(u=>u.Owner==0&&VisualCatalog.Visible(u)&&GroupOf(u.Slot)==group).OrderBy(u=>u.Slot).Take(SelectionRules.Maximum).ToArray();
}
