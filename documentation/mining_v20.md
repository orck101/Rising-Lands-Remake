# Mineração — mining-v20

Data: 28/09/2026. Executável: `releases/mining-v20/RisingLandsRemake.exe`.

## Entrega jogável

Construtor (tipo 7) pode extrair rocha de jazidas importadas dos mapas ETA. Clique na jazida ou pressione M para procurar a mais próxima acessível. E seguido de clique no armazém/santuário define o destino; clique direto na estrutura também funciona. Sem destino, a carga fica com o construtor. Repetir controla novas viagens à mesma jazida. Não muda automaticamente para outra mina.

A entrega credita rocha somente na chegada e compartilha os 100 lugares do armazém com alimento, descontada a reserva inicial conforme a política de estoque já existente. Sem espaço, preserva a carga e tenta novamente. Construir, retomar obra, mover ou parar interrompe mineração, sem descartar rocha. Voltar a entregar requer E na estrutura quando a ordem anterior foi interrompida.

A jazida esgotada muda para os dois tiles observados no original. Selecionar o minerador mostra carga, atividade, destino e quantidade restante da jazida. B mantém a lista de construções e tecnologias. Pesquisa e agricultura das versões anteriores permanecem disponíveis.

## Evidências

- **CONFIRMADO:** `LoadMine`, VA 0x475276, lê uint16 count, seguido de count registros de quatro bytes: célula uint16 e reserva uint16. Tabela nativa em 0x509944, passo 4; `MineGetCapacite` 0x47546d lê +2. O bloco segue `LoadEngin`, conforme 0x46c40c–0x46c411. Catálogo dos 34 mapas em `extracted/mining_catalog.json`, incluindo offset e hash de cada mapa.
- **CONFIRMADO:** MONDE01.ETA offset 340520: célula 2091, reserva 50; offset 340524: célula 15110, reserva 200. Dados lidos da cópia preservada, sem recursos extras na partida.
- **CONFIRMADO POR EXECUÇÃO EMULADA:** `MineRecolte` 0x4754d3 e sua chamada a `MineGetIndice` 0x475378 aceitam âncora, +1, +256 e +257; retiram min(1,reserva); não deixam reserva negativa. Ao esgotar, tiles da âncora e +1 tornam-se 0x1db/0x1dc. 30 combinações de quantidade (0,1,2,31,255,65535) e célula, incluindo fora da área. Relatório `mining_original_validation.json`.
- **CONFIRMADO SOB ESTADO CONTROLADO:** `BatimentAjouteMatiere` 0x4234a7 com recurso 2 aceita rocha até a capacidade restante, inclusive quando ocupada por comida. 21 combinações de armazéns, alimento armazenado e entrega. A colocação de excedentes no chão foi interceptada, não reproduzida. Relatório `mining_deposit_validation.json`.
- **CONFIRMADO ANTERIORMENTE:** capacidade básica tipo 7 = 1, extraída por `EnginTypeGetCapacite` 0x4475ac; `food_rules.json`.
- **FORTE EVIDÊNCIA ESTÁTICA:** a lógica próxima de 0x44c14b chama MineRecolte; 0x44c166/0x44c16b insere recurso 2 na bolsa; 0x44c175 recarrega contador 0x32 (50). Isto não confirma 50 atualizações completas do jogo por extração; o controlador original tem estados e bônus adicionais.

Método reproduzível: executar `tools/trace_mining.py`. As instruções originais são emuladas em memória isolada, com terreno e inventário explicitamente preparados. Não se executa nem modifica o EXE original como processo Windows.

## Política provisória e limites

**PROVISÓRIO:** navegação BFS até perímetro 2x2, escolha da mina mais próxima, trabalhador exposto somente como construtor tipo 7, 50 ticks por retirada no controlador moderno, retentativa a cada 30 ticks, painéis/atalho M. Elegibilidade completa de outros trabalhadores/máquinas e ciclo de entrada/saída, animações e timing nativo continuam não reconstruídos. A capacidade básica é confirmada, mas não implica confirmação de toda a política de trabalhadores. Sem áudio de mineração, bônus, transporte de pilhas de rocha no chão ou descarte voluntário de rocha. A política de segurar excedente difere da colocação nativa no terreno.

## Saves e integridade

Formato próprio v11 grava reservas das minas e estado do minerador. Versões 4–10 continuam aceitas com jazidas originais intactas e sem criar ordens de mineração. Restaurar reconstrói os tiles esgotados; reiniciar recupera terreno e reserva originais. Validação exige identidade das minas, reserva não crescente, carga limitada, trabalhador válido, ausência de obra/mineração simultâneas e conservação global de rocha incluindo gastos. Rejeição não altera a partida corrente.

Nenhum arquivo em D:/RISING foi escrito. 1.340 arquivos preservados de original/working comparados com o inventário inicial. Alterações ficam em source_remake, nova release, tools, extracted, tests, reports, logs e documentation da área separada.

## Verificações

344 verificações aprovadas no executável publicado; 35 adicionais de mineração, incluindo importação, quatro células clicáveis, ordens inválidas, reserva/carga/estoque, construção com carga preservada, esgotamento da mina real de 50, save/load determinístico, terreno após carregar/reiniciar, conservação, migração v10, limite compartilhado com comida e disputa entre dois construtores (segundo recrutado pelo santuário).

Casos de limite e disputa usam reservas sintéticas de 200 e 1 na primeira jazida em mundos de teste; não alteram mapas nem recursos do jogo normal. Testes usam limites de atualizações para detectar travamentos, não durações do jogo original.

Compilação sem avisos/erros. Captura `prototype_v20_mining_a.png` inspecionada: missão 1 original, armazém construído pagando recursos, construtor à mina por ordens reais. Não equivale a teste manual completo ou validação em Windows 10 e 11 separadamente. Relatório de hashes: `reports/mining_v20_validation.json`; log: `logs/selftest_mining_v20.log`.

## Continuidade

Próxima lacuna: integrar a avaliação e apresentação do fim da missão 1, usando as condições já reconstruídas em `mission_one_conditions.md` e o ciclo de recursos agora disponível. Campanha automática, funções especializadas dos prédios, outros ramos/bônus de pesquisa, IA estratégica, áudio e fidelidade integral de comandos permanecem incompletos. Não é o remaster final.
