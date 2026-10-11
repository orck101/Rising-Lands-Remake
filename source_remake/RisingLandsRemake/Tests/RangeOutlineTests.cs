namespace RisingLands;
public static class RangeOutlineTests {
 static void Check(bool ok,string label){if(!ok)throw new Exception("FAIL: range union "+label);Console.WriteLine("PASS: range union "+label);}
 static double Length(List<RangeArc> arcs)=>arcs.Sum(a=>(a.End-a.Start)*a.Circle.Radius);
 static void Near(double value,double expected,string label)=>Check(Math.Abs(value-expected)<1e-7,label);
 static void Validate(RangeDisk[] disks,string label){
  var arcs=RangeOutline.Build(disks);
  foreach(var arc in arcs){Check(arc.Start>=0&&arc.End<=RangeOutline.Tau&&arc.End>arc.Start,label+" valid angular interval");for(int i=0;i<9;i++){double angle=arc.Start+(arc.End-arc.Start)*(i+.5)/9;double x=arc.Circle.X+Math.Cos(angle)*arc.Circle.Radius,y=arc.Circle.Y+Math.Sin(angle)*arc.Circle.Radius;Check(disks.All(d=>Math.Pow(x-d.X,2)+Math.Pow(y-d.Y,2)>=d.Radius*d.Radius-1e-6),label+" exposed point stays outside every other range");}}
 }
 public static void Run(){
  var pair=new[]{new RangeDisk(0,0,10),new RangeDisk(10,0,10)};
  Near(Length(RangeOutline.Build(pair)),80*Math.PI/3,"overlapping pair has only exterior perimeter");
  Near(Length(RangeOutline.Build(new[]{new RangeDisk(0,0,10)})),20*Math.PI,"single troop retains entire circle");
  Check(RangeOutline.Build(Array.Empty<RangeDisk>()).Count==0,"empty selection has no contour");
  Near(Length(RangeOutline.Build(new[]{new RangeDisk(0,0,10),new RangeDisk(30,0,10)})),40*Math.PI,"separated troops retain two complete circles");
  Near(Length(RangeOutline.Build(new[]{new RangeDisk(0,0,10),new RangeDisk(20,0,10)})),40*Math.PI,"tangent circles retain real exterior boundaries");
  Near(Length(RangeOutline.Build(new[]{new RangeDisk(0,0,10),new RangeDisk(2,0,1)})),20*Math.PI,"contained melee range adds no internal contour");
  Near(Length(RangeOutline.Build(new[]{new RangeDisk(0,0,10),new RangeDisk(9,0,1)})),20*Math.PI,"internally tangent smaller circle is hidden");
  Near(Length(RangeOutline.Build(new[]{new RangeDisk(0,0,10),new RangeDisk(0,0,10)})),20*Math.PI,"overlapping identical circles appear once");
  Near(Length(RangeOutline.Build(new[]{new RangeDisk(0,0,1),new RangeDisk(0,0,10)})),20*Math.PI,"concentric ranges retain only largest");
  var surrounded=new[]{new RangeDisk(0,0,1),new RangeDisk(1,0,1.5),new RangeDisk(-1,0,1.5),new RangeDisk(0,1,1.5),new RangeDisk(0,-1,1.5)};
  Check(!RangeOutline.Build(surrounded).Any(a=>a.Circle==surrounded[0]),"circle covered by several neighbors has no exposed boundary");
  Validate(pair,"pair");
  Validate(new[]{new RangeDisk(0,0,10),new RangeDisk(10,0,5)},"mixed radii");
  Validate(new[]{new RangeDisk(0,0,10),new RangeDisk(-10,1,10),new RangeDisk(-10,-1,10)},"coverage wrapping around zero angle");
  var dense=Enumerable.Range(0,36).Select(i=>new RangeDisk((i%6)*2,(i/6)*2,i%5==0?1:10)).ToArray();
  Validate(dense,"large formation");Check(Length(RangeOutline.Build(dense))<dense.Sum(d=>d.Radius*RangeOutline.Tau)/5,"dense formation eliminates redundant circles");
  var reverse=RangeOutline.Build(dense.Reverse().ToArray());Near(Length(reverse),Length(RangeOutline.Build(dense)),"selection order does not alter covered area");
  var translated=dense.Select(d=>d with{X=d.X+12345,Y=d.Y-8765}).ToArray();Near(Length(RangeOutline.Build(translated)),Length(RangeOutline.Build(dense)),"camera independent world geometry");
  var scaled=dense.Select(d=>new RangeDisk(d.X*3,d.Y*3,d.Radius*3)).ToArray();Near(Length(RangeOutline.Build(scaled)),3*Length(RangeOutline.Build(dense)),"zoom preserves shape and scales perimeter");
  var random=new Random(64);for(int k=0;k<12;k++){var ds=Enumerable.Range(0,9).Select(_=>new RangeDisk(random.NextDouble()*30,random.NextDouble()*30,1+random.NextDouble()*12)).ToArray();Validate(ds,"random group "+k);}
  Check(RangeOutline.Build(new[]{new RangeDisk(0,0,10),new RangeDisk(10,0,10),new RangeDisk(100,0,5)}).Any(a=>a.Circle.X==100&&a.End-a.Start==RangeOutline.Tau),"distant group remains independent beside overlapping group");
  Console.WriteLine("ALL RANGE OUTLINE TESTS PASSED");
 }
}
