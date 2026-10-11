namespace RisingLands;
public static class CropV61Tests {
 static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: v61 "+name);Console.WriteLine("PASS: v61 "+name);}
 public static Simulation Load(string root,int level=2){var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE01.ETA"));new VisualCatalog().Apply(w);var levels=Simulation.UnlockData.Missions.First(m=>m.Mission==1).Levels.ToArray();levels[0]=level;return new(w,new(){StartingLevels=levels});}
 public static Building BuildCrop(Simulation s,bool staff=true){int slot=ResearchTests.Build(s,4,false);var b=s.World.Buildings.First(b=>b.Slot==slot);if(staff){var settler=s.ReleaseResident(s.World.Buildings.First(b=>b.Type==2&&b.Owner==0).Slot)!;if(!s.EnterBuilding(settler,slot))throw new Exception("Crop staff route missing");for(int i=0;i<10000&&s.ResidentTrips.Count>0;i++)s.Step();if(s.Occupants(slot)!=1)throw new Exception("Crop staff did not enter");}return b;}
 public static void FundFood(Simulation s){ResearchTests.Build(s,3);ResearchV58Tests.GatherFood(s);}
 public static Actor Supply(Simulation s,int amount=1){var f=s.World.Units.First(u=>u.Owner==0&&u.Type==5);s.Stop(f);if(s.World.Resources[0]<amount||f.Food+amount>s.Capacity(f))throw new Exception("Crop delivery fixture lacks conserved resources");s.World.Resources[0]-=(ushort)amount;f.Food+=amount;f.CargoKind=1;return f;}
 public static void Run(string root){
  var preview=new Building(1,4,0,20*256+20);
  foreach(var row in CropPresentation.Data.Rows){int level=row.Discount==0?2:row.Discount==1?6:15;var actual=CropPresentation.Commands(preview,row.Residents,row.Food,level);int x=20*32+17,y=20*32-12;
   Check(actual.SequenceEqual(row.Commands.Select(c=>c with{X=c.X+x,Y=c.Y+y})),"native gauge placement and art, residents "+row.Residents+", food "+row.Food+", discount "+row.Discount);
  }
  foreach(int level in new[]{2,6,15}){
   var s=Load(root,level);var crop=BuildCrop(s);int cost=s.CultureFoodCost();var culture=s.Culture(crop.Slot);var f=s.World.Units.First(u=>u.Owner==0&&u.Type==5);var picture=CropPresentation.Commands(crop,1,0,level);
   Check(picture.Count(c=>c.Sprite==0x2a3b)==4+(cost-1)&&picture.Count(c=>c.Sprite==0x2a3c)==1,"empty food row has "+(cost-1)+" native boxes and five colony places");
   for(int i=0;i<cost-1;i++){Supply(s);Check(s.DepositCropFood(f,crop),"native one-food deposit accepted at level "+level);s.Step();}
   Check(s.BornActors.Count==0&&culture.Food==cost-1&&CropPresentation.Commands(crop,1,culture.Food,level).Count(c=>c.Sprite==0x2a3d)==cost-1,"food boxes fill individually without early birth at level "+level);
   string partial=s.Serialize();var copy=Load(root,level);copy.Restore(partial);Check(copy.Serialize()==partial&&copy.CropFood(crop.Slot)==cost-1,"partial native food gauge survives save/load at level "+level);
   Supply(s);Check(s.DepositCropFood(f,crop),"final food is delivered at level "+level);s.Step();
   Check(s.BornActors.Count==1&&s.Occupants(crop.Slot)==2&&culture.Food==0&&culture.Consumed==cost&&s.BornActors.Single().LegacyState==14,"last food creates a real housed colonist and clears food at level "+level);
   Check(CropPresentation.Commands(crop,2,0,level).Count(c=>c.Sprite==0x2a3c)==2&&CropPresentation.Commands(crop,2,0,level).All(c=>c.Sprite!=0x2a3d),"native indicators swap food for another colonist at level "+level);
   string born=s.Serialize();copy.Restore(born);Check(copy.Serialize()==born,"new colonist and actual food consumption roundtrip at level "+level);
  }
  var empty=Load(root);var inactive=BuildCrop(empty,false);for(int i=0;i<5;i++){var f=Supply(empty);Check(empty.DepositCropFood(f,inactive),"empty crop accepts one food "+i);empty.Step();}
  Check(empty.CropFood(inactive.Slot)==5&&empty.BornActors.Count==0&&!empty.Inhabited(inactive),"five foods wait in inactive crop until a colonist enters");
  var excess=Supply(empty);empty.DepositCropFood(excess,inactive);Check(empty.CropFood(inactive.Slot)==5&&empty.Culture(inactive.Slot).Consumed==1,"original five-food counter clamp consumes excess without fake birth");
  var settler=empty.ReleaseResident(empty.World.Buildings.First(b=>b.Type==2&&b.Owner==0).Slot)!;Check(empty.EnterBuilding(settler,inactive.Slot),"inactive crop can receive founding colonist");for(int i=0;i<10000&&empty.ResidentTrips.Count>0;i++)empty.Step();Check(empty.BornActors.Count==1&&empty.Occupants(inactive.Slot)==2&&empty.CropFood(inactive.Slot)==0,"arrival activates crop and immediately converts its stored food");
  var full=Load(root);var crowded=BuildCrop(full);FundFood(full);var worker=full.World.Units.First(u=>u.Owner==0&&u.Type==5);
  for(int n=0;n<4;n++)for(int i=0;i<5;i++){Supply(full);full.DepositCropFood(worker,crowded);full.Step();}
  Check(full.Occupants(crowded.Slot)==5&&full.CropFood(crowded.Slot)==0,"crop reaches exactly five housed colonists");
  int total=full.BornActors.Count;Supply(full);Check(full.DepositCropFood(worker,crowded)&&full.Occupants(crowded.Slot)==4&&full.BornActors.Count==total,"full-crop delivery releases an existing colonist without fabricating a sixth");
  Check(worker.GroundDrops.Count==1&&full.World.Objects[worker.GroundDrops.Single()]==67&&full.World.Quantities[worker.GroundDrops.Single()]==1,"full-crop food reaches ground and is excluded from accidental self-recollection");
  var released=full.World.Units.LastOrDefault(u=>u.Type==9&&u.Owner==0&&VisualCatalog.Visible(u));var door=full.DoorOrigin(crowded);Check(released!=null&&Math.Abs(released.X-door.X)<=1024&&released.Y==door.Y,"released colonist emerges along the front door line");
  string overflow=full.Serialize();var loaded=Load(root);loaded.Restore(overflow);Check(loaded.Serialize()==overflow,"full-crop exit, ground food and collector history survive save/load");
  var capacity=Load(root);var housing=BuildCrop(capacity);for(int i=0;i<5;i++){Supply(capacity);capacity.DepositCropFood(capacity.World.Units.First(u=>u.Type==5&&u.Owner==0),housing);}
  int existing=capacity.World.Units.Count(u=>u.Health>0);for(int i=existing;i<Simulation.NativeEntityCapacity;i++)capacity.World.Units.Add(new Actor{Slot=10000+i,Type=9,Owner=1,Health=100,LegacyState=7});capacity.Step();Check(capacity.CropFood(housing.Slot)==5&&capacity.BornActors.Count==0,"original 800-entity capacity preserves food while birth is blocked");
  Console.WriteLine("ALL V61 CROP TESTS PASSED");
 }
}
