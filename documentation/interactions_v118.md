# Regras de recuperação e ofensiva — v118

2026-10-09. Regra de remake solicitada pelo usuário; não atribuída à IA original.

## Tecnologia inicial do verde

Em toda missão, cada ramo do proprietário 3 tem piso `min(máximo do ramo, nível nativo dessa missão + 2)`. O nível efetivo é o maior entre esse piso e o progresso herdado. Não se somam dois pontos a cada carregamento, reinício ou transição. Não altera os pisos do jogador, vermelho ou amarelo, nem cria adversários onde eles não existem.

Na entrada direta da missão 5: verde Agricultura 4/19, Engenharia 9/24, Armamento 4/20, Espiritualidade 2/18. Progresso herdado superior é mantido. Saves anteriores usam o novo piso ao serem carregados nesta versão, sem edição dos arquivos existentes. Não implica desbloquear automaticamente habilidades que exigem aprendizado/produção, dispensar custo ou permitir pesquisas acima do máximo.

## Defesa e ataque

Meta de produção comum: maior entre a meta do perfil e guarnição dinâmica + tamanho máximo do grupo ofensivo. Feridos abaixo de 50% permitem reposição adicional limitada ao tamanho de um grupo. Continuam vivos; não são curados gratuitamente, não viram candidatos saudáveis e não se removem as restrições de ataque, economia ou defesa. A missão 4 preserva sua política específica de pressão tardia.

A preparação de ataque utiliza a guarnição existente e completa somente as vagas faltantes. Não faz união com uma segunda guarnição independente. Recrutamento militar pago pode liberar capacidade do estoque; a reserva de recursos para armazém não paralisa esses treinos. O recrutador ainda exige os recursos reais, habitantes, tecnologia e margem alimentar.

## Recuperação produtiva

O piso de fazendeiros considera manutenção, produtividade observada e limite da missão, com mínimo dois. Trabalhadores já a caminho da reconversão deixam de contar como futuros produtores, evitando consumir o mesmo excedente várias vezes.

A recuperação pode transferir um colono excedente de serviços secundários ao santuário, preservando o último operador, reservas de treinamento e os operadores de armazém/cultivo. Um único fazendeiro pode fornecer ao cultivo quando é preciso recuperar população. Assim que existe colono livre no santuário, a próxima comida vai ao estoque para contratar trabalhadores antes de continuar gastando tudo em nascimentos.

Expansão a 70% continua registrada, mas sua reserva de operador/orçamento é adiada quando a base precisa recuperar trabalhadores. O pedido não é apagado e volta a ser avaliado depois. Sem créditos gratuitos, unidades gratuitas, aceleração de coleta ou mudanças em fome/visão/alianças.

## Arquivos

Engine/OpponentRecovery.cs, OpponentDevelopment.cs, OpponentPlanning.cs, OpponentSimulation.cs, OpponentOpening.cs, OpponentWarehouses.cs, OpponentNativeRules.cs; Tests/RecoveryV118Tests.cs e Program.cs.

Formato de save 41 e nomes de slots v33 mantidos. Backup: backups/antes-regras-ia-v118-20261009. Pacote: releases/interactions-v118. Evidências: experiments/regras-ia-v118. Ver reports/regras_ia_v118.md para resultados e limites.
