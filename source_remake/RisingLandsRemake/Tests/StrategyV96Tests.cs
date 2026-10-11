using System.Reflection;
using System.Text.Json;
namespace RisingLands;
public static class StrategyV96Tests {
 static int checks;static void Check(bool ok,string label){if(!ok)throw new Exception("FAIL v96: "+label);checks++;Console.WriteLine("PASS v96: "+label);}
 static void Invoke(Simulation s,string method,int owner)=>typeof(Simulation).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(s,new object[]{owner});
 static void Time(Simulation s,long tick)=>typeof(Simulation).GetProperty("Tick")!.SetValue(s,tick);
 public static Simulation Load(string root,string map="MONDE04.ETA",bool army=false){
  var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/"+map));new VisualCatalog().Apply(w);Array.Fill(w.Collision,(byte)0);Array.Fill(w.Tiles,(ushort)349);Array.Clear(w.Objects);Array.Clear(w.Quantities);
  if(army){var template=w.Units.First(a=>Simulation.AdvancedAttacks.Rules.Any(r=>r.Type==a.Type));w.Units.RemoveAll(a=>a.Owner==1&&a.Type is 0 or 1 or 2 or 3 or 4 or 8 or 25);var home=w.Buildings.First(b=>b.Owner==1&&b.Type is 2 or 3);int next=w.Units.Max(a=>a.Slot)+1;
   for(int i=0;i<6;i++){var a=JsonSerializer.Deserialize<Actor>(JsonSerializer.Serialize(template))!;a.Slot=next++;a.Owner=1;a.Type=0;a.X=((home.Cell&255)+i)*1024;a.Y=((home.Cell>>8)+6)*1024;a.Health=template.Health;a.Satiety=4;a.Route.Clear();a.AttackTarget=-1;w.Units.Add(a);}
  }return new(w);
 }
 static (Actor Farmer,Building Hostile,int Food) HostileFood(Simulation s){var farmer=s.World.Units.First(a=>a.Owner==1&&a.Type==5&&a.Health>0);var enemy=s.World.Buildings.First(b=>b.Owner==0&&b.Type==2);
  int hostile=Enumerable.Range(0,65536).Where(c=>s.GroundResourceSpace(c,1)).OrderBy(c=>Math.Abs((c&255)-(enemy.Cell&255))+Math.Abs((c>>8)-(enemy.Cell>>8))).First();
  s.World.Objects[hostile]=67;s.World.Quantities[hostile]=10;s.Opponent.Explored[1][enemy.Cell]=s.Opponent.Explored[1][hostile]=1;return(farmer,enemy,hostile);
 }
 public static void Run(string root){
  var s=Load(root);var(farmer,enemy,hostile)=HostileFood(s);
  Check(!s.Gather(farmer,hostile,false),"enemy farmer rejects food beside a known hostile base");Check(!s.Order(farmer,hostile&255,hostile>>8,true),"civilian path cannot enter hostile base buffer");
  var player=s.World.Units.First(a=>a.Owner==0&&a.Type==5);Check(s.Gather(player,hostile,false),"player resource controls are unaffected");
  var home=s.World.Buildings.First(b=>b.Owner==1&&b.Type==3);int local=s.WarehouseOverflowCells(home).First(c=>s.GroundResourceSpace(c,1)&&s.OpponentSafeCell(1,c));s.World.Objects[local]=67;s.World.Quantities[local]=20;s.Opponent.Explored[1][local]=1;
  Check(s.Gather(farmer,local,false)&&farmer.Route.All(c=>s.OpponentSafeCell(1,c)),"nearby safe food remains accessible through safe path");
  s=Load(root);(farmer,enemy,hostile)=HostileFood(s);home=s.World.Buildings.First(b=>b.Owner==1&&b.Type==3);local=s.WarehouseOverflowCells(home).First(c=>s.GroundResourceSpace(c,1)&&s.OpponentSafeCell(1,c));s.World.Objects[local]=67;s.World.Quantities[local]=20;s.Opponent.Explored[1][local]=1;
  farmer.Satiety=0;s.World.Resources[1]=0;for(int i=0;i<60;i++)s.Step();Check(farmer.FoodSource!=hostile&&farmer.Route.All(c=>s.OpponentSafeCell(1,c)),"hunger searches safe local food rather than hostile stockpile");
  Check(farmer.FoodSource==local||farmer.Satiety>0,"hungry farmer actually reaches safe food");
  s=Load(root);(farmer,enemy,hostile)=HostileFood(s);foreach(var a in s.World.Units.Where(a=>a.Owner==0&&Simulation.AdvancedAttacks.Rules.Any(r=>r.Type==a.Type)))a.Health=0;
  farmer.X=(hostile&255)*1024;farmer.Y=(hostile>>8)*1024;farmer.Food=1;farmer.CargoKind=1;for(int i=0;i<30;i++)s.Step();Check(farmer.Food==1&&farmer.Route.Count>0&&s.OpponentSafeCell(1,farmer.Route[^1]),"endangered worker retreats without losing carried food");
  s=Load(root);foreach(var a in s.World.Units.Where(a=>a.Owner==1&&a.Type==5))a.Health=0;for(int i=0;i<300;i++)s.Step();Check(s.Recruitments.Any(j=>j.Owner==1&&j.Type==5),"lost farmers are replaced before first-deposit gate");
  s=Load(root);foreach(var a in s.World.Units.Where(a=>a.Owner==1&&a.Type==7))a.Health=0;for(int i=0;i<300;i++)s.Step();Check(s.Recruitments.Any(j=>j.Owner==1&&j.Type==7),"lost builder is replaced using real residents and stocks");
  s=Load(root,"MONDE05.ETA",true);Invoke(s,"PrepareOpponentWave",1);Check(s.Opponent.Sorties.Count==0,"first three minutes remain peaceful");Time(s,6000);Invoke(s,"PrepareOpponentWave",1);var army=s.World.Units.Where(a=>a.Owner==1&&a.Health>0&&a.Type==0).ToArray();Check(s.Opponent.Sorties.Count<=2&&army.Count(a=>!s.Opponent.Sorties.ContainsKey(a.Slot))>=3,"early small wave leaves three guards behind");
  s=Load(root,"MONDE05.ETA",true);Time(s,9001);Invoke(s,"PrepareOpponentWave",1);army=s.World.Units.Where(a=>a.Owner==1&&a.Health>0&&a.Type==0).ToArray();Check(s.Opponent.Sorties.Count>0&&army.Count(a=>!s.Opponent.Sorties.ContainsKey(a.Slot))>=3,"developed army attacks without emptying garrison");
  var wave=army.Where(a=>s.Opponent.Sorties.ContainsKey(a.Slot)).ToArray();wave[0].Health=1;Invoke(s,"AssessOpponentRetreat",1);Check(!s.Opponent.Sorties.ContainsKey(wave[0].Slot),"badly wounded attacker is recalled");
  s=Load(root,"MONDE05.ETA",true);Time(s,9001);army=s.World.Units.Where(a=>a.Owner==1&&a.Health>0&&a.Type==0).ToArray();foreach(var a in army.Skip(3))a.Health=0;Invoke(s,"PrepareOpponentWave",1);Check(s.Opponent.Sorties.Count==0,"three remaining soldiers defend instead of launching suicidal attack");
  s=Load(root,"MONDE05.ETA",true);Time(s,9001);Invoke(s,"PrepareOpponentWave",1);army=s.World.Units.Where(a=>a.Owner==1&&a.Health>0&&a.Type==0).ToArray();wave=army.Where(a=>s.Opponent.Sorties.ContainsKey(a.Slot)).ToArray();foreach(var a in wave){a.X=40*1024;a.Y=40*1024;}
  var hostileTemplate=s.World.Units.First(a=>a.Owner==0&&Simulation.AdvancedAttacks.Rules.Any(r=>r.Type==a.Type));int next=s.World.Units.Max(a=>a.Slot)+1;
  for(int i=0;i<12;i++){var foe=JsonSerializer.Deserialize<Actor>(JsonSerializer.Serialize(hostileTemplate))!;foe.Slot=next++;foe.Owner=0;foe.Type=0;foe.LegacyState=7;foe.Health=100;foe.X=(42+i%2)*1024;foe.Y=(40+i/2)*1024;s.World.Units.Add(foe);}
  Invoke(s,"AssessOpponentRetreat",1);Check(wave.All(a=>!s.Opponent.Sorties.ContainsKey(a.Slot)),"attack group withdraws from visibly superior force");
  s=Load(root);(farmer,enemy,hostile)=HostileFood(s);var builder=s.World.Units.First(a=>a.Owner==1&&a.Type==7&&a.Health>0);s.World.Mines.Add(new(){Cell=hostile,Quantity=20});Check(!s.StartMining(builder,s.World.Mines.Count-1,false),"builder rejects mine in hostile base buffer");
  s=Load(root);farmer=s.World.Units.First(a=>a.Owner==1&&a.Type==5&&a.Health>0);home=s.World.Buildings.First(b=>b.Owner==1&&b.Type==3);int hx=home.Cell&255,hy=home.Cell>>8,side=hx+45<s.World.Width?1:-1,lineY=Math.Min(s.World.Height-8,hy+10),barrier=lineY*256+hx+side*25,goal=lineY*256+hx+side*45;
  s.World.Buildings.Add(new(990,3,0,barrier));s.Opponent.Explored[1][barrier]=1;farmer.X=(hx+side*4)*1024;farmer.Y=lineY*1024;
  Check(s.Order(farmer,goal&255,goal>>8,true)&&farmer.Route.All(c=>s.OpponentSafeCell(1,c)),"worker detours around hostile area even when destination itself is safe");
  s=OpponentV75Tests.Load(root);int maxWorkers=0,minWorkers=100;for(int i=0;i<120000;i++){s.Step();int workers=s.World.Units.Count(a=>a.Owner==1&&a.Health>0&&a.Type is 5 or 7);maxWorkers=Math.Max(maxWorkers,workers);minWorkers=Math.Min(minWorkers,workers);if(i%12000==0){_=s.OrderPreview();Console.WriteLine($"ECONOMY tick={s.Tick} workers={workers} army={s.OpponentArmyCount(1)} sorties={s.Opponent.Sorties.Count} deposits={s.Opponent.Deposited[1]}/{s.Opponent.Deposited[5]}");}}
  Check(minWorkers>=3&&s.Opponent.Deposited[1]>0&&s.Opponent.Deposited[5]>0,"mission four preserves workers and real economy for 66 simulated minutes");
  Check(s.Constructions.Where(c=>c.Owner==1).Take(2).Select(c=>c.Type).SequenceEqual(new[]{16,8})&&s.World.Buildings.Any(b=>b.Owner==1&&b.Type==1),"market then exchange, followed by barracks");
  Check(s.OpponentArmyCount(1)>=3,"economy builds at least three actual defenders");var copy=OpponentV75Tests.Load(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"strategy saves without changing resource conservation");
  Console.WriteLine($"ALL v96 STRATEGY TESTS PASSED: {checks}, workers {minWorkers}..{maxWorkers}, army {s.OpponentArmyCount(1)}");
 }
}
