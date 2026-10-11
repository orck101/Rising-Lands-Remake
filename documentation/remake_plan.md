# ReconstruÃ§Ã£o: arquitetura e critÃ©rios de fidelidade

Este Ã© um plano de implementaÃ§Ã£o, nÃ£o descriÃ§Ã£o de uma engine moderna jÃ¡ pronta. A pesquisa recuperou ativos, formatos e partes da lÃ³gica; ainda nÃ£o autoriza afirmar equivalÃªncia de gameplay.

## Base de referÃªncia

Fixar como referÃªncia o executÃ¡vel original SHA-256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`, igual Ã  cÃ³pia do CD. Manter variantes modificadas em perfis separados. O executÃ¡vel atual contÃ©m alteraÃ§Ã£o de temporizaÃ§Ã£o: hook em `0x474f38`, seÃ§Ã£o adicional `.spd`, espera por GetTickCount. Misturar esta variante com a referÃªncia contamina mediÃ§Ãµes de velocidade.

## SeparaÃ§Ã£o proposta

1. Importador offline: formatos legados â†’ pacote versionado, preservando IDs, paletas, mÃ¡scaras, dados desconhecidos e hashes de origem. Os leitores Python existentes sÃ£o referÃªncia inicial; um importador de produÃ§Ã£o precisa limites explÃ­citos de memÃ³ria e mensagens para arquivo invÃ¡lido.
2. SimulaÃ§Ã£o determinÃ­stica: estado de entidades, recursos, construÃ§Ã£o, pesquisa, combate, IA, objetivos e RNG. Tempo lÃ³gico inteiro e serializaÃ§Ã£o independente de ponteiros. NÃ£o copiar tamanhos de structs antigas sem interpretar campos.
3. ApresentaÃ§Ã£o: renderizaÃ§Ã£o 2D indexada/paletas, ordenaÃ§Ã£o de sprites, interface, cÃ¢mera, Ã¡udio e vÃ­deo; nenhuma regra deve depender da taxa de quadros.
4. Plataforma Windows 10/11: janela, entrada, armazenamento e dispositivos via camada substituÃ­vel. A escolha final da biblioteca deve ocorrer apÃ³s protÃ³tipo que teste paletas, transparÃªncia, vÃ­deo, latÃªncia e empacotamento nas mÃ¡quinas alvo.
5. LaboratÃ³rio de equivalÃªncia: comandos/replays, snapshots por tick, diferenÃ§as por campo e evidÃªncias vinculadas Ã  rotina original. NÃ£o tornar semelhanÃ§a visual o Ãºnico critÃ©rio.

## SequÃªncia de entregas

| Etapa | Entrega verificÃ¡vel | CondiÃ§Ã£o de conclusÃ£o |
|---|---|---|
| 1 â€” ImportaÃ§Ã£o | sprites, imagens, Ã¡udio, mapas e configuraÃ§Ãµes normalizados | todos os bytes classificados; diferenÃ§as conhecidas documentadas; casos invÃ¡lidos rejeitados |
| 2 â€” Terreno | mapa navegÃ¡vel, paletas e classificaÃ§Ã£o de tiles | comparaÃ§Ã£o com referÃªncia e capturas equivalentes |
| 3 â€” Entidades | tabelas de unidades/construÃ§Ãµes, animaÃ§Ãµes e ordens | campos e estados decodificados; cenÃ¡rios mÃ­nimos reproduzÃ­veis |
| 4 â€” Economia/combate | custos, coleta, produÃ§Ã£o, dano, alcance, pesquisas | transiÃ§Ãµes e ticks iguais em experimentos controlados |
| 5 â€” Caminhos/IA | movimento, desempates, planejamento e RUL | mesmas decisÃµes para entradas fixas; RNG identificado |
| 6 â€” Campanha | eventos, objetivos, vitÃ³rias/derrotas e progressÃ£o | cada missÃ£o e ramificaÃ§Ã£o com teste rastreÃ¡vel |
| 7 â€” Saves | importador legado e save moderno versionado | round-trip sem perda do estado conhecido e determinismo apÃ³s load |
| 8 â€” Produto | executÃ¡vel Windows, instalaÃ§Ã£o, opÃ§Ãµes e acessibilidade | matriz Windows 10/11, resoluÃ§Ã£o/DPI/Ã¡udio, partidas longas sem divergÃªncia |

## LÃ³gica jÃ¡ recuperada

`source_remake/terrain_reference.py` contÃ©m 16 predicados de terreno transcritos de rotinas nomeadas, com comparaÃ§Ã£o diferencial emulada. Ã‰ cÃ³digo de referÃªncia reaproveitÃ¡vel, nÃ£o jogo funcional. Predicados podem se sobrepor: montanha e alto de montanha nÃ£o sÃ£o categorias exclusivas. `TerrainEstMarecage` retorna falso nesta versÃ£o, apesar do nome. NÃ£o inventar comportamento para preencher funÃ§Ãµes vazias.

O contador em `0x474f38` avanÃ§a um contador primÃ¡rio; os outros avanÃ§am a cada mÃºltiplo de 2 e 3. Ainda Ã© necessÃ¡rio descobrir todos os consumidores, o relÃ³gio externo e o significado das velocidades antes de fixar ticks por segundo. `CheminCalcul` estÃ¡ nomeada em `0x42f2b8`, mas a estratÃ©gia de busca e desempates nÃ£o estÃ£o reconstruÃ­dos. NÃ£o substituir por A* e chamar isso de fiel.

## Experimentos necessÃ¡rios

Preparar instalaÃ§Ã£o de laboratÃ³rio inteiramente separada, incluindo saves e configuraÃ§Ãµes. Registrar hash da versÃ£o, resoluÃ§Ã£o, velocidade, seed quando identificada, sequÃªncia de comandos e instante de captura. Comparar uma variÃ¡vel por vez: coleta de um recurso, produÃ§Ã£o de uma unidade, ataque isolado, obstÃ¡culo Ãºnico, rota com empate, mudanÃ§a de pesquisa e save/load no mesmo tick. ExecuÃ§Ã£o do jogo nÃ£o ocorreu nesta pesquisa; timings de parede e comportamento integrado permanecem sem mediÃ§Ã£o.

As tarefas de maior retorno sÃ£o: interpretar os campos de entidades a partir dos getters; transcrever a VM RUL com suas chamadas; recuperar caminhos e RNG; decodificar a sequÃªncia integral de GameSave/GameLoad; identificar uso da cauda ETA; mapear eventos de campanha aos vÃ­deos e condiÃ§Ãµes. Cada implementaÃ§Ã£o deve apontar para hash, VA/offset, mÃ©todo e teste.


## Decisão de plataforma desta etapa

C#/.NET8 x64 + MonoGame DesktopGL3.8.4.1, fixados no projeto. Atende à preferência do PDF e permite renderização 2D e entrada com integração direta aos parsers; DesktopGL evita a dependência de DirectX legado do WindowsDX. [Plataformas oficiais](https://docs.monogame.net/articles/getting_started/platforms.html), [empacotamento](https://docs.monogame.net/articles/getting_started/packaging_games.html). SDK portátil8.0.425 na área de trabalho. Versão fixada para reprodução, não alegação de versão mais recente.

Protótipo compilado e testado; ainda não é vertical slice da seção68. Próximo marco exige coleta/depósito, construção, combate, triggers e vitória/derrota baseados em evidências. Não preencher lacunas com regras inventadas para declarar conclusão.
