using Microsoft.Xna.Framework;
namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareCropV61Capture(){
  int level=captureMenu=="crop-v61-research6"?6:captureMenu=="crop-v61-research15"?15:2;
  var levels=simulation.Campaign.StartingLevels.ToArray();levels[0]=level;LoadMap(new CampaignProgress{StartingLevels=levels});var crop=CropV61Tests.BuildCrop(simulation,captureMenu!="crop-v61-empty");
  if(captureMenu=="crop-v61-full"){
   CropV61Tests.FundFood(simulation);for(int n=0;n<4;n++)for(int i=0;i<simulation.CultureFoodCost();i++){var f=CropV61Tests.Supply(simulation);simulation.DepositCropFood(f,crop);simulation.Step();}
  }else if(captureMenu is "crop-v61-born" or "crop-v61-release"){
   int times=captureMenu=="crop-v61-release"?4:1;if(times==4)CropV61Tests.FundFood(simulation);
   for(int n=0;n<times;n++)for(int i=0;i<simulation.CultureFoodCost();i++){var f=CropV61Tests.Supply(simulation);simulation.DepositCropFood(f,crop);simulation.Step();}
   if(times==4){var f=CropV61Tests.Supply(simulation);simulation.DepositCropFood(f,crop);}
  }else{
   int food=captureMenu=="crop-v61-food4"?4:captureMenu=="crop-v61-research6"?3:captureMenu=="crop-v61-research15"?2:captureMenu=="crop-v61-food2"?2:0;
   for(int i=0;i<food;i++){var f=CropV61Tests.Supply(simulation);simulation.DepositCropFood(f,crop);simulation.Step();}
  }
  viewedBuilding=crop.Slot;SelectOnly(null);viewedBuilding=crop.Slot;nativePage=0;paused=true;zoom=2;
  camera=new Vector2((crop.Cell&255)*32+32-ViewWidth/(2*zoom),(crop.Cell>>8)*32+35-ViewHeight/(2*zoom));ClampCamera();
  if(NativeActions().Length!=0||!sprites.ContainsKey(0x2a3d))throw new InvalidDataException("Crop must use original food gauge, not generic actions");
  message=simulation.CropStatus(crop);
 }
}
