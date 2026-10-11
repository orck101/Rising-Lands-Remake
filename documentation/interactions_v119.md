# Observação sem derrota — v119

2026-10-09. Alteração temporária solicitada pelo usuário, exclusiva da missão 5.

A extinção do jogador continua sendo reconhecida pelo classificador de clãs, mas não dispara derrota nem interrompe a simulação da missão 5. Unidades continuam podendo sofrer dano, fome e morrer. Não há imortalidade, recursos concedidos ou alteração de visão. Os adversários continuam sua economia, pesquisa, defesa e ofensiva normalmente.

Vitória e eliminação dos adversários permanecem com as regras anteriores. Um jogador eliminado não recebe vitória automática. A missão 6 conserva sua derrota por ausência de sobreviventes.

Ao carregar uma partida da missão 5 sem sobreviventes nesta versão, a simulação pode continuar. Não há alteração de formato: 41; nomes dos slots v33 mantidos. Para reativar a derrota futuramente, mudar FifthPlayerDefeatEnabled para true em Engine/MissionFive.cs e publicar uma nova entrega.

Arquivos alterados: Engine/MissionFive.cs, Engine/MissionSix.cs e Tests/MissionFiveEndingTests.cs. Backup: backups/antes-observacao-v119-20261009.

Verificação: 36 checks da missão 5 e 40 da missão 6. Inclui 300 passos após extinção, avanço da economia inimiga, persistência/save-load, continuação determinística, vitória diplomática/militar/mista, reinício e derrota preservada na missão 6. Testes de engine; não foi realizada partida manual pela interface.

Pacote: releases/interactions-v119. Somente o atalho Jogar-Quinta-Missao.cmd foi atualizado; atalho anterior v118 preservado. Uma partida já aberta não muda de versão automaticamente.
