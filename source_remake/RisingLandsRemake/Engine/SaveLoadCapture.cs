using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 void VerifyUserSaveLoad(){
  captureSlotDirectory=Path.Combine(Directory.GetParent(root)!.Parent!.FullName,"experiments","save-load-v104");
  foreach(int slot in new[]{4,5}){
   OpenSlots(true);
   var rect=MenuButton(slot-1);oldMouse=default;
   UpdateMenu(default,new MouseState(rect.Center.X,rect.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released));
   if(pauseMenu.Open)throw new InvalidDataException($"Slot {slot} menu load failed: {menuStatus}");
   Console.WriteLine($"MENU LOAD PASS slot {slot}: tick={simulation.Tick}, map={simulation.World.MapName}");
  }
  paused=true;oldMouse=default;
 }
}
