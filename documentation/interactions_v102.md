# v102 — Pressão ofensiva progressiva exclusiva da Missão 4

Regra de balanceamento solicitada pelo usuário; não é apresentada como comportamento recuperado do executável original.

## Comportamento implementado

- Somente MONDE04.ETA. Ativação em 108000 passos a 30 Hz: 60 minutos efetivos de simulação. Pausas e telas que suspendem a simulação não avançam Tick; velocidades 2x–5x antecipam o horário real sem mudar o tempo de missão exigido.
- Na ativação, reagrupar as antigas pequenas surtidas na primeira onda de dez. Uma surtida anterior de duas/três unidades não aumenta antecipadamente a progressão.
- Formar exatamente a meta atual com combatentes vivos, visíveis e disponíveis, com ataque efetivamente implementado. Trabalhadores, colonos, mensageiros e transportes não contam. Composição pode variar conforme unidades tecnologicamente liberadas; não desbloqueia arqueiros ou outras classes por concessão.
- Retirar teto militar fixo, reserva de guarnição, teto da onda, espera entre surtidas, exigência de estabilidade financeira, trava da abertura comercial e reservas estratégicas que barravam treinamento ofensivo. Produção paga integralmente comida, pedra e engrenagens e continua dependendo de prédio habilitado/habitado, colonos e fila livre.
- Dar ordens diretas contra unidades/estruturas do jogador. Membros ficam identificados até a morte de todos, sem prazo de 90 segundos e sem recuo por ferimentos, desvantagem ou recuperação econômica. Se precisarem buscar alimentação, continuam pertencendo à mesma onda e retomam a ofensiva; não há substituição por uma segunda onda enquanto algum sobreviver.
- Após a destruição completa: próxima meta = anterior + teto(anterior/4), equivalente a teto(anterior × 1,25). Progressão: 10 → 13 → 17 → 22 → 28 → 35 → 44 → 55. Decisão e produção retomam no próximo ciclo de IA de um segundo, sem espera de 90 segundos.
- Recrutamento militar tem prioridade enquanto a onda é formada; não iniciar novos reforços enquanto uma onda está ativa. Filas já pagas podem concluir normalmente. Economia, obras e alimentação reais continuam funcionando.
- Parar ataques após vitória, eliminação do jogador ou aliança. Sem meios reais para financiar/produzir a próxima onda, não criar recursos ou unidades e não enviar um grupo incompleto. Uma falta temporária pode ser superada pela coleta/cultivo já existente; não declarar falência irreversível só porque o estoque momentâneo é zero.
- Nenhuma alteração de objetivos ou tela de vitória/derrota: incapacidade econômica encerra a capacidade ofensiva, não concede automaticamente a vitória ao jogador.

## Persistência e compatibilidade

Campo opcional Opponent.MissionFourWaves: meta de tamanho e lista fixa de membros, independente por clã. Saves antigos sem o campo inicializam meta dez na primeira avaliação após 60 minutos; novos saves preservam progresso e membros. Validador rejeita estado inválido sem substituir a partida. Restart limpa cronômetro e progressão. Formato interno continua 34 e nomes dos slots permanecem iguais. Não modifica arquivos de UserSaves nem executáveis/saves originais.

## Arquivos

- Engine/MissionFourPressure.cs: política da missão, membros, progressão, alvo e validação.
- Engine/OpponentSimulation.cs: integração com ciclo, produção prioritária e save/load.
- Engine/OpponentOpening.cs, OpponentSafety.cs, OpponentBasicPolicy.cs: desvio exclusivo após o prazo, sem mudar a política das demais fases.
- Engine/OpponentDevelopment.cs, OpponentTradeOpening.cs e EnemyStartingBalance.cs: retirada das travas estratégicas da abertura/treinamento depois do prazo.
- Tests/MissionFourV102Tests.cs e Program.cs: verificações e replay.

Cópias dos arquivos anteriores à mudança: backups/mission4-v102-before. A árvore não contém repositório Git; alterações foram comparadas com essas cópias e o código existente, sem pressupor exclusividade sobre arquivos compartilhados.

## Verificação no pacote jogável

- --test-mission-four-v102: 28 verificações (limite do tempo, composição, última unidade viva, progressão até 44, inexistência de recursos gratuitos, save/load, save antigo, corrupção rejeitada, restart, aliança, eliminação do jogador, vitória e fronteira da missão 5).
- --test-mission-four-replay-v102 --save experiments/military-slot5-analysis/snapshot.json: 9 verificações. O snapshot real começou com oito soldados; retomou produção debitando os custos completos, respeitou tecnologias, enviou dez soldados em 41,2 segundos adicionais e causou 76 pontos de dano às unidades do jogador em 101,8 segundos adicionais. Save/load ativo e mais 300 passos após reload tiveram resultados idênticos.
- --test-warehouse-v101: 34 verificações.
- --test-policy-v98: 20 verificações, incluindo defesa/economia nas outras fases e continuidade da abertura anterior aos 60 minutos.
- --test-trade-v100: 68 verificações.
- --check-player-saves: 15 saves carregados e preservados. Auditoria de todos os 19 arquivos da pasta UserSaves confirmou hashes e inventário intactos.
- Total: 159 verificações das cinco suítes, mais leitura dos 15 saves; todos os processos terminaram com código zero.

Logs: reports/v102-release-mission-four.txt, v102-release-replay.txt, v102-release-warehouse.txt, v102-release-policy.txt, v102-release-trade.txt e v102-release-saves.txt. Auditoria do pacote/saves: reports/v102-release-audit.json. SHA-256 do snapshot preservado: 8648b93c4d5ff3c33a733f5ceff8c49f55f5e213dc2f5dd15045f56e8da89509.

## Entrega e limites

Versão Windows x64 independente do .NET instalado: releases/interactions-v102. Usa os assets existentes em working/RISING e saves compartilhados em UserSaves. A v101 não foi substituída.

Abrir:
C:\Users\Victor Freitas\Documents\Codex\2026-09-25\quero-que-acesse-uma-pasta-do\outputs\RisingLandsResearch\releases\interactions-v102\Jogar-Quarta-Missao.cmd

Com o monitor existente:
C:\Users\Victor Freitas\Documents\Codex\2026-09-25\quero-que-acesse-uma-pasta-do\outputs\RisingLandsResearch\releases\interactions-v102\Jogar-Quarta-Missao-Com-Monitor.cmd

CONFIRMADO na execução do remake: formação, ataque com dano, crescimento controlado e persistência nos cenários acima. Não houve partida manual completa até vitória/derrota, nem prova de conclusão para toda construção de mapa/save possível. Caminhos bloqueados, suprimento, natalidade e capacidade finita de população/recursos permanecem condições reais; não receberam teletransporte ou concessões. Música original ausente, magia da IA e fidelidade integral do remake continuam pendências gerais anteriores. O modo experimental da IA original para pesquisa não foi alterado; o pacote jogável usa a política do remake.
