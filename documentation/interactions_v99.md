# v99 — minimapa, quantidades e entrada na troca

## Mudanças

- Minimapa ligado por padrão na abertura, nova partida, reinício, carregamento e mudança de missão. Tab continua permitindo ocultá-lo manualmente durante a partida; a próxima abertura/carregamento restaura o padrão ligado.
- Na edição de quantidades da troca, pressionar a seta soma/subtrai 1 imediatamente. Após 350 ms, repete a cada 70 ms enquanto o botão estiver pressionado sobre a mesma seta. Soltar, sair do botão ou perder o foco interrompe a repetição. Para recomeçar, é necessário um novo clique. A velocidade 1x–5x não altera essa cadência.
- A quantidade permanece entre zero e o estoque disponível. Digitação, Enter para confirmar e o atalho existente Shift para passos de 10 permanecem disponíveis.
- Mensageiros no mercado/santuário e tropas entregues à bolsa agora fazem uma aproximação final visível pela porta, usando seus frames originais de caminhada. Somente após entrar ficam ocultos. A transferência de tropas espera a conclusão da entrada.
- A entrada em andamento é salva com a negociação e retomada após carregar. A curta passagem pela porta não é desviada pela procura automática de alimento; a abordagem anterior continua respeitando a fome.

## Investigação do original

Fonte: cópia preservada de `RISING-ORIG.EXE`, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. Nenhum original foi modificado ou executado como processo do jogo.

CONFIRMADO por desassemblagem: `BatimentGetCaseEntrer @ 0x41d0f7` diferencia a entrada na bolsa (tipo 8). Retorna âncora + `0x303` para personagens e + `0x403` para veículos/tipo 3. `SetEnginRentreBatiment @ 0x445bb8` e `CalculEntreBatiment @ 0x449716` tratam a entrada; a região `0x449827–0x449873` atribui estado `0x15`.

CONFIRMADO em renderização controlada: `AffEngin @ 0x415a27`, tipos 0, 5, 7, 9 e 10, direção 5, oito fases. Estado 21 produz os mesmos comandos gráficos da caminhada no estado 7; estado 12 não desenha a unidade. Foram observados 120 casos. O registro do mensageiro foi construído a partir de um registro de colono com o tipo alterado explicitamente. Não foram inventados novos sprites de entrada.

Evidências: `reports/v99-native-entry-render.json`, `reverse_engineering/v99-native-entry-evidence.txt`. Ferramenta reproduzível: `tools/trace_trade_entry_v99.py`.

O remake antes escondia a unidade no ponto frontal usado para entregas. Agora termina a caminhada 24 pixels mais para dentro, alinhando os pés à porta já calibrada na arte, mantendo os frames originais. Essa distância, o deslocamento de 2 pixels por tick e a duração resultante são adaptações da geometria do remake; o tempo exato e todas as variações de entrada de veículos do original não foram medidos nesta etapa. Portanto, os frames e a sequência visível→oculta estão comprovados, mas não se afirma reprodução integral de todos os trajetos originais.

## Validação

- `v99-tests.txt`: 16 verificações de pressionar/segurar/soltar, cancelamento ao sair do botão, decremento, entrada visível do mensageiro e da tropa, transferência após entrada e salvamento determinístico durante a entrada.
- `v99-ui.txt` e `v99-market-ui.png`: interação real dos botões de quantidade, limite do estoque, minimapa ligado na abertura e após carregar, confirmação e avaliação da oferta. Captura conferida visualmente.
- `v99-regression-trade.txt`: 24 verificações de troca de recursos/tropas, conservação, pré-requisitos e saves.
- `v99-regression-policy.txt`: 20 verificações da política econômica dos inimigos, incluindo simulação prolongada da missão 4.
- `v99-regression-v71.txt`: 96 verificações das mecânicas anteriores.
- `v99-player-saves.txt`: 15 saves antigos carregados somente para inspeção, preservando os arquivos.
- `v99-publish.txt`: publicação da versão jogável.

## Uso

Abra `releases/interactions-v99/Jogar-Quarta-Missao-Com-Monitor.cmd`. Para negociar recursos, escolha o recurso e segure a seta para cima/baixo até chegar ao valor desejado; solte e confirme com Enter. Para trocar tropas, envie as unidades prometidas à sua bolsa após a aceitação e observe a passagem pela porta antes de elas serem recebidas na bolsa do outro clã.
