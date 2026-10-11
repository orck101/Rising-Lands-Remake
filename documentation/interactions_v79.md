# v79 — abertura do adversário e socorro aos aliados

Alteração de balanceamento solicitada e aprovada pelo jogador; estes tempos e multiplicadores não são apresentados como regras extraídas do executável original.

## Abertura

A simulação roda a 30 atualizações por segundo: três minutos correspondem a 5.400 ticks; cinco minutos a 9.000 ticks. O acelerador avança a simulação, portanto acelera também estes prazos. Todos os proprietários adversários utilizam a mesma política a partir da missão 4.

Antes de três minutos, a economia, treinamento e expansão continuam; militares ficam junto à base e podem reagir a ameaças contra ela. Não há expedições ofensivas ou cerco ao jogador. O bloqueio cobre tanto a decisão estratégica como a aquisição automática e a reação direta ao dano fora da base.

Entre três e cinco minutos, uma expedição por adversário com até duas unidades: Swordsman (tipo 0, soldado comum) e Scorcher (tipo 1, lança-chamas), identificados em `source_remake/Data/combat_rules.json`. Tropas de longo alcance podem defender a base, mas não integram ofensivas nesse período.

A partir de cinco minutos, o teto começa em três e aumenta em uma unidade a cada dois minutos, até dez. As decisões não acumulam expedições: uma janela de 90 segundos e o estado de seus participantes controlam os grupos. O limite é por adversário. Tropas fora da expedição permanecem na defesa da base. Soldados de expedições expiradas deixam de receber ordens ofensivas até uma nova autorização.

## Defesa

A percepção para acudir aliados é `ceil(alcance × 1,5)` para alcance maior que um quadrado e `alcance × 2` para combate próximo. A classificação acompanha a geometria de ataque já usada pela engine. Um ataque contra aliado dentro dessa percepção pode gerar aproximação; a distância necessária para efetivamente causar dano continua sendo o alcance original da arma.

A visibilidade compartilhada continua necessária: não se revelam inimigos em regiões ocultas. A percepção adicional não aumenta a descoberta do mapa. As ordens explícitas de movimentação do jogador permanecem prioritárias. A busca comum de contatos visíveis permanece no raio de visão existente; o multiplicador solicitado se aplica ao socorro contra agressores.

## Persistência e evidências

`OpponentState.NextWaveAt` e `Sorties` armazenam prazos e participantes. Saves anteriores sem estes campos recebem os valores padrão; o estoque e os saves originais não foram modificados. A validação verifica os participantes e os prazos. Os testes verificam os limites exatos de três/cinco minutos, composição/tamanho de expedição real, ausência de ataques na abertura, pesquisa/economia, percepção, aproximação sem ataque remoto e continuidade determinística após salvar/carregar.

Fontes da implementação: `Engine/OpponentOpening.cs`, `Engine/OpponentSimulation.cs`, `Engine/AutomaticDefense.cs`, `Engine/ProjectileSimulation.cs`. Testes específicos: `Tests/OpeningV79Tests.cs`; relatório: `reports/v79-final-0.txt`.

A reprodução integral da IA original continua pendente conforme documentado na v78. Esta versão ajusta o adversário jogável; não ativa o backend experimental de execução nativa.

Validação final: 24 casos específicos de abertura/percepção, 11 de defesa, suíte geral completa, integração adversária v75 (incluindo partida longa e slots), 543 casos v76 e 17 casos v77 passaram. Resumo e hashes: `reports/v79-validation.json`.
