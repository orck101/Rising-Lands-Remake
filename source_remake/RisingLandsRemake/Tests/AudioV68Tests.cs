using System.Text.Json;
namespace RisingLands;
public static class AudioV68Tests {
 public static bool Run(string root){
  var setup=ImprovementsV67Tests.Fixture(root);setup.World.Quantities[4+4*256]=30;foreach(var u in setup.World.Units){u.Satiety=u.SatietyMaximum;u.FatigueClock=0;}var s=new Simulation(setup.World,setup.Campaign);var a=s.World.Units.First(a=>a.Type==5&&a.Owner==0);var events=new List<GameSound>();s.SoundIssued+=events.Add;
  if(!s.Gather(a,4+4*256)||!s.SetFoodDestination(a,2,900))throw new Exception("Audio regression fixture cannot collect/deliver");
  int stock=s.World.Resources[0];for(int i=0;i<8000&&s.World.Resources[0]<stock+8;i++)s.Step();
  int responses=events.Count(e=>e.Source==a.Slot&&e.Channel==4&&(e.Key>>8&255)==15);int work=events.Count(e=>e.Source==a.Slot&&(e.Key>>8&255)==16);
  Console.WriteLine(JsonSerializer.Serialize(new{scenario="one collection order, thirty food, repeated warehouse deliveries",responses,work_events=work,delivered=s.World.Resources[0]-stock,ticks=s.Tick}));
  bool passed=responses==1&&s.World.Resources[0]>=stock+8&&work>=8;
  int checks=0;void Check(bool ok,string name){checks++;passed&=ok;Console.WriteLine((ok?"PASS: ":"FAIL: ")+name);}
  Check(passed,"one player order produces one acknowledgement across automatic collection cycles");
  Check(!AudioConcurrency.CanStart(new[]{new AudioActivity("P",0x50f00,1,4)},"P",0x70f00,2,4),"group orders cannot overlap different unit voices");
  Check(!AudioConcurrency.CanStart(new[]{new AudioActivity("P",0x50f00,1,4)},"P",0x50800,1,2),"busy unit cannot start a second action sound");
  Check(AudioConcurrency.CanStart(new[]{new AudioActivity("P",0x50f00,1,4)},"P",0x72500,2,2),"another worker can play its work effect during a voice");
  Check(!AudioConcurrency.CanStart(new[]{new AudioActivity("B",1,900,2)},"B",2,900,2),"building sounds cannot overlap on the same building");
  Check(!AudioConcurrency.CanStart(new[]{new AudioActivity("A",1,0,3)},"A",1,0,3),"duplicate active alerts are suppressed");
  Check(AudioConcurrency.CanStart(new[]{new AudioActivity("A",1,0,3)},"A",2,0,3),"different alerts remain available");
  int before=responses;Check(s.Gather(a,4+4*256),"a new explicit collection order is accepted");
  Check(events.Count(e=>e.Source==a.Slot&&e.Channel==4&&(e.Key>>8&255)==15)==before+1,"new explicit order acknowledges once after automatic work");
  var withdrawal=ImprovementsV67Tests.Fixture(root,false);var worker=withdrawal.World.Units.First(u=>u.Type==5&&u.Owner==0);worker.X=11*1024;worker.Y=13*1024;var withdrawalEvents=new List<GameSound>();withdrawal.SoundIssued+=withdrawalEvents.Add;
  bool ordered=withdrawal.GatherWarehouse(worker,900)&&withdrawal.SetFoodDestination(worker,1,8+5*256);
  for(int i=0;i<8000&&withdrawal.World.Quantities[8+5*256]<5;i++)withdrawal.Step();
  Console.WriteLine(JsonSerializer.Serialize(new{scenario="warehouse withdrawal",ordered,quantity=withdrawal.World.Quantities[8+5*256],worker.Food,worker.FoodJob,worker.RepeatFood,worker.X,worker.Y}));
  Check(ordered&&withdrawal.World.Quantities[8+5*256]>=5,"warehouse withdrawal repeats and delivers multiple loads");
  Check(withdrawalEvents.Count(e=>e.Source==worker.Slot&&e.Channel==4&&(e.Key>>8&255)==15)==1,"repeated warehouse withdrawals do not repeat acknowledgement");
  var mineWorld=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(mineWorld);mineWorld.Mines[0].Quantity=1;var probe=new Simulation(mineWorld);int anchor=mineWorld.Mines[0].Cell;
  int stone=Enumerable.Range(0,65536).Where(c=>Simulation.CellDistance(c,anchor)<=6&&probe.GroundResourceSpace(c,2)&&mineWorld.Objects[c]==0).OrderBy(c=>Simulation.CellDistance(c,anchor)).First();mineWorld.Objects[stone]=68;mineWorld.Quantities[stone]=3;
  var mining=new Simulation(mineWorld);var miner=mineWorld.Units.First(u=>u.Owner==0&&u.Type==7);int rockDrop=Enumerable.Range(0,65536).Where(c=>mining.GroundResourceSpace(c,2)&&mineWorld.Objects[c]==0&&Simulation.CellDistance(c,anchor)>8).OrderBy(c=>Simulation.CellDistance(c,anchor)).First();var mineEvents=new List<GameSound>();mining.SoundIssued+=mineEvents.Add;var orders=new WorkerResourceOrders();
  Check(orders.Source(mining,miner,anchor)&&orders.Destination(mining,miner,rockDrop),"miner receives a valid persistent collection order");
  for(int i=0;i<60000&&(mineWorld.Mines[0].Quantity!=0||mineWorld.Quantities[stone]!=0);i++)mining.Step();
  Check(mineWorld.Mines[0].Quantity==0&&mineWorld.Quantities[stone]==0,"miner exhausts mine and automatically collects nearby loose stone");
  Check(mineEvents.Count(e=>e.Source==miner.Slot&&e.Channel==4&&(e.Key>>8&255)==15)==1,"automatic mine-to-ground transition never repeats the player acknowledgement");
  Console.WriteLine($"AUDIO v68 CHECKS: {checks}; PASSED: {passed}");return passed;
 }
}
