namespace RisingLands;
public static class PauseMenuTests {
    static void Check(bool value,string label){if(!value)throw new Exception("FAIL: "+label);Console.WriteLine("PASS: "+label);}
    public static void Run(string root){var menu=new PauseMenuState();menu.Show(MenuPage.Main);Check(menu.Open&&menu.Choice==0,"opening menu defaults to continue, not exit");menu.Move(-1,7);Check(menu.Choice==6,"menu keyboard selection wraps backward");menu.Move(1,7);Check(menu.Choice==0,"menu keyboard selection wraps forward");menu.Show(MenuPage.Exit);menu.Back();Check(menu.Page==MenuPage.Main,"Escape from exit returns to main menu");menu.Show(MenuPage.Controls);menu.Back();Check(menu.Page==MenuPage.Main,"Escape from controls returns to main menu");menu.Back();Check(!menu.Open,"Escape from main resumes without exiting");
        menu.Show(MenuPage.Restart);Check(menu.Choice==0,"restart confirmation defaults to cancel");menu.Back();Check(menu.Page==MenuPage.Main,"Escape cancels restart");
        foreach(bool training in new[]{false,true}){
            var world=LegacyAssets.Map(PackageLayout.Asset(root,"CD","WORLD","MONDE01.ETA"));new VisualCatalog().Apply(world);
            var sim=new Simulation(training?Simulation.TrainingWorld(world,true):world);string initial=sim.Serialize();
            if(training)sim.Attack(sim.World.Units.First(u=>u.Owner==0&&u.Type==8),sim.World.Units.Last());
            for(int i=0;i<20;i++)sim.Step();string progressed=sim.Serialize();Check(progressed!=initial,"restart fixture progressed "+training);
            sim.Restart();Check(sim.Serialize()==initial&&sim.LastAttack.Cell==65535,"restart resets full simulation and alert "+training);
            sim.Restore(progressed);Check(sim.Serialize()==progressed,"pre-restart save remains loadable "+training);
            sim.Restart();Check(sim.Serialize()==initial,"loading save does not replace restart baseline "+training);
        }
}
}
