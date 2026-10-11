namespace RisingLands;
public static class AudioV81Tests {
 public static void Run(string root){int checks=0;void Check(bool ok,string name){if(!ok)throw new Exception("FAIL v81: "+name);checks++;Console.WriteLine("PASS v81: "+name);}
  var w=LegacyAssets.Map(PackageLayout.Asset(root,"CD/WORLD/MONDE04.ETA"));new VisualCatalog().Apply(w);var s=new Simulation(w);var events=new List<GameSound>();s.SoundIssued+=events.Add;
  for(int i=0;i<6*60*30;i++)s.Step();var enemyVoices=events.Where(e=>e.Owner!=0&&e.Bank=="P"&&e.Channel==4).ToArray();
  Console.WriteLine("Enemy automatic voice requests in six minutes: "+enemyVoices.Length);
  Check(enemyVoices.Length>0,"real enemy simulation reproduces automatic voice requests");
  Check(enemyVoices.All(e=>!AudioPlaybackPolicy.CanPlay(e)),"enemy automatic responses cannot reach playback");
  Check(AudioPlaybackPolicy.CanPlay(new("P",0x50800,4,0,0,1)),"player selection keeps its voice");
  Check(AudioPlaybackPolicy.CanPlay(new("P",0x50f00,4,0,0,1)),"player collection order keeps its voice");
  Check(AudioPlaybackPolicy.CanPlay(new("P",0x20700,2,0,1,1)),"enemy combat effects stay audible");
  Check(AudioPlaybackPolicy.CanPlay(new("P",0x20500,2,0,1,1)),"enemy death effects stay audible");
  var active=new[]{new AudioActivity("P",0x20700,1,2),new AudioActivity("P",0x20700,2,2)};
  Check(!AudioConcurrency.CanStart(active,"P",0x20700,3,2),"crowded combat caps simultaneous copies of the same effect");
  Check(AudioConcurrency.CanStart(active,"P",0x20500,3,2),"different combat effect can still play");
  Check(AudioConcurrency.CanStart(active.Take(1),"P",0x20700,3,2),"two nearby combatants may share an effect");
  Console.WriteLine("PASS v81 total "+checks);
 }
}
