# Informações das unidades e Espaço — v11

Release: `releases/info-v11/RisingLandsRemake.exe`, 27/09/2026.

## Mudança jogável

Segurar Espaço mostra as barras de vida de todas as unidades visíveis. Soltar a tecla volta a mostrar apenas as barras das unidades próprias selecionadas e da unidade sob o cursor. Espaço não pausa mais a partida. A tecla Pause alterna a pausa, como escolha provisória moderna. X continua localizando o último ataque; Backspace interrompe ordens.

As informações do painel lateral permanecem disponíveis para a unidade inspecionada. O novo comportamento altera as barras sobre as unidades, não esconde o painel. F2/F3 abrem treinos; segure Espaço para visualizar os dois lados do duelo.

## Evidência e certeza

Fonte: working/RISING/RISING-ORIG.EXE, SHA256 af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e.

CONFIRMADO por leitura estática: InputGame limpa o byte 0x508f7f em 0x429361 a cada processamento. Consulta VK_SPACE (0x20) em 0x42976b–0x429777 e define esse byte em 0x429779 enquanto pressionado. Isso demonstra um estado de tecla mantida, não um alternador de pausa nesse ramo.

CONFIRMADO por emulação: AffEngin (0x415a27), com unidades de tipos 0, 5, 7 e 8 em estado 7, solicita AffInfoSelection (0x410be4) quando a unidade é do jogador local e está selecionada, ou corresponde ao índice 0x496ac2, ou o byte 0x508f7f está ativo. O teste cobre todas as combinações de dono próprio/outro, seleção, índice de destaque e estado da tecla: 64 casos.

Trechos estáticos equivalentes encontrados em 0x412a53–0x412a95, 0x4139b8–0x4139fa, 0x4148c5–0x414903 e 0x4152d4–0x415312. As comparações de dono usam 0x508031 e a seleção da unidade usa memória+189.

A rotina de informações é interceptada, contando solicitações. **Não foi reconstruído nesta etapa seu desenho completo**, que pode incluir outros indicadores. A versão moderna aplica a condição confirmada às barras de vida existentes. O formato, a cor e a posição das barras continuam modernos.

**APROXIMAÇÃO:** o índice de destaque é associado ao cursor pelo teste de limites gráficos já usado no protótipo. A forma exata de apontar uma unidade no original ainda precisa ser reconstruída. Os 64 testes cobrem estado 7 de quatro tipos, não todos os estados e unidades do jogo.

## Reprodução

`tools/trace_unit_info.py` executa as rotinas na cópia em memória, sem iniciar o jogo original. Saídas: `source_remake/Data/unit_info_rules.json` e `reports/unit_info_original_validation.json`.

`logs/selftest_info_v11.log`: regressões do executável, incluindo comparação dos 64 casos. Capturas `reports/prototype_info_v11_normal.png` e `reports/prototype_info_v11_all.png`: visual normal e opção de captura que força a mesma condição de exibição de Espaço. A opção `--capture-info` só tem efeito junto de `--capture`; não altera saves nem o relógio do jogo e não simula fisicamente uma tecla.

A pausa provisória em Pause não foi atribuída ao original. Menus, cliques, conteúdo integral das informações e atalhos restantes continuam parciais. Alertas e grupos seguem limitados à sessão; saves continuam no formato v7.
