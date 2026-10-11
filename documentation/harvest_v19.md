# Coleta contínua — harvest-v19

Data: 28/09/2026. Executável: `releases/harvest-v19/RisingLandsRemake.exe`.

## Comportamento implementado e verificado

- Ao dar uma nova ordem sobre uma planta, o agricultor registra uma área fixa. Com Repetir ativado, esgota a planta e procura outras plantas acessíveis nessa área, preservando o destino escolhido pelo jogador.
- Área provisória: distância Manhattan máxima de seis células em torno da primeira planta. O centro não muda ao encontrar outras plantas. Pilhas no chão não entram na busca de plantas; uma ordem sobre uma pilha continua transportando somente aquela pilha.
- Destino cheio ou caminho interrompido conserva a carga. A cada 30 atualizações da simulação, a ordem pendente tenta novamente o mesmo destino. A entrega atual continua mesmo com Repetir desligado; isso só impede iniciar uma nova carga.
- Uma construção real de armazém pode liberar a entrega que aguardava no santuário. Nenhum recurso é creditado antes da entrega.
- Backspace ou ordem de movimento encerra o trabalho e suas tentativas automáticas. A carga e a escolha do destino permanecem preservadas.
- Dois agricultores que encontram a planta esgotada por outro trabalhador podem continuar na área sem duplicar alimento.
- Verde marca a fonte e o limite de busca; azul marca o destino escolhido, no chão ou na estrutura. Indicadores aparecem para o agricultor selecionado.
- Save versão 10 grava o centro da área. Versões 4–9 continuam aceitas. Ordens antigas continuam de fonte única, evitando ampliar silenciosamente o trabalho ao carregar. Uma nova ordem numa planta ativa a área.

## Evidência e grau de certeza

**CONFIRMADO NO REMAKE, NÃO NO EXECUTÁVEL ORIGINAL:** comportamentos acima exercitados em `Tests/HarvestTests.cs` e `Tests/FoodTests.cs`; funções `GatherAt`, `ContinueHarvest`, `FoodStep` e `ValidateFoodSave` em `Engine/FoodSimulation.cs`. Não correspondem a uma nova confirmação da máquina de estados original.

**CONFIRMADO ANTERIORMENTE NO ORIGINAL:** códigos de plantas 29–52 e transições de colheita, capacidade básica do agricultor, pilha de alimento 67 e depósito compartilhado. Ver `food_reconstruction_v3.md`, `food_original_validation.json` e `food_deposit_original_validation.json`, com endereços e casos de emulação. Esta etapa não altera essas regras.

**PROVISÓRIO:** raio de seis células, ordenação das fontes pela distância Manhattan, repetição da tentativa a cada 30 atualizações, contornos modernos e os 60 ticks de colheita já existentes. Ainda falta provar alcance, prioridade, temporização e animações do original. Não foi introduzida regeneração de plantas nem geração de alimento.

## Validação

309 verificações aprovadas no executável publicado, incluindo 16 novas: área fixa, fonte de duas colheitas, destino preservado, ausência de recolhimento da própria pilha, repetição desligada, parada, bloqueio no caminho, espera por capacidade, construção real que libera entrega, competição entre agricultores, save/load determinístico, rejeição atômica de área inválida e migração v9.

Compilação sem avisos/erros. Captura real `reports/prototype_v19_harvest_a.png` inspecionada: mapa original da missão 1, agricultor real, ordens por API da simulação, sem inserir recursos ou plantas. A captura é uma conferência visual; não equivale a teste manual completo de mouse/teclado ou teste em diferentes versões do Windows.

Relatório com hashes: `reports/harvest_v19_validation.json`. Log: `logs/selftest_harvest_v19.log`. Originais em D:/RISING não foram escritos; as 1.340 cópias preservadas em original/working foram comparadas ao inventário.

## Limites e próximos trabalhos

Continua sendo um protótipo. Pesquisa de engenharia parcial da v18 está preservada. Ainda faltam mineração e economia completa, função do Cultivo e geração de colonos, outros ramos e bônus tecnológicos, campanha automática, IA estratégica, áudio e fidelidade de comandos/animações/timing. A próxima prioridade deve permitir cumprir integralmente os objetivos da primeira missão, começando pela obtenção de rocha com evidência do original.
