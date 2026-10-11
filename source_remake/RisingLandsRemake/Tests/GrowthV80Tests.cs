namespace RisingLands;
public static class GrowthV80Tests {
 public static void Run(string root){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE04.ETA"));new VisualCatalog().Apply(w);var s=new Simulation(w);int checks=0;void Check(bool ok,string name){if(!ok)throw new Exception("FAIL v80: "+name);checks++;Console.WriteLine("PASS v80: "+name);}
 int Trained()=>s.Recruitments.Count(j=>j.Owner==1&&j.Finished&&j.Type is not (5 or 6 or 7));
 for(int i=0;i<6*60*30;i++){s.Step();if(i%1800==1799)Console.WriteLine("TRACE v80 minute "+s.Tick/1800+" trained="+Trained());}int atSix=Trained();for(int i=0;i<6*60*30;i++){s.Step();if(i%1800==1799)Console.WriteLine("TRACE v80 minute "+s.Tick/1800+" trained="+Trained());}
 Check(Trained()>atSix,"real enemy keeps training military between minutes six and twelve");
 Check(w.Units.Count(a=>a.Owner==1&&a.Type==5&&a.Health>0)>=6,"enemy supports its economy with additional real farmers");
 Check(s.Cultures.Where(c=>w.Buildings.Any(b=>b.Slot==c.Building&&b.Owner==1)).Sum(c=>c.Born)>=3,"food deliveries sustain repeated colonist births");
 int atTwelve=Trained();for(int i=0;i<12*60*30;i++){s.Step();if(i%1800==1799)Console.WriteLine("TRACE v80 minute "+s.Tick/1800+" trained="+Trained());}Check(Trained()>atTwelve,"military development continues between minutes twelve and twenty-four");
 var copyWorld=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE04.ETA"));new VisualCatalog().Apply(copyWorld);var copy=new Simulation(copyWorld);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"twenty-four-minute economy remains conserved and loadable");for(int i=0;i<90;i++){s.Step();copy.Step();}Check(copy.Serialize()==s.Serialize(),"reinforcement economy continues deterministically after load");
 Console.WriteLine("PASS v80 total "+checks+" trained6="+atSix+" trained12="+atTwelve+" trained24="+Trained());
 }
}
