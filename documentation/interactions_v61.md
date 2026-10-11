# v61 — cultivo com indicadores e regras do original

## Entrega

Versão: `releases/interactions-v61`. Iniciadores da primeira, segunda e terceira missão; F9 retoma os cinco slots compartilhados. Saves continuam no formato v24. Não houve edição dos saves do jogador ou de D:\RISING.

- Os dois botões genéricos de alimento/colono foram retirados do menu do cultivo. O retrato original permanece.
- Cultivo próprio selecionado ou sob o mouse mostra os cinco lugares de colonos e a fileira original de comida, com sprites, cores, espaçamento e posição extraídos do executável.
- A comida entra uma unidade de cada vez. Existem quatro quadradinhos no início: o quinto alimento gera o colono e limpa o indicador. As pesquisas reduzem o custo para quatro e depois três alimentos, com três e dois quadradinhos visíveis, respectivamente.
- Precisa haver pelo menos um colono dentro para gerar outros; máximo de cinco residentes. O novo colono permanece alojado. Dois cliques retiram um residente. Alimentar um cultivo lotado libera um residente existente e deixa o alimento no chão, sem criar um sexto.
- Cultivo vazio aceita alimento, mas só começa a gerar quando recebe seu primeiro colono. Contador nativo de comida limitado a cinco; entrega excedente no cultivo vazio é consumida pelo comportamento nativo.
- Impedimento por capacidade de unidades preserva a comida. Comida descartada por cultivo lotado entra no histórico de entregas do trabalhador, evitando recolher imediatamente a própria entrega.

## Evidências e grau de certeza

**CONFIRMADO — aparência:** `RISING-ORIG.EXE`, `AffJaugeBatiment` 0x4020ec → `AffContenanceBatiment` 0x401a70. Coluna de residentes 0x401aff–0x401b74; comida 0x401c16–0x401cbc. Foram executadas 108 combinações controladas de desconto, residentes e alimento, registrando chamadas reais de desenho. `reverse_engineering/crop_v61.json` preserva evidências e offsets dos frames; `source_remake/Data/crop_visuals.json` contém a tabela usada pelo jogo.

Sprites de `OTHERS.CMP`, base nativa 0x1dd8: vazio 0x2a3b, residente 0x2a3c, comida 0x2a3d. A fileira tem `4 - desconto` lugares, espaçados oito pixels. Coordenadas relativas à âncora do prédio: residentes (-6,2), (-6,10), (-6,18), (-6,26), (-6,34); comida (26,74), (34,74), (42,74), (50,74). Descontos removem os últimos lugares. São ampliados junto com o cenário. Gate nativo de exibição de prédio próprio selecionado/sob o mouse: 0x40f3eb–0x40f442.

**CONFIRMADO — produção:** `BatimentNextEtape` 0x42696c, ramo 0x426ca8–0x426df4. Comida em prédio+0x78; custo em 0x426d66 e teste em 0x426d7f. Sucesso limpa todo o contador em 0x426db3 e aloja uma unidade tipo 9, estado 14. Reexecução de `trace_culture_v40.py`: 168 vetores aprovados. Agricultura em índices internos 0–5 custa 5; 6–14 custa 4; 15–19 custa 3. Efeitos AGRICULTURE5 e AGRICULTURE14. A tabela `culture_native.json` fornece os preços diretamente à implementação.

**CONFIRMADO — entrega:** `BatimentAjouteMatiere` 0x4234a7, cultivo 0x4235a9–0x4235ed. Soma uma comida, limitada a cinco. Cultivo lotado retira um residente em 0x4235d0 e descarta uma comida em 0x4235e8, referência célula+512. Transporte: quantidade um em 0x44f0ad–0x44f0dc e recarga dez em 0x44f08d. Reexecução de `trace_crop_delivery_v57.py`: 36 vetores aprovados. Teste de integração v57 confirma a entrega real, com intervalo de dez atualizações, e save/load no meio da entrega.

**CONFIRMADO — espaço global:** `EnginDisponible` 0x457e0c–0x457e48 procura um registro livre entre 800 registros de 276 bytes, estado em +2. Três sondagens: 0/799 ocupados aceitam, 800 bloqueiam. `reverse_engineering/crop_entity_space_v61.json`.

Executável de referência SHA-256: af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e. Todas as sondagens usaram cópia do PE em emulador, sem executar o programa original ou escrever em sua instalação. Chamadas de bônus e bolsa foram controladas para isolar os ramos examinados.

## Limites conhecidos

**DESCONHECIDO:** cadência exata em segundos do ciclo geral do original. O ramo de produção não contém uma espera própria; o remake verifica a geração em sua atualização de 30 Hz. A recarga de dez é confirmada em contagens, não em segundos originais. Não se afirma equivalência de tempo absoluto.

**APROXIMAÇÃO:** limite de 800 aplicado a entidades vivas do remake. O reaproveitamento e a limpeza dos registros mortos originais não foram reconstruídos. Busca de chão válido próximo ao cultivo e espera com carga quando não há espaço continuam sendo políticas do remake; a ordem exata dessa busca no original ainda não foi comprovada. Os textos de ajuda são do remake.

## Validação

1.777 verificações aprovadas: 157 do cultivo/apresentação e apoio de coleta, 782 da missão 1, 351 gerais, 295 de coleta/cultivo/formações/saves, 83 da missão 2, 46 de exploração, 53 de pesquisa e 10 de leitura/preservação dos cinco saves reais. As contagens são verificações, não partidas independentes. Adicionalmente, 315 vetores nativos (168 produção + 36 entrega + 108 desenho + 3 capacidade).

Capturas reais em `reports/v61-crop-*-review.png`: cultivo vazio, quatro comidas, nascimento, lotado, liberação, agricultura 6 e agricultura 15. As sete foram conferidas visualmente. Testes salvam e restauram alimento parcial, novo colono alojado e saída com alimento no chão. Compilação Release para Windows x64 concluída.

## Como conferir jogando

Abra a v61 e retome por F9. Selecione um cultivo e mande o fazendeiro entregar alimento pelo comando Ctrl já existente. Observe os quadradinhos preencherem; o último alimento aumenta a coluna de residentes. Faça dois cliques para retirar um. Um cultivo vazio precisa receber um colono. Quando lotado, nova entrega libera um dos residentes.
