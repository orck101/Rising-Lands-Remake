# Economia, construção e formação — versão 4

Data: 26/09/2026. Esta etapa amplia o protótipo; não conclui o PDF nem constitui remaster completo.

## O que funciona

- Armazém (tipo 3), quartel (1) e farol (0): escolha de local, checagem de área e custo, deslocamento do construtor, progressão, conclusão, imagem e piso originais, bloqueio de caminhos e save/load. O armazém concluído habilita armazenamento; recrutamento militar, guarnição e efeitos de visão de outros prédios ainda não implementados.
- Agricultor (5) e construtor (7) podem ser formados no santuário inicial. Consomem alimento e um colono alojado; não criam população do nada. Um trabalho por santuário; custo cobrado uma vez. Saída bloqueada mantém a formação pronta aguardando espaço.
- Seleção por arraste, Shift para acrescentar unidades, movimento em grupo com destinos distintos e ordem de parar. Inimigos não recebem ordens do jogador. A formação de movimento é implementação moderna provisória, não o algoritmo original comprovado.
- Construção pausada pode ser retomada sem novo pagamento. Cargas, gastos, canteiros, formação e alterações de terreno de alimento são salvos em JSON versão 4. Versões 2/3 e seus saves permanecem separados.

## Como usar

Abra `releases/economy-v4/RisingLandsRemake.exe` mantendo a estrutura de pastas da pesquisa.

B entra na escolha de construção e seleciona um construtor. 1 escolhe armazém, 2 quartel, 3 farol. Clique em área livre; verde indica local aceito. Botão direito ou Escape cancela a escolha antes do pagamento. R retoma uma obra pausada com construtor selecionado. X para as unidades selecionadas sem devolver custos já pagos e sem apagar carga/canteiro.

C seleciona um agricultor e inicia busca de alimento; também é possível dar botão direito na célula de alimento com agricultor selecionado. G inicia formação de agricultor (4 alimentos); H inicia construtor (6). Arraste para selecionar um grupo; Shift acrescenta unidades. Botão direito desloca. F5 salva; F9 carrega a versão 4. Espaço pausa; controles de câmera anteriores permanecem.

## Evidências de construção

Origem de código: `working/RISING/RISING-ORIG.EXE`, SHA256 af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e. Valores configurados: `working/RISING/RISING.INI`; exportação rastreável em `source_remake/Data/buildings.json`.

**CONFIRMADO:** tabela de tipos em 0x495d8c, stride 52; largura/altura +0/+2, limite de progresso +4. Armazém 5×5, 100 pontos, 10 alimentos, 6 rochas; quartel 5×6, 400 pontos, 16 alimentos, 14 rochas; farol 3×4, 200 pontos, 12 alimentos, 4 rochas. Os custos configurados prevalecem sobre defaults estáticos; o quartel é um exemplo de diferença.

**CONFIRMADO sob fixture controlada:** BatimentConstructible 0x41fbb2 percorre a área e testa colisão bit 1, com exceção para célula ocupada pelo próprio construtor. 26 testes: área livre e cada uma das 25 células bloqueada. O protótipo é mais restritivo: exige área sem objetos/unidades e construtor fora dela; não implementa restrições por pesquisa, evento de missão nem todas as exceções de terreno.

**CONFIRMADO:** BatimentConstruction 0x41cff9 soma 2 ao progresso +10, limita ao valor da tabela e define estado +5 = 5 ao concluir. 101 valores anteriores comparados com x86 emulado. Atualização do mapa ao concluir foi interceptada no teste; frequência de chamada no motor integral ainda não determinada. O protótipo aplica uma chamada por tick quando o construtor chega ao ponto de trabalho; isso não comprova duração em segundos fiel.

**FORTE EVIDÊNCIA:** `tools/trace_construction.py` reaproveita registros originais dos tipos 0, 1 e 3, reloca a âncora, chama PlaceBatiment 0x41c54c e captura AffBatiments 0x407dd3. Piso e colisão são obtidos da memória resultante. Ocupação do prédio usa retângulo, visibilidade forçada e tempo controlado. Templates em `source_remake/Data/construction_rules.json`. Teste exploratório do estado 4 com progresso 0/50/98 produziu os mesmos comandos isolados de imagem; não se concluiu a aparência completa do canteiro original. Por isso a obra em curso usa marcação e barra de progresso modernas.

**CONFIRMADO:** EnleveMatiere 0x4677c0 reduz tanto saldo quanto reserva inicial, limitando ambos a zero. 45 casos emulados. Isso corrige o cálculo de ocupação do armazém após pagar uma obra ou formação; reserva não deve ficar congelada no valor inicial.

## Evidências de formação

**CONFIRMADO:** bag do prédio começa em payload +12; capacidade +14; códigos/índices de unidades em +18+2i; categorias em +78+i. Categoria 1 aponta para unidade. A primeira missão possui oito colonos vinculados ao santuário; importer segue essas ligações, não apenas proximidade no mapa.

**CONFIRMADO sob fixture controlada:** InitRecruteTroupe 0x42847a aceita os tipos 5 e 7 no santuário do exemplo, cobra respectivamente 4/6 alimentos, ativa +116 e inicia contador u32 +140 em 1. Valores do INI carregados nas tabelas emuladas; `EnginGetTimeRecrute` 0x44115b lê duração do tipo. O teste usa plano de colisão vazio para a busca inicial de saída, não uma inicialização integral da missão.

**CONFIRMADO:** ConstruitPerso 0x423c33 compara contador com TMPSRECRUTE × 100. No santuário, incremento = 100 + trunc(colonos / capacidade × 100). Constante double 100 em 0x4943ef; capacidade 16 vem de LOGEMENT no INI. 34 comparações para 0..16 colonos e dois tipos. Com 8 colonos, contador 1 vira 151. Outros tipos de prédio têm exceções não implementadas.

**CONFIRMADO:** TransformeColonsEnTroupe 0x41a0fc remove um colono da bag. **IMPLEMENTAÇÃO MODERNA PARCIAL:** o protótipo reaproveita o slot desse colono como trabalhador, usa propriedades da amostra original do tipo e procura uma saída livre ao redor do santuário. Sequência original completa de alocação/saída, fila, desbloqueios e ocupação dinâmica ainda não reproduzida. Não alegar que o slot é reaproveitado exatamente assim pelo original.

Script `tools/probe_recruitment.py`, catálogo `source_remake/Data/recruitment_rules.json`, relatório `reports/recruitment_original_validation.json`. A primeira tentativa sem planos inicializados falhou por leitura não mapeada na busca de saída; fixture corrigida explicitamente, sem esconder a leitura inválida.

## Validação desta entrega

135 verificações passaram no executável publicado. Além das verificações anteriores: pagamento único, rejeição de sobreposição sem efeito, chegada do construtor, colisão após conclusão, pausa/retomada, três tipos de prédio, comandos de grupo, custo de formação, colono único, trabalhador formado funcional e rejeição atômica de save adulterado.

Teste integrado: recursos 30 alimentos/30 rochas; construir armazém (-10/-6), formar agricultor (-4), coletar a pilha inicial (+14) termina com 30 alimentos/24 rochas, um armazém e um trabalhador adicional. Salvar no meio e retomar produz estado idêntico. Não é somente teste de componentes separados.

Compilação sem avisos; captura real inspecionada em `reports/prototype_economy_v4.png`. Log `logs/selftest_economy_v4_published.log`. Não foi feita bateria manual completa de mouse/teclado nem matriz Windows 10/11.

## O que falta

Combate, IA, campanha/VM de eventos, mineração, consumo, pesquisas/desbloqueios, tropas militares, efeitos funcionais de quartel/farol, áudio integrado, animações de trabalho/carga, colisão dinâmica de unidades e timing integral. Construções e formação expostas aqui são opções do protótipo; permissões específicas de cada missão ainda não são aplicadas. Não marcar seção 68 ou remaster completo como concluídos.

Nenhum arquivo original ou save legado foi escrito. Alterações, executável e saves novos ficam na área de pesquisa.
