# Missão 9 — MONDE09A/B/C/D.ETA

Estado em 2026-10-10: base jogável, duas descobertas e passagem para 10 implementadas. 16 rotas, 13 vitórias/3 derrotas na A. Balanceamento específico adiado; tecnologias avançadas ainda têm lacunas e confirmação de ramo inválido na saída da 8 é bloqueada com aviso.

Pacote: releases/interactions-v125-final. Atalho padrão D:\CHATGPT\Jogar-Nona-Missao.cmd, variante9A; campanha escolhe A/B/C/D conforme quantidade de clãs rivais estabelecidos na missão8 (0/1/2/3). Cada variante inclui três adversários e abertura própria.

Objetivo nativo confirmado: duas descobertas. Obras, laboratório vazio e níveis herdados não contam como novas pesquisas. Vitória com sobreviventes e duas conclusões, limite2 sem terceira paga. Derrota por ausência de sobreviventes. Próximo arquivo10A/B/C: menor simpatia com vermelho/amarelo/verde; primeiro dessa ordem vence empate.

Implementação: Engine/MissionNine.cs, MissionOne.cs, MissionSix.cs, MissionResearchFlow.cs, CampaignChoiceMenu.cs, ResearchGame.cs, PerformanceProfile.cs, MissionNineCapture.cs, Tests/MissionNineTests.cs e MissionNineProgressionTests.cs. Salvar mantém formato44 e importa anteriores.

Preservar pisos tecnológicos por número da missão e níveis herdados; verde+2 não acumula. Bônus de dano militar herdado passou a usar TechnologyApplied. Proteção de ramos indisponíveis não pode ser substituída por pesquisas gratuitas/pular efeitos.

Não refazer análise completa sem dúvida concreta: provas nativas em experiments/mission09-v125/native_mission09.json e experiments/mission08-v123/native_next_map.json. Fonte readonly em working/RISING/CD/WORLD; seção mission_end nos offsets371976/376526/376652/377948. Relação e testes completos em reports/missao09_v125.md.

Próximo trabalho: reconstruir efeitos avançados (MILITAIRE5/GENIECIVIL10/RELIGIEUX7 e subsequentes), avaliar pressão da9A/defesa/reforços, preparar adversários e ampliar cobertura de interface/caminhos/desempenho. A10 carrega na transição, mas não foi preparada integralmente nesta entrega. Não alterar UserSaves em testes.
