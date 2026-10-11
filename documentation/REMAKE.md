# Reconstrução: arquitetura e critérios de fidelidade

Este é um plano de implementação, não descrição de uma engine moderna já pronta. A pesquisa recuperou ativos, formatos e partes da lógica; ainda não autoriza afirmar equivalência de gameplay.

## Base de referência

Fixar como referência o executável original SHA-256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`, igual à cópia do CD. Manter variantes modificadas em perfis separados. O executável atual contém alteração de temporização: hook em `0x474f38`, seção adicional `.spd`, espera por GetTickCount. Misturar esta variante com a referência contamina medições de velocidade.

## Separação proposta

1. Importador offline: formatos legados → pacote versionado, preservando IDs, paletas, máscaras, dados desconhecidos e hashes de origem. Os leitores Python existentes são referência inicial; um importador de produção precisa limites explícitos de memória e mensagens para arquivo inválido.
2. Simulação determinística: estado de entidades, recursos, construção, pesquisa, combate, IA, objetivos e RNG. Tempo lógico inteiro e serialização independente de ponteiros. Não copiar tamanhos de structs antigas sem interpretar campos.
3. Apresentação: renderização 2D indexada/paletas, ordenação de sprites, interface, câmera, áudio e vídeo; nenhuma regra deve depender da taxa de quadros.
4. Plataforma Windows 10/11: janela, entrada, armazenamento e dispositivos via camada substituível. A escolha final da biblioteca deve ocorrer após protótipo que teste paletas, transparência, vídeo, latência e empacotamento nas máquinas alvo.
5. Laboratório de equivalência: comandos/replays, snapshots por tick, diferenças por campo e evidências vinculadas à rotina original. Não tornar semelhança visual o único critério.

## Sequência de entregas

| Etapa | Entrega verificável | Condição de conclusão |
|---|---|---|
| 1 — Importação | sprites, imagens, áudio, mapas e configurações normalizados | todos os bytes classificados; diferenças conhecidas documentadas; casos inválidos rejeitados |
| 2 — Terreno | mapa navegável, paletas e classificação de tiles | comparação com referência e capturas equivalentes |
| 3 — Entidades | tabelas de unidades/construções, animações e ordens | campos e estados decodificados; cenários mínimos reproduzíveis |
| 4 — Economia/combate | custos, coleta, produção, dano, alcance, pesquisas | transições e ticks iguais em experimentos controlados |
| 5 — Caminhos/IA | movimento, desempates, planejamento e RUL | mesmas decisões para entradas fixas; RNG identificado |
| 6 — Campanha | eventos, objetivos, vitórias/derrotas e progressão | cada missão e ramificação com teste rastreável |
| 7 — Saves | importador legado e save moderno versionado | round-trip sem perda do estado conhecido e determinismo após load |
| 8 — Produto | executável Windows, instalação, opções e acessibilidade | matriz Windows 10/11, resolução/DPI/áudio, partidas longas sem divergência |

## Lógica já recuperada

`source_remake/terrain_reference.py` contém 16 predicados de terreno transcritos de rotinas nomeadas, com comparação diferencial emulada. É código de referência reaproveitável, não jogo funcional. Predicados podem se sobrepor: montanha e alto de montanha não são categorias exclusivas. `TerrainEstMarecage` retorna falso nesta versão, apesar do nome. Não inventar comportamento para preencher funções vazias.

O contador em `0x474f38` avança um contador primário; os outros avançam a cada múltiplo de 2 e 3. Ainda é necessário descobrir todos os consumidores, o relógio externo e o significado das velocidades antes de fixar ticks por segundo. `CheminCalcul` está nomeada em `0x42f2b8`, mas a estratégia de busca e desempates não estão reconstruídos. Não substituir por A* e chamar isso de fiel.

## Experimentos necessários

Preparar instalação de laboratório inteiramente separada, incluindo saves e configurações. Registrar hash da versão, resolução, velocidade, seed quando identificada, sequência de comandos e instante de captura. Comparar uma variável por vez: coleta de um recurso, produção de uma unidade, ataque isolado, obstáculo único, rota com empate, mudança de pesquisa e save/load no mesmo tick. Execução do jogo não ocorreu nesta pesquisa; timings de parede e comportamento integrado permanecem sem medição.

As tarefas de maior retorno são: interpretar os campos de entidades a partir dos getters; transcrever a VM RUL com suas chamadas; recuperar caminhos e RNG; decodificar a sequência integral de GameSave/GameLoad; identificar uso da cauda ETA; mapear eventos de campanha aos vídeos e condições. Cada implementação deve apontar para hash, VA/offset, método e teste.
