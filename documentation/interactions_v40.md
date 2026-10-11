# Interações e cultivo — v40

Originais e saves do jogo não foram escritos. Implementação em source_remake; versão jogável em releases/interactions-v40/Jogar-Primeira-Missao.cmd. Save do remake: v17; leitura de versões 4–16 mantida.

## Comandos com carga

Causa: SourceAvailable exigia carga zero; o gesto sempre iniciava coleta, mesmo quando só era necessário redirecionar a entrega. Agora o trabalhador carregado, com Ctrl, pode indicar diretamente um destino válido; a comida permanece na mão até a entrega. Solo, armazém e cultivo são aceitos para fazendeiros. Uma fonte plantada pode iniciar o gesto de dois cliques mesmo com carga; a carga atual é entregue primeiro. Fonte e destino são validados em uma cópia da simulação antes de aplicar o comando. Soltar Ctrl cancela o gesto incompleto. A seleção usa os limites da sprite correspondente ao estado atual, incluindo carregado.

## Cultivo: evidência original

Executável de evidência: working/RISING/RISING-ORIG.EXE, SHA256 af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e.

CONFIRMADO: BatimentNextEtape, entrada 0x42696c, caso cultura 0x426ca8–0x426df4. O contador de comida é word em building+0x78. Exige ao menos um colono, capacidade no alojamento e espaço para uma nova unidade. Requisito = 5 menos BonusGetValEngin(player,2,9), chamado em 0x426d66. A verificação compara comida diretamente em 0x426d7f, sem temporizador de recrutamento. Ao produzir, zera o contador inteiro em 0x426db3, cria tipo9, aloja e coloca estado14. Foram executados 168 vetores controlados: habitantes 0/1/4/5, descontos 0/1/2, comidas 0..6, espaço global disponível/indisponível. Dependências de contagem, criação e alojamento foram interceptadas e são declaradas no relatório.

CONFIRMADO: efeitos de AGRICULTURE5 e AGRICULTURE14, reproduzidos pelo executável via InventionSetEffet/BonusGetValEngin. Níveis 0–5 exigem5; níveis6–14 exigem4; níveis15–19 exigem3. Dados em source_remake/Data/culture_native.json. A configuração COLON/COUTPATATE=2 não é o custo deste mecanismo! I_DonneSeuilPatateProdColon=75 é um limiar de IA, também não é o custo.

CONFIRMADO por análise estática: BatimentAjouteMatiere, 0x4235a9: contador limitado a5; alojamento cheio leva à retirada de um colono (0x4235ce) e depósito do alimento no terreno (0x4235e8), perto da posição +2 linhas. O remake reproduz a saída de um colono e o depósito, escolhendo uma célula acessível do perímetro para evitar sobrepor construção. Essa escolha exata de célula é uma ADAPTAÇÃO, ainda não uma reprodução do algoritmo TerrainDeposeSac.

As regras provisórias de10 alimentos e300 ciclos foram removidas. O campo antigo Progress permanece zerado apenas para compatibilidade. Consumed registra a comida efetivamente consumida; saves v16 preservam o consumo histórico de10 por nascimento, sem recriar ou apagar unidades. Novos nascimentos usam o mecanismo comprovado. Agricultura passou a poder pesquisar os efeitos já implementados: colheita, carga, preço dos colonos e construções com assets disponíveis. Sua evolução e efeitos são salvos. A primeira missão não libera laboratório; o teste de pesquisa usa MONDE03. Não se deve confundir isso com completar todos os ramos e efeitos do jogo.

DESCONHECIDO: frequência real de atualização global do original em tempo de parede. O remake verifica o cultivo no seu próximo passo de simulação; não existe o atraso artificial da v39. O limite de1000 unidades geradas é uma proteção do remake, não um limite original comprovado.

## Cursores originais

CONFIRMADO: banco GRAPHICS/OTHERS.CMP, base global7640; frames extraídos existentes em converted/sprites/OTHERS. Sprite global =7640+índice do PNG. Desenho com paleta e transparência originais; sem redesenho ou recoloração. O tamanho da textura é preservado.

| Uso | IDs globais | PNGs locais no banco | Evidência |
|---|---|---|---|
| Coleta, verde para cima | 0x1e3d–0x1e3f | 00101–00103 | InputGame 0x42ad40, 0x42ad4e |
| Entrega, vermelho para baixo | 0x1e40–0x1e42 | 00104–00106 | InputGame 0x429c8a, 0x429c98 |
| Entrada, porta | 0x1e16–0x1e18 | 00062–00064 | InputGame 0x42a9fc |
| Proibido | 0x1e0e | 00054 | InputGame 0x429c71 |
| Trabalho, ferramenta animada | 0x1e10–0x1e15 | 00056–00061 | InputGame 0x42afd8 e outros contextos |

As cores do executável disponível diferem do exemplo lembrado pelo usuário. Preservamos as cores observadas nos arquivos e no código original. Coleta/entrega/entrada usam três frames, contador módulo3. O desenho original em0x417dcb chama PasteBobCmp na própria posição do mouse, sem centralizar; ECX=1 escolhe clipping, não origem central. Foram executados36 vetores de atribuição e desenho com posições conferidas. O cursor do remake foi ajustado a esse posicionamento. A cadência é baseada nos desenhos do remake; a frequência real do contador0x508f74 continua não medida.

Os vetores de cursor executam os trechos após validação; não equivalem a emular InputGame inteiro. As restrições por trabalhador vêm da política já extraída; o remake valida fonte, carga, dono e destino. Os adornos de cesta e caixas de texto no mouse foram removidos. A ferramenta original substitui o retrato/texto usado para indicar ajuda em obra.

## Entrada do colono

CanEnterBuilding é uma consulta sem alterações na unidade. Valida tipo9, unidade visível e viva, dono, reservas, ocupação e viagens pendentes, capacidade e caminho. EnterBuilding usa a mesma consulta; o colono fica visível durante o caminho e só entra na chegada. Capacidades-base vêm do LOGEMENT dos22 tipos em RISING.INI, exportadas para resident_capacities.json; bônus futuros de alojamento ainda não foram reconstruídos.

## Verificação e limites

Relatórios: reports/v40-validation.json; logs v40-test-mission-one.log e v40-self-test.log. Capturas da interface: coleta, entrega, entrada, fazendeiro carregado e pesquisa. Incluem um comando real de redirecionamento ao armazém pela interface, com carga conferida antes/depois. Testes automáticos conferem destinos, cancelamento, conservação, nascimento sem espera, lotação, pesquisa e save/load. A aparência foi inspecionada nas capturas; testes automáticos não substituem comparar uma partida completa do original.
