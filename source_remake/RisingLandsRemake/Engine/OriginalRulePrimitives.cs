namespace RisingLands;
public static class OriginalRulePrimitives {
 // Native 0x480c48..0x480cca: integer and boolean behavior, including -1 failure.
 public static bool TryExecute(string name,int[] a,out int result){
  result=0;
  switch(name){
   case "B_Egalite":result=a[0]==a[1]?1:-1;break;
   case "B_Superieur":result=a[0]>a[1]?1:-1;break;
   case "I_Addition":result=unchecked(a[0]+a[1]);break;
   case "I_Soustraction":result=a[0]<a[1]?0:unchecked(a[0]-a[1]);break;
   case "I_PourcentageAB":result=unchecked(a[0]/a[1]*100);break;
   case "I_Affecte":result=a[0];break;
   case "I_Divise":result=a[1]==0?-1:a[0]/a[1];break;
   default:return false;
  }
  return true;
 }
}
