# Construção no terreno verde e animação original — v28

29/09/2026. Versão: `releases/construction-v28/Jogar-Primeira-Missao.cmd`.

## Entrega

A área inteira da construção deve estar sobre grama original. Uma única célula fora da grama rejeita a ordem, sem cobrar recursos ou mudar a ordem do trabalhador. Continuam válidas as restrições de ocupação, recursos, tecnologia e acesso. A indicação verde/vermelha de posicionamento usa a mesma validação.

Durante a obra, os sprites nativos da armação e da estrutura são revelados conforme o progresso, usando os recortes calculados pelo original. O construtor usa sua animação nativa de construção. A porcentagem e a barra permanecem; a porcentagem é desenhada sobre o cenário para continuar legível. Pausar/retomar mantém progresso e aparência. A estrutura concluída continua funcional.

## Evidências confirmadas

Original preservado: `working/RISING/RISING-ORIG.EXE`, SHA256 af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e.

- `BatimentPosable`, VA 0x41fce0: chamada de `TerrainEstHerbe` em 0x41fe59 para cada célula da área. A regra nativa também verifica descoberta e colisão; portão (tipo 12) possui uma ramificação especial, ainda fora desta reprodução.
- `TerrainEstHerbe`, VA 0x476296, offset 0x75696: aceita IDs de tile 349 até 420 inclusive. Todos os 1.346 IDs de terreno testados na rotina instrumentada. Nenhuma inferência pela cor RGB de pixels.
- `CreerBatiment` escreve estado 2 em 0x42013f. O catálogo visual anterior amostrava estado 4, que não reproduzia a obra em andamento. Esta extração corrige o estado e preserva `SetClipping` (0x40f519), anteriormente ignorado.
- Desenho `0x407dd3`: 40.501 invocações para 101 porcentagens em 22 tipos de edifício. Armazém: armação sprite 7093 e estrutura 7094, com limites de recorte variáveis. Evidência em `source_remake/Data/construction_visuals.json`.
- Construtor: estado 11, subfase 1, `AffEngin` 0x415a27; 128 quadros/direções extraídos em `builder_construction.json`. `AnimConstruireBatiment` 0x447e12 confirma avanço por intervalo e quantidade de quadros. A exibição usa progresso de obra para conservar determinismo e congelar na pausa.

Extração reprodutível: `tools/export_construction_visuals.py`. Relatório `reports/construction_visuals_native.json`. Emulação controlada do código original, com visibilidade fixada e chamadas de desenho interceptadas; não é uma sessão manual do original.

## Validação

275 verificações focadas na primeira missão e 346 verificações de regressão aprovadas (há sobreposição). Testes novos: grama em todo o contorno, rejeição de uma célula seca sem mutação, terreno transitável não construtível, estágios e recortes, animação do construtor, restauração, pausa/retomada e conclusão funcional.

Teste histórico de quartel/santuário/cultivo buscava apenas uma faixa estreita que incluía solo seco. Ajustado para procurar terreno legal no mapa, preservando exigência de acesso e conclusão.

Capturas do próprio jogo inspecionadas: `reports/v28-site-10.png`, `v28-site-50.png`, `v28-site-90.png`; versão final com porcentagem legível em `v28-site-50-final.png`.

## Limites

Cadência em segundos, sons de construção e posicionamento exato do construtor em relação a cada edifício ainda não são declarados iguais ao original. O caminho/posição de trabalho continua sendo o do remake. Descoberta por neblina e regra especial de portões ainda exigem trabalho próprio. A restrição aplica-se às novas ordens; saves modernos antigos podem restaurar construções feitas antes da correção, sem apagar seu progresso. Save v14 preservado. Nenhum original ou save original alterado.
