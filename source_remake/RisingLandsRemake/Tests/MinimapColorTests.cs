using Microsoft.Xna.Framework;
using System.Text.Json;
namespace RisingLands;
public static class MinimapColorTests {
 public static void Run(string root,string output){
  int checks=0;
  void Check(bool ok,string name){if(!ok)throw new InvalidDataException(name);checks++;Console.WriteLine("PASS minimap: "+name);}
  var expected=new[]{Color.CornflowerBlue,Color.IndianRed,Color.Yellow,Color.LightGreen};
  for(int owner=0;owner<4;owner++){
   var live=MinimapColors.ForOwner(owner);var memory=MinimapColors.ForOwner(owner,true);
   Check(live==expected[owner],"owner "+owner+" has its own live color");
   Check(memory.R==live.R/2&&memory.G==live.G/2&&memory.B==live.B/2&&memory.A==255,"owner "+owner+" memory darkens without transparency or color substitution");
  }
  Check(expected.Distinct().Count()==4,"four distinct owner identities");
  Check(MinimapColors.ForOwner(-1)==Color.Gray&&MinimapColors.ForOwner(4)==Color.Gray,"unknown owner is neutral, never another clan");
  var palette=LegacyAssets.Palette(PackageLayout.Asset(root,"PLATGAME.PIE"));
  Check(palette[164].B>palette[164].R&&palette[144].R>palette[144].G&&palette[136].R>palette[136].B&&palette[136].G>palette[136].B&&palette[48].G>palette[48].R,"identities agree with sprite palette blue/red/yellow/green");
  File.WriteAllText(output,JsonSerializer.Serialize(new{Checks=checks,Passed=true,Colors=expected.Select((c,owner)=>new{Owner=owner,c.R,c.G,c.B,c.A}),Limit="Color/render contract; no manual gameplay"}));
 }
}
