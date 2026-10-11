using System.Security.Cryptography;
using Microsoft.Xna.Framework;

namespace RisingLands;

public record Frame(int Width,int Height,Color[] Pixels);
public sealed class Actor {
    public int Slot {get;set;} public int Type {get;set;} public int Owner {get;set;}
    public int Satiety {get;set;}=4;public int SatietyMaximum {get;set;}=4;public int FatigueClock {get;set;}
    public int Mana {get;set;} public long PetrifiedUntil {get;set;} public long ArmoredUntil {get;set;} public long CastAnimationUntil {get;set;} public int RepairTarget {get;set;}=-1;
    public int AttackWindup {get;set;}
    public int AttackTarget {get;set;}=-1; public int AttackCooldown {get;set;} public int HitFlash {get;set;}
    public int LastAttacker {get;set;}=-1; public long LastHitTick {get;set;}
    public bool AutomaticDefense {get;set;} public int DefenseAnchor {get;set;}=-1;public long CombatRetryAt {get;set;}
    public long DefenseAcquireAfter {get;set;}
    public int LegacyState {get;set;} public int Health {get;set;}
    public int X {get;set;} public int Y {get;set;}
    public int Direction {get;set;}=1; public int AnimationFrame {get;set;}
    public int AnimationCount {get;set;}=8;public int AnimationDelay {get;set;}=1;public int AnimationCounter {get;set;}
    public int CargoKind {get;set;}=1; // Native resource: 0 parts, 1 food, 2 rock.
    public int FoodSourceBuilding {get;set;}=-1;
    public int Food {get;set;} public int FoodSource {get;set;}=-1; public int FoodJob {get;set;} public int WorkTicks {get;set;} public int DeliveryCell {get;set;}=-1;
    public int FoodDestinationKind {get;set;} // 0: none, 1: ground cell, 2: building slot.
    public int FoodDestination {get;set;}=-1;
    public int OverflowCell {get;set;}=-1;
    public bool PersistentCollection {get;set;}
    public bool FoodPlantsOnly {get;set;}
    public bool RepeatFood {get;set;}=true;
    public int Rock {get;set;} public int MineIndex {get;set;}=-1; public int MineJob {get;set;} public int MineTicks {get;set;} public int MineWorkCell {get;set;}=-1; public int RockDestination {get;set;}=-1; public int RockGround {get;set;}=-1; public bool RepeatMine {get;set;}=true;
    public List<int> GroundDrops {get;set;}=new();
    public int FoodField {get;set;}=-1; // Fixed local food-area anchor (planted food or pile); -1 means no area.
    public List<int> Route {get;set;} = new();
}
public record Building(int Slot,int Type,int Owner,int Cell);
public sealed class Mine {public int Cell{get;set;} public int Quantity{get;set;}}
public sealed class World {
    public string SourcePath {get;init;}="";
    public List<Mine> Mines{get;set;}=new();
    public required string MapName {get;init;} public required string SourceHash {get;init;}
    public int Width {get;init;} public int Height {get;init;}
    public required ushort[] Tiles {get;init;} public required byte[] Collision {get;init;}
    public required byte[] Objects {get;set;} public required byte[] Quantities {get;set;}
    public required ushort[] Resources {get;set;}
    public Dictionary<int,List<int>> BuildingResidents {get;init;}=new();
    public required List<Actor> Units {get;set;} public required List<Building> Buildings {get;init;}
}
public static class LegacyAssets {
    // All paths are opened read-only. No legacy file writer exists in this project.
    public static Color[] Palette(string path) {
        byte[] b=File.ReadAllBytes(path);
        if(b.Length<777 || b[0]!='P' || b[1]!='i' || b[2]!='e') throw new InvalidDataException("Invalid PIE palette");
        Color[] p=new Color[256];
        for(int i=0;i<256;i++) p[i]=new Color((byte)(b[9+i*3]<<2),(byte)(b[10+i*3]<<2),(byte)(b[11+i*3]<<2));
        p[0]=Color.Black;return p;
    }
    public static List<Frame> Sprites(string path,Color[] palette) {
        using var s=File.OpenRead(path);using var r=new BinaryReader(s);int count=r.ReadUInt16();var frames=new List<Frame>();
        long total=0;
        for(int i=0;i<count;i++) {
            int w=r.ReadUInt16(),h=r.ReadUInt16();uint size=r.ReadUInt32();long end=s.Position+size;
            if(w==0 || h==0 || (long)w*h>16000000 || end>s.Length || (total+=(long)w*h)>100000000)throw new InvalidDataException("CMP bounds");
            var pixels=new Color[w*h];int cursor=0;
            while(s.Position<end) {
                int control=r.ReadSByte();
                if(control>=0)cursor+=control;
                else {int n=-control;if(s.Position+n>end || cursor+n>pixels.Length)throw new InvalidDataException("CMP literal overrun");for(int j=0;j<n;j++)pixels[cursor++]=palette[r.ReadByte()];}
                if(cursor>pixels.Length)throw new InvalidDataException("CMP skip overrun");
            }
            if(cursor!=pixels.Length)throw new InvalidDataException("CMP incomplete frame");
            frames.Add(new Frame(w,h,pixels));
        }
        if(s.Position!=s.Length)throw new InvalidDataException("CMP trailing bytes");return frames;
    }
    public static World Map(string path) {
        byte[] b=File.ReadAllBytes(path);if(b.Length<327732)throw new InvalidDataException("ETA truncated");
        using var s=new MemoryStream(b,false);using var r=new BinaryReader(s);
        // The signature is preserved in the research corpus; no guessed magic is required.
        s.Position=4;byte[] collision=r.ReadBytes(65536);ushort[] tiles=new ushort[65536];
        for(int i=0;i<tiles.Length;i++)tiles[i]=r.ReadUInt16();
        s.Position=327684;ushort[] resources=new ushort[12];for(int i=0;i<12;i++)resources[i]=r.ReadUInt16();
        int w=r.ReadUInt16(),h=r.ReadUInt16();if(w<1 || h<1 || w>256 || h>256)throw new InvalidDataException("ETA dimensions");
        s.Position=327728;int count=r.ReadUInt16(),size=r.ReadUInt16();var buildings=new List<Building>();var residents=new Dictionary<int,List<int>>();
        if(size!=204)throw new InvalidDataException("Unsupported building layout");
        for(int i=0;i<count;i++){int slot=r.ReadUInt16();byte[] d=r.ReadBytes(size);if(d.Length!=size)throw new InvalidDataException("Building truncated");buildings.Add(new(slot,d[8],d[4],BitConverter.ToUInt16(d,2)));residents[slot]=Enumerable.Range(0,Math.Min(30,(int)BitConverter.ToUInt16(d,14))).Where(j=>d[78+j]==1).Select(j=>(int)BitConverter.ToUInt16(d,18+j*2)).ToList();}
        count=r.ReadUInt16();size=r.ReadUInt16();if(size<682 || count>4096)throw new InvalidDataException("Unit layout");
        var units=new List<Actor>();
        for(int i=0;i<count;i++) {
            int slot=r.ReadUInt16();byte[] d=r.ReadBytes(size);if(d.Length!=size)throw new InvalidDataException("Unit truncated");
            int cell=BitConverter.ToUInt16(d,124);
            units.Add(new Actor{Slot=slot,Owner=d[0],Type=d[1],LegacyState=d[2],Health=BitConverter.ToUInt16(d,666),Satiety=BitConverter.ToUInt16(d,4),SatietyMaximum=Math.Max(1,(int)BitConverter.ToUInt16(d,118)),FatigueClock=BitConverter.ToUInt16(d,116),Mana=d[1]==4?d[730]:0,X=(cell&255)*1024,Y=(cell>>8)*1024,Direction=d[657],AnimationFrame=d[676],AnimationCount=d[677],AnimationDelay=Math.Max(1,(int)d[679]),AnimationCounter=d[681]});
        }
        int mineCount=r.ReadUInt16();if(mineCount>20)throw new InvalidDataException("Mine count");var mines=new List<Mine>();for(int i=0;i<mineCount;i++)mines.Add(new(){Cell=r.ReadUInt16(),Quantity=r.ReadUInt16()});
        return new World{SourcePath=Path.GetFullPath(path),Mines=mines,MapName=Path.GetFileName(path),SourceHash=Convert.ToHexString(SHA256.HashData(b)),Width=w,Height=h,Tiles=tiles,Collision=collision,Objects=b[196612..262148],Quantities=b[262148..327684],Resources=resources,Units=units,Buildings=buildings,BuildingResidents=residents};
    }
}
