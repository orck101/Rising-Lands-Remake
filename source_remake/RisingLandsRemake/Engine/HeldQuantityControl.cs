namespace RisingLands;
public sealed class HeldQuantityControl {
 int heldDirection;double next;
 public void Reset(){heldDirection=0;next=0;}
 public int Step(bool pressed,int direction,double now,bool fresh){
  if(!pressed||direction==0){Reset();return 0;}
  if(fresh){heldDirection=direction;next=now+.35;return direction;}
  if(direction!=heldDirection){Reset();return 0;}
  if(heldDirection==0||now+1e-8<next)return 0;
  int count=Math.Min(4,1+(int)Math.Floor((now-next+1e-8)/.07));next+=count*.07;return count*direction;
 }
}
