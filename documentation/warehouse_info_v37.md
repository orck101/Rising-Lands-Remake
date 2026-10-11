# Armazém — v37

Entrega releases/warehouse-info-v37/Jogar-Primeira-Missao.cmd. Selecionar armazém mostra número de armazéns, ocupação/capacidade, espaço livre e situação. Os três ícones de recurso informam seus totais; quarto ícone original do armazém mostra resumo da rede. Composição dessa barra e tooltip são adaptadas, não confirmadas como menu exato original.

CONFIRMADO: GetRessourceStock0x467531 executado isoladamente em Unicorn para quatro jogadores. Tabela0x4e2e94, stride32, pares reserva/total: engrenagens+0, comida+8, pedra+16. Retorna diferenças comida/pedra/engrenagens. Evidência reports/warehouse_stock_v37.json; tools/trace_warehouse_stock_v37.py.

FORTE EVIDÊNCIA: BatimentRecalculRessource0x41a08c..0x41a093 soma comida/pedra, sem engrenagens, e usa capacidade multiplicada pela contagem de armazéns. A reserva moderna continua calculada pelo modelo pré-existente de recursos iniciais/custos de construção; identidade completa com todos os gastos nativos não confirmada. Capacidade100 por armazém é regra pré-existente do protótipo, não nova descoberta deste trace. Não há inventários individuais implementados.

WarehouseSummary centraliza informações da rede, ocupação e espaço. Entregas de comida e mineração consultam o mesmo WarehouseSpace, mantendo comportamento anterior. Acima da capacidade, espaço é zero e resumo indica Superlotado. Engrenagens mantêm a entrega anterior; seu limite próprio original ainda precisa de análise. Tooltip reduz escala quando necessário para caber com a fonte original.

Validação: publicação, suite missão1 e regressão aprovadas. Sete novos checks: reserva inicial, capacidade compartilhada, engrenagens excluídas da ocupação, lotação exata, superlotação, retirada libera espaço, segundo armazém expande capacidade. Captura v37-warehouse.png revisada. Suites existentes cobrem retirada real/entrega/carga preservada e construção conjunta. Sem teste físico manual.

Alterações só em cópias: Engine/BuildingInteraction.cs, FoodSimulation.cs, MiningSimulation.cs, NativeActionBar.cs; Tests/WarehouseNetworkTests.cs e registro na suite; ferramenta/relatório novos. Originais e saves originais preservados. Próximas pendências: conferir distribuição nativa por armazém e reserva consumida por recrutamento, limite de engrenagens e outras ações de construções.
