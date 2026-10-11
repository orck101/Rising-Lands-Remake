namespace RisingLands;

// Portable resources are independent of the legacy installation layout.
// Existing development trees and historical test fixtures remain supported.
public static class PackageLayout {
 [System.Runtime.InteropServices.DllImport("kernel32.dll",CharSet=System.Runtime.InteropServices.CharSet.Unicode,SetLastError=true)]
 static extern bool SetDllDirectory(string directory);
 public static void PrepareNativeLibraries(){
  string directory=Path.Combine(AppContext.BaseDirectory,"Dependencias");
  if(OperatingSystem.IsWindows()&&Directory.Exists(directory)&&!SetDllDirectory(directory))
   throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
 }
 public static bool Organized(string root)=>File.Exists(Path.Combine(root,"layout.json"));
 public static string FindAssets(){
  var local=Path.Combine(AppContext.BaseDirectory,"Recursos");
  if(Organized(local))return local;
  for(var p=new DirectoryInfo(AppContext.BaseDirectory);p!=null;p=p.Parent){
   var candidate=Path.Combine(p.FullName,"working","RISING");
   if(Directory.Exists(candidate))return candidate;
  }
  throw new DirectoryNotFoundException("Recursos do remake ausentes. Copie a pasta completa ou use --assets.");
 }
 public static string UserRoot(string root)=>Organized(root)?Directory.GetParent(Path.GetFullPath(root))!.FullName:Path.GetFullPath(Path.Combine(root,"../.."));
 public static string Asset(string root,params string[] parts){
  string relative=Path.Combine(parts).Replace('\\','/');
  if(Organized(root)){
   if(relative.StartsWith("CD/WORLD/",StringComparison.OrdinalIgnoreCase))relative="Mapas/"+relative[9..];
   else if(relative.Equals("CD/WORLD",StringComparison.OrdinalIgnoreCase))relative="Mapas";
   else if(relative.StartsWith("GRAPHICS/",StringComparison.OrdinalIgnoreCase))relative="Graficos/Bancos/"+relative[9..];
   else if(relative.Equals("GRAPHICS",StringComparison.OrdinalIgnoreCase))relative="Graficos/Bancos";
   else if(relative.Equals("PLATGAME.PIE",StringComparison.OrdinalIgnoreCase))relative="Graficos/Paleta/PLATGAME.PIE";
  }
  return Path.Combine(root,relative);
 }
 public static string FromBase(params string[] parts){
  string relative=Path.Combine(parts).Replace('\\','/');
  string root=Path.Combine(AppContext.BaseDirectory,"Recursos");
  if(Organized(root)&&relative.StartsWith("Content/",StringComparison.OrdinalIgnoreCase)){
   relative=relative[8..];
   if(relative.Equals("Data/Audio",StringComparison.OrdinalIgnoreCase))relative="Sons";
   else if(relative.StartsWith("Data/Audio/",StringComparison.OrdinalIgnoreCase))relative="Sons/"+relative[11..];
   else if(relative.StartsWith("Data/",StringComparison.OrdinalIgnoreCase)&&relative.EndsWith(".wav",StringComparison.OrdinalIgnoreCase))relative="Sons/"+relative[5..];
   else if(relative.StartsWith("Data/Scenes/",StringComparison.OrdinalIgnoreCase))relative="Mapas/Cenas/"+relative[12..];
   else if(relative.StartsWith("Data/",StringComparison.OrdinalIgnoreCase))relative="Dados/"+relative[5..];
   else if(relative.StartsWith("ResearchAnimation",StringComparison.OrdinalIgnoreCase)||relative.StartsWith("ResearchCompletion",StringComparison.OrdinalIgnoreCase))relative="Animacoes/"+relative;
   else relative=(relative.EndsWith(".png",StringComparison.OrdinalIgnoreCase)?"Graficos/Interface/":"Dados/Fontes/")+relative;
   return Path.Combine(root,relative);
  }
  return Path.Combine(AppContext.BaseDirectory,relative);
 }
}
