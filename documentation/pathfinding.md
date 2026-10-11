# Caminhos

Stride256 confirmado; CheminCalcul em0x42f2b8. Algoritmo, prioridades e desempates desconhecidos. O protótipo usa BFS de quatro vizinhos com restrições de terreno simplificadas, explicitamente experimental.

## Pontos de investigação

| Símbolo original | VA | Offset EXE |
|---|---|---|
| Chemin_Accessible | 0x42d59c | 182684 |
| OptimiseChemin | 0x42e934 | 187700 |
| Chemin_Remap | 0x42eec0 | 189120 |
| Chemin_Open | 0x42f285 | 190085 |
| Chemin_sauvedebug | 0x42f2a3 | 190115 |
| Chemin_Close | 0x42f2a4 | 190116 |
| CheminCalcul | 0x42f2b8 | 190136 |
| Chemin_Verouille | 0x43b3af | 239535 |
| Chemin_ModifieZone | 0x43e1dc | 251356 |
| Chemin_Charge | 0x43e347 | 251719 |
| Chemin_Sauve | 0x43e433 | 251955 |

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
