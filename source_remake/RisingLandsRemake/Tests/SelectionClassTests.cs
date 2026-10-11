namespace RisingLands;
public static class SelectionClassTests {
 public static void Run(){
  var a=new Actor{Slot=1,Type=5,Health=10,LegacyState=7,Food=1,FoodJob=3};var b=new Actor{Slot=2,Type=5,Health=10,LegacyState=7};var c=new Actor{Slot=3,Type=8,Health=10,LegacyState=7};var outside=new Actor{Slot=4,Type=5,Health=10,LegacyState=7};var units=new[]{a,b,c};
  void Check(bool v,string name){if(!v)throw new Exception("FAIL: "+name);Console.WriteLine("PASS: "+name);}
  Check(SelectionClasses.Types(units).SequenceEqual(new[]{5,8}),"selection classes are distinct and include workers and soldiers");
  Check(SelectionClasses.Filter(units,5).SequenceEqual(new[]{a,b})&&!SelectionClasses.Filter(units,5).Contains(outside),"class filtering keeps only matching units of the selected group");
  Check(a.Food==1&&a.FoodJob==3,"class filtering preserves cargo and current orders");
  Check(SelectionClasses.Filter(units,8).Single()==c,"soldier class can be filtered independently");
  Check(SelectionClasses.Types(Array.Empty<Actor>()).Length==0&&!SelectionClasses.Filter(units,7).Any(),"empty selection and absent class remain empty");
  c.Health=0;Check(SelectionClasses.Types(units).SequenceEqual(new[]{5}),"defeated unit is removed from class list");
 }
}
