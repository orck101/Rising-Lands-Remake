namespace RisingLands;
public sealed class AttackAlert {
    public int Cell{get;private set;}=65535;
    public uint LastTick{get;private set;}
    public static bool CanRecord(uint now,uint last,int owner)=>owner==0&&(last>now||now-last>=200);
    public static bool Recent(uint now,uint last,int cell)=>cell!=65535&&unchecked(now-last)<=1000;
    public static (int X,int Y) Camera(int cell,int width,int height,int mapWidth,int mapHeight)=>(Math.Clamp((cell&255)-width/2,0,Math.Max(0,mapWidth-width)),Math.Clamp((cell>>8)-height/2,0,Math.Max(0,mapHeight-height)));
    public void Record(uint tick,int cell,int owner){if(owner!=0)return;if(Cell==65535||CanRecord(tick,LastTick,owner)){Cell=cell;LastTick=tick;}}
    public void Clear(){Cell=65535;LastTick=0;}
}
