namespace RisingLands;
public static class MouseCommandTests {
    static void Check(bool value,string label){if(!value)throw new Exception("FAIL: "+label);Console.WriteLine("PASS: "+label);}
    public static void Run(){
        Check(MouseCommands.Left(false,false,true,null)==LeftAction.Order,"left click on ground orders selected player units");
        Check(MouseCommands.Left(false,false,true,1)==LeftAction.Order,"left click on rival dispatches contextual command");
        Check(MouseCommands.Left(false,false,true,0)==LeftAction.Select,"left click on own unit replaces selection instead of moving");
        Check(MouseCommands.Left(true,false,true,null)==LeftAction.Select,"drag selection cannot accidentally issue a movement order");
        Check(MouseCommands.Left(false,true,true,null)==LeftAction.Select,"additive click cannot issue unintended orders");
        Check(MouseCommands.Left(false,false,false,1)==LeftAction.Select,"enemy inspection without own selection remains possible");
    }
}
