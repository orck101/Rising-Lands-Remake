# v54 — zoom sem área vazia fora do mapa

Abrir `releases/interactions-v54/Jogar-Primeira-Missao.cmd`.

O zoom mínimo agora é calculado por mapa e tamanho da área de jogo: o maior entre 0,5, largura da área / largura do mapa em pixels e altura da área / altura do mapa em pixels. O limite é arredondado para cima em precisão float para não expor uma coluna por arredondamento. Se um mapa excepcionalmente pequeno exigir zoom acima do máximo normal de 2, o máximo também é elevado até o mínimo necessário.

A rodinha continua aproximando/afastando o cenário e preserva o ponto sob o mouse quando os limites do mapa permitem. Afastar mais que o mínimo deixa o zoom no limite seguro. A posição da câmera continua limitada ao cenário. A correção do zoom é aplicada ao carregar mapas, navegar, atualizar e desenhar, incluindo janela redimensionada, tela cheia, pausa, menus e vitória. Se o limite precisar subir, o ajuste usa o centro da área de jogo como referência.

**CONFIRMADO no remake:** o limite fixo 0,5 permitia que a imagem do mapa ficasse menor que a área de jogo. `ClampCamera` limitava apenas a posição e não aumentava o zoom. O teste inicial reproduziu a área vazia em MONDE01.ETA na janela 1280 × 800 (`logs/v54-zoom-before.err`). Corrigidos os dois pontos pelo cálculo compartilhado `ViewportZoom.Clamp`.

Os quadrados de terreno passam a arredondar o tamanho desenhado para cima, evitando frestas entre eles em zoom fracionário. A alteração não modifica sprites, mapas, colisões ou regras de gameplay.

## Verificação

- 782 verificações de missão/interações passaram (`logs/v54-mission.log`).
- O teste de zoom cobre 1.224 combinações: todos os 34 mapas ETA disponíveis, seis janelas (800 × 600 a 5120 × 2880, incluindo ultrawide) e seis valores de zoom solicitados. Em todos, o mapa é pelo menos tão largo e alto quanto a área de jogo.
- Caso adicional: mapa sintético pequeno exige zoom acima de 2 sem lançar erro ou expor área vazia.
- Capturas conferidas das missões 1 e 2 após 100 solicitações de afastamento e câmera no canto inferior direito: `reports/v54-MONDE01.ETA-zoom.png` e `reports/v54-MONDE02.ETA-zoom.png`.
- Navegação, seleção por classe, alcance e capacidade do armazém permanecem preservados. Save continua v20; originais e saves originais não foram modificados.

Esta é uma adaptação visual solicitada pelo jogador. As demais pendências do projeto continuam registradas nas versões anteriores.
