using System.Text.Json;
namespace RisingLands;
// Diagnostics are written separately from player save slots. Logging must never throw.
public static class CrashDiagnostics {
 public static Func<object?>? Context;
 public static Func<string?>? Snapshot;
 public static string Phase="Inicialização";
 static string? directory;static readonly object gate=new();static bool recorded;
 public static void Initialize(string root){directory=Path.Combine(PackageLayout.UserRoot(root),"runtime","crashes");}
 public static string? Record(Exception exception,string origin){
  lock(gate){if(recorded)return null;recorded=true;
   try{string folder=directory??PackageLayout.FromBase("CrashReports");Directory.CreateDirectory(folder);
    string name=DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+"-"+System.Environment.ProcessId;
    var errors=new List<string>();object? context=null;string? snapshot=null;
    try{context=Context?.Invoke();}catch(Exception e){errors.Add("Context: "+e);}
    try{snapshot=Snapshot?.Invoke();}catch(Exception e){errors.Add("Snapshot: "+e);}
    string report=Path.Combine(folder,name+".json");
    File.WriteAllText(report,JsonSerializer.Serialize(new {TimestampUtc=DateTime.UtcNow,Origin=origin,Phase,Executable=System.Environment.ProcessPath,Runtime=System.Environment.Version.ToString(),Arguments=System.Environment.GetCommandLineArgs(),Exception=exception.ToString(),Context=context,DiagnosticErrors=errors},new JsonSerializerOptions{WriteIndented=true}));
    if(snapshot!=null)try{File.WriteAllText(Path.Combine(folder,name+".snapshot.json"),snapshot);}catch(Exception e){File.AppendAllText(Path.Combine(folder,name+".snapshot-error.txt"),e.ToString());}
    Console.Error.WriteLine("Registro da falha: "+report);return report;
   }catch(Exception e){Console.Error.WriteLine("Não foi possível registrar a falha: "+e.Message);return null;}
  }
 }
}
public sealed partial class ResearchGame {
 void RegisterCrashContext(){
  CrashDiagnostics.Context=()=>new {Map=simulation?.World.MapName,Tick=simulation?.Tick,Speed=gameSpeed,Selected=selectedSlots.ToArray(),LastMessage=message,Paused=paused,Phase=CrashDiagnostics.Phase};
  CrashDiagnostics.Snapshot=()=>simulation?.Serialize();
 }
}
