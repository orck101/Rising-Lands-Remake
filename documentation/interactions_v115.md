# Interações v115 — defesa coordenada
2026-10-09. Regra de IA solicitada pelo usuário, não algoritmo original comprovado.

Pacote: releases/interactions-v115. Formato40, slots research-v33.json mantidos. v114 e backups/antes-defesa-v115-20261008 preservados. Originais e saves reais inalterados.

BaseDefense.cs reúne vigilância/alerta/invasão, memória de ameaças visíveis, força estimada, guarnição, setores, reservas, resposta a prédios/trabalhadores, contramedidas, suporte mágico e fortificações amarelas. OpponentSimulation/Opening/BasicPolicy/Development/Planning/EconomicOpening integram guarnição, custos, operadores e prioridades de construção/produção. AutomaticDefense limita repetição de alvo inacessível. BuildingCombat/ProjectileSimulation notificam ataques; MissionFourPressure respeita guarda/retorno emergencial. SpecialAbilities separa aprendizado por proprietário, visão e poderes inimigos; ConstructionSimulation contabiliza seus custos por clã. Simulation importa anteriores e salva40, limpando atribuições de propriedade obsoleta.

Tests/BaseDefenseTests.cs: 59 verificações. SpecialV66Tests mantém ensaios controlados de poderes; o alvo foi reinicializado após fome na preparação (falha também presente na v114). Program registra --test-base-defense. Regressões e cinco slots passaram; 21 arquivos de saves preservados. Missão6 permanece sem inimigos.

Relatório detalhado, limites e medições: reports/defesa_bases_v115.md. Evidências: experiments/base-defense-v115. Sem aprovação manual de balanceamento/FPS. Setores e flancos são heurísticas, não análise completa de todo gargalo topológico.
