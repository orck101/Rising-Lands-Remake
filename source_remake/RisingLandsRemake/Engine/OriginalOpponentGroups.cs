using System.Text.Json;
namespace RisingLands;
public sealed class OriginalOpponentGroup {
 public int Id{get;set;}public int Owner{get;set;}public int Type{get;set;}public bool Active{get;set;}
 public int Maximum{get;set;}=32767;public int Behavior{get;set;}=255;public int Target{get;set;}=-1;
 public List<int> Units{get;set;}=new();
}
public sealed class OriginalOpponentGroups {
 // iaGroupNew: shared 100-entry pool; owner/type chains insert at the head.
 public List<OriginalOpponentGroup> Pool{get;set;}=Enumerable.Range(0,100).Select(i=>new OriginalOpponentGroup{Id=i}).ToList();
 public Dictionary<string,List<int>> Chains{get;set;}=new();public int Cursor{get;set;}
 static string Key(int owner,int type)=>owner+":"+type;
 public int Create(int owner,int type){
  int id=Enumerable.Range(Cursor,100-Cursor).FirstOrDefault(i=>!Pool[i].Active,-1);if(id<0)return -1;
  Pool[id]=new(){Id=id,Owner=owner,Type=type,Active=true};if(!Chains.TryGetValue(Key(owner,type),out var chain))Chains[Key(owner,type)]=chain=new();chain.Insert(0,id);
  // The original increments its previous cursor, not the selected free index.
  Cursor=Cursor<99?Cursor+1:0;return id;
 }
 public void Delete(int owner,int id){if(id<0||id>=100)return;var group=Pool[id];group.Active=false;if(id<Cursor)Cursor=id;if(Chains.TryGetValue(Key(owner,group.Type),out var chain))chain.Remove(id);}
 public int First(int owner,int type)=>Chains.GetValueOrDefault(Key(owner,type))?.FirstOrDefault(-1)??-1;
 public int NotFull(int owner,int type)=>Chains.GetValueOrDefault(Key(owner,type))?.FirstOrDefault(id=>Pool[id].Units.Count<Pool[id].Maximum,-1)??-1;
 public void Add(int group,int unit)=>Pool[group].Units.Insert(0,unit);
 public bool Remove(int group,int unit)=>Pool[group].Units.Remove(unit);
 public int Find(int group,int type,Func<int,int> unitType)=>Pool[group].Units.FirstOrDefault(slot=>unitType(slot)==type,-1);
 public int TransferFromPool(int owner,int destination,int type,Func<int,int> unitType){int source=First(owner,0);if(source<0)return -1;int unit=Find(source,type,unitType);if(unit<0)return -1;Remove(source,unit);Add(destination,unit);return unit;}
 public void ReturnToPool(int owner,int group){int target=First(owner,0);while(Pool[group].Units.Count>0){int unit=Pool[group].Units[0];Remove(group,unit);if(target>=0)Add(target,unit);}}
 public string Serialize()=>JsonSerializer.Serialize(this);
 public static OriginalOpponentGroups Restore(string json){var s=JsonSerializer.Deserialize<OriginalOpponentGroups>(json)??throw new InvalidDataException("Empty original groups");s.Validate();return s;}
 public void Validate(){if(Pool==null||Pool.Count!=100||Cursor<0||Cursor>=100||Chains==null)throw new InvalidDataException("Invalid original group pool");var ids=new HashSet<int>();var units=new HashSet<int>();foreach(var pair in Chains){if(pair.Value==null)throw new InvalidDataException("Missing original group chain");foreach(int id in pair.Value){if(id<0||id>=100||!ids.Add(id)||!Pool[id].Active||Key(Pool[id].Owner,Pool[id].Type)!=pair.Key)throw new InvalidDataException("Invalid original group chain");}}
  foreach(var g in Pool){if(g==null||g.Id<0||g.Id>=100||Pool[g.Id]!=g||g.Active!=ids.Contains(g.Id)||g.Owner<0||g.Owner>4||g.Type<0||g.Type>20||g.Maximum<0||g.Maximum>65535||g.Units==null)throw new InvalidDataException("Invalid original group record");if(g.Active)foreach(int unit in g.Units)if(unit<0||!units.Add(unit))throw new InvalidDataException("Unit appears in multiple original groups");}
 }
}
