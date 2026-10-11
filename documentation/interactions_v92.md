# v92 — base inicial funcional do vermelho na missão 4

## Abrir

`releases/interactions-v92/Jogar-Quarta-Missao-Com-Monitor.cmd` abre a nova missão 4 e o monitor independente. Para obter a configuração nova, inicie ou reinicie a missão 4 na v92. Carregar uma partida anterior preserva a população, economia e construções salvas.

## Início exato

- Santuário, cultivo e armazém originais completos, nas posições nativas, ativos com um colono em cada prédio.
- Um fazendeiro e um construtor vivos.
- Dois colonos adicionais livres, colocados na saída do armazém.
- Portanto, cinco colonos no total: três operadores internos e dois livres; sete personagens vivos contando os dois trabalhadores.
- 15 alimento, 15 pedra, zero engrenagens.
- Nenhum soldado, arqueiro ou carro inicial do vermelho. O quartel nativo não faz parte da base inicial.

Mercado e bolsa de trocas continuam sendo a primeira e a segunda obra nova. A IA pode produzir trabalhadores pelo santuário e novos colonos pelo cultivo, usando seus recursos reais. Depois constrói quartel, treina soldados e ataca se não houver aliança. Não foram alterados os recursos iniciais nem a lógica da missão 5. Reparos, animações de dano/desmoronamento, defesa e monitor da v91 permanecem.

## Resultado medido e limites

No teste determinístico sem ordens do jogador, o primeiro soldado treinado apareceu em 84.939 ticks: aproximadamente 47,2 minutos em 1x ou 9,4 minutos em 5x. A abertura da v91 levava 230.139 ticks, aproximadamente 127,9 minutos em 1x. Redução de aproximadamente 63% nesse cenário.

Isso comprova que a economia progride com a nova base, mas ainda não estabelece um ritmo ideal de dificuldade. Mercado, bolsa, quartel, alimentação e formação de trabalhadores continuam disputando os pequenos estoques iniciais. Não foi criado um bônus invisível de recursos nem modificado o tempo de coleta para esconder o custo da expansão.

## Evidência e preservação

Configuração intencional de balanceamento solicitada pelo jogador, não conclusão sobre a abertura exata do original. `Engine/EnemyStartingBalance.cs` preserva os três prédios nativos do proprietário 1 (tipos 2, 3 e 4), distribui cinco colonos nativos e mantém somente um trabalhador de cada classe solicitada. Dados originais de atores/prédios permanecem como referência de identidade para validar saves. Os demais personagens nativos são aposentados na simulação; seus arquivos originais não são alterados.

A alteração ocorre na criação da simulação. Carregar saves restaura seus próprios registros de prédios, operadores e personagens, sem aplicar novamente o novo início por cima de uma partida existente. O monitor lê a população real, agora sete personagens e três prédios na abertura.

## Verificações

- `reports/v92-build.txt`: compilação/publicação Windows autocontida.
- `reports/v92-test-0.txt`: 13 verificações — prédios completos/ativos, três operadores e dois colonos livres, trabalhadores e estoques, save/load, obras na ordem pedida, nascimentos, quartel, tropa com custos reais, desenvolvimento salvo, ataque, aliança e reinício.
- `reports/v92-test-1.txt`: 25 verificações do monitor, incluindo sete personagens e três prédios reais na abertura.
- `reports/v92-test-2.txt`: 36 verificações de reparos, cooperação, defesa e desenvolvimento dos três inimigos da missão 5.
- `reports/v92-regression-0.txt`: compatibilidade e conservação de saves históricos, incluindo correção de recrutamento de arqueiros proibidos.
- `reports/v92-regression-1.txt`: pesquisa, vitória da missão 3 e transição para a missão 4 sem carro inicial.

Nenhum arquivo do jogo original ou save original foi modificado.
