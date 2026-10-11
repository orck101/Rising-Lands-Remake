# Trabalhadores: carga e animações — v27

Implementado em 29/09/2026. Executar `releases/worker-visuals-v27/Jogar-Primeira-Missao.cmd`.

## Comportamento entregue

- Fazendeiro usa as imagens originais carregando alimento, parado e andando, nas 16 direções e oito quadros de movimento.
- Construtor usa as imagens originais com saco nas costas tanto para pedra extraída como pedra recolhida de pilhas.
- Plantação: quatro poses originais de capinar alternam a cada dois passos da simulação durante a espera de colheita. A pose de trabalho prevalece sobre carga parcial. Comida solta não usa enxada.
- Mineração: construtor, sombra, círculo de seleção e barra de vida deixam de ser desenhados enquanto extrai; ele reaparece carregado ao concluir. A seleção existente e comandos permanecem disponíveis.
- Descarregar restaura a imagem sem carga. Recursos continuam entrando no estoque somente na entrega apropriada.
- Estado visual derivado da carga e contadores existentes, mantendo formato moderno de save v14. Nenhum arquivo/salvamento original alterado.

## Evidências

CONFIRMADO por execução instrumentada das rotinas do original preservado: `working/RISING/RISING-ORIG.EXE`, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`.

`AffEngin` VA 0x415a27, invocada com registros de unidades derivados de MONDE01, produz os comandos de desenho registrados em `source_remake/Data/worker_visuals.json`. Ferramenta reprodutível: `tools/export_worker_visuals.py`. Relatório: `reports/worker_visuals_native.json`. São 448 combinações de estado/direção/quadro e 16 atualizações de animação. `AnimWorkEngin` VA 0x447f58 confirma ciclo 0,1,1,2,2,3,3,0 para oito atualizações a partir do estado inicial de teste.

Campos usados: estado +0x02; carga +0x0a; fase de trabalho +0x89; direção +0x91; quadro +0xa4; quantidade de quadros +0xa5; intervalo +0xa7. Estado 5/fase 1 seleciona trabalho. Exemplos direção 9/quadro 0: fazendeiro vazio sprite 1634, carregado 1730, colhendo 1682; construtor vazio 1378, carregado 1506; minerando sem comandos de desenho. O teste intercepta chamadas de desenho: não equivale a uma sessão manual do original.

## Validação

264 verificações focadas aprovadas e 346 de regressão geral aprovadas (há sobreposição). Novos testes cobrem poses carregadas em todas as direções/quadros, ciclo de enxada, distinção de comida solta, espera antes de crédito da carga, ocultação/reaparição, descarregamento e restauração determinística durante colheita.

Quatro capturas renderizadas pelo próprio jogo e inspecionadas visualmente: `reports/v27-worker-food.png`, `reports/v27-worker-hoeing.png`, `reports/v27-worker-stone.png`, `reports/v27-worker-mining.png`. Nenhuma afirmação de teste manual completo com mouse.

## Limites de fidelidade

A entrada/saída física exata pela porta da mina ainda não está reconstruída. O construtor alcança a célula de trabalho do remake e fica oculto; não foi implementada nesta versão a retirada nativa completa da ocupação/combate/minimapa. A duração real do original ainda é DESCONHECIDA: mantidos os contadores documentados (250 para colheita básica, 100 para primeira extração e 10 para pilha), com frequência provisória do remake de 30 passos/s. Assim a extração básica dura aproximadamente 3,3 s, ainda diferente da lembrança de 1–2 s do usuário. As poses são nativas; os tempos em segundos e o trajeto não são declarados idênticos ao original.

Foco permanece na primeira missão. IA/campanha completa e demais lacunas da auditoria continuam pendentes.
