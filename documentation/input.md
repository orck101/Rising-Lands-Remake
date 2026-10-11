# Entrada

Entrada do protótipo: WASD/setas câmera; roda zoom; Home centralizar; clique selecionar; direito mover; F5/F9 save/load; PgUp/PgDn mapas; espaço pausa; F11 tela cheia; Esc sair. Esses atalhos não foram apresentados como cópia dos originais.

## Pontos de investigação

| Símbolo original | VA | Offset EXE |
|---|---|---|
| AffInfoSelection | 0x410be4 | 65508 |
| AffBoxSelection | 0x4122d5 | 71381 |
| BatimentGetSelection | 0x41a556 | 104790 |
| BatimentDeselectionner | 0x41b703 | 109315 |
| BatimentSelection | 0x41b713 | 109331 |
| Deselection | 0x429218 | 165400 |
| MouseEstDeplacer | 0x429254 | 165460 |
| KeyboardInit | 0x42c3b3 | 178099 |
| MouseInit | 0x4716d4 | 461524 |
| EnginEstSelectionne | 0x472064 | 463972 |
| EnginGetSelection | 0x472098 | 464024 |
| EnginGetNbSelection | 0x4720be | 464062 |
| EnginSelectionneEstUnique | 0x4720ce | 464078 |
| EnginTypeSelectionne | 0x47231e | 464670 |
| EnginTravailSelectionne | 0x47237f | 464767 |
| DesTroupesSelectionnes | 0x47242d | 464941 |
| SelectionInit | 0x472445 | 464965 |
| RetireSelectionEngin | 0x472466 | 464998 |
| SelectionAddEngin | 0x4724de | 465118 |
| DeselectionEngin | 0x472538 | 465208 |
| DeSelectionGroupe | 0x47257b | 465275 |
| SauveSelectionGroupe | 0x4725e7 | 465383 |
| RestaureSelectionGroupe | 0x472659 | 465497 |
| EnginSelectionnable | 0x474059 | 472153 |
| EnginAddSelection | 0x4740bd | 472253 |
| SelectionEngin | 0x4741cc | 472524 |
| MouseSetZone | 0x4798e0 | 494816 |
| MouseTestZone | 0x47992e | 494894 |
| MouseZone | 0x47993c | 494908 |

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
