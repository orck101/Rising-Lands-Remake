# v91 — reparos, destruição, defesa e desenvolvimento dos inimigos

## Como jogar

Abra `releases/interactions-v91/Jogar-Primeira-Missao.cmd`, `Jogar-Quarta-Missao.cmd` ou `Jogar-Quinta-Missao.cmd`. O atalho `Jogar-Quarta-Missao-Com-Monitor.cmd` abre também o monitor independente v89, compatível com esta versão.

Para reparar: selecione um construtor e clique em um prédio seu com vida reduzida. O cursor original de trabalho aparece ao passar sobre ele. O construtor caminha até uma posição de trabalho, bate o martelo e recupera a vida em passos. Outros construtores podem ajudar, em posições diferentes. Uma nova ordem ou Stop cancela o reparo; a carga transportada é preservada. Prédios intactos, inimigos ou destruídos não podem ser reparados. A fome pausa o trabalho para alimentação. A ordem, o progresso de animação, a vida e o desmoronamento são salvos.

Os efeitos de dano usam sprites originais de fogo/fumaça nos prédios e a sequência original com recorte quando desmoronam. A barra de vida já existente acompanha os ataques e os reparos. A colisão e as funções do prédio são removidas quando sua vida chega a zero; a animação visual continua por um breve período.

## Pedidos anteriores concluídos

- Defesa: percepção de auxílio de 3 vezes o alcance de ataque para tropas de contato; 1,5 vez para tropas de alcance. Não aumenta a distância dos golpes. A tropa aproxima-se para atacar quando precisa. Inimigos também protegem aliados fora da base, sem que isso autorize ataques ofensivos no período inicial de paz.
- Missão 4: vermelho inicia somente com seu armazém habitado, 15 alimento, 15 pedra, zero engrenagens, um fazendeiro, um construtor e três colonos reais. Um está no armazém e dois ficam livres. Sem exército ou carro inicial. Mercado é a primeira obra; bolsa de trocas é a segunda. Depois há cultivo, apoio logístico, santuário, quartel, produção e ataques se não houver aliança. A IA pode transferir operadores temporariamente para iniciar o cultivo, conservando um mínimo de alimento para os trabalhadores. Descobertas de minas continuam utilizáveis sem visão atual. Posições nativas ocupadas recebem busca alternativa em terreno válido próximo, com os custos reais.
- Missão 5: todos os três inimigos nativos começam com 15 alimento e 10 pedra. Seus arqueiros iniciais são removidos; o arqueiro inicial do jogador permanece. Recrutamento futuro depende da tecnologia e dos recursos. Planejamento de expansão ocorre a cada cinco segundos de simulação e busca aumentar a força de trabalho. Não há recursos ou soldados gratuitos.

**Limite de balanceamento medido:** no cenário sem ordens do jogador, a abertura reduzida da missão 4 demorou 230.139 ticks (aproximadamente 128 minutos em 1x, 26 minutos em 5x) até o primeiro soldado treinado. Portanto, sua progressão funciona, mas o ritmo ainda precisa de avaliação do jogador. Não afirmar que ficou rápida. A exigência de rapidez foi aplicada à missão 5; nela os três clãs coletaram e treinaram/expandiram durante o teste de dez minutos. A IA de decisões permanece uma política do remake, não a tradução integral do interpretador original.

Inícios reduzidos aplicam-se a novas partidas ou ao reinício dessas missões. Saves existentes preservam sua economia e população; não são reescritos para simular outro início.

## Evidências e graus de certeza

**CONFIRMADO:** executável copiado `working/RISING/RISING-ORIG.EXE`, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. `BatimentRepare` em `0x41cf64` reduz o dano do registro do prédio, offset `+6`, em dois pontos, sem ficar negativo. 102 entradas foram emuladas isoladamente. `CalculReparer` chama essa rotina em `0x44b991`; define espera inicial 50 em `0x44b75b` e espera entre golpes 20 em `0x44ba33`. Não foi encontrado débito de recursos nesse caminho; o remake não inventa custo de reparo.

**CONFIRMADO:** estado 15 do construtor, subestado 1, desenhado por `0x415a27`, produz os mesmos frames originais de martelo já extraídos para construção. Comparação de todas as direções e fases em `reports/repair_native_validation_v91.json`.

**CONFIRMADO:** os efeitos de dano/desmoronamento são extraídos de `AffBatiments`, `0x407dd3`, com recorte nativo `0x40f519`; estado 5 para prédio vivo e estado 3 para desmoronamento. Dano em `+6`, avanço do desmoronamento em `+0x84`; contador de animação de fumaça/fogo em `0x508f70`. Catálogo cobre os 22 tipos de construção, 32 fases dos efeitos e 26 estados do desmoronamento. Os assets e posicionamentos foram extraídos das chamadas de desenho, sem edição das artes originais.

**APROXIMAÇÃO DOCUMENTADA:** o catálogo usa níveis amostrados de dano (0%, 25%, 50%, 75%, 99%), escolhidos sem antecipar a etapa seguinte. Assim, os sprites/frames são originais, mas todos os limiares intermediários de dano ainda não foram transcritos. O avanço temporal usa o relógio de 30 ticks por segundo do remake; fidelidade completa ao tempo real original não foi medida. A vida numérica mantém resolução exata, independentemente da amostragem visual.

Ferramentas reproduzíveis: `tools/trace_building_damage_v91.py` e `tools/trace_repair_v91.py`. Saídas: `source_remake/Data/building_damage_visuals.json`, `builder_repair.json`, `reports/building_damage_native_v91.json`, `reports/repair_native_validation_v91.json` e `reverse_engineering/v91-damage-repair.asm`. Somente cópias e área de pesquisa foram alteradas.

## Validação

- `reports/v91-build.txt`: publicação Windows autocontida concluída.
- `reports/v91-regression-0.txt`: 36 verificações de reparo, cooperação, animações, defesa dos dois lados e economia dos três inimigos da missão 5.
- `reports/v91-final-0.txt`: 13 verificações prolongadas da missão 4, incluindo a sequência de obras, colonos reais, custos, recrutamento, ataque, aliança, reinício e conservação no save/load.
- `reports/v91-regression-1.txt`: 89 verificações de combate, animações, projéteis, vida, destruição e salvamento.
- `reports/v91-regression-2.txt`: 11 regressões de defesa.
- `reports/v91-regression-3.txt`: 9 regressões de tropas historicamente proibidas e saves antigos.
- `reports/v91-regression-4.txt`: 16 regressões de pesquisa e transição da missão 3.
- `reports/v91-final-2.txt`: 25 verificações do monitor de economia/tecnologia.
- Capturas reais da interface: `reports/v91-repair.png` e `reports/v91-collapse.png`.

Para o teste manual: atacar um prédio e observar o dano; mandar um ou dois construtores repararem; dar outra ordem durante o trabalho; salvar e carregar com reparo em andamento; conferir inícios das missões 4 e 5 pelo monitor. O reinício altera apenas a partida do remake, nunca os arquivos originais.
