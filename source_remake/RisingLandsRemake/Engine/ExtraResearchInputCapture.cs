using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 void VerifyExtraResearchInput(){
  if(captureMenu=="research-v58-ui-wide"){graphics.PreferredBackBufferWidth=1600;graphics.ApplyChanges();}
  string snapshot=Path.Combine(Directory.GetParent(root)!.Parent!.FullName,"experiments","extra-research","slot4.json");
  simulation.Restore(File.ReadAllText(snapshot));
  int food=simulation.World.Resources[0];
  void Click(Point point){
   oldMouse=default;
   UpdateResearchOrNativeActions(default,new MouseState(point.X,point.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released));
  }
  BeginExtraResearchChoice();
  var icon=ResearchRect(409,495,72,48);
  var point=new Point(icon.Center.X,icon.Bottom-4);
  if(point.Y<NativeTop)throw new InvalidDataException("Fixture must click the visible icon over the underlying action bar");
  Click(point);
  if(pendingResearchBranch!=0)throw new InvalidDataException("Visible Agriculture icon click intercepted: extra research branch was not selected");
  if(simulation.World.Resources[0]!=food)throw new InvalidDataException("Preview spent food");
  Click(ExtraResearchCancel.Center);
  if(extraResearchChoosing||simulation.World.Resources[0]!=food)throw new InvalidDataException("Cancel spent food or did not close choice");
  BeginExtraResearchChoice();Click(point);Click(ExtraResearchConfirm.Center);
  if(extraResearchChoosing||!simulation.Research.ExtraPurchased||simulation.ActiveResearchBranch!=0||simulation.World.Resources[0]!=food-50)throw new InvalidDataException("Extra purchase did not confirm exactly once");
  Click(ExtraResearchConfirm.Center);
  if(simulation.World.Resources[0]!=food-50)throw new InvalidDataException("Repeated click charged extra research twice");
  ResearchV58Tests.Advance(simulation,300);
  if(simulation.AgricultureResearch.Progress<=0)throw new InvalidDataException("Purchased extra research did not advance");
  string saved=simulation.Serialize();simulation.Restore(saved);
  if(!simulation.Research.ExtraPurchased||simulation.ActiveResearchBranch!=0||simulation.AgricultureResearch.Progress<=0)throw new InvalidDataException("Extra purchase lost on restore");
  paused=true;oldMouse=default;researchMovies.Clear();CloseResearchMovie();
  Console.WriteLine("Extra research UI: visible icon, cancel, purchase, single charge, progress and restore passed.");
 }
}
