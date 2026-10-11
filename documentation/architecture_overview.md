# Arquitetura inicial

A evidência indica um executável x86 com subsistemas ligados estaticamente e chamadas DirectDraw/DirectSound/DirectPlay. Tabelas indexadas por slots de 276 bytes são usadas tanto para unidades como construções, com bases distintas; formatos serializados não equivalem ao layout em memória.

| Símbolo original | VA | Offset EXE |
|---|---|---|
| CalculGame | 0x42c900 | 179456 |
| CheminCalcul | 0x42f2b8 | 190136 |
| FileRead | 0x458461 | 358497 |
| FileWrite | 0x458476 | 358518 |
| GameLoad | 0x46b721 | 437025 |
| GameSave | 0x46bbd9 | 438233 |
| MissionLoad | 0x46c163 | 439651 |
| LoadPlayer | 0x46dcec | 446700 |
| RechercheLoad | 0x4708e9 | 457961 |
| IaLoad | 0x47f534 | 518452 |

A engine moderna separa `Importers/LegacyAssets.cs`, `Engine/Simulation.cs` e `Engine/ResearchGame.cs`. A simulação usa coordenadas inteiras e tick fixo de laboratório; apresentação e persistência não escrevem em assets originais.

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
