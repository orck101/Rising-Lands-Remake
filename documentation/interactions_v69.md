# v69 — fazendeiros sem falas repetidas ao trabalhar

Pacote: `../releases/interactions-v69`, primeira/segunda/terceira missão. Save v29; originais, saves reais e versão anterior preservados.

## Causa confirmada

A v68 corrigiu confirmações de ordens repetidas, mas a ação P 8 continuava sendo emitida por `Simulation.Step` a cada quadrado e P 0x10 por `FoodSimulation.FoodStep` a cada coleta. Para o fazendeiro essas chamadas tocavam falas no canal 2, tratado como efeitos e fora do bloqueio global de vozes. A ação 8 também é a seleção.

Fontes: `Engine/Simulation.cs`, `FoodSimulation.cs`, `AudioEvents.cs`, `AudioSystem.cs`; catálogo `source_remake/Data/audio_catalog.json`: tipo 5, ação 8 → P-159/P-160; ação 16 → P-171/P-172. O encaminhamento do código moderno é confirmado; não se afirma fidelidade completa à cadência sonora original.

## Correção

`AudioPlaybackPolicy` bloqueia P 8 no canal automático de efeitos, em todos os personagens, e P 0x10 do fazendeiro nesse canal. O ponto comum de reprodução aplica o filtro antes do cooldown/criação de instâncias. Seleção e ordens explícitas no canal de voz preservam seus sons originais. Martelo, combate, morte e avisos continuam disponíveis. Avisos de fome permanecem. Nenhum WAV foi substituído; a coleta não foi modificada. Eventos continuam registrados na simulação, mas não chegam ao dispositivo nesses contextos automáticos.

## Validação

Dez fazendeiros e 2.200 atualizações: 1.030 eventos de caminhada e 458 de trabalho. Antes, 1.488 solicitações eram elegíveis para reprodução; depois, zero desses pedidos automáticos passam. São solicitações, não contagem de arquivos efetivamente ouvidos: cooldown e ocupação já descartavam parte deles.

Todos os dez fazem entregas e continuam com ordens ativas. A primeira verificação de entrega foi ajustada para reconhecer os quadrados realmente utilizados pelo descarregamento, em vez de exigir o quadrado âncora. A falha sonora do teste anterior permanece reproduzida em `../logs/v69-audio-before.log`.

No dispositivo real: 200 pedidos automáticos bloqueados sem iniciar áudio; seleção explícita seguinte inicia uma voz. Concorrência da v68 e decodificação/reprodução silenciosa dos 551 clips aprovadas. 486 verificações aprovadas, incluindo oito novas e 14 da v68. Leitura dos saves é somente leitura e o número de arquivos existentes pode variar durante os testes.

Evidências: `../reports/v69-validation.json`, `../reports/v69-audio-runtime.json`, `../logs/v69-audio-after.log`. A escuta subjetiva deve ser conferida ao jogar a nova versão; teste do dispositivo usa volume zero.
