namespace RisingLands;
public readonly record struct RangeDisk(double X,double Y,double Radius);
public readonly record struct RangeArc(RangeDisk Circle,double Start,double End);
// Exact exposed arcs of the union of circular ranges. No enclosing circle or hull inflation.
public static class RangeOutline {
 public const double Tau=2*Math.PI;
 const double Epsilon=1e-9;
 public static List<RangeArc> Build(IReadOnlyList<RangeDisk> input){
  var disks=input.Where(d=>double.IsFinite(d.X)&&double.IsFinite(d.Y)&&double.IsFinite(d.Radius)&&d.Radius>0).Distinct().ToArray();
  var result=new List<RangeArc>();
  for(int i=0;i<disks.Length;i++){
   var c=disks[i];var covered=new List<(double Start,double End)>();bool hidden=false;
   for(int j=0;j<disks.Length;j++){
    if(i==j)continue;var other=disks[j];double dx=other.X-c.X,dy=other.Y-c.Y,d=Math.Sqrt(dx*dx+dy*dy);
    if(d+c.Radius<=other.Radius+Epsilon){hidden=true;break;}
    if(d>=c.Radius+other.Radius-Epsilon||d+other.Radius<=c.Radius+Epsilon)continue;
    double middle=Math.Atan2(dy,dx);if(middle<0)middle+=Tau;
    double half=Math.Acos(Math.Clamp((d*d+c.Radius*c.Radius-other.Radius*other.Radius)/(2*d*c.Radius),-1,1));
    double start=middle-half,end=middle+half;
    if(start<0){covered.Add((start+Tau,Tau));covered.Add((0,end));}
    else if(end>Tau){covered.Add((start,Tau));covered.Add((0,end-Tau));}
    else covered.Add((start,end));
   }
   if(hidden)continue;
   covered.Sort((a,b)=>a.Start.CompareTo(b.Start));double cursor=0;
   foreach(var interval in covered){if(interval.Start>cursor+Epsilon)result.Add(new(c,cursor,interval.Start));cursor=Math.Max(cursor,interval.End);}
   if(cursor<Tau-Epsilon)result.Add(new(c,cursor,Tau));
  }
  return result;
 }
}
