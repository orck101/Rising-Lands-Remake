# v44 — seleção somente por ação do jogador

Removida a escolha automática da primeira unidade ao carregar o cenário, carregar um save e reiniciar a partida. A câmera inicial continua centrada na base/unidades do jogador, sem selecionar ninguém.

Retirar colono por duplo clique não chama mais SelectOnly(colonist). O prédio pode continuar exibindo suas informações, mas o personagem retirado não recebe seleção. Tropas recém-formadas já não modificavam a seleção e mantêm esse comportamento. Seleção explícita por clique, caixa, Shift e recuperação manual de grupos preservada.

Verificação: publicação Windows x64 sem avisos; captura inicial com validação de selected==null e lista de selecionados vazia; captura population valida duplo clique, saída real com seleção vazia, seleção explícita posterior para ordenar entrada e conclusão da viagem. Captura inicial inspecionada: sem contorno ou retrato de personagem selecionado. Save permanece v17. Não repetida a bateria de simulação, pois esta alteração fica no estado da interface.

Arquivos alterados: Engine/ResearchGame.cs, PauseMenu.cs, BuildingInteraction.cs e PopulationCapture.cs. Modos de captura de movimento conservam sua seleção explícita de teste, sem afetar o início normal do jogo. Original e saves originais não alterados.
