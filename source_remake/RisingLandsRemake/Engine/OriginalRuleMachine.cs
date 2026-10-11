using System.Text.Json;
namespace RisingLands;

public sealed record OriginalRuleNode(int Index,string? Function,int? Root,uint Descriptor,int Success,int Failure,int Immediate,int Offset,string? Address) {
 public int ArgumentCount=>(int)(Descriptor>>4&15);
 public int Destination=>(int)(Descriptor&15);
 public int Selector(int index)=>(int)(Descriptor>>(8+index*4)&15);
}
public sealed record OriginalRuleProgram(string SourceHash,int[] Roots,List<OriginalRuleNode> Nodes);
public sealed class OriginalProblem {
 public int Id{get;set;} public int Parent{get;set;}=-1;public int Node{get;set;}
 public int[] Variables{get;set;}=new int[15];public int Delay{get;set;}public int Value{get;set;}public int ChildRoot{get;set;}
 public int Owner{get;set;}public bool Waiting{get;set;}public bool Returned{get;set;}
}
public sealed class OriginalRuleMachine {
 public const int Empty=0x1a1a000e,Exhausted=0x1a1a000c;
 public OriginalRuleProgram Program{get;}
 public List<OriginalProblem> Pool{get;set;}=Enumerable.Range(0,100).Select(i=>new OriginalProblem{Id=i}).ToList();
 public List<int> Free{get;set;}=Enumerable.Range(0,100).ToList();
 public Dictionary<int,List<int>> Queues{get;set;}=new();
 public Func<OriginalProblem,string,int[],int> Invoke{get;set;}=(_,name,args)=>OriginalRulePrimitives.TryExecute(name,args,out int result)?result:throw new NotSupportedException("Original IA primitive requires an adapter: "+name);
 public OriginalRuleMachine(OriginalRuleProgram program){Program=program;}
 public sealed record SavedState(string SourceHash,List<OriginalProblem> Pool,List<int> Free,Dictionary<int,List<int>> Queues);
 public string Serialize()=>JsonSerializer.Serialize(new SavedState(Program.SourceHash,Pool,Free,Queues));
 public void Restore(string json){
  var state=JsonSerializer.Deserialize<SavedState>(json)??throw new InvalidDataException("Empty original IA state");
  if(state.SourceHash!=Program.SourceHash||state.Pool==null||state.Pool.Count!=100||state.Free==null||state.Queues==null||state.Queues.Any(p=>p.Key is <0 or >4||p.Value==null))throw new InvalidDataException("Invalid original IA state");
  var occupied=state.Queues.Values.SelectMany(q=>q).ToArray();var all=occupied.Concat(state.Free).ToArray();
  if(all.Length!=100||all.Any(id=>id is <0 or >=100)||all.Distinct().Count()!=100)throw new InvalidDataException("Invalid original IA task allocation");
  foreach(int id in Enumerable.Range(0,100)){var p=state.Pool[id];if(p==null||p.Id!=id||p.Variables==null||p.Variables.Length!=15)throw new InvalidDataException("Invalid original IA task variables");}
  foreach(var queue in state.Queues)foreach(int id in queue.Value){var p=state.Pool[id];if(p.Owner!=queue.Key||p.Node<4||p.Node>=Program.Nodes.Count||p.Delay<0||p.ChildRoot is <0 or >=64||p.Parent< -1||p.Parent>=100||p.Parent==id||p.Parent>=0&&!queue.Value.Contains(p.Parent))throw new InvalidDataException("Invalid original IA continuation");
   var parents=new HashSet<int>{id};for(int parent=p.Parent;parent>=0;parent=state.Pool[parent].Parent)if(!parents.Add(parent))throw new InvalidDataException("Cyclic original IA continuation");
  }
  Pool=state.Pool;Free=state.Free;Queues=state.Queues;
 }
 public static OriginalRuleProgram Load()=>JsonSerializer.Deserialize<OriginalRuleProgram>(File.ReadAllText(PackageLayout.FromBase("Content/Data/opponent_rules.json")))??throw new InvalidDataException("Original RULE program missing");
 OriginalProblem? Allocate(int root){if(root<=0||root>=64||Program.Roots[root]==0||Free.Count==0)return null;int id=Free[0];Free.RemoveAt(0);var p=Pool[id];p.Parent=-1;p.Node=Program.Roots[root];p.Delay=0;p.ChildRoot=0;p.Waiting=p.Returned=false;return p;}
 public int Post(int owner,int root,params int[] parameters){var p=Allocate(root);if(p==null)return -1;p.Owner=owner;for(int i=0;i<6;i++)p.Variables[i]=i<parameters.Length?parameters[i]:0;if(!Queues.TryGetValue(owner,out var queue))Queues[owner]=queue=new();queue.Add(p.Id);return p.Id;}
 public void Sleep(OriginalProblem p,int delay){p.Waiting=true;p.Delay=delay;}
 public void Resume(OriginalProblem p,int value){p.Waiting=false;p.Returned=true;p.Delay=0;p.Value=value;}
 void Remove(OriginalProblem p,int? result){if(p.Parent>=0){var parent=Pool[p.Parent];if(result.HasValue)parent.Value=result.Value;parent.Waiting=false;parent.Returned=true;}Queues[p.Owner].Remove(p.Id);Free.Insert(0,p.Id);}
 public int Run(int owner,int instructionBudget=10000){
  if(!Queues.TryGetValue(owner,out var queue)||queue.Count==0)return Pool.Count==Free.Count?Empty:0;
  // iaSolver stores the next link before execution; newly inserted children wait.
  foreach(int id in queue.ToArray()){
   if(!queue.Contains(id))continue;var p=Pool[id];
   if(p.Waiting){if(p.Delay<=0)continue;if(--p.Delay>0)continue;Resume(p,1);}
   while(true){
    if(--instructionBudget<0)throw new InvalidDataException("Original IA rule instruction budget exceeded");
    if(p.Node<0||p.Node>=Program.Nodes.Count)throw new InvalidDataException("Invalid original IA instruction");
    var node=Program.Nodes[p.Node];if(node.ArgumentCount>6)throw new InvalidDataException("Invalid original IA argument count");
    int value;
    if(p.Returned){if(p.ChildRoot!=0)p.ChildRoot=0;else if(node.Destination>0)p.Variables[node.Destination-1]=p.Value;p.Returned=false;value=p.Value;}
    else {
     var args=Enumerable.Range(0,node.ArgumentCount).Select(i=>node.Selector(i)>0?p.Variables[node.Selector(i)-1]:node.Immediate).ToArray();
     if(node.Function==null){
      var child=Allocate(node.Root??0);if(child==null){Remove(p,null);break;}
      for(int i=0;i<args.Length;i++)child.Variables[i]=args[i];child.Owner=p.Owner;child.Parent=p.Id;
      queue.Insert(queue.IndexOf(p.Id)+1,child.Id);p.Waiting=true;p.ChildRoot=node.Root??0;break;
     }
     value=Invoke(p,node.Function,args);if(p.Waiting)break;if(node.Destination>0)p.Variables[node.Destination-1]=value;
    }
    int next=value==-1?node.Failure:node.Success;
    if(next<=3){if(next is 0 or 3){Remove(p,null);break;}Remove(p,next==1?1:-1);break;}
    p.Node=next;
   }
  }
  return Free.Count==100?Empty:0;
 }
}
