# Estados de unidades

Campo state u8 em+2, getter0x440eb5. Missão01 contém estados7 e14. O significado e todas as transições precisam ser confirmados; não rotular7 como idle apenas por observação dos dados.

## Pontos de investigação

| Símbolo original | VA | Offset EXE |
|---|---|---|
| EnginGetEtat | 0x440eb5 | 262837 |
| EnginSetEtat | 0x441203 | 263683 |
| AnimArretEngin | 0x4478ee | 290030 |
| AnimDyingEngin | 0x44792f | 290095 |
| AnimAttaqueTermineEngin | 0x447977 | 290167 |
| AnimAttaqueEngin | 0x4479c4 | 290244 |
| AnimRechargeEngin | 0x447d95 | 291221 |
| AnimWorkEngin | 0x447f58 | 291672 |
| AnimDeplaceEngin | 0x448088 | 291976 |

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
