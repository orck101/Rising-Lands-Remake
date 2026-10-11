using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace RisingLands;
public sealed partial class ResearchGame {
 // Player color indices, original PE table 0x496cfc, initialized by 0x45fdca.
 static readonly int[][] PlayerPaletteIndices={Array.Empty<int>(),new[]{144,145,146,148,149,150,253,254},new[]{136,137,138,139,140,141,142,143},new[]{48,50,52,54,56,58,59,61}};
 readonly Dictionary<(int,int),Texture2D> playerTextures=new();
 Texture2D PlayerTexture(int id,Texture2D texture,int owner){if(owner is <1 or >3)return texture;if(playerTextures.TryGetValue((id,owner),out var cached))return cached;
  var colors=new Color[texture.Width*texture.Height];texture.GetData(colors);for(int p=0;p<colors.Length;p++){if(colors[p].A==0)continue;for(int i=0;i<8;i++)if(colors[p]==palette[160+i]){colors[p]=palette[PlayerPaletteIndices[owner][i]];break;}}
  var result=new Texture2D(GraphicsDevice,texture.Width,texture.Height);result.SetData(colors);playerTextures[(id,owner)]=result;return result;
 }
 void PrepareOpponentCapture(){for(int i=0;i<1500;i++)simulation.Step();paused=true;pauseMenu.Show(MenuPage.Closed);revealMap=true;var home=simulation.World.Buildings.First(b=>b.Owner==1&&b.Type==2);camera=new Vector2((home.Cell&255)*32-ViewWidth/2f+200,(home.Cell>>8)*32-ViewHeight/2f+120);ClampCamera();message="Clã Vermelho: economia própria, treinamento e defesa ativos.";}
}
