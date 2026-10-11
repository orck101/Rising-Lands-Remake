namespace RisingLands;
public static class SaveSlotTests {
 static void Check(bool value,string name){if(!value)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
 public static void Run(string root){
  string directory=Path.GetFullPath(PackageLayout.Asset(root,"../../reports/v45-slot-tests"));Directory.CreateDirectory(directory);
  // Each invocation uses its own isolated test directory, never player slots.
  directory=Path.Combine(directory,Guid.NewGuid().ToString("N"));var s=MissionOneTests.Load(root);var store=new SaveSlots(directory,s.World.MapName);var states=new string[5];
  Check(Enumerable.Range(1,5).All(i=>store.Existing(i)==null&&store.Description(i).Contains("Vazio")),"five new slots are empty");
  for(int slot=1;slot<=5;slot++){for(int i=0;i<20;i++)s.Step();states[slot-1]=s.Serialize();store.Save(s,slot);Check(File.Exists(store.PathFor(slot)),"slot saved independently "+slot);}
  Check(states.Distinct().Count()==5,"five slots preserve distinct game states");
  foreach(int slot in new[]{5,2,4,1,3}){store.Load(s,slot);Check(s.Serialize()==states[slot-1],"arbitrary slot restores its own full state "+slot);}
  store.Save(s,2);Check(File.ReadAllText(store.PathFor(1))==states[0]&&File.ReadAllText(store.PathFor(5))==states[4],"overwriting slot two leaves other slots unchanged");
  string before=s.Serialize();File.WriteAllText(store.PathFor(4),"invalid");try{store.Load(s,4);throw new Exception("corrupt slot accepted");}catch(System.Text.Json.JsonException){}Check(s.Serialize()==before&&store.Description(4).Contains("invalido"),"corrupt slot preserves current game and reports invalid");
  var empty=new SaveSlots(directory+"-empty",s.World.MapName);try{empty.Load(s,5);throw new Exception("empty slot accepted");}catch(FileNotFoundException){}Check(s.Serialize()==before,"empty slot preserves current game");
  try{store.PathFor(6);throw new Exception("sixth slot accepted");}catch(ArgumentOutOfRangeException){}Check(true,"slot bounds reject unavailable slot");
  var legacy=Path.Combine(directory,"legacy");Directory.CreateDirectory(legacy);s.SaveTo(Path.Combine(legacy,s.World.MapName+".research-v17.json"));var migrated=new SaveSlots(directory+"-migrated",s.World.MapName,new[]{legacy});Check(migrated.Existing(1)!=null&&migrated.Existing(2)==null,"legacy save appears in slot one without copying or modifying it");
  string oldSlotDir=directory+"-v17";Directory.CreateDirectory(oldSlotDir);var oldSlots=new SaveSlots(oldSlotDir,s.World.MapName);string oldPath=System.Text.RegularExpressions.Regex.Replace(oldSlots.PathFor(3),@"research-v\d+\.json$","research-v17.json");var oldJson=System.Text.Json.Nodes.JsonNode.Parse(s.Serialize())!;oldJson["Version"]=17;oldJson.AsObject().Remove("Campaign");string oldBytes=oldJson.ToJsonString();File.WriteAllText(oldPath,oldBytes);oldSlots.Load(s,3);Check(oldSlots.Existing(3)==oldPath&&File.ReadAllText(oldPath)==oldBytes,"v17 numbered slot loads without modifying previous save");oldSlots.Save(s,3);Check(oldSlots.Existing(3)==oldSlots.PathFor(3)&&File.ReadAllText(oldPath)==oldBytes,"current save takes precedence while retaining v17 original slot");
  foreach(var page in new[]{MenuPage.Save,MenuPage.Load}){var menu=new PauseMenuState();menu.Show(page);menu.Move(-1,6);Check(menu.Choice==5,"slot menu keyboard wraps to return "+page);menu.Back();Check(menu.Page==MenuPage.Main,"slot menu returns to main "+page);}
 }
}
