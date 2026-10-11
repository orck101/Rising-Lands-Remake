# Tecnologias em save1.sav

Arquivo analisado: `D:\RISING\save1.sav`. Rótulo: **Mission 5 (5)**. Tamanho: 2032386 bytes.
SHA-256: `c442f2083a32e036b42a87d96842fd23e4449a120ab46634fb942226cf3d2d9c`.

**CONFIRMADO:** o bloco de pesquisa começa no offset de arquivo `0x19451b` (1656091 decimal). Os offsets são contados desde zero. Não são endereços de memória. O offset depende da quantidade de unidades e construções gravadas: não reutilizar esse número cegamente em outro save.

## Níveis atuais do jogador 0

| Ramo | Offset no SAV | Byte hexadecimal | Nível interno |
|---|---|---|---|
| Agricultura | `0x19451b` | `0E` | 14 |
| Militar | `0x19451c` | `14` | 20 |
| Engenharia civil | `0x19451d` | `0D` | 13 |
| Religiao/magia | `0x19451e` | `08` | 8 |

Os quatro bytes são `0E 14 0D 08`. O byte seguinte, `0x19451f`, vale `02`: ramo selecionado **engenharia civil**. Os oito bytes a partir de `0x194520` são um double little-endian de progresso, aqui **0.0**. O contador de descobertas do jogador 0 é **2**, no byte `0x194528`. A tabela de quatro contadores é repetida em cada registro serializado; não é um inteiro único de descobertas.

O nível é um índice interno cumulativo. A rotina de conclusão aplica o efeito do índice atual e depois incrementa o byte. Portanto nível civil 13 indica que a posição seguinte é GENIECIVIL13; não prova que o jogador pesquisou manualmente treze melhorias nesta missão. Inicialização de campanha, entradas puladas e efeitos já aplicados também precisam ser considerados.

## Todos os jogadores

| Jogador | Início do registro | Agricultura | Militar | Engenharia | Religião/magia | Descobertas registradas |
|---|---|---|---|---|---|---|
| 0 | `0x19451b` | 14 | 20 | 13 | 8 | 2 |
| 1 | `0x19452c` | 11 | 20 | 22 | 4 | 0 |
| 2 | `0x19453d` | 17 | 13 | 22 | 14 | 2 |
| 3 | `0x19454e` | 19 | 19 | 18 | 11 | 2 |

## Tabela de referência

Começa em `0x19455f` e ocupa 64 bytes (16 por jogador). Para o jogador 0, contém **14, 20, 11, 8**. Comparada à tabela atual, apenas engenharia mudou: **11 → 13**. O contador atual registra duas descobertas. **FORTE EVIDÊNCIA:** as duas posições avançadas são GENIECIVIL11 e GENIECIVIL12. O instante exato em que a referência foi capturada exige rastrear o contexto completo de RechercheStore; esta análise não o apresenta como comprovadamente o início da missão.

## Como foi confirmado

1. Percurso de serialização em GameSave (`0x46bbd9`) e GameLoad (`0x46b721`): cabeçalho, influência, jogadores, disponibilidade, feitiços, unidades, dados auxiliares, construções, projéteis, terreno, pesquisas.
2. Contagens lidas do próprio save: 223 unidades e 39 construções, cada registro com 276 bytes e índice de dois bytes. O mapa ocupa 17×65536 + 3×16384 + 80 bytes.
3. RechercheLoad (`0x4708e9`) lê quatro bytes de nível, um de ramo, oito de progresso e quatro contadores por jogador. O registro no arquivo tem 17 bytes; em memória tem 16, pois o contador é global e o alinhamento é diferente.
4. A rotina foi emulada usando os bytes reais do save; apenas FileRead/FileWrite foram substituídos por operações em memória. RechercheGetNiveau (`0x4716b2`), RechercheGetBranche (`0x4716a1`) e RechercheGetNbInvention (`0x470444`) retornaram os valores acima: **24 consultas verificadas**.
5. RechercheSave (`0x470999`) reserializou pesquisa e bônus: **2180 bytes idênticos** ao arquivo, sem gravação no original.
6. RechercheEndTechnologie (`0x47138d`, incremento em `0x4713ca`) confirma o uso do nível como índice que avança após aplicar um efeito. RechercheStore (`0x4713e0`) copia a tabela atual para a referência.

Endereços de memória: níveis atuais `0x5080e4 + 16*jogador + ramo`; referência `0x508124 + 16*jogador`; contadores `0x50870c + jogador`.

## Preservação e reprodução

O arquivo original foi apenas lido e seu conteúdo foi conferido novamente ao final. A cópia de evidência é `C:\Users\Victor Freitas\Documents\Codex\2026-09-25\quero-que-acesse-uma-pasta-do\outputs\RisingLandsResearch\extracted\saves\save1-c442f2083a32.sav`.

Ferramenta: `tools/analyze_save_research.py`. Relatório estruturado: `reports/save1_research_analysis.json`, incluindo cada chamada nativa de leitura e as seções que antecedem a pesquisa. Nenhuma alteração foi feita no remake ou no save. Não foi executado o processo Windows do jogo original.
