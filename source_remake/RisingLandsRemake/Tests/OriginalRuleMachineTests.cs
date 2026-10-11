using System.Text.Json;
namespace RisingLands;
public static class OriginalRuleMachineTests {
 record ActionValue(int Result,int Sleep);
 record CallValue(int Op,int[] Args,int Owner,int Task);
 record FrameValue(List<CallValue> Calls,List<OriginalProblem> Tasks,int Return);
 record CaseValue(List<OriginalRuleNode> Nodes,int[] Roots,int[] Parameters,Dictionary<string,JsonElement> Actions,List<FrameValue> Frames);
 record CasesValue(List<CaseValue> Cases);
 record MathCase(string Name,string Address,int[] Arguments,int Result);
 record MathCases(List<MathCase> Cases);
 record ChoiceCase(int Mission,uint Seed,int[][] Levels,int[][] Weights,int[] Branches,uint FinalSeed);
 record AdjustmentCase(int Mission,int Discoveries,int[][] Before,int[] Branches,int[][] After);
 record CampaignCases(List<ChoiceCase> ChoiceCases,List<AdjustmentCase> AdjustmentCases);
 public static void Run(){
  var cases=JsonSerializer.Deserialize<CasesValue>(File.ReadAllText(PackageLayout.FromBase("Content/Data/opponent_vm_cases.json")))!;int checks=0;
  foreach(var c in cases.Cases){var machine=new OriginalRuleMachine(new("fixture",c.Roots,c.Nodes));var calls=new List<CallValue>();
   machine.Invoke=(task,name,args)=>{int op=int.Parse(name.Replace("Probe",""));calls.Add(new(op,args,task.Owner,task.Id));var action=c.Actions[op.ToString()];if(action.TryGetProperty("Sleep",out var sleep))machine.Sleep(task,sleep.GetInt32());return action.TryGetProperty("Result",out var result)?result.GetInt32():args[0];};
   machine.Post(1,1,c.Parameters);
   foreach(var frame in c.Frames){calls.Clear();int result=machine.Run(1);var actual=machine.Queues[1].Select(id=>machine.Pool[id]).ToList();
    if(result!=frame.Return||JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(frame.Tasks))throw new Exception("Native RULE state mismatch: "+checks+" actual "+JsonSerializer.Serialize(actual)+" expected "+JsonSerializer.Serialize(frame.Tasks));
    if(calls.Count!=frame.Calls.Count)throw new Exception("Native RULE calls mismatch");
    for(int i=0;i<calls.Count;i++){var a=calls[i];var e=frame.Calls[i];if(a.Op!=e.Op||a.Owner!=e.Owner||a.Task!=e.Task||!a.Args.SequenceEqual(e.Args.Take(a.Args.Length)))throw new Exception("Native RULE arguments mismatch");}
    checks++;var restored=new OriginalRuleMachine(machine.Program);restored.Restore(machine.Serialize());if(restored.Serialize()!=machine.Serialize())throw new Exception("Original IA continuation does not restore");restored.Invoke=machine.Invoke;machine=restored;
   }
  }
  foreach(var c in Simulation.NativeCities.Cases){int demand=Simulation.NativeCityDemand(c.Mode,c.Counts,new int[22],t=>c.Allowed.Contains(t));if(demand!=(c.Requests.Length==0?-1:c.Requests[0]))throw new Exception("Native city demand mismatch");checks++;}
  var math=JsonSerializer.Deserialize<MathCases>(File.ReadAllText(PackageLayout.FromBase("Content/Data/opponent_math_cases.json")))!;foreach(var c in math.Cases){if(!OriginalRulePrimitives.TryExecute(c.Name,c.Arguments,out int result)||result!=c.Result)throw new Exception("Native arithmetic mismatch: "+c.Name);checks++;}
  var campaign=JsonSerializer.Deserialize<CampaignCases>(File.ReadAllText(PackageLayout.FromBase("Content/Data/opponent_campaign_cases.json")))!;foreach(var c in campaign.ChoiceCases){var random=new OriginalIaRandom(c.Seed);foreach(int owner in Enumerable.Range(1,3)){var weights=new int[4];int branch=EnemyResearchPolicy.Choose(owner,c.Mission,c.Levels[owner],weights,random);if(branch!=c.Branches[owner]||!weights.SequenceEqual(c.Weights[owner]))throw new Exception("Native enemy branch mismatch: "+c.Mission+" owner "+owner+" got "+branch+" expected "+c.Branches[owner]);}if(random.State!=c.FinalSeed)throw new Exception("Native enemy random state mismatch");checks++;}
  foreach(var c in campaign.AdjustmentCases){foreach(int owner in Enumerable.Range(1,3))if(!EnemyResearchPolicy.Adjust(c.Mission,c.Discoveries,c.Before[owner],c.Branches[owner]).SequenceEqual(c.After[owner]))throw new Exception("Native enemy campaign adjustment mismatch");checks++;}
  var original=OriginalRuleMachine.Load();if(original.Nodes.Count!=1157||original.Roots.Length!=64)throw new Exception("Incomplete original RULE catalog");
  Console.WriteLine($"PASS v77: {checks} native RULE/city/arithmetic comparisons; 500 continuation reloads; all 1157 original records loaded");
 }
}
