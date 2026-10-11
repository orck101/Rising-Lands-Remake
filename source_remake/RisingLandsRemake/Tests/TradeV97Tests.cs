namespace RisingLands;
public static class TradeV97Tests {
 static int checks;
 static void Check(bool ok,string label){checks++;if(!ok)throw new Exception("FAIL v97: "+label);Console.WriteLine("PASS v97: "+label);}
 public static Simulation Fixture(string root){var s=SpecialV66Tests.Load(root,true);var bag=s.Exchange(1)!;foreach(var a in s.World.Units.Where(a=>a.Owner==1&&VisualCatalog.Visible(a)))s.PlaceAtExit(a,bag,s.BuildingExit(bag));ProductionV65Tests.Staff(s,16);if(!s.World.Buildings.Any(b=>b.Owner==0&&b.Type==3))ProductionV65Tests.Staff(s,3);return s;}
 static void Until(Simulation s,Func<bool> done){for(int i=0;i<20000&&!done();i++)s.Step();Check(done(),"negotiation reaches expected stage within time limit");}
 public static void Run(string root){
  var s=Fixture(root);var messenger=SpecialV66Tests.Recruit(s,10);var market=s.World.Buildings.First(b=>b.Type==16&&b.Owner==1);
  Check(s.SendMessenger(messenger,market.Slot),"foreign market accepts qualified messenger");
  Check(messenger.Route.Count>0,"entry order preserves navigable route instead of clearing it");
  var pending=s.Serialize();var copy=Fixture(root);copy.Restore(pending);Check(copy.Serialize()==pending,"actual approach path survives save/load");
  Until(s,()=>s.NegotiationFor(1)?.Stage=="Ready");var n=s.NegotiationFor(1)!;
  Check(!VisualCatalog.Visible(messenger),"messenger enters through the market door");
  long[] totals=Enumerable.Range(0,3).Select(k=>Enumerable.Range(0,4).Sum(o=>(long)s.World.Resources[k*4+o])).ToArray();
  Check(s.SubmitTrade(n,new[]{10,0,0},new[]{0,8,0}),"material offer accepted for evaluation");Until(s,()=>!n.Active);
  Check(n.Stage=="Completed"&&Enumerable.Range(0,3).All(k=>Enumerable.Range(0,4).Sum(o=>(long)s.World.Resources[k*4+o])==totals[k]),"direct material exchange conserves resources without a vehicle");
  s=Fixture(root);messenger=SpecialV66Tests.Recruit(s,10);market=s.World.Buildings.First(b=>b.Type==16&&b.Owner==1);var bag=s.Exchange(1)!;s.World.Buildings.Remove(bag);pending=s.Serialize();
  Check(s.MessengerEntryError(messenger,market.Slot)?.Contains("Bolsa")??false,"missing foreign Exchange gives a specific reason");Check(!s.SendMessenger(messenger,market.Slot)&&s.Serialize()==pending,"failed prerequisite never changes orders or stocks");s.World.Buildings.Add(bag);
  Check(s.SendMessenger(messenger,market.Slot),"adding missing Exchange enables entry");Until(s,()=>s.NegotiationFor(1)?.Stage=="Ready");n=s.NegotiationFor(1)!;
  var worker=s.TradeUnits(0).First(a=>a.Type==7);var enemy=s.TradeUnits(1).First();Check(s.SetFoodDestination(worker,2,s.World.Buildings.First(b=>b.Owner==0&&b.Type==3).Slot),"trade worker has a genuine previous warehouse delivery order");
  Check(s.SubmitTrade(n,new[]{3,0,0},new int[3],new[]{worker.Slot},new[]{enemy.Slot}),"bilateral troop agreement submitted");Until(s,()=>n.Stage=="Delivery");
  Check(s.StoreTradeUnit(worker,s.Exchange(0)!)&&worker.Route.Count>0,"own troop keeps its route to the Exchange");pending=s.Serialize();copy=Fixture(root);copy.Restore(pending);Check(copy.Serialize()==pending,"both troop deliveries survive save/load");Until(s,()=>!n.Active);
  Check(n.Stage=="Completed"&&worker.Owner==1&&enemy.Owner==0&&VisualCatalog.Visible(worker)&&VisualCatalog.Visible(enemy),"both units change ownership and emerge from their new Exchanges");
  pending=s.Serialize();copy=Fixture(root);copy.Restore(pending);Check(copy.Serialize()==pending,"completed troop transfer remains loadable");
  s=Fixture(root);messenger=SpecialV66Tests.Recruit(s,10);market=s.World.Buildings.First(b=>b.Type==16&&b.Owner==1);Check(s.SendMessenger(messenger,market.Slot),"incoming policy fixture starts");Until(s,()=>s.NegotiationFor(1)?.Stage=="Ready");n=s.NegotiationFor(1)!;n.Incoming=true;n.Stage="Incoming";worker=s.TradeUnits(0).First(a=>a.Type==7);n.GiveUnits.Add(worker.Slot);pending=s.Serialize();
  Check(!s.SubmitTrade(n,new int[3],new[]{0,1,0})&&s.Serialize()==pending,"incoming requested troops cannot be silently removed");
  Check(s.SubmitTrade(n,new int[3],new[]{0,1,0},new[]{worker.Slot}),"player may choose counter-demand while preserving the incoming request");
  Console.WriteLine("ALL TRADE v97 TESTS PASSED: "+checks);
 }
}
