using System.Reflection;
using System.Security.Cryptography;
namespace RisingLands;
public static class MilitarySlotAnalysis {
 public static void Run(string root,string path){
  var raw=File.ReadAllBytes(path);var hash=Convert.ToHexString(SHA256.HashData(raw));var s=OpponentV75Tests.Load(root);s.Restore(System.Text.Encoding.UTF8.GetString(raw));long start=s.Tick;
  Console.WriteLine($"SNAPSHOT sha256={hash} tick={start} seconds={start/30.0:F2} policy={s.PolicyFor(1)}");
  Console.WriteLine("UNLOCKED "+string.Join(",",Simulation.AdvancedAttacks.Rules.Select(r=>r.Type).Where(t=>s.OpponentUnitUnlocked(1,t))));
  Console.WriteLine("RESIDENTS "+string.Join("; ",s.World.Buildings.Where(b=>b.Owner==1).Select(b=>$"slot={b.Slot} {s.BuildName(b.Type)} resident={s.Residents(b.Slot)} capacity={s.ResidentCapacity(b)} cell={b.Cell}")));
  Console.WriteLine("PLAYER BUILDINGS "+string.Join(";",s.World.Buildings.Where(b=>b.Owner==0).Select(b=>$"slot={b.Slot} type={b.Type} cell={b.Cell}")));
  var count=s.Recruitments.Count(j=>j.Owner==1);var recruit=typeof(Simulation).GetMethod("OpponentRecruit",BindingFlags.NonPublic|BindingFlags.Instance)!;recruit.Invoke(s,new object[]{1});Console.WriteLine($"RECRUIT_WITH_EIGHT before={count} after={s.Recruitments.Count(j=>j.Owner==1)}");
  var controlled=OpponentV75Tests.Load(root);controlled.Restore(System.Text.Encoding.UTF8.GetString(raw));var soldier=controlled.World.Units.First(a=>a.Owner==1&&a.Type==0&&a.Health>0);soldier.Health=0;recruit.Invoke(controlled,new object[]{1});Console.WriteLine($"CONTROL_ONLY_REMOVE_ONE army={controlled.World.Units.Count(a=>a.Owner==1&&a.Type==0&&a.Health>0)} newRecruitments={controlled.Recruitments.Count(j=>j.Owner==1)-count}");
  var previous=s.Opponent.Sorties.ToDictionary();int waves=0,attackingSamples=0;int initialPlayerHealth=s.World.Units.Where(a=>a.Owner==0).Sum(a=>a.Health);int initialDamage=s.Opponent.BuildingDamage.Values.Sum();
  int Distance(Actor a)=>s.World.Buildings.Where(b=>s.Hostile(a.Owner,b.Owner)).Select(b=>Math.Abs(a.X/1024-(b.Cell&255))+Math.Abs(a.Y/1024-(b.Cell>>8))).DefaultIfEmpty(-1).Min();
  void State(string label){var e=s.EconomyFor(1);Console.WriteLine($"{label} elapsed={(s.Tick-start)/30.0:F1} food={s.World.Resources[1]} stone={s.World.Resources[5]} army={s.World.Units.Count(a=>a.Owner==1&&a.Health>0&&Simulation.AdvancedAttacks.Rules.Any(r=>r.Type==a.Type))} recruits={s.Recruitments.Count(j=>j.Owner==1)} ready={s.EconomyReady(1)} phase={e.Phase} income={e.Income:F2} research={s.Opponent.ResearchCompleted[1]} scientists={s.EnemyScientists(1)} waves=["+string.Join(";",s.World.Units.Where(a=>a.Owner==1&&s.Opponent.Sorties.ContainsKey(a.Slot)).Select(a=>$"id={a.Slot} at={a.X/1024},{a.Y/1024} path={a.Route.Count} goal={a.Route.LastOrDefault(-1)} remainingSeconds={(s.Opponent.Sorties[a.Slot]-s.Tick)/30.0:F1} distanceToPlayerBuilding={Distance(a)} attack={a.AttackTarget}"))+"]");}
  State("INITIAL");
  for(int i=0;i<36000;i++){
   s.Step();if(s.World.Units.Any(a=>a.Owner==1&&a.AttackTarget>=0)||s.Opponent.Sieges.Any(p=>s.World.Units.Any(a=>a.Slot==p.Key&&a.Owner==1)))attackingSamples++;
   if(s.Tick%30==0){var current=s.Opponent.Sorties.ToDictionary();foreach(var p in previous.Where(p=>!current.ContainsKey(p.Key))){var a=s.World.Units.First(a=>a.Slot==p.Key);Console.WriteLine($"SORTIE_END elapsed={(s.Tick-start)/30.0:F1} id={a.Slot} deadlineReached={s.Tick>=p.Value} hp={a.Health} at={a.X/1024},{a.Y/1024} distanceToPlayerBuilding={Distance(a)} route={a.Route.Count}");}
    if(current.Keys.Except(previous.Keys).Any()){waves++;State("WAVE");}previous=current;
   }
   if(i%1800==1799)State("STATE");
  }
  Console.WriteLine($"RESULT wavesStarted={waves} attackTicks={attackingSamples} playerHealthChange={s.World.Units.Where(a=>a.Owner==0).Sum(a=>a.Health)-initialPlayerHealth} structureDamageChange={s.Opponent.BuildingDamage.Values.Sum()-initialDamage}");
  if(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))!=hash)throw new Exception("Snapshot changed");Console.WriteLine("ANALYSIS COMPLETED: snapshot preserved");
 }
}
