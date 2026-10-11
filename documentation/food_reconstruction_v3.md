# Coleta de alimento — versão 3, 26/09/2026

## Resultado e limites

Primeiro ciclo interativo: agricultor do jogador, deslocamento até alimento, retirada de uma unidade, carga, retorno ao santuário/armazém e entrega. Repete a fonte escolhida até esgotar. Tecla C seleciona um agricultor e procura alimento acessível; botão direito sobre a célula de alimento também inicia coleta. Outros cliques dão ordem de movimento e interrompem o trabalho, preservando carga. Interface mostra estoque, carga e atividade. Objetos de alimento são redesenhados quando retirados ou depositados. Save de exploração passa a JSON versão 3, contendo objetos, quantidades, estoque e estado do trabalho; versões anteriores continuam preservadas em suas pastas.

Isto NÃO é economia completa: somente agricultor tipo 5 pode iniciar a ação. Mineração, máquinas, consumo, construção, recrutamento, combate, IA e eventos permanecem pendentes. Animações específicas de trabalho/carga ainda não reconstruídas. A interface usa a sequência visual disponível e informa carga numericamente.

Controlador moderno de coleta é parcial: BFS, escolha de fonte/base mais próxima e cadência 30 Hz são de laboratório. Retirada de saco espera 10 ticks (constante observada em EnginCalculTransporte); colheita espera 60 ticks provisórios, sem reproduzir bônus/configuração do original. Original possui busca de outra célula ao ultrapassar 31 itens na pilha; esta versão pausa com a carga preservada. Se local de entrega estiver ocupado ou sem caminho, pausa. Não implementa movimentação dinâmica entre unidades nem a máquina de estados original integral.

## Evidências

Todas as rotinas abaixo pertencem a `working/RISING/RISING-ORIG.EXE`, hash preservado no projeto. Scripts emulam instruções em memória isolada, sem executar o binário como processo Windows.

- **CONFIRMADO:** EnginTypeGetCapacite, VA 0x4475ac: capacidade de agricultor 5 = 1; máquina 13 = 5. 26 tipos extraídos por execução. `source_remake/Data/food_rules.json`.
- **CONFIRMADO:** TerrainEstGc 0x4761e5 aceita objetos 29..52. TerrainRecolte 0x478114 limpa 29..50; 51 e 52 tornam-se 41 e 42. 256 entradas comparadas com instruções originais. Hipótese inicial de reduzir também 39..50 foi refutada pelo teste e descartada.
- **CONFIRMADO:** TerrainGetSacType 0x475f64: códigos 66..68 correspondem a recursos 0..2. Código 67 é tipo 1, alimento. `LoadRessource` 0x46a92a lê quatro jogadores do recurso 1, depois 2 e depois 0. Campos de custo COUTPATATE no RISING.INI ajudam a vincular o nome.
- **CONFIRMADO:** TerrainRetireMatiere 0x47658e reduz quantidade no plano 3 e limpa objeto no plano 2 quando zerado. 256 quantidades exercitadas. Planos ETA: objetos +196612, quantidades +262148.
- **CONFIRMADO:** MONDE01 célula 5642 contém código 67, quantidade 14. Recursos iniciais alimento/rocha do jogador 0 = 30/30. Santuário tipo 2 tem âncora 2828; célula de descarte 2828 + 0x303 = 3599.
- **CONFIRMADO sob condições controladas:** BatimentAjouteMatiere 0x4234a7 usa quantidade de armazéns e capacidade 100, descontando a reserva inicial ao medir ocupação. 27 casos de entrega comparados com execução original, incluindo nenhum armazém e limite atingido. Configuração base de capacidade em 0x4190e0; contagem de tipo 3 em BatimentInitCave 0x4191b2. Tipos 4 e comportamentos especiais não foram implementados.
- **CONFIRMADO:** excedente no santuário é encaminhado a TerrainDeposeSac em âncora +0x303 (0x423635–0x423649). Sem armazém na primeira missão, o estoque fica em 30 e a comida aparece em pilha junto à base. Emulação captura essa chamada de excedente, não sua busca integral no mundo.
- **CONFIRMADO:** CreerSac 0x475f89 inicializa quantidade 1 ao usar célula vazia e limita pilha a 31, procurando outra célula quando cheia. A versão 3 conserva a carga ao encontrar pilha cheia; a busca alternativa está pendente.
- **FORTE EVIDÊNCIA:** imagens variáveis de alimento extraídas de AffVegetTerrain 0x410085, com visibilidade e tempo controlados. 279 variantes catalogadas. Isso não comprova crescimento/regeneração nem todos os efeitos do motor.

## Validação e reprodução

`tools/trace_food.py` gera catálogo e relatórios: `reports/food_original_validation.json`, `reports/food_deposit_original_validation.json`. Testes C# exercitam consumo real da pilha inicial, carga, depósito em chão, estoque com armazém sintético, disputa entre dois agricultores pelo último item, pilha cheia, rejeição atômica de save inválido e continuidade determinística após save/load. Fixture de armazém é teste sintético; não modifica mapas nem cria construção no jogo.

Executável em `releases/food-v3/RisingLandsRemake.exe`. Registro final em `logs/selftest_food_v3_published.log`. Captura real em `reports/prototype_food_v3_final.png`. Revisão visual e testes automatizados não equivalem a teste manual completo de teclado/mouse nem validação em ambas as versões do Windows.

## Próxima etapa

Reconstruir construção do armazém: disponibilidade, custos, terreno permitido, duração, trabalhador, conclusão e incremento de capacidade. Depois integrar recrutamento/consumo e combate. Não executar `document_project.py` indiscriminadamente: documentos históricos gerados por ele podem substituir registros novos; este arquivo independente deve ser preservado.

Validação final: 97 verificações aprovadas no executável publicado; zero avisos de compilação. Regras de colheita/retirada/depósito comparadas em 539 casos com rotinas originais.
