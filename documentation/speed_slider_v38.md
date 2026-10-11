# Velocidade — v38

Entrega: releases/speed-slider-v38/Jogar-Primeira-Missao.cmd.

Slider no canto inferior direito, cinco posições1x..5x, padrão1x. Clique ou arraste. Soltar fora da barra conclui arraste sem ordem ao mapa. Velocidade altera somente passos de simulação por atualização; câmera e interface mantêm cadência. Coleta, transporte, construção, combate e pesquisa aceleram juntos. Pausa, menu de pausa e vitória continuam bloqueando simulação; loop interrompe imediatamente ao vencer. Pesquisa aberta usa velocidade selecionada. Preferência vale nesta sessão, não altera versão de save ou originais.

Implementação: Engine/GameSpeed.cs e ResearchGame.cs. Verificação de captura --capture-menu speed: extremos1/5,5x produz estado idêntico a cinco passos normais, pausa não avança. Captura reports/v38-speed.png revisada visualmente. Publicação e suite completa da primeira missão aprovadas. Controle é adição solicitada pelo usuário, não atribuído ao jogo original. Sem teste físico manual.
