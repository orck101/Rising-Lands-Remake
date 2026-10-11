# Fazendeiro — v23

## Regra confirmada pelo usuário

Com o fazendeiro selecionado, manter Ctrl pressionado mostra um cursor de coleta sobre comida plantada ou pilhas de alimento; em locais sem alimento, mostra círculo com barra de proibido. Ctrl + clique define a fonte; clique sem Ctrl define o destino. Alimento pode ser transportado entre pontos de chão livre acessível ou entregue ao armazém. Só o depósito credita o estoque disponível.

## Causa e correção

v22 generalizava SourceAvailable e GatherAt para todas as pilhas (66/67/68), inclusive para o fazendeiro. O teste de rejeição de engrenagem falhou antes da correção (logs/farmer_v23_before.log). Agora as duas camadas restringem o fazendeiro a IsFood; regras do construtor permanecem como estavam. Clique comum em comida não inicia coleta independente de Ctrl.

A indicação anterior era texto/quadrado com seta do sistema. Agora a seta é ocultada durante Ctrl no mapa, sendo substituída por cesta com seta para baixo ou círculo cortado. Ao soltar Ctrl, sair do mapa ou abrir menu, a seta normal retorna. São ícones modernos, não imagens extraídas do cursor original.

Também corrigida a transferência a partir do último destino: o destino anterior era indevidamente usado para rejeitar a nova fonte. A nova ordem limpa esse destino antes de iniciar a coleta e o restaura se a ordem falhar.

## Validação e limites

120 verificações focadas na primeira missão passaram. Incluem recusa de pedra/engrenagem, planta para chão, chão para outro chão, nova coleta no destino anterior, entrega final ao armazém, conservação e save/load. Capturas v23-farmer-collect.png e v23-farmer-prohibited.png revisadas: são cenários controlados de desenho, não sessão manual de mouse. Percurso automatizado da missão até a vitória permanece aprovado.

Chão livre significa célula acessível, sem edifício ou objeto incompatível; pilhas mantêm limite de31 unidades. Não permite depositar dentro de obstáculos. Temporização e alcance da repetição continuam provisórios. A versão não pretende concluir os comandos de todos os personagens.
