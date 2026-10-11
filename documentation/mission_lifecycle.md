# Contagem de unidades, obras e encerramento — 28/09/2026

Esta etapa complementa `mission_one_conditions.md`. A versão jogável continua objectives-v15; nenhuma regra nova de campanha foi ativada nesta etapa.

## Contagem usada para derrota

**CONFIRMADO por execução de CalculEnginNextEtape (`0x456db5`):** o contador local em `0x4e1a3a` inclui unidades cujo proprietário é o jogador local, estado difere de 0 e 4 e tipo difere de 17. Estados 10, 12 e 14 também contam, embora a renderização do protótipo os oculte. O código não consulta diretamente HP para esse incremento; a transição de estado por morte é uma questão separada.

224 vetores cobrem jogadores locais 0/1, proprietários 0/1/4/200, tipos 0/5/17/25 e estados 0/1/4/7/10/12/14. Em cada vetor, o buffer dos 800 registros de unidade é zerado e um registro sintético é inserido. A função completa de contagem executa; EnginNextEtape (`0x45442c`) e IATestUtilite (`0x47f5c5`) são interceptadas, preservando o estado fornecido. Assim, não se validam as transições de morte, produção ou IA.

**Consequência:** contar apenas unidades visíveis no remake produziria derrotas incorretas. Por outro lado, conservar LegacyState de uma unidade morta também não basta para reproduzir o contador nativo. A integração precisa relacionar o estado moderno de morte aos estados nativos.

## Prédios em construção

**CONFIRMADO:** BatimentExiste (`0x41a7d5`) considera existente qualquer registro com byte de estado `+5` diferente de zero. Foram testados os 256 valores possíveis. A função não consulta o progresso de construção `+10`.

Depois, 12 casos executaram o avaliador completo da missão 1 com os getters nativos de existência, proprietário e tipo, usando estados 0/1/4/5 e progresso 0/50/100. Célula e região foram fornecidas por fixtures em zona0, e estoques fixados em50/50. Todos os estados diferentes de zero permitiram satisfazer o objetivo de armazém, independentemente do progresso. Estado4 com progresso0 conta neste caminho. Isso não prova que qualquer sequência de gameplay possa criar todas as combinações sintéticas.

**Consequência:** exigir obra concluída para esse objetivo alteraria o comportamento do avaliador original. A liberação de capacidade de armazenamento é uma regra diferente e não foi modificada por esta descoberta.

## Momento de abrir a tela de resultado

**CONFIRMADO em bloco isolado `0x42d518`:** quando o modo em `0x4e0730` é zero e TestFinDeMission retorna verdadeiro, o contador `0x4e061c` é incrementado. Ao atingir100, há chamada para interromper sons e solicitação de menu19. Com retorno falso, esse bloco não zera nem incrementa o contador; portanto ele não exige100 resultados verdadeiros consecutivos por si só.

16 vetores cobrem modo0/1, resultado falso/verdadeiro e contadores0/98/99/100. O predicado final e a parada de som são interceptados apenas nesses testes do despacho; a execução para antes da inicialização do menu. Os testes usam missão1; exceções da missão6 não foram exercitadas.

**DESCONHECIDO:** duração em segundos dessas100 verificações, resets externos do contador, sequência gráfica/sonora do menu19 e avanço completo da campanha. Não foi implementado um temporizador arbitrário equivalente.

## Evidências e continuidade

Ferramenta reproduzível: `tools/trace_mission_lifecycle.py`. Resultados completos: `reverse_engineering/mission_lifecycle_vectors.json`; resumo: `reports/mission_lifecycle_validation.json`. **508 casos nativos aprovados**, além dos111 da etapa anterior. Fonte: cópia RISING-ORIG.EXE, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`.

Foram criados somente script, vetores e documentação na área de pesquisa. Não houve alteração de executável, save ou asset original. Próxima etapa: reconstruir menu19 e MissionNext, verificar transições de estado das mortes e só então integrar o encerramento da missão1 ao protótipo.
