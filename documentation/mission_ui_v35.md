# Interface da missão — v35

Executar releases/mission-ui-v35/Jogar-Primeira-Missao.cmd. Esc → Objetivo da missão. Setas desenhadas no painel, PgUp/PgDn ou esquerda/direita mudam páginas; confirmação e seta superior retornam ao menu.

CONFIRMADO: arte PAGEMIS.PIE, texto extraído do original em objective_pages.json. Rotinas AffObjectif 0x47c363 e ObjectifTextScroll 0x47c45e executadas isoladamente em Unicorn: 18 linhas, y212+15*i; recorte55,212..310,486; avanço e retorno18 linhas. Largura de fonte controlada200, desenhos interceptados. Evidência reports/objective_display_v35.json; ferramenta tools/trace_objective_display_v35.py. Cor e fonte moderna adaptadas; não se declara emulação completa do menu.

Painel mostra páginas na oval superior e tecnologias atuais nas colunas, seguindo ordem original já documentada na v34. Áreas de clique foram alinhadas aos desenhos por inspeção da arte; correspondência exata das hitboxes originais ainda não confirmada.

Conclusão usa MENUPRIN.PIE e disposição centralizada. É uma adaptação com arte original; ainda não foi confirmado que esta era a tela específica da vitória original. ECRANFIN.LZS não foi adotado por não haver evidência suficiente de sua aplicação ao final da primeira missão. Texto solicitado pelo usuário e opção Próxima missão mantidos. Botão e Enter continuam intencionalmente sem efeito, jogo permanece concluído.

Validação: build/publicação, suites missão1/regressão aprovadas; três capturas visualmente revisadas (objetivo primeiro/último, vitória). Capturas finais v35-verified-* executam handlers reais: anterior/próxima, limites de paginação, confirmação, retorno, navegação sem mutação do estado, próximo mapa sem mutação após vitória. Sem teste físico manual. Coleta e construção conjunta continuam cobertas pelas suites anteriores.

Arquivos em cópias: Engine/ObjectiveMenu.cs, Engine/MissionCompleteMenu.cs, Engine/PauseMenu.cs, Engine/ResearchGame.cs, csproj, Content/original-objectives.png. Originais e saves originais preservados. Pendências: fonte original, tela específica da vitória, pesquisa demais ramos e demais interações de gameplay.
