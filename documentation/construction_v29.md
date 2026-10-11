# Construção por marteladas e tufos de grama — v29

## Correção

O remake avançava +2 de progresso a cada passo, enquanto a pose do construtor era derivada desse progresso. A animação não comandava a obra. Agora existe relógio independente da animação (oito quadros de dois passos), contador de recarga 20 e avanço de +2 somente quando a recarga chega a zero e o quadro do martelo é 2. Entre impactos, a estrutura permanece exatamente igual. A porcentagem acompanha os impactos.

A rejeição das variações de grama vinha também de `World.Objects != 0`: tufos e decorações não bloqueantes eram tratados como obstáculos. Objetos decorativos 12–28 passam a ser permitidos sobre grama; colisão, obstáculos, cultivos, pilhas de recursos, unidades e obras continuam protegidos. Decorações cobertas deixam de aparecer sob a obra, sem alterar os dados originais do mapa. A faixa de grama nativa 349–420 permanece: não houve liberação indiscriminada de bordas secas.

## Evidência

CONFIRMADO: `AnimConstruireBatiment` 0x447e12; contador em unidade +0x8b, decrementado em 0x44b20b–0x44b22b; teste de quadro 2 em 0x44b231; chamada `BatimentConstruction` 0x44b2be; recarga 20 em 0x44b2c3. Com o registro original do construtor, a execução isolada produziu impactos nos passos 20, 52, 84, 116, 148 e 180. Ferramenta `tools/trace_construction_rhythm.py`, relatório `reports/construction_v29_native.json`.

CONFIRMADO: `BatimentPosable` 0x41fce0 aceitou os 17 códigos decorativos 12–28 em uma área de grama livre de colisão, além do controle sem objeto, e rejeitou a mesma área com uma célula seca. Na primeira missão foram encontrados tufos 12–22 sobre grama sem bit de colisão. `TerrainEstObstacleNaturel` 0x4751e4 classifica objetos 1–11 como obstáculos. A proteção de pilhas/cultivos do remake foi mantida; não afirmamos reconstrução de todas as regras de remoção/ocupação nativas.

## Validação e compatibilidade

311 verificações focadas e 347 de regressão aprovadas (baterias com sobreposição). Testes cobrem decorações permitidas, recursos bloqueados, contato do martelo, aparência estável entre impactos, save/load entre marteladas, pausa/retomada e rejeição atômica de contador inválido.

Testes antigos de construção/recrutamento tinham esperas fixas do ritmo acelerado. Foram ajustados para aguardar a conclusão dentro de limites explícitos e verificar o armazém antes de atribuir entregas. A economia continua conservada.

`AnimationTick` e `StrikeCooldown` persistem na construção. Saves modernos v14 anteriores que não contêm esses campos iniciam o ciclo no quadro 0 com contador 20, conservando progresso pago. Nenhum save original alterado. Captura renderizada e inspecionada em `reports/v29-construction.png`; a sincronização temporal é coberta pelos testes, não por essa imagem estática.

## Limites

O ritmo foi reconstruído em passos, não calibrado em tempo real contra uma sessão do original. O ramo alternativo nativo pode segurar quadros aleatoriamente; rotação, efeitos/sons de martelo e posicionamento exato junto a cada edifício continuam pendentes. O caminho básico de contador/quadro está reproduzido e demonstrado por emulação isolada.

Executar `releases/construction-v29/Jogar-Primeira-Missao.cmd`.
