# Preparação dos ataques e avanço das flechas — v7

Release: `releases/timing-v7/RisingLandsRemake.exe`, 27/09/2026. Reconstrução parcial, ainda sem campanha completa ou IA estratégica.

## Resultado jogável

Espadachim e arqueiro agora aguardam a preparação da animação antes de golpear ou disparar. A flecha nova avança 30 posições da linha por atualização e inclui os quatro componentes de rastro extraídos do original. O impacto ocorre depois de alcançar a última posição; o rastro pode continuar visível após o dano. Parar durante a preparação cancela o disparo, mas não remove uma flecha já lançada.

F2 abre o treino de espadachins; F3, arqueiros. Selecione a unidade verde e clique direito na vermelha. Trocar de treino reinicia a partida de laboratório: salve antes com F5. F9 procura saves v7, v6, v5 e v4 na mesma pasta. Os saves originais permanecem separados.

Saves novos usam versão 7 e conservam a preparação em andamento. Flechas importadas de saves v6 mantêm seu avanço antigo de 8 pixels por atualização até terminar o voo, evitando uma mudança de estado no carregamento. Flechas novas usam a regra v7.

## Evidências

Fonte: cópia `working/RISING/RISING-ORIG.EXE`, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. Instâncias de unidades amostradas de MONDE01. A emulação executa rotinas delimitadas em memória; chamadas de dano e desenho são interceptadas. Não executa o programa legado no Windows.

| Conclusão | Região e método | Certeza e escopo |
|---|---|---|
| Espadachim e arqueiro liberam ataque na fase 6, após 12 atualizações | `CalculAttaqueEngin` 0x451914, ramo 0x4523dc; comparações de memória+169 e +164/+122 em 0x4523e5–0x4523fc. Emulação da preparação, com fase inicial zero e atraso 2 | CONFIRMADO para os dois tipos e 16 direções, 32 casos; não demonstra o ciclo inteiro de recarga |
| Arqueiro cria projétil tipo 9 | Interceptação de `ProjectileInit` 0x46e33c durante a preparação completa acima | CONFIRMADO para a amostra |
| Uma chamada de atualização realiza 30 micropassos | `ProjectileCalcul` 0x46faff, comparação com 30 em 0x46fb20, chamada de avanço em 0x46fb4f | CONFIRMADO: 18 comprimentos e 71 atualizações completas |
| Rastro usa tipos 10–13, com atrasos de 2, 4, 6 e 8 micropassos | `ProjectileAvance`, criação em 0x46ee56–0x46ef39; leitura dos registros nativos de 56 bytes | CONFIRMADO nos mesmos vetores; byte+4=2 controla criação do rastro, não velocidade |
| Impacto acontece no micropasso seguinte ao último ponto da linha | Avanço 0x46ef3d, término do tipo 9 em 0x46ef6c; captura da chamada de dano | CONFIRMADO nos vetores, incluindo comprimento zero |
| Impacto encaminha célula de destino e identidade da origem | `ProjectileInfligeDegat` 0x46e4ee, argumentos em 0x46e4fc–0x46e510 para `InfligeDegat` 0x450874 | CONFIRMADO como argumentos; seleção final da vítima ainda DESCONHECIDA |
| Os cinco componentes desenham sprites 10896–10900 com sombra e deslocamento −1/−1 | `AffProjectile` 0x46fe60, iluminação controlada e captura dos comandos | CONFIRMADO para os tipos 9–13 no estado testado |

Os vetores de avanço cobrem comprimentos 0, 1, 2, 8, 29, 30, 31, 32, 59, 60, 61, 120, 128, 149, 150, 151, 319 e 320. São horizontais; a geometria em outras direções tem os testes anteriores da v6 (40 trajetórias, 9.255 posições), mas o agendamento nativo completo não foi novamente medido em todas essas direções.

## Reprodução

- `tools/trace_projectile_schedule.py`: produz `source_remake/Data/projectile_schedule.json` e `reports/projectile_schedule_original_validation.json`.
- `tools/trace_attack_release.py`: produz `source_remake/Data/attack_release.json` e `reports/attack_release_original_validation.json`.
- Executável com `--self-test`: 192 verificações aprovadas em `logs/selftest_timing_v7_initial.log`.
- `reports/prototype_timing_v7.png`: captura do treino inspecionada; confirma apresentação do cenário e combate, não mede sozinha o instante do disparo.
- `reports/timing_v7_validation.json`: hashes dos arquivos publicados e conferência das cópias preservadas.

Os testes incluem comparação das 71 atualizações, espera antes do golpe, fase gráfica durante preparação, cancelamento, retomada determinística, migração v6 e salvamento após impacto com rastro restante, sem duplicar dano. As regressões de movimento, economia, construção e recrutamento continuam aprovadas.

## Limites explícitos

**DESCONHECIDO:** frequência global e ordem completa das rotinas no jogo original. A engine continua usando seu relógio de laboratório de 30 Hz. Trinta micropassos por chamada nativa não comprovam a velocidade em segundos do jogo inteiro.

**PROVISÓRIO:** ciclo completo da recarga, pose durante o restante da recarga e ordem relativa entre unidades e projéteis. A fase de liberação foi reconstruída, mas não toda a máquina de estados de combate.

**PROVISÓRIO:** o remake ainda aplica o dano somente ao alvo originalmente selecionado se ele continuar vivo e na célula mirada. O original encaminha uma célula à rotina geral de dano. Acertar outro ocupante, construções, obstáculos, diplomacia e bônus exige reconstruir essa rotina antes de afirmar equivalência. O voo e o rastro novos continuam mesmo se o alvo morrer.

Próxima prioridade: reconstruir a seleção de vítima em `InfligeDegat`, preservando vetores de teste antes de mudar a integração. Campanha, eventos, IA estratégica, áudio e demais unidades continuam pendentes.
