using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareSpecialV66Capture(){
  var state=SpecialV66Tests.Load(root,captureMenu=="special-v66-market");LoadMap(restored:state);paused=true;revealMap=true;researchMovies.Clear();CloseResearchMovie();
  if(captureMenu=="special-v66-library"){
   viewedBuilding=ProductionV65Tests.Staff(simulation,6);nativePage=0;var r=NativeRect(31,547,58,52);oldMouse=default;
   if(!UpdateNativeActions(default,new MouseState(r.Center.X,r.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released))||!simulation.Special.Learning.Any(j=>j.Spell==0))throw new InvalidDataException("Original spell portrait must start learning");
   for(int i=0;i<400;i++)simulation.Step();var b=ViewedBuilding!;camera=new Vector2((b.Cell&255)*32-ViewWidth/2f,(b.Cell>>8)*32-ViewHeight/2f);ClampCamera();message="Biblioteca: aprendendo Visao. Os cinco icones sao originais.";
  }else if(captureMenu=="special-v66-druid"){
   SpecialV66Tests.Teach(simulation);var a=SpecialV66Tests.Recruit(simulation,4);a.Mana=simulation.ManaMaximum(a);SelectOnly(a);Center();var r=NativeRect(31,547,58,52);oldMouse=default;
   if(!UpdateNativeActions(default,new MouseState(r.Center.X,r.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released))||spellChoice!=0)throw new InvalidDataException("Original spell portrait must select casting");message="Druida: Visao selecionada. Clique no mapa para lancar.";
  }else if(captureMenu=="special-v66-flight"){
   var a=SpecialV66Tests.Recruit(simulation,16);int water=Enumerable.Range(0,65536).First(c=>(c&255)>6&&(c>>8)>6&&(c&255)<simulation.World.Width-6&&(c>>8)<simulation.World.Height-6&&simulation.World.Tiles[c] is >=0x1dd and <0x261);if(!simulation.Order(a,water&255,water>>8))throw new InvalidDataException("Aircraft water route");for(int i=0;i<10000&&a.Route.Count>0;i++)simulation.Step();SelectOnly(a);Center();message="Helipede sobre a agua: voo nao usa as restricoes terrestres.";
  }else{
   int slot=ProductionV65Tests.Staff(simulation,16);var a=SpecialV66Tests.Recruit(simulation,10);var b=simulation.World.Buildings.First(b=>b.Type==16&&b.Owner==1);a.X=(b.Cell&255)*1024;a.Y=((b.Cell>>8)+3)*1024;simulation.UpdateVisibility();if(!simulation.ContactMarket(a,b.Slot))throw new InvalidDataException("Market contact");for(int i=0;i<10000&&!simulation.MarketPartners(slot).Any();i++)simulation.Step();if(!simulation.MarketPartners(slot).Any())throw new InvalidDataException("Market contact arrival");OpenMarket(slot);marketGive[0]=10;marketTake[1]=8;message="Exemplo: oferecer 10 alimentos para receber 8 pedras.";
  }
  oldMouse=default;researchMovies.Clear();CloseResearchMovie();
 }
}
