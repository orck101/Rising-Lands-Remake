using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace RisingLands;
public sealed partial class ResearchGame {
 Texture2D GrayTexture(int id,Texture2D original){if(graySprites.TryGetValue(id,out var texture))return texture;var colors=new Color[original.Width*original.Height];original.GetData(colors);for(int i=0;i<colors.Length;i++){var c=colors[i];int l=(c.R*30+c.G*59+c.B*11)/100;colors[i]=new Color(l,l,l,(int)c.A);}texture=new Texture2D(GraphicsDevice,original.Width,original.Height);texture.SetData(colors);graySprites[id]=texture;return texture;}
}
