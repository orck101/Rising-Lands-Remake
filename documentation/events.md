# Eventos e triggers

EVENTS.INI tem168 pares extraídos, preservando nomes/linhas/offsets. Objetivos ETA contêm4×100 entradas u16+u8. Vídeos decodificados, mas condições, flags e temporização ainda não reconstruídos.

## Pontos de investigação

| Símbolo original | VA | Offset EXE |
|---|---|---|
| InitFinMission | 0x458ab2 | 360114 |
| LoadFinMission | 0x458ad1 | 360145 |
| FinMissionLoad | 0x458b3e | 360254 |
| FinMissionSave | 0x458b8c | 360332 |
| ObjectifReset | 0x47bfec | 504812 |
| PlayerGetObjectif | 0x47c02a | 504874 |
| GetObjectifCase | 0x47c071 | 504945 |
| LoadObjectif | 0x47c094 | 504980 |
| ObjectifLoadText | 0x47c147 | 505159 |
| ObjectifGetLigneText | 0x47c349 | 505673 |
| AffObjectif | 0x47c363 | 505699 |
| ObjectifTextScroll | 0x47c45e | 505950 |
| iabPlusCible_ChangeDeCibleOuObjectifRealise | 0x48984e | 560206 |

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
