# Menu de partida — v13, 27/09/2026

Release: `releases/menu-v13/RisingLandsRemake.exe`.

## Mudança

Esc abre o menu em vez de fechar o jogo. Opções: continuar, salvar, carregar, controles e sair. Navegação por clique esquerdo ou setas/Enter. Esc volta de uma página secundária ao menu principal e, deste, à partida.

Sair abre uma página com salvar e sair, sair sem salvar ou voltar. Se salvar falhar, o jogo continua aberto e mostra uma mensagem. O primeiro item é salvar e sair. O salvamento usa o mesmo arquivo moderno de F5, sem tocar saves originais.

Enquanto o menu está aberto, o Update retorna antes de processar comandos de unidades e antes de avançar a simulação. Ao fechar, o estado anterior da pausa manual é preservado: se a partida já estava pausada com Pause, continua assim. Abrir Esc cancela um arraste em andamento. No modo de colocação de prédio, o primeiro Esc apenas cancela a colocação; o seguinte abre o menu.

A tela de controles reúne os comandos atuais, incluindo esquerdo contextual, direito para desfazer seleção, Espaço, X e grupos numéricos.

## Evidência original e limites

Fonte: cópia working/RISING/RISING-ORIG.EXE, SHA256 af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e.

**CONFIRMADO por leitura estática:** InputGame consulta Escape/VK_ESCAPE 0x1b em 0x42932a–0x42939b. Sem evento em curso, envia ordem 13 por SetPlayerOrdre em 0x4293b8. Com evento em curso, há caminho separado para EvenementEnd em 0x4293d7; esse comportamento de campanha não está implementado.

**CONFIRMADO por emulação:** ProcessInputGame 0x45ec5c, com ordem 13, chama MenuInit 0x46814f com menu 16 somente para o jogador local. Foram comparadas 16 combinações de quatro jogadores locais e quatro emissores. MenuInit é interceptado; a função de despacho nativa executa e retorna normalmente.

Ferramenta: tools/trace_escape_menu.py. Resultado: reports/escape_menu_original_validation.json. A execução usa a cópia em memória, sem iniciar processo legado ou APIs do Windows.

**MODERNO/PROVISÓRIO:** aparência, opções, distribuição visual, navegação das páginas, suspensão da simulação e confirmação de saída foram implementadas para o protótipo. O teste confirma que Escape abre o menu original 16, não que esse menu tenha exatamente estas opções ou layout. As páginas nativas ainda precisam ser extraídas. Não é uma reprodução gráfica do menu original.

## Saves

Salvar e carregar reutilizam o formato moderno v7 e a pasta PrototypeSaves desta release, com busca de versões v7/v6/v5/v4 da mesma missão. Só há um arquivo rápido por mapa/treino. Grupos e alertas continuam sendo da sessão e são reiniciados ao carregar. Colocação de construção e indicador de destino também são limpos após carregamento bem-sucedido, para não usar referências à partida anterior.

Erros esperados de leitura, gravação ou validação aparecem no menu. Carregamento inválido passa pelas validações da simulação antes de substituir o mundo. Gravação usa o mecanismo existente de arquivo temporário e substituição. Não há importação de saves legados nesta tela.

## Verificação

229 verificações aprovadas em logs/selftest_menu_v13.log. Os novos casos cobrem escolha inicial, navegação circular, retorno de saída/controles e fechamento do menu principal sem sair do jogo. Testes existentes continuam cobrindo salva/carrega e rejeição de saves inválidos.

As capturas reports/prototype_menu_v13_main.png, prototype_menu_v13_controls.png e prototype_menu_v13_exit.png foram inspecionadas em 1280×800. A interface e a legibilidade foram verificadas; ainda não foi feita automação completa de todos os cliques físicos, falhas de disco ou tamanhos de janela. Capturas são produzidas por --capture com --capture-menu main/controls/exit.

O próximo trabalho deve extrair a composição de MenuInit(16) e menus de construção, sem atribuir a este menu moderno uma fidelidade ainda não demonstrada.
