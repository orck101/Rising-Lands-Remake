# Construções

CONFIRMADO: IDs indexam a tabela estática original; nomes internos resolvem seções de RISING.INI. NAME é o texto literal configurado. Custos, tempos e matrizes foram preservados com linha/offset em JSON, sem atribuir unidades físicas ou fórmula não comprovadas.

| ID | Nome interno | NAME configurado | VA do registro |
|---:|---|---|---|
| 0 | PHARE | Lighthouse | 0x495d8c |
| 1 | CASERNE | Barracks | 0x495dc0 |
| 2 | SANCTUAIRE | Sanctuary | 0x495df4 |
| 3 | CAVE | Storehouse | 0x495e28 |
| 4 | CULTURE | Culture | 0x495e5c |
| 5 | ATELIER | Workshop | 0x495e90 |
| 6 | BIBLIOTHEQUE | Library | 0x495ec4 |
| 7 | LABORATOIRE | Laboratory | 0x495ef8 |
| 8 | HANGAR | Exchange | 0x495f2c |
| 9 | MUR | Wall | 0x495f60 |
| 10 | TOURCATAPULTE | Tower catapult | 0x495f94 |
| 11 | TOURCANON | Tower canon | 0x495fc8 |
| 12 | HERSE | Portcullis | 0x495ffc |
| 13 | CIRQUE | Circus | 0x496030 |
| 14 | GARAGE | Garage | 0x496064 |
| 15 | MONASTERE | Temple | 0x496098 |
| 16 | MARCHE | Market | 0x4960cc |
| 17 | CRISTAL | Crystal | 0x496100 |
| 18 | HELIOPORT | Heliport | 0x496134 |
| 19 | HOSPI | Hospital | 0x496168 |
| 20 | MONASTEREFUSION | Fusion Temple | 0x49619c |
| 21 | TENTE | Campaign tent | 0x4961d0 |

## Campos de instância

| Campo | Offset memória | Offset payload ETA | Getter |
|---|---:|---:|---|
| owner | 4 | 4 | 0x41a12c |
| type | 8 | 8 | 0x41a594 |
| anchor_cell | 2 | 2 | 0x41af82 |

Offsets de payload não incluem o prefixo slot u16. Unidades saltam512 bytes no carregamento após140; portanto HP em memória+154 vem do payload+666. Stride de memória276. Getters e loader fundamentam essa relação; animações, ordens e todos os outros campos ainda pendentes.
