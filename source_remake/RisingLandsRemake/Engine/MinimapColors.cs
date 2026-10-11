using Microsoft.Xna.Framework;
namespace RisingLands;

public static class MinimapColors {
 // Owner identity is unchanged by alliance, selection or construction progress.
 public static Color ForOwner(int owner,bool remembered=false){
  var color=owner switch {0=>Color.CornflowerBlue,1=>Color.IndianRed,2=>Color.Yellow,3=>Color.LightGreen,_=>Color.Gray};
  return remembered?new Color((byte)(color.R/2),(byte)(color.G/2),(byte)(color.B/2),(byte)255):color;
 }
}
