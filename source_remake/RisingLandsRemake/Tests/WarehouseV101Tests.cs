using System.Reflection;
namespace RisingLands;
public static class WarehouseV101Tests {
 static int checks;
 static void Check(bool value,string label){if(!value)throw new Exception("FAIL v101: "+label);checks++;Console.WriteLine("PASS v101: "+label);}
 static object? Call(Simulation s,string name,int owner)=>typeof(Simulation).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(s,new object[]{owner});
 static Simulation Fixture(string root,int owner=1){var s=StrategyV96Tests.Load(root,"MONDE05.ETA",true);if(!s.World.Buildings.Any(b=>b.Owner==owner&&b.Type==3))s.World.Buildings.Add(new Building(98000+owner,3,owner,s.World.Buildings.First(b=>b.Owner==owner).Cell+10));foreach(var b in s.World.Buildings.Where(b=>b.Owner==owner&&b.Type==3)){if(!s.Inhabited(b)){var a=new Actor{Slot=99000+s.World.Units.Count,Owner=owner,Type=9,Health=100,LegacyState=14};s.World.Units.Add(a);s.World.BuildingResidents[b.Slot]=new(){a.Slot};}}s.World.Resources[owner]=0;s.World.Resources[4+owner]=0;return s;}
 public static void Run(string root){
  for(int owner=1;owner<=3;owner++){
   var s=Fixture(root,owner);int capacity=s.WarehouseSummary(owner).Capacity;Check(capacity>0,"fixture has active storage clan "+owner);
   s.World.Resources[owner]=(ushort)(capacity*7/10-1);Check((int)Call(s,"PolicyConstruction",owner)! !=3,"below 70% does not expand clan "+owner);
   s.World.Resources[owner]++;Check((int)Call(s,"PolicyConstruction",owner)! ==3,"exactly 70% prioritizes storage clan "+owner);
   s.World.Resources[owner]=20;Check((int)Call(s,"PolicyConstruction",owner)! ==3,"request persists after stock is spent clan "+owner);
   s=Fixture(root,owner);s.World.Resources[owner]=10;s.World.Resources[4+owner]=(ushort)(capacity*7/10-10);Check((int)Call(s,"PolicyConstruction",owner)! ==3,"stone also counts clan "+owner);
   s=Fixture(root,owner);s.World.Resources[owner]=10;s.World.Resources[8+owner]=500;Check((int)Call(s,"PolicyConstruction",owner)! !=3,"parts do not consume this warehouse capacity clan "+owner);
   s.World.Resources[owner]=0;s.World.Resources[4+owner]=(ushort)capacity;Check((int)Call(s,"PolicyConstruction",owner)! !=3,"starvation overrides expansion clan "+owner);
  }
  var f=Fixture(root);f.World.Resources[1]=(ushort)f.WarehouseSummary(1).Capacity;
  f.Constructions.Add(new Construction{Id=99010,Owner=1,Type=3,Cell=f.World.Buildings.First(b=>b.Owner==1).Cell+10});Check((int)Call(f,"PolicyConstruction",1)! !=3,"pending warehouse prevents duplicate request");
  f=Fixture(root);f.World.Resources[1]=(ushort)f.WarehouseSummary(1).Capacity;f.World.Buildings.Add(new Building(99011,3,1,f.World.Buildings.First(b=>b.Owner==1).Cell+10));Check((int)Call(f,"PolicyConstruction",1)! !=3,"empty completed warehouse is activated before another is requested");
  f=Fixture(root);int originalCapacity=f.WarehouseSummary(1).Capacity;f.World.Resources[1]=(ushort)(originalCapacity*7/10);var farmer=f.World.Units.First(a=>a.Owner==0&&a.Type==8);var home=f.World.Buildings.First(b=>b.Owner==1);farmer.X=(home.Cell&255)*1024;farmer.Y=(home.Cell>>8)*1024;Check((int)Call(f,"PolicyConstruction",1)! !=3,"base invasion postpones storage expansion");
  f=Fixture(root);for(int i=f.WarehouseSummary(1).Count;i<4;i++){var b=new Building(99020+i,3,1,f.World.Buildings.First(x=>x.Owner==1).Cell+10+i);f.World.Buildings.Add(b);var a=new Actor{Slot=99030+i,Owner=1,Type=9,Health=100,LegacyState=14};f.World.Units.Add(a);f.World.BuildingResidents[b.Slot]=new(){a.Slot};}f.World.Resources[1]=(ushort)(f.WarehouseSummary(1).Capacity*7/10);Check((int)Call(f,"PolicyConstruction",1)! ==3,"expansion is not capped at four warehouses");
  var live=OpponentV75Tests.Load(root);live.World.Resources[1]+=50;live.Tests.Food+=50;Array.Fill(live.Opponent.Explored[1],(byte)1);int oldActive=live.WarehouseSummary(1).ActiveCount;
  int reserved=(int)Call(live,"WarehouseOperatorSlot",1)!;Check(reserved>=0,"real opening reserves a colonist before storage construction");
  int food=live.World.Resources[1],rock=live.World.Resources[5];Call(live,"OpponentExpand",1);var site=live.Constructions.Single(c=>c.Owner==1&&c.Type==3&&!live.Complete(c));
  Check(live.World.Resources[1]==food-live.Rule(3).Food&&live.World.Resources[5]==rock-live.Rule(3).Rock,"real builder pays actual warehouse costs");
  Check((int)Call(live,"WarehouseOperatorSlot",1)! ==reserved,"operator remains reserved during construction");
  var saved=OpponentV75Tests.Load(root);saved.Restore(live.Serialize());Check(saved.Serialize()==live.Serialize()&&(int)Call(saved,"WarehouseOperatorSlot",1)! ==reserved,"pending expansion and operator survive save/load");
  for(int tick=0;tick<24000&&live.WarehouseSummary(1).ActiveCount==oldActive;tick++)live.Step();
  Check(live.Complete(site)&&live.World.Buildings.Any(b=>b.Slot==site.Id&&live.Inhabited(b)),"warehouse is completed and activated by a real colonist");
  Check(live.WarehouseSummary(1).Capacity>=100*(oldActive+1),"capacity increases only after activation");
  Check(live.Constructions.Count(c=>c.Owner==1&&c.Type==3)<=2,"expansion does not flood parallel warehouse orders");
  saved.Restore(live.Serialize());Check(saved.Serialize()==live.Serialize(),"completed staffed expansion saves and loads");
  for(int tick=0;tick<300;tick++){live.Step();saved.Step();}Check(saved.Serialize()==live.Serialize(),"expanded economy continues deterministically after loading");
  Console.WriteLine($"ALL v101 WAREHOUSE TESTS PASSED: {checks}");
 }
}
