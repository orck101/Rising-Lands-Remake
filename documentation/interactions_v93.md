# v93 — área inicial conhecida dos inimigos triplicada

## Cálculo e implementação

Para um círculo, A = πr². Se A_nova = 3A, então r_novo = √3 × r = aproximadamente 1,73205 × r: aumento de 73,205% do raio, em vez de triplicar o raio e revelar nove vezes a área.

O mapa real é quadriculado e a visão inicial é a união de vários prédios e unidades, com sobreposição e bordas. Por isso a implementação conta as casas únicas conhecidas pela visão normal inicial e acrescenta casas próximas da base até atingir exatamente três vezes essa contagem. Preserva as casas originalmente conhecidas e respeita largura e altura do mapa. Se o mapa físico não comportar três vezes a área, o limite é seu tamanho total.

Resultados das novas partidas:

| Missão | Inimigo | Área normal inicial | Área conhecida nova | Fator |
| --- | --- | ---: | ---: | ---: |
| 4 | Vermelho | 616 casas | 1.848 casas | 3× |
| 5 | Clã 1 | 238 casas | 714 casas | 3× |
| 5 | Clã 2 | 240 casas | 720 casas | 3× |
| 5 | Clã 3 | 347 casas | 1.041 casas | 3× |

A expansão considera terreno ao redor dos centros dos prédios da própria base; empates usam a posição da casa para manter partidas e reinícios determinísticos. Não escolhe regiões distantes só para revelar recursos. Na missão 4, a área nova inclui 59 casas de alimento e duas minas. Na abertura da missão 5, não foram detectadas casas de alimento prontas nessa área; ela não concede recursos e não elimina a necessidade de explorar mais ou esperar o crescimento normal. O clã 2 conhece uma mina; os outros dois ainda precisam descobri-las.

## Comportamento

- Conhecimento ampliado somente ao iniciar uma nova simulação com adversários ativos.
- Visão atual de tropas/prédios mantém os raios normais. O terreno conhecido não concede visão permanente de personagens inimigos.
- Contatos militares são registrados pela visão normal, antes da expansão; os recursos da área conhecida podem ser planejados pelo sistema de coleta existente.
- Ao mover tropas, a descoberta adicional usa a visão normal, sem reaplicar o multiplicador.
- A descoberta do jogador permanece igual.
- Conhecimento é salvo e carregado normalmente. Saves antigos mantêm sua própria área já explorada; não são ampliados retroativamente. Reiniciar uma missão na v93 recria o início novo.
- Prédios, população, tecnologias, recursos e custos da v92 permanecem.

Para usar: `releases/interactions-v93/Jogar-Quarta-Missao-Com-Monitor.cmd` ou `Jogar-Quinta-Missao.cmd`.

## Validação e desempenho da economia

`reports/v93-exploration-tests.txt`: 27 verificações — cálculo, área exata, bordas, limite físico, preservação de casas, quatro clãs/missões, visão normal separada, descoberta do jogador, salvamento e reinício.

`reports/v93-regression-0.txt`: 13 verificações da economia/produção da missão 4. Mercado e bolsa continuam sendo as primeiras obras, a economia gera colonos, constrói quartel, treina tropas com custos reais e respeita alianças. Primeiro soldado em 85.239 ticks, aproximadamente 47,4 minutos em 1x. A v92 tinha 84.939 ticks (47,2 minutos). Neste cenário o conhecimento maior não acelerou a primeira produção militar; o gargalo principal continua sendo a economia, não apenas a exploração. Não apresentar a ampliação como solução já comprovada para o ritmo de desenvolvimento.

`reports/v93-regression-1.txt`: 36 verificações de reparos, defesa e desenvolvimento real dos três inimigos da missão 5. `reports/v93-regression-2.txt`: 9 verificações de compatibilidade de partidas antigas e recrutas historicamente proibidos. `reports/v93-build.txt`: publicação Windows autocontida concluída.

Código: `Engine/OpponentInitialExploration.cs`, chamada no início de `Engine/Simulation.cs`; testes em `Tests/ExplorationV93Tests.cs`. Somente o remake e seus relatórios foram alterados; arquivos e saves originais permaneceram intactos.
