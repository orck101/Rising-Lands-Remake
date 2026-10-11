> Correção em28/09: os mapas9,10,12,17 existem como variantes com letras; todos os34mapas têm texto correspondente. Integração corrigida na v16. Ver campaign_text_v16.md.

# Objetivos no menu — v15, 27/09/2026

Release: `releases/objectives-v15/RisingLandsRemake.exe`.

Esc → **Objetivo da missão** abre os textos originais em páginas de 18 linhas, preservando a divisão obtida da rotina nativa. Esquerda/Direita, PgUp/PgDn ou os botões Anterior/Próxima mudam a página. Esc, Enter ou Voltar retornam ao menu principal. A simulação permanece suspensa enquanto o menu está aberto. Reabrir o leitor retorna à primeira página.

Os textos permanecem no idioma dos arquivos fornecidos, predominantemente inglês. Não foram reescritos ou traduzidos. A apresentação gráfica e os controles do leitor são modernos; a divisão em páginas vem da emulação. Linhas muito largas têm a escala reduzida para caber no painel.

## Cobertura e limites

Há 25 arquivos de texto, com 39 páginas recuperadas. Apenas 21 deles têm o mapa ETA correspondente na instalação preservada; estes somam 33 páginas acessíveis nos 34 mapas do protótipo. Os textos MONDE09, MONDE10, MONDE12 e MONDE17 permanecem preservados na exportação mesmo sem o respectivo mapa. Os outros 13 mapas exibem aviso de texto indisponível. Treinos exibem aviso de treino livre, sem atribuir-lhes objetivos de campanha.

Os textos descrevem a campanha original, cujos eventos e condições de vitória ainda não foram integrados. Mostrar o objetivo não significa que o protótipo já possa reconhecer sua conclusão. Nenhum save ou arquivo original é alterado pelo leitor. Saves continuam no formato moderno v7; não há migração automática de saves de outras releases.

## Evidências

**CONFIRMADO por execução isolada:** MenuNewSelect (`0x42c47c`), com menu 16 e índice 2, chama MenuInit com menu 23 quando o byte em `0x4e0730` é zero. Oito casos cobrem quatro valores de jogador e dois valores desse byte; com valor 1 não há chamada. MenuBefore (`0x42c80e`), recebendo 23, retorna 16. MenuInit foi interceptada; não foram executados gráficos nem lógica de campanha nesses vetores. Ferramenta: `tools/trace_objective_dispatch.py`; relatório: `reports/objective_dispatch_original_validation.json`.

**CONFIRMADO por leitura estática:** o caminho de menu 23 chama MissionGetNum e ObjectifLoadText em `0x468375–0x46837f`. O clique chama MenuGetItemSelect e MenuNewSelect em `0x42d1c6–0x42d1df`, passando o menu atual e jogador local.

**CONFIRMADO por emulação das rotinas:** ObjectifLoadText, ObjectifGetLigneText e ObjectifTextScroll produziram as 39 páginas, com offsets, comparação das linhas aos bytes e limites de navegação. Ver `objective_text_reconstruction.md` e `reports/objective_text_original_validation.json`. `tools/trace_objective_text.py` agora também exporta `source_remake/Data/objective_pages.json`, empacotado na release. O acesso nativo a arquivos é interceptado em memória; o fallback de arquivo ausente não foi emulado.

## Verificação e alterações

247 verificações aprovadas em `logs/selftest_objectives_v15.log`. Casos novos cobrem primeira missão, limites das páginas, correspondência de todos os mapas com os textos existentes, treino, ausência de texto, reabertura e retorno ao menu. O primeiro teste de cobertura revelou quatro textos sem mapa; a expectativa foi corrigida após conferir os arquivos, sem removê-los.

Capturas `reports/prototype_v15_main.png`, `prototype_v15_objectives.png` e `prototype_v15_objectives-last.png` inspecionadas em 1280×800. Elas foram feitas antes da correção da expectativa no teste; nenhum código de interface mudou depois. Não houve automação de cliques físicos. Janelas menores e fonte idêntica à original continuam sem validação.

Adicionados ObjectiveReader, ObjectiveMenu e ObjectiveTests. Alterados PauseMenu, ResearchGame, PauseMenuTests, SelfTests e projeto para incluir os dados. Ferramentas, fontes e resultados ficam fora das cópias preservadas. Hashes e integridade em `reports/objectives_v15_validation.json`.

Próximo passo: investigar as estruturas de objetivo e sua ligação com os eventos da primeira missão, separando o que aparece no diário do que o jogo efetivamente verifica.
