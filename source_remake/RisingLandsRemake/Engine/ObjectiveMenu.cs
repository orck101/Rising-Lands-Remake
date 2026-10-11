using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
namespace RisingLands;
public sealed partial class ResearchGame {
    Texture2D nativeObjectives=null!;
    readonly ObjectiveReader objectiveReader=new();
    void OpenObjectives(){objectiveReader.Open(simulation.World.MapName,training);menuStatus="";pauseMenu.Show(MenuPage.Objectives);}
    Rectangle ObjectiveButton(int index)=>index switch{0=>ResearchRect(269,153,28,35),1=>ResearchRect(298,153,28,35),_=>ResearchRect(290,511,47,38)};
    void UpdateObjectives(KeyboardState k,MouseState m){
        if(Press(k,Keys.Left)||Press(k,Keys.PageUp))objectiveReader.Move(-1);
        if(Press(k,Keys.Right)||Press(k,Keys.PageDown))objectiveReader.Move(1);
        if(Press(k,Keys.Enter)){pauseMenu.Back();return;}
        if(m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released){
            if(ResearchRect(60,48,77,46).Contains(m.Position)){pauseMenu.Back();return;}
            for(int i=0;i<3;i++)if(ObjectiveButton(i).Contains(m.Position)){if(i==2)pauseMenu.Back();else objectiveReader.Move(i==0?-1:1);break;}
        }
    }
    void VerifyObjectiveClicks(){
        string before=simulation.Serialize();int page=objectiveReader.Page;var previous=oldMouse;
        void Click(Rectangle rect){oldMouse=default;UpdateObjectives(default,new MouseState(rect.Center.X,rect.Center.Y,0,ButtonState.Pressed,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released));}
        objectiveReader.Move(int.MinValue);Click(ObjectiveButton(0));if(objectiveReader.Page!=0)throw new InvalidDataException("Objective first-page bound");
        Click(ObjectiveButton(1));if(objectiveReader.Page!=Math.Min(1,Math.Max(0,objectiveReader.Count-1)))throw new InvalidDataException("Objective next-page click");
        Click(ObjectiveButton(0));if(objectiveReader.Page!=0)throw new InvalidDataException("Objective previous-page click");
        objectiveReader.Move(int.MaxValue);Click(ObjectiveButton(1));if(objectiveReader.Page!=Math.Max(0,objectiveReader.Count-1))throw new InvalidDataException("Objective last-page bound");
        Click(ObjectiveButton(2));if(pauseMenu.Page!=MenuPage.Main)throw new InvalidDataException("Objective confirm click");
        OpenObjectives();Click(ResearchRect(60,48,77,46));if(pauseMenu.Page!=MenuPage.Main)throw new InvalidDataException("Objective back click");
        OpenObjectives();objectiveReader.Move(page);oldMouse=previous;if(simulation.Serialize()!=before)throw new InvalidDataException("Objective navigation changed game");
    }
    void DrawObjectives(){
        batch.Draw(white,GraphicsDevice.Viewport.Bounds,Color.Black);batch.Draw(nativeObjectives,NativeMenuBounds,Color.White);
        var textArea=ResearchRect(55,212,255,274);batch.Draw(white,textArea,new Color(48,48,48));
        IReadOnlyList<string> lines=simulation.IsFifthMission?new[]{"- Missao 5 -","Encontre os outros tres clas.","Elimine aqueles que nao forem aliados.","",$"Clas encontrados: {simulation.Exploration.ClansFound.Count(v=>v)} / 3",simulation.FifthClanStatus(1),simulation.FifthClanStatus(2),simulation.FifthClanStatus(3),"","Eliminado: nenhuma unidade viva","nem predio com colono vivo.","Unidades na mina ou na Bolsa contam.","Predios vazios e obras nao contam."}:objectiveReader.Lines;
        for(int i=0;i<Math.Min(18,lines.Count);i++){
            string line=lines[i];float width=font.MeasureString(line).X;
            float scale=MenuScale*(width>0?Math.Min(.52f,253/width):.52f);
            Text(line,textArea.Center.X-(int)(width*scale/2),ResearchRect(55,212+i*15,1,1).Y,Color.White,scale);
        }
        if(objectiveReader.Count==0)ResearchText(objectiveReader.Notice,55,235,muted,.45f);
        ResearchText(objectiveReader.Count>0?$"{objectiveReader.Page+1} / {objectiveReader.Count}":"0 / 0",682,62,gold,.85f);
        var levels=simulation.EffectiveLevels;levels[2]=simulation.Research.Index;levels[0]=simulation.AgricultureResearch.Index;
        for(int col=0;col<4;col++)for(int row=0;row<Math.Clamp(levels[ResearchColumns[col]],0,20);row++)batch.Draw(white,ResearchRect(442+103*col,425-15*row,5,11),gold);

        for(int i=0;i<3;i++)if(ObjectiveButton(i).Contains(Mouse.GetState().Position))Outline(ObjectiveButton(i),gold);
    }
}
