using System.Text.Json;
namespace RisingLands;
public record FoodVisual(int Code,int Quantity,List<DrawCommand> Commands);
public record FoodRules(int[] Capacities,int[] HarvestTransitions,List<FoodVisual> Visuals);
public sealed partial class Simulation {
    public static readonly FoodRules FoodData=JsonSerializer.Deserialize<FoodRules>(File.ReadAllText(PackageLayout.FromBase("Content/Data/food_rules.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true,PropertyNamingPolicy=JsonNamingPolicy.SnakeCaseLower})!;
    public static readonly List<FoodVisual> PileVisuals=JsonSerializer.Deserialize<List<FoodVisual>>(File.ReadAllText(PackageLayout.FromBase("Content/Data/resource_pile_visuals.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
    public static int ResourceIndex(int kind)=>kind==1?0:kind==2?4:8;
    public static string ResourceName(int kind)=>kind==1?"Alimento":kind==2?"Rocha":"Engrenagem";
    public bool IsResourcePile(int cell)=>cell>=0&&cell<65536&&World.Objects[cell] is >=66 and <=68&&World.Quantities[cell]>0;
    bool IsCargoSource(Actor u,int cell)=>cell>=0&&cell<65536&&OpponentKnowsResource(u.Owner,cell)&&OpponentLocalResource(u,cell)&&!OpponentPreservesPlant(u,cell)&&(IsResourcePile(cell)&&World.Objects[cell]==66+u.CargoKind||OriginalWorkerPolicy.Collects(u.Type,1)&&u.CargoKind==1&&World.Objects[cell] is >=29 and <=52);
    readonly ushort[] InitialResources;readonly byte[] InitialObjects,InitialQuantities;
    public bool IsFood(int cell)=>cell>=0&&cell<65536&&(World.Objects[cell] is >=29 and <=52 || World.Objects[cell]==67&&World.Quantities[cell]>0);
    public int Capacity(Actor u)=>CargoCapacity(u);
    public string FoodStatus(Actor u)=>u.FoodJob switch{1=>"Indo buscar recurso",2=>"Recolhendo recurso",3=>"Levando ao destino",4=>u.Food>0?"Aguardando entrega livre":"Aguardando acesso a coleta",_=>u.Food>0?"Carga pronta para entrega":"Aguardando ordem"};
    public string FoodDestinationName(Actor u)=>u.FoodDestinationKind switch{1=>"Chao ("+(u.FoodDestination&255)+","+(u.FoodDestination>>8)+")",2=>World.Buildings.FirstOrDefault(b=>b.Slot==u.FoodDestination) is {} b?BuildName(b.Type)+" #"+b.Slot:"Estrutura indisponivel",_=>"Escolha onde entregar"};
    public bool Gather(Actor u,int cell,bool respond=true){if(!GatherAt(u,cell,true))return false;if(respond)EmitUnitSound(u,0xf,4);return true;}
    bool GatherAt(Actor u,int cell,bool newArea){
        if(!OpponentLocalResource(u,cell))return false;
        if(newArea&&cell>=0&&cell<65536&&World.Objects[cell] is >=29 and <=52){
            foreach(int candidate in HarvestCandidates(u,cell,cell,true))if(GatherAt(u,candidate,false)){u.GroundDrops.Clear();u.FoodField=cell;u.FoodPlantsOnly=true;u.OverflowCell=-1;return true;}
            return false;
        }
        if(u.Owner is <0 or >3||!OriginalWorkerPolicy.Carrier(u.Type)||!(IsResourcePile(cell)&&OriginalWorkerPolicy.Collects(u.Type,World.Objects[cell]-66)||OriginalWorkerPolicy.Collects(u.Type,1)&&IsFood(cell))||!VisualCatalog.Visible(u)||u.FoodDestinationKind==1&&u.FoodDestination==cell)return false;
        int kind=IsResourcePile(cell)?World.Objects[cell]-66:1;if(u.Rock>0||u.Food>=Capacity(u)||u.Food>0&&u.CargoKind!=kind)return false;
        if(!Order(u,cell&255,cell>>8,true))return false;
        CancelFeeding(u);u.OverflowCell=-1;u.CargoKind=kind;u.MineJob=0;DetachBuilder(u.Slot);
        if(newArea){u.GroundDrops.Clear();u.OverflowCell=-1;u.FoodPlantsOnly=World.Objects[cell] is >=29 and <=52;}
        if(newArea)u.FoodField=cell;
        u.FoodSourceBuilding=-1;u.FoodSource=cell;u.FoodJob=1;u.WorkTicks=0;return true;
    }
    // Provisional local field policy, not a claim about the original search radius.
    public const int FoodFieldRadius=6;
    IEnumerable<int> HarvestCandidates(Actor u,int anchor,int origin,bool preferFree=false){
        int x=anchor&255,y=anchor>>8;
        var claims=World.Units.Where(a=>a!=u&&a.Health>0&&a.FoodJob is 1 or 2).GroupBy(a=>a.FoodSource).ToDictionary(g=>g.Key,g=>g.Count());
        var cells=new List<int>();
        for(int ny=Math.Max(0,y-FoodFieldRadius);ny<=Math.Min(World.Height-1,y+FoodFieldRadius);ny++)
            for(int nx=Math.Max(0,x-FoodFieldRadius);nx<=Math.Min(World.Width-1,x+FoodFieldRadius);nx++){
                int cell=ny*256+nx;if(Math.Abs(nx-x)+Math.Abs(ny-y)<=FoodFieldRadius&&IsCargoSource(u,cell)&&(!u.FoodPlantsOnly||World.Objects[cell] is >=29 and <=52)&&!(u.FoodDestinationKind==1&&u.FoodDestination==cell)&&!u.GroundDrops.Contains(cell))cells.Add(cell);
            }
        return cells.OrderBy(c=>preferFree&&claims.GetValueOrDefault(c)>=(World.Objects[c] is 51 or 52?2:1)?1:0).ThenBy(c=>Math.Abs((c&255)-(origin&255))+Math.Abs((c>>8)-(origin>>8))).ThenBy(c=>claims.GetValueOrDefault(c)).ThenBy(c=>c);
    }
    bool ContinueHarvest(Actor u){
        if(!u.RepeatFood)return false;
        if(u.Food>=Capacity(u))return Deliver(u);
        // A loaded pile carrier chose a mine: finish delivery before starting mining.
        if(u.Type==7&&u.CargoKind==2&&u.FoodSource<0&&u.FoodSourceBuilding<0&&u.MineIndex>=0){
            if(u.Food>0)return Deliver(u);
            u.RockDestination=u.FoodDestinationKind==2?u.FoodDestination:-1;u.RockGround=u.FoodDestinationKind==1?u.FoodDestination:-1;u.FoodJob=0;return ContinueMine(u);
        }
        if(u.FoodSourceBuilding>=0){if(GatherWarehouse(u,u.FoodSourceBuilding,false))return true;u.FoodJob=4;return false;}
        if(u.Type==7&&u.FoodField<0&&u.FoodSource>=0)u.FoodField=u.FoodSource;
        if(u.FoodField>=0){
            var cells=HarvestCandidates(u,u.FoodField,u.Y/1024*256+u.X/1024).ToArray();
            foreach(int cell in cells)if(GatherAt(u,cell,false))return true;
            u.FoodJob=cells.Length>0||u.PersistentCollection&&u.FoodDestinationKind!=0?4:0;return false;
        }
        if(IsCargoSource(u,u.FoodSource)&&GatherAt(u,u.FoodSource,false))return true;
        u.FoodJob=IsCargoSource(u,u.FoodSource)||u.PersistentCollection&&u.FoodDestinationKind!=0?4:0;return false;
    }
    public bool GatherNearest(Actor u){foreach(int cell in Enumerable.Range(0,65536).Where(IsFood).OrderBy(i=>Math.Abs((i&255)-u.X/1024)+Math.Abs((i>>8)-u.Y/1024)))if(Gather(u,cell))return true;return false;}
    public Building? BuildingAt(int cell){EnsureSpatialIndex();return cell>=0&&cell<65536?cellBuildings[cell]:null;}
    public bool AcceptsFood(Building b)=>b.Owner is >=0 and <=3&&b.Type==3;
    public string? FoodDestinationError(Actor u,int kind,int target){
        if(u.Owner is <0 or >3||!OriginalWorkerPolicy.Carrier(u.Type)||!VisualCatalog.Visible(u))return "Selecione um trabalhador.";
        if(kind==2){var b=World.Buildings.FirstOrDefault(b=>b.Slot==target);return b==null||b.Owner!=u.Owner||!(AcceptsFood(b)||b.Type==4&&u.Type==5&&u.CargoKind==1)?"Escolha armazem ativo ou cultivo para comida.":null;}
        if(kind!=1||target<0||target>=65536||!Passable(target&255,target>>8)||BuildingAt(target)!=null||MineAt(target)>=0||(World.Objects[target]!=0&&World.Objects[target]!=66+u.CargoKind))return "Escolha um ponto livre no chao.";
        if(u.FoodSourceBuilding<0&&target==u.FoodSource)return "Escolha um destino diferente da coleta.";
        if(!GroundDeliveryCells(target,u.CargoKind,u.FoodSourceBuilding<0?u.FoodSource:-1).Any())return "Sem espaco livre perto do destino.";
        return null;
    }
    IEnumerable<int> DeliveryCells(int kind,int target){
        if(kind==1){foreach(int c in GroundDeliveryCells(target,1))yield return c;yield break;}
        var b=World.Buildings.FirstOrDefault(b=>b.Slot==target&&(AcceptsFood(b)||b.Type==4));if(b==null)yield break;
        foreach(var cell in BuildingInteractionCells(b))yield return cell;
    }
    // Only destinations explicitly selected by the player are used. No nearest-base fallback.
    public bool SetFoodDestination(Actor u,int kind,int target){
        if(FoodDestinationError(u,kind,target)!=null)return false;
        var oldRoute=u.Route;bool found=false;int arrival=-1;
        foreach(int cell in kind==1?GroundDeliveryCells(target,u.CargoKind,u.FoodSourceBuilding<0?u.FoodSource:-1):DeliveryCells(kind,target).OrderBy(i=>Math.Abs((i&255)-u.X/1024)+Math.Abs((i>>8)-u.Y/1024)))if(Order(u,cell&255,cell>>8,true)){found=true;arrival=cell;break;}
        if(!found)return false;
        CancelFeeding(u);u.OverflowCell=-1;u.FoodDestinationKind=kind;u.FoodDestination=target;
        if(u.Food>0){if(u.MineJob!=0){u.FoodSource=u.FoodField=u.FoodSourceBuilding=-1;}u.MineJob=u.MineTicks=0;DetachBuilder(u.Slot);u.DeliveryCell=arrival;u.FoodJob=3;u.WorkTicks=0;}else u.Route=oldRoute;
        return true;
    }
    bool Deliver(Actor u){
        if(u.FoodDestinationKind==0){u.FoodJob=0;return false;}
        u.OverflowCell=-1;var depot=u.FoodDestinationKind==2?World.Buildings.FirstOrDefault(b=>b.Slot==u.FoodDestination&&AcceptsFood(b)&&Inhabited(b)):null;
        if(depot!=null&&WarehouseSpace(u.Owner)==0)return RouteWarehouseOverflow(u,depot,false);
        if(SetFoodDestination(u,u.FoodDestinationKind,u.FoodDestination))return true;
        u.FoodJob=4;return false;
    }
    void FoodStep(Actor u){
        if(u.FoodJob==0)return;
        if(u.FoodSourceBuilding>=0&&u.FoodJob is 1 or 2 or 4){WarehousePickupStep(u);return;}
        if(u.FoodJob==4){if(Tick%30==0){if(u.Food>0)Deliver(u);else if(IsCargoSource(u,u.FoodSource)){if(!GatherAt(u,u.FoodSource,false))ContinueHarvest(u);}else{u.FoodJob=0;ContinueHarvest(u);}}return;}
        if(u.FoodJob is 1 or 2&& (u.X!=(u.FoodSource&255)*1024||u.Y!=(u.FoodSource>>8)*1024)){u.FoodJob=4;return;}
        if(u.FoodJob==1){if(!IsCargoSource(u,u.FoodSource)){u.FoodJob=0;if(u.Food>0)Deliver(u);else ContinueHarvest(u);return;}u.FoodJob=2;EmitUnitSound(u,0x10);u.WorkTicks=IsResourcePile(u.FoodSource)?10:u.Type is 5 or 13?WorkerCountdown(u):60;return;}
        if(u.FoodJob==2){
            if(--u.WorkTicks>0)return;u.WorkTicks=0;int cell=u.FoodSource;
            if(!IsCargoSource(u,cell)||u.Food>=Capacity(u)){u.FoodJob=0;if(u.Food>0)Deliver(u);else ContinueHarvest(u);return;}
            if(IsResourcePile(cell)){World.Quantities[cell]--;if(World.Quantities[cell]==0)World.Objects[cell]=0;}else World.Objects[cell]=(byte)FoodData.HarvestTransitions[World.Objects[cell]];
            u.Food++;if(u.Food<Capacity(u)){if(IsCargoSource(u,cell)){u.FoodJob=1;return;}if(ContinueHarvest(u))return;}Deliver(u);return;
        }
        if(u.FoodJob==3){
            if(u.OverflowCell>=0){OverflowArrival(u,false);return;}
            var deliveryBuilding=u.FoodDestinationKind==2?World.Buildings.FirstOrDefault(b=>b.Slot==u.FoodDestination):null;
            if(deliveryBuilding!=null&&!ReachBuildingInteraction(u,deliveryBuilding,u.DeliveryCell))return;
            if((deliveryBuilding==null&&(u.X!=(u.DeliveryCell&255)*1024||u.Y!=(u.DeliveryCell>>8)*1024))||FoodDestinationError(u,u.FoodDestinationKind,u.FoodDestination)!=null){u.FoodJob=4;return;}
            if(u.FoodDestinationKind==1){int cell=u.DeliveryCell;if(!GroundResourceSpace(cell,u.CargoKind)){Deliver(u);return;}int quantity=World.Objects[cell]==66+u.CargoKind?World.Quantities[cell]:0;int amount=Math.Min(u.Food,31-quantity);World.Objects[cell]=(byte)(66+u.CargoKind);World.Quantities[cell]=(byte)(quantity+amount);u.Food-=amount;if(amount>0&&!u.GroundDrops.Contains(cell))u.GroundDrops.Add(cell);}
            else {
                var target=World.Buildings.First(b=>b.Slot==u.FoodDestination);if(target.Type==3&&!Inhabited(target)){u.FoodJob=4;return;}
                if(target.Type==4){
                  // Native transport reload 10 (0x44f08d), quantity 1 (0x44f0ad).
                  if(u.WorkTicks>0){u.WorkTicks--;return;}u.WorkTicks=9;
                  if(!DepositCropFood(u,target)){u.FoodJob=4;return;}
                  if(u.Food>0)return;
                }
                else{int owner=u.Owner;int accepted=Math.Min(u.Food,WarehouseSpace(owner));World.Resources[ResourceIndex(u.CargoKind)+owner]+=(ushort)accepted;if(owner>0)Opponent.Deposited[ResourceIndex(u.CargoKind)+owner]+=accepted;u.Food-=accepted;if(u.Food>0){RouteWarehouseOverflow(u,target,false);return;}}
            }
            // A full destination leaves the cargo with its carrier. The player chooses another destination.
            u.FoodJob=u.Food>0?4:0;
            if(u.Food==0)ContinueHarvest(u);
        }
    }
    static long PileTotal(byte[] objects,byte[] quantities,int code)=>Enumerable.Range(0,65536).Where(i=>objects[i]==code).Sum(i=>(long)quantities[i]);
    static long FoodTotal(byte[] objects,byte[] quantities,IEnumerable<Actor> actors,ushort[] resources){long n=actors.Where(u=>u.CargoKind==1).Sum(u=>(long)u.Food)+resources.Take(4).Sum(v=>(long)v);for(int i=0;i<65536;i++)n+=objects[i]==67?quantities[i]:objects[i] is 51 or 52?2:objects[i] is >=29 and <=50?1:0;return n;}
    void ValidateFoodSave(Save s){if(s.Objects==null||s.Quantities==null||s.Resources==null||s.Objects.Length!=65536||s.Quantities.Length!=65536||s.Resources.Length!=12)throw new InvalidDataException("Invalid resource arrays");
        foreach(var u in s.Units)if(u.CargoKind is <0 or >2||u.Food<0||u.Food+u.Rock>CargoCapacity(u,s.Research)||u.FoodJob<0||u.FoodJob>4||u.WorkTicks<0||u.WorkTicks>255||u.FoodSource< -1||u.FoodSource>=65536||u.DeliveryCell< -1||u.DeliveryCell>=65536||((u.FoodJob!=0||u.Food>0)&&(u.Owner is <0 or >3||!OriginalWorkerPolicy.Carrier(u.Type)))||(u.FoodJob is 1 or 2&&u.FoodSource<0)||(u.FoodJob==3&&u.DeliveryCell<0))throw new InvalidDataException("Invalid food job");
        foreach(var u in s.Units){
            if(u.GroundDrops==null||u.GroundDrops.Count>65536||u.GroundDrops.Distinct().Count()!=u.GroundDrops.Count||u.GroundDrops.Any(c=>c<0||c>=65536||(c&255)>=World.Width||(c>>8)>=World.Height))throw new InvalidDataException("Invalid ground delivery history");
            if(u.FoodSourceBuilding< -1||u.FoodSourceBuilding>=0&&!InitialBuildings.Concat(s.Constructions.Where(Complete).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).Any(b=>b.Slot==u.FoodSourceBuilding&&b.Owner==u.Owner&&AcceptsFood(b)))throw new InvalidDataException("Invalid warehouse source");
            if(u.FoodJob!=0&&(u.MineJob!=0||s.Constructions.Any(c=>HasBuilder(c,u.Slot)&&!Complete(c))))throw new InvalidDataException("Conflicting worker orders");
            if(u.FoodField!= -1&&(!OriginalWorkerPolicy.Carrier(u.Type)||u.Owner is <0 or >3||u.FoodField<0||u.FoodField>=65536||(u.FoodField&255)>=World.Width||(u.FoodField>>8)>=World.Height))throw new InvalidDataException("Invalid crop area");
            if(u.FoodDestinationKind is <0 or >2||u.FoodDestinationKind==0&&u.FoodDestination!=-1||u.FoodDestinationKind==1&&(u.FoodDestination<0||u.FoodDestination>=65536||(u.FoodDestination&255)>=World.Width||(u.FoodDestination>>8)>=World.Height||u.FoodSourceBuilding<0&&u.FoodDestination==u.FoodSource)||u.FoodDestinationKind==2&&!InitialBuildings.Concat(s.Constructions.Where(c=>Complete(c)).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell))).Any(b=>b.Slot==u.FoodDestination&&b.Owner==u.Owner&&(b.Type==3||b.Type==4&&u.Type==5&&u.CargoKind==1))||u.FoodJob==3&&u.FoodDestinationKind==0)throw new InvalidDataException("Invalid food destination");
        }
        var spending=Spending(s.Constructions,s.Recruitments,s.Special??new(),s.TrainingQueue??new());for(int k=0;k<12;k++)spending[k]+=OpeningDebit(s,k);for(int i=0;i<12;i++)if(i%4!=0&&s.Resources[i]<Math.Max(0,InitialResources[i]-spending[i]+TradeNet(s,i/4,i%4)-(i<4?(s.Hunger?.Consumed[i]??0):0)+(i<4?(s.Hunger?.SharedFoodNet[i]??0):0)))throw new InvalidDataException("Unsupported resource mutation");
        for(int i=0;i<65536;i++){byte before=InitialObjects[i],after=s.Objects[i];bool mutable=before==0||before is >=29 and <=52 or >=66 and <=68;
            if(after!=before&&!ValidGrowthMutation(s,i)&&!(mutable&&(after==0||after is >=66 and <=68))&&!(before is 51 or 52&&after==before-10))throw new InvalidDataException($"Unsupported terrain mutation at {i}: {before} -> {after}");
            if(before is not (0 or >=66 and <=68)&&after is not (>=66 and <=68)&&s.Quantities[i]!=InitialQuantities[i])throw new InvalidDataException("Invalid quantity mutation");
            if(after is >=66 and <=68&&(after!=before||s.Quantities[i]!=InitialQuantities[i])&&(s.Quantities[i]==0||s.Quantities[i]>Math.Max(31,(int)InitialQuantities[i])))throw new InvalidDataException("Invalid resource pile");
        }
        if(PileTotal(s.Objects,s.Quantities,66)+s.Units.Where(u=>u.CargoKind==0).Sum(u=>(long)u.Food)+s.Resources.Skip(8).Sum(v=>(long)v)+spending.Skip(8).Sum()!=PileTotal(InitialObjects,InitialQuantities,66)+InitialResources.Skip(8).Sum(v=>(long)v)+(s.Tests?.Parts??0))throw new InvalidDataException("Parts conservation failed");

        for(int i=9;i<12;i++)if(s.Resources[i]!=InitialResources[i]-spending[i]+TradeNet(s,2,i%4)+(s.Opponent?.Deposited[i]??0))throw new InvalidDataException("Unsupported foreign parts mutation");
        if(FoodTotal(s.Objects,s.Quantities,s.Units,s.Resources)+(s.Cultures?.Sum(c=>(long)c.Food+(s.Version<=16?c.Born*10:c.Consumed))??0)+spending.Take(4).Sum()+(s.Research?.ExtraPurchased==true?50:0)+(s.Environment?.DecayedFood??0)+(s.Hunger?.Consumed.Sum()??0)!=FoodTotal(InitialObjects,InitialQuantities,Array.Empty<Actor>(),InitialResources)+(s.Environment?.GeneratedFood??0)+(s.Tests?.Food??0))throw new InvalidDataException("Food conservation failed");
    }
}

