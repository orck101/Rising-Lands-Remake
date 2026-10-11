namespace RisingLands;
public static class TradeV100Tests {
 static int checks;
 static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL v100: "+name);checks++;Console.WriteLine("PASS v100: "+name);}
 static void Until(Simulation s,Func<bool> done){for(int i=0;i<20000&&!done();i++)s.Step();Check(done(),"operation finishes within its time limit");}
 public static void Run(string root){
  var s=TradeV97Tests.Fixture(root);var worker=s.World.Units.First(a=>a.Owner==0&&a.Type==7&&VisualCatalog.Visible(a));
  Check(s.StoreTradeUnit(worker,s.Exchange(0)!),"troop can enter the Exchange before any agreement exists");
  Until(s,()=>!VisualCatalog.Visible(worker));
  for(int i=0;i<2000;i++)s.Step();
  Check(worker.Owner==0&&worker.Health>0&&!VisualCatalog.Visible(worker),"stored troop waits without reverting or disappearing");
  var copy=TradeV97Tests.Fixture(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"waiting troops survive save/load");
  var released=s.ReleaseExchangeUnit(s.Exchange(0)!.Slot);Check(released==worker&&VisualCatalog.Visible(worker)&&worker.Type==7,"waiting unit exits unchanged and without automatic selection");
  foreach(int type in Enumerable.Range(0,18).Append(25).Where(t=>t!=9)){worker.Type=type;Check(Simulation.UnitExchangeable(type)&&s.CanStoreTradeUnit(worker,s.Exchange(0)!),"Exchange accepts class "+type);}worker.Type=7;
  Check(!s.StoreTradeUnit(worker,s.Exchange(1)!),"foreign Exchange rejects an own troop");
  var alternative=SpecialV66Tests.Recruit(s,7);var extra=s.World.Units.First(a=>a.Owner==0&&a.Type==5&&VisualCatalog.Visible(a));
  Check(s.StoreTradeUnit(extra,s.Exchange(0)!),"unrelated class may wait in the Exchange");Until(s,()=>!VisualCatalog.Visible(extra));
  var messenger=SpecialV66Tests.Recruit(s,10);var market=s.World.Buildings.First(b=>b.Type==16&&b.Owner==1);
  Check(s.SendMessenger(messenger,market.Slot),"messenger starts troop agreement");Until(s,()=>s.NegotiationFor(1)?.Stage=="Ready");var n=s.NegotiationFor(1)!;
  var enemy=s.TradeUnits(1).First();Check(s.SubmitTrade(n,new[]{3,0,0},new int[3],new[]{worker.Slot},new[]{enemy.Slot}),"agreement records the requested class and count");Until(s,()=>n.Stage=="Delivery");
  for(int i=0;i<2500;i++)s.Step();Check(n.Stage=="Delivery"&&worker.Owner==0&&enemy.Owner==1,"wrong class and elapsed time do not complete or cancel the agreement");
  Check(s.StoreTradeUnit(alternative,s.Exchange(0)!),"another unit of the promised class can fulfill delivery");
  Until(s,()=>n.Stage=="Completed");Check(alternative.Owner==1&&worker.Owner==0&&VisualCatalog.Visible(worker)&&enemy.Owner==0,"matching uses class, transfers the substitute, and preserves original unit");
  Check(extra.Owner==0&&!VisualCatalog.Visible(extra)&&s.ExchangeOccupants(s.Exchange(0)!.Slot).Contains(extra),"extra waiting troop remains inside after settlement");
  copy=TradeV97Tests.Fixture(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"completed transfer and extra waiting troop roundtrip");
  var totals=s.World.Resources.ToArray();int exchanges=s.Special.UnitExchanges.Count;for(int i=0;i<60;i++)s.Step();Check(exchanges==s.Special.UnitExchanges.Count,"completed agreement never transfers twice");
  messenger=SpecialV66Tests.Recruit(s,10);Check(s.SendMessenger(messenger,market.Slot),"next agreement can start");Until(s,()=>s.NegotiationFor(1)?.Stage=="Ready");n=s.NegotiationFor(1)!;
  Check(s.TradeUnits(0).Contains(extra)&&s.SubmitTrade(n,new int[3],new int[3],new[]{extra.Slot}),"troops already stored can be offered in the market");Until(s,()=>n.Stage=="Completed");Check(extra.Owner==1&&VisualCatalog.Visible(extra),"pre-stored troop settles automatically upon approval");
  s=TradeV97Tests.Fixture(root);var bag=s.Exchange(0)!;var home=s.World.Buildings.First(b=>b.Owner==0&&b.Type==2);
  var troops=s.World.Units.Where(a=>a.Owner==0&&VisualCatalog.Visible(a)&&Simulation.UnitExchangeable(a.Type)).Take(8).ToList();while(troops.Count<8)troops.Add(SpecialV66Tests.Recruit(s,7));
  Check(troops.All(a=>s.StoreTradeUnit(a,bag)),"group reserves all eight Exchange places");var ninth=SpecialV66Tests.Recruit(s,7);Check(!s.StoreTradeUnit(ninth,bag)&&s.ExchangeReserved(bag.Slot)==8,"in-flight entries cannot overbook the Exchange");Until(s,()=>s.ExchangeOccupants(bag.Slot).Count()==8);
  string before=s.Serialize();copy=TradeV97Tests.Fixture(root);copy.Restore(before);Check(copy.Serialize()==before,"mixed-class full Exchange survives save/load");
  var invalid=System.Text.Json.Nodes.JsonNode.Parse(before)!;invalid["Special"]!["ExchangeResidents"]!.AsArray().Add(invalid["Special"]!["ExchangeResidents"]![0]!.DeepClone());try{s.Restore(invalid.ToJsonString());throw new Exception("duplicate resident accepted");}catch(InvalidDataException){}Check(s.Serialize()==before,"duplicate storage identity rejected atomically");
  var outUnit=s.ReleaseExchangeUnit(bag.Slot);Check(outUnit!=null&&s.StoreTradeUnit(ninth,bag),"releasing a troop frees capacity immediately");
  var marching=s.Special.ExchangeResidents.Single(r=>r.Slot==ninth.Slot);s.Stop(ninth);Check(!s.Special.ExchangeResidents.Contains(marching)&&!s.TradeEntering(ninth),"changing an entry order cancels the reservation");
  s=TradeV97Tests.Fixture(root);var builder1=s.World.Units.First(a=>a.Type==7&&a.Owner==0&&VisualCatalog.Visible(a));var builder2=SpecialV66Tests.Recruit(s,7);var farmer=s.World.Units.First(a=>a.Type==5&&a.Owner==0&&VisualCatalog.Visible(a));
  messenger=SpecialV66Tests.Recruit(s,10);market=s.World.Buildings.First(b=>b.Type==16&&b.Owner==1);s.SendMessenger(messenger,market.Slot);Until(s,()=>s.NegotiationFor(1)?.Stage=="Ready");n=s.NegotiationFor(1)!;
  Check(s.SubmitTrade(n,new int[3],new int[3],new[]{builder1.Slot,builder2.Slot,farmer.Slot}),"agreement may demand multiple units of one class and another class");Until(s,()=>n.Stage=="Delivery");s.StoreTradeUnit(builder1,s.Exchange(0)!);s.StoreTradeUnit(farmer,s.Exchange(0)!);Until(s,()=>s.ExchangeDelivered(n,0)==2);
  Check(n.Stage=="Delivery"&&builder1.Owner==0&&farmer.Owner==0,"partial correct delivery never transfers units early");copy=TradeV97Tests.Fixture(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"partial class-count delivery survives save/load");
  s.StoreTradeUnit(builder2,s.Exchange(0)!);Until(s,()=>n.Stage=="Completed");Check(new[]{builder1,builder2,farmer}.All(a=>a.Owner==1),"exact required class counts settle as one transfer");
  s=TradeV97Tests.Fixture(root);messenger=SpecialV66Tests.Recruit(s,10);worker=s.World.Units.First(a=>a.Owner==0&&a.Type==7&&VisualCatalog.Visible(a));s.StoreTradeUnit(worker,s.Exchange(0)!);Until(s,()=>!VisualCatalog.Visible(worker));market=s.World.Buildings.First(b=>b.Owner==1&&b.Type==16);s.SendMessenger(messenger,market.Slot);Until(s,()=>s.NegotiationFor(1)?.Stage=="Ready");n=s.NegotiationFor(1)!;s.SubmitTrade(n,new int[3],new int[3],new[]{worker.Slot});s.CancelNegotiation(n);Check(worker.Owner==0&&!VisualCatalog.Visible(worker)&&s.ExchangeOccupants(s.Exchange(0)!.Slot).Contains(worker),"cancelling an agreement preserves independently stored troops");
  s=TradeV97Tests.Fixture(root);var aircraft=SpecialV66Tests.Recruit(s,16);bag=s.Exchange(0)!;Check(s.StoreTradeUnit(aircraft,bag),"real aircraft receives an Exchange entry route");Until(s,()=>!VisualCatalog.Visible(aircraft));copy=TradeV97Tests.Fixture(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"stored aircraft survives save/load");Check(s.ReleaseExchangeUnit(bag.Slot)==aircraft&&VisualCatalog.Visible(aircraft)&&Simulation.Airborne(aircraft),"aircraft exits with its original class intact");
  worker=s.World.Units.First(a=>a.Owner==0&&a.Type==7&&VisualCatalog.Visible(a));s.StoreTradeUnit(worker,bag);Until(s,()=>!VisualCatalog.Visible(worker));typeof(Simulation).GetMethod("HitStructure",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(s,new object[]{bag,s.BuildingMaxHealth(bag)});Check(worker.Health==0&&s.ExchangeReserved(bag.Slot)==0,"destroying the Exchange clears stored units without orphaned hidden state");copy=TradeV97Tests.Fixture(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"destroyed Exchange state remains loadable");
  Console.WriteLine("ALL v100 TESTS PASSED: "+checks);
 }
}

