# Spawn

Entradas ETA count+size em LoadEntrerMonstre0x47a8c0; campo u16+4 é multiplicado por30 e campo+8 zerado. Outras semânticas não confirmadas. Não gerar uma cronologia em segundos a partir dessa multiplicação.

## Pontos de investigação

| Símbolo original | VA | Offset EXE |
|---|---|---|
| CreerEngin | 0x457e6c | 356972 |
| MarcheCreerEngin | 0x464916 | 408854 |
| InitEntrerMonstre | 0x47a291 | 497297 |
| LoadEntrerMonstre | 0x47a8c0 | 498880 |
| InitMissionMonstre | 0x47ac20 | 499744 |

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
