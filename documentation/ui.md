# Interface

Imagens e sprites de interface disponíveis. Layout, estados dos botões, tooltips e interação original ainda não reconstruídos. HUD do protótipo é uma ferramenta de laboratório.

## Pontos de investigação

| Símbolo original | VA | Offset EXE |
|---|---|---|
| AffInfoBatiment | 0x401040 | 1088 |
| AffInfoAide | 0x40f8aa | 60586 |
| AffInfoSelection | 0x410be4 | 65508 |
| AffZoneMiniMap | 0x417310 | 91920 |
| InterfaceGetEtatMiniMap | 0x42909a | 165018 |
| MenuNewSelect | 0x42c47c | 178300 |
| MenuBefore | 0x42c80e | 179214 |
| AffInfoBob | 0x43f900 | 257280 |
| AffInfoEngin | 0x43fad4 | 257748 |
| AffInfoProposition | 0x46225b | 398939 |
| AffInfoNombre | 0x4622c2 | 399042 |
| AffInfoPlayer | 0x4637f0 | 404464 |
| AffInfoDivers | 0x46382d | 404525 |
| AffInfoMatiere | 0x46384f | 404559 |
| AffInfoPerso | 0x46386f | 404591 |
| AffInfoPersoVeut | 0x4638dc | 404700 |
| AffInfoPersoSeule | 0x463937 | 404791 |
| MenuInit | 0x46814f | 423247 |
| MenuScroll | 0x4691b4 | 427444 |
| MenuGetNbItem | 0x469201 | 427521 |
| MenuSetSelectItem | 0x469208 | 427528 |
| MenuGetItemSelect | 0x469211 | 427537 |
| MenuModifyItem | 0x469218 | 427544 |
| MenuSetTitle | 0x46922c | 427564 |
| MenuAddItem | 0x469232 | 427570 |
| MenuReset | 0x469255 | 427605 |
| MenuSetFonte | 0x469269 | 427625 |
| MenuDisplay | 0x46927b | 427643 |
| AffMenuMultiSession | 0x469650 | 428624 |
| MiniMapReset | 0x469d94 | 430484 |
| MiniMapAlloc | 0x469dfc | 430588 |
| MiniMapFree | 0x469e91 | 430737 |
| MiniMapRecentre | 0x469eaf | 430767 |
| MiniMapSetPos | 0x46a032 | 431154 |
| MiniMapSetLock | 0x46a06b | 431211 |
| MiniMapGetPos | 0x46a08c | 431244 |
| MiniMapGetTaille | 0x46a0a2 | 431266 |
| MiniMapGetCase | 0x46a0b8 | 431288 |
| MiniMapGetCenterSelect | 0x46a107 | 431367 |
| SetMiniMap | 0x46a21c | 431644 |
| AffMiniMap | 0x46a5a0 | 432544 |

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
