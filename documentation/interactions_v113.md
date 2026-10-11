# Bolsa de Trocas e simpatia — v113

Entrega em 2026-10-08: releases/interactions-v113. Regra comum aos jogadores e inimigos; formato 39 importa anteriores, mantendo os nomes dos slots research-v33.

## Colonos
Regra solicitada pelo usuário: colono não é mercadoria. Type 9 foi removido do catálogo de trocas e rejeitado nas propostas. Ordem de entrada na Bolsa usa ocupação normal, respeitando proprietário, capacidade e caminhada. Outros personagens permanecem armazenáveis para troca. Saves antigos com colonos armazenados são normalizados para operadores; colonos ainda a caminho continuam a aproximação como ocupantes. Negociações antigas ativas exigindo colonos são canceladas. Histórico concluído é preservado, sem desfazer transferências passadas ou editar o save real.

## Evidência do original
CONFIRMADO por análise e execução isolada do PE copiado working/RISING/RISING-ORIG.EXE: EvalueCoutProposition (0x466a40) calcula valor da oferta, ajusta pela simpatia e aliança e subtrai o pedido. Comida/pedra têm peso 1, engrenagem peso 2 (tabela 0x496f7c). Valores de personagens seguem tabela 0x496f60; o fallback existente para entradas zero não foi reavaliado.

Para a orientação jogador → inimigo:
valor ajustado = oferta × (80 + simpatia + 40 se aliado) / 100, truncado.
excedente = valor ajustado − pedido.
ganho = excedente / 5, truncado e limitado a zero no remake.
nova simpatia = simpatia anterior + ganho, limitada entre −50 e 50.

O original escreve excedente WORD em 0x4e2934 (0x466bd1). MarcheMajSympathie (0x460161, divisão em 0x460185) e finalização em 0x466478/0x466544 dividem por cinco. CalculNewSympathie (0x475193) soma e limita −50..50. Trinta e dois vetores nativos confirmaram avaliação e atualização, incluindo aliados e diferentes simpatias.

Exemplo sem aliança e simpatia zero: entregar 100 alimentos e receber 50 pedras resulta em valor ajustado 80, excedente 30, ganho de 6 pontos. Entregar 10 e receber 8 resulta em zero ganho. Isso altera a simpatia usada nas negociações seguintes; não cria uma aliança automaticamente.

O remake aplica o ganho somente depois de transferir efetivamente recursos/tropas, uma vez por negociação. Propostas, recusas, cancelamentos e falta de estoque não dão recompensa. Não reproduz o overflow unsigned de excedente negativo do original. Orientação específica do cálculo nativo para propostas recebidas de jogador tipo 2 permanece DESCONHECIDA; remake normaliza Give como jogador → inimigo em ambos os sentidos. Proibição de trocar colonos é regra do usuário, não descoberta comprovada do original.

## Validação
- 21 verificações específicas de ocupação, migração, propostas, simpatia, liquidação única e persistência.
- 32 provas executadas no binário original copiado.
- 24 regressões de comércio v97 e 67 de armazenamento v100.
- 35 verificações de conclusão da missão 5.
- Cópias dos cinco slots atuais carregadas, normalizadas e simuladas deterministicamente por 300 ticks.
- Hashes dos saves reais conferidos; arquivos não editados.

Evidências: experiments/trade-v113; ferramenta de investigação: tools/trace_trade_goodwill_v113.py. Backup: backups/antes-troca-v113-20261008.

## Limites
Não foi feita partida manual pela interface nem validação completa de toda a diplomacia original. Durante o teste de entrada foi observada uma janela transitória de referência de combate para unidade que acaba de ficar oculta: save nesse instante pode falhar com Invalid combat target; após dois ticks o estado fica carregável. Esse problema de sincronização é pendência separada, não corrigida nesta entrega. Gargalos anteriores da economia/produção da IA permanecem fora deste pedido.
