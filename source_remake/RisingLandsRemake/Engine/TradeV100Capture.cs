using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareTradeV100Capture(){
  var s=TradeV97Tests.Fixture(root);LoadMap(restored:s);paused=true;revealMap=true;pauseMenu.Show(MenuPage.Closed);researchMovies.Clear();CloseResearchMovie();
  var bag=s.Exchange(0)!;var commands=s.Rule(bag.Type).Objects.SelectMany(o=>o.Commands).Select(c=>c with{X=c.X+(bag.Cell&255)*32,Y=c.Y+(bag.Cell>>8)*32}).ToList();scene.Objects.Add(new(bag.Cell,commands));
  camera=new Vector2((bag.Cell&255)*32-ViewWidth/2f,(bag.Cell>>8)*32-ViewHeight/2f);ClampCamera();
  var workers=s.World.Units.Where(a=>a.Owner==0&&VisualCatalog.Visible(a)&&a.Type is 5 or 7).ToArray();SelectOnly(workers[0]);foreach(var a in workers)selectedSlots.Add(a.Slot);
  var screen=ToScreen((bag.Cell&255)*32+16,(bag.Cell>>8)*32+16);
  if(!HandleSpecialPointer(new((int)screen.X,(int)screen.Y))||s.ExchangeReserved(bag.Slot)!=workers.Length)throw new InvalidDataException("Group click did not send all workers into Exchange");
  for(int i=0;i<20000&&s.ExchangeOccupants(bag.Slot).Count()!=workers.Length;i++)s.Step();
  if(s.ExchangeOccupants(bag.Slot).Count()!=workers.Length)throw new InvalidDataException("Exchange group arrival failed");
  selected=null;selectedSlots.Clear();viewedBuilding=bag.Slot;nativePage=0;var actions=NativeActions();
  if(actions.Length!=workers.Select(a=>a.Type).Distinct().Count()||actions.Any(a=>a.Kind!="exchange-release"))throw new InvalidDataException("Exchange portraits must reflect stored classes");
  var rect=NativeRect(31,547,58,52);oldMouse=default;
  if(!UpdateNativeActions(default,new MouseState(rect.Center.X,rect.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released))||s.ExchangeOccupants(bag.Slot).Count()!=workers.Length-1||SelectedUnits().Any())throw new InvalidDataException("Portrait withdrawal must release without auto-selection");
  message="Bolsa: personagens aguardam a troca. Clique no retrato para retirar um.";
  Console.WriteLine("PASS v100 UI: group click, entry, class portraits and withdrawal without auto-selection");
 }
}
