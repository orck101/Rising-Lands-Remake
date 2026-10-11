# v41 — obra limpa e indicador original de colonos

Versão: `releases/interactions-v41/Jogar-Primeira-Missao.cmd`.

## Entrega

Removidos o contorno amarelo da obra, sua barra de progresso, o nome junto da porcentagem e a caixa atrás do texto. Permanece a porcentagem, junto dos estágios originais e dos golpes de martelo já implementados. A prévia verde/vermelha antes de confirmar a posição da obra continua sendo uma indicação de posicionamento.

O prédio selecionado ou sob o mouse mostra seus lugares com as sprites originais: lugar ocupado `0x2a3c`, vazio `0x2a3b`, extraídas de `GRAPHICS/OTHERS.CMP`. Não foram desenhados ícones novos nem acrescentada uma contagem de colonos em texto. A grade completa representa a capacidade; os lugares preenchidos representam os colonos presentes. O santuário inicial tem oito preenchidos e oito vazios; o armazém tem dois lugares. Tamanho e transparência dos assets preservados, com o mesmo zoom do cenário e amostragem por pixel.

A indicação é calculada a cada desenho a partir dos colonos vivos, ocultos dentro do prédio e associados a ele. Inclui colonos reservados para treinamento e cientistas no laboratório. Caminhar para um prédio ainda não conta como estar dentro. Entrada, retirada e restauração do save atualizam a indicação. A validação de entrada também considera cientistas ocupando a capacidade e viagens reservadas para o laboratório.

## Evidências e certeza

- CONFIRMADO: `AffContenanceBatiment`, endereço `0x401a70`, escolhe sprites ocupado/vazio pelo conteúdo da bolsa; passo vertical de oito pixels; após oito lugares, nova coluna nove pixels à esquerda.
- CONFIRMADO: `AffJaugeBatiment`, `0x4020ec`, aplica deslocamentos por tipo e chama a rotina de conteúdo em `0x402263`.
- CONFIRMADO: condição de seleção/mouse/propriedade em `AffBatiments`, região `0x40f3eb–0x40f442`. No remake a condição também vale para prédio vazio, atendendo ao pedido explícito de mostrar os lugares vazios.
- CONFIRMADO SOB CONDIÇÕES CONTROLADAS: 56 vetores de desenho cobrindo ocupação zero, parcial e completa, em 19 tipos com capacidade positiva. Bolsa e colonos fornecidos explicitamente à emulação isolada. Hospital: colonos saudáveis, sem pacientes. Três tipos sem alojamento não recebem símbolos fictícios.
- CONFIRMADO SOB CONDIÇÕES CONTROLADAS: pontos de ancoragem obtidos executando `PlaceBatiment` e o desenho nativo da área de cada tipo no mapa de referência. Efeito cosmético aleatório incidental controlado para não invocar serviços externos.
- ADAPTAÇÃO EXPLÍCITA: no tipo 8 o original separa um marcador de ativação e lugares horizontais para ocupantes mistos. Mantivemos o desenho e sua disposição, com sete lugares adicionais para a capacidade atual de oito no remake; o marcador fica vazio quando não há colono, conforme solicitado. Isso não confirma toda a lógica de veículos do original.

Reprodução: `tools/trace_building_residents_v41.py`. Resultados com chamadas, coordenadas e hash do executável em `reverse_engineering/building_residents_v41.json`; catálogo utilizado em `source_remake/Data/building_resident_visuals.json`.

## Verificação

Publicação Windows x64 concluída. Primeira missão: **489 PASS**; regressão geral: **351 PASS**. Comparação das chamadas do indicador com o catálogo nativo, treinamento sem retirada antecipada, entrada real, retirada real, vazio, lotado, save/load e cientistas incluídos na ocupação.

Capturas inspecionadas: santuário selecionado, armazém vazio, um e dois colonos, armazém apenas sob o mouse, obra com dois construtores a 50%. Arquivos `reports/v41-*.png` e logs correspondentes. Save do remake continua v17.

## Limites preservados

Bônus de alojamento por pesquisas avançadas ainda não implementados. Indicador mostra a capacidade atualmente implementada, baseada no INI original. Remapeamento de paleta entre jogadores não validado nesta etapa; usa os assets e a paleta já carregados pelo remake. A linha adicional de pacientes do hospital e a lógica de ocupantes não colonos do hangar não fazem parte desta entrega. Não houve comparação quadro a quadro com uma execução interativa do jogo original.

Os originais e saves em `D:\RISING` não foram alterados. O executável original foi apenas analisado em emulação isolada a partir da cópia de trabalho. Alterações realizadas exclusivamente no código do remake, catálogos gerados, script de investigação, documentação, capturas, logs e nova publicação.
