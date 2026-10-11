# Fechamentos da missão3 na versão portátil — v128

## Diagnóstico confirmado

Registros originais de COMP: 20261011-012845-418-21512.json e 20261011-013144-497-6284.json. Os dois stacks terminam em ResourceGesture.Commit -> OrderPreview -> Restore. Exceções: Invalid warehouse overflow order e Conflicting worker orders. Contexto: MONDE03.ETA, Ctrl, fonte marcada; ticks39508/14978, velocidades5x/3x. A primeira cópia já possui duas pesquisas concluídas. Não foi falha do decodificador/animação da pesquisa.

Reprodução antes: ambas as snapshots reais falham com código1 no carregamento da v127. Evidências copiadas sem alterar as partidas.

## Causas e correção

StartConstruction/ResumeConstruction cancelavam a coleta mas retinham OverflowCell. Agora descartam somente esse destino obsoleto, preservando a carga.

O gesto em dois cliques escondia temporariamente a carga para escolher fonte nova. Ao restituir pedra coletada do chão, o trabalhador mantinha MineJob1 junto de FoodJob3. Entrega de carga cancela o outro trabalho ativo; depois da entrega, a mina escolhida é retomada. Entrega de pedra minerada também cancela transporte genérico concorrente.

Formato46 aceita4–45. Migração estreita recupera os dois estados antigos conhecidos em memória, sem reescrever saves. Marcadores fora do mapa e conflitos em arquivos46 continuam rejeitados.

Arquivos: ConstructionSimulation.cs, FoodSimulation.cs, MiningSimulation.cs, Simulation.cs, novoWorkerOrderRecovery.cs e WorkerOrderRecoveryTests.cs. Program.cs adiciona teste; ResearchGame.cs/ResearchCompletion.cs adicionam captura isolada da segunda descoberta.

## Validação

21 checks de recuperação, incluindo as duas snapshots reais, continuidade1800ticks com previews, novo gesto carregado, retomada da mineração3000ticks, começo e ajuda de construção com carga; passaram na candidata final e no executável entregue emCOMP.

Regressões: pesquisa 53, descoberta/animações 46, coleta 52, fila 54: passaram. Dois arquivos inválidos foram rejeitados, saída1 esperada. Captura da segunda descoberta concluiu duas pesquisas e abriu o filme original, saída0.

O primeiro ensaio visual usou um fixture antigo sem provisão militar e parou durante sua preparação, antes de pesquisar. A captura final usa o fixture de tempo de pesquisa já existente, com provisão exclusiva do ensaio. Nenhuma regra de fome/dificuldade modificada.

## Entrega

D:\RISING\COMP\Rising.exe (DLL atualizada, apphost de mesmo nome).
Espelho: releases/portable-v128-final2, self-contained win-x64 com mapas/sons/gráficos.
Backup dos arquivos substituídos: backups/portable-comp-antes-v128.
3718 arquivos de aplicação conferidos por SHA256; assets portáteis preservados. UserSaves/UserSettings/logs do usuário não substituídos. Hashes dos saves reais iguais aos anteriores.

Evidências/inventário: experiments/crash-missao03-v128. Logs e candidatos preservados. Atalhos de outras entregas não alterados. Não houve campanha manual completa; a correção foi comprovada em reprodução por snapshots, simulação e renderização real.

## Publicação adicional no projeto

Em10/10/2026 compilado releases/interactions-v128/RisingLandsRemake.exe, .NET8 win-x64 self-contained.21checks reais de recuperação passaram novamente; cinco slotsv33 atuais copiados carregaram, saída0, e captura da segunda descoberta passou, saída0.13 atalhos ativos1–9/variantes apontam para esta versão. Backup em backups/atalhos-antes-v128.21saves reais tiveram hashes preservados. Atalhos históricos e monitor não modificados. Evidências extras em experiments/crash-missao03-v128/project-release.
