namespace RisingLands;
public sealed class Negotiation {
 public int Id{get;set;}public int Messenger{get;set;}public int Target{get;set;}public int Home{get;set;}public int Owner{get;set;}public int Cell{get;set;}
 public bool Incoming{get;set;}public bool Alliance{get;set;}public string Stage{get;set;}="Travelling";public long Deadline{get;set;}
 public int[] Give{get;set;}=new int[3];public int[] Take{get;set;}=new int[3];public List<int> GiveUnits{get;set;}=new();public List<int> TakeUnits{get;set;}=new();
 public List<int> GiveTypes{get;set;}=new();public List<int> TakeTypes{get;set;}=new();
 public Dictionary<int,long> Entering{get;set;}=new();
 public Dictionary<int,int> DeliveryCells{get;set;}=new();public HashSet<int> Stored{get;set;}=new();
 public bool Active=>Stage is "Travelling" or "Ready" or "Evaluating" or "Incoming" or "Delivery";
}
public sealed class UnitExchange {public int Session{get;set;}public int Slot{get;set;}public int From{get;set;}public int To{get;set;}public long Tick{get;set;}}
public sealed partial class Simulation {
 // Original counters: 17*50 evaluation updates; delivery expires after >32*50 updates.
 public const int TradeEvaluationTicks=850,TradeDeliveryTicks=1650;
 static readonly int[] TradeUnitWeights={10,13,22,18,20,10,13,10,16,5,10,25,35,30,25,45,0,0,0,0,0,0,0,0,0,25};
 public string TradeNotice{get;private set;}="";
 public static bool UnitExchangeable(int type)=>type!=9&&(type is >=0 and <=17 or 25);
 public Building? Exchange(int owner)=>World.Buildings.FirstOrDefault(b=>b.Type==8&&b.Owner==owner);
 public Negotiation? NegotiationFor(int owner)=>Special.Negotiations.LastOrDefault(n=>n.Owner==owner&&n.Active);
 public IEnumerable<Actor> TradeUnits(int owner,int session=-1)=>World.Units.Where(a=>a.Owner==owner&&a.Health>0&&(VisualCatalog.Visible(a)||Special.ExchangeResidents.Any(r=>r.Slot==a.Slot&&r.Stored))&&UnitExchangeable(a.Type)&&!Special.Mirrors.Any(m=>m.Seed.Slot==a.Slot)&&!Recruitments.Any(j=>j.SettlerSlot==a.Slot&&!j.Finished)&&!Special.Negotiations.Any(n=>n.Id!=session&&n.Active&&(n.Messenger==a.Slot||n.GiveUnits.Contains(a.Slot)||n.TakeUnits.Contains(a.Slot))));
 public bool Allied(int owner)=>owner is >0 and <4&&Campaign.Alliances[owner];
 public int Goodwill(int owner)=>owner is >0 and <4?Campaign.Goodwill[owner]:0;
 public string? MessengerEntryError(Actor actor,int slot){
  var b=World.Buildings.FirstOrDefault(b=>b.Slot==slot&&b.Type is 2 or 16);
  if(actor.Type!=10||!VisualCatalog.Visible(actor)||actor.Owner is <0 or >3)return "Selecione um mensageiro disponível.";
  if(b==null||actor.Owner==b.Owner||b.Owner is <0 or >3||(actor.Owner!=0&&b.Owner!=0))return "Envie o mensageiro ao Mercado ou Santuário de outro clã.";
  if(!Inhabited(b))return "O prédio de destino precisa estar concluído e ter colono.";
  int owner=actor.Owner==0?b.Owner:actor.Owner;
  if(NegotiationFor(owner)!=null)return "Já existe uma negociação com esse clã. Use a cartola para continuar.";
  if(!World.Buildings.Any(x=>x.Owner==0&&x.Type==b.Type&&Inhabited(x)))return b.Type==16?"Seu Mercado precisa estar concluído e ter colono.":"Seu Santuário precisa estar concluído e ter colono.";
  if(b.Type==16&&Exchange(0)==null)return "Construa sua Bolsa de Trocas antes de negociar.";
  if(b.Type==16&&Exchange(owner)==null)return "Esse clã ainda precisa construir uma Bolsa de Trocas.";
  return null;
 }
 public bool SendMessenger(Actor actor,int slot){if(MessengerEntryError(actor,slot)!=null)return false;var b=World.Buildings.First(b=>b.Slot==slot);int owner=actor.Owner==0?b.Owner:actor.Owner;
  var home=World.Buildings.FirstOrDefault(x=>x.Owner==0&&x.Type==(b.Type==2?2:16)&&Inhabited(x));if(home==null||b.Type==16&&(Exchange(0)==null||Exchange(owner)==null))return false;
  foreach(int cell in BuildingInteractionCells(b)){if(!Order(actor,cell&255,cell>>8,true))continue;StopExceptTravel(actor);Special.Contacts.RemoveAll(c=>c.Messenger==actor.Slot);Special.Negotiations.Add(new(){Id=Special.Negotiations.Count+1,Messenger=actor.Slot,Target=b.Slot,Home=home.Slot,Owner=owner,Cell=cell,Incoming=actor.Owner!=0,Alliance=b.Type==2});return true;}return false;
 }
 void StopExceptTravel(Actor actor){var route=actor.Route.ToList();if(actor.Owner==0)Stop(actor);else{CancelFeeding(actor);ClearOrders(actor);ResidentTrips.RemoveAll(j=>j.Slot==actor.Slot);actor.FoodSourceBuilding=actor.FoodSource=actor.FoodField=-1;actor.RepeatFood=actor.RepeatMine=false;}actor.Route=route;}
 bool TradeDeliveryPending(Actor actor)=>Special.ExchangeResidents.Any(r=>r.Slot==actor.Slot)||Special.Negotiations.Any(n=>n.Stage=="Delivery"&&n.DeliveryCells.ContainsKey(actor.Slot));
 public bool SubmitTrade(Negotiation n,int[] give,int[] take,IEnumerable<int>? giveUnits=null,IEnumerable<int>? takeUnits=null){if(!n.Active||n.Alliance||n.Stage is not ("Ready" or "Incoming")||give.Length!=3||take.Length!=3||give.Concat(take).Any(x=>x<0||x>65535))return false;
  var gu=(giveUnits??Array.Empty<int>()).Distinct().ToList();var tu=(takeUnits??Array.Empty<int>()).Distinct().ToList();if(gu.Count>8||tu.Count>8||gu.Intersect(tu).Any()||gu.Any(id=>!TradeUnits(0,n.Id).Any(a=>a.Slot==id))||tu.Any(id=>!TradeUnits(n.Owner,n.Id).Any(a=>a.Slot==id))||give.Sum()+take.Sum()+gu.Count+tu.Count==0)return false;
  if(n.Incoming&&(!give.SequenceEqual(n.Give)||!gu.Order().SequenceEqual(n.GiveUnits.Order())))return false;
  if(Exchange(0) is {} ownBag&&gu.Count>ResidentCapacity(ownBag)-Occupants(ownBag.Slot)||Exchange(n.Owner) is {} otherBag&&tu.Count>ResidentCapacity(otherBag)-Occupants(otherBag.Slot))return false;
  for(int kind=0;kind<3;kind++)if(World.Resources[kind*4]<give[kind]||World.Resources[kind*4+n.Owner]<take[kind])return false;
  n.Give=(int[])give.Clone();n.Take=(int[])take.Clone();n.GiveUnits=gu;n.TakeUnits=tu;n.GiveTypes=gu.Select(id=>World.Units.First(a=>a.Slot==id).Type).ToList();n.TakeTypes=tu.Select(id=>World.Units.First(a=>a.Slot==id).Type).ToList();n.Stage="Evaluating";n.Deadline=Tick+TradeEvaluationTicks;return true;
 }
 long ExchangeUnitWeight(int type){if(TradeUnitWeights[type]>0)return TradeUnitWeights[type];var rule=RecruitData.Rules.FirstOrDefault(r=>r.Type==type);return rule==null?0:rule.Food+rule.Rock+2L*rule.Parts;}
 public int NegotiationScore(Negotiation n){var give=n.GiveTypes.Count==n.GiveUnits.Count?n.GiveTypes:n.GiveUnits.Select(id=>World.Units.First(a=>a.Slot==id).Type).ToList();var take=n.TakeTypes.Count==n.TakeUnits.Count?n.TakeTypes:n.TakeUnits.Select(id=>World.Units.First(a=>a.Slot==id).Type).ToList();long offer=Enumerable.Range(0,3).Sum(k=>(long)n.Give[k]*(k==2?2:1))+give.Sum(ExchangeUnitWeight);long request=Enumerable.Range(0,3).Sum(k=>(long)n.Take[k]*(k==2?2:1))+take.Sum(ExchangeUnitWeight);long adjusted=offer*(80+Goodwill(n.Owner)+(Allied(n.Owner)?40:0))/100;return request==0?adjusted>0?200:0:(int)Math.Clamp(adjusted*100/request,0,65535);}
 public void CancelNegotiation(Negotiation n,string reason="Troca cancelada."){if(!n.Active)return;ReleaseStored(n);FinishNegotiation(n,"Cancelled",reason);}
 public bool AnswerAlliance(Negotiation n,bool accept){if(!n.Alliance||n.Stage!="Incoming")return false;if(accept){Campaign.Alliances[n.Owner]=true;Campaign.Goodwill[n.Owner]=Math.Clamp(Campaign.Goodwill[n.Owner]+5,-50,50);}FinishNegotiation(n,accept?"Completed":"Rejected",accept?"Alianca firmada.":"Alianca recusada.");UpdateVisibility();return true;}
 void FinishNegotiation(Negotiation n,string stage,string notice){n.Entering.Clear();n.Stage=stage;n.Deadline=0;TradeNotice=notice;var a=World.Units.FirstOrDefault(a=>a.Slot==n.Messenger);if(a!=null){a.Route.Clear();a.LegacyState=12;}Special.Contacts.RemoveAll(c=>c.Messenger==n.Messenger);}
 void ReleaseStored(Negotiation n){foreach(int id in n.Stored){if(Special.ExchangeResidents.Any(r=>r.Slot==id&&r.Stored))continue;var a=World.Units.FirstOrDefault(a=>a.Slot==id);if(a==null)continue;var bag=Exchange(a.Owner);if(bag!=null){int exit=BuildingExit(bag);if(exit>=0)PlaceAtExit(a,bag,exit);}a.LegacyState=7;}n.Stored.Clear();n.DeliveryCells.Clear();}
 public int TradeGoodwillGain(Negotiation n){long offer=Enumerable.Range(0,3).Sum(k=>(long)n.Give[k]*(k==2?2:1))+n.GiveTypes.Sum(ExchangeUnitWeight);long request=Enumerable.Range(0,3).Sum(k=>(long)n.Take[k]*(k==2?2:1))+n.TakeTypes.Sum(ExchangeUnitWeight);long surplus=offer*(80+Goodwill(n.Owner)+(Allied(n.Owner)?40:0))/100-request;return (int)Math.Clamp(surplus/5,0,100000); }
 bool SettleTrade(Negotiation n){if(n.Stage is not ("Evaluating" or "Delivery" or "Incoming")||n.GiveTypes.Concat(n.TakeTypes).Any(t=>!UnitExchangeable(t)))return false;int goodwillGain=TradeGoodwillGain(n);for(int k=0;k<3;k++){int a=World.Resources[k*4],b=World.Resources[k*4+n.Owner];if(a<n.Give[k]||b<n.Take[k]||a-n.Give[k]+n.Take[k]>65535||b-n.Take[k]+n.Give[k]>65535)return false;}
  foreach(int id in n.GiveUnits.Concat(n.TakeUnits)){var actor=World.Units.FirstOrDefault(a=>a.Slot==id);if(actor==null||actor.Health<=0||!n.Stored.Contains(id)||actor.Owner!=(n.GiveUnits.Contains(id)?0:n.Owner))return false;var bag=Exchange(actor.Owner==0?n.Owner:0);if(bag==null||BuildingExit(bag)<0)return false;}
  for(int k=0;k<3;k++){World.Resources[k*4]=(ushort)(World.Resources[k*4]-n.Give[k]+n.Take[k]);World.Resources[k*4+n.Owner]=(ushort)(World.Resources[k*4+n.Owner]-n.Take[k]+n.Give[k]);}
  Special.Trades.Add(new(){NativeValuation=true,Session=true,SessionId=n.Id,Sympathy=Goodwill(n.Owner),Relation=Allied(n.Owner)?1:0,Market=n.Home,Partner=World.Buildings.First(b=>b.Type==16&&b.Owner==n.Owner).Slot,Owner=n.Owner,Given=(int[])n.Give.Clone(),Received=(int[])n.Take.Clone(),Tick=Tick});
  foreach(int id in n.GiveUnits.Concat(n.TakeUnits)){var a=World.Units.First(a=>a.Slot==id);int from=a.Owner;a.Owner=from==0?n.Owner:0;var bag=Exchange(a.Owner)!;PlaceAtExit(a,bag,BuildingExit(bag));a.LegacyState=7;a.Route.Clear();a.FoodDestinationKind=0;a.FoodDestination=a.FoodSourceBuilding=a.FoodSource=a.FoodField=a.DeliveryCell=-1;a.RockDestination=a.RockGround=a.MineIndex=a.MineWorkCell=-1;a.RepeatFood=a.PersistentCollection=a.RepeatMine=false;Special.UnitExchanges.Add(new(){Session=n.Id,Slot=id,From=from,To=a.Owner,Tick=Tick});}
  Campaign.Goodwill[n.Owner]=Math.Clamp(Goodwill(n.Owner)+goodwillGain,-50,50);Special.ExchangeResidents.RemoveAll(r=>n.Stored.Contains(r.Slot));foreach(int id in n.Stored)Special.ExchangeEntering.Remove(id);n.Stored.Clear();n.DeliveryCells.Clear();FinishNegotiation(n,"Completed","Troca concluida. Recursos transferidos; tropas recebidas na Bolsa.");return true;
 }
 void NegotiationStep(){
  foreach(var n in Special.Negotiations.Where(n=>n.Active).ToArray()){var actor=World.Units.FirstOrDefault(a=>a.Slot==n.Messenger);var target=World.Buildings.FirstOrDefault(b=>b.Slot==n.Target);if(actor==null||actor.Health<=0||target==null||!Inhabited(target)){CancelNegotiation(n,"Negociacao interrompida.");continue;}
   if(n.Stage=="Travelling"){if(Feeding(actor)||!WalkIntoTradeBuilding(n,actor,target,n.Cell))continue;actor.LegacyState=12;n.Stage=n.Alliance?"Incoming":n.Incoming?"Incoming":"Ready";n.Deadline=Tick+TradeDeliveryTicks;if(!n.Alliance){Special.Contacts.Add(new(){Messenger=actor.Slot,Market=World.Buildings.First(b=>b.Owner==n.Owner&&b.Type==16).Slot,Cell=n.Cell,Arrived=true});if(n.Incoming){int kind=Enumerable.Range(0,3).OrderBy(k=>World.Resources[k*4+n.Owner]).First();n.Give[kind]=Math.Min(10,(int)World.Resources[kind*4]);}}else if(!n.Incoming){bool accept=Goodwill(n.Owner)>=0;Campaign.Alliances[n.Owner]=accept;if(accept)Campaign.Goodwill[n.Owner]=Math.Clamp(Campaign.Goodwill[n.Owner]+5,-50,50);FinishNegotiation(n,accept?"Completed":"Rejected",accept?"A tribo aceitou a alianca.":"A tribo recusou a alianca.");UpdateVisibility();}continue;}
   if(n.Stage=="Evaluating"&&Tick>=n.Deadline){if(NegotiationScore(n)<100){FinishNegotiation(n,"Rejected","A tribo recusou a troca. Ajuste a oferta.");continue;}if(n.GiveUnits.Count+n.TakeUnits.Count==0){if(!SettleTrade(n))FinishNegotiation(n,"Cancelled","Estoque insuficiente; troca cancelada.");}else{n.Stage="Delivery";n.Deadline=0;PrepareEnemyExchange(n);if(ResolveExchangeTroops(n))SettleTrade(n);if(n.Active)TradeNotice="Troca aceita. Reúna as classes e quantidades combinadas na Bolsa de Trocas.";}continue;}
   if(n.Stage=="Delivery"){if(Exchange(0)==null||Exchange(n.Owner)==null){CancelNegotiation(n,"Bolsa perdida; troca cancelada.");continue;}foreach(var pair in n.DeliveryCells.ToArray()){var a=World.Units.FirstOrDefault(u=>u.Slot==pair.Key);var bag=a==null?null:Exchange(a.Owner);if(a==null||a.Health<=0||bag==null){n.DeliveryCells.Remove(pair.Key);n.Entering.Remove(pair.Key);continue;}if(n.Stored.Contains(a.Slot)||Feeding(a)||!WalkIntoTradeBuilding(n,a,bag,pair.Value))continue;a.LegacyState=12;n.Stored.Add(a.Slot);}PrepareEnemyExchange(n);if(n.Active&&ResolveExchangeTroops(n)&&SettleTrade(n))continue;}
   if(n.Active&&n.Stage!="Delivery"&&Tick>=n.Deadline)CancelNegotiation(n,"Prazo encerrado; troca cancelada.");
  }
  if(Tick%150==0)foreach(var a in World.Units.Where(a=>a.Type==10&&a.Owner is >0 and <4&&VisualCatalog.Visible(a)&&a.Route.Count==0).ToArray()){if(Special.Negotiations.Any(n=>n.Messenger==a.Slot)||NegotiationFor(a.Owner)!=null)continue;OpponentVision(a.Owner);var target=World.Buildings.FirstOrDefault(b=>b.Owner==0&&b.Type==16&&Inhabited(b)&&Opponent.KnownStructures[a.Owner].ContainsKey(b.Slot));if(target!=null)SendMessenger(a,target.Slot);}
 }
 int ExpectedTradeOwner(int slot,SpecialState? state){int owner=InitialActors[slot].Owner;foreach(var x in state?.UnitExchanges??new()){if(x.Slot==slot){if(x.From!=owner||x.To is <0 or >3||x.From==x.To)throw new InvalidDataException("Invalid ownership ledger");owner=x.To;}}return owner;}
 bool TradeHidden(int slot,SpecialState? state)=>(state?.ExchangeResidents??new()).Any(r=>r.Slot==slot&&r.Stored)||(state?.Negotiations??new()).Any(n=>(n.Stage!="Travelling"&&n.Messenger==slot)||n.Stored.Contains(slot));
 bool SavedExchangeable(int type,Save s,Negotiation? n=null)=>UnitExchangeable(type)||type==9&&(s.Version<=38||n!=null&&!n.Active);
 void ValidateNegotiations(Save s,SpecialState state){if(state.WallPlans==null||state.Negotiations==null||state.UnitExchanges==null||state.Negotiations.Count>1000||state.UnitExchanges.Count>10000)throw new InvalidDataException("Invalid negotiation state");foreach(var p in state.WallPlans)if(p.Sites==null||p.Sites.Count>256||!s.Units.Any(a=>a.Slot==p.Builder&&a.Type==7&&a.Owner==0)||p.Sites.Any(id=>!s.Constructions.Any(c=>c.Id==id&&c.Type==9)))throw new InvalidDataException("Invalid wall queue");
  ValidateExchangeStorage(s,state);
  MigrateExchangeAgreements(s,state);
  foreach(var n in state.Negotiations){
   if(n.GiveTypes==null||n.TakeTypes==null||n.GiveTypes.Count>8||n.TakeTypes.Count>8||n.GiveTypes.Concat(n.TakeTypes).Any(t=>!SavedExchangeable(t,s,n))||n.Stage is "Evaluating" or "Delivery" or "Completed"&&(n.GiveTypes.Count!=n.GiveUnits.Count||n.TakeTypes.Count!=n.TakeUnits.Count))throw new InvalidDataException("Invalid troop agreement classes");
  }
  foreach(var n in state.Negotiations){if(n.Owner is <1 or >3||n.Give==null||n.Take==null||n.Give.Length!=3||n.Take.Length!=3||n.Give.Concat(n.Take).Any(v=>v<0||v>65535)||n.GiveUnits==null||n.TakeUnits==null||n.Stored==null||n.DeliveryCells==null||n.Entering==null||n.Entering.Count>17||n.Entering.Any(p=>p.Value<0||p.Value>s.Tick||!n.Active||n.Stage=="Travelling"&&p.Key!=n.Messenger||n.Stage=="Delivery"&&(!n.DeliveryCells.ContainsKey(p.Key)||n.Stored.Contains(p.Key))||n.Stage is not ("Travelling" or "Delivery")||!s.Units.Any(a=>a.Slot==p.Key&&a.Health>0))||n.GiveUnits.Count>8||n.TakeUnits.Count>8||n.GiveUnits.Intersect(n.TakeUnits).Any()||!s.Units.Any(a=>a.Slot==n.Messenger&&a.Type==10)||n.Active&&(n.Deadline<0||n.Stage is not ("Travelling" or "Delivery")&&n.Deadline<s.Tick)||n.Stored.Any(id=>!n.GiveUnits.Contains(id)&&!n.TakeUnits.Contains(id))||n.GiveUnits.Concat(n.TakeUnits).Any(id=>!s.Units.Any(a=>a.Slot==id&&SavedExchangeable(a.Type,s,n)))||n.Stage is not ("Travelling" or "Ready" or "Incoming" or "Evaluating" or "Delivery" or "Completed" or "Cancelled" or "Rejected"))throw new InvalidDataException("Invalid negotiation");}
  foreach(var trade in state.Trades.Where(t=>t.Session))if(!state.Negotiations.Any(n=>n.Id==trade.SessionId&&n.Stage=="Completed"&&!n.Alliance&&n.Owner==trade.Owner&&n.Home==trade.Market&&n.Give.SequenceEqual(trade.Given)&&n.Take.SequenceEqual(trade.Received)))throw new InvalidDataException("Trade/session mismatch");
  if(state.Negotiations.Select(n=>n.Id).Distinct().Count()!=state.Negotiations.Count)throw new InvalidDataException("Duplicate negotiation");
  foreach(var x in state.UnitExchanges)if(!state.Negotiations.Any(n=>n.Id==x.Session&&n.Stage=="Completed"&&(x.From==0&&x.To==n.Owner&&n.GiveUnits.Contains(x.Slot)||x.From==n.Owner&&x.To==0&&n.TakeUnits.Contains(x.Slot)))||x.Tick<0||x.Tick>s.Tick||!s.Units.Any(a=>a.Slot==x.Slot)||x.From is <0 or >3||x.To is <0 or >3||x.From==x.To)throw new InvalidDataException("Invalid exchanged unit");
 }
}


