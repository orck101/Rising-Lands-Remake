# Fome sem bloquear tarefas — v112 — 2026-10-07

Regra solicitada pelo usuário, para jogador e três inimigos, em todas as missões. Personagens sujeitos à fome continuam sua ação quando não encontram alimento acessível. Fome, perda de vida e morte continuam; ter fome não é mais uma ordem de parar. A cada dois segundos simulados procuram fontes no raio circular de 200 quadrados, nesta ordem: comida solta no chão, armazém próprio, armazém de aliado. Cada categoria prioriza a fonte mais próxima acessível. Caminhos até 400 passos permitem desvios locais; perigo de inimigos e colisões continuam respeitados pela navegação existente.

Causa confirmada: HungerStep inseria FeedingTrip e apagava a rota antes de encontrar comida. Feeding bloqueava FoodStep/MiningStep e outras ações mesmo com Arrival=-1. Agora só publica viagem depois de confirmar fonte/caminho; procura malsucedida restaura a rota intacta. Durante viagem guarda carga e tarefa; após comer ou perder a fonte ao chegar, retoma o destino anterior. Saves antigos com espera sem fonte removem esse estado e recuperam a tarefa. Pesquisa emergencial específica do fazendeiro inimigo deixou de contornar a rotina comum de alimentação: todos usam a mesma prioridade.

Armazéns aliados debitam alimento do dono, mantendo saldo compartilhado e conservação de recursos. Formato 38 aceita anteriores até 37; nomes de slots v33 continuam.

Verificações: 24 checks de continuidade (fazendeiro/construtor de cada proprietário, fome com dano, missão 1, carga, viagem e retomada, preferência por depósito próprio e determinismo); nove de alcance 200/201, alimentação aliada, conservação, ruptura e persistência; 47 de abertura econômica; cinco slots atuais copiados/carregados/continuados em duas instâncias. Todos passaram. Relatório de preservação SHA256: experiments/fome-continua-v112/saves-preservation.json. Nenhum ensaio regrava UserSaves. Build/publish Windows passou.

Ensaios de prioridade usam cenário controlado: uma primeira falha esperava um armazém específico, mas o motor escolhia corretamente outro armazém próprio mais perto; teste corrigido para validar propriedade. O teste de raio também foi corrigido para não transbordar uma coordenada de tile para a linha seguinte. Falhas do fixture não são crashes de uma partida humana.

Limitações: simulação do motor, não partida manual; raio 200 é balanceamento pedido, não regra comprovada do original. Não foi feita uma auditoria de desempenho com centenas de personagens procurando ao mesmo tempo, nem uma nova aprovação geral do balanceamento da campanha.

Código: Engine/HungerSimulation.cs, Simulation.cs, Program.cs; Tests/HungerContinuesTests.cs e MealRadiusTests.cs. Evidências: experiments/fome-continua-v112. Backup: backups/antes-fome-continua-v112-20261007 e pacote v111 preservado. Publicação: releases/interactions-v112.
