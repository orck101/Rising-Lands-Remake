# Pesquisas

81 seções de configuração agrupadas em AGRICULTURE, MILITAIRE, GENIECIVIL e RELIGIEUX foram exportadas sem perda em `../source_remake/Data/technologies.json`, com linha e offset de cada chave. Isso não demonstra81 tecnologias disponíveis: algumas seções podem conter campos reservados ou não acessíveis. Dependências, efeitos e desbloqueios permanecem pendentes.

| Símbolo original | VA | Offset EXE |
|---|---|---|
| RechercheCoordonneVehicule | 0x402738 | 6968 |
| BatimentSetTechnologie | 0x41c80c | 113676 |
| BatimentInitTechnologie | 0x41ce80 | 115328 |
| RechercheDecalageSortie | 0x41d270 | 116336 |
| RechercheEmplacementSortieHangar | 0x41d610 | 117264 |
| RechercheEmplacementSortieTente | 0x41d7ef | 117743 |
| RechercheDecalageProjectile | 0x425198 | 148888 |
| EnginSetTechnologie | 0x441285 | 263813 |
| EnginInitTechnologie | 0x4414fd | 264445 |
| RechercheEmplacementBatiment | 0x45cfe0 | 377824 |
| RechercheHangar | 0x46107a | 394362 |
| RechercheTypeBatiment | 0x4610b6 | 394422 |
| RechercheTroupeHangar | 0x461192 | 394642 |
| RecherchePlaceLibre | 0x46124c | 394828 |
| RechercheEmplacementLibre | 0x4612de | 394974 |
| RechercheTroupeDansHangar | 0x461ae3 | 397027 |
| RechercheTypeEngins | 0x46288b | 400523 |
| BatimentTechnologieReset | 0x46a7dd | 433117 |
| LoadBatimentTechnologie | 0x46a812 | 433170 |
| SauveBatimentTechnologie | 0x46a847 | 433223 |
| EnginTechnologieReset | 0x46a882 | 433282 |
| LoadEnginTechnologie | 0x46a8b7 | 433335 |
| SauveEnginTechnologie | 0x46a8ef | 433391 |
| TechnologieReset | 0x46b3ab | 436139 |
| RechercheGetNbInvention | 0x470444 | 456772 |
| RechercheLoad | 0x4708e9 | 457961 |
| RechercheSave | 0x470999 | 458137 |
| RechercheSetBranche | 0x470a27 | 458279 |
| RechercheReset | 0x470a36 | 458294 |
| RechercheInit | 0x470a8c | 458380 |
| RechercheAddScientifique | 0x470ae7 | 458471 |
| RechercheProgress | 0x470af5 | 458485 |
| RechercheEffetEnCours | 0x470d5a | 459098 |
| RechercheCodeEnCours | 0x470dc8 | 459208 |
| RechercheTypeEnCours | 0x470e0d | 459277 |
| RechercheCalcul | 0x47114c | 460108 |
| RechercheEndTechnologie | 0x47138d | 460685 |
| RechercheStore | 0x4713e0 | 460768 |
| RechercheCalculNew | 0x4713ff | 460799 |
| RechercheCheat | 0x4715ee | 461294 |
| RechercheGetEtat | 0x471692 | 461458 |
| RechercheGetBranche | 0x4716a1 | 461473 |
| RechercheGetNiveau | 0x4716b2 | 461490 |
| LoadSortilegeTechnologie | 0x474500 | 473344 |
| SauveSortilegeTechnologie | 0x47454f | 473423 |
| SortilegeResetTechnologie | 0x4748e2 | 474338 |
| SortilegeSetTechnologie | 0x474928 | 474408 |
| SortilegeInitTechnologie | 0x474985 | 474501 |
| IaRechercheAjuste | 0x47f450 | 518224 |

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
