using System.Text.Json;
namespace RisingLands;

// Laboratory navigation only: this is NOT the reconstructed CheminCalcul algorithm.
public sealed partial class Simulation {
    public World World {get;} public long Tick {get;private set;}
    readonly string initialState;
    public void Restart()=>Restore(initialState);
    readonly byte[] initialCollision;readonly List<Mine> InitialMines; readonly ushort[] InitialTiles;
    public const int LabStep=64; // 1/16 tile per laboratory tick; original speed not established.
    public Simulation(World world,CampaignProgress? campaign=null){World=world;Campaign=campaign==null?DefaultCampaign():CloneState(campaign);ValidateCampaign(Campaign);initialCollision=(byte[])world.Collision.Clone();InitialMines=world.Mines.Select(m=>new Mine{Cell=m.Cell,Quantity=m.Quantity}).ToList();InitialTiles=(ushort[])world.Tiles.Clone();InitializeSeventhCreatures();InitialActors=world.Units.ToDictionary(u=>u.Slot,u=>JsonSerializer.Deserialize<Actor>(JsonSerializer.Serialize(u))!);InitialBuildings=world.Buildings.ToList();InitialResources=(ushort[])world.Resources.Clone();InitialObjects=(byte[])world.Objects.Clone();InitialQuantities=(byte[])world.Quantities.Clone();Research=InitialResearch();InitializeEnemyCampaign();InitializePopulation();ApplyReducedRedOpening();ApplyFifthOpening();RemoveEnemyStartingCart();InitializeOcean();InitializeOpponentExploration();UpdateVisibility();initialState=Serialize();}
    public bool Passable(int x,int y) {
        if(x<0||y<0||x>=World.Width||y>=World.Height)return false;
        int tile=World.Tiles[y*256+x];
        if(GateCell(y*256+x))return GatePassable(y*256+x);
        if(WallAt(y*256+x))return false;
        if(DestroyedAt(y*256+x))return !BuildBlocks(y*256+x)&&World.Objects[y*256+x] is not (>=1 and <=11);
        // Restriction for this prototype only, not original movement masks.
        if(World.Collision[y*256+x]==initialCollision[y*256+x]&&World.Collision[y*256+x]!=255&&InteractionPassage(y*256+x))return true;
        // Native mountain art includes open ramps and plateau floors. CheminCalcul
        // tests the collision plane (0x42f5e8..0x42f603), not the mountain tile category.
        return !BuildBlocks(y*256+x) && World.Objects[y*256+x] is not (>=1 and <=11) && (World.Collision[y*256+x]&1)==0 && !(tile>=0x94&&tile<0x9d);
    }
    public bool InteractionPassage(int cell){if(DoorCorridor(cell))return true;foreach(var mine in World.Mines)if(mine.Cell==cell||mine.Cell+256==cell)return true;return false;}
    readonly int[] navigationPrevious=new int[65536],navigationQueue=new int[65536];
    static readonly (int X,int Y)[] NavigationDirections={(0,-1),(-1,0),(1,0),(0,1)};
    public bool Order(Actor u,int x,int y,bool keepJob=false,bool avoidOpponentDanger=false) {
        if(!VisualCatalog.Visible(u)||!CanTraverse(u,x,y))return false;
        if(!keepJob){if(u.Type==17&&Ocean.Jobs.Any(j=>j.Balloon==u.Slot&&j.Stage>0))return RedirectLoadedBalloon(u,y*256+x);Ocean.Jobs.RemoveAll(j=>j.Actor==u.Slot&&j.Stage<2);}
        int start=(u.Y/1024)*256+u.X/1024,goal=y*256+x;
        var danger=OpponentRouteDanger(u,avoidOpponentDanger);if(danger!=null&&danger[goal]>0)return false;
        var previous=navigationPrevious;Array.Fill(previous,-1);var queue=navigationQueue;int head=0,tail=0;queue[tail++]=start;previous[start]=start;
        if(Airborne(u)){int cx=u.X/1024,cy=u.Y/1024,dx=Math.Abs(x-cx),dy=Math.Abs(y-cy),sx=Math.Sign(x-cx),sy=Math.Sign(y-cy),error=dx-dy,prev=start;while(cx!=x||cy!=y){int e=2*error;if(e> -dy){error-=dy;cx+=sx;}if(e<dx){error+=dx;cy+=sy;}int next=cy*256+cx;previous[next]=prev;prev=next;}}
        while(head<tail && previous[goal]<0){int cell=queue[head++];foreach(var (dx,dy) in IsSixthMission?SixthNavigationDirections:NavigationDirections){int nx=(cell&255)+dx,ny=(cell>>8)+dy;if(!CanTraverse(u,nx,ny))continue;int n=ny*256+nx;if(previous[n]>=0||!SixthNavigationStep(u,cell,n)||!SafeRouteStep(danger,start,cell,n))continue;previous[n]=cell;queue[tail++]=n;}}
        if(previous[goal]<0)return RequestFlight(u,goal,keepJob);
        var path=new List<int>();for(int p=goal;p!=start;p=previous[p])path.Add(p);path.Reverse();if(goal==start&&(u.X!=x*1024||u.Y!=y*1024))path.Add(goal);u.Route=path;if(!keepJob){u.DefenseAcquireAfter=0;u.AutomaticDefense=false;u.DefenseAnchor=-1;u.CombatRetryAt=0;Opponent.Sieges.Remove(u.Slot);Opponent.Repairs.Remove(u.Slot);CancelFeeding(u);EmitUnitSound(u,0xb,4);CancelSpecialOrder(u);u.PersistentCollection=false;u.OverflowCell=-1;u.FoodSourceBuilding=u.FoodSource=u.FoodField=-1;if(u.Rock==0)u.MineIndex=-1;else u.RepeatMine=false;u.FoodPlantsOnly=false;ResidentTrips.RemoveAll(j=>j.Slot==u.Slot);u.MineJob=0;u.MineTicks=0;CancelResearchTrip(u);u.AttackWindup=0;u.AttackTarget=-1;u.FoodJob=0;u.WorkTicks=0;DetachBuilder(u.Slot);}return true;
    }
    public void Step(){if(MissionWon||MissionLost)return;Tick++;CreatureStep();OceanStep();OpponentStep();HungerStep();SpecialStep();EnvironmentStep();foreach(var u in World.Units){if(u.Health<=0||u.PetrifiedUntil>Tick||FlightHidden(u.Slot,Ocean))continue;if(u.Route.Count==0){if(!Feeding(u)&&!WaitingForFlight(u)){FoodStep(u);MiningStep(u);}continue;}int p=u.Route[0];if(!CanTraverse(u,p&255,p>>8)||!SixthNavigationStep(u,u.Y/1024*256+u.X/1024,p)){if(Tick%30==0){int goal=u.Route.Last();Order(u,goal&255,goal>>8,true);}continue;}int x=(p&255)*1024,y=(p>>8)*1024;int dx=Math.Sign(x-u.X),dy=Math.Sign(y-u.Y);u.Direction=dx<0?(dy<0?3:dy>0?15:1):dx>0?(dy<0?7:dy>0?11:9):dy<0?5:13;u.X+=Math.Clamp(x-u.X,-LabStep,LabStep);u.Y+=Math.Clamp(y-u.Y,-LabStep,LabStep);u.AnimationCounter++;if(u.AnimationCounter>=u.AnimationDelay){u.AnimationCounter=0;u.AnimationFrame=(u.AnimationFrame+1)%Math.Max(1,u.AnimationCount);}if(u.X==x&&u.Y==y){u.Route.RemoveAt(0);EmitUnitSound(u,8);}}CombatStep();SiegeStep();RepairStep();BuildStep();GateStep();WallQueueStep();PopulationStep();RecruitStep();ResearchStep();OpponentResearchStep();UpdateVisibility();MissionStep();}
    public string Serialize(){PruneBaseGuardOwnership();return JsonSerializer.Serialize(new Save(46,World.SourceHash,World.MapName,Tick,World.Units,World.Resources,World.Objects,World.Quantities,Constructions,Recruitments,Arrows,Research,LaboratoryWorkers,World.Mines,MissionChecks,World.BuildingResidents,ResidentTrips,Cultures,BornActors,Campaign,Reversions,Exploration,EnvironmentState,Tests,Special,Hunger,Opponent,IsSeventhMission?Creatures:null,Ocean,TrainingQueue));}
    void RestoreCore(string json) {
        var save=JsonSerializer.Deserialize<Save>(json)??throw new InvalidDataException("Empty save");
        if(save.Version is not (4 or 5 or 6 or 7 or 8 or 9 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 20 or 21 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 37 or 38 or 39 or 40 or 41 or 42 or 43 or 44 or 45 or 46)||save.SourceHash!=World.SourceHash||save.MapName!=World.MapName||save.Tick<0)throw new InvalidDataException("Save version/map mismatch");
        ValidateTestState(save);
        Campaign=save.Version>=18?(save.Campaign??throw new InvalidDataException("Missing campaign state")):DefaultCampaign();ValidateCampaign(Campaign);if(save.Version<33)InitializeEnemyCampaign();else if(Campaign.EnemyMission!=MissionNumber)throw new InvalidDataException("Missing enemy research initialization");if(Campaign.TransitionOpen&&save.MissionChecks!=100)throw new InvalidDataException("Transition requires victory");
        if(save.Units==null||save.Units.Count!=InitialActors.Count)throw new InvalidDataException("Invalid units");
        if(save.Version<=16&&save.Research!=null&&save.Research.Agriculture==null)save.Research.Agriculture=InitialResearch().Agriculture;
        if(save.Version<8)foreach(var u in save.Units)if(u.FoodJob==3){u.FoodJob=0;u.WorkTicks=0;u.Route.Clear();}
        if(save.Version<10)foreach(var u in save.Units)u.FoodField=-1; // Preserve legacy single-source orders.
        if(save.Version<9)save=save with{Research=InitialResearch(),LaboratoryWorkers=new()};
        if(save.Version<11)save=save with{Mines=InitialMines.Select(m=>new Mine{Cell=m.Cell,Quantity=m.Quantity}).ToList()};
        if(save.Version<13)foreach(var u in save.Units)u.CargoKind=1;
        if(save.Version<12)save=save with{MissionChecks=0};
        if(save.Version<14)foreach(var u in save.Units){
            u.RockGround=-1;
            if(u.RockDestination>=0&&InitialBuildings.Any(b=>b.Slot==u.RockDestination&&b.Type==2)){u.RockDestination=-1;if(u.MineJob==3){u.MineJob=0;u.Route.Clear();}}
            if(u.FoodDestinationKind==2&&InitialBuildings.Any(b=>b.Slot==u.FoodDestination&&b.Type==2)){u.FoodDestinationKind=0;u.FoodDestination=-1;if(u.FoodJob==3){u.FoodJob=0;u.Route.Clear();}}
        }
        MigrateWorkerOrders(save);MigrateResearchFlow(save);ValidateExploration(save);
        ValidateReversions(save);if(save.Version>=16)ValidatePopulation(save);MigrateProduction(save);ValidateRecruitSave(save);ValidateTrainingQueue(save);var slots=InitialActors;
        var seen=new HashSet<int>();
        foreach(var u in save.Units){if(!slots.ContainsKey(u.Slot)||!seen.Add(u.Slot)||u.X<0||u.Y<0||u.X>=World.Width*1024||u.Y>=World.Height*1024||u.Route==null||u.Route.Count>65536||u.Route.Any(p=>p<0||p>=65536||(p&255)>=World.Width||(p>>8)>=World.Height))throw new InvalidDataException("Invalid actor state");var original=ExpectedActor(u.Slot,save.Recruitments,save.Reversions);if(u.Type!=original.Type||u.Owner!=ExpectedTradeOwner(u.Slot,save.Special)||(u.Health<0||u.Health>original.Health||save.Version==4&&u.Health!=original.Health)||u.LegacyState!=(FlightHidden(u.Slot,save.Ocean)||TradeHidden(u.Slot,save.Special)?12:save.LaboratoryWorkers?.FirstOrDefault(j=>j.SettlerSlot==u.Slot) is {} assignment?assignment.Stage==1?14:7:original.Type==9&&u.Type==9&&save.Version>=16?(save.Residents!.Values.Any(ids=>ids.Contains(u.Slot))?14:OriginalActorSeeds.GetValueOrDefault(u.Slot)?.LegacyState==14&&!OriginalResidents.Values.Any(ids=>ids.Contains(u.Slot))?14:7):original.LegacyState)||u.AnimationCount!=original.AnimationCount||u.AnimationDelay!=original.AnimationDelay||u.AnimationFrame<0||u.AnimationFrame>=Math.Max(1,u.AnimationCount)||u.Direction<1||u.Direction>16||u.AnimationCounter<0||u.AnimationCounter>=u.AnimationDelay)throw new InvalidDataException($"Unsupported actor fields: slot {u.Slot}, type {u.Type}/{original.Type}, owner {u.Owner}/{original.Owner}, state {u.LegacyState}/{original.LegacyState}, animation {u.AnimationCount}:{u.AnimationDelay}/{original.AnimationCount}:{original.AnimationDelay}, health {u.Health}/{original.Health}");}
        ValidateOpponent(save);ValidateHunger(save);ValidateSpecial(save);ValidateCombatSave(save);ValidateProjectileSave(save);ValidateBuildSave(save);ValidateFoodSave(save);ValidateOverflow(save);ValidateResearchSave(save);ValidateMiningSave(save);
        // Recover premature victories from older remake saves in memory only.
        if(IsSecondMission&&save.Version<=20&&save.MissionChecks>0&&!SavedMissionSatisfied(save)){
            save=save with{MissionChecks=0};Campaign.TransitionOpen=false;
        }
        ValidateEnvironment(save);ValidateMissionSave(save);MissionChecks=save.MissionChecks;World.Mines=save.Mines!;Array.Copy(InitialTiles,World.Tiles,InitialTiles.Length);foreach(var mine in World.Mines.Where(m=>m.Quantity==0))DepleteMine(mine);Research=save.Research!;LaboratoryWorkers=save.LaboratoryWorkers!;ResearchNotice="";Arrows=save.Arrows??new();Constructions=save.Constructions;Recruitments=save.Recruitments;TrainingQueue=save.TrainingQueue??new();World.Buildings.Clear();doorCacheCount=-1;InvalidateSpatialIndex();World.Buildings.AddRange(InitialBuildings.Where(b=>!ReplacedWall(b.Slot)));World.Buildings.AddRange(Constructions.Where(c=>Complete(c)&&!ReplacedWall(c.Id)).Select(c=>new Building(c.Id,c.Type,c.Owner,c.Cell)));World.Units=save.Units;World.Resources=save.Resources;World.Objects=save.Objects;World.Quantities=save.Quantities;EnvironmentState=save.Environment??new();Hunger=save.Hunger??new();Opponent=save.Opponent??new();World.Buildings.RemoveAll(b=>DestroyedBuilding(b.Slot));RefreshDestroyedFootprint();Tick=save.Tick;LastAttack.Clear();RemoveEnemyStartingCart();
    }
    public void SaveTo(string path){Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);string temp=path+".tmp";using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)){byte[] data=System.Text.Encoding.UTF8.GetBytes(Serialize());stream.Write(data);stream.Flush(true);}if(File.Exists(path)){File.Copy(path,path+".bak",true);using var backup=new FileStream(path+".bak",FileMode.Open,FileAccess.ReadWrite,FileShare.None);backup.Flush(true);}File.Move(temp,path,true);}
    public record Save(int Version,string SourceHash,string MapName,long Tick,List<Actor> Units,ushort[] Resources,byte[] Objects,byte[] Quantities,List<Construction> Constructions,List<Recruitment> Recruitments,List<Arrow>? Arrows=null,EngineeringResearch? Research=null,List<LaboratoryWorker>? LaboratoryWorkers=null,List<Mine>? Mines=null,int MissionChecks=0,Dictionary<int,List<int>>? Residents=null,List<ResidentTrip>? Trips=null,List<CultureState>? Cultures=null,List<Actor>? BornActors=null,CampaignProgress? Campaign=null,Dictionary<int,int>? Reversions=null,ExplorationState? Exploration=null,EnvironmentState? Environment=null,TestState? Tests=null,SpecialState? Special=null,HungerState? Hunger=null,OpponentState? Opponent=null,CreatureState? Creatures=null,OceanState? Ocean=null,List<TrainingOrder>? TrainingQueue=null);
}



