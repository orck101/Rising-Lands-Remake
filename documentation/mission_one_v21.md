# Primeira missão jogável até a conclusão — v21

## Escopo acordado

Trabalhar fase por fase. Esta entrega integra exclusivamente a vitória da primeira missão. Ao completar os requisitos, a partida congela e aparece “VOCE CUMPRIU A MISSAO”, com um único botão “Proxima missao”, intencionalmente sem efeito. Não há transferência nem carregamento de outra missão por esse botão. F5/F9 continuam disponíveis na tela final; fechar a janela encerra o aplicativo.

## Regras e evidências

**CONFIRMADO no original:** armazém próprio (tipo3) com âncora dentro de x0..28/y0..25 inclusivos; estoque de alimento >=50 e rocha >=50. Uma obra tipo3 em andamento também satisfaz o predicado de existência, embora não conceda capacidade antes de terminar. Fonte MONDE01.ETA, condições a partir de 340552 e região em340538. Avaliador TestFinDeMission0x4585d2, recurso0x467579, região0x47bbf6, existência0x41a7d5. Evidências anteriores preservadas em mission_one_conditions.md e mission_lifecycle.md.

A implementação FirstMissionSatisfied é comparada diretamente aos75 vetores nativos salvos em reverse_engineering/mission_one_vectors.json, sem novo desenvolvimento nas outras fases. Inclui falta de prédio, proprietário/tipo incorretos, estoque49/50/51 e ausência de sobreviventes.

**CONFIRMADO no bloco original0x42d518:** acumulação de100 avaliações positivas; uma negativa não zera o contador nesse bloco. O remake guarda MissionChecks, limitado a100. **PROVISÓRIO:** uma avaliação por atualização moderna; não se afirma equivalência em segundos ou dos resets externos do motor original.

Sobreviventes: proprietário0, tipo diferente de17, estado diferente de0/4, incluindo colonos escondidos estado14. **ADAPTAÇÃO MODERNA explícita:** Health>0 também é exigido, pois a transição completa de morte nativa não está integrada. Isso impede vitória apenas com unidades mortas. Esta entrega não acrescenta tela automática de derrota.

## Implementação e persistência

Engine/MissionOne.cs: predicado, região, contador, congelamento e RequestNextMission sem efeito. Engine/MissionCompleteMenu.cs: modal final, único botão sem ação, bloqueio de comandos e indicador de progresso. ResearchGame chama a tela antes dos menus/ordens e a desenha por cima de qualquer painel. Simulation.Step não avança após vitória.

Savev12 armazena MissionChecks. Saves4–11 começam com contador0 e têm os objetivos reavaliados normalmente. Vitória salva restaura a tela. Save com contador inválido ou vitória incompatível com requisitos é rejeitado antes da aplicação. Reiniciar limpa o resultado e restaura30/30 e o mapa original. Validação do contador não pretende provar todo o histórico de um save editado externamente.

## Verificação focada

Comando: RisingLandsRemake.exe --test-mission-one. Testes de fronteiras, sobrevivente oculto, obra, contador interrompido, save/load e botão inerte; inclui FoodTests e MiningTests na missão1. Não executa a bateria geral das outras missões.

PlayToVictory constrói o armazém com custos reais, dá destinos aos trabalhadores, procura alimentos existentes, minera a jazida original e espera estoques50/50 e conclusão. Nenhum objeto/recurso extra é injetado nessa partida. As fixtures sintéticas dos testes de limites continuam identificadas separadamente nas etapas anteriores.

Capturas reais: reports/prototype_v21_start_a.png e prototype_v21_victory_a.png, visualmente inspecionadas. Vitória obtida na captura pelo mesmo percurso de ordens, não pela alteração direta dos estoques. A inspeção visual e testes da simulação não substituem jogar manualmente com teclado/mouse nem comprovam fidelidade integral ao original. A tela final é moderna, usa a fonte ASCII existente e não é uma reprodução dos assets do menu19.

## Pendências para aprovação da primeira fase

Comparação manual original/remake de comandos, tempos, rotas, animações, formação de unidades, estruturas específicas, áudio e acontecimentos da missão. A vitória funcional não significa que todos esses comportamentos já estejam fiéis. Nenhuma nova fase será desenvolvida antes da validação da primeira.

Originais de D:/RISING não escritos. Alterações restritas ao remake, testes, nova release e documentação/relatórios na área separada. Registro de validação e hashes: reports/mission_one_v21_validation.json.
