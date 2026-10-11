# Mouse: seleção e ordens — v12, 27/09/2026

Release: `releases/mouse-v12/RisingLandsRemake.exe`.

## Mudança importante

O botão direito deixou de mover e atacar. Use:

- Esquerdo numa unidade própria: selecionar.
- Arrastar com esquerdo: selecionar uma área.
- Com unidades próprias selecionadas, esquerdo no chão: mover; no rival: atacar com os tipos implementados; no alimento: coletar com agricultor individual.
- Shift + clique: seleção adicional, sem emitir ordem contextual.
- Direito: desfazer seleção. Movimento, coleta ou ataque já ordenados continuam. Backspace continua sendo a parada provisória.
- Durante a escolha do local de construção, direito cancela a colocação sem pagar recursos e mantém o construtor selecionado.

Um rival pode ser inspecionado com esquerdo quando não há grupo próprio selecionado. Para inspecioná-lo durante uma seleção, primeiro desfaça a seleção com direito.

## Evidência original

Fonte: working/RISING/RISING-ORIG.EXE, SHA256 af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e.

**CONFIRMADO por execução completa dos cinco eventos:** WndProc 0x4182a9 recebe mensagens 0x201, 0x202, 0x203, 0x204 e 0x205 e grava, respectivamente, os bytes 0x50999e, 0x50999f, 0x5099a0, 0x5099a1 e 0x5099a2. Correspondem a esquerdo pressionado, esquerdo solto, duplo esquerdo, direito pressionado e direito solto. O caso de duplo clique foi testado dentro da área vertical de jogo.

**CONFIRMADO em ramo isolado:** 0x4299d7–0x429a27 exige os indicadores de esquerdo pressionado e solto para emitir ordem 5, passando a célula em SI. Quatro combinações testadas. O despachante ProcessInputGame associa a ordem 5 ao ramo 0x45ee76, que chama SetDeplaceEnginSelect em 0x45ee8c. Isso confirma movimento com clique esquerdo nesse contexto.

**CONFIRMADO em ramo isolado:** 0x429639–0x42968b consulta direito pressionado e chama DeSelectionGroupe e BatimentDeselectionner, exceto na zona 11. O teste cobre oito combinações de botão e zona. Essas duas chamadas são interceptadas; registramos o encaminhamento, sem executar seus efeitos no mundo.

**EVIDÊNCIA estática complementar:** diversos ramos de ação de InputGame consultam os mesmos indicadores de clique esquerdo antes de enviar SetPlayerOrdre. A v12 passa as ações contextuais já existentes ao esquerdo, mas não reconstrói todos os critérios do cursor, estados de ação ou menus originais. A escolha exata de alvo, os cliques em prédios, o minimapa, o duplo clique e as exceções de zona continuam parciais.

Ferramenta: tools/trace_mouse_buttons.py. Entradas e saídas: reverse_engineering/mouse_button_vectors.json. Resumo: reports/mouse_buttons_original_validation.json. Nenhum processo original ou API do Windows foi executado: mensagens sintéticas foram entregues à função emulada na cópia.

## Integração e limites

As ordens contextuais são emitidas ao soltar o esquerdo, apenas quando o gesto não virou arraste e Shift não está pressionado. Isso evita que selecionar um grupo também lhe dê uma ordem de movimento. Clique em outra unidade própria continua substituindo seleção. O limite entre clique e arraste permanece o limiar moderno anterior de oito pixels somados nos eixos; não foi comprovado como limiar original.

O direito também cancela um arraste pendente. Cancelamento de construção mantém o construtor por conveniência do protótipo; não foi demonstrado como reprodução de todas as rotinas originais. O protótipo não reproduz a exceção de zona 11 do original e mantém seu minimapa moderno. Grupos de agricultores ainda usam movimento coletivo, sem distribuição automática de coleta.

Saves permanecem v7, compatíveis com leitura v4–v7 na pasta desta release. Grupos e alertas permanecem informações da sessão. Nenhum arquivo ou save original é alterado.

## Verificação

223 verificações passaram em logs/selftest_mouse_v12.log. Novas regressões cobrem ordem no chão, ação em rival, seleção de unidade própria, proteção contra ordem ao arrastar, Shift e inspeção sem seleção própria. São testes da política de decisão usada pelo jogo; não equivalem a automatizar todos os gestos físicos na janela.

A captura reports/prototype_mouse_v12.png foi inspecionada para inicialização, combate e instruções atualizadas. A captura usa o treino automatizado e não prova sozinha os cliques físicos. Evidência nativa adicional: 5 eventos, 4 condições de movimento e 8 condições de cancelamento.
