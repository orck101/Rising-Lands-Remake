namespace RisingLands;
public enum FormationShape { Square,Line,Triangle }
public static class TroopFormations {
 public static (int X,int Y)[] Positions(int count,FormationShape shape){
  if(count<1||count>1000||!Enum.IsDefined(shape))return Array.Empty<(int,int)>();var points=new List<(int X,int Y)>();
  if(shape==FormationShape.Line){for(int i=0;i<count;i++)points.Add((i*2,0));}
  else if(shape==FormationShape.Square){int width=(int)Math.Ceiling(Math.Sqrt(count));for(int i=0;i<count;i++)points.Add(((i%width)*2,(i/width)*2));}
  else if(count==2){points.Add((-1,0));points.Add((1,0));}
  else{
   int rows=(int)Math.Floor((Math.Sqrt(8*count+1)-1)/2);
   for(int row=0;row<rows;row++)for(int col=0;col<=row;col++)points.Add((col*2-row,row*2));
   double coreY=points.Average(p=>p.Y);var interior=new List<(int X,int Y)>();
   for(int y=1;y<=2*(rows-1);y++)for(int x=-y/2;x<=y/2;x++)if(!points.Contains((x,y)))interior.Add((x,y));
   points.AddRange(interior.OrderBy(p=>p.X*p.X+(p.Y-coreY)*(p.Y-coreY)).ThenBy(p=>p.Y).ThenBy(p=>p.X).Take(count-points.Count));
  }
  double cx=points.Average(p=>p.X),cy=points.Average(p=>p.Y);
  return points.Select(p=>(X:p.X-(int)Math.Round(cx),Y:p.Y-(int)Math.Round(cy))).OrderBy(p=>(p.X-(cx-Math.Round(cx)))*(p.X-(cx-Math.Round(cx)))+(p.Y-(cy-Math.Round(cy)))*(p.Y-(cy-Math.Round(cy)))).ThenBy(p=>p.Y).ThenBy(p=>p.X).ToArray();
 }
}
public sealed partial class Simulation {
 public bool CanFormTroops(IEnumerable<Actor> actors){var units=actors.ToArray();return units.Length>0&&units.All(u=>u.Owner==0&&World.Units.Contains(u)&&AttackRangeSelection(new[]{u}).Any());}
 public bool FormTroops(IEnumerable<Actor> actors,FormationShape shape,int? anchor=null){
  var units=actors.DistinctBy(u=>u.Slot).ToArray();if(!CanFormTroops(units)||!Enum.IsDefined(shape))return false;
  int x=anchor.HasValue?anchor.Value&255:(int)Math.Round(units.Average(u=>u.X/1024.0));int y=anchor.HasValue?anchor.Value>>8:(int)Math.Round(units.Average(u=>u.Y/1024.0));if(x<0||y<0||x>=World.Width||y>=World.Height)return false;
  var positions=TroopFormations.Positions(units.Length,shape);var sorted=units.OrderByDescending(u=>CombatRule(u.Type).Range).ThenBy(u=>u.Slot).ToArray();var used=World.Units.Where(u=>!units.Contains(u)&&VisualCatalog.Visible(u)).Select(u=>u.Y/1024*256+u.X/1024).ToHashSet();var paths=new Dictionary<Actor,List<int>>();var goals=new Dictionary<Actor,int>();
  for(int i=0;i<sorted.Length;i++){
   var u=sorted[i];bool found=false;int px=x+positions[i].X,py=y+positions[i].Y;
   for(int radius=0;radius<=3&&!found;radius++)for(int dy=-radius;dy<=radius&&!found;dy++)for(int dx=-radius;dx<=radius&&!found;dx++){
    if(Math.Max(Math.Abs(dx),Math.Abs(dy))!=radius)continue;int nx=px+dx,ny=py+dy,cell=ny*256+nx;if(nx<0||ny<0||nx>=World.Width||ny>=World.Height||used.Contains(cell)||!CanTraverse(u,nx,ny))continue;
    var old=u.Route;if(!Order(u,nx,ny,true))continue;paths[u]=u.Route;goals[u]=cell;u.Route=old;used.Add(cell);found=true;
   }
   if(!found)return false;
  }
  double centerX=goals.Values.Average(c=>(double)(c&255)),centerY=goals.Values.Average(c=>(double)(c>>8));var destinations=goals.Values.OrderBy(c=>Math.Pow((c&255)-centerX,2)+Math.Pow((c>>8)-centerY,2)).ThenBy(c=>c).ToArray();
  var assigned=AssignFormationCells(sorted,destinations);
  for(int i=0;i<sorted.Length;i++){var u=sorted[i];var old=u.Route;int cell=assigned[u.Slot];if(!Order(u,cell&255,cell>>8,true))return false;paths[u]=u.Route;u.Route=old;}
  foreach(var u in sorted){Stop(u);u.Route=paths[u];}return true;
 }
 public Dictionary<int,int> AssignFormationCells(Actor[] units,int[] cells){
  var result=new Dictionary<int,int>();var remaining=cells.ToList();var captains=units.Where(u=>u.Type==2).OrderBy(u=>u.Slot).ToArray();
  // Reserve separate protected positions for captains before filling center with ranged troops.
  if(captains.Length>1){double cx=cells.Average(c=>c&255),cy=cells.Average(c=>c>>8);var covered=new HashSet<int>();var leaders=new List<int>();
   foreach(var captain in captains){var candidates=remaining.Where(c=>cells.Length<8||Array.IndexOf(cells,c)>=Math.Max(1,cells.Length/8)&&Array.IndexOf(cells,c)<Math.Max(2,cells.Length*4/5)).ToArray();if(candidates.Length==0)candidates=remaining.ToArray();
    int chosen=candidates.OrderByDescending(c=>{
     int coverage=cells.Count(p=>ProjectileMath.WithinRange((p&255)-(c&255),(p>>8)-(c>>8),100));int fresh=cells.Count(p=>!covered.Contains(p)&&ProjectileMath.WithinRange((p&255)-(c&255),(p>>8)-(c>>8),100));
     double separation=leaders.Count==0?0:leaders.Min(p=>Math.Pow((p&255)-(c&255),2)+Math.Pow((p>>8)-(c>>8),2));double exposure=Math.Pow((c&255)-cx,2)+Math.Pow((c>>8)-cy,2);
     return fresh*10000+coverage*100+separation*2-exposure*.25;
    }).ThenBy(c=>c).First();result[captain.Slot]=chosen;remaining.Remove(chosen);leaders.Add(chosen);foreach(int c in cells)if(ProjectileMath.WithinRange((c&255)-(chosen&255),(c>>8)-(chosen>>8),100))covered.Add(c);
   }
  }
  foreach(var u in units.Where(u=>!result.ContainsKey(u.Slot)).OrderByDescending(u=>CombatRule(u.Type).Range).ThenBy(u=>u.Slot)){result[u.Slot]=remaining[0];remaining.RemoveAt(0);}return result;
 }
}
