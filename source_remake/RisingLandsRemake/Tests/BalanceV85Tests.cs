using System.Text.Json.Nodes;
namespace RisingLands;
public static class BalanceV85Tests {
 public static void Run(string root){int count=0;void Check(bool ok,string name){if(!ok)throw new Exception("FAIL v85: "+name);count++;Console.WriteLine("PASS v85: "+name);}
  var s=OpponentV75Tests.Load(root);var cart=s.World.Units.Single(a=>a.Owner==1&&a.Type==13);
  Check(cart.Health==0&&!VisualCatalog.Visible(cart),"enemy starting cart removed from play");
  Check(!s.OpponentUnitUnlocked(1,13),"initial technology still forbids new carts");
  Check(s.World.Units.Count(a=>a.Owner==1&&a.Type==5&&a.Health>0)==3,"no replacement farmers or free resources added");
  var copy=OpponentV75Tests.Load(root);copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"retired native slot preserves save compatibility");
  var old=JsonNode.Parse(s.Serialize())!;old["Units"]!.AsArray().Single(n=>n!["Slot"]!.GetValue<int>()==cart.Slot)!["Health"]=100;
  // Match actual native health, making this a valid previous-version live cart.
  var raw=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE04.ETA"));old["Units"]!.AsArray().Single(n=>n!["Slot"]!.GetValue<int>()==cart.Slot)!["Health"]=raw.Units.Single(a=>a.Slot==cart.Slot).Health;
  copy.Restore(old.ToJsonString());Check(!copy.World.Units.Any(a=>a.Owner==1&&a.Type==13&&a.Health>0),"loading an older save cannot revive the starting cart");
  copy.Restart();Check(!copy.World.Units.Any(a=>a.Owner==1&&a.Type==13&&a.Health>0),"restart keeps starting cart removed");
  for(int i=0;i<3000;i++)s.Step();Check(s.Opponent.Deposited[1]>0,"ordinary farmers sustain real food deliveries");
  copy.Restore(s.Serialize());Check(copy.Serialize()==s.Serialize(),"economy without starting cart remains conserved");
  Console.WriteLine("PASS v85 total "+count);
 }
}
