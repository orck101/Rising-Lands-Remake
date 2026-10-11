using System.Text.Json;
using System.IO.Compression;
namespace RisingLands;

public record OriginalCommand(uint Address,uint Unit,uint Destination,uint Target,uint Flags);
public record OriginalBrainState(string MapHash,byte[] Memory);
// Original code selects the groups, RULE continuations, demands and targets.
// Rendering and physical actions are intercepted at the modern simulation boundary.
public sealed class OriginalOpponentBrain : IDisposable {
 public OriginalX86Machine Cpu{get;}
 public HashSet<int> Owners{get;}
 public Dictionary<uint,long> Callbacks{get;}=new();
 public List<OriginalCommand> Commands{get;}=new();
 public Func<OriginalCommand,bool>? Execute{get;set;}
 public Func<int,int,bool>? Allied{get;set;}
 public Func<int,int,bool>? Visible{get;set;}
 readonly string mapHash;readonly HashSet<uint> callbacks;
 readonly HashSet<uint> skipped=new(){0x410be4,0x40f4b7,0x40f4e8,0x40f519,0x4022c8,0x4020ec,0x402864,0x46b3ab,0x428f09,0x441285,0x41ce80,0x4697df,0x4424f1,0x4287b2,0x428910,0x47489d,0x41c80c,0x46a21c};
 static readonly uint[] actions={0x444f65,0x444ff0,0x444f27,0x444e96,0x444e55,0x445576,0x445083,0x445bb8,0x445ade,0x4459ef,0x4445da,0x44587b,0x42847a};
 public OriginalOpponentBrain(World world){
  mapHash=world.SourceHash;Owners=world.Units.Where(a=>a.Owner is >0 and <4).Select(a=>a.Owner).ToHashSet();
  var root=Directory.GetParent(world.SourcePath)!.Parent!.Parent!.FullName;
  Cpu=new(PackageLayout.Asset(root,"RISING-ORIG.EXE"));
  var rules=OriginalRuleMachine.Load();callbacks=rules.Nodes.Where(n=>!string.IsNullOrEmpty(n.Address)).Select(n=>Convert.ToUInt32(n.Address,16)).ToHashSet();
  Cpu.Intercept=Hook;
  foreach(uint address in callbacks.Concat(skipped).Concat(actions).Concat(new uint[]{0x46d707,0x47f61c,0x476602,0x47661c,0x476866,0x43e1dc,0x41b713,0x41b703,0x46d728}).Distinct())Cpu.Hook(address);
  Cpu.Dword(0x4e19a0,0x1000000);Cpu.Word(0x508714,world.Width);Cpu.Word(0x508716,world.Height);
  Cpu.Write(0x1000000,new byte[800*276]);Cpu.Write(0x49a0c8,new byte[1000*276]);Cpu.Write(0x512310,new byte[0x2580]);Cpu.Dword(0x5148f4,0);Cpu.Write(0x51a97c,new byte[0x39dc]);Cpu.Dword(0x498d34,12345);Cpu.Write(0x508f6c,new byte[12]);
  Cpu.Dword(0x508098,0x1260000);Cpu.Write(0x1260000,new byte[100]);Cpu.Word(0x496f94,int.Parse(Path.GetFileNameWithoutExtension(world.MapName)[5..]));foreach(int owner in Owners)Cpu.Byte(0x1260000+(uint)owner*20+1,2);
  using(var reader=new BinaryReader(File.OpenRead(world.SourcePath))){reader.BaseStream.Position=327728;int count=reader.ReadUInt16(),size=reader.ReadUInt16();for(int i=0;i<count;i++){int slot=reader.ReadUInt16();Cpu.Write(0x49a0c8+(uint)slot*276,reader.ReadBytes(size));}count=reader.ReadUInt16();size=reader.ReadUInt16();for(int i=0;i<count;i++){int slot=reader.ReadUInt16();byte[] bytes=reader.ReadBytes(size);Cpu.Write(0x1000000+(uint)slot*276,bytes[..140].Concat(bytes[652..]).ToArray());}}
  foreach(var p in new (uint Pointer,uint Address,byte[] Data)[]{(0x507d94,0x1240000,world.Collision),(0x507d5c,0x1220000,Words(world.Tiles)),(0x507d60,0x1200000,world.Objects),(0x507d98,0x1210000,world.Quantities),(0x4e0f38,0x1300000,new byte[131072])}){Cpu.Dword(p.Pointer,p.Address);Cpu.Write(p.Address,p.Data);}
  foreach(int owner in Enumerable.Range(0,5))foreach(var plane in new[]{(0x507d64u,0x1100000u),(0x507d74u,0x1150000u),(0x507d84u,0x11a0000u)}){Cpu.Dword(plane.Item1+(uint)owner*4,plane.Item2+(uint)owner*65536);Cpu.Write(plane.Item2+(uint)owner*65536,new byte[65536]);}
  Cpu.Write(0x509944,new byte[80]);for(int i=0;i<world.Mines.Count;i++){Cpu.Word(0x509944+(uint)i*4,world.Mines[i].Cell);Cpu.Word(0x509946+(uint)i*4,world.Mines[i].Quantity);}
  Cpu.Write(0x51a710,Words(rules.Roots.Select(i=>(ushort)i).ToArray()));
  using(var stream=new MemoryStream()){using var w=new BinaryWriter(stream);foreach(var n in rules.Nodes){w.Write(n.Function!=null?Convert.ToUInt32(n.Address,16):(uint)(n.Root??0));w.Write(n.Descriptor);w.Write(n.Success);w.Write(n.Failure);w.Write(n.Immediate);}Cpu.Write(0x514900,stream.ToArray());}
  var plans=NativeOpponentPlans.For(world)!;if(plans.Plans.Count>750)throw new InvalidDataException("Original ghost capacity");Cpu.Word(0x50b17c,plans.Plans.Count);Cpu.Write(0x50c314,Enumerable.Repeat((byte)255,750).ToArray());foreach(var p in plans.Plans){Cpu.Word(0x50b17e+(uint)p.Index*2,p.Cell);Cpu.Dword(0x50b75c+(uint)p.Index*4,p.ForbiddenTypes);}
  var table=new byte[1440];for(int i=0;i<120;i++){BitConverter.GetBytes(65000).CopyTo(table,i*12);table[i*12+4]=table[i*12+5]=100;table[i*12+6]=255;table[i*12+7]=1;}
  using(var data=JsonDocument.Parse(File.ReadAllText(PackageLayout.FromBase("Content/Data/build_unlocks.json")))){var tech=data.RootElement.GetProperty("technologies").EnumerateArray().ToArray();foreach(var t in tech){int offset=(t.GetProperty("branch").GetInt32()*30+t.GetProperty("index").GetInt32())*12;BitConverter.GetBytes(t.GetProperty("cost").GetInt32()).CopyTo(table,offset);int k=4;foreach(string key in new[]{"appearance","mission","effect","param1","param2","param3"})table[offset+k++]=(byte)t.GetProperty(key).GetInt32();}Cpu.Write(0x4983e4,Enumerable.Range(0,4).Select(b=>(byte)(tech.Where(t=>t.GetProperty("branch").GetInt32()==b).Max(t=>t.GetProperty("index").GetInt32())+1)).ToArray());}
  Cpu.Write(0x50816c,table);Cpu.Write(0x508124,new byte[64]);Resources(world.Resources);
  foreach(int owner in Enumerable.Range(0,4)){Cpu.Dword(0x4dfaa8+(uint)owner*4,(uint)world.Units.Count(a=>a.Owner==owner&&a.Health>0));Cpu.Word(0x4e1928+(uint)owner*2,world.Buildings.Count(b=>b.Owner==owner));}
  Cpu.Call(0x4713ff);Cpu.Call(0x47ed36);
 }
 static byte[] Words(ushort[] values){var bytes=new byte[values.Length*2];Buffer.BlockCopy(values,0,bytes,0,bytes.Length);return bytes;}
 public void Resources(ushort[] values){for(int owner=0;owner<4;owner++)foreach(var k in new[]{(0,0x4e2e9eu),(4,0x4e2ea6u),(8,0x4e2e96u)})Cpu.Word(k.Item2+(uint)owner*32,values[k.Item1+owner]);}
 readonly Dictionary<int,int> selectedBuilding=new();
 void Hook(uint address){
  if(callbacks.Contains(address))Callbacks[address]=Callbacks.GetValueOrDefault(address)+1;
  if(skipped.Contains(address)){Cpu.Return();return;}
  if(!actions.Contains(address)&&address is not (0x46d707 or 0x47f61c or 0x46d728 or 0x476602 or 0x47661c or 0x476866 or 0x43e1dc or 0x41b713 or 0x41b703))return;
  uint a=Cpu.Register(OriginalX86Machine.Eax),d=Cpu.Register(OriginalX86Machine.Edx),b=Cpu.Register(OriginalX86Machine.Ebx),c=Cpu.Register(OriginalX86Machine.Ecx);
  if(address==0x46d707){Cpu.Return(Owners.Contains((int)(a&255))?2u:0u);return;}
  if(address==0x47f61c){Cpu.Return(b);return;}
  if(address==0x46d728&&Allied!=null){Cpu.Return(Allied((int)(a&255),(int)(d&255))?1u:0u);return;}
  if(address is 0x476602 or 0x47661c or 0x476866){Cpu.Return(Visible?.Invoke((int)a,(int)d)==false?0u:1u);return;}
  if(address==0x43e1dc){Cpu.Return(stackArguments:4);return;}
  if(address==0x41b713){int owner=Cpu.Read(0x49a0c8+(a&65535)*276+4,1)[0];selectedBuilding[owner]=(int)(a&65535);Cpu.Return();return;}
  if(address==0x41b703){selectedBuilding.Remove((int)(a&255));Cpu.Return();return;}
  if(actions.Contains(address)){
   var command=new OriginalCommand(address,a&65535,d&65535,address==0x42847a?(uint)selectedBuilding.GetValueOrDefault((int)(d&255),-1):b&65535,c);
   Commands.Add(command);bool accepted=Execute?.Invoke(command)??true;Cpu.Return(accepted?1u:0u);return;
  }
  if(skipped.Contains(address))Cpu.Return();
 }
 public void Step(long tick){Cpu.Dword(0x508f6c,(uint)tick);foreach(int owner in Owners.Order())Cpu.Call(0x47f2ef,(uint)owner);}
 public OriginalBrainState Save(){using var stream=new MemoryStream();using(var zip=new GZipStream(stream,CompressionLevel.Fastest,true)){zip.Write(Cpu.Read(0x490000,0x170000));zip.Write(Cpu.Read(0x1000000,0x400000));}return new(mapHash,stream.ToArray());}
 public void Restore(OriginalBrainState state){if(state.MapHash!=mapHash||state.Memory==null||state.Memory.Length>0x570000)throw new InvalidDataException("Invalid original brain save");using var zip=new GZipStream(new MemoryStream(state.Memory),CompressionMode.Decompress);var data=new byte[0x570000];zip.ReadExactly(data);if(zip.ReadByte()!=-1)throw new InvalidDataException("Original brain save exceeds bounds");Cpu.Write(0x490000,data[..0x170000]);Cpu.Write(0x1000000,data[0x170000..]);selectedBuilding.Clear();}
 public void Dispose()=>Cpu.Dispose();
}
