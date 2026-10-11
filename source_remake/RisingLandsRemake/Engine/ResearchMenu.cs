using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
namespace RisingLands;
public sealed partial class ResearchGame {
    Texture2D nativeResearch=null!;int researchBranch=2;
    bool researchMenu,extraResearchChoosing;int pendingResearchBranch=-1;int laboratoryIndex;int displayedResearchCount;
    Rectangle ResearchRect(int x,int y,int w,int h)=>new(NativeMenuBounds.X+(int)(x*MenuScale),NativeMenuBounds.Y+(int)(y*MenuScale),(int)(w*MenuScale),(int)(h*MenuScale));
    void ResearchText(string value,int x,int y,Color color,float size=.72f)=>Text(value,ResearchRect(x,y,1,1).X,ResearchRect(x,y,1,1).Y,color,MenuScale*size);
    static readonly int[] ResearchColumns={0,2,1,3};
    static readonly string[] ResearchBranches={"Agricultura","Armamento","Engenharia","Espiritualidade"};
    Rectangle ResearchOpenButton=>new(ViewWidth+20,GraphicsDevice.Viewport.Height-207,Side-40,26);
    Rectangle ResearchPanel=>new(24,Top+18,Math.Max(420,ViewWidth-48),Math.Max(420,ViewHeight-36));
    Rectangle ResearchButton(int index)=>index switch{2=>ResearchRect(28,372,320,25),5=>ResearchRect(62,48,75,44),_=>Rectangle.Empty};
    Rectangle ExtraResearchButton=>ResearchRect(28,454,320,64);
    Rectangle ExtraResearchConfirm=>ResearchRect(28,471,320,25);
    Rectangle ExtraResearchCancel=>ResearchRect(28,504,320,25);
    Building[] PlayerLaboratories=>simulation.World.Buildings.Where(b=>b.Type==7&&b.Owner==0).ToArray();
    Building? ViewedLaboratory=>PlayerLaboratories.Length==0?null:PlayerLaboratories[Math.Clamp(laboratoryIndex,0,PlayerLaboratories.Length-1)];
    void OpenResearch(){researchMenu=true;extraResearchChoosing=false;pendingResearchBranch=-1;constructionMenu=false;buildMode=false;foodTargetMode=0;dragStart=null;laboratoryIndex=0;researchBranch=simulation.ActiveResearchBranch;message="Ramo da missao: "+ResearchBranches[researchBranch]+".";}
    bool UpdateResearchOrNativeActions(KeyboardState k,MouseState m){
        // The research screen covers the action bar, including on wide windows.
        if(researchMenu){UpdateResearchMenu(k,m);return true;}
        return UpdateNativeActions(k,m);
    }
    void UpdateResearchMenu(KeyboardState k,MouseState m){
        if(Press(k,Keys.Escape)||Press(k,Keys.T)||m.RightButton==ButtonState.Pressed&&oldMouse.RightButton==ButtonState.Released){if(extraResearchChoosing){extraResearchChoosing=false;pendingResearchBranch=-1;}else researchMenu=false;return;}
        if(m.LeftButton!=ButtonState.Pressed||oldMouse.LeftButton!=ButtonState.Released)return;
        if(ResearchButton(5).Contains(m.Position)){researchMenu=false;extraResearchChoosing=false;return;}
        if(extraResearchChoosing){
            if(ExtraResearchCancel.Contains(m.Position)){extraResearchChoosing=false;pendingResearchBranch=-1;message="Compra cancelada. Nenhum alimento gasto.";return;}
            for(int col=0;col<4;col++)if(ResearchRect(405+103*col,125,78,420).Contains(m.Position)){
                int branch=ResearchColumns[col];if(simulation.ExtraResearchError(branch) is {} error){message=error;return;}
                pendingResearchBranch=branch;message="Confirme para pagar 50 alimentos.";return;
            }
            if(ExtraResearchConfirm.Contains(m.Position)&&pendingResearchBranch>=0){
                if(simulation.PurchaseExtraResearch(pendingResearchBranch)){extraResearchChoosing=false;researchBranch=simulation.ActiveResearchBranch;message="Pesquisa extra liberada. Pagamento: 50 alimentos.";}
                else message=simulation.ExtraResearchError(pendingResearchBranch)??"Pesquisa indisponivel";
            }
            return;
        }
        if(ExtraResearchButton.Contains(m.Position)){if(ExtraOfferReady)BeginExtraResearchChoice();else message=simulation.Research.ExtraPurchased?"Pesquisa extra ja adquirida nesta missao":simulation.MissionResearchCount<2?"Conclua as duas pesquisas obrigatorias":"Alimento insuficiente: precisa de 50";return;}
        if(ResearchButton(2).Contains(m.Position)||ResearchRect(292,346,43,32).Contains(m.Position)){simulation.ToggleResearch(simulation.ActiveResearchBranch);message=simulation.ResearchStatus(simulation.ActiveResearchBranch);}
    }
    void VerifyResearchMenuClicks(){
        var previous=oldMouse;int branch=simulation.ActiveResearchBranch;
        void Click(Rectangle rect){oldMouse=default;UpdateResearchMenu(default,new MouseState(rect.Center.X,rect.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released));}
        string before=simulation.Serialize();for(int col=0;col<4;col++)Click(ResearchRect(405+103*col,125,78,420));
        if(researchBranch!=branch||simulation.Serialize()!=before)throw new InvalidDataException("Locked research columns changed mission branch");
        bool wasPaused=simulation.Research.ManualPaused;if(simulation.ResearchAvailable(branch)&&simulation.MissionResearchCount<simulation.ResearchLimit){Click(ResearchButton(2));if(simulation.Research.ManualPaused==wasPaused)throw new InvalidDataException("Research pause button");Click(ResearchButton(2));}
        Click(ResearchButton(5));if(researchMenu)throw new InvalidDataException("Research back button");OpenResearch();oldMouse=previous;
    }
    void ResearchNotification(){int count=simulation.MissionResearchCount;if(count>displayedResearchCount)message=simulation.ResearchNotice;displayedResearchCount=count;}
    void DrawResearchButton(){var r=ResearchOpenButton;batch.Draw(white,r,new Color(42,57,46));Outline(r,gold);Text("T  Pesquisa",r.X+8,r.Y+3,gold,.72f);}
    void DrawResearchMenu(){
        if(!researchMenu)return;
        researchBranch=simulation.ActiveResearchBranch;int shown=extraResearchChoosing&&pendingResearchBranch>=0?pendingResearchBranch:researchBranch;
        batch.Draw(white,GraphicsDevice.Viewport.Bounds,Color.Black);batch.Draw(nativeResearch,NativeMenuBounds,Color.White);
        DrawInventionAnimation(shown);var levels=simulation.EffectiveLevels;
        for(int col=0;col<4;col++){
            int branch=ResearchColumns[col];for(int row=0;row<Math.Clamp(levels[branch],0,20);row++)batch.Draw(white,ResearchRect(442+103*col,425-15*row,5,11),gold);
            ResearchText(levels[branch].ToString(),430+103*col,459,gold,.85f);
            if(branch==shown)Outline(ResearchRect(409+103*col,495,72,48),gold);
            if(extraResearchChoosing&&simulation.ExtraResearchError(branch)!=null)batch.Draw(white,ResearchRect(405+103*col,125,78,420),new Color(0,0,0,120));
        }
        ResearchText(ResearchBranches[shown],45,145,gold,.94f);ResearchText("Nivel "+levels[shown],45,193,Color.White);
        bool finished=!extraResearchChoosing&&simulation.MissionResearchCount>=simulation.ResearchLimit;
        var t=finished?Simulation.UnlockData.Technologies.FirstOrDefault(t=>t.Key==simulation.ResearchState(shown).Completed.LastOrDefault()):simulation.ResearchInvention(shown);string name=t==null?"Ramo no nivel maximo":(finished?"Concluida: ":"")+TechnologyBenefit(t);
        int midpoint=name.LastIndexOf(' ',Math.Min(31,Math.Max(0,name.Length-1)));
        if(name.Length>32&&midpoint>0){ResearchText(name[..midpoint],42,267,Color.White,.6f);ResearchText(name[(midpoint+1)..],42,290,Color.White,.6f);}else ResearchText(name,42,267,Color.White,.6f);
        var lab=ViewedLaboratory;ResearchText(lab==null?"Sem laboratorio":"Colonos: "+simulation.Scientists(lab.Slot)+" / "+simulation.LaboratoryCapacity,32,230,gold,.65f);
        int percent=simulation.ResearchPercent;ResearchText(extraResearchChoosing?"Extra":percent+"%",683,61,gold,extraResearchChoosing?.7f:.9f);
        if(extraResearchChoosing){
            ResearchText("Terceira descoberta opcional",28,389,gold,.6f);
            ResearchText(pendingResearchBranch<0?"Escolha um ramo nas colunas":"Custo: 50 alimentos do estoque",28,416,Color.White,.57f);
            ResearchText("Estoque: "+simulation.World.Resources[0]+" alimentos",28,443,gold,.58f);
            Outline(ExtraResearchConfirm,pendingResearchBranch>=0?gold:muted);Outline(ExtraResearchCancel,gold);
            ResearchText("Confirmar compra: 50 alimentos",28,475,pendingResearchBranch>=0&&simulation.ExtraResearchError(pendingResearchBranch)==null?gold:muted,.57f);
            ResearchText("Cancelar",28,508,Color.White,.65f);
        }else{
            ResearchText(simulation.ResearchStage,28,405,Color.White,.62f);
            // Fill only the channel between the native silver rails; retain both original end caps.
            var bar=ResearchRect(36,557,289,7);batch.Draw(white,bar,new Color(35,29,22));
            int fill=bar.Width*percent/100;
            if(fill>0){batch.Draw(white,new Rectangle(bar.X,bar.Y,fill,bar.Height),new Color(143,121,77));batch.Draw(white,new Rectangle(bar.X,bar.Y+Math.Max(1,bar.Height/3),fill,Math.Max(1,bar.Height/3)),gold);}
            string remaining=simulation.ResearchSecondsRemaining is {} seconds?"Restante: "+TimeSpan.FromSeconds(Math.Ceiling(seconds/(double)gameSpeed)).ToString(@"mm\:ss")+" em "+gameSpeed+"x":"";
            ResearchText(remaining,28,432,Color.White,.58f);
            if(simulation.MissionResearchCount<simulation.ResearchLimit&&simulation.ResearchAvailable(researchBranch))ResearchText(simulation.Research.ManualPaused?"Retomar pesquisa":"Pausar pesquisa",28,376,Color.White,.65f);
            DrawExtraResearchOffer();
        }
        string notice=message;while(notice.Length>0&&font.MeasureString(notice).X*.48f>322)notice=notice[..^1];if(notice!=message&&notice.Length>3)notice=notice[..^3]+"...";
        ResearchText(notice,24,534,muted,.48f);
        ResearchText(extraResearchChoosing?"Escolha e confirme a pesquisa extra":simulation.ResearchStatus(researchBranch),24,572,gold,.52f);
    }
}
