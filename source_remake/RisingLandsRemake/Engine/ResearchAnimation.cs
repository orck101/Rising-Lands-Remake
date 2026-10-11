using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace RisingLands;
public sealed partial class ResearchGame {
 readonly Dictionary<int,Texture2D> inventionFrames=new();double inventionClock;
 void DrawInventionAnimation(int branch){if(branch is <0 or >3)return;int phase=(int)(inventionClock*12)%16;int index=(branch switch{0=>0,2=>16,1=>32,_=>48})+phase;
  if(!inventionFrames.TryGetValue(index,out var texture)){using var stream=File.OpenRead(PackageLayout.FromBase("Content/ResearchAnimation",index.ToString("D5")+".png"));inventionFrames[index]=texture=Texture2D.FromStream(GraphicsDevice,stream);}
  var pos=branch switch{0=>new Point(397,484),2=>new Point(507,473),1=>new Point(610,475),_=>new Point(716,455)};batch.Draw(texture,ResearchRect(pos.X,pos.Y,texture.Width,texture.Height),Color.White);
 }
 string TechnologyBenefit(Invention t)=>t.Effect==0?t.Param1 switch{1=>"Liberar "+RecruitName(t.Param2),3=>"Liberar "+simulation.BuildName(t.Param2),4=>"Liberar poder espiritual",_=>"Nova tecnologia"}:t.Param1 switch{1=>"Capacidade de carga +"+t.Param3,2=>"Menor consumo de alimento",3=>(t.Param2==5?"Colheita ":t.Param2==7?"Mineracao ":"Coleta ")+(t.Param3/10.0).ToString("0.#",System.Globalization.CultureInfo.GetCultureInfo("pt-BR"))+"x mais rapida",4=>"Alcance de tiro +"+t.Param3,5=>"Dano de combate +"+t.Param3,6=>"Melhoria de resistencia",8=>"Construcao "+(t.Param3/10.0).ToString("0.#",System.Globalization.CultureInfo.GetCultureInfo("pt-BR"))+"x mais rapida",10=>"Druida: dano +"+t.Param3,9=>"Druida recupera energia mais rapido",12=>"Druida: energia maxima +"+t.Param3,_=>"Melhoria "+t.Key};
 void DrawCampaignBenefits(int branch){if(branch<0){ResearchText("Selecione um dos quatro ramos",42,267,gold,.6f);return;}
  ResearchText(ResearchBranches[branch],28,191,Color.White,.7f);var upcoming=Simulation.UnlockData.Technologies.Where(t=>t.Branch==branch&&t.Index>=simulation.EffectiveLevels[branch]).OrderBy(t=>t.Index).ToArray();var available=simulation.NextMissionTechnologies(branch);
  for(int i=0;i<2;i++){string value=i<available.Length?TechnologyBenefit(available[i]):i==0?"Sem evolucao nesta missao":upcoming.Length>0?"Disponivel na missao "+upcoming[0].Mission:"Ramo concluido";if(available.Length==1&&i==1)value="Sem segunda evolucao nesta missao";ResearchText(value,42,267+i*25,Color.White,value.Length>30?.51f:.6f);}
 }
}
