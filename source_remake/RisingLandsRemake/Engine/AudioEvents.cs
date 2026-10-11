namespace RisingLands;
public record GameSound(string Bank,int Key,int Channel,int Cell,int Owner,int Source);
public sealed partial class Simulation {
 public event Action<GameSound>? SoundIssued;
 void EmitUnitSound(Actor a,int action,int channel=2)=>SoundIssued?.Invoke(new("P",a.Type<<16|action<<8,channel,a.Y/1024*256+a.X/1024,a.Owner,a.Slot));
 void EmitBuildingSound(Building b,int action)=>SoundIssued?.Invoke(new("B",b.Type<<16|action<<8,2,b.Cell,b.Owner,b.Slot));
}
public sealed partial class ResearchGame {
 void PlayAlert(int key){if(capture==null)originalAudio?.Play("A",key,3,simulation.Tick);}
 void ReceiveGameSound(GameSound sound){if(capture!=null||originalAudio==null||!AudioPlaybackPolicy.CanPlay(sound))return;var p=ToScreen((sound.Cell&255)*32,(sound.Cell>>8)*32);if(sound.Bank=="A"){PlayAlert(sound.Key);return;}if(!SeenCell(sound.Cell)||p.X<0||p.X>ViewWidth||p.Y<Top||p.Y>ViewHeight+Top)return;originalAudio.Play(sound.Bank,sound.Key,sound.Channel,simulation.Tick,sound.Source,p.X/ViewWidth*2-1);}
}
