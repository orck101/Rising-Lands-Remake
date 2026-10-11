# v86 — objetivo e conclusão da missão 3

## Causa

MissionOne.HasMissionEnding reconhecia missões 1, 2 e 4, mas não a missão 3. Por isso concluir pesquisa na terceira missão nunca abria sua vitória normal.

## Regra solicitada

O objetivo original MONDE03 era uma descoberta (bloco mission_end, offset 338196, bytes 03000100ffffffffffff; texto original traduzido: construa laboratório e desenvolva uma tecnologia). A nova exigência é um ajuste explícito do jogador: construir laboratório, concluir as duas pesquisas normais, pagar 50 de alimento e concluir a terceira pesquisa extra.

MissionThreeSatisfied exige laboratório concluído do jogador, uma unidade própria viva, ExtraPurchased e pelo menos três descobertas efetivamente concluídas no histórico de pesquisa. Comprar a extra, progresso parcial, níveis herdados ou níveis concedidos no menu de testes não contam como a terceira descoberta. Mantém a avaliação de 100 ticks usada no remake; não é uma equivalência afirmada do tempo original.

A missão 3 foi incluída nas fases com encerramento. A validação de vitória em saves usa o histórico da pesquisa salvo, não o estado atual da simulação. Partidas antigas com três pesquisas e compra já feitas passam a concluir ao retomar a simulação. Progresso anterior com uma ou duas pesquisas é preservado.

Objetivos em português atualizados somente para MONDE03; o formato de 18 linhas por página foi preservado. Demais missões mantêm seus textos. O formato de save não mudou.

## Verificação

- 16 verificações v86: laboratório insuficiente, uma/duas insuficientes, custo exato 50, extra incompleta insuficiente, save/load, rejeição de vitória forjada antes da terceira descoberta, terceira concluída abre vitória, recuperação de save antes travado e passagem para missão 4 com ramo escolhido.
- Suite ResearchV58: pesquisas, tempos, extra, pagamento, pausas e salvamento aprovados.
- Self-test geral: ALL TESTS PASSED.
- Captura real dos objetivos inspecionada: reports/v86-objectives.png.

Relatórios: reports/v86-validation-0.txt, v86-validation-1.txt, v86-validation-2.txt. Publicação completa em releases/interactions-v86. A primeira tentativa foi interrompida por falta de espaço; o usuário liberou espaço e a publicação posterior terminou com código zero. Nenhum arquivo original foi alterado.
