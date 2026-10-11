# Menu de pausa — v33

Entrega: releases/native-menu-v33/Jogar-Primeira-Missao.cmd.

O menu de pausa, confirmação de reinício, saída e controles usam o fundo extraído de MENUPRIN.PIE. A tela mantém proporção 800x600 com margens pretas quando necessário. Opções de texto centralizadas substituem os botões retangulares. Clique, setas, Enter e Esc continuam funcionando; saves modernos permanecem v15.

CONFIRMADO: arte do arquivo original convertido. FORTE EVIDÊNCIA: disposição vertical de título/opções, baseada na leitura estática de MenuDisplay (0x46927b): título y246, primeira opção y286, avanço25. Não foi executado um novo rastreamento dessa rotina nesta etapa.

ADAPTAÇÕES: fonte moderna, tradução e opções do protótipo. Não se declara reprodução completa do menu original. Pesquisa, objetivos e vitória continuam pendentes; a pesquisa precisa de reconstrução dos controles e confirmação das quatro ramificações, antes de substituir sua lógica.

Validação: publicação aprovada, testes focados da missão1 e regressão aprovados. Captura v33-pause.png revisada visualmente. Construção conjunta e gesto Ctrl continuam cobertos pelos testes existentes. Não houve teste manual de interação nesta entrega.

Originais e saves originais não foram modificados. Alterações em cópias: Engine/PauseMenu.cs, Engine/ResearchGame.cs, csproj; nova cópia Content/original-menu.png. Evidência: reports/native_menu_v33_validation.json.
