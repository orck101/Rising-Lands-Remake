using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareProductionV65Capture(){
  LoadMap(restored:ProductionV65Tests.Funded(root));int type=int.Parse(captureMenu![15..]);
  viewedBuilding=type==2?simulation.World.Buildings.First(b=>b.Type==2&&b.Owner==0).Slot:ProductionV65Tests.Staff(simulation,type);nativePage=0;selected=null;selectedSlots.Clear();paused=true;revealMap=true;researchMovies.Clear();CloseResearchMovie();
  var b=ViewedBuilding!;var actions=NativeActions();if(!actions.Select(a=>a.Value).SequenceEqual(Simulation.ProductionTypes(type)))throw new InvalidDataException("Native production icons/order mismatch");
  var prior=oldMouse;oldMouse=default;var rect=NativeRect(31,547,58,52);int before=simulation.Recruitments.Count;
  if(!UpdateNativeActions(default,new MouseState(rect.Center.X,rect.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released))||simulation.Recruitments.Count!=before+1)throw new InvalidDataException("Original troop portrait must start correct building production");
  // Restore idle panel after testing the click, so all native portraits are visible.
  var job=simulation.Recruitments.Last();for(int i=0;i<10000&&!job.Finished;i++)simulation.Step();if(!job.Finished)throw new InvalidDataException("Production portrait did not complete");
  if(type!=2){var resident=simulation.ReleaseResident(simulation.World.Buildings.First(h=>h.Type==2&&h.Owner==0).Slot);if(resident!=null&&simulation.EnterBuilding(resident,b.Slot))for(int i=0;i<10000&&simulation.ResidentTrips.Count>0;i++)simulation.Step();}
  oldMouse=prior;Center();message=simulation.BuildName(b.Type)+": opcoes originais de producao.";
 }
 void PrepareResearchV65Capture(){
  var campaign=new CampaignProgress{StartingLevels=Simulation.UnlockData.Missions.First(m=>m.Mission==3).Levels.ToArray(),SelectedBranch=0};LoadMap(campaign);
  ResearchV58Tests.Staff(simulation);simulation.AgricultureResearch.Progress=0;ResearchV58Tests.Advance(simulation,14400);ResearchV58Tests.GatherFood(simulation);
  researchMovies.Clear();CloseResearchMovie();paused=true;OpenResearch();
  var prior=oldMouse;void Click(Rectangle rect){oldMouse=default;UpdateResearchMenu(default,new MouseState(rect.Center.X,rect.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released));}
  int food=simulation.World.Resources[0];Click(ExtraResearchButton);if(!extraResearchChoosing||simulation.World.Resources[0]!=food)throw new InvalidDataException("Prominent offer must open choice without charging");
  Click(ExtraResearchCancel);if(extraResearchChoosing||simulation.World.Resources[0]!=food)throw new InvalidDataException("Cancel must preserve food");
  researchMenu=false;oldMouse=default;
  if(!UpdateExtraResearchShortcut(new MouseState(ExtraResearchShortcut.Center.X,ExtraResearchShortcut.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released))||!researchMenu||!extraResearchChoosing)throw new InvalidDataException("HUD shortcut must open extra choice directly");
  if(captureMenu=="research-v65-choice"){}
  else if(captureMenu=="research-v65-used"){
   Click(ResearchRect(611,125,78,420));Click(ExtraResearchConfirm);if(!simulation.Research.ExtraPurchased||simulation.World.Resources[0]!=food-50)throw new InvalidDataException("Visible confirmation must charge exactly once");ResearchV58Tests.Advance(simulation,7200);researchMovies.Clear();CloseResearchMovie();
  }else{Click(ExtraResearchCancel);if(captureMenu=="research-v65-shortcut")researchMenu=false;else if(captureMenu=="research-v65-poor"){simulation.World.Resources[0]=25;Click(ExtraResearchButton);if(extraResearchChoosing)throw new InvalidDataException("Insufficient food must disable offer");}}
  oldMouse=prior;message="Pesquisa extra: terceira descoberta opcional por 50 alimentos.";
 }
}
