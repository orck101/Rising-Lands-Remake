# v84 — produção inimiga exige tecnologia

## Diagnóstico confirmado

OpponentNativeRules.OpponentUnitUnlocked tratava a presença de uma classe em OriginalActorSeeds como liberação de treinamento. A alternativa ignorava a tecnologia. Teste reproduzível antes da correção em reports/v84-production-before.txt.

MONDE04 começa com níveis [2,2,7,0] por jogador, conforme Data/opponent_technology.json. Archer é tipo 8, liberado por MILITAIRE6 (RISING.INI, PARAM2=8); o teste Index < nível significa nível efetivo 7. O mapa original contém um arqueiro vermelho. Isso preserva a unidade inicial e não autoriza sua reprodução. O carro de alimento inicial tem a mesma distinção.

## Correção

Removida a exceção baseada em unidades iniciais. Toda nova ordem de treinamento inimigo exige o desbloqueio pelas pesquisas efetivamente desenvolvidas. Os caminhos normais, o diretor econômico e o backend original experimental usam o mesmo método. Não foram alteradas unidades existentes, parâmetros de combate, construções, mapa ou original.

Saves continuam compatíveis. Tropas já existentes e treinamentos já pagos em saves antigos são preservados; a correção rege novas ordens. Para observar o balanceamento inicial sem tropas criadas pela versão anterior, iniciar uma nova missão.

## Validação

- 88 verificações v84: missão real por seis minutos, arqueiro bloqueado, unidade inicial preservada, soldado permitido concluído, carro bloqueado, limiar de tecnologia 6/7, todas as 26 classes dos três donos inimigos, save/load.
- v75: 28 verificações da IA e suite de slots aprovadas.
- v79: 24 verificações de abertura e defesa aprovadas.
- v80: seis verificações, simulação de 24 minutos, economia e desenvolvimento contínuos, save/load e retomada determinística.

Registros em reports/v84-validation-0.txt a v84-validation-3.txt. Publicação interactions-v84 concluída.
