# v43 — sobreposição permitida na saída de prédios

Pedido: permitir que qualquer unidade saindo de uma construção apareça sobre outra unidade, evitando bloqueio da saída por personagens.

A seleção de saída continua na frente do prédio. O predicado específico `TraversableBuildingExit` valida terreno e construções, mas ignora ocupação por unidades. Aplicado à retirada de colonos, à conclusão de formação de todas as tropas (sem filtro de tipo), e à saída de moradores para viagens de pesquisa. Não foi alterada a movimentação geral, o pathfinding, a entrada, a seleção, os combates ou a coleta para implementar essa exceção.

Uma unidade parada na frente não impede a saída. Vários colonos retirados podem aparecer no mesmo ponto; uma tropa recém-formada pode sair nesse ponto ocupado. Terreno impassável ou prédio na frente ainda bloqueiam; nesse caso, a tropa espera dentro e a retirada manual não consome o morador.

Validação: 516 verificações da primeira missão e 351 gerais, todas aprovadas. Inclui três retiradas no mesmo ponto, tropa formada saindo sobre colono parado, preservação de vagas de entrada, save/load, frente bloqueada por terreno e saída após desbloqueio.

Arquivos alterados: Engine/BuildingExits.cs, Engine/ResearchSimulation.cs e Tests/InteractionV42Tests.cs. Nova publicação em releases/interactions-v43. Save v17 preservado. Originais e saves originais não modificados. A nova exceção não estabelece uma validação completa das colisões de movimento entre unidades no restante do remake.
