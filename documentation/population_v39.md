# Ocupação, cultivo e controles — v39

Entrega: releases/population-v39/Jogar-Primeira-Missao.cmd.

## Como usar

Dois cliques rápidos em uma construção habitada retiram um colono disponível. Selecione o colono e clique numa construção: ele caminha até ela e entra. Somente a chegada ativa a construção. Obras concluídas não ganham colonos automaticamente. Colono reservado para transformação em tropa não pode ser retirado no meio da produção.

Armazém vazio fica cinza, não oferece capacidade e não permite retirar recursos. Ao retirar seu último colono, entregadores preservam a carga e aguardam. Reocupar libera as entregas novamente. Armazéns compartilham o estoque global já existente; não foram criados inventários individuais.

Trabalhador selecionado → manter Ctrl → clicar no armazém ativo → clicar no chão, mantendo Ctrl. Comida para fazendeiro, pedra para construtor e engrenagens para engenheiro. Desconto no momento da retirada efetiva. Soltar Ctrl entre os cliques cancela a escolha sem trocar a ordem anterior.

Cultivo recebe comida pelo mesmo fluxo Ctrl em dois cliques. Também precisa de um colono dentro para produzir. A comida armazenada no cultivo não aumenta o estoque global. Regra provisória implementada:10 alimentos e300 passos de simulação por novo colono, com limite5 residentes e100 alimentos no cultivo. São parâmetros de protótipo, NÃO números confirmados do Rising Lands; a cadência é afetada pelo slider1x..5x. A capacidade5 é do LOGEMENT original do cultivo; capacidade2 padrão de outras construções precisa de auditoria individual fora da primeira missão. Novos colonos podem sair por duplo clique e entrar no santuário/quartel para formação.

Produção de tropas agora usa o colono que o jogador colocou no quartel; foi removido o transporte automático oculto a partir do santuário. Retratos de produção cinza quando faltam recursos, colono livre ou a construção está ocupada produzindo. Prédios sem residentes usam textura cinza real, com alpha preservado. O botão Stop foi retirado; comando existente de parar pelo teclado permanece.

Construtor selecionado sobre obra inacabada mostra retrato do construtor e indicação Construir no cursor. Hit test é compartilhado com a ação de ajudar a obra, incluindo desenho além do retângulo do terreno. Gráfico do indicador e conversão de cinza são adaptações, não extração confirmada do cursor/paleta nativos.

## Evidências e validação

Comportamento autorizado pela descrição do usuário. Leitura estática de BatimentRetireColon0x41de80 confirma busca por personagem tipo9 na bolsa do prédio. O retorno75 em I_DonneSeuilPatateProdColon0x4857fa é um limiar de IA; não foi tratado como custo de geração, cujo valor original permanece desconhecido.

Suites completas da primeira missão e regressão aprovadas. Novos testes percorrem retirada, ocupação, armazém ativo/inativo, transferência real para o chão, entrega no cultivo, nascimento, movimentação e formação de um colono gerado. Serialização idêntica após restauração de população/produção. Testes históricos passaram a ocupar explicitamente armazéns e quartéis antes de usar sua produção; as expectativas de ativação automática foram removidas porque contradiziam a regra solicitada.

Captura population executa o handler real duas vezes no santuário, confirma retirada de exatamente um colono, usa clique para entrar no armazém, espera chegada e depois mostra prédio desabitado cinza. unavailable altera recurso somente no cenário de captura para confirmar retratos cinza; essa captura não gera save. Capturas farmer e work-hover verificadas visualmente. Sem teste físico manual do mouse/teclado.

Save moderno passou para16 (arquivo research-v16.json), com moradores, viagens, comida/progresso do cultivo e descritores dos colonos gerados. Saves4..15 permanecem aceitos, sem sobrescrever arquivos antigos. Originais e saves originais preservados. Lista e contagem de verificações em reports/population_v39_validation.json.

## Arquivos alterados em cópias

Engine/BuildingPopulation.cs, PopulationCapture.cs e GrayGraphics.cs novos. Simulation.cs, RecruitmentSimulation.cs, BarracksRecruitment.cs, FoodSimulation.cs, MiningSimulation.cs, WarehouseTransfer.cs, BuildingInteraction.cs, NativeActionBar.cs, WorkerResourceInput.cs, ResearchGame.cs, GroupCommands.cs e PauseMenu.cs alterados. Tests/PopulationTests.cs e PopulationFixture.cs novos; fixtures e registro atualizados em ResearchTests, ConstructionTests, RecruitmentTests, FoodTests, InteractionTests e MissionOneTests.

Pendências de fidelidade: tempos/custos exatos do cultivo, lotações dos demais tipos, estoques individuais/reserva inicial e cursor/paleta exatos do original. Não se declara gameplay integral do jogo reconstruído.
