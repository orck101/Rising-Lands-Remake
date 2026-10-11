# Condições da primeira missão — 28/09/2026

## Descobertas verificadas

**CONFIRMADO no avaliador original, com consultas de prédios controladas:** a primeira missão exige simultaneamente um prédio próprio de tipo 3 em zona 0 e dois estoques maiores ou iguais a 50. O tipo 3 está identificado como CAVE / Storehouse na tabela original de construções. O primeiro estoque está em `0x4e2e9e`, o segundo em `0x4e2ea6`, para jogador 0. Correspondem aos IDs de recurso 1 e 2, respectivamente, na estrutura de estoque. A associação a alimento e pedra é consistente com a documentação de recursos e o resumo textual da missão.

Fonte: `working/RISING/CD/WORLD/MONDE01.ETA`. A seção de condições começa no offset decimal **340552** e contém dez registros de dez bytes. Registros relevantes:

| Índice | Offset | Bytes | Interpretação verificada |
|---|---:|---|---|
| 0 | 340552 | 040003000000ffffffff | Tipo de condição 4; construção 3; zona 0 |
| 1 | 340562 | 00003200ffffffffffff | Condição 0; primeiro estoque ≥ 50 |
| 2 | 340572 | 01003200ffffffffffff | Condição 1; segundo estoque ≥ 50 |
| 9 | 340642 | 01003200ffffffffffff | Repete o segundo estoque ≥ 50 |

Os registros 3–8 começam em FF. O registro duplicado foi preservado; não há evidência de que seja necessário eliminá-lo.

**CONFIRMADO separadamente na rotina de região:** a zona 0 da primeira missão inclui os limites `0 ≤ x ≤ 28` e `0 ≤ y ≤ 25`, em células, com índice `y*256+x`. O retângulo vem do offset **340538**, bytes `000000001c0019000000`. Foram comparadas 36 combinações de pontos internos, bordas e externos; fora dessa região a função retorna 255. Isso é região lógica do cenário, não posição da câmera.

## Método e testes

`tools/trace_mission_one.py` executa LoadFinMission (`0x458ad1`) com os 104 bytes da seção e cabeçalho fornecidos por FileRead emulado. Compara os cem bytes carregados em `0x4e1d20` aos do arquivo. Depois executa TestFinDeMission (`0x4585d2`) e TestRessource (`0x467579`) nativos.

75 casos cobrem proprietários 0/1, tipos 3/1, zonas 0/1, estoques 49/50/51, ausência de prédio e contagem global de unidades igual a zero. Consultas de existência, proprietário, tipo, célula e zona de prédio retornam dados de uma única construção sintética. Não se emula o jogo inteiro.

As 36 verificações de região executam InfluenceReset (`0x47bbbc`) e CaseGetZoneInfluence (`0x47bbf6`) sobre os bytes copiados do retângulo. O carregador nativo de retângulos não foi executado. Total: **111 casos nativos**, além da verificação do carregamento das condições.

Resultados: `reports/mission_one_original_validation.json` e `reverse_engineering/mission_one_vectors.json`. Catálogo adicional: `extracted/mission_end_catalog.json`, com registros brutos, hashes e offsets das condições dos **34 mapas**. Os significados das condições das demais missões ainda não foram validados.

Executável de referência: SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. Nenhum processo legado, API do Windows ou arquivo original foi modificado.

## Limites para integração

Se a contagem em `0x4e1a3a` é zero, a rotina retorna encerramento com resultado de derrota, inclusive quando os requisitos de construção e recursos estão satisfeitos. **CONFIRMADO no consumidor desse campo**; sua produção ainda não foi emulada. Leitura estática de CalculEnginNextEtape (`0x456db5`, incremento em `0x456e84`) mostra contagem de unidades do jogador local e exceção para tipo 17. É incorreto substituir isso sem análise por uma contagem genérica de todos os atores visíveis ou vivos.

Os testes reiniciam flags/resultados entre vetores: não validam persistência de vitória, avaliação durante vários quadros, coexistência de várias construções, estado de obra, save/load da campanha ou transição para a missão seguinte. A implementação de região e recursos do protótipo ainda deve ser comparada a essas estruturas antes da integração.

**Versão jogável permanece objectives-v15.** Esta etapa entrega evidência e ferramentas, sem adicionar uma vitória automática ainda incompleta. Próximo trabalho: validar a contagem de unidades, existência de prédios durante construção e fluxo de encerramento, depois integrar a primeira missão.
