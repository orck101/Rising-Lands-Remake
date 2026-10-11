# Seleção da vítima no impacto — 27/09/2026

## Resultado

Foram aprovados **6.030 casos nativos**: 5.148 de ocupação de células, 441 de seleção de unidade e 441 de encaminhamento do impacto. A rotina original procura uma construção antes de procurar unidades quando o sinalizador de destino é zero, como nas flechas tipo 9 estudadas na v7.

O protótipo v7 ainda exige que a unidade originalmente selecionada esteja na célula atingida. A rotina original aqui reconstruída recebe uma célula e pode selecionar outro ocupante. Essa diferença está **CONFIRMADA na seleção**, mas ainda não comprova o dano final a aliados, construções ou unidades substitutas: o experimento interrompe a execução antes do tratamento de dano.

Não foi publicada uma nova versão jogável nesta etapa. O código de seleção usa campos que a simulação moderna ainda não atualiza fielmente, e a rotina posterior contém decisões de dano, estado e diplomacia. Aplicar somente parte dessas regras ao combate produziria uma nova aproximação.

## Fonte e método reproduzível

Fonte: `working/RISING/RISING-ORIG.EXE`, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`.

Execute `tools/trace_impact_selection.py` com o ambiente Python de pesquisa. O script carrega a cópia do EXE no Unicorn e executa as rotinas com registros sintéticos controlados de 276 bytes. Cada chamada possui limite de um milhão de instruções. Não inicia o executável original, não chama APIs do Windows e não grava o corpus.

Saídas:

- `reverse_engineering/impact_selection_vectors.json`: entradas e resultados de todos os casos, hash da fonte e limites do ensaio.
- `reports/impact_selection_original_validation.json`: contagens, endereços e resultado.
- `reports/impact_selection_research_integrity.json`: hashes dos novos artefatos e integridade das cópias preservadas.

## Regras confirmadas nas entradas testadas

### Ocupação: EnginOccupeCase, 0x442512

EAX recebe índice da unidade; EDX, célula consultada; BL, modo. Retorno AL é booleano. Campos de memória: tipo +1, estado +2, célula +8, segunda célula +124, byte +171.

- Modo 1 exclui registros cujo byte +171 seja diferente de zero; modo 2 exclui os de byte +171 zero. O modo 0 admite ambos. Região 0x442543–0x442567.
- Estado 14 é excluído. Região 0x442568–0x442575.
- Tipos 3, 11, 12, 13, 14, 15, 16 e 25 ocupam as duas células declaradas e, para cada uma, os deslocamentos −1, −256 e −257. Região 0x4425c6–0x442615. A rotina compara índices lineares, sem corrigir bordas nessa região.
- Demais tipos no intervalo 0–25 usam igualdade com célula +8 ou +124. Região 0x4425a1–0x4425af, com seleção por tipo nos ramos anteriores.

O ensaio cobre os 26 tipos, estados 7/14, byte +171 em 0/1/50, modos 0/1/2 e 11 posições ao redor de duas células distintas. A referência foi corrigida durante a verificação: os tipos 4 e 5 usam célula simples, ao contrário do tipo 3. O conjunto final passou integralmente.

**DESCONHECIDO:** significado completo do byte +171 e atualização das duas células ao longo do movimento. Os nomes nos vetores preservam os offsets, sem assumir que representem simplesmente altura ou posição atual/futura.

### Busca: ChercheIndiceEnginCible, 0x4465df

EAX recebe célula; EDX, índice a excluir; BL, sinalizador. Retorna índice ou 65535.

- Percorre índices de 0 a 799, em ordem crescente (0x4465f7–0x446602).
- Ignora o índice excluído e estados 0 e 4 (0x446608–0x446632).
- Exige byte +114 diferente de zero (0x446645–0x44664b).
- Consulta a ocupação com modo `1 + (sinalizador != 0)` (0x44664d–0x44665f).
- Para registros com byte +171 zero, continua a busca e conserva o **último** índice que combina. Para byte +171 não zero, retorna o **primeiro** que combina (0x446668–0x446680).
- O dono da unidade não é consultado nessa busca. Isso **não significa** que aliados necessariamente recebam dano; decisões posteriores não foram executadas.

Casos incluem busca vazia, ocupante deslocado, índice 799, dois ocupantes, sinalizadores 0/1/2, exclusões 1/3/65535, estados 0–19, byte +114 zero/não zero e donos trocados.

### Encaminhamento: InfligeDegat, 0x450874

- Sinalizador de destino zero consulta `ChercheIndiceBatiment` 0x41b5ea antes da busca de unidades (0x450885–0x450899).
- A consulta de construção lê a palavra da célula na tabela apontada por 0x4e0f38. Bit 0x8000 indica construção; índice é `(palavra & 0x7fe0) >> 5` (0x41b5f0–0x41b610).
- Se houver construção, segue para o ramo de construções. A execução do ensaio para em 0x4508af, **antes de calcular dano**.
- Sem construção, chama a busca de unidades com exclusão 65535 e o sinalizador original (0x450a9d–0x450aab). Se encontrou, o ensaio para em 0x450abf, **antes de processar a vítima**; sem resultado, para na saída 0x4510d5.
- Sinalizador não zero ignora a consulta de construção e passa à busca de unidades.

Essas combinações foram testadas com construção ausente, índice 0 e índice 17, cruzadas com todos os cenários e sinalizadores da busca. A leitura da tabela e a busca de ocupantes executam o código original, sem substituir essas funções por respostas prontas.

## Integração pendente

Antes de substituir o impacto v7, é necessário:

1. Reconstruir a continuação de `InfligeDegat` após a seleção: filtros de estado, jogador, diplomacia, cálculo de dano por vítima e efeitos de derrota.
2. Identificar quando o original atualiza os campos +8/+124, +114 e +171, incluindo transições de movimento e unidades recém-formadas.
3. Definir migração de saves, pois a v7 guarda dano calculado para o alvo inicial e não conserva todos esses campos.
4. Comparar dano e estado final em cenários com troca de ocupante, aliado no destino, construção, célula vazia e atacante derrotado.

Fidelidade da seleção isolada está confirmada no domínio testado; fidelidade do impacto completo permanece **PARCIAL**.
