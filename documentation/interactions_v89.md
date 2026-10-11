# v89 — monitor separado dos inimigos

## Como abrir
- `releases/interactions-v89/Jogar-Quarta-Missao-Com-Monitor.cmd`: abre o remake v89 na missão 4 e o monitor.
- `releases/interactions-v89/Abrir-Monitor.cmd`: abre somente o monitor; serve também para acompanhar uma partida já aberta na v89.
- `releases/enemy-monitor-v89/RisingLandsEnemyMonitor.exe`: programa independente, sem instalação adicional.

Use o remake v89 ou posterior. A v88 não transmite dados ao monitor. Para continuar uma partida, abra a v89, carregue normalmente pelo menu e abra o monitor. Abrir o monitor não reinicia nem altera o jogo. O atalho combinado inicia uma nova missão 4.

## O que mostra
Economia: três inimigos separados, estoque real disponível de comida, pedra e engrenagens; personagens vivos, inclusive colonos em prédios; estruturas ativas. Um clã ausente fica identificado e usa traços em vez de números.
Tecnologia: níveis efetivos de Agricultura, Engenharia, Armamento e Espiritualidade, comparados ao máximo de cada ramo. Selecione um inimigo para ler as melhorias já liberadas, o ramo escolhido, progresso, número de colonos no laboratório e descobertas concluídas na missão.
As melhorias liberadas incluem os níveis iniciais e os herdados de missões anteriores. Não significam que todas foram pesquisadas durante a missão atual. O limite indicado é o limite atual da pesquisa inimiga do remake (duas descobertas), não o limite de pesquisas extras do jogador.
Conexão: missão, tempo de simulação, velocidade e estado. Sem atualização por quatro segundos, mostra "Sem conexão" e identifica o horário do último retrato; não apresenta dados antigos como atuais. Havendo mais de uma sessão ativa, a caixa Partida permite selecionar a desejada. Reconecta automaticamente a uma sessão viva quando a anterior encerra.

## Implementação e preservação
Somente leitura do lado do monitor. O jogo transmite um retrato leve a cada 500 ms de tempo real, inclusive pausado ou em segundo plano. Não depende de salvar a partida nem lê a memória do executável original.
Fonte dos estoques: World.Resources[owner], [4+owner] e [8+owner]. Fonte dos níveis: OpponentLevel(owner, branch), que combina níveis nativos e campanha. Fonte de melhorias: catálogo build_unlocks, índice inferior ao nível efetivo. Fonte de pesquisa: Campaign.EnemyBranches, Opponent.ResearchProgress, ResearchCompleted e EnemyScientists.
Arquivos temporários em `runtime/enemy-monitor/game-PID.json`, substituídos atomicamente para impedir leituras incompletas. O fechamento normal publica Running=false; fechamento inesperado é detectado pela falta de atualização. Arquivos originais e saves do usuário não são modificados pelo monitor.
Código: source_remake/EnemyMonitor e Engine/EnemyMonitorFeed.cs; modelo compartilhado MonitorModel.cs. Windows Forms com .NET 8 autocontido, executado sem privilégios administrativos.

## Validação
- reports/v89-monitor-tests.txt: 25 verificações; recursos reais, três slots, todos os ramos e seus níveis, catálogo desenvolvido, inimigos ausentes, não alteração da simulação, escrita atômica, ritmo/pausa e dados dos três clãs sem mistura.
- reports/v89-live-ui-check.txt: interface confere valores emitidos pelo jogo real em captura controlada. Após 1500 ticks da missão 4, vermelho com 5 comida, 9 pedra e zero engrenagens (estoque após construção).
- reports/v89-three-ui-check.txt: interface validada com fixture sintética de três clãs; não confundir esses valores com uma partida real.
- reports/v89-opening-regression.txt: nove verificações do novo começo da missão 4, preservado.
- reports/v89-monitor-economy.png e v89-monitor-technology.png: interface inspecionada visualmente, sem recortar a terceira linha. Capturas utilizam uma fixture do começo real da missão 4; o estado Sem conexão nas imagens comprova a identificação de dados antigos.
- reports/v89-build.txt, v89-monitor-build.txt: publicações aprovadas.
