namespace RisingLands;
public sealed record UnitMemory(int Owner,int Cell);
public sealed class StructureMemory {
 public Building Building{get;set;}=new(0,0,0,0);public long Seen{get;set;}public int Health{get;set;}public bool Occupied{get;set;}public int Progress{get;set;}=-1;public int WallMask{get;set;}public int GateOrientation{get;set;}public int GateFrame{get;set;}=15;
}
public sealed partial class Simulation {
 long sharedVisionTick=-1,visionEpoch,cachedVisionEpoch=-1;int cachedAlliances=-1;readonly Dictionary<int,byte[]> sharedVisions=new();readonly HashSet<int> refreshedObservers=new();
 bool SharesVision(int observer,int source)=>observer==source||observer==0&&Allied(source)||source==0&&Allied(observer);
 void InvalidateSharedVision(){visionEpoch++;sharedVisions.Clear();refreshedObservers.Clear();opponentDanger.Clear();safetyTick=-1;}
 byte[] SharedCurrentVision(int observer){
  int alliances=Enumerable.Range(1,3).Aggregate(0,(mask,o)=>mask|(Allied(o)?1<<o:0));
  if(sharedVisionTick!=Tick||cachedVisionEpoch!=visionEpoch||cachedAlliances!=alliances){sharedVisions.Clear();refreshedObservers.Clear();sharedVisionTick=Tick;cachedVisionEpoch=visionEpoch;cachedAlliances=alliances;}
  if(sharedVisions.TryGetValue(observer,out var existing))return existing;
  var visible=new byte[65536];sharedVisions[observer]=visible;
  void Stamp(int x,int y,int radius){foreach(var d in VisionDisk(radius,false)){int nx=x+d.X,ny=y+d.Y;if(nx>=0&&ny>=0&&nx<World.Width&&ny<World.Height)visible[ny*256+nx]=1;}}
  foreach(var a in World.Units.Where(a=>SharesVision(observer,a.Owner)&&a.Health>0&&VisualCatalog.Visible(a)))Stamp(a.X/1024,a.Y/1024,VisibilityData.Units.FirstOrDefault(r=>r.Type==a.Type)?.Radius??5);
  foreach(var b in World.Buildings.Where(b=>SharesVision(observer,b.Owner))){var r=Rule(b.Type);Stamp((b.Cell&255)+r.Width/2,(b.Cell>>8)+r.Height/2,VisibilityData.Buildings.FirstOrDefault(r=>r.Type==b.Type)?.Radius??0);}
  foreach(var z in Special.Zones.Where(z=>z.Spell==0&&z.Until>Tick&&SharesVision(observer,z.Owner)))Stamp(z.Cell&255,z.Cell>>8,Spell(0).Radius);
  return visible;
 }
 bool VisibleFootprint(Building b,byte[] vision)=>Enumerable.Range(0,Rule(b.Type).Height).Any(y=>Enumerable.Range(0,Rule(b.Type).Width).Any(x=>ValidMapCell(b.Cell+y*256+x)&&vision[b.Cell+y*256+x]!=0));
 IEnumerable<Building> ExistingStructures()=>World.Buildings.Concat(Constructions.Where(c=>!Complete(c)&&!DestroyedBuilding(c.Id)).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell)));
 void RememberStructures(Dictionary<int,StructureMemory> memories,int observer,byte[] vision){
  var all=ExistingStructures().ToArray();foreach(int id in memories.Where(p=>VisibleFootprint(p.Value.Building,vision)&&!all.Any(b=>b.Slot==p.Key&&b==p.Value.Building)).Select(p=>p.Key).ToArray())memories.Remove(id);
  foreach(var b in all.Where(b=>b.Owner!=observer&&VisibleFootprint(b,vision))){var site=Constructions.FirstOrDefault(c=>c.Id==b.Slot);memories[b.Slot]=new(){Building=b,Seen=Tick,Health=BuildingHealth(b),Occupied=Inhabited(b),Progress=site?.Progress??-1,WallMask=b.Type==9?WallMask(b.Cell):0,GateOrientation=b.Type==12?site?.GateOrientation??Math.Max(0,GateOrientationAt(b.Cell)):0,GateFrame=b.Type==12?GateFrameAt(b.Cell):15};}
 }
 void InitializeTerrainKnowledge(){
  if(!OpponentEnabled)return;foreach(int owner in InitialActors.Values.Select(a=>a.Owner).Where(o=>o is >0 and <4).Distinct()){
   if(!Opponent.Explored.TryGetValue(owner,out var known))Opponent.Explored[owner]=known=new byte[65536];for(int y=0;y<World.Height;y++)for(int x=0;x<World.Width;x++)known[y*256+x]=1;
   if(!Opponent.KnownStructures.ContainsKey(owner))Opponent.KnownStructures[owner]=new();
   if(!Opponent.KnownUnits.ContainsKey(owner))Opponent.KnownUnits[owner]=new();
   if(!Opponent.KnownResources.ContainsKey(owner)){var kinds=new byte[65536];for(int c=0;c<65536;c++)if(ValidMapCell(c))kinds[c]=ResourceKnowledgeKind(InitialObjects[c]);foreach(var mine in InitialMines)kinds[mine.Cell]=3;Opponent.KnownResources[owner]=kinds;}
  }
 }
 static byte ResourceKnowledgeKind(int code)=>code is >=29 and <=52 or 67?(byte)2:code==66?(byte)1:code==68?(byte)3:(byte)0;
 bool OpponentKnowsResource(int owner,int cell)=>owner==0||owner is >0 and <4&&ValidMapCell(cell)&&Opponent.KnownResources.TryGetValue(owner,out var known)&&known[cell]!=0;
 void RefreshOpponentKnowledge(int owner,byte[] vision){
  if(!refreshedObservers.Add(owner))return;
  if(!Opponent.KnownStructures.TryGetValue(owner,out var memories))Opponent.KnownStructures[owner]=memories=new();RememberStructures(memories,owner,vision);
  if(!Opponent.KnownUnits.TryGetValue(owner,out var units))Opponent.KnownUnits[owner]=units=new();
  foreach(int id in units.Where(p=>vision[p.Value.Cell]!=0&&!World.Units.Any(a=>a.Slot==p.Key&&a.Owner==p.Value.Owner&&Hostile(owner,a.Owner)&&a.Health>0&&VisualCatalog.Visible(a)&&ActorCell(a)==p.Value.Cell)).Select(p=>p.Key).ToArray())units.Remove(id);
  foreach(var a in World.Units.Where(a=>Hostile(owner,a.Owner)&&a.Health>0&&VisualCatalog.Visible(a)&&vision[ActorCell(a)]!=0))units[a.Slot]=new(a.Owner,ActorCell(a));
  if(Opponent.KnownResources.TryGetValue(owner,out var resources))for(int c=0;c<65536;c++)if(vision[c]!=0)resources[c]=ResourceKnowledgeKind(World.Objects[c]);
 }
 void ValidateStructureMemories(Dictionary<int,StructureMemory> memories,Save s){
  if(memories==null||memories.Count>4096)throw new InvalidDataException("Invalid structure memory");foreach(var p in memories){var m=p.Value;var b=m?.Building;if(b==null||p.Key!=b.Slot||b.Type is <0 or >21||(b.Owner is <0 or >3)&&!(IsEighthMission&&b.Owner==200&&oceanOriginalBuildings.Any(a=>a.Slot==b.Slot&&a.Type==18))||!ValidMapCell(b.Cell)||m!.Seen<0||m.Seen>s.Tick||m.Health<0||m.Health>BuildingMaxHealth(b)||m.Progress< -1||m.Progress>Rule(b.Type).Duration||m.WallMask is <0 or >15||m.GateOrientation is <0 or >1||m.GateFrame is <0 or >31||!InitialBuildings.Any(a=>a.Slot==b.Slot&&a.Type==b.Type&&a.Cell==b.Cell)&&!s.Constructions.Any(c=>c.Id==b.Slot&&c.Type==b.Type&&c.Cell==b.Cell))throw new InvalidDataException("Invalid remembered building");}
 }
 void ValidateOpponentKnowledge(Save s,OpponentState state){if(state.KnownUnits==null||state.KnownUnits.Count>3||state.KnownUnits.Any(p=>p.Key is <1 or >3||p.Value==null||p.Value.Count>2048||p.Value.Any(q=>q.Value==null||q.Value.Owner is <0 or >3||!ValidMapCell(q.Value.Cell)||!s.Units.Any(a=>a.Slot==q.Key))))throw new InvalidDataException("Invalid observed unit knowledge");if(state.KnownStructures==null||state.KnownStructures.Count>3||state.KnownResources==null||state.KnownResources.Count>3)throw new InvalidDataException("Invalid opponent knowledge");foreach(var p in state.KnownStructures){if(p.Key is <1 or >3)throw new InvalidDataException("Invalid observer");ValidateStructureMemories(p.Value,s);}foreach(var p in state.KnownResources)if(p.Key is <1 or >3||p.Value==null||p.Value.Length!=65536||p.Value.Where((v,c)=>v>3||!ValidMapCell(c)&&v!=0).Any())throw new InvalidDataException("Invalid resource knowledge");}
}
