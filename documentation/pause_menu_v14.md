# Reinício de partida e conteúdo do menu original — v14

Release: `releases/menu-v14/RisingLandsRemake.exe`, 27/09/2026.

## Entrega jogável

Esc → Reiniciar partida abre confirmação, com **Voltar sem reiniciar** selecionado inicialmente. Confirmar restaura o estado inicial do mapa ou treino atual, inclusive relógio, unidades, recursos, construções, recrutamentos e projéteis. Grupos, alerta de ataque, arraste, colocação e destino são limpos; a câmera volta à unidade selecionada. A partida reiniciada começa sem pausa manual.

O estado inicial é uma fotografia em memória criada pelo construtor da simulação. Carregar um save não substitui essa fotografia. Reiniciar não grava, apaga ou substitui nenhum save. Não há alteração do formato v7. Os saves das releases anteriores continuam em suas respectivas pastas; esta versão não os migra automaticamente.

## Evidência do original

**CONFIRMADO:** o bloco de configuração de MenuInit(16), iniciado em `0x468e8b`, pede o título de ID 31 e seis itens, nesta ordem:

| ID | Texto de TEXTES.TXT |
|---|---|
| 5 | LOAD |
| 6 | SAVE |
| 10 | MISSION OBJECTIVE |
| 20 | ABANDON THE MISSION |
| 11 | RESTART MISSION |
| 12 | BACK TO THE GAME |

Método: execução isolada das instruções da cópia de RISING-ORIG.EXE em Unicorn. TextGetMessage (`0x478967`) retorna ponteiros sintéticos identificáveis; são registradas as chamadas e lida a tabela resultante em `0x4e2f30`, com contagem em `0x4e2f2c`. InitGameZone (`0x471720`) é interceptada e recebe 3,0,0,0. A execução para em `0x468b73`, antes de entrar no restante do menu. Nenhuma API gráfica ou processo legado é executado.

Fonte executável: SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. A correspondência entre IDs e textos usa a tabela comentada de `working/RISING/TEXTES.TXT`; o relatório registra hash e offsets de cada linha. O resolvedor TextGetMessage completo não foi executado.

Ferramenta reproduzível: `tools/trace_pause_contents.py`. Evidências: `reports/pause_contents_original_validation.json`.

**DESCONHECIDO nesta investigação:** despacho nativo dos cliques, rotina original de reinício, consequências para campanha/eventos, desenho e posições dos itens. A existência de RESTART MISSION é comprovada; a implementação moderna de reinício não foi comparada à rotina nativa.

**MODERNO/PROVISÓRIO:** o menu jogável mantém Continuar primeiro e acrescenta Reiniciar antes de Sair. Não reproduz ainda a ordem ou aparência original. Objetivo de missão e abandono de campanha dependem da reconstrução de eventos; não foram substituídos por ações fictícias.

## Validação e registro de alterações

239 verificações aprovadas em `logs/selftest_menu_v14.log`. Os novos testes cobrem cancelamento, restauração exata da simulação em mapa normal e treino de arqueiros, carregamento do estado anterior ao reinício e preservação da fotografia inicial após carregar. Capturas do menu principal e confirmação inspecionadas em 1280×800, em `reports/prototype_menu_v14_*.png`. Não houve teste automatizado de cliques físicos nem comparação visual com a tela nativa.

Arquivos alterados: `Engine/Simulation.cs` (fotografia e reinício), `Engine/PauseMenu.cs` (confirmação e ação), `Engine/ResearchGame.cs` (captura da confirmação), `Tests/PauseMenuTests.cs` e `Tests/SelfTests.cs`. Código e resultados permanecem fora das árvores original/working. O relatório `reports/menu_v14_validation.json` registra hashes da entrega e integridade das 1.340 cópias preservadas.

Próxima investigação: seguir o despacho do item MISSION OBJECTIVE e relacionar o texto exibido às missões, sem confundir o diário completo com o objetivo ativo. Campanha, IA estratégica e fidelidade integral continuam pendentes.
