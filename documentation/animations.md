# Animações

Frames CMP extraídos com IDs e dimensões. Sequências/direções/timing por tipo ainda pendentes. O protótipo usa TROOPS frame0 para representar todas as unidades, com aviso visível; não é associação fiel.

## Pontos de investigação

| Símbolo original | VA | Offset EXE |
|---|---|---|
| EnginGetSpriteMort | 0x415496 | 84118 |
| AnimTuyauCave | 0x420d7f | 131455 |
| AnimPorteBatiment | 0x420dda | 131546 |
| AnimArretEngin | 0x4478ee | 290030 |
| AnimDyingEngin | 0x44792f | 290095 |
| AnimAttaqueTermineEngin | 0x447977 | 290167 |
| AnimAttaqueEngin | 0x4479c4 | 290244 |
| AnimHelipede | 0x447acf | 290511 |
| AnimBallonStatic | 0x447b2f | 290607 |
| AnimRechargeEngin | 0x447d95 | 291221 |
| AnimConstruireBatiment | 0x447e12 | 291346 |
| AnimWorkEngin | 0x447f58 | 291672 |
| AnimDeplaceEngin | 0x448088 | 291976 |

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
