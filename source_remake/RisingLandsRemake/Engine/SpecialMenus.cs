using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 void DrawSpecialEffects(){
  foreach(var z in simulation.Special.Zones.Where(z=>SeenCell(z.Cell)))foreach(var c in visuals.ProjectileCommands(z.Spell==0?28:29,(int)(simulation.Tick%16)))Command(c,(z.Cell&255)*32+16,(z.Cell>>8)*32+16);
  foreach(var a in simulation.World.Units.Where(a=>VisualCatalog.Visible(a)&&SeenUnit(a))){if(a.Type==4&&selectedSlots.Contains(a.Slot)){var p=ToScreen(a.X/32f,a.Y/32f-12);batch.Draw(white,new Rectangle((int)p.X,(int)p.Y,30,3),new Color(30,30,45));batch.Draw(white,new Rectangle((int)p.X,(int)p.Y,Math.Clamp(a.Mana*30/Math.Max(1,simulation.ManaMaximum(a)),0,30),3),Color.CornflowerBlue);}}
 }
 int spellChoice=-1;bool marketMenu;int marketSlot=-1;int[] marketGive=new int[3],marketTake=new int[3];
 void OpenMarket(int slot){marketSession=simulation.Special.Negotiations.LastOrDefault(n=>n.Home==slot&&n.Active&&n.Stage!="Travelling")?.Id??-1;marketUnits[0]=marketUnits[1]=false;marketGiveUnits.Clear();marketTakeUnits.Clear();marketSlot=slot;marketQuantityKind=-1;marketGive=new int[3];marketTake=new int[3];marketMenu=true;CancelPointerNavigation();}
 bool HandleSpecialPointer(Point pointer){
  var pos=new Vector2(pointer.X,pointer.Y-Top)/zoom+camera;int cell=(int)(pos.Y/32)*256+(int)(pos.X/32);var target=simulation.World.Units.Where(a=>VisualCatalog.Visible(a)&&SeenUnit(a)&&ActorBounds(a).Contains(pointer)).OrderByDescending(a=>a.Y).FirstOrDefault();
  if(!KnownCell(cell))return false;
  if(spellChoice>=0){if(target!=null)cell=target.Y/1024*256+target.X/1024;bool cast=selected!=null&&simulation.CastSpell(selected,spellChoice,cell,target);message=cast?simulation.Spell(spellChoice).Name+" lancada.":"Alvo invalido, fora do alcance ou mana insuficiente.";if(cast)spellChoice=-1;return true;}
  var building=BuildingHit(pointer);
  if(selected is {Owner:0,Type:10}&&building is {Owner:>0} b&&b.Type is 2 or 16){var error=simulation.MessengerEntryError(selected,b.Slot);message=error??(simulation.SendMessenger(selected,b.Slot)?"Mensageiro a caminho da entrada. Ao chegar, clique na cartola para negociar.":"Sem caminho livre até a porta do prédio.");return true;}
  if(building is {Owner:0,Type:8} bag){int sent=SelectedUnits().Count(a=>simulation.StoreTradeUnit(a,bag));if(sent>0){message=sent+" unidades a caminho da Bolsa de Trocas.";return true;}}
  return false;
 }
 Rectangle MenuNativeRect(int x,int y,int w,int h)=>new(NativeMenuBounds.X+(int)(x*MenuScale),NativeMenuBounds.Y+(int)(y*MenuScale),(int)(w*MenuScale),(int)(h*MenuScale));
}
