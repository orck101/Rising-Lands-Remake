using System.Text.Json;
using System.Diagnostics;
using System.Security.Cryptography;
namespace RisingLands;

// Isolated emulated CPU. No Windows import, original process, filesystem or network
// is available to the guest. The helper avoids Unicorn's documented .NET/CFG crash.
public sealed class OriginalX86Machine : IDisposable {
 public const uint Stop=0x2000000,Stack=0x2080000;
 public const int Eax=19,Ebx=21,Ecx=22,Edx=24,Eip=26,Esp=30;
 readonly Process helper; readonly StreamWriter input;readonly StreamReader output;
 public Action<uint>? Intercept{get;set;}
 public OriginalX86Machine(string copiedExecutable){
  byte[] pe=File.ReadAllBytes(copiedExecutable);
  if(Convert.ToHexString(SHA256.HashData(pe)).ToLowerInvariant()!="af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e")throw new InvalidDataException("Original IA executable does not match verified evidence");
  string research=Directory.GetParent(copiedExecutable)!.Parent!.Parent!.FullName;
  string python=Environment.GetEnvironmentVariable("RISING_PYTHON")??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe");
  var info=new ProcessStartInfo(python){UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true};
  info.ArgumentList.Add("-u");info.ArgumentList.Add(Path.Combine(research,"tools/original_x86_worker.py"));info.ArgumentList.Add(copiedExecutable);
  helper=Process.Start(info)??throw new InvalidOperationException("Original IA helper could not start");input=helper.StandardInput;output=helper.StandardOutput;
  helper.ErrorDataReceived+=(_,e)=>{if(e.Data!=null)Debug.WriteLine("Original IA: "+e.Data);};helper.BeginErrorReadLine();
  try{Reply();}catch{Dispose();throw;}
 }
 JsonElement Reply(){
  while(true){var task=output.ReadLineAsync();if(!task.Wait(TimeSpan.FromSeconds(8)))throw new InvalidDataException("Original IA helper response timeout");string? line=task.Result;if(line==null)throw new InvalidDataException("Original IA helper terminated");using var json=JsonDocument.Parse(line);var data=json.RootElement;
   if(data.TryGetProperty("hook",out var address)){try{Intercept?.Invoke(address.GetUInt32());}catch{Send(new{op="stop"});Send(new{op="resume"});throw;}Send(new{op="resume"});continue;}
   if(data.TryGetProperty("error",out var error))throw new InvalidDataException("Original IA helper: "+error.GetString());return data.Clone();
  }
 }
 void Send(object data){input.WriteLine(JsonSerializer.Serialize(data));input.Flush();}
 JsonElement Exchange(object data){Send(data);return Reply();}
 public void Hook(uint address)=>Exchange(new{op="hook",address});
 public byte[] Read(uint address,int length)=>Convert.FromBase64String(Exchange(new{op="read",address,length}).GetProperty("data").GetString()!);
 public void Write(uint address,byte[] bytes)=>Exchange(new{op="write",address,data=Convert.ToBase64String(bytes)});
 public void Byte(uint address,int value)=>Write(address,new[]{unchecked((byte)value)});
 public void Word(uint address,int value)=>Write(address,BitConverter.GetBytes(unchecked((ushort)value)));
 public void Dword(uint address,uint value)=>Write(address,BitConverter.GetBytes(value));
 public uint Register(int id)=>Exchange(new{op="regread",id}).GetProperty("value").GetUInt32();
 public void Register(int id,uint value)=>Exchange(new{op="regwrite",id,value});
 public void Return(uint? result=null,int stackArguments=0)=>Exchange(new{op="return",result,stackArguments});
 public uint Call(uint address,uint eax=0,uint edx=0,uint ebx=0,uint ecx=0)=>Exchange(new{op="call",address,eax,edx,ebx,ecx}).GetProperty("value").GetUInt32();
 public void Dispose(){if(!helper.HasExited){Send(new{op="close"});if(!helper.WaitForExit(1000))helper.Kill();}helper.Dispose();}
}
