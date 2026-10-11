using System.Text.Json;
namespace RisingLands;
public static class AudioV69Tests {
 public static bool Run(string root){
  int checks=0;bool passed=true;void Check(bool ok,string name){checks++;passed&=ok;Console.WriteLine((ok?"PASS: ":"FAIL: ")+name);}
  var setup=ImprovementsV67Tests.Fixture(root,false);var w=setup.World;var seed=w.Units.First(a=>a.Owner==0&&a.Type==5);var farmers=new List<Actor>();
  for(int n=0;n<10;n++){var a=JsonSerializer.Deserialize<Actor>(JsonSerializer.Serialize(seed))!;a.Slot=3000+n;a.X=(2+n)*1024;a.Y=4*1024;a.Satiety=a.SatietyMaximum;a.FatigueClock=0;farmers.Add(a);w.Units.Add(a);int cell=2+n+5*256;w.Objects[cell]=67;w.Quantities[cell]=20;}
  var s=new Simulation(w,setup.Campaign);var events=new List<GameSound>();s.SoundIssued+=events.Add;
  Check(farmers.All(a=>s.Gather(a,a.X/1024+5*256)&&s.SetFoodDestination(a,1,a.X/1024+7*256)),"ten farmers receive valid repeated collection orders");
  events.Clear();for(int i=0;i<2200;i++)s.Step();
  var automatic=events.Where(e=>farmers.Any(a=>a.Slot==e.Source)).ToArray();var chatter=automatic.Where(e=>e.Bank=="P"&&((e.Key>>8&255)==8||(e.Key>>16)==5&&(e.Key>>8&255)==16)&&AudioPlaybackPolicy.CanPlay(e.Bank,e.Key,e.Channel)).ToArray();
  Console.WriteLine(JsonSerializer.Serialize(new{farmers=10,ticks=2200,movement_events=automatic.Count(e=>(e.Key>>8&255)==8),work_events=automatic.Count(e=>(e.Key>>8&255)==16),audible_automatic_chatter=chatter.Length,delivered=farmers.Sum(a=>(int)w.Quantities[a.X/1024+7*256])}));
  Check(chatter.Length==0,"autonomous collection and walking do not play farmer speech");
  Console.WriteLine(JsonSerializer.Serialize(farmers.Select(a=>new{a.Slot,a.FoodJob,drops=a.GroundDrops.Count})));
  Check(farmers.All(a=>a.GroundDrops.Count>0),"all ten farmers continue delivering without new orders");
  Check(AudioPlaybackPolicy.CanPlay("P",0x50800,4),"explicit farmer selection keeps original voice");
  Check(AudioPlaybackPolicy.CanPlay("P",0x50f00,4),"explicit collection order keeps original acknowledgement");
  Check(AudioPlaybackPolicy.CanPlay("P",0x72500,2),"construction hammer remains audible");
  Check(AudioPlaybackPolicy.CanPlay("P",0x50700,2)&&AudioPlaybackPolicy.CanPlay("P",0x50500,2),"combat and death effects remain audible");
  Check(AudioPlaybackPolicy.CanPlay("A",0,3),"game alerts remain available");
  Console.WriteLine($"AUDIO v69 CHECKS: {checks}; PASSED: {passed}");return passed;
 }
}
