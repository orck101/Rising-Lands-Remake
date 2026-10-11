# Unidades

CONFIRMADO: IDs indexam a tabela estática original; nomes internos resolvem seções de RISING.INI. NAME é o texto literal configurado. Custos, tempos e matrizes foram preservados com linha/offset em JSON, sem atribuir unidades físicas ou fórmula não comprovadas.

| ID | Nome interno | NAME configurado | VA do registro |
|---:|---|---|---|
| 0 | SOLDAT | Swordsman | 0x4965ae |
| 1 | GARDIEN DU FEU | Scorcher | 0x4965e0 |
| 2 | CAPITAINE | Captain | 0x496612 |
| 3 | OURS DE COMBAT | Killer | 0x496644 |
| 4 | PRETRE | Druid | 0x496676 |
| 5 | FERMIER | Farmer | 0x4966a8 |
| 6 | MECANO | Mechanic | 0x4966da |
| 7 | BATISSEUR | Builder | 0x49670c |
| 8 | VOLEUR | Archer | 0x49673e |
| 9 | COLON | Settler | 0x496770 |
| 10 | MESSAGER | Messenger | 0x4967a2 |
| 11 | SPYDER | Speeder | 0x4967d4 |
| 12 | BUNKER | Boomer | 0x496806 |
| 13 | RECOLTEUSE | Reaper | 0x496838 |
| 14 | PIEGEUR | Bomber | 0x49686a |
| 15 | SUN | Hellfire | 0x49689c |
| 16 | HELIPEDE | Heliped | 0x4968ce |
| 17 | ZEEPELIN | Balloon | 0x496900 |
| 18 | ARAIGNEE | ARAIGNEE | 0x496932 |
| 19 | FOURMI | FOURMI | 0x496964 |
| 20 | MASTAAR | Basher | 0x496996 |
| 21 | LEZARD | Raptor | 0x4969c8 |
| 22 | SANGLIER | Hurler | 0x4969fa |
| 23 | FOOTMAN | Bukka | 0x496a2c |
| 24 | TORTUE | Snapper | 0x496a5e |
| 25 | RHINO | Stomper | 0x496a90 |

## Campos de instância

| Campo | Offset memória | Offset payload ETA | Getter |
|---|---:|---:|---|
| owner | 0 | 0 | 0x4420e9 |
| type | 1 | 1 | 0x4420c9 |
| state | 2 | 2 | 0x440eb5 |
| cell | 124 | 124 | 0x4411e2 |
| health | 154 | 666 | 0x45718a |

Offsets de payload não incluem o prefixo slot u16. Unidades saltam512 bytes no carregamento após140; portanto HP em memória+154 vem do payload+666. Stride de memória276. Getters e loader fundamentam essa relação; animações, ordens e todos os outros campos ainda pendentes.
