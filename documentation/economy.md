# Economia

Custos literais preservados nos catálogos. Fórmulas, consumo, limites e sequência coleta/transporte/depósito não reconstruídos. Não há economia funcional no protótipo.

## Pontos de investigação

| Símbolo original | VA | Offset EXE |
|---|---|---|
| AffCompteurCoutMatiere | 0x4181e0 | 95712 |
| BatimentEvalueCoutReparation | 0x41b1e9 | 108009 |
| GarageCalculCoutReparation | 0x41b266 | 108134 |
| HospiCalculCoutReparation | 0x41b3c7 | 108487 |
| TerrainDeposeMatiere | 0x41bc2a | 110634 |
| DeposeContenu | 0x41f50e | 125198 |
| StockPossible | 0x423076 | 140406 |
| BatimentStockEngin | 0x424ff4 | 148468 |
| InfoSetZoneCout | 0x4292b6 | 165558 |
| SetEnginDepose | 0x4445da | 276954 |
| SetRecolteEngin | 0x444f65 | 279397 |
| RecolteTermine | 0x446c94 | 286868 |
| EnginDeposeSac | 0x456c34 | 352308 |
| MarcheResetAll | 0x460110 | 390416 |
| MarcheMajSympathie | 0x460161 | 390497 |
| B_EtatMarche | 0x46019e | 390558 |
| ChargeMarche | 0x4609b0 | 392624 |
| SauveMarche | 0x460c74 | 393332 |
| ForgetMarche | 0x460f38 | 394040 |
| MarcheReponseQuitte | 0x461079 | 394361 |
| InitZoneMarche | 0x4610fe | 394494 |
| MarcheRestoreZone | 0x46116c | 394604 |
| MarcheEchangePrend | 0x4614b5 | 395445 |
| MarcheEchangeDonne | 0x46172e | 396078 |
| MarcheOrdreEchange | 0x461a7e | 396926 |
| MarcheOrdreSortie | 0x461b79 | 397177 |
| InitDemoMarche | 0x461d61 | 397665 |
| MarcheInit | 0x461d74 | 397684 |
| MarcheRecentre | 0x461d90 | 397712 |
| MarcheSetPos | 0x461f16 | 398102 |
| MarcheSetLock | 0x461f4f | 398159 |
| MarcheGetPos | 0x461f70 | 398192 |
| MarcheGetTaille | 0x461f86 | 398214 |
| MarcheGetCenterSelect | 0x461f9c | 398236 |
| MarcheDonneVeut | 0x46213d | 398653 |
| MarcheDebarasse | 0x462331 | 399153 |
| CompteStockage | 0x4626d4 | 400084 |
| Stockage | 0x462704 | 400132 |
| MarcheDebarasseMiniDonne | 0x462af9 | 401145 |
| MarcheDebarasseMiniVeut | 0x462c1d | 401437 |
| MarcheRemplitTableau | 0x462cf7 | 401655 |
| MarcheRemplitTableauHautBas | 0x462e1c | 401948 |
| MarcheActif | 0x463131 | 402737 |
| InitTableauMarche | 0x4631ab | 402859 |
| MarcheChangePlayerContact | 0x463674 | 404084 |
| MarcheCreerEngin | 0x464916 | 408854 |
| AffCompteurMarche | 0x4656ca | 412362 |
| AffCompteurMarchePreparation | 0x4657a3 | 412579 |
| AffJaugeMarche | 0x465989 | 413065 |
| AffMarcheProposition | 0x465a20 | 413216 |
| ResetMarche | 0x4661e2 | 415202 |
| MarcheQuitte | 0x46626a | 415338 |
| AffMarche | 0x466611 | 416273 |
| EvalueCoutProposition | 0x466a40 | 417344 |
| CalculCompteurMarche | 0x466c65 | 417893 |
| CalculMarcheProposition | 0x466f3c | 418620 |
| CalculMarche | 0x4673eb | 419819 |
| GetRessourceStock | 0x467531 | 420145 |
| InventionGetCout | 0x470b2d | 458541 |
| EnginSelectPeutDeposer | 0x472195 | 464277 |
| SetRecolteEnginSelect | 0x473180 | 468352 |
| SortilegeGetCoutMana | 0x4745d4 | 473556 |
| SortilegeGetCoutApprentissage | 0x4745ee | 473582 |
| MineRecolte | 0x4754d3 | 477395 |
| TerrainDeposeSac | 0x4760d5 | 480469 |
| TerrainRecolte | 0x478114 | 488724 |
| StockeAction | 0x47a62a | 498218 |
| I_CombienCouteUnite | 0x480d3a | 524602 |
| I_CombienCouteBatiment | 0x480d61 | 524641 |
| B_TestStock | 0x480d80 | 524672 |
| V_Recolter | 0x481a33 | 527923 |
| V_DeposerPiece | 0x481c78 | 528504 |
| B_TestMaxRocheDansStock | 0x4859b6 | 544182 |
| iagRessGetStockAlerte | 0x485a8f | 544399 |

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
