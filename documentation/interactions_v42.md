# v42 — saídas, entrada em grupo, armazém e câmera

Versão publicada: `releases/interactions-v42/Jogar-Primeira-Missao.cmd`.

## Comportamentos

1. Saídas de tropas treinadas, colonos retirados por duplo clique e moradores encaminhados para pesquisa usam a frente da construção: a linha imediatamente ao sul de sua área no mapa, começando pelos lugares centrais livres. Respeitam terreno, construções e células ocupadas por personagens visíveis. Não teletransportam personagens para trás quando a frente está bloqueada: retirada é recusada sem modificar o morador; formação concluída espera dentro até liberar uma saída. A orientação e posição são decisões baseadas na solicitação e na projeção atual do remake, sem reivindicar reconstrução do algoritmo binário original de saída.

2. Um clique no prédio encaminha todos os colonos selecionados elegíveis. Cada pedido reserva uma vaga antes do próximo pedido, conta moradores físicos, cientistas e viagens já reservadas. Cada personagem conserva sua própria rota e só passa a contar dentro após chegar. Quando não cabem todos, os excedentes permanecem fora e podem receber uma nova ordem; não há fila automática de entrada. O cursor considera se algum colono selecionado pode entrar. A ordem de entrada tem prioridade sobre a retirada por duplo clique enquanto houver colonos visíveis selecionados.

3. Retirados os três botões de alimento, rocha e engrenagens das opções do armazém. Mantidos a ação informativa do armazém, seu retrato, o indicador original de colonos e a informação de estoque, ocupação, capacidade, espaço livre, número de armazéns, ativos/inativos e situação geral. Os recursos no painel superior e a retirada usando Ctrl continuam funcionando.

4. Mouse em uma faixa de 12 pixels junto às bordas do cenário move a câmera nas quatro direções; cantos combinam os dois eixos. WASD e setas continuam. Mesma velocidade de câmera de teclado, ajustada ao zoom, independente do multiplicador de velocidade da simulação. Cursor sobre o painel inferior, fora da janela ou janela sem foco não dispara o movimento. Menus, pesquisa e tela de vitória usam suas interrupções existentes; limites do mapa e minimapa preservados. Não foi medida a velocidade exata da câmera do original.

## Causas localizadas

A saída anterior reutilizava o contorno inteiro `WorkCells`, começando pela borda superior. Formação, retirada e viagem de pesquisa agora compartilham a definição da frente em `Engine/BuildingExits.cs`.

O clique anterior testava apenas `selected`, mesmo com outros IDs em `selectedSlots`. Agora processa `SelectedUnits`, com validação de capacidade em cada entrada, antes do gesto de duplo clique. O armazém tinha quatro ações em `NativeActions`; agora mantém somente a ação informativa. A câmera recebia apenas teclado; o deslocamento considera também posição do cursor em relação à área do cenário.

## Verificação

- Primeira missão: **514 PASS**, incluindo 25 verificações adicionais de bordas, cantos, janela inativa, frente livre/ocupada, posições diferentes, reserva de duas vagas para um grupo de três, chegada, save/load, frente bloqueada e formação aguardando liberação.
- Regressão geral: **351 PASS**, incluindo recrutamento, economia, retirada pelo Ctrl, pesquisa e determinismo.
- Capturas executadas e inspecionadas: `reports/v42-front-exit.png`, `v42-group-entry.png`, `v42-warehouse-clean.png`. O modo de captura valida o próprio clique do prédio para três colonos e o conteúdo do menu do armazém.
- Publicação Windows x64 concluída; save do remake permanece v17.

Testes de bordas exercitam a regra usada pela câmera; não houve automação de movimento físico do mouse dentro da janela do jogo. A sensação de velocidade deve ser conferida em uso pelo jogador. Validação focada na primeira missão; não houve uma bateria visual nova em toda a campanha. Permanecem os limites de fidelidade e pesquisas avançadas documentados na v41.

## Preservação e registro

Originais de `D:\RISING` e saves originais não modificados. Mudanças no remake: `BuildingExits.cs`, `BuildingPopulation.cs`, `RecruitmentSimulation.cs`, `ResearchSimulation.cs`, `BuildingInteraction.cs`, `WorkerResourceInput.cs`, `NativeActionBar.cs`, `EdgeCamera.cs`, `ResearchGame.cs`; testes em `Tests/InteractionV42Tests.cs` e chamada em `MissionOneTests.cs`; captura em `InteractionV42Capture.cs`. Acrescentados os relatórios, logs, imagens e publicação v42. Nenhuma mudança no formato do save.
