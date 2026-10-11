# Histórico de pesquisa
## 2026-09-25 — adoção do PDF e protótipo
Fonte: Prompt_Mestre_Rising_Lands_Remake.pdf,33páginas,70seções. Texto integral extraído para PROMPT_PDF.txt. Pedido do usuário adota o documento como requisito de projeto.

Descoberta:26 IDs de unidades e22 de construções recuperados das tabelas estáticas via EnginGetTypeName/BatimentGetTypeName. Offsets/getters em entity_fields.json. HP de unidade está em memória+154 mas no payload ETA+666 devido ao salto de512bytes. CONFIRMADO por leitura das instruções; exportadas34 missões e81 seções de tecnologias.

Implementação: C# MonoGame DesktopGL, importadores originais, mapa, câmera, seleção, navegação experimental e JSON de save moderno. Compilação sem avisos;34mapas e12268frames lidos; teste de continuidade após save/load aprovado; janela abriu e imagem capturada inspecionada. Não alegar que navegação BFS corresponde ao algoritmo original. Nenhum binário legado foi executado no Windows.

Próximo passo: reconstruir animações/tipos, máscaras de passabilidade, economia, combate e VM RUL antes de entregar vertical slice fiel. As limitações estão no prompt_status.md.

## Etapas anteriores
Ver RETOMADA.md na raiz, logs/actions.jsonl, FORMATOS.md e relatórios diferenciais. Originais foram copiados, não editados; divergência posterior no save1.sav da origem registrada separadamente. Paleta corrigida e hipótese CMP=LZSS refutada; preservar resultados dos testes que contradizem hipóteses.


## 2026-09-26 — correção visual
Ver visual_reconstruction_v2.md para endereços, método, limites e alterações. Corrigido erro de indexação dos gráficos; exportações de mapas regeneradas. Catálogos obtidos por emulação controlada, sem executar processo legado no Windows.

## 2026-09-26 — ciclo inicial de alimento
Implementação e evidências em food_reconstruction_v3.md. Rotinas nativas confirmam capacidade, retirada, mutação de colheita e regras de estoque/excedente; controlador moderno de deslocamento/trabalho permanece parcial. Testes de conservação e save/load incluídos. Nenhum original alterado.

## 2026-09-26 — construção, formação e comandos de grupo
Registro completo em economy_reconstruction_v4.md. 127 casos de construção, 45 de gastos e 34 de progressão de formação emulados; custos de formação conferidos. Executável separado economy-v4; saves JSON v4. Teste conjunto construir-formar-coletar com continuidade determinística após save/load. Combate, IA e eventos permanecem pendentes.

## 2026-09-27 — combate inicial
Registro em combat_reconstruction_v5.md. 2.544 comparações x86, 160 fases/direções gráficas e 156 verificações no executável publicado. Cópias original/RISING e working/RISING conferidas integralmente contra inventário (670 cada), sem divergências. Treino separado em memória, sem escrita nos originais. IA estratégica, eventos, projéteis e fidelidade temporal continuam pendentes.

## 2026-09-27 — arqueiros e projéteis
Registro em archer_reconstruction_v6.md. 1.875 casos de alcance, 40 trajetórias / 9.255 posições e 160 combinações de animação capturadas. 178 verificações no executável publicado. Treino F3 separado, saves v6 com projéteis, compatibilidade de leitura v4/v5. 670 arquivos de cada cópia original/working conferidos, sem divergências. Impacto, velocidade global, IA e campanha ainda parciais.

## 2026-09-27 — preparação dos ataques e rastro
Release timing-v7: 192 verificações aprovadas. 32 preparações de ataque e 71 atualizações nativas de projéteis comparadas. Saves v7 com leitura v4–v6 e conservação do voo legado. Captura inspecionada. As duas cópias de 670 arquivos conferidas sem divergências. Evidências e limites em timing_reconstruction_v7.md.

## 2026-09-27 — seleção da vítima no impacto
6.030 casos aprovados: 5.148 ocupações, 441 buscas e 441 encaminhamentos. Construção tem prioridade no modo zero; busca usa campos +114/+171 e duas células. Efeitos de dano e diplomacia não executados. Ver impact_selection_reconstruction.md. Duas cópias de 670 arquivos conferidas sem divergências. Release jogável continua v7.

## 2026-09-27 — controles v8
Seleção por células comparada em 1.430 casos nativos; limite 255 e propriedade na seleção adicional. Correção moderna de foco. 201 verificações aprovadas, captura inspecionada e duas cópias de 670 arquivos intactas. Evidências em controls_reconstruction_v8.md. Teclas físicas e menus continuam parciais.

## 2026-09-27 — grupos v9
36 combinações de modificadores e 72 atribuições nativas comparadas. Grupos exclusivos incorporados durante a sessão; saves permanecem v7. 207 testes aprovados, captura inspecionada e cópias de 670 arquivos intactas. X original chama GoLastAttaque; próxima prioridade documentada em control_groups_v9.md.

## 2026-09-27 — alertas v10
X restaurado como navegação ao último ataque. Backspace é parada provisória. 76 vetores nativos e 216 testes da publicação aprovados. Alertas da sessão; frequência global continua parcial. Duas cópias de 670 arquivos intactas. Ver attack_alert_v10.md.

## 2026-09-27 — informações v11
Espaço mantido solicita informações; barras modernas condicionadas à seleção própria, cursor ou Espaço. 64 casos nativos e 217 testes aprovados. Pausa transferida para Pause como escolha moderna. Capturas normal/todas inspecionadas e duas cópias de 670 arquivos intactas. Ver unit_info_v11.md.

## 2026-09-27 — mouse v12
Corrigida inversão dos botões. Eventos nativos e ramos isolados comparados em 17 casos; 223 testes da publicação aprovados. Esquerdo contextual, direito desseleciona sem parar. Menus e exceções de zona ainda parciais. Captura inspecionada; duas cópias de 670 arquivos intactas. Ver mouse_controls_v12.md.

## 2026-09-27 — menu v13
Esc abre menu moderno em vez de sair diretamente. Despacho original do menu 16 confirmado em 16 casos. 229 verificações aprovadas e três telas inspecionadas. Salva/carrega reutilizam v7. Duas cópias de 670 arquivos intactas. Ver pause_menu_v13.md.


## 27/09/2026 — v14
MenuInit(16) isolado: título 31, itens 5/6/10/20/11/12; tabela e offsets em pause_contents_original_validation.json. Reinício moderno com confirmação na interface, com fotografia inicial em memória e sem escrita de saves. 239 testes; duas capturas inspecionadas. Detalhes e limites em pause_menu_v14.md. Próximo: despacho de objetivo de missão.


## 27/09/2026 — textos de objetivo
ObjectifLoadText, ObjectifGetLigneText e ObjectifTextScroll executados para 25 textos: 39 páginas nativas, com offsets e limites de navegação verificados. Ver objective_text_reconstruction.md. Sem nova release; menu-v14 permanece atual. Próximo: despacho de MISSION OBJECTIVE e leitor de páginas.


## 27/09/2026 — objetivos v15
Leitor de objetivos integrado, páginas nativas e aviso de treino/texto ausente. Despacho menu16 item2 → menu23 verificado em 8 casos; retorno23 →16. 25 textos recuperados, dos quais 21 correspondem aos mapas ETA disponíveis (33 páginas acessíveis). 247 testes aprovados, três capturas inspecionadas. Ver objectives_v15.md. Próximo: estruturas de objetivo e eventos da missão1.


## 28/09/2026 — condições missão 1
LoadFinMission e TestFinDeMission executados com consultas de prédio controladas: 75 casos; CaseGetZoneInfluence: 36 casos. Armazém próprio em zona0, estoques IDs1/2 >=50; contagem global zero força derrota. Zona0 inclusiva x0..28/y0..25. Catálogo bruto de condições dos34mapas. Ver mission_one_conditions.md. 1340 cópias preservadas conferidas. Sem nova release; v15 permanece atual.


## 28/09/2026 — ciclo de missão
508 casos nativos: contagem de unidades inclui estados ocultos, exclui0/4 e tipo17; existência de prédio depende de estado+5 não nulo; missão1 aceita estado4/progresso0 em fixture. Despacho para menu19 após contador100; retorno falso não o zera nesse bloco. Ver mission_lifecycle.md. 1340 cópias verificadas. Sem nova release. Próximo: menu19/MissionNext e estados de morte.


## 28/09/2026 — variantes v16
86 casos nativos de MissionNext, MakeMissionName e continuação. Identificadas variantes9A-D e10/12/17A-C: corrige a conclusão anterior sobre mapas faltantes. Textos agora acessíveis em todos34mapas. 251 testes aprovados, captura09D inspecionada. Transferências de experiência/pesquisa/jogador ainda interceptadas. Ver campaign_text_v16.md.


## 28/09 — worker-orders-v17

> Versão atual: **worker-orders-v17**, em `releases/worker-orders-v17/RisingLandsRemake.exe`. B abre catálogo; agricultor recebe destino explícito com E/L e pode repetir a coleta. 268 verificações aprovadas. [Evidências e limitações](documentation/worker_orders_v17.md). Pesquisa durante a partida e funções especializadas de prédios permanecem pendentes. Registros abaixo são históricos.

Alterações restritas a código/dados exportados/documentação/distribuição; 1340 hashes preservados. Native unlocks: 81 efeitos e 25 inicializações. Fonte de verdade: worker_orders_v17.md e worker_orders_v17_validation.json.


## 28/09 — engineering-v18

> Versão atual: **engineering-v18**, em `releases/engineering-v18/RisingLandsRemake.exe`. B: construções; E/L: destino da comida; T: pesquisa de engenharia com colonos no laboratório e desbloqueios durante a partida. 293 verificações aprovadas. [Entrega e limites](documentation/engineering_v18.md). Bônus, outros ramos, funções especializadas e campanha completa continuam pendentes. Registros abaixo são históricos.

Integração após pedido de continuidade: 396 casos nativos de progresso,17 contagens de colonos e1 troca de ramo. Save v9; mudanças restritas a artefatos do remake. Integridade: 1340 hashes. Fonte de verdade: engineering_v18.md e engineering_v18_validation.json.


## 28/09/2026 — harvest-v19

Implementado ciclo de campo com área fixa, retomada do mesmo destino/caminho e indicadores visuais. Save v10 com migração conservadora. 309 testes passaram; captura real inspecionada. Alterados somente fonte do remake, nova release, testes, documentação e relatórios. Originais e cópias preservadas sem alterações. Detalhes: harvest_v19.md.


## 28/09/2026 — mining-v20

Integradas jazidas ETA, extração/entrega de rocha pelo construtor, estoque compartilhado, esgotamento visual e save v11. 344 testes do executável e 51 casos emulados de retirada/depósito passaram. Catálogo de 34 mapas. Captura real inspecionada. Arquivos originais preservados. Evidência versus regras provisórias: mining_v20.md.


## 28/09/2026 — mission-one-v21

Foco exclusivo na fase1 acordado pelo usuário. Integração da vitória, contador de100 avaliações positivas, tela final com botão de próxima fase deliberadamente inerte, savev12 e indicador de progresso. Teste completo desde início sem recursos injetados;77 verificações focadas passaram, incluindo75 vetores nativos. Capturas inicial/final inspecionadas.1340 arquivos preservados conferidos. Skills binary-analysis-patterns e systematic-debugging consultadas. Detalhes: mission_one_v21.md.


## 2026-09-28 — worker-input-v22

Implementada a sequência confirmada pelo usuário: Ctrl segurado + clique na fonte, depois clique sem Ctrl no destino. Transporte de pilhas 66/67/68 por agricultor e construtor, validação de fontes reais, destino pendente e save v13. 119 verificações focadas passaram, incluindo percurso até vitória pela camada de ordens; 62 casos nativos de pilhas passaram. Captura visual revisada; não foi uma sessão manual de teclado/mouse. Integridade de 1340 arquivos preservados em original/working conferida. Rocha minerada diretamente para chão permanece pendente. Ver worker_input_v22.md.


## 2026-09-29 — farmer-v23

# Fazendeiro — v23

## Regra confirmada pelo usuário

Com o fazendeiro selecionado, manter Ctrl pressionado mostra um cursor de coleta sobre comida plantada ou pilhas de alimento; em locais sem alimento, mostra círculo com barra de proibido. Ctrl + clique define a fonte; clique sem Ctrl define o destino. Alimento pode ser transportado entre pontos de chão livre acessível ou entregue ao armazém. Só o depósito credita o estoque disponível.

## Causa e correção

v22 generalizava SourceAvailable e GatherAt para todas as pilhas (66/67/68), inclusive para o fazendeiro. O teste de rejeição de engrenagem falhou antes da correção (logs/farmer_v23_before.log). Agora as duas camadas restringem o fazendeiro a IsFood; regras do construtor permanecem como estavam. Clique comum em comida não inicia coleta independente de Ctrl.

A indicação anterior era texto/quadrado com seta do sistema. Agora a seta é ocultada durante Ctrl no mapa, sendo substituída por cesta com seta para baixo ou círculo cortado. Ao soltar Ctrl, sair do mapa ou abrir menu, a seta normal retorna. São ícones modernos, não imagens extraídas do cursor original.

Também corrigida a transferência a partir do último destino: o destino anterior era indevidamente usado para rejeitar a nova fonte. A nova ordem limpa esse destino antes de iniciar a coleta e o restaura se a ordem falhar.

## Validação e limites

120 verificações focadas na primeira missão passaram. Incluem recusa de pedra/engrenagem, planta para chão, chão para outro chão, nova coleta no destino anterior, entrega final ao armazém, conservação e save/load. Capturas v23-farmer-collect.png e v23-farmer-prohibited.png revisadas: são cenários controlados de desenho, não sessão manual de mouse. Percurso automatizado da missão até a vitória permanece aprovado.

Chão livre significa célula acessível, sem edifício ou objeto incompatível; pilhas mantêm limite de31 unidades. Não permite depositar dentro de obstáculos. Temporização e alcance da repetição continuam provisórios. A versão não pretende concluir os comandos de todos os personagens.



## 2026-09-29 — gameplay-v24

Revisão de papéis e destinos baseada em 906 casos nativos. Transporte em grupo; jazida → chão → armazém; santuário rejeitado no comando manual. Trechos de temporização: 11 casos com configuração/bônus controlados; fazendeiro 250, início de mineração 100, continuação 50. Save v14 e migração de destinos antigos preservando carga/estoque.

Validação: 227 verificações focadas e 346 de regressão; contagens se sobrepõem. 1.340 hashes das cópias preservadas coincidem. Captura de mineração revisada; nenhuma partida manual do original ou teste manual de mouse nesta revisão. Falhas iniciais e correções documentadas em gameplay_v24.md. Auditoria cobre 18 sistemas e permissões de 26 tipos; não declara gameplay completa.


## 2026-09-29 — Save fornecido pelo usuário

SAV6 Mission 5 (5), hash c442f2083a32e036b42a87d96842fd23e4449a120ab46634fb942226cf3d2d9c. Pesquisa localizada em 0x19451b: jogador 0 agricultura14, militar20, civil13, religião8. Referência civil11; duas descobertas registradas. 24 getters nativos e roundtrip de2180bytes confirmados. Fonte intacta; cópia separada e relatório save1_technology_analysis.md. Sem mudanças no remake.


## 2026-09-29 — retomada exclusiva do remake

# Remake — tecnologia v25

Esta entrega integra os bônus nativos de carga e recarga dos trabalhadores à simulação. O foco de desenvolvimento e teste jogável continua sendo a primeira missão. Não importa nem aplica ao remake as alterações feitas anteriormente em saves do original.

## Alterações

- Capacidade de transporte passa a considerar tecnologia, em vez de usar somente a capacidade básica por personagem.
- A primeira extração de pedra usa RECHARGE menos o bônus tecnológico. A continuação da mesma carga mantém o contador nativo de 50.
- Engenharia civil 12 permite ao construtor carregar duas pedras; nível 13 reduz o contador inicial de 100 para 66. Esses valores foram extraídos das rotinas nativas.
- Pesquisas de capacidade/recarga do construtor receberam suporte e nomes no painel. Outros efeitos não implementados continuam bloqueados; a árvore ainda NÃO oferece um caminho completo até todos esses níveis.
- Validação de partidas do remake calcula capacidade usando a pesquisa presente no estado carregado, evitando comparar contra a tecnologia da sessão anterior.

Na missão 1, os níveis iniciais permanecem agricultura 2, militar 2, engenharia 4 e religião 0. Portanto, o construtor continua com capacidade 1 e contador inicial 100 nessa fase. A integração não dá tecnologias avançadas gratuitamente.

## Evidência

`source_remake/Data/worker_technology.json` contém 2.000 combinações de níveis de agricultura/engenharia e tipos 5, 6, 7 e 13, extraídas com 4.000 consultas nativas. InventionSetEffet 0x470e88 e BonusGetValEngin 0x428d58 fornecem os bônus. O carregamento real de 2.048 bytes de bônus do save analisado anteriormente foi reproduzido byte a byte, como validação da interpretação. O save não é carregado pelo remake.

O exportador é uma ferramenta de pesquisa, separada do jogo. A simulação usa apenas o JSON empacotado. Atualizações de entidades e interface do original não são executadas pelo exportador.

## Validação

**240 verificações na bateria focada** e **346 na regressão geral**, com sobreposição. A bateria focada inclui 13 verificações adicionais de tecnologia em cenário controlado, e não uma campanha jogada até os níveis avançados. Confirma contadores, carga de duas pedras, conservação, espera por destino e reinício. Não houve partida manual nesta revisão.

A versão usa o formato moderno v14 existente; nenhuma mudança estrutural foi necessária. A validação geral de partidas passou, mas um percurso completo de pesquisa avançada → gravar → carregar ainda não foi coberto por um cenário jogável legítimo.

## Limites e próxima prioridade

Agricultura não tem progressão de pesquisa integrada. Construção acelerada, fome, combate tecnológico e demais ramos continuam parciais. O tempo real da simulação ainda é provisório. A auditoria v24 continua válida, exceto pela capacidade e recarga de trabalhadores agora integradas parcialmente.

Prioridade para a missão 1: completar o papel do Cultivo e o destino de alimento nessa estrutura, com armazenamento local, consumo e geração de colonos confirmados no original. Não usar crédito direto no estoque global como substituto dessa mecânica.

## Diretriz atual do usuário

Foco exclusivo no remake. Não voltar a alterar saves originais ou produzir versões modificadas deles. Os artefatos antigos são apenas histórico; só retomar esse assunto mediante novo pedido explícito.


## Coleta v26

# Coleta v26

## Problemas reproduzidos e corrigidos

1. WorkerResourceOrders removia o trabalhador da lista de escolha de destino após a primeira entrega ser definida. Mesmo carregado, um segundo clique deixava de passar pelo comando de entrega. WaitingFor agora reconhece carga presente; o jogador pode redirecionar a entrega enquanto a transporta.
2. SourceGroup encaminhava todos os agricultores para a mesma planta. A busca agora prioriza plantas que ainda têm produção disponível para os trabalhadores já designados, mantendo a âncora escolhida e o limite da área. Pilhas continuam sendo compartilhadas; não são desviadas automaticamente para outro recurso.
3. FoodStep iniciava entrega assim que uma planta acabava, mesmo com espaço na carga e outras plantas próximas. Com repetição ligada, o trabalhador agora procura outra planta antes de retornar. Repetição desligada continua encerrando após a coleta escolhida.
4. O comando de destino interceptava cliques sobre outras unidades amigas. Esses cliques agora seguem para a seleção normal, preservando as ordens existentes.
5. O indicador de destino mostrava qualquer local como válido. Agora verifica o tipo de estrutura, ocupação da célula e capacidade da pilha. Essa prévia não calcula caminho a cada movimento do mouse; a acessibilidade é validada na emissão da ordem.

## Desempenho

A busca de caminho criava um vetor de 65.536 inteiros por chamada (aproximadamente 256 KiB), além da fila e de vetores de direções em cada célula expandida. Os vetores e a fila agora são reutilizados por instância da simulação; as direções são constantes. A ordem de expansão permanece a mesma. Dez pedidos de caminho no cenário de teste alocaram 408.960 bytes no total, incluindo demais operações de consulta. Isso não é uma medição de FPS nem garante ausência de travamentos em mapas maiores.

## Validação

Dois testes falharam antes da correção: redirecionamento e distribuição dos agricultores. Logs: `collection_v26_before.log` e `collection_v26_before_errors.log`.

Após as mudanças: **249 verificações na bateria focada**, incluindo nove novos casos de coleta/desempenho, e **346 na regressão geral**, com sobreposição. Testes existentes verificam conservação de recursos, disputa pela última unidade, repetição, bloqueio de caminhos, determinismo de partidas gravadas e missão até a vitória. Os novos casos verificam distribuição, âncora, redirecionamento, carga parcial e rotas determinísticas com memória reutilizada.

`reports/v26-harvest.png` foi revisada no renderizador real: fonte verde, área delimitada e destino azul. É cenário preparado, não uma sessão manual de mouse. Clique em unidade amiga e prévia de entrega foram verificados no código; não houve automação de interação manual nesta revisão.

## Fidelidade e limites

As permissões por papel, transições de plantas, capacidade e contadores previamente demonstrados permanecem. **A distribuição por disponibilidade de plantas é uma política do remake; ainda não foi demonstrada como o algoritmo original.** O raio local continua provisório em seis células. Não há promessa de rotas ótimas, formação, colisão entre todas as unidades ou controle completo de Cultivo/fome. Trabalhadores voltam com carga parcial se nenhuma planta alcançável permanece. Destino cheio mantém a carga com o trabalhador para nova escolha; não há despejo automático em outra área.

A atualização é compatível com o formato moderno v14 e não modifica saves do original. Versões anteriores do remake permanecem disponíveis. Foco exclusivo no remake e na primeira missão.


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


# Construção: som e orientação — v30

30/09/2026. Executar `releases/construction-v30/Jogar-Primeira-Missao.cmd`.

Som de martelada extraído do original associado ao evento de avanço da obra. Uma emissão por impacto, sem replay no carregamento ou na pausa. Obras fora da câmera e capturas automáticas ficam silenciosas. Volume 0,45 e panorama conforme posição horizontal são escolhas do remake, não mistura original reconstruída. Falta de dispositivo de áudio desativa o efeito sem impedir o jogo.

O construtor agora olha para o ponto mais próximo da área construída durante o trabalho. Mapeamento das oito direções confirmado em execução isolada de `CalculDirection` VA 0x476f6e; resultado em `reports/construction_facing_native.json`. A escolha do ponto de trabalho e rotação instantânea ainda são adaptações do remake.

## Origem do som

CONFIRMADO no código original em 0x44b322–0x44b33d: banco 0x50 (P), chave formada pelo tipo 7 deslocado 16 bits e ação 0x2500, resultando em 0x72500. `SOUND/P.HED` grupo 143, registro no offset 1732, descritor 250 no offset 7352. Dados em `SOUND/P.SND`, offset 4104192, tamanho comprimido 3072 bytes. PCM convertido: 6102 amostras mono, 16 bits, 22050 Hz (aproximadamente 0,277 s). SHA256 PCM `5138b42429363dc855042a448f6ba14face3776f6d5031784ded21df99a27a6a`, conferido novamente contra a extração anteriormente validada pela rotina original. Relatório `reports/construction_audio_mapping.json`. Arquivo de distribuição `Content/Data/builder_hammer.wav`.

## Verificação

314 verificações focadas e 347 de regressão passaram, com sobreposição. Novos casos verificam emissão única por impacto, ausência de sons na pausa e direção voltada para a obra. Compilação concluída; captura `reports/v30-construction.png` inspecionada. O acionamento do evento e PCM foram verificados; saída audível pelo dispositivo e mixagem subjetiva não foram confirmadas em sessão manual. Sons de outros personagens, ambiente e música continuam pendentes.

Estado moderno v14 preservado; eventos sonoros não fazem parte do save nem alteram o resultado da simulação. Nenhum arquivo ou save original modificado.


## 2026-09-30 — interactions-v31

Implementados rascunho de coleta com Ctrl contínuo, retirada por profissão com débito na coleta, painéis de construções e formação de engenheiro/espadachim. Corrigidos descarte junto ao armazém e saída de colono da população do santuário. 336 verificações focadas e 347 gerais aprovadas. Quatro capturas inspecionadas. Catálogo de dez unidades verificado contra configuração e amostras de mapas. Ver interactions_v31.md para auditoria de 22 estruturas, evidências e limites; não declarar todas as ações concluídas nem estoque individual implementado. Nenhum arquivo original modificado.


## 2026-09-30 — cooperative-ui-v32

Equipe de construção com pontos e ciclos individuais. Dois presentes somam o dobro do trabalho; parar um preserva os demais. Save remake v15 com leitura anterior. Barra de quatro posições, retratos e ícones originais, ligada a recrutamento e colocação. VA 0x417031 e decoder 0x45dac9 emulados; relatório native_action_bar_v32.json. Clique no retrato do fazendeiro e adesão à obra verificados por capturas instrumentadas. 352 verificações focadas e 347 gerais passaram. Pausa/pesquisa/objetivos/vitória e interface integral ainda precisam de reconstrução; não declarar todos os menus originais. Originais preservados.
