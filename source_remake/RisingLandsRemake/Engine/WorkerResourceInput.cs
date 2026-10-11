using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public sealed partial class ResearchGame {
    readonly WorkerResourceOrders resourceOrders=new();
    readonly ResourceGesture resourceGesture=new();
    bool resourceCtrlPress;
    bool HandleResourceClick(Point pointer,bool ctrl){
        var workers=SelectedUnits().ToArray();
        if(workers.Length==0||!workers.Any(u=>OriginalWorkerPolicy.Carrier(u.Type)))return false;
        var pos=new Vector2(pointer.X,pointer.Y-Top)/zoom+camera;int cell=BuildingHit(pointer)?.Cell??((int)(pos.Y/32)*256+(int)(pos.X/32));
        if(ctrl){
            if(!KnownCell(cell)){message="Explore o local antes de coletar ou entregar.";return true;}
            bool completing=resourceGesture.Pending;
            bool redirect=!completing&&resourceGesture.RedirectAvailable(simulation,workers,cell);
            bool ok=completing?resourceGesture.Commit(simulation,cell):redirect?resourceGesture.Redirect(simulation,workers,cell):resourceGesture.Begin(simulation,workers,cell);
            message=ok?(completing||redirect?"Destino confirmado. Carga preservada.":"Fonte marcada. Mantenha Ctrl e clique no destino."):"Local invalido para este trabalhador.";
            return true;
        }
        return false;
    }
    void DrawResourceCursor(){
        IsMouseVisible=true;
        if(simulation.MissionWon||selected==null||selected.Owner!=0||constructionMenu||researchMenu||buildMode||pauseMenu.Open||viewedBuilding>=0)return;
        var mouse=Mouse.GetState();var pointer=interactionCapturePointer??mouse.Position;if(captureMenu=="work-hover"){var site=simulation.Constructions.First(c=>!simulation.Complete(c));var screen=ToScreen((site.Cell&255)*32+16,(site.Cell>>8)*32+16);pointer=new Point((int)screen.X,(int)screen.Y);}
        bool preview=capture!=null&&captureMenu is "farmer-collect" or "farmer-prohibited" or "resource-order";
        if(preview){int hover=5642;if(captureMenu is "farmer-prohibited" or "resource-order")hover=Enumerable.Range(0,65536).Where(i=>simulation.World.Objects[i]==0&&simulation.Passable(i&255,i>>8)&&simulation.BuildingAt(i)==null).OrderBy(i=>Math.Abs((i&255)-10)+Math.Abs((i>>8)-22)).First();var p=ToScreen((hover&255)*32+12,(hover>>8)*32+12);pointer=new Point((int)p.X,(int)p.Y);}
        if(!View.Contains(pointer))return;var keys=Keyboard.GetState();
        bool ctrl=preview||keys.IsKeyDown(Keys.LeftControl)||keys.IsKeyDown(Keys.RightControl);if(!ctrl&&BalloonHit(pointer) is {} balloon&&SelectedUnits().Any(a=>a.Type!=17)){IsMouseVisible=false;bool allowed=SelectedUnits().Any(a=>simulation.CanBoardBalloon(a,balloon));DrawNativeInteractionCursor(pointer,allowed?0x1e16:0x1e0e,allowed?3:1);return;}if(!ctrl&&selected.Type==10&&BuildingHit(pointer) is {Owner:>0} destination&&destination.Type is 2 or 16){IsMouseVisible=false;bool allowed=simulation.MessengerEntryError(selected,destination.Slot)==null;DrawNativeInteractionCursor(pointer,allowed?0x1e16:0x1e0e,allowed?3:1);return;}if(!ctrl&&BuildingHit(pointer) is {Owner:0,Type:8} bag){IsMouseVisible=false;bool allowed=SelectedUnits().Any(a=>simulation.CanStoreTradeUnit(a,bag));DrawNativeInteractionCursor(pointer,allowed?0x1e16:0x1e0e,allowed?3:1);return;}if(selected.Type==9||!ctrl&&BuildingHit(pointer)?.Type==2){var building=BuildingHit(pointer);if(building!=null){IsMouseVisible=false;bool canEnter=SelectedUnits().Any(u=>simulation.CanEnterBuilding(u,building.Slot));DrawNativeInteractionCursor(pointer,canEnter?0x1e16:0x1e0e,canEnter?3:1);}return;}
        if(!ctrl){if(selected.Type==7){var site=ConstructionHit(pointer);if(site!=null||BuildingHit(pointer) is {} repair&&simulation.CanRepair(selected,repair)){IsMouseVisible=false;DrawNativeInteractionCursor(pointer,0x1e10,6);}}return;}
        var pos=new Vector2(pointer.X,pointer.Y-Top)/zoom+camera;int cell=BuildingHit(pointer)?.Cell??((int)(pos.Y/32)*256+(int)(pos.X/32));
        bool delivering=resourceGesture.Pending||resourceGesture.RedirectAvailable(simulation,SelectedUnits().ToArray(),cell);
        bool valid=KnownCell(cell)&&(delivering?(resourceGesture.Pending?SelectedUnits().All(u=>resourceGesture.DestinationAvailable(simulation,u,cell)):resourceGesture.RedirectAvailable(simulation,SelectedUnits().ToArray(),cell)):WorkerResourceOrders.GroupSourceAvailable(simulation,SelectedUnits().ToArray(),cell));
        IsMouseVisible=false;DrawCollectionIcon(pointer,valid,delivering);

    }
    // Original InputGame: pickup 0x1e3d..3f, delivery 0x1e40..42,
    // entry 0x1e16..18, prohibited 0x1e0e. Art decoded from OTHERS.CMP.
    void DrawNativeInteractionCursor(Point pointer,int first,int frames){
        int id=first+(int)(draws%frames);if(!sprites.TryGetValue(id,out var texture))throw new InvalidDataException("Missing native cursor sprite "+id);
        batch.Draw(texture,new Vector2(pointer.X,pointer.Y),Color.White);
    }
    void DrawCollectionIcon(Point pointer,bool allowed,bool delivering=false)=>DrawNativeInteractionCursor(pointer,!allowed?0x1e0e:delivering?0x1e40:0x1e3d,allowed?3:1);
}
