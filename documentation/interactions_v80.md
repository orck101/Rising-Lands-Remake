# v80 — desenvolvimento autônomo do clã inimigo

O usuário escolheu um novo sistema de desenvolvimento da IA, mantendo os quatro ramos tecnológicos. Este planejador é uma política do remake solicitada pelo jogador, não uma tradução integral do código de IA original.

## Problemas demonstrados

Na v79, a missão 4 treinava oito militares até aproximadamente seis minutos e não voltava a produzir até doze minutos. O quartel ficava com um colono e o santuário conservava cinco ociosos. Havia apenas um nascimento no cultivo em doze minutos. Parte dos militares de uma expedição ainda recebia ordens de guarnição por causa da antiga divisão por paridade de slot.

Os ensaios longos também identificaram um limite espacial: aos doze minutos de um cenário intermediário não havia comida alcançável perto do armazém inicial, apesar de 244 fontes alcançáveis em outras regiões descobertas. A busca limitada à região inicial deixava fazendeiros ociosos. Uma busca global sem exploração gradual também foi rejeitada nos testes: enviava trabalhadores em viagens longas cedo demais. Os relatórios preservam estes experimentos.

## Novo planejamento

- Consolidação inicial com objetivo de seis fazendeiros. Depois de cinco minutos, objetivo de oito, aumentando gradualmente até doze. O crescimento econômico cede prioridade à reposição urgente do quartel quando já existe uma base mínima de fazendeiros.
- Distribuição de coletores entre cultivo e estoque; liberação e entrada física de colonos para abastecer os serviços, produção e laboratórios existentes. Colonos reservados por treinamento ou pesquisa não são contados como disponíveis. Mantém-se um operador nas fontes de transferência.
- Armazém adicional em posição original permitida, priorizando proximidade dos campos. Destinos de entrega usam o depósito ativo mais próximo da área de coleta. Existe reserva de alimento para investir nessa logística, sem criar recursos.
- Dois construtores podem trabalhar na mesma obra usando a mecânica normal de ajuda. A expansão posterior continua apoiada nas prioridades e posições nativas já reconstruídas.
- Controle de coleta excessiva de pedra e engrenagens para não ocupar todo o armazenamento às custas da comida.
- Fontes próximas têm prioridade. Quando esgotadas, a IA busca fontes conhecidas no restante do mapa, com verificação de conectividade terrestre. Regiões ainda desconhecidas são exploradas por deslocamentos graduais até áreas próximas alcançáveis. Não são escolhidos recursos ocultos como destinos de coleta.
- Tentativas de ordens são limitadas por atualização. Máscaras de conectividade são compartilhadas entre trabalhadores do mesmo tipo naquela atualização, evitando repetir buscas para cada destino candidato. Estes caches não fazem parte do save.
- Colheita sustentável: preserva uma planta por pequena região de solo fértil para que o crescimento e a propagação originais possam continuar. Os fazendeiros do jogador não recebem esta restrição. A IA não cria abóboras; a geração permanece no sistema de ambiente existente, com seu registro de conservação de recursos.
- Recuperação de fome dos fazendeiros inimigos quando o estoque zera: eles podem colher alimento real e consumir uma unidade transportada para retomar o abastecimento. A saciedade e o consumo são registrados; não existe alimento gratuito ou imunidade à perda de vida.
- Toda tropa integrante de uma expedição pode avançar; a decisão antiga de deixar metade dessas tropas parada foi removida. A guarnição é controlada separadamente pelo estado da expedição.

## Regras preservadas

Abertura de três minutos sem ofensivas; entre três e cinco minutos, grupos de até dois soldados comuns/lança-chamas; crescimento gradual posterior. A percepção defensiva de ranged/melee e os alcances das armas continuam como na v79. Produção respeita edifícios, desbloqueios, custos e colonos reais. Pesquisas continuam dependentes de laboratórios e colonos, respeitando os tempos e a quota existentes.

Não foi criado um quinto ramo de pesquisa. Os arquivos originais e os saves do jogador não foram modificados. O save do remake conserva o formato v33: o planejamento é derivado do tempo, população, edifícios e recursos já persistidos. Os dados de expedição da v79 continuam persistidos.

## Validação

`Tests/GrowthV80Tests.cs` acompanha uma missão real por vinte e quatro minutos, verifica produção após os minutos seis/doze, novos fazendeiros, nascimentos repetidos, continuidade militar após doze minutos, conservação ao salvar/carregar e retomada determinística. A regressão original falha antes da correção. Relatório final: `reports/v80-validation-0.txt`.

Também são executados a suíte geral, a suíte de adversários/slots v75, os 24 casos de abertura e percepção da v79, os 543 casos de mapas/adversários v76 e os 17 casos de pesquisa v77. O resumo final e os hashes ficam em `reports/v80-validation.json`.

Este cenário sem ordens do jogador confirma continuidade funcional; dificuldade e intensidade dos ataques ainda devem ser ajustadas conforme os testes de partida. A integração integral da IA binária, eventos e decisões da campanha original permanece pendente conforme documentado na v78.
