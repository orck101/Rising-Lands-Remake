namespace RisingLands;
public sealed partial class Simulation {
 int OpeningDebit(Save s,int index)=>s.Opponent?.BalancedFifthOpening==true&&index<8&&index%4>0?InitialResources[index]-(index<4?15:10):s.Opponent?.ReducedOpening==true&&index%4==1?InitialResources[index]-(index<8?15:0):0;
 void ApplyFifthOpening(){
  if(MissionNumber!=5)return;Opponent.BalancedFifthOpening=true;
  foreach(int owner in Enumerable.Range(1,3)){World.Resources[owner]=15;World.Resources[4+owner]=10;}
  foreach(var a in World.Units.Where(a=>a.Owner is >0 and <4&&a.Type==8)){a.Health=0;ClearOrders(a);CancelFeeding(a);}
 }
 bool RedOpeningEconomyPending(int owner)=>Opponent.ReducedOpening&&owner==1&&!MissionFourPressure(owner)&&(!World.Buildings.Any(b=>b.Owner==1&&b.Type==3&&Inhabited(b))||Opponent.Deposited[1]==0);
 void ApplyReducedRedOpening(){
  if(MissionNumber!=4)return;var home=World.Buildings.FirstOrDefault(b=>b.Owner==1&&b.Type==3);if(home==null)return;var startingBuildings=new[]{3,2,4}.Select(type=>World.Buildings.First(b=>b.Owner==1&&b.Type==type)).ToArray();
  var farmers=World.Units.Where(a=>a.Owner==1&&a.Type==5).OrderBy(a=>a.Slot).Take(2).ToArray();var builder=World.Units.First(a=>a.Owner==1&&a.Type==7);
  var colonists=World.Units.Where(a=>a.Owner==1&&a.Type==9).OrderBy(a=>a.Slot).Take(7).ToArray();
  var keep=colonists.Select(a=>a.Slot).Concat(farmers.Select(a=>a.Slot)).Append(builder.Slot).ToHashSet();
  foreach(var a in World.Units.Where(a=>a.Owner==1&&!keep.Contains(a.Slot))){a.Health=0;if(a.Type==9&&OriginalResidents.Values.Any(ids=>ids.Contains(a.Slot)))a.LegacyState=7;ClearOrders(a);}
  foreach(var ids in World.BuildingResidents.Values)ids.RemoveAll(id=>World.Units.Any(a=>a.Slot==id&&a.Owner==1));
  for(int i=0;i<startingBuildings.Length;i++)World.BuildingResidents[startingBuildings[i].Slot]=new(){colonists[i].Slot};World.BuildingResidents[startingBuildings.First(b=>b.Type==2).Slot].AddRange(colonists.Skip(3).Take(2).Select(a=>a.Slot));foreach(var a in colonists){a.LegacyState=colonists.Take(5).Contains(a)?14:7;ClearOrders(a);}
  foreach(var b in World.Buildings.Where(b=>b.Owner==1&&!startingBuildings.Contains(b)).ToArray()){Opponent.BuildingDamage[b.Slot]=BuildingMaxHealth(b);World.Buildings.Remove(b);}
  RefreshDestroyedFootprint();doorCacheCount=-1;InvalidateSpatialIndex();World.Resources[1]=15;World.Resources[5]=15;World.Resources[9]=0;Opponent.ReducedOpening=true;Opponent.TradeFirstOpening=true;
  foreach(var a in colonists.Skip(5)){int exit=BuildingExit(home);if(exit>=0)PlaceAtExit(a,home,exit);}
 }
 // User-requested balance override. Keep the native slot for old-save identities;
 // only the seeded enemy food cart is retired, never a legitimately trained cart.
 void RemoveEnemyStartingCart(){
  if(MissionNumber!=4)return;
  foreach(var seed in OriginalActorSeeds.Values.Where(a=>a.Owner is >0 and <4&&a.Type==13)){
   var cart=World.Units.FirstOrDefault(a=>a.Slot==seed.Slot&&a.Owner is >0 and <4&&a.Type==13);
   if(cart==null)continue;
   cart.Health=0;ClearOrders(cart);CancelFeeding(cart);
  }
 }
}
