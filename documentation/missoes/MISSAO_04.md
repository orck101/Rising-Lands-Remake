# Missão 4 — contexto de trabalho

Atualizado em 2026-10-03. Mapa `MONDE04.ETA`.

## Confirmado na entrega v101

- Vermelho usa economia real e tecnologias iniciais agricultura 2, armamento 2, engenharia 7, espiritualidade 0. Consulta atual de desbloqueio permite militar tipo 0, espadachim; não produzir arqueiros/carros sem desbloqueio.
- Abertura tem santuário com três colonos, cultivo e armazém habitados, dois fazendeiros, um construtor e colonos livres; estoque inicial reduzido.
- Mercado e Bolsa são prioridades da abertura; depois quartel e desenvolvimento. Regras gerais e expansão de armazéns estão no contexto comum.
- O perfil da v101 limita o exército a oito vivos, retém três defensores e envia ondas de duas a três. Esse limite é a causa confirmada da estagnação militar no slot 5 analisado.

## Diagnóstico já concluído — não repetir sem motivo

Relatório: `reports/analise_producao_militar_slot5_v101.md`, relativo à raiz da pesquisa.
Snapshot: `experiments/military-slot5-analysis/snapshot.json`.
Replay: `reports/slot5-military-replay.txt`.

Tick 812633: comida 618, pedra 182, oito espadachins vivos, três colonos no quartel e nenhuma produção militar em andamento. Economia pronta, reserva 41. Retirar um soldado da contagem numa segunda cópia fez começar outro recrutamento imediatamente. No replay normal de vinte minutos, oito soldados permanecem e não há recrutamento novo. Pequenas ondas recuam feridas/desfavorecidas; feridos ainda contam no teto e não conseguem formar outra onda. Não atribuir este caso exclusivamente ao prazo de 90 segundos: recuos observados ocorreram antes do prazo.

## Pedido observado em execução em outra conversa

A conversa local MISSAO 4 recebeu um pedido do usuário para uma regra exclusiva dessa missão: após 60 minutos de simulação, ondas mínimas de dez combatentes válidos, persistentes até eliminação da onda; próxima onda cresce 25% com arredondamento para cima, respeitando custos/tecnologias. Essa informação identifica trabalho externo em andamento, não declara implementação concluída e não manda esta conversa executar o pedido de outra por conta própria. Conferir a conversa e o código atual antes de integrar ou alterar esses arquivos.

## Verificações relevantes

- `--test-policy-v98`: política geral; ajustar expectativas apenas onde houver mudança intencional e preservar outras missões.
- `--test-warehouse-v101`: armazenamento/colonistas/recursos.
- `--check-player-saves`: leitura sem modificação de partidas existentes.
- `--analyze-military-slot --save <snapshot>`: diagnóstico, não teste universal de vitória.
- Acrescentar verificações específicas de fronteira dos 60 minutos, tamanho/composição das ondas, sobrevivência de uma última unidade, arredondamento, recursos/tecnologias e persistência, caso implementar o pedido nessa conversa.

## Estado de integração

Implementado e validado nesta conversa em 2026-10-03: pacote `releases/interactions-v102`. A seção v101 acima fica como evidência histórica; as regras militares passam a ser substituídas após 60 minutos pela implementação abaixo.


## Implementação v102 — ondas após 60 minutos


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
D:\CHATGPT\outputs\RisingLandsResearch\releases\interactions-v102\Jogar-Quarta-Missao.cmd

Com o monitor existente:
D:\CHATGPT\outputs\RisingLandsResearch\releases\interactions-v102\Jogar-Quarta-Missao-Com-Monitor.cmd

CONFIRMADO na execução do remake: formação, ataque com dano, crescimento controlado e persistência nos cenários acima. Não houve partida manual completa até vitória/derrota, nem prova de conclusão para toda construção de mapa/save possível. Caminhos bloqueados, suprimento, natalidade e capacidade finita de população/recursos permanecem condições reais; não receberam teletransporte ou concessões. Música original ausente, magia da IA e fidelidade integral do remake continuam pendências gerais anteriores. O modo experimental da IA original para pesquisa não foi alterado; o pacote jogável usa a política do remake.

## Defesa coordenada v115 — 2026-10-09
Base comum dos três clãs: avaliação de ameaça/força, resposta a ataques em prédios, setores, guarnição/reserva, perseguição limitada, recrutamento e apoio legal por proprietário. Perfis preservados; fortificações amarelas limitadas e pagas. Emergência pode recolher membros de ondas e impedir novas saídas enquanto faltar defesa. Formato40 importa anteriores; slots v33 e 21 arquivos reais preservados. 59 específicos, 71 poderes, 89 combate, 47 abertura, 21 comércio, 10 planejamento, 35 conclusão e cinco slots passaram. Medida engine com 57/147 personagens, sem teste manual de dificuldade/FPS. Pacote interactions-v115; v114 e backup preservados. Relatório reports/defesa_bases_v115.md; evidências experiments/base-defense-v115. As regras econômicas anteriores continuam sujeitas aos gargalos documentados; não se declara fidelidade da IA original.

## Ofensiva verde v116 — 2026-10-09
Base de personalidade ampliada: produção/apoio/ataques fortes, maior grupo, seleção por poder de cerco e defesa conservada. Alianças inalteradas. Regra de pressão tardia específica da missão4 preservada. Formato40, slotsv33 e saves reais preservados; 21/40/59/10/47/35 checks e cinco slots passaram. Ver reports/ofensiva_verde_v116.md; sem aprovação manual de equilíbrio. Pacote interactions-v116.

## Visão aliada v117 — 2026-10-09
Base comum: visão recíproca direta, memória após ruptura, inimigos conhecem terreno/recursos iniciais mas observam unidades/prédios novos. Conhecimento privado por clã e patrulhas sem visão atual. Formato41/slotsv33; cinco slots preservados. Missão6 continua sem adversários. Ver reports/visao_aliada_v117.md e documentation/interactions_v117.md para 443 checks, capturas, migração e limites. Pacote interactions-v117; atalhos1–6 atualizados, v116 preservada. Sem partida manual completa.

