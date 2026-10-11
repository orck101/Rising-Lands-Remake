using System.Text.Json;
namespace RisingLands;
public static class OriginalOpponentGroupsTests {
 record Group(int Id,int Type,int Maximum,List<int> Units);
 record Snapshot(List<Group> Groups,Dictionary<string,List<int>> Chains,int Cursor);
 record Action(string Op,int Owner,int Value,int Extra,uint Result,Snapshot Snapshot);
 record Cases(Dictionary<string,int> Types,List<Action> Actions);
 public static void Run(){var data=JsonSerializer.Deserialize<Cases>(File.ReadAllText(PackageLayout.FromBase("Content/Data/opponent_group_cases.json")))!;var groups=new OriginalOpponentGroups();int checks=0;
  foreach(var a in data.Actions){int result=0;switch(a.Op){case "create":result=groups.Create(a.Owner,a.Value);break;case "delete":groups.Delete(a.Owner,a.Value);break;case "add":groups.Add(a.Value,a.Extra);break;case "remove":groups.Remove(a.Value,a.Extra);break;case "max":groups.Pool[a.Value].Maximum=a.Extra;break;case "find":result=groups.Find(a.Value,a.Extra,id=>data.Types[id.ToString()]);break;case "transfer":result=groups.TransferFromPool(a.Owner,a.Value,a.Extra,id=>data.Types[id.ToString()]);break;case "return":groups.ReturnToPool(a.Owner,a.Value);break;default:throw new Exception(a.Op);}
   var actual=groups.Pool.Where(g=>g.Active).Select(g=>new Group(g.Id,g.Type,g.Maximum,g.Units)).ToList();var nonempty=groups.Chains.Where(p=>p.Value.Count>0).ToDictionary(p=>p.Key,p=>p.Value);
   if(JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(a.Snapshot.Groups)||nonempty.Count!=a.Snapshot.Chains.Count||nonempty.Any(p=>!a.Snapshot.Chains.TryGetValue(p.Key,out var chain)||!p.Value.SequenceEqual(chain))||groups.Cursor!=a.Snapshot.Cursor)throw new Exception("Original group state mismatch at "+checks+" "+a.Op);
   if(a.Op=="create"&&unchecked((uint)result)!=a.Result||a.Op is "find" or "transfer"&&(result&65535)!=(a.Result&65535))throw new Exception("Original group result mismatch");
   groups=OriginalOpponentGroups.Restore(groups.Serialize());checks++;
  }Console.WriteLine("PASS v78: "+checks+" original group state comparisons and restores");
 }
}
