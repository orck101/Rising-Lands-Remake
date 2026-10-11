# Correção visual e navegação — 26/09/2026

Esta versão continua sendo um protótipo de reconstrução. Economia, combate, IA, eventos de missão e campanha jogável permanecem pendentes. A navegação usa BFS de laboratório, 30 ticks/s e 64/1024 de célula por tick; esses valores não são timing original comprovado.

## Evidências e reprodução

- **CONFIRMADO:** IDs gráficos começam em 1. `RISING-ORIG.EXE`, PasteBobCmp 0x45b1eb (acesso 0x45b1fd), PastePlayerCmp 0x45c40d e AffTerrain 0x41078a usam ponteiro[id-1]. A versão anterior usava índice deslocado; terreno e componentes de construções eram incorretos. LoadSTRATBank 0x41881a usa a tabela de seis bancos em 0x495bc8. Bases corretas: BLOCS 1, TROOPS 1346, VEHICULE 4194, CREATURE 5109, BUILDING 7029, OTHERS 7640.
- **CONFIRMADO sob condições controladas:** `tools/trace_rendering.py` emula AffEngin 0x415a27 e intercepta comandos de desenho. Catálogo cobre os 19 tipos presentes nos mapas, 16 valores de direção e fases de animação. Arquivo: `source_remake/Data/unit_rendering.json`. Estado forçado 7, efeitos desativados; isso não cobre ataque, morte, cargas e todos os estados possíveis.
- **CONFIRMADO:** estado 14 não desenha unidade no dispatcher; os oito ocupantes iniciais de MONDE01 não devem aparecer como unidades sobrepostas. Evidência: `reports/initial_unit_rendering.json`.
- **CONFIRMADO:** direção no payload ETA +657; fase +676; quantidade de fases +677; atraso +679; contador +681. AnimDeplaceEngin 0x448088 incrementa contador e fase circularmente. A cadência da simulação moderna ainda é experimental. Orientação visual dos valores usados na navegação: 1 oeste, 5 norte, 9 leste, 13 sul; validação completa da atribuição de direção pelo motor pendente.
- **FORTE EVIDÊNCIA:** cenas extraídas com PlaceBatiment 0x41c54c, AffBatiments 0x407dd3 e AffVegetTerrain 0x410085. `source_remake/Data/Scenes` contém comandos, hash do mapa, alterações de terreno e colisão. Ocupação de construções preenchida pelo retângulo da tabela de tipos; clipping suprimido, visibilidade verdadeira, tempo zero, atualização de cache de caminhos interceptada. Não equivale a inicialização integral do jogo.
- **CONFIRMADO:** BatimentModifyTerrain 0x41bae9 testa máscara de footprint e chama SetCollision com bit 1 em 0x41bbf6–0x41bbfd. O protótipo usa esse bit e as alterações obtidas por emulação para bloquear rotas. Máscaras particulares por classe de unidade e algoritmo original de caminhos permanecem desconhecidos.

## Implementação

Terreno corrigido, prédios e objetos com gráficos originais, unidades diferenciadas com direção e fases, minimapa derivado do terreno e seleção por limites visuais. Sombras e cores por jogador ainda aproximadas; interface moderna provisória. Save de exploração JSON versão 2 preserva direção e animação e rejeita versões incompatíveis; não importa saves de campanha original. Fonte de interface atual: atlas gerado localmente a partir de Segoe UI instalada; direitos de redistribuição precisam ser resolvidos antes de publicação pública.

Todos os trabalhos ficam na área de pesquisa; nenhum original é alterado. A imagem `reports/prototype_visual_v2.png` foi capturada do programa e inspecionada: santuário composto, terreno contínuo e unidades distintas. Testes de controles manuais completos e matriz Windows 10/11 continuam pendentes.

## Validação da versão publicada

Publicação independente em `releases/visual-v2/RisingLandsRemake.exe`; versão anterior preservada. 84 verificações aprovadas, incluindo 34 mapas e cenas, 12268 quadros CMP, índices gráficos, ocupantes ocultos, colisão, rotas, animação e continuidade determinística após save/load. Logs em `logs/selftest_visual_v2.log`. Capturas de janela inicial e movimentação inspecionadas. Hash e limitações em `reports/visual_v2_validation.json`.
