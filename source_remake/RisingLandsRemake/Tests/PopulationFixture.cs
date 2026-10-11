namespace RisingLands;
public static class PopulationFixture {
 public static void Inhabit(World world,int slot){if(world.BuildingResidents.GetValueOrDefault(slot,new()).Count>0)return;var actor=world.Units.First(a=>a.Type==9&&a.Owner==0&&a.LegacyState==14);foreach(var ids in world.BuildingResidents.Values)ids.Remove(actor.Slot);world.BuildingResidents[slot]=new(){actor.Slot};}
}
