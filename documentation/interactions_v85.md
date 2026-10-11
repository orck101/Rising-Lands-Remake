# v85 — remoção do carro inicial inimigo

Alteração de balanceamento explicitamente solicitada pelo jogador, distinta do mapa original. MONDE04 contém um carro de alimento do jogador 1; MONDE03 não contém inimigos. O carro inicial da missão 4 foi retirado de jogo. Não foi substituído por trabalhador, nem concedida compensação de recursos.

EnemyStartingBalance.RemoveEnemyStartingCart mantém o identificador original do ator inativo (vida zero), preservando a identidade exigida pela validação de saves. Não aparece, movimenta ou coleta. A regra é aplicada na criação, no reinício e após carregar um save, impedindo que a unidade inicial reapareça. Carga já existente no registro de um save antigo permanece contabilizada e inativa, sem crédito gratuito ao estoque. Não há alterações nos arquivos originais ou nos saves originais.

O filtro afeta exclusivamente o slot inicial de carro inimigo da missão 4. Carros novos, treinados legitimamente a partir de colonos após a tecnologia correspondente, continuam permitidos. A regra de desbloqueio da v84 permanece.

Validação: oito verificações v85 (nova partida, restart, save antigo, conservação e entregas reais); 88 verificações v84; 28 v75 e testes de slots; seis v80, incluindo desenvolvimento por 24 minutos e retomada determinística. Nesta simulação, treinamentos militares acumulados passaram de 9/11/16 na v84 para 7/9/13 aos 6/12/24 minutos, mantendo os demais ajustes iguais. Isso indica redução da economia, não uma medida completa de dificuldade em partida humana.

Relatórios: reports/v85-validation-0.txt até v85-validation-3.txt. Publicação: releases/interactions-v85.
