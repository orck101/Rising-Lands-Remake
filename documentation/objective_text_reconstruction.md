> Correção em28/09: os mapas9,10,12,17 existem como variantes com letras; todos os34mapas têm texto correspondente. Integração corrigida na v16. Ver campaign_text_v16.md.

# Leitura e paginação dos objetivos — 27/09/2026

**CONFIRMADO em emulação:** ObjectifLoadText (`0x47c147`) constrói o nome `\World\mondeNN.TXT`, lê até 16.000 bytes, separa linhas por CR/LF e prepara uma tabela de ponteiros. ObjectifGetLigneText (`0x47c349`) usa o índice da página atual; ObjectifTextScroll (`0x47c45e`) avança ou recua em blocos de 18 linhas, com limites.

Foram executadas as três rotinas nativas para todos os 25 arquivos MONDE*.TXT disponíveis. Resultado: **39 páginas**, com verificação da ida, volta, limites e correspondência das linhas aos bytes de origem. Os offsets de cada linha, hashes e páginas estão em `reverse_engineering/objective_text_pages.json`. Resumo em `reports/objective_text_original_validation.json`; ferramenta reproduzível em `tools/trace_objective_text.py`.

Arquivo executável: cópia `working/RISING/RISING-ORIG.EXE`, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. FileOpen, FileRead e FileClose são substituídos por fornecimento dos bytes em memória. O restante dessas três rotinas é executado sob limite de instruções. Não se inicia o processo legado nem se escreve nos originais ou cópias preservadas.

**CONFIRMADO por leitura estática:** MenuInit nos caminhos 17 e 23 (`0x468356–0x46837f`) chama MissionGetNum e passa o número para ObjectifLoadText. AffObjectif (`0x47c363`) desenha até 18 linhas, com espaçamento de 15 pixels; esta função gráfica não foi emulada nesta etapa.

O texto da primeira missão começa no prólogo do diário. A rotina de leitura não procura um marcador de objetivo dentro do arquivo: carrega seu conteúdo textual desde o início. Assim, o diário encontrado anteriormente é de fato consumido por essa tela; isso não significa que todo texto narrativo seja uma condição de vitória.

**DESCONHECIDO nesta etapa:** ligação completa do clique em MISSION OBJECTIVE aos caminhos do menu, fallback de arquivo ausente, regras de vitória/derrota, desbloqueios e atualização de objetivos durante eventos. A fonte principal foi simulada como disponível em todos os casos. Codificação CP1252 é a interpretação textual adotada; as linhas de origem permanecem rastreáveis por offsets.

Entrega jogável permanece **menu-v14**. Nenhuma alteração do jogo foi publicada nesta etapa; as páginas recuperadas ainda precisam ser integradas à interface. Próximo passo: confirmar o despacho do menu e adicionar um leitor paginado, distinguindo textos da missão de regras de campanha ainda não implementadas.
