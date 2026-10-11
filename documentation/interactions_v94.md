# v94 — três colonos no santuário e dois fazendeiros

Abra `releases/interactions-v94/Jogar-Quarta-Missao-Com-Monitor.cmd`. Inicie ou reinicie a missão 4 na v94 para obter o novo início. Carregar uma partida antiga conserva seus próprios personagens e operadores.

## Configuração inicial do vermelho

- Santuário completo com três colonos dentro.
- Cultivo completo com um colono dentro.
- Armazém completo com um colono dentro.
- Dois colonos livres na saída do armazém.
- Dois fazendeiros e um construtor.
- Total: sete colonos, dois fazendeiros e um construtor, dez personagens vivos.
- 15 alimento, 15 pedra, zero engrenagens; nenhum soldado, arqueiro ou carro inicial.

Mercado e bolsa de trocas permanecem a primeira e a segunda obra nova. Os três colonos são a ocupação inicial do santuário: depois a IA pode treinar trabalhadores ou transferir colonos segundo suas necessidades. Não mantém artificialmente três residentes ao longo da partida. O segundo fazendeiro e os colonos adicionais usam personagens nativos existentes no mapa, sem duplicar identidades.

A área conhecida inicial continua exatamente triplicada. Com a nova população, a área normal inicial do vermelho soma 645 casas e passa a 1.935 casas conhecidas. O alcance normal de visão permanece. Configuração e IA da missão 5 não foram alteradas nesta versão.

No teste sem intervenção, o primeiro soldado treinado surgiu em 55.501 ticks: aproximadamente 30,8 minutos em 1x ou 6,2 minutos em 5x, contra 47,4 minutos na v93. O ritmo ainda depende da coleta, nascimento de colonos, custos e prioridades de desenvolvimento; não foram concedidos recursos ocultos.

## Evidências e testes

Alteração de balanceamento solicitada pelo jogador, implementada em `Engine/EnemyStartingBalance.cs`. As referências nativas originais de atores/prédios permanecem para validar identidades de saves. Somente o remake, seus testes e relatórios foram alterados.

- `reports/v94-build.txt`: publicação Windows autocontida.
- `reports/v94-test-0.txt`: 13 verificações — ocupação 3/1/1, dois colonos livres, dois fazendeiros, um construtor, recursos, salvamento, mercado/bolsa, nascimento de colonos, quartel, recrutamento com custos, ataques, alianças e reinício.
- `reports/v94-test-1.txt`: 25 verificações do monitor, incluindo dez personagens e três prédios na abertura.
- `reports/v94-test-2.txt`: 27 verificações de área inicial exata, visão normal, descoberta do jogador, missões 4/5 e salvamento/reinício.
- `reports/v94-old-save-tests.txt`: 9 verificações de compatibilidade e conservação das partidas históricas.

Os 74 testes passaram. Arquivos e saves originais não foram modificados.
