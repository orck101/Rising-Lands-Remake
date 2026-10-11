using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 void VerifyNavigationV49(){
  MouseState State(Point p,ButtonState right=ButtonState.Released,ButtonState middle=ButtonState.Released)=>new(p.X,p.Y,0,ButtonState.Released,middle,right,ButtonState.Released,ButtonState.Released);
  var actor=simulation.World.Units.First(u=>u.Owner==0&&u.Type==5);SelectOnly(actor);showMini=true;oldMouse=default;var start=MiniRect.Center;
  var down=State(start,ButtonState.Pressed);if(!UpdatePointerNavigation(down))throw new InvalidDataException("Right drag must capture minimap");oldMouse=down;
  var moved=State(new Point(View.Left+100,View.Top+100),ButtonState.Pressed);UpdatePointerNavigation(moved);oldMouse=moved;var panel=MiniRect;UpdatePointerNavigation(State(moved.Position));if(MiniRect!=panel||selected!=actor)throw new InvalidDataException("Minimap drag must preserve placement and unit selection");
  camera=new(500,500);zoom=1.5f;oldMouse=default;var anchor=new Point(View.Width/2,View.Height/2);down=State(anchor,middle:ButtonState.Pressed);UpdatePointerNavigation(down);oldMouse=down;moved=State(anchor+new Point(90,60),middle:ButtonState.Pressed);UpdatePointerNavigation(moved);oldMouse=moved;if(Vector2.Distance(camera,new Vector2(440,460))>.01f)throw new InvalidDataException("Middle drag must pan both axes respecting zoom");UpdatePointerNavigation(State(moved.Position));var released=camera;oldMouse=State(moved.Position);UpdatePointerNavigation(State(moved.Position+new Point(100,100)));if(camera!=released||selected!=actor)throw new InvalidDataException("Released middle drag must stop without changing selection");
  CancelPointerNavigation();zoom=1;CenterStart();miniPosition=new Point(16,16);message="Minimapa reposicionado; arraste com o direito. Rodinha pressionada: mover camera.";
 }
}
