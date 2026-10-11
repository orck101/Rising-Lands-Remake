using RisingLands;

static string? Option(string[] a,string key){int i=Array.IndexOf(a,key);return i>=0 && i+1<a.Length?a[i+1]:null;}
static string FindAssets()=>PackageLayout.FindAssets();
AppDomain.CurrentDomain.UnhandledException+=(sender,eventArgs)=>{if(eventArgs.ExceptionObject is Exception error)CrashDiagnostics.Record(error,"ExceÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o nÃƒÆ’Ã‚Â£o tratada");};
try {
PackageLayout.PrepareNativeLibraries();
string root=Path.GetFullPath(Option(args,"--assets")??FindAssets());
CrashDiagnostics.Initialize(root);
string map=Option(args,"--map")??"MONDE01.ETA";
if(args.Contains("--test-worker-order-recovery")){WorkerOrderRecoveryTests.Run(root,Option(args,"--snapshot-dir")!,Option(args,"--output")!);return;}
if(args.Contains("--test-training-queue")){TrainingQueueTests.Run(root,Option(args,"--output")!);return;}
if(args.Contains("--test-diplomacy-research-v126")){DiplomacyResearchV126Tests.Run(root,Option(args,"--save")!,Option(args,"--output")!);return;}
if(args.Contains("--test-nearest-meal")){NearestMealTests.Run(root,Option(args,"--save")!,Option(args,"--output")!);return;}
if(args.Contains("--test-minimap-colors")){MinimapColorTests.Run(root,Option(args,"--output")!);return;}
if(args.Contains("--test-recovery-v118")){RecoveryV118Tests.Run(root,Option(args,"--save")!,Option(args,"--output")!);return;}
if(args.Contains("--test-shared-vision")){SharedVisionTests.Run(root,Option(args,"--output")!);return;}
if(args.Contains("--test-green-assault")){GreenAssaultTests.Run(root,Option(args,"--output")!);return;}
if(args.Contains("--test-base-defense")){BaseDefenseTests.Run(root,Option(args,"--output")!);return;}
if(args.Contains("--test-combat-diplomacy")){CombatDiplomacyTests.Run(root,Option(args,"--output")!);return;}
if(args.Contains("--test-trade-v113")){TradeV113Tests.Run(root,Option(args,"--output")!);return;}
if(args.Contains("--test-hunger-continues")){HungerContinuesTests.Run(root,Option(args,"--save")!,Option(args,"--output")!);return;}
if(args.Contains("--test-meal-radii")){MealRadiusTests.Run(root,Option(args,"--save")!,Option(args,"--output")!);return;}
if(args.Contains("--test-local-hunger-development")){HungerContinuesTests.Run(root,Option(args,"--save")!,Option(args,"--output")!);return;}
if(args.Contains("--replay-local-hunger-development")){LocalHungerDevelopmentTests.Replay(root,Option(args,"--save")!,Option(args,"--output")!);return;}
if(args.Contains("--test-economic-opening")){EconomicOpeningTests.Run(root,Option(args,"--output")??"economic-opening.json");return;}
if(args.Contains("--test-mission-five-ending")){MissionFiveEndingTests.Run(root,Option(args,"--output")??"mission-five-ending.json");return;}
if(args.Contains("--benchmark-mission-seven")){MissionSevenTests.Benchmark(root,Option(args,"--output")!);return;}
if(args.Contains("--test-balloon-v124")){BalloonTransportTests.Run(root,Option(args,"--output")??"balloon-v124.json");return;}
if(args.Contains("--test-mission-nine-progression")){MissionNineProgressionTests.Run(root,Option(args,"--output")??"mission-nine-progression.json");return;}
if(args.Contains("--test-mission-nine")){MissionNineTests.Run(root,Option(args,"--output")??"mission-nine.json");return;}
if(args.Contains("--test-mission-eight")){MissionEightTests.Run(root,Option(args,"--output")??"mission-eight.json");return;}
if(args.Contains("--test-mission-seven")){MissionSevenTests.Run(root,Option(args,"--output")??"mission-seven.json");return;}
if(args.Contains("--test-mission-six")){MissionSixTests.Run(root,Option(args,"--output")??"mission-six.json");return;}
if(args.Contains("--benchmark-mission-six")){MissionSixTests.Benchmark(root,Option(args,"--output")??"mission-six-performance.json");return;}
if(args.Contains("--test-ramps-v106")){RampNavigationTests.Run(root);return;}
if(args.Contains("--test-planning-v105")){OpponentPlanningTests.Run(root,Option(args,"--output")??"planning.json",Option(args,"--save"));return;}
if(args.Contains("--test-planning-recovery-v105")){OpponentPlanningTests.Recovery(root,Option(args,"--save")??throw new ArgumentException("Copied save required"),Option(args,"--output")??"recovery.json");return;}
if(args.Contains("--test-enemy-personalities")){EnemyPersonalityTests.Run(root);return;}
if(args.Contains("--cases-mission-five")){MissionFiveCases.Run(root,Option(args,"--output")??throw new ArgumentException("Output required"));return;}
if(args.Contains("--script-mission-five")){MissionFiveScript.Run(root,Option(args,"--output")??throw new ArgumentException("Output required"));return;}
if(args.Contains("--audit-mission-five")){MissionFiveAudit.Run(root,Option(args,"--output")??throw new ArgumentException("Output required"),int.Parse(Option(args,"--audit-minutes")??"10"),Option(args,"--save"));return;}
if(args.Contains("--probe-save-load")){SaveLoadProbe.Run(root,Option(args,"--save")??throw new ArgumentException("Copied save required"));return;}
if(args.Contains("--probe-extra-research")){ExtraResearchProbe.Run(root,Option(args,"--save")??throw new ArgumentException("Copied save required"));return;}
if(args.Contains("--analyze-military-slot")){MilitarySlotAnalysis.Run(root,Option(args,"--save")??throw new ArgumentException("Provide a copied save"));return;}
if(args.Contains("--test-mission-four-v102")){MissionFourV102Tests.Run(root);return;}
if(args.Contains("--test-mission-four-replay-v102")){MissionFourV102Tests.Replay(root,Option(args,"--save")??throw new ArgumentException("Provide a copied snapshot"));return;}
if(args.Contains("--test-warehouse-v101")){WarehouseV101Tests.Run(root);return;}
if(args.Contains("--test-slot-five-v100")){SlotFiveV100Tests.Run(root,Option(args,"--save")??throw new ArgumentException("Provide a copied save with --save"));return;}
if(args.Contains("--test-trade-v100")){TradeV100Tests.Run(root);return;}
if(args.Contains("--test-trade-v99")){TradeV99Tests.Run(root);return;}
if(args.Contains("--test-policy-v98")){PolicyV98Tests.Run(root);return;}
if(args.Contains("--test-trade-v97")){TradeV97Tests.Run(root);return;}
if(args.Contains("--test-crash-report-v95")){CrashV95Tests.ReportTest(root);return;}
if(args.Contains("--test-strategy-v96")){StrategyV96Tests.Run(root);return;}
if(args.Contains("--test-crashes-v95")){CrashV95Tests.Run(root);return;}
if(args.Contains("--test-rule-v77")){OriginalRuleMachineTests.Run();return;}
if(args.Contains("--test-groups-v78")){OriginalOpponentGroupsTests.Run();return;}
if(args.Contains("--test-brain-v78")){OriginalBrainV78Tests.Run(root);return;}
if(args.Contains("--test-growth-v80")){GrowthV80Tests.Run(root);return;}
if(args.Contains("--test-audio-v81")){AudioV81Tests.Run(root);return;}
if(args.Contains("--test-production-v84")){ProductionV84Tests.Run(root);return;}
if(args.Contains("--test-balance-v85")){BalanceV85Tests.Run(root);return;}
if(args.Contains("--test-mission-three-v86")){MissionThreeV86Tests.Run(root);return;}
if(args.Contains("--test-exploration-v93")){ExplorationV93Tests.Run(root);return;}
if(args.Contains("--test-improvements-v91")){ImprovementsV91Tests.Run(root);return;}
if(args.Contains("--test-trade-opening-v90")){TradeOpeningV90Tests.Run(root);return;}
if(args.Contains("--test-monitor-v89")){MonitorV89Tests.Run(root,Option(args,"--output")??"monitor-fixture");return;}
if(args.Contains("--test-opening-v88")){OpeningV88Tests.Run(root);return;}
if(args.Contains("--test-combat-v88")){CombatV88Tests.Run(root);return;}
if(args.Contains("--test-enemy-production-v87")){EnemyProductionV87Tests.Run(root);return;}
if(args.Contains("--probe-growth-long")){OpponentGrowthProbe.Run(root,24);return;}
if(args.Contains("--probe-growth")){OpponentGrowthProbe.Run(root);return;}
if(args.Contains("--test-opening-v79")){OpeningV79Tests.Run(root);return;}
if(args.Contains("--test-defense-v78")){DefenseV78Tests.Run(root);return;}
if(args.Contains("--test-v77")){OpponentV77Tests.Run(root);return;}
if(args.Contains("--test-v76")){OpponentV76Tests.Run(root);return;}
if(args.Contains("--test-v75")){try{OpponentV75Tests.Run(root);}catch(Exception e){Console.Error.WriteLine(e);Environment.ExitCode=1;}return;}
if(Path.GetFileName(map)!=map)throw new ArgumentException("Map must be a filename");
if(args.Contains("--test-mission-one")){MissionOneTests.Run(root);return;}
if(args.Contains("--test-v73")){ImprovementsV73Tests.Run(root);return;}
if(args.Contains("--test-v72")){ImprovementsV72Tests.Run(root);return;}
if(args.Contains("--test-v71")){ImprovementsV71Tests.Run(root);return;}
if(args.Contains("--test-v67")){ImprovementsV67Tests.Run(root);return;}
if(args.Contains("--test-audio-v68")){Environment.ExitCode=AudioV68Tests.Run(root)?0:1;return;}
if(args.Contains("--test-audio-v69")){Environment.ExitCode=AudioV69Tests.Run(root)?0:1;return;}
if(args.Contains("--test-gameplay-v70")){Environment.ExitCode=GameplayV70Tests.Run(root)?0:1;return;}
if(args.Contains("--benchmark-v67")){PerformanceV67.Run(root,Option(args,"--output")??"performance-v67.json");return;}
if(args.Contains("--self-test")){SelfTests.Run(root);return;}
if(args.Contains("--test-collection-v50")){CollectionV50Tests.Run(root);return;}
if(args.Contains("--test-special-v66")){SpecialV66Tests.Run(root);return;}
if(args.Contains("--test-production-v65")){ProductionV65Tests.Run(root);return;}
if(args.Contains("--test-range-outline")){RangeOutlineTests.Run();return;}
if(args.Contains("--test-attack-range")){AttackRangeTests.Run(root);return;}
if(args.Contains("--test-warehouse-accounting")){WarehouseAccountingTests.Run(root);return;}
if(args.Contains("--test-viewport-zoom")){ViewportZoomTests.Run(root);return;}
if(args.Contains("--test-mission-two")){MissionTwoTests.Run(root);return;}
if(args.Contains("--check-player-saves")){ImprovementsV57Tests.InspectPlayerSaves(root);return;}
if(args.Contains("--test-research-v58")){ResearchV58Tests.Run(root);return;}
if(args.Contains("--test-v57")){ImprovementsV57Tests.Run(root);return;}
if(args.Contains("--test-discovery-v60")){DiscoveryV60Tests.Run(root);return;}
if(args.Contains("--test-crop-v61")){CropV61Tests.Run(root);return;}
if(args.Contains("--test-menu-v63")){TestMenuV63Tests.Run(root);return;}
if(args.Contains("--test-environment-v62")){EnvironmentV62Tests.Run(root);return;}
using var game=new ResearchGame(root,map,Option(args,"--capture"),args.Contains("--capture-walk"),args.Contains("--capture-food"),args.Contains("--capture-build"),args.Contains("--capture-combat"),args.Contains("--capture-archer"),args.Contains("--capture-info"),Option(args,"--capture-menu"));game.Run();

} catch(Exception e){CrashDiagnostics.Record(e,"ExecuÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â£o principal");Console.Error.WriteLine(e);Environment.ExitCode=1;}




