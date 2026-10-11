# Missão 6 — MONDE06.ETA

Atualizado: 2026-10-04. Entrega: releases/interactions-v107.

## Resultado e regra original

Fase de exploração, sem clãs adversários. Objetivo em português: **Explore a mina para obter conhecimento espiritual.** Preserva o mapa original 256×128, 11 registros de personagens do jogador (incluindo colonos internos), santuário, 20 alimentos e 20 pedras. Não acrescenta inimigos, ondas, laboratórios obrigatórios ou extração da mina.

**CONFIRMADO:** o ramo especial da missão 6 no executável original exige visão atual da célula (225,59), na entrada da mina abandonada situada em (225,58), com quantidade zero. Não exige destruir adversários. Terreno apenas lembrado não conclui a fase. O teste geral de sobreviventes ainda pode produzir derrota.

Fontes: cópia working/RISING/CD/WORLD/MONDE06.ETA, retângulo de influência offset 336680; executável copiado RISING-ORIG.EXE, TestFinDeMission 0x4585d2, ramo 0x4586c7..0x4586ed, CaseEstIlluminee 0x476866→0x476614. Fog bit 0x10 apagado indica visão atual. experiments/missao06-v107/original_mission06.json: 64 combinações de visibilidade/sobreviventes executadas nativamente em memória. Texto CD/WORLD/MONDE06.TXT.

## Implementação

Engine/MissionSix.cs: objetivo, derrota, navegação diagonal específica da fase, validação de vitória nos saves pela visão reconstruída do estado salvo. Engine/MissionOne.cs conecta essa política ao avaliador comum. Engine/Simulation.cs interrompe a partida encerrada e usa a conectividade diagonal nesta fase. ResearchGame.cs exibe derrota. CampaignChoiceMenu.cs informa a descoberta na transição simples existente para a missão 7.

A confirmação da vitória exige 100 avaliações consecutivas enquanto o alvo permanece visível. **PROVISÓRIO:** 100 ticks modernos (aproximadamente 3,33 segundos a 1x) conservam a confirmação já usada pelo remake; o tempo exato de relógio do original não foi medido.

O caminho original tem passagens naturais conectadas diagonalmente. A busca anterior, com quatro vizinhos, isolava o objetivo. O cálculo de vizinhos original 0x47701a foi executado para 16 direções e confirma as quatro diagonais. A política nova permanece exclusiva da missão 6: respeita o bit de colisão do terreno, árvores, prédios, muros e portões; não corta cantos de obras/muros. **FORTE EVIDÊNCIA** para a conectividade necessária; equivalência integral do algoritmo original de caminhos continua desconhecida.

A transição 6→7 preserva a campanha e aplica o piso tecnológico nativo da missão 7, incluindo os quatro desbloqueios espirituais iniciais presentes em build_unlocks.json. Não concede uma pesquisa extra fictícia na missão 6.

## Verificação

- 40 verificações específicas passaram no código e no executável publicado. Percurso físico via Order/Step, sem teleporte ou concessão de recursos, concluído em 8339 ticks (277,97 segundos simulados); oito checkpoints e continuação determinística após carregar.
- Vitória falsa sem visão rejeitada pelo motivo correto e sem alterar a partida. Visão perdida reinicia confirmação; confirmação parcial salva/carrega e termina corretamente.
- Derrota, reinício, ausência de adversários, visão versus exploração lembrada, transição 6→7, paredes nos cantos e rota diagonal validados.
- 54 verificações de rampas nos 34 mapas, 23 de muros/portões/voo e 16 de missão 3 passaram. Saves reais carregados em leitura; 21 arquivos preservados por SHA256.
- Capturas reais da interface: abertura a 1x/5x, mina e tela de conclusão. Não se trata de uma partida manual jogada por mouse/teclado.
- Release: simulação média 0,159 ms/tick; p95 0,206 ms/tick. Interface em 5x: atualização média 0,709 ms, desenho médio 3,877 ms, aproximadamente 4,59 ms de CPU/quadro. p95 separados: 1,815 e 7,422 ms. Tem margem para a meta existente de 30 quadros/s; medições não incluem latência de GPU nem garantem todos os cenários tardios.

## Entrega e limites

Abrir: D:\CHATGPT\Jogar-Sexta-Missao.cmd.
Executável: D:\CHATGPT\outputs\RisingLandsResearch\releases\interactions-v107\RisingLandsRemake.exe.
Evidências: D:\CHATGPT\outputs\RisingLandsResearch\experiments\missao06-v107.
Relatório: reports/missao06_v107.md.

A missão 6 tem início direto, objetivo e encerramento implementados. O carregamento do mapa 7 foi verificado; a jogabilidade completa da missão 7 não foi reconstruída nesta tarefa. A conclusão normal da missão 5 ainda não está no avaliador comum: a ligação natural 5→6 continua pendente na conversa dessa missão. Não declarar campanha inteira concluída. A descoberta por um jogador iniciante e uma sessão longa com base muito expandida ainda precisam de avaliação humana.

## Fronteira da defesa v115 — 2026-10-09
Pacote interactions-v115 atualiza os atalhos comuns; teste específico preserva missão6 sem adversários e sem estado de guarnição. Objetivo de exploração não foi alterado. Documentação interactions_v115.md.

## Visão aliada v117 — 2026-10-09
Base comum: visão recíproca direta, memória após ruptura, inimigos conhecem terreno/recursos iniciais mas observam unidades/prédios novos. Conhecimento privado por clã e patrulhas sem visão atual. Formato41/slotsv33; cinco slots preservados. Missão6 continua sem adversários. Ver reports/visao_aliada_v117.md e documentation/interactions_v117.md para 443 checks, capturas, migração e limites. Pacote interactions-v117; atalhos1–6 atualizados, v116 preservada. Sem partida manual completa.


## Continuidade v122 — 2026-10-10
Missão7 agora jogável e testada em oito rotas simuladas. Missão6 regressão40 checks passou; atalho normal usa v122. Pendências históricas sobre missão7 acima foram substituídas por documentation/missoes/MISSAO_07.md. Missão8 completa segue sem aprovação.
