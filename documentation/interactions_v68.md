# v68 — correção das respostas sonoras repetidas

Abra `../releases/interactions-v68/Jogar-Terceira-Missao.cmd` (primeira e segunda também disponíveis). Use esta versão para conferir a correção; a v67 foi preservada.

## Causa confirmada

Uma ordem de coleta acionava novamente a fala de confirmação nas repetições automáticas. Isso ocorria em `Engine/FoodSimulation.cs`, `MiningSimulation.cs`, `WarehouseTransfer.cs` e `DeliveryIntelligence.cs`. Separadamente, `Engine/AudioSystem.cs` só usava intervalos de 500 ms para vozes e 120 ms para efeitos, permitindo iniciar outra instância antes de o arquivo anterior terminar. Há falas originais de coleta superiores a um segundo.

O teste anterior falhou: uma ordem produziu 19 confirmações em 8.000 atualizações (18 entregas). Evidência: `../logs/v68-audio-before.log` e pacote de reprodução `../experiments/v68-before`.

## Mudanças

- A ordem explícita aceita confirma uma vez; a repetição automática da tarefa não solicita uma nova fala.
- Coleta do chão, colheita, mineração, retirada do armazém e mudança automática de fonte preservam as tarefas e suas cargas.
- Apenas uma voz de confirmação toca por vez, inclusive com grupos de classes diferentes. Pedidos suprimidos não entram em uma fila para tocar atrasados.
- Um personagem ou prédio não inicia uma segunda cópia enquanto seu áudio está ativo. Efeitos de outros trabalhadores continuam permitidos.
- Alertas idênticos ativos não se sobrepõem; alertas diferentes continuam disponíveis.
- Nova ordem pode substituir o som de trabalho daquele personagem para confirmar a ordem, sem empilhar falas.
- Seleção por retrato usa o mesmo evento original da seleção normal.
- Atualização de volume preserva a atenuação de cada instância.

Nenhum WAV foi substituído. Formato de save continua v29; originais e saves existentes não foram reescritos.

## Validação

854 verificações aprovadas: 14 específicas de áudio e 840 de coleta, fome, missão 2, simulação e leitura de saves. O teste corrigido para após oito entregas: uma confirmação, oito eventos de trabalho. Os períodos anterior/posterior diferem; não são uma comparação de desempenho.

O teste de mina esgotada também confirma uma única resposta ao mudar automaticamente para pedras próximas. A retirada do armazém entrega várias cargas sem repetir a confirmação.

No dispositivo de áudio real, cinco solicitações silenciosas iniciam duas instâncias e bloqueiam três sobreposições; a voz e o efeito de outro trabalhador podem coexistir. Os 551 arquivos originais foram decodificados e reproduzidos silenciosamente. Ver `../reports/v68-final-audio-runtime.json` e `../reports/v68-validation.json`.

A qualidade subjetiva da escuta não foi avaliada automaticamente. Para conferir no jogo, mande um trabalhador coletar e entregar, aguarde vários ciclos e repita com um grupo. Deve haver confirmação inicial sem coro de falas nem respostas automáticas a cada viagem; sons de trabalho continuam existindo. Música do CD e outras pendências da v67 permanecem fora desta correção.
