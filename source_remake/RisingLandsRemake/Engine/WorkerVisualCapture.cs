namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareWorkerVisualCapture(string mode){
  bool builder=mode is "worker-mining" or "worker-stone";
  SelectOnly(simulation.World.Units.First(u=>u.Type==(builder?7:5)&&u.Owner==0));
  var u=selected!;u.RepeatFood=false;u.RepeatMine=false;
  if(builder)simulation.MineNearest(u);else {bool found=Enumerable.Range(0,65536).Where(c=>simulation.World.Objects[c] is >=29 and <=52).OrderBy(c=>Math.Abs((c&255)-u.X/1024)+Math.Abs((c>>8)-u.Y/1024)).Any(c=>simulation.Gather(u,c));if(!found)throw new InvalidDataException("No reachable crop for capture");}
  for(int i=0;i<20000;i++){
   if(mode=="worker-mining"&&u.MineJob==2||mode=="worker-stone"&&u.Rock>0||mode=="worker-hoeing"&&u.FoodJob==2||mode=="worker-food"&&u.Food>0)break;
   simulation.Step();
  }
  Center();message=mode switch{"worker-mining"=>"Construtor dentro da jazida.","worker-stone"=>"Construtor carregando rocha.","worker-hoeing"=>"Fazendeiro colhendo a plantacao.",_=>"Fazendeiro carregando alimento."};
 }
}
