# v63 — menu de testes

Versão: releases/interactions-v63. Iniciadores da primeira, segunda e terceira missão. A v62 permanece disponível. Clique em **Testes**, acima da barra de velocidade, durante a partida. Esc, X, botão Testes ou clique fora fecha o painel. O jogo fica pausado enquanto o painel está aberto; a pausa anterior e a velocidade são preservadas.

## Comandos

- +50 comida; +50 pedra; +50 engrenagem: acrescentam ao estoque utilizável do jogador 0. Podem ser repetidos. Não dependem da lotação do armazém; são concessões explícitas para testes. O resumo dos armazéns mantém a contagem real e pode indicar superlotação após uma concessão. Não aumenta artificialmente a capacidade física. O comando fica indisponível antes de ultrapassar o limite numérico do recurso.
- +1 nível de Agricultura, Engenharia, Armamento ou Espiritualidade: aumenta exatamente um nível do ramo e concede seus desbloqueios e efeitos já implementados. Mostra o nível atual e fica indisponível no máximo. Não muda o ramo escolhido nem consome as duas pesquisas normais ou a terceira paga. O progresso parcial da tecnologia que foi concedida é zerado, pois ela foi superada pelo teste; a pausa manual permanece. As descobertas de teste não tocam uma animação de conclusão de pesquisa normal.
- Mostrar/Esconder mapa: a opção anterior foi transferida para este painel. É somente uma revelação visual; não altera a exploração, a visão persistida ou o histórico da partida. Continua sem ser persistida no save, como antes.
- Passar de fase: registra uma conclusão de teste e considera os requisitos satisfeitos para abrir a tela normal **Você cumpriu a missão**. Não constrói prédios falsos, não ocupa casas e não move automaticamente para o próximo mapa. O botão Proxima missão abre a transição habitual de escolha de tecnologia. A marca de conclusão de teste é removida ao iniciar a próxima missão ou reiniciar.

## Salvamento

Save v26 acrescenta `Tests`: totais de recursos concedidos, chaves de tecnologias concedidas e conclusão forçada. As validações de conservação continuam estritas: a comida/pedra/engrenagem concedida é contabilizada, e incrementos sem origem válida continuam sendo rejeitados. As tecnologias de teste são validadas junto das pesquisas normais, sem consumir sua quota. O formato lê v25 e anteriores sem reescrever os arquivos existentes. Novos saves usam `campaign.slot-N.research-v26.json`; os cinco slots e a confirmação de sobrescrita continuam presentes.

Reiniciar remove todas as concessões da missão atual. Tecnologias levadas na transição passam a integrar os níveis herdados da nova missão; seus registros de concessão anteriores não são duplicados. Os recursos da próxima fase seguem o carregamento normal do mapa.

## Validação

1887 verificações aprovadas, incluindo 64 do novo menu. Cobertura: +50 de cada recurso, repetição, conservação, save/load, quatro ramos, níveis máximos, pesquisa normal intercalada com concessões, rejeição de dados inválidos sem alterar a partida atual, reinício e vitória forçada em missões 1–3. Vitória no instante inicial pode ser salva corretamente. Os cinco saves existentes foram lidos e preservados. Regressões de missões, coleta, pesquisa, cultivo, descoberta, ambiente, formações e salvamento aprovadas.

Capturas usam os mesmos manipuladores de clique do jogo: abrir botão, pausar atualizações, conceder os sete incrementos, mostrar/esconder mapa, fechar com Esc e concluir missão. A saída de missão 3 para missão 4 pela transição habitual foi conferida. Capturas: reports/v63-menu-final.png, reports/v63-button-final.png, reports/v63-victory-final.png, reports/v63-next-mission.png e reports/v63-map-toggle-final.png. Compilação Windows x64 autocontida.

## Limites

Este é um painel de teste do remake, não uma interface extraída do original. Não acrescenta efeitos de tecnologia ainda ausentes na implementação. Concluir missão por teste não implementa os objetivos reais das missões além daquelas já reconstruídas; permite verificar a navegação de campanha pelos mapas disponíveis. Os níveis máximos seguem o catálogo extraído existente. Não houve alteração nos originais de D:/RISING nem nos saves reais do usuário.
