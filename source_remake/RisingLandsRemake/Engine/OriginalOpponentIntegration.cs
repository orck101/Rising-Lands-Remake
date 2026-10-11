namespace RisingLands;
public sealed partial class Simulation {
 OriginalOpponentBrain? originalBrain;
 public bool OriginalOpponentActive=>originalBrain!=null;
 // Research-only integration: not enabled by the playable build until all action
 // notifications and saved continuations are validated.
 public void EnableOriginalOpponentForResearch(){
  if(!OpponentEnabled||World.SourcePath.Length==0)throw new InvalidOperationException("Original opponent requires a copied native map");
  originalBrain?.Dispose();originalBrain=new(World){Execute=ExecuteOriginalCommand,Allied=(a,b)=>!Hostile(a,b)};
  SynchronizeOriginalOpponent();
 }
 void OriginalOpponentStep(){if(originalBrain==null)return;if(Tick%10==0)SynchronizeOriginalOpponent();originalBrain.Step(Tick);}
 bool RecruitOriginal(int owner,int type,int slot){
  var b=World.Buildings.FirstOrDefault(b=>b.Slot==slot&&b.Owner==owner);if(b==null||!Inhabited(b)||!Produces(b.Type,type)||!OpponentUnitUnlocked(owner,type)||Recruitments.Any(j=>j.BuildingSlot==slot&&!j.Finished)||!RecruitData.Rules.Any(r=>r.Type==type))return false;
  var r=TrainingRule(type);if(World.Resources[owner]<r.Food||World.Resources[4+owner]<r.Rock||World.Resources[8+owner]<r.Parts)return false;
  int settler=World.BuildingResidents.GetValueOrDefault(slot,new()).FirstOrDefault(id=>World.Units.Any(a=>a.Slot==id&&a.Owner==owner&&a.Type==9&&a.Health>0&&a.LegacyState==14)&&!ResidentReserved(id),-1);if(settler<0)return false;
  World.Resources[owner]-=(ushort)r.Food;World.Resources[4+owner]-=(ushort)r.Rock;World.Resources[8+owner]-=(ushort)r.Parts;
  Recruitments.Add(new(){Owner=owner,BuildingSlot=slot,SettlerSlot=settler,Type=type,SourceBuildingSlot=type is 5 or 6 or 7?-1:slot,TravelCell=b.Cell});return true;
 }
 bool ExecuteOriginalCommand(OriginalCommand command){
  int slot=(int)command.Unit,destination=(int)command.Destination,target=(int)command.Target;
  if(command.Address==0x42847a)return RecruitOriginal(destination,slot,target);
  if(command.Address==0x41de80){var b=World.Buildings.FirstOrDefault(b=>b.Slot==slot);return b!=null&&ReleaseResident(slot,b.Owner)!=null;}
  var a=World.Units.FirstOrDefault(u=>u.Slot==slot&&u.Owner>0&&u.Health>0);if(a==null)return false;
  switch(command.Address){
   case 0x444f65:case 0x444f27:return Gather(a,destination,false);
   case 0x444ff0:var mine=World.Mines.FindIndex(m=>m.Cell==destination);return mine>=0&&StartMining(a,mine,false);
   case 0x444e96:return GatherWarehouse(a,destination,false);
   case 0x444e55:if(a.Type==7){if(destination==1){a.RockGround=target;a.RockDestination=-1;return true;}return SetRockDestination(a,target);}return SetFoodDestination(a,destination,target);
   case 0x44587b:return Order(a,destination&255,destination>>8);
   case 0x445bb8:return EnterBuilding(a,destination);
   case 0x445576:var site=Constructions.FirstOrDefault(c=>c.Owner==a.Owner&&c.Type==destination&&c.Cell==target&&!Complete(c));return site!=null?ResumeConstruction(a,site):StartConstruction(a,destination,target);
   case 0x445ade:if(destination==3)return AttackBuilding(a,target);var enemy=World.Units.FirstOrDefault(u=>u.Slot==target);if(enemy==null||!Hostile(a.Owner,enemy.Owner)||!CanAttack(a,enemy))return false;ClearOrders(a);a.AttackTarget=target;return true;
   case 0x4459ef:return Gather(a,destination,false);
   default:return false;
  }
 }
 void SynchronizeOriginalOpponent(){
  if(originalBrain==null)return;var cpu=originalBrain.Cpu;var units=cpu.Read(0x1000000,800*276);var buildings=cpu.Read(0x49a0c8,1000*276);var occupancy=new ushort[65536];
  void Word(byte[] data,int offset,int value)=>BitConverter.GetBytes(unchecked((ushort)value)).CopyTo(data,offset);
  foreach(var a in World.Units.Where(a=>a.Slot<800)){
   int offset=a.Slot*276,cell=a.Y/1024*256+a.X/1024;units[offset]=(byte)a.Owner;units[offset+1]=(byte)a.Type;
   int state=a.Health<=0?4:a.LegacyState==14?14:a.AttackTarget>=0||Sieging(a)?1:ResidentTrips.Any(t=>t.Slot==a.Slot)?13:Constructions.Any(c=>!Complete(c)&&HasBuilder(c,a.Slot))?11:a.FoodJob>0||a.MineJob>0?5:a.Route.Count>0?2:a.LegacyState;
   units[offset+2]=(byte)state;Word(units,offset+124,cell);Word(units,offset+154,a.Health);Word(units,offset+4,a.Satiety);Word(units,offset+12,a.Food+a.Rock);
   var home=World.BuildingResidents.FirstOrDefault(p=>p.Value.Contains(a.Slot));Word(units,offset+192,home.Value!=null?home.Key:65535);
   if(a.Health>0&&VisualCatalog.Visible(a))occupancy[cell]=(ushort)(0x4000|(a.Slot<<3));
  }
  foreach(var b in InitialBuildings.Concat(Constructions.Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).Where(b=>b.Slot<1000)){
   int offset=b.Slot*276;Word(buildings,offset+2,b.Cell);buildings[offset+4]=(byte)b.Owner;buildings[offset+8]=(byte)b.Type;
   bool active=World.Buildings.Any(v=>v.Slot==b.Slot);bool pending=Constructions.Any(c=>c.Id==b.Slot&&!Complete(c));buildings[offset+5]=(byte)(pending?2:active?Recruitments.Any(j=>j.BuildingSlot==b.Slot&&!j.Finished)?4:5:3);
   Word(buildings,offset+6,Opponent.BuildingDamage.GetValueOrDefault(b.Slot));Word(buildings,offset+10,BuildingDamageLimits[b.Type]);Word(buildings,offset+12,Occupants(b.Slot));Word(buildings,offset+14,ResidentCapacity(b));Array.Clear(buildings,offset+78,30);
   var ids=World.BuildingResidents.GetValueOrDefault(b.Slot,new()).Where(id=>ResidentActor(id) is {Health:>0,LegacyState:14}).Take(30).ToArray();for(int i=0;i<30;i++){Word(buildings,offset+18+i*2,i<ids.Length?ids[i]:65535);if(i<ids.Length)buildings[offset+78+i]=1;}
   if(active||pending){var r=Rule(b.Type);for(int y=0;y<r.Height;y++)for(int x=0;x<r.Width;x++)occupancy[b.Cell+y*256+x]=(ushort)(0x8000|(b.Slot<<5));}
  }
  cpu.Write(0x1000000,units);cpu.Write(0x49a0c8,buildings);var raw=new byte[131072];Buffer.BlockCopy(occupancy,0,raw,0,raw.Length);cpu.Write(0x1300000,raw);cpu.Write(0x1200000,World.Objects);cpu.Write(0x1210000,World.Quantities);originalBrain.Resources(World.Resources);
  foreach(int owner in Enumerable.Range(0,4)){cpu.Dword(0x4dfaa8+(uint)owner*4,(uint)World.Units.Count(a=>a.Owner==owner&&a.Health>0));cpu.Word(0x4e1928+(uint)owner*2,World.Buildings.Count(b=>b.Owner==owner));
   for(int type=0;type<26;type++)cpu.Byte(0x4e1934+(uint)(owner*26+type),owner==0?UnitUnlocked(type)?1:0:OpponentUnitUnlocked(owner,type)?1:0);
  }
  foreach(var r in RecruitData.Rules){cpu.Word(0x4965b0+(uint)r.Type*50,r.Parts);cpu.Word(0x4965b2+(uint)r.Type*50,r.Rock);cpu.Word(0x4965b4+(uint)r.Type*50,r.Food);}
 }
}
