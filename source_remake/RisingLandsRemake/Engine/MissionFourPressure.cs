namespace RisingLands;

public sealed class MissionFourWave {
 public long RequiredSize{get;set;}=10;
 public List<int> Members{get;set;}=new();
}

public sealed partial class Simulation {
 // Tick advances only with simulation steps (30 Hz), including the speed multiplier.
 public const long MissionFourPressureTicks=60*60*30;
 public bool MissionFourPressure(int owner)=>IsFourthMission&&Tick>=MissionFourPressureTicks&&owner is >0 and <4;
 bool MissionFourCombatContinues(int owner)=>!MissionWon&&!Allied(owner)&&World.Units.Any(a=>a.Owner==0&&a.Health>0)&&World.Units.Any(a=>a.Owner==owner&&a.Health>0);
 long MissionFourRequiredSize(int owner)=>Opponent.MissionFourWaves.GetValueOrDefault(owner)?.RequiredSize??10;
 bool MissionFourActiveWave(int owner)=>Opponent.MissionFourWaves.TryGetValue(owner,out var wave)&&wave.Members.Count>0;

 void PrepareMissionFourWave(int owner){
  if(!MissionFourPressure(owner)||DefenseEmergency(owner))return;
  if(!Opponent.MissionFourWaves.TryGetValue(owner,out var wave)){
   Opponent.MissionFourWaves[owner]=wave=new();
   // Regroup an old timed sortie into the first full wave; do not count it as a defeat.
   foreach(var a in World.Units.Where(a=>a.Owner==owner&&Opponent.Sorties.ContainsKey(a.Slot))){Opponent.Sorties.Remove(a.Slot);ClearOrders(a);}
  }
  if(!MissionFourCombatContinues(owner)){
   foreach(int slot in wave.Members){Opponent.Sorties.Remove(slot);var a=World.Units.FirstOrDefault(a=>a.Slot==slot);if(a!=null)ClearOrders(a);}
   return;
  }
  if(wave.Members.Count>0){
   if(wave.Members.Any(slot=>World.Units.Any(a=>a.Slot==slot&&a.Health>0)))return;
   foreach(int slot in wave.Members)Opponent.Sorties.Remove(slot);
   wave.Members.Clear();
   // Integer ceiling, with protection against overflow of the serialized counter.
   long extra=wave.RequiredSize/4+(wave.RequiredSize%4==0?0:1);
   wave.RequiredSize=wave.RequiredSize>long.MaxValue-extra?long.MaxValue:wave.RequiredSize+extra;
  }
  var army=World.Units.Where(a=>a.Owner==owner&&DefenseAvailable(a)&&a.Satiety>0).OrderBy(a=>a.Slot).ToArray();
  army=army.Where(a=>!BaseDefenseControls(a)).ToArray();if(army.LongLength<wave.RequiredSize)return;
  foreach(var a in army.Take((int)wave.RequiredSize)){
   ClearOrders(a);wave.Members.Add(a.Slot);Opponent.Sorties[a.Slot]=long.MaxValue;
   AdvanceMissionFourAttacker(a);
  }
 }

 void AdvanceMissionFourAttacker(Actor a){
  var vision=OpponentVision(a.Owner);var structure=World.Buildings.Where(b=>b.Owner==0&&VisibleFootprint(b,vision)).OrderBy(b=>CellDistance(b.Cell,a.Y/1024*256+a.X/1024)).ThenBy(b=>b.Slot).FirstOrDefault();
  int destination=structure?.Cell??Opponent.KnownStructures.GetValueOrDefault(a.Owner,new()).Values.Where(m=>m.Building.Owner==0).OrderBy(m=>CellDistance(ActorCell(a),m.Building.Cell)).Select(m=>m.Building.Cell).FirstOrDefault(-1);
  if(destination<0){Scout(a,a.Owner);return;}
  // Advance only toward observed or last-known player structures.
  if(structure!=null&&AttackBuilding(a,structure.Slot))return;
  foreach(int cell in NearbyCells(destination,6).OrderBy(c=>CellDistance(c,destination)).ThenBy(c=>CellDistance(c,a.Y/1024*256+a.X/1024)))if(Order(a,cell&255,cell>>8,true))return;
 }

 void ValidateMissionFourWaves(Save save,OpponentState state){
  if(state.MissionFourWaves==null||state.MissionFourWaves.Count>3)throw new InvalidDataException("Invalid mission four waves");
  foreach(var p in state.MissionFourWaves){var w=p.Value;
   if(!IsFourthMission||save.Tick<MissionFourPressureTicks||p.Key is <1 or >3||w==null||w.RequiredSize<10||w.Members==null||w.Members.Count>w.RequiredSize||w.Members.Distinct().Count()!=w.Members.Count||w.Members.Any(slot=>!save.Units.Any(a=>a.Slot==slot&&a.Owner==p.Key)))throw new InvalidDataException("Invalid mission four wave");
  }
 }
}
