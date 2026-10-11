# v70 — pesquisa, exploração, terreno e carga

Abra `../releases/interactions-v70/Jogar-Terceira-Missao.cmd` (primeira e segunda também disponíveis). Save permanece v29; originais, saves reais e v69 preservados.

## Pesquisa

Dois caminhos corrigidos: `Scientists` contava uma atribuição Stage 1 apenas pela vida, mesmo com personagem fora do estado alojado; agora exige colono vivo, do jogador, no estado interno 14. `AdvanceMissionResearch` também pulava tecnologias indisponíveis e alterava o índice antes de verificar a presença de colonos. Agora zero pesquisadores bloqueia qualquer mudança de nível/progresso e desativa o andamento, preservando o progresso existente. A entrada posterior retoma o trabalho; pausa manual e tempos 7/4 minutos permanecem.

Fontes: `Engine/ResearchSimulation.cs`, `MissionResearchFlow.cs`. Testes cobrem registro antigo de trabalho com colono visível, retirada do último residente, retomada e salto de nível em laboratório vazio. Não houve redução retroativa de níveis já armazenados nos saves.

## Descoberta e ordens

`CanInteractAt` autoriza interações em células exploradas. Ctrl fonte/destino, redirecionamento, entrada/seleção de prédio, ajuda na obra e construção usam esse estado. A construção exige toda a área já explorada. Na parte nunca visitada, clique normal com unidade selecionada vira apenas movimento/exploração; coleta, entrega, construção e ações especiais são bloqueadas. O botão de testes continua revelando o mapa sem alterar a exploração.

Visão atual continua controlando exibição de inimigos, efeitos e memória do mapa. O jogo pode validar uma ordem contra o estado real do recurso/terreno; não há nova reconstrução de inteligência inimiga nem simulação de edifícios inimigos surgindo sem visão.

O teste dentro da janela real acionou os dois cliques Ctrl em área descoberta sem visão, comprovou rejeição de recursos em célula desconhecida sem alterar ordens e confirmou movimento até a região desconhecida. `../reports/v70-fog-orders-fixed.json`.

## Terreno

Apenas tipos 9 (muralha) e 12 (portão/HERSE) dispensam grama. Obstáculos, sobreposição, limites do mapa, recursos, tecnologia, exploração e acesso continuam sendo verificados.

**Limitação preexistente do portão:** seu registro em `construction_rules.json` não possui objetos, tiles ou estágios gráficos extraídos. A regra de terreno foi corrigida, mas não se liberou um botão que construiria um prédio invisível. A muralha já está utilizável; reconstruir os gráficos e o funcionamento do portão permanece pendente.

## Quantidade carregada

Na seleção, hover ou Espaço, fazendeiro, engenheiro e construtor mostram um marcador original por unidade carregada. Ao entregar, os marcadores diminuem; vazio remove os marcadores. Unidade selecionada sozinha também mostra um contador usando os dígitos originais ao lado do retrato, separado dos estoques globais.

Sondagens do original: `AffInfoSelection` VA 0x410ee3 chama o desenho de inventário em 0x410a2c. Lista no registro +10, contagem +12; recursos food/stone em listas sintéticas de tamanho 2..8 geram sprite 0x2a3d com intervalo vertical de oito pixels. 18 amostras reproduzidas em memória isolada, sem executar o processo Windows original. `../tools/trace_cargo_indicators_v70.py`, `../reverse_engineering/cargo_indicators_v70.json`.

O original suprime lista de tamanho um nesse caminho. Mostrar um marcador para uma carga, aplicar o mesmo desenho às engrenagens do engenheiro e posicionar um contador no rodapé são adaptações explícitas para cumprir a indicação solicitada; não foram apresentadas como reprodução comprovada de todos os estados originais. Captura conferida: `../reports/v70-cargo.png`.

## Validação

561 verificações aprovadas, além de duas verificações de preparação do laboratório e 18 sondagens nativas. Pesquisa v58, descoberta v60, fome v67, áudio v69, simulação e leitura de saves passaram. Uma primeira captura de entrada falhou porque a montagem de teste removia prédios sem converter seus residentes; a montagem foi corrigida. Resultado final: `../reports/v70-validation.json`.
