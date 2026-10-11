using System.Text.Json;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed class AudioSettings {
 public int[] Volumes{get;set;}=new[]{80,60,70,85,85};public bool[] Muted{get;set;}=new bool[5];
 public float Gain(int channel)=>Muted[0]||Muted[channel]?0:Volumes[0]/100f*(channel==0?1:Volumes[channel]/100f);
 public static AudioSettings Read(string path){try{var s=JsonSerializer.Deserialize<AudioSettings>(File.ReadAllText(path));if(s is {Volumes.Length:5,Muted.Length:5}&&s.Volumes.All(v=>v is >=0 and <=100))return s;}catch(Exception e)when(e is IOException or JsonException or UnauthorizedAccessException){}return new();}
 public void Save(string path){Directory.CreateDirectory(Path.GetDirectoryName(path)!);string temp=path+".tmp";File.WriteAllText(temp,JsonSerializer.Serialize(this));File.Move(temp,path,true);}
}
public record AudioClip(string Bank,int Key,string File,int Flags,int Descriptor,string[]? Files=null);
public record AudioCatalog(List<AudioClip> Clips);
public record AudioActivity(string Bank,int Key,int Source,int Channel);
public static class AudioPlaybackPolicy {
 public static bool CanPlay(GameSound sound)=>(sound.Bank!="P"||sound.Channel!=4||sound.Owner==0)&&CanPlay(sound.Bank,sound.Key,sound.Channel);
 // Action 8 is also used for selection: never replay it on each path cell.
 // Farmer action 16 contains work chatter, not a neutral harvesting effect.
 public static bool CanPlay(string bank,int key,int channel)=>bank!="P"||channel!=2||((key>>8&255)!=8&&!(key>>16==5&&(key>>8&255)==16));
}
public static class AudioConcurrency {
 public static bool CanStart(IEnumerable<AudioActivity> active,string bank,int key,int source,int channel)=>!(channel==2&&active.Count(a=>a.Channel==2&&a.Bank==bank&&a.Key==key)>=2)&&!active.Any(a=>channel==4&&a.Channel==4||(bank is "P" or "B")&&a.Bank==bank&&a.Source==source||bank=="A"&&a.Bank==bank&&a.Key==key);
}
public sealed class OriginalAudio:IDisposable {
 sealed record ActiveSound(SoundEffectInstance Sound,AudioActivity Activity,float Level){public int Channel=>Activity.Channel;}
 readonly Dictionary<(string,int),AudioClip> catalog;readonly Dictionary<string,SoundEffect> sounds=new();readonly List<ActiveSound> playing=new();readonly Dictionary<(string,int,int),long> last=new();readonly Random random=new(0x524953);readonly string folder;public AudioSettings Settings{get;}public bool Unavailable{get;private set;}public int StartedCount{get;private set;}public int BusySuppressedCount{get;private set;}public int PolicySuppressedCount{get;private set;}
 public OriginalAudio(AudioSettings settings){Settings=settings;folder=PackageLayout.FromBase("Content/Data/Audio");var j=JsonSerializer.Deserialize<AudioCatalog>(File.ReadAllText(PackageLayout.FromBase("Content/Data/audio_catalog.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;catalog=j.Clips.ToDictionary(c=>(c.Bank,c.Key));}
 public void Play(string bank,int key,int channel,long clock,int source=0,float pan=0,float level=1){
  if(!AudioPlaybackPolicy.CanPlay(bank,key,channel)){PolicySuppressedCount++;return;}
  if(Unavailable||Settings.Gain(channel)==0||!catalog.TryGetValue((bank,key),out var clip))return;long now=Environment.TickCount64;var id=(bank,key,channel is 3 or 4?-1:source);if(last.TryGetValue(id,out long previous)&&now-previous<(channel is 3 or 4?500:120))return;Update();
  // Player orders may replace that unit's work sound, but never stack acknowledgements.
  if(channel==4&&!playing.Any(p=>p.Channel==4))for(int i=playing.Count-1;i>=0;i--)if(playing[i].Channel==2&&playing[i].Activity.Bank==bank&&playing[i].Activity.Source==source){playing[i].Sound.Stop();playing[i].Sound.Dispose();playing.RemoveAt(i);}
  if(!AudioConcurrency.CanStart(playing.Select(p=>p.Activity),bank,key,source,channel)){BusySuppressedCount++;return;}
  if(playing.Count>=16){int victim=channel is 3 or 4?playing.FindIndex(p=>p.Channel==2):-1;if(victim<0)return;playing[victim].Sound.Stop();playing[victim].Sound.Dispose();playing.RemoveAt(victim);}
  try{var files=clip.Files??new[]{clip.File};string name=files[random.Next(files.Length)];if(!sounds.TryGetValue(name,out var sound)){using var stream=File.OpenRead(Path.Combine(folder,name));sounds[name]=sound=SoundEffect.FromStream(stream);}var instance=sound.CreateInstance();instance.Volume=Math.Clamp(Settings.Gain(channel)*level,0,1);instance.Pan=Math.Clamp(pan,-1,1);instance.Play();playing.Add(new(instance,new(bank,key,source,channel),level));StartedCount++;last[id]=now;}
  catch(Exception e)when(e is NoAudioHardwareException or InvalidOperationException or IOException){Unavailable=true;Console.Error.WriteLine("Audio indisponivel: "+e.Message);}
 }
 public void Update(){for(int i=playing.Count-1;i>=0;i--){var p=playing[i];if(p.Sound.State==SoundState.Stopped){p.Sound.Dispose();playing.RemoveAt(i);}else p.Sound.Volume=Math.Clamp(Settings.Gain(p.Channel)*p.Level,0,1);}}
 public void VerifyConcurrency(){using var probe=new OriginalAudio(new());probe.Play("P",0x50f00,4,0,77,level:0);probe.Play("P",0x50b00,4,0,77,level:0);probe.Play("P",0x70b00,4,0,78,level:0);probe.Play("P",0x50700,2,0,77,level:0);probe.Play("P",0x72500,2,0,78,level:0);if(probe.Unavailable||probe.StartedCount!=2||probe.BusySuppressedCount!=3||probe.playing.Any(p=>p.Sound.Volume!=0))throw new InvalidDataException("Audio concurrency regression: active voice must block repeated responses without muting another worker's effect");}
 public void VerifyAutonomousChatter(){using var probe=new OriginalAudio(new());for(int cycle=0;cycle<10;cycle++)for(int worker=0;worker<10;worker++){probe.Play("P",0x50800,2,cycle,worker,level:0);probe.Play("P",0x51000,2,cycle,worker,level:0);}if(probe.StartedCount!=0||probe.PolicySuppressedCount!=200)throw new InvalidDataException("Automatic collection chatter reached the audio device");probe.Play("P",0x50800,4,0,77,level:0);if(probe.Unavailable||probe.StartedCount!=1||probe.playing.Any(p=>p.Sound.Volume!=0))throw new InvalidDataException("Explicit selection voice was lost");}
 public int VerifyCatalog(){int loaded=0;foreach(string file in catalog.Values.SelectMany(c=>c.Files??new[]{c.File}).Distinct()){using var stream=File.OpenRead(Path.Combine(folder,file));using var sound=SoundEffect.FromStream(stream);using var voice=sound.CreateInstance();voice.Volume=0;voice.Play();voice.Stop();loaded++;}return loaded;}
 public void Dispose(){foreach(var p in playing){p.Sound.Stop();p.Sound.Dispose();}foreach(var s in sounds.Values)s.Dispose();playing.Clear();sounds.Clear();}
}
public sealed partial class ResearchGame {
 OriginalAudio? originalAudio;AudioSettings audioSettings=new();string AudioSettingsPath=>Path.Combine(PackageLayout.UserRoot(root),"UserSettings","audio.json");
 void InitAudio(){audioSettings=AudioSettings.Read(AudioSettingsPath);originalAudio=new(audioSettings);}
 void UnitSound(Actor a,int action,int channel=2){if(capture!=null||originalAudio==null||!AudioPlaybackPolicy.CanPlay(new("P",a.Type<<16|action<<8,channel,0,a.Owner,a.Slot)))return;var p=ToScreen(a.X/32f,a.Y/32f);if(channel==2&&(p.X<0||p.X>ViewWidth||p.Y<Top||p.Y>ViewHeight+Top||!SeenUnit(a)))return;originalAudio.Play("P",a.Type<<16|action<<8,channel,simulation.Tick,a.Slot,p.X/ViewWidth*2-1);}
 void BuildingSound(Building b,int action){if(capture!=null||originalAudio==null)return;originalAudio.Play("B",b.Type<<16|action<<8,2,simulation.Tick,b.Slot);}
 void SaveAudioSettings(){if(capture!=null){originalAudio?.Update();return;}try{audioSettings.Save(AudioSettingsPath);originalAudio?.Update();}catch(Exception e)when(e is IOException or UnauthorizedAccessException){menuStatus="Nao foi possivel guardar os ajustes de som.";}}
 static readonly string[] AudioLabels={"Volume geral","Musica","Efeitos sonoros","Avisos do jogo","Vozes dos personagens"};
 void UpdateAudioMenu(KeyboardState k,MouseState m){if(m.LeftButton!=ButtonState.Pressed||oldMouse.LeftButton!=ButtonState.Released)return;for(int row=0;row<5;row++){int y=282+row*42;for(int button=0;button<3;button++)if(MenuNativeRect(455+button*55,y-3,50,28).Contains(m.Position)){if(button==2)audioSettings.Muted[row]=!audioSettings.Muted[row];else audioSettings.Volumes[row]=Math.Clamp(audioSettings.Volumes[row]+(button==0?-5:5),0,100);SaveAudioSettings();return;}}if(MenuNativeRect(240,520,320,30).Contains(m.Position))pauseMenu.Back();}
 void DrawAudioMenu(){batch.Draw(white,GraphicsDevice.Viewport.Bounds,Color.Black);batch.Draw(nativeMenu,NativeMenuBounds,Color.White);MenuText("AJUSTES DE SOM",235,gold);for(int row=0;row<5;row++){int y=282+row*42;var label=MenuNativeRect(190,y,1,1);Text(AudioLabels[row]+": "+audioSettings.Volumes[row]+"%",label.X,label.Y,Color.Wheat,.66f*MenuScale);for(int button=0;button<3;button++){var r=MenuNativeRect(455+button*55,y-3,50,28);Outline(r,gold);Text(button==0?"-":button==1?"+":audioSettings.Muted[row]?"Som":"Mudo",r.X+4,r.Y+4,Color.Wheat,.6f*MenuScale);}}MenuText("Musica: faixas do CD nao encontradas",475,Color.Wheat,.6f);MenuText("Ajustes guardados automaticamente",495,Color.Wheat,.65f);MenuText("Voltar",522,gold);}
}
