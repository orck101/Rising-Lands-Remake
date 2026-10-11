using RisingLands.Monitoring;
namespace RisingLands;
public sealed partial class Simulation {
 public MonitorSnapshot EconomySnapshot(int process,string session,int speed,string status,bool running=true)=>new(1,process,session,World.MapName,MissionNumber,Tick,speed,status,running,DateTimeOffset.UtcNow,Enumerable.Range(1,3).Select(owner=>{
  bool present=OriginalActorSeeds.Values.Any(a=>a.Owner==owner)||InitialBuildings.Any(b=>b.Owner==owner);int units=World.Units.Count(a=>a.Owner==owner&&a.Health>0),buildings=World.Buildings.Count(b=>b.Owner==owner);
  int branch=Campaign.EnemyBranches[owner],scientists=EnemyScientists(owner);string research=!present?"Ausente nesta missão":units==0&&buildings==0?"Clã derrotado":Opponent.ResearchCompleted[owner]>=2?"Limite da missão atingido":branch<0?"Sem pesquisa selecionada":OpponentLevel(owner,branch)>=EnemyResearchPolicy.Maximum(branch)?"Ramo no máximo":scientists==0?"Sem colonos no laboratório":"Pesquisando";
  var technologies=Enumerable.Range(0,4).Select(b=>new TechnologySnapshot(b,new[]{"Agricultura","Armamento","Engenharia","Espiritualidade"}[b],OpponentLevel(owner,b),EnemyResearchPolicy.Maximum(b),UnlockData.Technologies.Where(t=>t.Branch==b&&t.Index<OpponentLevel(owner,b)).OrderBy(t=>t.Index).Select(MonitorTechnologyBenefit).ToArray())).ToArray();
  return new EnemySnapshot(owner,owner==1?"Inimigo 1 · Vermelho":"Inimigo "+owner,present,present&&units==0&&buildings==0,World.Resources[owner],World.Resources[4+owner],World.Resources[8+owner],units,buildings,scientists,branch,Math.Clamp(Opponent.ResearchProgress[owner],0,1),Opponent.ResearchCompleted[owner],research,technologies){Relation=Allied(owner)?"Aliado":"Hostil",Sympathy=Goodwill(owner),Military=AdvancedAttacks.Rules.Select(r=>r.Type).Where(t=>t!=10).Distinct().Order().Select(t=>new MilitaryUnitSnapshot(t,MonitorUnitName(t),World.Units.Count(a=>a.Owner==owner&&a.Health>0&&a.Type==t))).ToArray()};
 }).ToArray());
 static string MonitorUnitName(int type)=>type switch{0=>"Espadachim",1=>"Incendiário",2=>"Capitão",3=>"Killer",4=>"Druida",8=>"Arqueiro",11=>"Speeder",12=>"Boomer",13=>"Unidade 13",14=>"Bomber",15=>"Hellfire",16=>"Helipede",25=>"Stomper",_=>"Unidade "+type};
 string MonitorTechnologyBenefit(Invention t){
  string name=t.Effect==0?t.Param1 switch{1=>"Liberar "+(t.Param2 switch{0=>"Espadachim",1=>"Incendiário",2=>"Capitão",4=>"Druida",5=>"Fazendeiro",6=>"Engenheiro",7=>"Construtor",8=>"Arqueiro",9=>"Colono",10=>"Mensageiro",17=>"Balão",_=>CombatRule(t.Param2).Name}),3=>"Liberar "+BuildName(t.Param2),4=>"Liberar poder espiritual "+(t.Param2+1),_=>"Tecnologia "+t.Key}:t.Param1 switch{0=>"Menor necessidade de alimento",1=>"Capacidade de carga +"+t.Param3,2=>t.Param2==9?"Cultivo precisa de menos comida":"Menor consumo de alimento",3=>(t.Param2==5?"Colheita":t.Param2==7?"Mineração":"Coleta")+" mais rápida",4=>"Alcance de tiro +"+t.Param3,5=>"Dano de combate +"+t.Param3,6=>"Melhoria de resistência",8=>"Construção mais rápida",_=>"Melhoria "+t.Key};
  return "Nível "+(t.Index+1)+": "+name;
 }
}
public sealed partial class ResearchGame {
 readonly string monitorSession=Guid.NewGuid().ToString("N");long monitorLastWrite=-1000;string monitorError="";
 string MonitorDirectory=>Path.Combine(PackageLayout.UserRoot(root),"runtime","enemy-monitor");
 void PublishEnemyMonitor(bool running=true){
  long now=Environment.TickCount64;if(running&&now-monitorLastWrite<500)return;monitorLastWrite=now;if(simulation==null)return;
  string status=!running?"Jogo encerrado":!IsActive?"Em segundo plano":pauseMenu.Open||paused||ResearchMovieOpen||marketMenu||testMenu||simulation.MissionWon?"Pausado":"Jogando";
  try{MonitorFeed.Write(MonitorDirectory,simulation.EconomySnapshot(Environment.ProcessId,monitorSession,gameSpeed,status,running));monitorError="";}
  catch(Exception e)when(e is IOException or UnauthorizedAccessException){if(monitorError!=e.Message)System.Diagnostics.Debug.WriteLine("Monitor da economia: "+e.Message);monitorError=e.Message;}
 }
}
