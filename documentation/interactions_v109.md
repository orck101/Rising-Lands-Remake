# Abertura econômica experimental — v109

Pedido: backup e prioridade de recursos nos primeiros dez minutos para os três inimigos, para avaliação em jogo. Aplicada na missão 5; demais missões preservadas.

Regra de design aprovada, não tradução comprovada da IA original. Tempo é simulado: dez minutos em 1x, aproximadamente dois minutos reais em 5x. Não injeta recursos, altera custo ou acelera coleta artificialmente.

Prioridades: santuário/armazenamento/cultivo, operadores vivos, crescimento de fazendeiros após armazém ativo, um construtor essencial, exploração segura de recursos quando conhecidos esgotarem, alimento ao cultivo apenas com reserva de sobrevivência e população abaixo da meta. Limita a três fazendeiros antes do cultivo operado e até seis depois, reduzindo a meta se a comida acessível for escassa. Mantém comida para alimentação antes de treinamento/novas obras. Adia laboratório, mercado, Bolsa, segunda base e ofensivas. Defesa automática permanece ativa; recrutamento militar durante abertura somente sob invasão e falta de guarnição, com recursos reais.

Corrigido conflito de planejamento durante abertura: a ausência de um cultivo ainda não construído fazia a IA reconverter trabalhadores recém-criados para colonos. Agora reconversão por falta de operador considera construções realmente existentes. Exceção condicionada à abertura da missão 5. Ao atingir 18.000 ticks, retoma os perfis e planejamento normais.

## Verificações
47 checks de abertura (sete checkpoints até 12 minutos, três clãs, limite temporal, ausência de saídas ofensivas e obras secundárias antes do limite, persistência/continuação determinística e isolamento das missões 4/6). 35 checks de conclusão da missão 5 passaram no pacote final. Comparação com v108 com mesmos recursos iniciais. Simulações do motor, não partidas manuais. Backup do código/dados com hashes e pacote v108 preservado.

## Aos dez minutos
| Clã | Comida antes → depois | Fazendeiros antes → depois |
|---|---:|---:|
| Vermelho | 1 → 4 | 2 → 3 |
| Amarelo | 2 → 19 | 2 → 6 |
| Verde | 1 → 0 | 4 → 4 |

Melhoria clara de crescimento no amarelo; vermelho com mais um fazendeiro, mas cultivo ainda atrasado aos dez minutos; verde continua com estoque baixo. Aos doze minutos o vermelho possuía cultivo sem operador no checkpoint. São limitações da experiência atual, não aprovação final de balanceamento. Fome militar antiga continua fora deste escopo e pode influenciar gastos e defesa. Estoque não é produção total: alimento é consumido por alimentação, recrutamento e construção.

Não substitui saves existentes nem reinicia partidas abertas. Recomenda-se iniciar partida nova da missão 5 para avaliar a abertura completa. Save após dez minutos não executa retroativamente a abertura; tempo da partida determina a política.

## Caminhos
Pacote: D:\CHATGPT\outputs\RisingLandsResearch\releases\interactions-v109
Atalho novo: D:\CHATGPT\Jogar-Quinta-Missao.cmd
Atalho anterior: D:\CHATGPT\Jogar-Quinta-Missao-Anterior-v108.cmd
Backup: D:\CHATGPT\outputs\RisingLandsResearch\backups\antes-abertura-economica-v109-20261004
Evidências: D:\CHATGPT\outputs\RisingLandsResearch\experiments\abertura-economica-v109

Arquivos: OpponentEconomicOpening.cs, OpponentBasicPolicy.cs, OpponentPlanning.cs, OpponentDevelopment.cs, OpponentOpening.cs, OpponentSimulation.cs, OpponentSafety.cs; Tests/EconomicOpeningTests.cs e Program.cs.
