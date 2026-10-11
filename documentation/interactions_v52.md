# v52 — ocupação do armazém consistente com os recursos disponíveis

Abrir `releases/interactions-v52/Jogar-Primeira-Missao.cmd`.

## Problema reproduzido

A imagem enviada pelo jogador mostra 110 alimentos e 50 pedras na barra, mas ocupação 100/100. O remake subtraía uma reserva aproximada dos recursos iniciais antes de somar a ocupação: `InitialResources - Spending`. Essa reserva não era um inventário separado, não aparecia no painel e também podia ser retirada pelos trabalhadores. O resumo e a verificação de entrega descontavam recursos que continuavam disponíveis para o jogador.

**CONFIRMADO no remake:** o teste `WarehouseAccountingTests` reproduziu 110 alimentos + 50 pedras a partir dos 30 + 30 iniciais, transferindo recursos de uma pilha/mina de teste para manter conservação. Antes da correção, a ocupação era 100, descontando 60. O teste falhou em `logs/v52-stock-before.err`. Após a correção, a ocupação é 160. O teste de restauração preserva todos os recursos desse estado.

## Correção

O resumo e a capacidade de entrega agora usam os mesmos totais de comida/pedra da barra superior e das retiradas do armazém. Removido o desconto aproximado da reserva. A ocupação é sempre comida + pedra; engrenagens mantêm a regra anterior e não entram nessa soma. A capacidade permanece 100 por armazém ativo e o espaço livre nunca fica negativo.

- 110 alimentos + 50 pedras, com um armazém ativo: **160/100, livre 0, Superlotado**.
- 50 alimentos + 50 pedras: **100/100, livre 0, Lotado**.
- 49 alimentos + 50 pedras: **99/100, livre 1**.

Novas entregas ficam limitadas ao espaço real; excedentes seguem o descarte ao redor do prédio implementado na v50. Se uma partida antiga estiver acima do limite, não são excluídos recursos nem alterados seus arquivos: ela exibe o total completo e impede novas entradas enquanto não houver espaço. Consumo, retirada ou ampliação da capacidade podem liberar espaço. Save permanece v20.

## Fidelidade e limites

O trace v37 confirmou que o executável original distingue reserva e total em sua estrutura interna. Isso não comprova a aproximação usada pelo remake para essa reserva. A v52 remove essa aproximação e atende à consistência solicitada pelo jogador; não afirma que reconstruiu a distribuição nativa de recursos entre os prédios ou a reserva interna original. Essa distribuição continua pendente de análise. A alteração v52 prevalece sobre os trechos de versões anteriores que excluíam recursos iniciais da ocupação.

## Validação

- 774 verificações de missão/interações e 351 gerais passaram: 1.125 no total.
- Teste dos valores exatos da imagem e restauração sem perda de recursos.
- Mineração real com armazém pago: 34 alimentos + 66 pedras completam 100; a pedra seguinte aparece no chão, sem crédito no estoque. O estado de descarte continua determinístico após salvar/carregar.
- Regressões de retirada contínua, espera, reabastecimento, construção, campanha e alcance de ataque passaram.
- Captura conferida `reports/v52-warehouse-accounting.png`: painel 160/100, livre 0, Superlotado e barra 110/50. Os valores dessa captura são preparados somente em memória para verificar a apresentação; não representam uma partida salva. A conservação é verificada separadamente pelo teste equilibrado.
- Logs: `logs/v52-test-mission-one.log`, `logs/v52-self-test.log`.

Arquivos, executáveis e saves originais preservados. As pendências da campanha continuam registradas nas entregas anteriores.
