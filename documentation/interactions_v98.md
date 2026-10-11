# v98 — inteligência básica comum aos três inimigos

A mesma política atende os clãs 1, 2 e 3. Cada clã possui seu próprio histórico de produção, consumo estimado, reserva, estabilidade e recuperação. As personalidades individuais ainda não foram acrescentadas. `PolicyFor(owner)` é o ponto de extensão para isso; atualmente retorna o perfil da missão para qualquer clã.

Esta política é um ajuste do remake solicitado pelo jogador. Não é apresentada como tradução comprovada da inteligência artificial original.

## Economia e defesa

- Mede comida efetivamente entregue ao armazém ou cultivo, numa janela de até três minutos de simulação. Amostras a cada dez segundos; decisões econômicas a cada segundo.
- Estima o consumo pela regra de fome já implementada, considerando personagens visíveis que precisam comer. Usa atividade contínua como previsão conservadora; operadores dentro de prédios não entram nesse cálculo.
- Conta alimento conhecido, alcançável e seguro perto da própria base. Plantas jovens contam como oferta potencial; não equivalem a comida já disponível no estoque.
- Calcula a meta de fazendeiros pela produção observada por fazendeiro, manutenção, margem de produção e alimento conhecido. Quando falta alimento local, limita essa meta. Os trabalhadores continuam usando custos, deslocamento, coleta e tempos normais, sem aceleração artificial.
- Preserva reserva alimentar para manutenção e reposição de um colono/trabalhador. Treinamento militar continua exigindo tecnologia, prédio correto, operador disponível, recursos reais e tempo de produção.
- Mantém combatentes de guarda antes de enviar uma ofensiva. Só permite novos ataques com santuário, armazém e cultivo ativos, fazendeiro e construtor, produção acima do consumo, reserva suficiente e estabilidade econômica.
- Perder trabalhadores ou prédios, faltar reserva ou sofrer invasão interrompe a estabilidade. Suspende novas ofensivas e chama de volta os grupos enviados enquanto a economia não estiver pronta. A defesa local continua funcionando.
- A recomposição segue as ordens normais de produção e construção. O estado de recuperação termina após estabilidade contínua; não oferece recursos ou personagens gratuitos.

Não existe mais um horário absoluto que libere o primeiro ataque. A janela de estabilidade econômica e o intervalo entre grupos são controles de segurança, não ordens para atacar quando o relógio chega a um minuto específico.

## Progressão da campanha

| Missões | Reserva de manutenção | Estabilidade exigida | Guardas | Grupo de ataque | Meta máxima de combatentes | Limite de fazendeiros |
|---|---:|---:|---:|---:|---:|---:|
| Até 4 | 10 min | 180 s | 3 | 2–3 | 8 | 8 |
| 5–6 | 7 min | 120 s | 3 | 2–5 | 12 | 10 |
| 7–10 | 4 min | 90 s | 4 | 3–7 | 16 | 12 |
| 11 em diante | 2 min | 60 s | 4 | 3–10 | 20 | 14 |

Todos os tempos são de simulação, escalados pela velocidade do jogo. Os números são metas ou limites, não unidades concedidas. As restrições tecnológicas de cada missão continuam valendo. Esses perfis não significam que todas as missões futuras estejam completas ou balanceadas.

Reserva de comida = maior valor entre o piso (20 até missão 4; 12 nas demais) e consumo previsto × minutos de segurança + custo de um colono + maior custo alimentar entre fazendeiro/construtor.

## Segunda base

- Não repete santuários ao lado da primeira base.
- Exige economia pronta, guarnição e grupo mínimo, seis colonos e recursos para santuário/armazém/cultivo além da reserva alimentar.
- Procura local conhecido e acessível, a pelo menos 28 casas de distância Manhattan da referência inicial, afastado dos outros prédios próprios, com alimento próximo e sem ameaça conhecida na área/entrada.
- Quando elegível, aumenta a meta de coleta de pedra para financiar santuário, armazém, cultivo e quartel. Antes, o limite de pedra impedia chegar ao custo da expansão.
- Após o santuário, prioriza armazém, cultivo e quartel a até 12 casas da nova base. As construções de apoio preservam a reserva de comida e pagam seus custos reais.
- A segurança depende do conhecimento do clã; não concede visão integral do mapa nem garante ausência de ameaças escondidas.

A abertura comercial do vermelho na missão 4 continua específica daquela missão: mercado e bolsa antes do desenvolvimento militar. A política comum não apaga essa configuração inicial.

## Persistência e validação

O histórico econômico é salvo com a partida e retomado deterministicamente. Saves antigos sem histórico inicializam a medição ao carregar. Isso pode exigir uma nova janela de estabilidade antes de permitir uma ofensiva. Históricos inválidos são recusados antes de alterar a partida.

Arquivos principais: `Engine/OpponentBasicPolicy.cs`, `OpponentDevelopment.cs`, `OpponentOpening.cs`, `OpponentSimulation.cs`. Testes: `Tests/PolicyV98Tests.cs`.

Relatórios em `reports/`:

- `v98-policy.txt`: decisões dos três clãs, progressão, defesa, recuperação, expansão financiada, simulação real da missão 4 e salvamento.
- `v98-regression-v91.txt`: combate, dano/reparo e economia real dos três inimigos na missão 5.
- `v98-regression-trade.txt`: 24 verificações de comércio e entrega de tropas.
- `v98-regression-crashes.txt`: 59 verificações, incluindo simulação prolongada com snapshots validados.
- `v98-regression-v71.txt`: 96 verificações das mecânicas anteriores.
- `v98-player-saves.txt`: 15 saves inspecionados somente para leitura, preservando os arquivos.
- `v98-publish.txt`: compilação/publicação da versão distribuída.

No cenário automatizado da missão 4 sem reforços gratuitos, a economia ficou pronta no tick 33990 (18 min 53 s) e o primeiro grupo saiu no tick 70860 (39 min 22 s). Em 5x, o segundo valor equivale a cerca de 7 min 52 s reais. É um resultado daquele cenário, não um prazo fixo de ataque. Intervenções do jogador, perdas, terreno e disponibilidade de alimento mudam o ritmo.

## Como testar

Use `releases/interactions-v98/Jogar-Quarta-Missao-Com-Monitor.cmd` para a missão 4, ou `Jogar-Quinta-Missao.cmd` para os três inimigos. Observe a coleta perto das bases, a formação de guardas, a recuperação após perdas e as ofensivas condicionadas ao estoque. Para avaliar a nova política desde o início, inicie uma nova partida; os saves existentes permanecem disponíveis.

A validação automatizada confirma as condições e conservação de recursos. O equilíbrio e as personalidades ainda precisarão de avaliação jogando cada missão.
