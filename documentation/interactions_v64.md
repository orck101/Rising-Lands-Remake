# v64 — contorno combinado do alcance de ataque

Versão: releases/interactions-v64. Iniciadores das três missões. F9 permite carregar os cinco slots anteriores. Save continua v26, sem migração ou alteração dos arquivos reais. A v63 permanece disponível.

## Alteração

A seleção de tropas próximas agora mostra somente os trechos expostos do contorno da área coberta pelos seus alcances. Não há círculos completos cruzando o interior do grupo. Tropas ou grupos separados mantêm contornos independentes. Círculos totalmente contidos em outros e círculos coincidentes são eliminados. Uma tropa isolada continua com um círculo inteiro.

O cálculo utiliza o centro e o alcance individual já usado pelo indicador. Não cria círculo envolvente nem aumenta o alcance para cobrir intervalos entre unidades. Os pontinhos continuam com 2×2 pixels, cor (237,220,167,170), espaçamento aproximado de oito pixels, ajustado ao zoom. A borda respeita a área de jogo e a descoberta do mapa existente. Nenhuma regra de dano, distância efetiva do ataque, seleção, formação ou save foi alterada.

A geometria é mantida em memória enquanto os personagens selecionados conservam suas posições e alcances. Mudança de posição, alcance ou seleção refaz os arcos. O zoom e a câmera reprojetam os pontos usando o contorno no mundo. Isso evita repetir a comparação entre todos os alcances quando a seleção está parada.

## Causa e método

**CONFIRMADO — código do remake:** Engine/RangeSelection.cs, DrawAttackRanges, desenhava cada circunferência inteira. A imagem enviada pelo usuário mostrava esse acúmulo em uma formação grande. Uma reprodução matemática com dois círculos de raio 10 separados por distância 10 reprovou antes da alteração: o perímetro desenhado incluía os dois trechos internos, conforme registrado em logs/v64-range-before.log.

Engine/RangeOutline.cs calcula os intervalos angulares de cada circunferência cobertos pelas demais, une intervalos sobrepostos e retorna apenas seu complemento. Trata passagem pelo ângulo zero, contenção, tangência, círculos idênticos e cobertura por vários vizinhos. O resultado representa a fronteira da união dos círculos, sem extrapolar sua área. O desenho continua sendo um indicador visual modernizado solicitado pelo usuário; não é apresentado como comportamento extraído do executável original.

## Validação

2192 verificações aprovadas: 1835 de geometria, 62 de seleção de alcance e 295 de regressões de formações/coleta/cultivo/salvamento. Muitos itens de geometria são pontos amostrados ao longo dos arcos; não são partidas independentes. Casos cobertos: tropa isolada, círculos separados, interseção parcial, alcances diferentes, círculo contido, tangência interna/externa, coincidência, grupo de 36 unidades, cobertura por vários vizinhos, ordem de seleção, translação, escala e 12 grupos aleatórios. Cada ponto amostrado do contorno ficou fora do interior de todos os outros alcances.

Capturas do jogo final verificaram 36 tropas com somente a borda exposta, duas tropas separadas, uma tropa deslocada de um grupo, uma seleção individual com zoom de 1,5×. Os auxiliares de captura também verificaram reutilização da geometria quando parada, invalidação após deslocamento e remoção após desmarcar uma unidade. Os personagens adicionados para essas capturas pertencem exclusivamente a simulações temporárias de teste.

Evidências visuais: reports/v64-range-dense-final.png, reports/v64-range-separated-final.png, reports/v64-range-moved-final.png e reports/v64-range-single-zoom.png. Compilação Windows x64 autocontida. Não houve escrita nos originais D:/RISING nem nos saves reais.
