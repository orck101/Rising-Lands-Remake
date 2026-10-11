# v57 — coleta, cultivo, formações e salvamento

## Abrir e testar

Abrir releases/interactions-v57/Jogar-Primeira-Missao.cmd. O menu Carregar partida (ou F9) mostra os mesmos cinco slots em qualquer missão e abre a missão do save escolhido. Jogar-Segunda-Missao.cmd inicia diretamente a missão 2. Originais em D:/RISING e seus saves não foram modificados.

## Seis alterações

1. **Salvamento:** cinco slots de campanha compartilhados, com missão e data no menu. O carregamento prepara e valida uma simulação separada antes de trocar o cenário, sem selecionar personagens. Saves antigos por missão são encontrados automaticamente; colisões de números são distribuídas em slots livres, sem mover ou reescrever os arquivos. Novos slots prevalecem sobre seu correspondente antigo. Gravação em temporário, descarga para disco e substituição do arquivo; sobrescrita confirmada preserva uma cópia .bak. JSON inválido pode ser recuperado da cópia anterior; outros erros de validação preservam a partida atual. Novos saves são v22 e a leitura de versões 4–21 permanece disponível. O diretório estável continua sendo UserSaves, separado das versões do executável.
2. **Construtor:** quando acaba uma pilha, procura pedra solta na região da fonte, mantendo entrega e repetição. Mina esgotada também pode continuar em pedra solta próxima. Novas ordens e Stop encerram o ciclo. A busca local reaproveita o raio provisório de seis células; não se apresenta esse raio como algoritmo original confirmado.
3. **Comida no chão:** mantém o ponto escolhido como referência. Enche a pilha até 31 e procura o ponto válido mais próximo, primeiro adjacente, dentro de três células ao redor da referência. Aproveita pilhas compatíveis. Se a região não tiver caminho ou espaço, espera com a carga preservada. O histórico dos pontos de entrega é salvo e evita recolher a própria pilha durante o mesmo ciclo.
4. **Armazém cheio:** ordena pontos pela distância ao contorno do prédio, antes da distância ao coletor. Completa pilhas compatíveis no anel mais próximo antes de abrir outra pilha nesse anel. Mantém o perímetro completo, inclusive diagonais, até três células para fora. Quantidades, carga e estoque são conservados; não há crédito fictício de recurso no chão. Destino e ciclos sobrevivem a save/load.
5. **Cultivo:** descarrega um alimento por vez, com recarga de dez contagens entre itens, preservando a arte de carga até descarregar. Cada entrega no cultivo cheio retira um colono e deposita aquele alimento no chão. O contador nativo é limitado a cinco; excedente entregue quando esse contador já está cheio é consumido pelo mecanismo nativo, contabilizado em Consumed para manter a auditoria. Um cultivo vazio pode receber comida, mas precisa de colono para gerar outro. Nascimento segue custo 5/4/3 conforme agricultura, sem temporizador artificial de recrutamento. A escolha exata do ponto de chão e a equivalência de ticks com segundos permanecem adaptações.
6. **Formações:** seleção somente de tropas próprias de combate mostra Quadrado, Linha e Triângulo no painel inferior. Unidades com maior alcance do catálogo ficam nas posições mais próximas do centro geométrico; menores alcances ficam nas posições externas. O triângulo conserva seus vértices e acomoda quantidades intermediárias no interior. O planejamento reserva destinos distintos, verifica caminhos e mantém as ordens anteriores se não conseguir organizar todos. Uma formação válida cancela a ordem anterior de combate/entrada no santuário. Movimentar o mesmo grupo mantém a formação escolhida; trocar a seleção exige escolher novamente. Posições e rotas são salvas; o modo de seleção da interface não é persistido. Obstáculos podem adaptar posições em até três células. É uma funcionalidade solicitada para o remake, não uma formação original comprovada.

## Diagnóstico e evidências

**CONFIRMADO no remake:** ContinueHarvest limitava a busca regional ao fazendeiro; construtores com pilha esgotada tentavam somente a fonte anterior. Deliver recusava o ponto de chão cheio e repetia a espera. WarehouseOverflow priorizava distância ao trabalhador, dispersando os pontos escolhidos. O teste anterior à correção falha depois de encher a pilha: logs/v57-before-test-v57.log/.err.

**CONFIRMADO no salvamento:** os quatro arquivos de UserSaves estavam presentes e carregaram corretamente antes da mudança (logs/v57-before-check-player-saves.log). O menu os filtrava pelo mapa atual. O novo menu compartilha slots e abre o mapa do arquivo. Testes verificam colisões de número entre missões, reabertura do diretório, substituição com backup e recuperação de JSON danificado.

**CONFIRMADO no original copiado:** BatimentAjouteMatiere, entrada 0x4234a7, ramo cultivo 0x4235a9–0x4235ed. Campo de alimento em building+0x78; uma chamada incrementa uma unidade até cinco. Sem vaga no alojamento, chama retirada em 0x4235d0 e deposita uma unidade no terreno em 0x4235e8, posição de referência duas linhas abaixo da âncora. Foram executados 36 vetores (habitantes 0–5 × alimento 0–5), com disponibilidade de alojamento, retirada e depósito explicitamente interceptados. Ferramenta tools/trace_crop_delivery_v57.py; resultados reverse_engineering/crop_delivery_v57.json. Executável copiado RISING-ORIG.EXE, SHA256 af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e.

**CONFIRMADO por análise estática do chamador:** EnginCalculTransporte recarrega contador com 0x0a em 0x44f08d; passa quantidade 1 em 0x44f0ad e chama a entrega em 0x44f0c7; retira o item da carga em 0x44f0dc. O custo de geração e bônus continuam baseados nos 168 vetores e preços da v40, não em novas hipóteses. Não foi emulado o loop completo do jogo nem o relógio global do original.

## Validação

- 1.511 verificações aprovadas: missão 1/interações 782, missão 2 83, gerais 351, melhorias v57 295.
- Mais 36 vetores da rotina original de entrega ao cultivo.
- Quatro saves reais do usuário carregados em leitura, com comparação antes/depois dos bytes (8 verificações em logs/v57-check-player-saves.log).
- Fonte de pedra esgotada, mina esgotada e pedra próxima; pilha cheia em trânsito; transbordo cheio/obstruído; conservação e continuidade de recursos após save/load.
- Cultivo com carga ampliada, descarga unitária, dez contagens, save durante descarga; cultivo sem colono e contador cheio; geração, lotação e pesquisas anteriores mantidas.
- Três formas com posições únicas para grupos de 1–80; alcance central, chegada a destinos distintos, rejeição de trabalhadores/inimigos e cancelamento de entrada anterior.
- Interface real: salvar duas missões em slots distintos, reabrir o mapa inicial, carregar ambas, rejeitar metadados inválidos preservando a partida, pedir confirmação antes de substituir.
- Capturas conferidas: reports/v57-verified-formations-v57-square.png, reports/v57-verified-formations-v57-line.png, reports/v57-verified-formations-v57-triangle.png, reports/v57-saves-final.png e reports/v57-verified-overwrite.png.
- Missão 2 continua exigindo santuário e quartel acabados e habitados, mais 50 alimentos, e avançando para a missão 3.

## Registro das mudanças

Arquivos de produção alterados: Engine/FoodSimulation.cs, MiningSimulation.cs, WarehouseOverflow.cs, SaveSlots.cs, Simulation.cs, PauseMenu.cs, ResearchGame.cs, SelectionClasses.cs, NativeActionBar.cs e Importers/LegacyAssets.cs. Novos módulos: DeliveryIntelligence.cs, TroopFormations.cs, FormationBar.cs e SavePersistenceCapture.cs. Entrada de testes em Program.cs; novo ImprovementsV57Tests.cs; ajustes em FoodTests.cs, CollectionV50Tests.cs e SaveSlotTests.cs para a regra nova e o save v22. Os arquivos de dados originais e as evidências anteriores foram preservados.

Limites gerais do projeto: IA nativa, final da missão 3 e efeitos de pesquisa ainda pendentes não foram concluídos por esta entrega. O contador de descarga usa ticks da simulação moderna; não há afirmação de reprodução do tempo real original.
