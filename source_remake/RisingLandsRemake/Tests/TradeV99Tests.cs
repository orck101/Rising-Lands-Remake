namespace RisingLands;
public static class TradeV99Tests {
 static int checks;
 static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL v99: "+name);checks++;Console.WriteLine("PASS v99: "+name);}
 public static void Run(string root){
  var repeat=new HeldQuantityControl();
  Check(repeat.Step(true,1,0,true)==1,"press adds exactly one immediately");
  Check(repeat.Step(true,1,.2,false)==0,"short click does not repeat prematurely");
  Check(repeat.Step(true,1,.35,false)==1&&repeat.Step(true,1,.42,false)==1,"held click repeats with real-time cadence");
  Check(repeat.Step(false,1,.6,false)==0&&repeat.Step(true,1,1,false)==0,"release stops and requires a fresh press");
  repeat.Reset();repeat.Step(true,1,1,true);Check(repeat.Step(true,0,1.5,false)==0&&repeat.Step(true,1,2,false)==0,"leaving button stops repetition without restarting on hover");
  Check(repeat.Step(true,-1,3,true)==-1&&repeat.Step(true,-1,3.35,false)==-1,"decrement uses the same held-click control");
  var s=TradeV97Tests.Fixture(root);var messenger=SpecialV66Tests.Recruit(s,10);var market=s.World.Buildings.First(b=>b.Owner==1&&b.Type==16);
  Check(s.SendMessenger(messenger,market.Slot),"qualified messenger approaches market");
  for(int i=0;i<20000&&!s.TradeEntering(messenger);i++)s.Step();
  Check(s.TradeEntering(messenger)&&VisualCatalog.Visible(messenger),"messenger remains visible while entering the door");
  var before=s.Serialize();var copy=TradeV97Tests.Fixture(root);copy.Restore(before);Check(copy.Serialize()==before,"entry animation survives save/load");
  int y=messenger.Y;for(int i=0;i<100&&s.NegotiationFor(1)?.Stage=="Travelling";i++){s.Step();copy.Step();}
  Check(s.NegotiationFor(1)?.Stage=="Ready"&&messenger.Y<y&&!VisualCatalog.Visible(messenger),"messenger walks inward before becoming hidden");
  Check(copy.Serialize()==s.Serialize(),"entry resumes deterministically");
  var n=s.NegotiationFor(1)!;var worker=s.TradeUnits(0).First(a=>a.Type==7);var enemy=s.TradeUnits(1).First();
  Check(s.SubmitTrade(n,new[]{3,0,0},new int[3],new[]{worker.Slot},new[]{enemy.Slot}),"troop trade submitted");
  for(int i=0;i<20000&&n.Stage!="Delivery";i++)s.Step();Check(n.Stage=="Delivery"&&s.StoreTradeUnit(worker,s.Exchange(0)!),"own troop receives Exchange entry order");
  for(int i=0;i<20000&&!s.TradeEntering(worker)&&n.Active;i++)s.Step();Check(s.TradeEntering(worker)&&VisualCatalog.Visible(worker)&&!n.Stored.Contains(worker.Slot),"troop still visible and untransferred during entry");
  before=s.Serialize();copy=TradeV97Tests.Fixture(root);copy.Restore(before);Check(copy.Serialize()==before,"troop entrance survives save/load");
  for(int i=0;i<20000&&n.Active;i++)s.Step();Check(n.Stage=="Completed"&&worker.Owner==1&&VisualCatalog.Visible(worker),"entry completes before ownership transfer and exit");
  Console.WriteLine("ALL v99 TESTS PASSED: "+checks);
 }
}
