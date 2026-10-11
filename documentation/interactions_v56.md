# v56 — conclusão da missão 2 exige colonos nas construções

## Comportamento corrigido

A missão 2 exige santuário e quartel próprios, completamente construídos na região inicial x=36–63, y=0–23, cada um com pelo menos um colono vivo efetivamente dentro. Mantém o requisito de 50 alimentos disponíveis e sobreviventes. Iniciar a obra, terminar um prédio vazio ou enviar um colono ainda em trânsito não conclui a missão.

A contagem de confirmação começa somente com todas essas condições satisfeitas. Se um dos prédios ficar vazio antes da vitória ou faltar alimento, a contagem é zerada. A missão 1 conserva sua regra anterior. Vitória → escolha de tecnologia → missão 3 permanece funcionando.

## Causa e evidência

**CONFIRMADO no remake anterior:** Engine/MissionOne.cs, SecondMissionSatisfied, aceitava `sites.Any(...)`, incluindo obras recém-iniciadas, e não consultava ocupantes. A v56 usa somente World.Buildings, onde as novas obras entram após sua conclusão, e exige associação no registro de residentes com unidade própria tipo 9, viva e estado 14 (dentro do prédio).

**REQUISITO DO USUÁRIO:** exigir conclusão e ocupação de cada prédio necessário para a missão 2. Essa alteração substitui expressamente o comportamento adotado na v55. Não é apresentada como resultado adicional da emulação do original. Os vetores e evidências de reverse_engineering/mission_two_v55.json continuam preservados; os testes mantêm a conferência espacial e de recursos com ocupantes válidos.

## Saves

Novos slots usam research-v21.json. Cinco slots continuam disponíveis; v20, v19, v18 e v17 continuam carregáveis. Uma vitória prematura da missão 2 em save anterior é recuperada em memória: remove a vitória e fecha a transição, preservando unidades, obras, recursos e demais dados. O arquivo carregado não é reescrito. Vitórias antigas que já satisfaçam a regra corrigida continuam válidas. Novos saves com vitória incompatível são rejeitados sem alterar a partida atual.

## Validação

- 782 verificações da missão 1/interações, 83 da missão 2, 351 gerais: **1.216 aprovadas**.
- Percurso integrado com recursos reais: construir/habitar armazém, coletar recursos, pagar e concluir santuário/quartel; manter ambos vazios por 150 ticks sem vitória; entrada de um colono em cada prédio; retirar/recolocar um colono durante a confirmação; vitória e avanço.
- Obra recém-iniciada, prédios vazios, residentes sem prédio concluído e colonos visíveis/em trânsito não contam.
- Saves de vitória e transição, recuperação de vitória antecipada v20, preservação de vitória válida v20 e rejeição transacional de vitória falsa v21.
- Quatro ramos de tecnologia abrem a missão 3; salvar/carregar e reiniciar permanecem funcionando.
- Capturas reports/v56-mission2-victory.png e reports/v56-mission2-third.png verificadas visualmente.
- Logs logs/v56-mission-one.log, logs/v56-mission-two.log e logs/v56-self-test.log.

Abrir releases/interactions-v56/Jogar-Segunda-Missao.cmd. Não foram modificados os arquivos originais nem saves originais. IA nativa, final da missão 3 e efeitos de pesquisa anteriormente pendentes permanecem fora desta correção.
