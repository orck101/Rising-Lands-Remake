using Microsoft.Xna.Framework.Audio;
namespace RisingLands;
public sealed partial class ResearchGame {
    void PlayConstructionStrike(Construction site){
        if(capture!=null)return;
        var point=ToScreen((site.WorkCell&255)*32,(site.WorkCell>>8)*32);
        if(point.X<0||point.X>=ViewWidth||point.Y<Top||point.Y>=Top+ViewHeight)return;
        var builder=simulation.World.Units.FirstOrDefault(a=>a.Slot==site.BuilderSlot);if(builder!=null)UnitSound(builder,0x25);
    }
}
