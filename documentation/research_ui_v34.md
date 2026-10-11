# Pesquisa — v34

Executar releases/research-ui-v34/Jogar-Primeira-Missao.cmd. Abrir pesquisa com T; retorno por seta original, T, Esc ou botão direito. Clicar nas colunas mostra seu nível. O botão de confirmação inicia/pausa a engenharia existente, quando permitida pela missão e pelo laboratório.

CONFIRMADO: fundo derivado de PAGERES.PIE. Getter nativo RechercheGetNiveau 0x4716b2 executado isoladamente em Unicorn: 32 casos de oito jogadores/quatro ramos; tabela 0x5080e4, stride16. Evidência em reports/research_levels_v34.json e ferramenta tools/trace_research_levels_v34.py.

FORTE EVIDÊNCIA: MenuInit 0x468576..0x468596 troca ramos1/2, resultando agricultura/engenharia/armamento/espiritualidade. Barras em x442 +103 por coluna, y425 -15 por nível, limite20, conforme leitura 0x4684a8..0x4685da. Fonte original não decodificada; cores aproximadas e fonte moderna permanecem.

ADAPTAÇÕES: controles de envio/retorno de colonos, laboratório e confirmação conectados à lógica existente, sem confirmação completa das ações nativas. Seleção de coluna usa área ampla adaptada. Outros ramos exibem níveis iniciais, mas não permitem pesquisa. Não se declara jogabilidade/pesquisa integralmente reconstruída. Lógica de engenharia continua limitada a dois efeitos suportados por missão; estoques individuais, pesquisa de outros ramos, objetivos e vitória originais pendentes.

Validação: build e publicação; suites missão1/regressão aprovadas. Capturas v34-research.png e v34-research-active-m3.png inspecionadas. Capturas finais v34-verified-research.png e v34-verified-research-active.png executam verificações do handler real: seleção agricultura/engenharia, confirmação de ramo indisponível sem mutação, botão voltar e serialização idêntica após navegar. Sem teste físico manual. Uma captura research-active inicialmente usada na missão1 falhou porque laboratório é bloqueado; repetida na missão3 com sucesso, sem alterar as restrições reais.

Arquivos em cópias alterados: Engine/ResearchMenu.cs, Engine/ResearchGame.cs, csproj, Content/original-research.png; ferramenta/evidência nova. Originais e saves originais preservados.
