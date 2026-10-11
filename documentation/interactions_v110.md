# Fome local e produção inimiga — v110 — 2026-10-04

A última orientação do usuário mantém fome também nas tropas militares. Substitui a antiga solicitação de isenção: fome militar, por si só, não é mais bug. Não foi removida nem enfraquecida a perda de vida por falta de alimento.

## Correções confirmadas

Busca automática de alimento limitada a um círculo de seis quadrados a partir do ponto onde a fome interrompeu a tarefa, com caminho máximo de doze quadrados. Prioriza comida solta no chão; depois armazém próprio ocupado com alimento. Sem opção local, espera e sofre as consequências da fome; não encadeia buscas que atravessam o mapa. Preserva carga e tarefa para retomada após comer. O raio é uma escolha do remake para operacionalizar “próximo”, não um valor comprovado no original.

Importação de saves antigos normaliza ordens de alimentação distantes. Formato interno 36, mesmos nomes de slots research-v33. Os arquivos reais não são convertidos ou regravados nos testes.

Em BuildingPopulation.ReleaseResident, registros históricos de treinamentos concluídos bloqueavam a saída de colonos reconvertidos. O slot 15 do vermelho na cópia da partida reproduziu esse caso. Agora somente treinamento em andamento bloqueia a saída. A distribuição de população também tenta os demais prédios quando não consegue atender o primeiro; uma economia com cultivo e população estabelecidos pode transferir excedentes do santuário mantendo pelo menos um operador.

## Continuação da partida real, sem recursos concedidos

Snapshot do slot 1, missão 5, tick 61865 (34m22s), continuado por dez minutos simulados. Nenhuma decisão manual pela interface.

| Clã | Comida inicial → final | Novas tropas treinadas | Pesquisas concluídas no período |
|---|---:|---:|---:|
| Vermelho | 14 → 11 | 0 | 0 |
| Amarelo | 49 → 78 | 0 | 1 |
| Verde | 37 → 24 | 4 | 0 |

O verde voltou a produzir e os laboratórios amarelo/verde chegaram a dois operadores. Contagem de soldados vivos não equivale ao total produzido: houve perdas no período, inclusive fome. Não é verdade que todos os clãs já sustentam desenvolvimento satisfatório. O vermelho permanece com comida abaixo da reserva (18 ao fim), apesar de cultivo/quartel operados. O amarelo mantém só um colono no quartel, insuficiente para consumir um no treinamento preservando o operador. Esses gargalos econômicos/populacionais exigem investigação adicional; não liberar produção gratuita nem prometer ofensiva garantida.

## Verificações

- 11 verificações específicas de alimentação, perda de vida, prioridade, saída do colono reconvertido e persistência passaram.
- 47 verificações de abertura econômica e 35 de conclusão da missão 5 passaram.
- Os cinco slots atuais foram copiados, carregados, normalizados, serializados novamente e continuados por 300 ticks em duas instâncias determinísticas: todos passaram.
- SHA256 de 21 saves reais: nenhum alterado.
- Build/publish Windows passou. Não houve teste humano de interface, animação ou áudio.
- Falhas iniciais do ensaio local vieram do próprio fixture (cache de pilhas não invalidado e recursos iniciais diferentes na instância de carregamento); corrigidas no teste, sem desativar validação de conservação.

Código: Engine/HungerSimulation.cs, BuildingPopulation.cs, Simulation.cs, OpponentPlanning.cs, Tests/LocalHungerDevelopmentTests.cs, Program.cs.
Evidências: experiments/fome-producao-v110 (replay-final.json, local-tests.json, opening.json, ending.json, save-compatibility.json, saves-preservation.json).
Backup dos quatro arquivos iniciais: backups/antes-fome-producao-v110-20261004. Entrega anterior completa interactions-v109 preservada.
Pacote: releases/interactions-v110. Atalho: D:\CHATGPT\Jogar-Quinta-Missao.cmd. Comparação: D:\CHATGPT\Jogar-Quinta-Missao-Anterior-v109.cmd.
