# v50 — coleta contínua e armazém cheio

Abrir `releases/interactions-v50/Jogar-Primeira-Missao.cmd`.

## Comportamentos entregues

1. Uma nova ordem válida de coleta reativa a repetição. Redirecionar uma carga preserva a repetição existente. Comandos de parar ou movimento cancelam a coleta, conservando o recurso carregado; outra ordem de coleta inicia um novo ciclo.
2. Retirar comida, pedra ou engrenagens do armazém repete a viagem até outra ordem. O estoque diminui na retirada efetiva. Quando o recurso acaba, o trabalhador mantém a ordem em espera e retoma se o estoque for reabastecido. Uma pilha de destino cheia também provoca espera, sem perda de carga.
3. Se um armazém ativo estiver cheio, a carga vai para um ponto acessível no chão, ao redor de toda a área ocupada pelo prédio, até três células para fora, incluindo diagonais. Prioridade: menor distância em células ao trabalhador; em empate, pilha do mesmo recurso com espaço; depois, índice da célula. Rotas inacessíveis são descartadas. Cada pilha comporta até 31 unidades. Recursos no chão não são creditados no estoque geral.
4. Se toda essa área estiver indisponível, a unidade espera conservando carga, fonte e destino. O jogo tenta novamente a cada 30 atualizações da simulação. O destino original continua sendo o armazém: o descarte no chão não substitui a ordem permanente. Após uma entrega, a unidade volta à fonte; se houver espaço no armazém na viagem seguinte, entrega nele.
5. Removido o pequeno retângulo verde sob os personagens selecionados. As informações de vida e os demais indicadores de ordens permanecem.
6. O alcance é desenhado para as unidades próprias visíveis selecionadas cujo catálogo possui alcance maior que uma célula. Funciona nas duas missões e em seleção múltipla, mesmo quando a unidade principal é de combate próximo. Mantido o círculo pontilhado anterior; o arqueiro usa o alcance de 10 células do catálogo.

## Diagnóstico e persistência

**CONFIRMADO no remake:** `ResourceGesture.Redirect` desativava as flags de repetição, e `WorkerResourceOrders.Source` não as reativava. Ao esgotar o estoque, a continuação de retirada podia deixar o personagem sem estado de espera. Corrigidos esses caminhos; testes verificam repetição, estoque vazio, reabastecimento e cancelamento.

Save v20 guarda o ponto temporário de descarte, o estado de coleta persistente e a distinção entre uma área de plantas e pilhas soltas. Essa distinção impede que o fazendeiro com ordem de colher plantas busque novamente suas próprias pilhas de excedentes. Os slots procuram v20, v19, v18 e v17. Saves anteriores continuam aceitos; uma ordem antiga que já estava sem repetição deve ser indicada novamente para iniciar o ciclo corrigido. Nenhum save original foi alterado.

**REQUISITO DO JOGADOR / ADAPTAÇÃO:** o descarte automático até três células ao redor do armazém é a solução solicitada pelo jogador. Não se declara que esse algoritmo foi comprovado no executável original. A distância usada é Manhattan para priorização; o limite externo inclui os cantos do retângulo da construção. A capacidade compartilhada existente continua sendo de 100 unidades de comida/pedra por armazém ativo; engrenagens não entram nessa ocupação, conforme a regra já implementada.

## Verificação

- 707 verificações da missão/interações e 351 gerais passaram: 1.058 no total.
- Retirada contínua dos três recursos, espera sem estoque e retomada após reabastecimento.
- Descarte automático, perímetro bloqueado, liberação de espaço, reaproveitamento de pilha com 30 unidades e mudança para outro ponto ao completar 31.
- Conservação de recursos e retomada determinística após salvar/carregar, incluindo uma rota de descarte em andamento.
- Interromper um construtor carregado conserva a pedra e produz um save válido.
- Captura `reports/v50-range-map2-centered.png`: grupo misto na missão 2, sem retângulos sob unidades e com círculo do arqueiro. Unidades reposicionadas e tipos ajustados apenas na preparação da captura; não representa uma partida ou formação paga. A captura verifica a mesma seleção e o mesmo desenho usados na partida.
- Logs: `logs/v50-test-mission-one.log`, `logs/v50-self-test.log`.

Esta entrega não conclui a campanha: vitória/IA específica da missão 2 e efeitos de pesquisa ainda pendentes continuam registrados na v49.
