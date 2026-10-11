# Grupos numéricos — versão 9, 27/09/2026

Release: `releases/groups-v9/RisingLandsRemake.exe`.

## Controles incorporados

- Ctrl + 1–9: memorizar seleção no grupo.
- 1–9: substituir a seleção pelas unidades do grupo.
- Shift + 1–9: selecionar o grupo e centralizar a câmera.
- Ctrl tem prioridade quando Ctrl e Shift estão pressionados juntos.
- Uma unidade pertence a um único grupo numérico. Reatribuí-la transfere sua participação.
- Memorizar uma seleção vazia limpa aquele grupo. Grupos não incluem unidades inimigas; unidades derrotadas são omitidas na chamada.

No modo de construção, 1/2/3 continuam escolhendo o prédio; os atalhos de grupo ficam suspensos para evitar conflito. Esta prioridade é provisória do protótipo.

**Limitação atual:** grupos são da sessão. Trocar de mapa, trocar treino, carregar partida ou fechar o jogo os reinicia. Os saves continuam no formato v7, com leitura v4–v7 na pasta PrototypeSaves desta release. A persistência de grupos ainda não foi integrada. A centralização usa a média das posições modernas; o cálculo exato da câmera original não foi reconstruído.

## Evidências

Fonte: working/RISING/RISING-ORIG.EXE, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`.

**CONFIRMADO, ramo isolado:** InputGame, região 0x429469–0x4295dd, lê o grupo em 0x49a05a. Consulta VK_CONTROL (0x11) em 0x42947a: envia ordem 11. Sem Ctrl, consulta VK_SHIFT (0x10) em 0x429494: envia ordem 12 e chama EnginGetCentreGroupe (0x4421dd). Sem modificador, envia ordem 12 sem centralizar. O script executou 36 combinações de nove grupos e quatro estados de modificadores. As consultas de teclado são simuladas, sem APIs do Windows; o auxiliar de centralização é interceptado. SetPlayerOrdre (0x45eb83) executa a gravação nativa do comando.

**FORTE EVIDÊNCIA estática da ligação física:** região 0x418423–0x418426 subtrai 0x30 do código recebido e grava 0x49a05a. O ensaio começa com esse byte preparado; não executa o despachante completo de mensagens do Windows.

**CONFIRMADO nas amostras:** SauveSelectionGroupe (0x4725e7) percorre 800 índices, filtra vida/estado e dono, limpa o grupo solicitado de seus membros anteriores e grava esse grupo no byte +188 de cada unidade selecionada (byte +189). 72 configurações de seleção e participação foram comparadas, com outro jogador preservado. Atribuição é exclusiva porque há um único byte de grupo por unidade.

**EVIDÊNCIA estática, implementação parcial:** RestaureSelectionGroupe (0x472659) limpa seleção, procura byte +188, exclui estado 14 e aplica limite 255. Não emulamos a restauração inteira e seus efeitos sonoros; a implementação moderna usa o filtro de visibilidade existente. Não alegamos equivalência completa desses efeitos.

## Reprodução e validação

`tools/trace_control_groups.py` produz `source_remake/Data/control_groups.json` e `reports/control_groups_original_validation.json`. Execução delimitada em memória da cópia, sem iniciar o jogo legado.

`logs/selftest_groups_v9.log`: 207 verificações aprovadas, incluindo os 36 vetores de modificadores, 72 atribuições e regressões de transferência, morte, limpeza e reinício de grupos. `reports/prototype_groups_v9.png` foi inspecionado para inicialização e apresentação do rodapé. Não substitui um teste manual de todas as teclas.

`reports/groups_v9_validation.json`: hashes da publicação e integridade do corpus.

## Outras diferenças encontradas, ainda não alteradas

O ramo 0x429802–0x429828 consulta VK_X (0x58) e chama GoLastAttaque (0x440c07). Assim, X do protótipo, que interrompe ordens, não é fiel a essa função original. O próximo trabalho deve reconstruir o registro do último ataque e sua navegação antes de mudar X, preservando uma ação acessível de parar.

Espaço ativa o byte 0x508f7f em 0x429779, consumido por rotinas de desenho. Seu comportamento completo ainda precisa ser medido; pausa no protótipo permanece provisória. Menus, seleção de construções e mapeamento completo dos cliques também continuam pendentes.
