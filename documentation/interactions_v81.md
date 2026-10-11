# v81 — áudio das tropas inimigas

## Causa e evidência

AudioEvents.ReceiveGameSound verificava visibilidade e posição, mas não propriedade para o canal de falas (4). Eventos de nascimento, fome e ordens de inimigos podiam chegar ao dispositivo como respostas do jogador. A simulação real de MONDE04 durante seis minutos produziu 20 desses eventos. O teste antes da correção falhou em reports/v81-audio-before.txt.

AudioConcurrency limitava um som por fonte, mas fontes diferentes podiam tocar até 16 cópias do mesmo efeito. Isso tornava batalhas com grupos numerosas e sobrepostas.

## Alterações

- AudioPlaybackPolicy.CanPlay(GameSound) bloqueia falas do canal 4 de personagens de outros jogadores.
- ReceiveGameSound e o caminho direto UnitSound aplicam a mesma política.
- Efeitos do canal 2 têm no máximo duas cópias simultâneas do mesmo banco/chave. Efeitos distintos, alertas e respostas explícitas do jogador preservados.
- Não há alteração na simulação, IA, recursos ou saves. Não foram alterados arquivos originais.

## Validação

Teste v81: nove verificações, com captura real de eventos da missão 4, falas próprias preservadas, efeitos de combate e morte inimigos disponíveis e limite de sobreposição. Regressões v68/v69: 14 e oito verificações aprovadas. Teste geral registrado em reports/v81-validation-3.txt. Essa validação verifica emissão e políticas de reprodução; não é uma avaliação auditiva de cada gravação original.
