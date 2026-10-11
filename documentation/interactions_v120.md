# Correção das cores do minimapa — v120

2026-10-09. CONFIRMADO no código anterior: unidades do jogador apareciam verdes e todos os inimigos vermelhos; construções do jogador verdes e do verde azuis; obras substituíam a cor do dono por Wheat.

Correção compartilhada: proprietário 0 azul (CornflowerBlue), 1 vermelho (IndianRed), 2 amarelo (Yellow), 3 verde (LightGreen). Tropas, construções, obras, muros e portões usam MinimapColors.ForOwner. Prédios lembrados usam metade da intensidade RGB e alpha255, conservando identidade sem transparência. Relação diplomática, seleção e progresso não substituem a cor. A visibilidade não foi alterada.

Arquivos: Engine/MinimapColors.cs, Engine/NativeActionBar.cs, Engine/WallConstruction.cs; Tests/MinimapColorTests.cs e comando em Program.cs. Backup antes-minimapa-v120-20261009. Não modifica cores dos sprites, barras de vida, dados ou saves. A derrota temporariamente desativada da missão 5 continua.

Verificação: build/publish sem erro, 11 checks de cor (incluindo concordância com paleta dos sprites), 36 checks da missão 5 no pacote publicado. Captura real do renderer com mapa revelado apenas no ensaio isolado, conferida visualmente; não é uma partida manual. Evidências experiments/minimapa-v120: diagnosis-before.json, colors.json, mission5.png, mission-five.json, saves-preserved.json. 21 arquivos de saves preservados.

Pacote novo releases/interactions-v120; atalhos das missões 1–6 atualizados, anteriores preservados. Partidas já abertas precisam reabrir pela versão nova; seus saves podem ser carregados normalmente. Formato41/nomes dos slotsv33 mantidos.
