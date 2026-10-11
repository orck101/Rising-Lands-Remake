namespace RisingLands;
public sealed partial class Simulation {
    void MigrateWorkerOrders(Save save){
        if(save.Version>45)return;
        foreach(var u in save.Units){
            // v127 retained an overflow marker when a carrier became a builder.
            if(u.FoodJob==0&&u.MineJob==0&&u.OverflowCell is >=0 and <65536)u.OverflowCell=-1;
            // The two-click gesture temporarily hid cargo to choose a new mine.
            // Restoring the cargo left both the mine trip and its delivery active.
            if(u.Type==7&&u.CargoKind==2&&u.Food>0&&u.FoodJob==3&&u.MineJob==1){
                u.MineJob=u.MineTicks=0;u.FoodSource=u.FoodField=u.FoodSourceBuilding=-1;
            }
        }
    }
}
