using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareTradeV99Capture(){
  if(!showMini)throw new InvalidDataException("Minimap must be enabled at startup");
  showMini=false;PrepareTradeV97Capture();
  if(!showMini)throw new InvalidDataException("Loading a match must restore default minimap");
  var n=MarketSession!;marketQuantitySide=0;marketQuantityKind=0;marketQuantity=0;marketQuantityRepeat.Reset();
  var up=MarketRect(249,43,22,26);var down=new MouseState(up.Center.X,up.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released);
  oldMouse=default;inventionClock=10;UpdateMarket(default,down);
  if(marketQuantity!=1)throw new InvalidDataException("First increment click must add one");
  oldMouse=down;inventionClock=10.5;UpdateMarket(default,down);
  if(marketQuantity!=4)throw new InvalidDataException("Held UI button must repeat");
  var released=new MouseState(up.Center.X,up.Center.Y,0,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released);
  UpdateMarket(default,released);oldMouse=released;inventionClock=11;UpdateMarket(default,released);
  if(marketQuantity!=4)throw new InvalidDataException("Release must stop UI increment");
  marketQuantity=simulation.World.Resources[0];oldMouse=default;UpdateMarket(default,down);
  if(marketQuantity!=simulation.World.Resources[0])throw new InvalidDataException("UI quantity must respect stock limit");
  marketQuantity=4;marketQuantityRepeat.Reset();oldMouse=released;
  Console.WriteLine("PASS v99 UI: startup/load minimap, press/hold/release quantities and stock limit");
 }
}
