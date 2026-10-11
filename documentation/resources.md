# Recursos

ETA armazena12 u16 em327684, quatro jogadores para cada recurso, ordem de chamadas SetMatiere IDs1,2,0. Chaves COUTPATATE/COUTROCHE/COUTPIECE aparecem em INI; ligação definitiva nome↔ID depende de consumidores.

## Pontos de investigação

| Símbolo original | VA | Offset EXE |
|---|---|---|
| AffZoneMatiere | 0x416fd9 | 91097 |
| AffCompteurMatiere | 0x418040 | 95296 |
| AffCompteurCoutMatiere | 0x4181e0 | 95712 |
| BatimentRecalculRessource | 0x41a068 | 103528 |
| BatimentGetRessource | 0x41ab6e | 106350 |
| TerrainDeposeMatiere | 0x41bc2a | 110634 |
| BatimentAjouteMatiere | 0x4234a7 | 141479 |
| BatimentEnleveMatiere | 0x423655 | 141909 |
| EnginGetRessource | 0x441176 | 263542 |
| EnginRentreMatiere | 0x456cd9 | 352473 |
| EnginApporteMatiere | 0x456d35 | 352565 |
| BatimentRessourceNecessaire | 0x45d6f7 | 379639 |
| TroupeRessourceNecessaire | 0x45d773 | 379763 |
| MatiereaSortir | 0x4626a0 | 400032 |
| GetNbRessource | 0x4628ec | 400620 |
| AffInfoMatiere | 0x46384f | 404559 |
| MatiereEstDemander | 0x464a36 | 409142 |
| AffRessource | 0x4653fd | 411645 |
| AffRessourceContact | 0x46555c | 411996 |
| VideMatiere | 0x46627f | 415359 |
| MesMatiere | 0x46673e | 416574 |
| MesMatiereContact | 0x46689d | 416925 |
| GetRessourceRMI | 0x4674d4 | 420052 |
| GetRessource | 0x467504 | 420100 |
| GetRessourceStock | 0x467531 | 420145 |
| TestRessource | 0x467579 | 420217 |
| CalculMatiere | 0x4675ac | 420268 |
| AjouteMatiere | 0x467786 | 420742 |
| EnleveMatiere | 0x4677c0 | 420800 |
| SetMatiere | 0x467830 | 420912 |
| LoadRessource | 0x46a92a | 433450 |
| LoadGameRessource | 0x46a9b5 | 433589 |
| SauveRessource | 0x46a9c7 | 433607 |
| TerrainRetireMatiere | 0x47658e | 481678 |
| iafRessourceSave | 0x480cf0 | 524528 |
| iafRessourceLoad | 0x480d02 | 524546 |
| iafRessourceReset | 0x480d14 | 524564 |
| iafRessourceResetMine | 0x480d27 | 524583 |
| B_TestRessource | 0x480dc8 | 524744 |
| V_AttendreRessource | 0x4857d8 | 543704 |

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
