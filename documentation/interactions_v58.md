# v58 — fluxo de pesquisa por missão

## Regras aprovadas pelo jogador

Pesquisa automática quando há pesquisadores e uma descoberta disponível; pausa manual persistente. As duas descobertas obrigatórias pertencem ao ramo escolhido entre fases. Após elas, uma terceira opcional custa 50 alimentos e permite escolher um dos quatro ramos disponíveis, inclusive o inicial. O desconto só ocorre na confirmação. Terceira descoberta desenvolvida com tempo normal; nenhuma quarta descoberta ou compra.

Cada pesquisa dura 12.600 atualizações com um pesquisador (420 segundos em 1x) ou 7.200 atualizações com dois (240 segundos em 1x). A simulação roda a 30 atualizações por segundo. Em 5x: 84 ou 48 segundos. Mais laboratórios não reduzem o tempo abaixo do de dois pesquisadores. Ao mudar o número de pesquisadores, a fração concluída é preservada. O tempo restante é estimado com a equipe atual e a velocidade selecionada. Ausência de pesquisadores suspende; retorno retoma se não houver pausa manual.

Essas durações e a terceira descoberta paga são regras novas do remake, não medições confirmadas do executável original. Os custos originais de pesquisa continuam no catálogo; são usados para normalizar a fração de progresso, não como cobrança de alimentos.

## Causa e mudanças

CONFIRMADO no código anterior: InitialResearch criava Enabled=false, OpenResearch apenas abria a tela e o botão minúsculo iniciava a pesquisa. Agricultura e engenharia possuíam contadores independentes com limite dois em cada ramo; selecionar colunas permitia trocar durante a missão. A interface mostrava percentual sem etapa e sem barra da descoberta atual.

MissionResearchFlow.cs centraliza a quota compartilhada, o ramo fixo, a compra e o avanço. Pesquisa 1/2 e 2/2 usa o ramo inicial; Pesquisa extra 3/3 usa o ramo confirmado. As colunas não trocam o ramo durante pesquisas obrigatórias; só aceitam escolha durante a compra extra. ResearchMenu.cs preserva o fundo, fonte, barras acumuladas e animações originais, apresenta andamento e pausa/retomada e remove Enviar/Retornar/Ver/Próximo laboratório. O botão original de confirmação também alterna a pausa. A tela de compra apresenta benefício, estoque, custo e cancelamento, sem descontar na prévia. Ao atingir a quota, a barra permanece em 100%, mostra a última descoberta e informa o limite.

Os quatro estados são salvos. EffectiveLevels, liberação de prédios/unidades e herança de campanha consideram todos os ramos. A compra entra na conservação de alimentos. O carregamento rejeita quota excedida, ramo alterado e compra sem duas descobertas. O avanço parcial só é herdado na transição se o ramo continuar o mesmo, evitando transferir progresso entre tecnologias diferentes.

Armamento MILITAIRE2 aplica +1 no cálculo de dano das unidades próprias que usam o combate implementado. Engenharia GENIECIVIL8 aplica o fator 15/10 do catálogo como 3 pontos por martelada em vez de 2; a cadência e a animação não mudam. Essa aplicação do fator ao controlador moderno é uma adaptação, não uma nova medição do tempo nativo.

## Compatibilidade e limites

Save v23 aceita versões 4–22 e os cinco slots continuam compartilhados. Saves antigos são migrados em memória; descobertas antigas e a fração de progresso são preservadas. Históricos que já tinham descobertas em vários ramos não são apagados para impor retroativamente a nova quota. A pesquisa antiga sem início explícito passa a automática conforme a regra aprovada. Os arquivos antigos permanecem intactos, com precedência dos novos saves quando um slot é salvo novamente.

Os dados originais ainda limitam espiritualidade à missão 7. Pesquisas com poderes ou efeitos de combate ainda ausentes no remake permanecem indisponíveis, com mensagem explícita e sem cobrança. Esta versão não implementa todos os poderes, unidades, IA ou finais das missões posteriores. Não altera a instalação original D:/RISING.

## Validação e evidências

ResearchV58Tests.cs verifica limites de tempo no tick anterior e no tick exato, alteração de equipe sem reset, pausa/retomada, ausência/retorno, compra insuficiente, escolha futura indisponível, cobrança única, terceira no mesmo ramo e em outro, dano militar, trabalho por martelada, rejeição de quarta evolução, migração v22 e restauração determinística de progresso, escolhas e consumo de comida. ResearchV58Capture.cs usa as áreas reais de clique para bloquear troca precoce e conferir escolha sem desconto, cancelamento sem cobrança e confirmação por 50 alimentos.

Logs finais: logs/v58-research-tests.log, v58-self-tests.log, v58-mission-one.log, v58-mission-two.log, v58-regression-v57.log, v58-player-saves.log. Capturas finais verificadas visualmente: reports/v58-final-active.png, v58-final-choice.png, v58-final-complete.png, v58-final-offer.png, v58-final-paused.png. Contagens e estado de publicação em reports/v58-validation.json.
