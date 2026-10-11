using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public record ResearchMovie(string Name,int Frames,int Width,int Height,int DurationMs);
public record ResearchMovies(Dictionary<string,string> Events,Dictionary<string,ResearchMovie> Movies);
public static class ResearchCompletionCatalog {
 public static readonly ResearchMovies Data=JsonSerializer.Deserialize<ResearchMovies>(File.ReadAllText(PackageLayout.FromBase("Content/Data/research_completion.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true,PropertyNamingPolicy=JsonNamingPolicy.SnakeCaseLower})!;
 public static bool Specific(Invention invention)=>Data.Events.ContainsKey((invention.Branch*100+invention.Index).ToString());
 public static ResearchMovie? For(Invention invention){
  string name=Data.Events.GetValueOrDefault((invention.Branch*100+invention.Index).ToString())??(invention.Branch switch{0=>"LABO1",1=>"LABO3",2=>"LABO2",_=>"INVMYS01"});
  return Data.Movies.GetValueOrDefault(name);
 }
}
public sealed partial class Simulation {
 public event Action<Invention>? ResearchCompleted;
}
public sealed partial class ResearchGame {
 readonly Queue<Invention> researchMovies=new();ResearchMovie? playingResearchMovie;Invention? playingInvention;
 double movieSeconds;int movieFrame=-1;Texture2D? movieTexture;
 bool ResearchMovieOpen=>playingResearchMovie!=null;
 void QueueResearchMovie(Invention invention){PlayAlert(0x40000);if(ResearchCompletionCatalog.For(invention)!=null)researchMovies.Enqueue(invention);}
 void StartResearchMovie(){if(ResearchMovieOpen||researchMovies.Count==0)return;playingInvention=researchMovies.Dequeue();playingResearchMovie=ResearchCompletionCatalog.For(playingInvention);movieSeconds=0;movieFrame=-1;CancelPointerNavigation();resourceGesture.Cancel();dragStart=null;}
 void CloseResearchMovie(){playingResearchMovie=null;playingInvention=null;movieTexture?.Dispose();movieTexture=null;movieFrame=-1;}
 void UpdateResearchMovie(GameTime time,KeyboardState keys,MouseState mouse){
  movieSeconds+=Math.Clamp(time.ElapsedGameTime.TotalSeconds,0,.1);
  if(Press(keys,Keys.Escape)||Press(keys,Keys.Enter)||mouse.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released||movieSeconds>=playingResearchMovie!.Frames*playingResearchMovie.DurationMs/1000.0)CloseResearchMovie();
 }
 void DrawResearchMovie(){
  if(playingResearchMovie is not {} movie)return;int frame=Math.Min(movie.Frames-1,(int)(movieSeconds*1000/movie.DurationMs));
  if(frame!=movieFrame){movieTexture?.Dispose();using var stream=File.OpenRead(PackageLayout.FromBase("Content/ResearchCompletion",movie.Name,frame.ToString("0000")+".png"));movieTexture=Texture2D.FromStream(GraphicsDevice,stream);movieFrame=frame;}
  batch.Draw(white,GraphicsDevice.Viewport.Bounds,new Color(0,0,0,210));
  int width=Math.Min(GraphicsDevice.Viewport.Width-60,640),height=width*movie.Height/movie.Width;int x=(GraphicsDevice.Viewport.Width-width)/2,y=Math.Max(40,(GraphicsDevice.Viewport.Height-height)/2);
  batch.Draw(movieTexture!,new Rectangle(x,y,width,height),Color.White);Outline(new Rectangle(x-2,y-2,width+4,height+4),gold,2);
  Text("Pesquisa concluida",x,y-34,gold,.85f);Text(playingInvention==null?"":TechnologyBenefit(playingInvention),x,y+height+12,Color.Wheat,.6f);
  Text("Clique, Enter ou Esc para continuar",x,y+height+38,muted,.5f);
 }
 void PrepareResearchMovieCapture(){
  var campaign=new CampaignProgress{StartingLevels=Simulation.UnlockData.Missions.First(m=>m.Mission==3).Levels.ToArray(),SelectedBranch=0};LoadMap(campaign);ResearchV58Tests.Staff(simulation);simulation.AgricultureResearch.Progress=0;ResearchV58Tests.Advance(simulation,7200);
  if(researchMovies.Count!=1)throw new InvalidDataException("One discovery must enqueue exactly one original movie");
  StartResearchMovie();string before=simulation.Serialize();AdvanceSimulation();if(simulation.Serialize()!=before)throw new InvalidDataException("Completion movie must suspend game simulation");
  UpdateResearchMovie(new GameTime(),new KeyboardState(Keys.Enter),default);if(ResearchMovieOpen||researchMovies.Count!=0||simulation.Serialize()!=before)throw new InvalidDataException("Enter must dismiss the movie without replay or gameplay changes");
  var t=Simulation.UnlockData.Technologies.First(t=>t.Key==simulation.AgricultureResearch.Completed.Last());QueueResearchMovie(t);StartResearchMovie();movieSeconds=2;paused=true;
 }
 void PrepareSecondResearchMovieCapture(){
  // Use the research timing fixture, whose army is provisioned for this isolated test.
  var prepared=ResearchV58Tests.Load(root);ResearchV58Tests.Staff(prepared);prepared.AgricultureResearch.Progress=0;LoadMap(prepared.Campaign,prepared);
  ResearchV58Tests.Advance(simulation,7200);
  if(simulation.MissionResearchCount!=1||researchMovies.Count!=1)throw new InvalidDataException("First discovery must enqueue one movie");
  StartResearchMovie();CloseResearchMovie();ResearchV58Tests.Advance(simulation,7200);
  if(simulation.MissionResearchCount!=2||researchMovies.Count!=1)throw new InvalidDataException("Second discovery must enqueue exactly one original movie");
  StartResearchMovie();movieSeconds=2;paused=true;
 }
}
