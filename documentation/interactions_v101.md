# v101 — Expansão preventiva dos armazéns inimigos

Regra de balanceamento solicitada pelo jogador, aplicada à política comum dos três clãs. Não é apresentada como algoritmo recuperado do executável original.

## Comportamento

- A cada segundo de simulação, observar comida + pedra disponíveis contra a capacidade dos armazéns ativos. Engrenagens não ocupam essa capacidade na implementação atual.
- Com ocupação >= 70%, registrar a necessidade de expansão. O pedido permanece mesmo após gastar recursos e é salvo com a partida.
- Priorizar a ativação de armazéns prontos vazios; evitar novas obras enquanto há um armazém incompleto ou aguardando ativação.
- Remover o limite artificial de quatro armazéns.
- Reservar dinamicamente um colono livre ou excedente do santuário/cultivo, mantendo o operador da construção de origem. Treinamento e distribuição de colonos respeitam essa reserva.
- Reservar o custo de construção (10 alimentos, 6 pedras) antes de gastos não essenciais e preservar uma reserva de sobrevivência: máximo entre 6 alimentos e dois minutos do consumo atual dos personagens.
- Faltando um trabalhador essencial, comida para sobreviver ou havendo invasão à base, adiar a expansão e permitir recuperação emergencial. Obras já iniciadas continuam pelo sistema habitual; não são canceladas nem recebem recursos grátis.
- Procurar terreno conhecido, seguro e permitido, com ponto de trabalho acessível. Preferir proximidade dos trabalhadores/coleta. Não interromper outra obra em andamento para abrir uma segunda.
- Concluir com o construtor e encaminhar um colono pelo movimento normal. A capacidade só aumenta com o armazém habitado.

## Implementação e evidência

`Engine/OpponentWarehouses.cs`: decisão, reserva de orçamento/colono, ativação e filtro de terreno. `OpponentBasicPolicy.cs`: prioridade e observação por segundo. `OpponentDevelopment.cs`, `OpponentSimulation.cs` e `OpponentTradeOpening.cs`: integração com produção, obras e distribuição de residentes. `BuildingPopulation.cs`: saída de residente específico, preservando os outros.

O campo opcional `Opponent.WarehouseExpansionRequested` contém quatro booleanos (índice zero reservado ao jogador). Saves antigos recebem o padrão sem pedidos e são reavaliados no próximo ciclo. O formato interno continua 34; os nomes existentes dos cinco slots permanecem compatíveis. Arquivos originais e saves do usuário não são modificados pelo desenvolvimento.

## Verificação

`--test-warehouse-v101` cobre limites 69%/70%, os três clãs, persistência do pedido, comida/pedra, exclusão das engrenagens, emergência por fome/invasão, obras/vazios, mais de quatro armazéns e ciclo real de construção/ativação/save/load.

Os testes de política econômica, Bolsa de Trocas e leitura dos saves existentes são registrados separadamente em `reports/v101-*`. As manipulações de recursos usadas para acelerar o cenário de teste são restritas ao cenário em memória e registradas como concessão de teste; não existem na política normal.

Resultados no executável publicado: 34 verificações da expansão, 20 da política econômica, 68 da Bolsa e 15 saves existentes carregados sem edição. A reprodução da cópia do slot 5 também passou: o clã alcançou estabilidade e liberou três soldados após aproximadamente 4min07s adicionais de simulação. Isso comprova liberação da ofensiva, não chegada à base do jogador. O SHA-256 do slot original permaneceu `c8d07f62ef2fc6e36c19c76a55db3e321b35c98e0d401eeb92978efc6c379a72`. Auditoria: `logs/v101-release-audit.json`.

## Como testar

Abrir `releases/interactions-v101/Jogar-Quarta-Missao-Com-Monitor.cmd`. É possível carregar o slot 5 existente. Quando houver espaço seguro, construtor disponível, colono e recursos suficientes, a IA passa a ampliar/ativar armazenamento antes de outras construções opcionais. A mudança não promete um horário fixo de ataque nem garante terreno ilimitado em qualquer mapa.
