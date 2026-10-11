using System.Text.Json;
namespace RisingLands;
public record EnvironmentVisual(int Code,int Phase,List<DrawCommand> Commands);
public record EnvironmentVisualData(List<EnvironmentVisual> Visuals);
public static class EnvironmentPresentation {
 public static readonly EnvironmentVisualData Data=JsonSerializer.Deserialize<EnvironmentVisualData>(File.ReadAllText(PackageLayout.FromBase("Content/Data/environment_visuals.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 static readonly Dictionary<(int,int),List<DrawCommand>> Frames=Data.Visuals.ToDictionary(v=>(v.Code,v.Phase),v=>v.Commands);
 public static List<DrawCommand>? Commands(int code,int phase)=>Frames.GetValueOrDefault((code,phase%(code>=23?16:4)));
 // Static scene commands must never retain a resource/plant drawn at map import time.
 public static bool DynamicCode(int code)=>code is >=1 and <=52 or >=66 and <=68;
}
public sealed class EnvironmentState {
 public int Fraction{get;set;}
 public long NativeTicks{get;set;}
 public int Row{get;set;}=1;
 public bool Updating{get;set;}
 public uint Random{get;set;}=0x52495349;
 public long GeneratedFood{get;set;}
 public long DecayedFood{get;set;}
}
public sealed partial class Simulation {
 public EnvironmentState EnvironmentState{get;private set;}=new();
 public const int NativeVegetRepousse=128; // RISING.INI / DIVERS, SetUpdateChamp 0x475254.
 public static int PlantFood(int code)=>code is 51 or 52?2:code is >=29 and <=50?1:0;
 public bool GrowingSoil(int cell)=>ValidMapCell(cell)&&World.Tiles[cell] is >=157 and <221 or >=132 and <135;
 uint EnvironmentRandom(){uint n=EnvironmentState.Random;n^=n<<13;n^=n>>17;n^=n<<5;return EnvironmentState.Random=n;}
 void ChangePlant(int cell,int code){int before=PlantFood(World.Objects[cell]),after=PlantFood(code);if(after>before)EnvironmentState.GeneratedFood+=after-before;else EnvironmentState.DecayedFood+=before-after;World.Objects[cell]=(byte)code;}
 public bool SpawnPlant(int cell,int style){
  if(!GrowingSoil(cell)||World.Objects[cell]!=0||(World.Collision[cell]&1)!=0||BuildingAt(cell)!=null||ConstructionCovers(cell)||MineAt(cell)>=0)return false;
  ChangePlant(cell,style%2==0?29:41);return true;
 }
 bool SpreadPlant(int anchor){
  foreach(int delta in new[]{-1,1,-256,256}){int c=anchor+delta;if(!ValidMapCell(c)||CellDistance(c,anchor)!=1||!GrowingSoil(c)||World.Objects[c]!=0)continue;if(SpawnPlant(c,(int)(EnvironmentRandom()&1)))return true;}return false;
 }
 void EnvironmentStep(){
  // User-requested 1.25x: exact integer accumulator, separate from game speed.
  EnvironmentState.Fraction+=5;
  while(EnvironmentState.Fraction>=4){EnvironmentState.Fraction-=4;EnvironmentState.NativeTicks++;
   if(EnvironmentState.NativeTicks%NativeVegetRepousse==0)EnvironmentState.Updating=true;
   if(!EnvironmentState.Updating&&EnvironmentState.Row<=1)continue;
   int row=EnvironmentState.Row;
   if(row<World.Height)for(int x=1;x<World.Width;x++){
    int cell=row*256+x,code=World.Objects[cell];if(code is <29 or >52||ConstructionCovers(cell)||EnvironmentRandom()%10!=0)continue;
    if(code is 40 or 52){ChangePlant(cell,0);SpreadPlant(cell);}
    else if(code is >=36 and <=39 or >=48 and <=51){if(EnvironmentRandom()%5==0)ChangePlant(cell,code+1);else SpreadPlant(cell);}
    else ChangePlant(cell,code+1);
   }
   if(++EnvironmentState.Row>255){EnvironmentState.Row=1;EnvironmentState.Updating=false;}
  }
 }
 bool ValidGrowthMutation(Save s,int cell){
  if(s.Environment==null)return false;int before=InitialObjects[cell],after=s.Objects[cell];
  return (before==0||before is >=29 and <=52 or >=66 and <=68)&&(after==0||after is >=29 and <=52)&&(before is >=29 and <=52||GrowingSoil(cell));
 }
 bool CanTraverse(Actor u,int x,int y){
  if(x<0||y<0||x>=World.Width||y>=World.Height)return false;
  if(Airborne(u))return true;
  if(WallAt(y*256+x))return false;
  if(GateCell(y*256+x))return GatePassable(y*256+x);
  if(Passable(x,y))return true;if(u.Type!=7||x<0||y<0||x>=World.Width||y>=World.Height)return false;
  int cell=y*256+x,current=u.Y/1024*256+u.X/1024;
  if((World.Collision[cell]&1)!=0||World.Objects[cell] is >=1 and <=11)return false;
  foreach(var c in Constructions){var r=Rule(c.Type);if(!Complete(c)&&HasBuilder(c,u.Slot)&&Inside(current,c.Cell,r.Width,r.Height)&&Inside(cell,c.Cell,r.Width,r.Height))return true;}
  return false;
 }
 void ValidateEnvironment(Save s){
  var e=s.Environment;if(s.Version<25&&e==null)return;
  if(e==null||e.Fraction is <0 or >3||e.NativeTicks<0||e.NativeTicks>s.Tick*5/4||e.Row is <1 or >255||e.Random==0||e.GeneratedFood<0||e.DecayedFood<0||e.GeneratedFood>s.Tick*256L||e.DecayedFood>s.Tick*512L)throw new InvalidDataException("Invalid environment growth state");
 }
}
