# Cobertura e lacunas

Cada um dos 670 arquivos tem uma entrada em `reports/coverage_all_files.json`, incluindo hash, estado, evidência e pendência. Inventariado não significa engenharia reversa completa. Duplicatas são relacionadas por SHA-256 no inventário; a análise aprofundada de um conteúdo pode servir às cópias idênticas.

| Família | Arquivos | Conteúdos únicos | Situação | Pendência |
|---|---:|---:|---|---|
| .acv | 8 | 8 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .avi | 3 | 3 | Decodificação por FFmpeg | sincronização com eventos |
| .bin | 2 | 1 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .cmp | 16 | 8 | Estrutura e pixels extraídos | paletas dinâmicas, agrupamento de animações |
| .col | 2 | 1 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .cpl | 1 | 1 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .csp | 8 | 4 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .ddb | 2 | 2 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .dll | 135 | 87 | Inventário e cabeçalhos PE quando aplicáveis | implementação de terceiros não reconstruída |
| .drv | 112 | 83 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .eta | 34 | 34 | Sequência de carregamento reconstruída | semântica de campos; cauda de 917504 bytes |
| .exe | 15 | 11 | PE/strings; análise aprofundada de variantes Rising | muitas rotinas ainda sem reconstrução |
| .flc | 38 | 38 | Decodificação por FFmpeg | sincronização com eventos |
| .fnt | 3 | 1 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .gid | 1 | 1 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .hed | 6 | 3 | Descritores extraídos | byte extra e flags |
| .hlp | 1 | 1 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .ico | 3 | 1 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .inf | 80 | 58 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .ini | 24 | 12 | Texto e pares extraídos | significado e unidade de cada chave |
| .lbm | 3 | 1 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .lzs | 4 | 2 | Decodificado e comparado com x86 original | integração visual |
| .map | 2 | 1 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .mpd | 2 | 2 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .nec | 1 | 1 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .pie | 14 | 6 | Decodificado | uso e composição em runtime |
| .rar | 1 | 1 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .rul | 2 | 1 | Estrutura e callbacks extraídos | VM, probabilidades e agendamento |
| .sav | 4 | 4 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .sbk | 2 | 1 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .sbl | 2 | 1 | Decodificado, 1981 símbolos | nomes não provam comportamento |
| .snd | 6 | 3 | 551 clips convertidos em três bancos | todos os clips iguais ao original emulado; mistura pendente |
| .txt | 35 | 29 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .uc | 4 | 2 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .vxd | 93 | 61 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |
| .x86 | 1 | 1 | Inspeção básica/inventário | estrutura ou semântica ainda parcial/desconhecida |

## Lacunas prioritárias

A análise de saves cobre cabeçalhos, não o estado integral. A cauda ETA permanece preservada. FNT/COL/MAP/LBM precisam de leitores específicos. Drivers, instaladores, arquivos de configuração de terceiros e arquivos auxiliares não foram ignorados: estão identificados, mas sua implementação completa não foi reconstruída. A cobertura por família não é uma afirmação de porcentagem de gameplay recuperado.
