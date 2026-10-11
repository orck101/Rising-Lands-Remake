# v60 — descoberta do mapa e filmes originais de pesquisa

## Entrega

Executável: `releases/interactions-v60/RisingLandsRemake.exe`. Iniciadores da primeira, segunda e terceira missão na mesma pasta. F9 carrega os cinco slots compartilhados. A versão anterior continua disponível.

1. Ao concluir uma descoberta, reproduz uma vez o filme original associado em EVENTS.INI, inclusive na terceira pesquisa opcional. A partida fica suspensa durante o filme; clique, Enter ou Esc encerra a exibição. O filme também termina automaticamente. A velocidade 1x–5x não acelera o filme. Carregar um save não repete descobertas anteriores.
2. Mapa e minimapa distinguem preto (nunca descoberto), terreno escurecido (descoberto sem visão atual) e visão normal. Unidades inimigas ficam ocultas fora da visão atual, inclusive na seleção e nos indicadores do minimapa. A memória dos recursos conserva a última quantidade observada, em vez de exibir alterações ocultas. Caminhar continua permitido no mapa desconhecido; fontes/destinos de coleta e locais de construção precisam de visão para comandos pela interface.
3. Botão **Mostrar mapa / Esconder mapa**, acima da velocidade, alterna uma revelação exclusivamente visual para testes. Não escreve exploração, não muda recursos ou ordens e não é gravado como descoberta. Ao carregar ou trocar missão, retorna à visão normal. Tab mostra/esconde o minimapa.
4. Save v24 acrescenta exploração e memória do cenário. Preserva leitura dos saves v4–v23. Arquivos antigos sem histórico de exploração começam com a região atualmente vista pelas unidades/construções próprias; não é possível recuperar um histórico que nunca foi gravado. Saves originais não são alterados.

## Evidências do original

### CONFIRMADO — duas camadas de visibilidade

`working/RISING/RISING-ORIG.EXE`, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`:

- `CaseEstDecouverte`, VA **0x476602**, lê a camada de exploração em `[0x507d64 + jogador*4]`. Bit central 0x10 limpo significa descoberto.
- `CaseEstIlluminee`, VA **0x476866**, lê visão atual em `[0x507d74 + jogador*4]`, com a mesma convenção de bit. Os bits 1/2/4/8 codificam bordas vizinhas; não representam quatro jogadores.
- `DecouvreTerrain`, VA **0x47677d**, percorre um disco de raio recebido + 1, com limite quadrático r² + 1 quando r >= 2.
- `IllumineTerrain`, VA **0x476bdf**, usa diretamente o raio recebido, sem esse incremento. O limite quadrático adicional segue a mesma regra.
- Chamadas de tropas em **0x441d23–0x441d64** usam raio + modificador + 2 para descoberta e raio + modificador para visão. Construções em **0x41ce39–0x41ce6a** acrescentam 3 apenas à descoberta.
- `RISING.INI`: o comentário de RAYON identifica expressamente o raio de visibilidade. Os valores, linhas e offsets de cada classe são exportados em `Data/visibility_native.json` a partir de `Data/units.json`. Fazendeiro, construtor, engenheiro e colono possuem raio base 5.
- `BatimentGetRayon`, VA **0x41c78c**, fornece os raios por tipo de prédio. Foram obtidos executando somente essa rotina em emulação isolada.

Método: 48 testes de rotinas x86, incluindo centro e cantos do mapa e raios 0/1/4/5/7/8/10/20. As chamadas de escrita de células foram interceptadas, sem executar o programa original ou APIs de Windows. Todas as listas de células foram comparadas com a fórmula implementada. Resultado e células completas: `reverse_engineering/visibility_v60.json`. O teste C# usa a lista nativa como referência independente para a visão do fazendeiro.

### CONFIRMADO — filme por descoberta

- `RechercheCalcul`, **0x4711f9**, conclui a descoberta e chama `InventionSetEffet`.
- `InventionSetEffet`, **0x470eeb–0x470f16**, para uma descoberta do jogador local, chama `MessageBoxSet(10, ramo*100 + índice)`.
- `MessageBoxSet`, **0x46991a**, consulta a seção EVENEMENT do arquivo EVENTS.INI usando esse código.
- `MessageBoxRun`, **0x469a43**, para mensagem tipo 10, chama **0x4580d4**. Essa rotina monta `Events\<nome>.FLC` (**0x45810b / 0x458156**) e chama `OpenFli`, **0x45a5b0**.
- Exemplo: `AGRICULTURE2` → evento 002 → `CD/EVENTS/LABO3.FLC`, 320×240, 121 frames, duração de frame 71 ms. `MILITAIRE2` usa evento 102, também LABO3.

Foram exportados **26 filmes / 2.812 frames**, sem redesenhar a arte. Há hashes dos arquivos de origem e de cada frame RGB. O conversor é `tools/export_discovery_v60.py`; metadados em `Data/research_completion.json` e evidências em `reverse_engineering/research_completion_v60.json`.

Particularidade dos FLC: chunks COPY declaram dois bytes a menos que seu conteúdo. A rotina nativa `ReadFli`, **0x45a952–0x45a9a8**, copia largura × altura bytes ignorando esse tamanho. O conversor corrige apenas uma cópia em memória dos comprimentos para o FFmpeg aceitar os dados; o original permanece intacto. Todos os filmes convertidos terminaram sem erros do decodificador. O frame adicional usado para retorno de loop não é reproduzido na exibição única.

## Adaptações e limites explícitos

- A reprodução é um painel central com texto em português e possibilidade de pular. Usa os frames, proporções e tempos dos arquivos originais, mas a disposição do painel é do remake.
- Os raios base, os dois estados persistente/temporário e os discos foram confirmados. Modificadores de visão, magias e visão compartilhada por alianças ainda não foram reconstruídos.
- O escurecimento usa sobreposição de células. O acabamento das bordas direcionais nativas e seus tons exatos continuam pendentes; não se afirma equivalência pixel a pixel.
- A memória de recursos está implementada. A memória de alterações futuras de edifícios inimigos ainda precisará acompanhar destruição/construção adversária quando esses sistemas forem ampliados; o cenário inicial estático é preservado sob a névoa.
- O evento 220 pede HELICOP, enquanto o corpus contém HELICO.FLC; não foi inventada uma associação para essa divergência. Descobertas sem associação existente não recebem filme fabricado.
- A primeira missão passa a ter descoberta ativa por padrão no remake. Não foi comprovada uma exceção de tutorial no original.

## Verificação

1.610 verificações aprovadas: missão 1 (782), missão 2 (83), suíte geral (351), regressão v57 (295), pesquisa v58 (53) e descoberta v60 (46). Cinco saves reais também foram carregados em inspeção somente leitura, com integridade dos arquivos conferida (10 verificações). Ver `reports/v60-validation.json` e `logs/v60-*.log`.

Capturas reais conferidas: descoberta inicial, terreno explorado fora de visão, revelação de teste, missões 2 e 3 e filme LABO3. A captura de revelação verifica os cliques reais do botão e igualdade integral do estado antes/depois. A captura do filme verifica enfileiramento único, suspensão da simulação e encerramento por Enter.

## Teste manual sugerido

Leve uma unidade para longe da base e depois retorne: a região visitada deve continuar escura, enquanto a parte nunca descoberta continua preta. Ligue e desligue Mostrar mapa: áreas reveladas só para teste devem voltar ao estado anterior. Salve/carregue para conferir o histórico. No laboratório, conclua uma pesquisa; confira o filme, pule com Enter e verifique que a próxima pesquisa continua normalmente.

Os originais em D:\RISING não foram escritos. Novos arquivos, conversões, scripts, código e relatórios permanecem na área separada RisingLandsResearch.
