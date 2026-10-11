using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace RisingLands;
public record MarketVisuals(List<DrawCommand> Commands);
public sealed partial class ResearchGame {
 static readonly int[] NativeMarketKinds={2,0,1};
 static readonly MarketVisuals MarketArt=JsonSerializer.Deserialize<MarketVisuals>(File.ReadAllText(PackageLayout.FromBase("Content/Data/market_visuals_v67.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 float MarketScale=>Math.Min(1.6f,Math.Min(ViewHeight/360f,ViewWidth/310f));
 Rectangle MarketRect(int x,int y,int width,int height)=>new(16+(int)(x*MarketScale),20+(int)(y*MarketScale),(int)Math.Ceiling(width*MarketScale),(int)Math.Ceiling(height*MarketScale));
 void MarketMini(int id,int x,int y){if(sprites.TryGetValue(id,out var t))batch.Draw(t,MarketRect(x,y,t.Width/2,t.Height/2),Color.White);}
 void MarketSprite(int id,int x,int y,Color? tint=null){if(sprites.TryGetValue(id,out var texture))batch.Draw(texture,MarketRect(x,y,texture.Width,texture.Height),tint??Color.White);}
 readonly HeldQuantityControl marketQuantityRepeat=new();
 int marketQuantityKind=-1,marketQuantitySide,marketQuantity,marketSession=-1;bool marketReplaceDigits;
 bool[] marketUnits=new bool[2];int[] marketUnitPage=new int[2];List<int> marketGiveUnits=new(),marketTakeUnits=new();
 Negotiation? MarketSession=>simulation.Special.Negotiations.FirstOrDefault(n=>n.Id==marketSession);
 void OpenNegotiation(Negotiation n){OpenMarket(n.Home);marketSession=n.Id;marketGive=(int[])n.Give.Clone();marketTake=(int[])n.Take.Clone();marketGiveUnits=new(n.GiveUnits);marketTakeUnits=new(n.TakeUnits);}
 Rectangle TradeShortcut(int index)=>new(12+index*68,NativeTop-92,60,54);
 void DrawTradeShortcuts(){int i=0;foreach(var n in simulation.Special.Negotiations.Where(n=>n.Active&&n.Stage!="Travelling")){var rect=TradeShortcut(i++);batch.Draw(white,rect,new Color(25,25,20,210));int icon=n.Alliance?0x2b13:0x2efd+((int)(inventionClock*12)&31);if(sprites.TryGetValue(icon,out var t)){float scale=Math.Min(rect.Width/(float)t.Width,rect.Height/(float)t.Height);var r=new Rectangle(rect.X+(rect.Width-(int)(t.Width*scale))/2,rect.Y+(rect.Height-(int)(t.Height*scale))/2,(int)(t.Width*scale),(int)(t.Height*scale));batch.Draw(t,r,Color.White);}if(rect.Contains(Mouse.GetState().Position))Text(n.Alliance?"Alianca: aceitar ou recusar":"Troca: clique para negociar",rect.X,rect.Y-24,Color.Wheat,.55f);}if(simulation.TradeNotice.Length>0){Text(simulation.TradeNotice,12,NativeTop-124,Color.Wheat,.55f);}}
 bool UpdateTradeShortcut(MouseState m){if(m.LeftButton!=ButtonState.Pressed||oldMouse.LeftButton!=ButtonState.Released)return false;var sessions=simulation.Special.Negotiations.Where(n=>n.Active&&n.Stage!="Travelling").ToArray();for(int i=0;i<sessions.Length;i++)if(TradeShortcut(i).Contains(m.Position)){OpenNegotiation(sessions[i]);return true;}return false;}
 int MarketPane(int side)=>(MarketSession?.Incoming??false)?1-side:side;
 Actor[] MarketActors(int side)=>simulation.TradeUnits(side==0?0:MarketSession?.Owner??1,marketSession).ToArray();
 void UpdateMarket(KeyboardState k,MouseState m){if(marketQuantityKind<0)marketQuantityRepeat.Reset();var n=MarketSession;if(n==null){marketMenu=false;return;}bool left=m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released;
  if(Press(k,Keys.Escape)){marketQuantityKind=-1;marketMenu=false;return;}
  if(left&&MarketRect(236,3,40,28).Contains(m.Position)){if(n.Active)simulation.CancelNegotiation(n,n.Alliance?"Alianca recusada.":"Troca recusada/cancelada.");marketMenu=false;return;}
  if(n.Alliance){if(left&&MarketRect(245,289,25,26).Contains(m.Position)){simulation.AnswerAlliance(n,true);marketMenu=false;}return;}
  if(!n.Active){if(left)marketMenu=false;return;}
  if(n.Stage=="Delivery"){if(left&&MarketRect(245,289,25,26).Contains(m.Position))marketMenu=false;return;}
  if(marketQuantityKind>=0){int max=simulation.World.Resources[Simulation.MarketResource(marketQuantityKind,marketQuantitySide==0?0:n.Owner)];for(int v=0;v<10;v++)if(Press(k,(Keys)((int)Keys.D0+v))||Press(k,(Keys)((int)Keys.NumPad0+v))){marketQuantity=Math.Clamp((marketReplaceDigits?0:marketQuantity)*10+v,0,max);marketReplaceDigits=false;}if(Press(k,Keys.Back))marketQuantity/=10;int direction=MarketRect(249,43,22,26).Contains(m.Position)?1:MarketRect(249,72,22,26).Contains(m.Position)?-1:0;int change=marketQuantityRepeat.Step(m.LeftButton==ButtonState.Pressed,direction,inventionClock,left);marketQuantity=Math.Clamp(marketQuantity+change*(k.IsKeyDown(Keys.LeftShift)?10:1),0,max);if(change!=0)marketReplaceDigits=false;if(Press(k,Keys.Enter)||left&&MarketRect(245,289,25,26).Contains(m.Position)){(marketQuantitySide==0?marketGive:marketTake)[marketQuantityKind]=marketQuantity;marketQuantityKind=-1;if(n.Stage=="Evaluating"){if(marketQuantitySide==0)n.Give=(int[])marketGive.Clone();else n.Take=(int[])marketTake.Clone();}}return;}
  if(!left)return;
  for(int side=0;side<2;side++){int top=MarketPane(side)==0?3:283;if(MarketRect(39,top,55,28).Contains(m.Position)){marketUnits[side]=true;return;}if(MarketRect(103,top,55,28).Contains(m.Position)){marketUnits[side]=false;return;}}
  for(int side=0;side<2;side++){
   if(n.Incoming&&side==0||n.Stage=="Evaluating"&&side==1)continue;
   var actors=MarketActors(side);var types=actors.Select(a=>a.Type).Distinct().OrderBy(x=>x).ToArray();
   if(n.Stage=="Evaluating"&&marketUnits[side])continue;
   if(marketUnits[side]&&MarketRect(245,MarketPane(side)==0?49:218,25,40).Contains(m.Position)){marketUnitPage[side]=(marketUnitPage[side]+1)%Math.Max(1,(types.Length+2)/3);return;}
   for(int slot=0;slot<3;slot++)if(MarketRect(39+slot*61,MarketPane(side)==0?49:218,56,48).Contains(m.Position)){if(marketUnits[side]){int index=marketUnitPage[side]*3+slot;if(index>=types.Length)return;var chosen=side==0?marketGiveUnits:marketTakeUnits;var a=actors.FirstOrDefault(a=>a.Type==types[index]&&!chosen.Contains(a.Slot));if(a!=null&&chosen.Count<8)chosen.Add(a.Slot);else chosen.RemoveAll(id=>simulation.World.Units.First(u=>u.Slot==id).Type==types[index]);}else{marketQuantitySide=side;marketQuantityKind=NativeMarketKinds[slot];marketQuantity=(side==0?marketGive:marketTake)[marketQuantityKind];marketReplaceDigits=true;}return;}
  }
  if(MarketRect(245,289,25,26).Contains(m.Position)){message=simulation.SubmitTrade(n,marketGive,marketTake,marketGiveUnits,marketTakeUnits)?"Oferta enviada para avaliacao.":"Confira estoques, unidades e pedido recebido.";}
 }
 void DrawMarket(){if(!marketMenu||MarketSession is not {} n)return;
  foreach(var c in MarketArt.Commands.Where(c=>c.Kind!="player"&&c.Sprite!=0x2b11))MarketSprite(c.Sprite,c.X,c.Y);MarketSprite(0x2b11,245,290);
  for(int side=0;side<2;side++){int row=MarketPane(side)==0?111:179;for(int i=0;i<6;i++)batch.Draw(white,MarketRect(40+i*30,row,28,24),palette[42]);var ids=side==0?marketGiveUnits:marketTakeUnits;if(n.Stage is "Delivery" or "Completed")ids=side==0?n.GiveUnits:n.TakeUnits;int mini=0;
   for(int slot=0;slot<3;slot++){var box=MarketRect(39+slot*61,MarketPane(side)==0?49:218,56,48);if(marketUnits[side]){batch.Draw(white,box,new Color(36,35,29));var actors=MarketActors(side);var types=actors.Select(a=>a.Type).Distinct().OrderBy(x=>x).ToArray();int index=marketUnitPage[side]*3+slot;if(index<types.Length){MarketSprite(UnitPortrait(types[index]),39+slot*61,MarketPane(side)==0?49:218);Text(actors.Count(a=>a.Type==types[index]).ToString(),box.X,box.Bottom,Color.Wheat,.4f*MarketScale);}}else{MarketSprite(0x29f0+slot,39+slot*61,MarketPane(side)==0?49:218);int kind=NativeMarketKinds[slot];Text(simulation.World.Resources[kind*4+(side==0?0:n.Owner)].ToString(),box.X,box.Bottom,Color.Wheat,.4f*MarketScale);int count=(side==0?marketGive:marketTake)[kind];if(count>0){MarketMini(0x29f0+slot,40+mini*30,row);var p=MarketRect(40+mini++*30,row+14,1,1);Text(count.ToString(),p.X,p.Y,gold,.33f*MarketScale);}}}
   var agreedTypes=n.Stage is "Evaluating" or "Delivery" or "Completed"?(side==0?n.GiveTypes:n.TakeTypes):ids.Select(id=>simulation.World.Units.First(u=>u.Slot==id).Type).ToList();foreach(var group in agreedTypes.GroupBy(type=>type).Take(6-mini)){MarketMini(UnitPortrait(group.Key),40+mini*30,row);var p=MarketRect(40+mini++*30,row+14,1,1);Text(group.Count().ToString(),p.X,p.Y,gold,.33f*MarketScale);}
  }
  int score=simulation.NegotiationScore(n);int height=Math.Clamp(score*84/100,0,84);if(!n.Alliance&&height>0)batch.Draw(white,MarketRect(253,200-height,10,height),palette[height<=30?0x91:height<=65?0xb4:0x30]);
  var title=MarketRect(1,324,1,1);string status=n.Alliance?"Alianca: confirmar aceita; X recusa":n.Stage switch{"Ready"=>"Escolha recursos ou tropas nos icones.","Incoming"=>"Oferta recebida: o pedido da tribo e fixo.","Evaluating"=>"Avaliando: "+simulation.NegotiationScore(n)+"% | "+Math.Max(0,n.Deadline-simulation.Tick)/30+"s","Delivery"=>"Leve as tropas indicadas para a Bolsa.",_=>simulation.TradeNotice};Text(status,title.X,title.Y,Color.Wheat,.4f*MarketScale);var bottom=MarketRect(1,341,1,1);Text(n.Stage=="Delivery"?"Na Bolsa: suas "+simulation.ExchangeDelivered(n,0)+" / "+n.GiveTypes.Count+" | tribo "+simulation.ExchangeDelivered(n,n.Owner)+" / "+n.TakeTypes.Count:marketQuantityKind>=0?"Quantidade: "+marketQuantity+" - Enter confirma":"Tribo "+n.Owner+" | Simpatia "+simulation.Goodwill(n.Owner),bottom.X,bottom.Y,gold,.4f*MarketScale);
  if(marketQuantityKind>=0){var q=MarketRect(36,109,193,27);batch.Draw(white,q,new Color(24,23,20,240));Text(marketQuantity.ToString(),q.X+5,q.Y+2,Color.Wheat,.55f*MarketScale);}
 }
}
