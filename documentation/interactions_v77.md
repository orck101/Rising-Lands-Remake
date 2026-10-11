# v77 — reconstrução das regras e tecnologias dos adversários

Esta entrega amplia o adversário jogável. **Não constitui ainda a reprodução integral da IA original.** O interpretador original foi reconstruído e validado isoladamente; seus adaptadores de grupos, recursos, guerra e diplomacia ainda não substituem integralmente o controlador de gameplay. Carregar os 1.157 registros não equivale a executar todos os seus comportamentos no jogo.

## Mudanças aplicadas ao jogo

- Expansão segue a tabela original de prioridades de construções, por níveis de demanda, com uma solicitação por decisão. Substitui o limite provisório de três armazéns.
- Adversários usam os níveis tecnológicos iniciais calculados pelo original para cada missão, junto dos níveis herdados da campanha. Trabalhadores, cultivo, fadiga, dano militar e velocidade de construção consultam a tecnologia do próprio jogador.
- Produção militar considera os desbloqueios tecnológicos, além das tropas iniciais do mapa. Continua exigindo prédio correto, colonos e recursos reais daquele proprietário.
- A escolha do ramo adversário reproduz os pesos e o gerador aleatório originais. O ramo espiritual fica excluído antes da missão 7. A semente do remake deriva do mapa para permitir testes e saves determinísticos; não reproduz a semente temporal de uma partida original.
- Laboratórios inimigos avançam apenas com colonos efetivamente presentes, pausam sem perder progresso e completam no máximo duas descobertas por missão. O tempo segue a alteração solicitada pelo usuário: 420 segundos com um colono e 240 com dois ou mais, em 1x. O tempo original é diferente.
- Na passagem de missão, o ajuste tecnológico adversário segue a rotina original que considera o número de descobertas humanas. Esse ajuste é separado das pesquisas feitas pelo laboratório inimigo.
- Save v33 preserva ramo, pesos, estado aleatório, níveis e pesquisa adversária em andamento. Saves anteriores continuam sendo lidos; o arquivo antigo não é sobrescrito pela migração.

## Evidência, método e certeza

Fonte: cópia `working/RISING/RISING-ORIG.EXE`, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. Análise estática com símbolos e execução isolada das rotinas x86 em Unicorn. As fixtures inicializam as tabelas de invenções com os valores extraídos do INI e interceptam efeitos externos indicados em cada relatório.

| Descoberta | Região original | Evidência e validação | Certeza |
|---|---|---|---|
| RULE usa registros de 20 bytes, descritor de argumentos/variáveis e chamadas a subproblemas | RISING.RUL, offsets em opponent_rules.json; iaSolver 0x47ccdc | 50 programas controlados, 500 estados/callbacks comparados ao x86; 500 restaurações | CONFIRMADO para o interpretador; cobertura não inclui os 140 callbacks reais |
| Apenas -1 representa falha; zero pode ser sucesso | desvios de iaSolver | fixtures exercitam retorno, filhos, espera e retomada | CONFIRMADO |
| Aritmética da IA inclui divisão inteira e porcentagem truncada antes da multiplicação | 0x480c48–0x480cca | 210 resultados nativos | CONFIRMADO |
| Cidade solicita o primeiro déficit elegível por nível e ordem da tabela | iagVilleNoyau 0x483fb0; tabelas em 0x498ba4 | 100 casos nativos com tipos permitidos e quantidades variáveis | CONFIRMADO para a demanda; seleção do modo ainda provisória |
| Níveis iniciais e desbloqueios por jogador | RechercheCalculNew 0x4713ff; 0x5080e4 | 25 missões × 4 jogadores, efeitos identificados | CONFIRMADO dentro das condições de fixture documentadas |
| Escolha do ramo e estado aleatório | iaReset 0x47ed36; iaRandom 0x4892d4 | 25 casos, escolha/pesos/semente final iguais ao x86 | CONFIRMADO; semente inicial adaptada |
| Ajuste dos inimigos ao passar de missão | IaRechercheAjuste 0x47f450; chamada em 0x42cb15 após MissionNext | 100 casos, 0 a 3 descobertas humanas, missões 1–25 | CONFIRMADO |
| Pesquisa processa os quatro jogadores, exige pesquisadores e limita duas descobertas | RechercheCalcul 0x47114c; pesquisadores 0x508164; contagem 0x50870c | 96 estados de jogador; 0–3 pesquisadores, 0–2 descobertas, limiar de conclusão | CONFIRMADO; efeitos de invenção interceptados |
| Incremento temporal original é 2 - 1/N para N pesquisadores, zero sem pesquisadores | 0x47118f–0x4711aa; constante em 0x494ca7 | valores de progresso conferidos em execução x86 | CONFIRMADO; substituído conscientemente por 420/240 s no remake |

## Validação da versão compilada

- 935 comparações do interpretador, aritmética, cidade e campanha; 500 restaurações do interpretador.
- 17 verificações de integração: laboratório vazio, um/dois colonos, tempo exato, pesquisa parcial salva/restaurada, duas descobertas, pausa e conservação dos estoques.
- 543 verificações da v76 nas missões disponíveis, incluindo restauração e continuação determinística. As variantes de mapas continuam cobertas pela leitura geral dos 34 mapas.
- 351 verificações gerais; 28 cenários adversários da v75 e 23 verificações de slots.
- Dez saves existentes lidos e hashes preservados, totalizando 20 verificações. Nenhum save do jogador foi alterado.
- Captura `reports/v77-opponent-gameplay.png` inspecionada: adversário vermelho ativo, economia, unidades e construção. Não é prova de uma partida completa nem de fidelidade integral.

## O que falta para a solicitação integral

O inventário `reports/opponent-integration-audit-v77.json` contém 42 raízes RULE ativas e 140 callbacks distintos. Sete callbacks de aritmética foram portados e comparados; **133 ainda precisam de adaptadores de gameplay comprovados**. Algumas rotinas foram reconstruídas separadamente e já aplicadas ao controlador, conforme a lista acima; isso não fecha esses adaptadores.

Ainda permanecem provisórios: composição e distribuição dos grupos, interrupções/prioridades de trabalho, recrutamento, exploração, seleção de alvos, ataques coordenados, associação dos planos às cidades e iniciativa diplomática. A condição de saída do modo de sobrevivência usa prédios habitados como aproximação das flags de serviço originais. Tripulação e alocação dos laboratórios ainda usam essa distribuição provisória.

Eventos, ondas de ataque e condições específicas de cada missão precisam de validação individual. Os registros de ataque de oito bytes não foram transformados em temporizadores ou destinos inventados; a rotina IAAttaqueCodeTest observada é uma função vazia. A navegação continua sendo a implementação provisória documentada no código. A continuidade de pesquisa parcial entre mapas e os efeitos completos das invenções adversárias também precisam de investigação adicional.

Próxima sequência de reconstrução: adaptadores de cidade e grupos de recursos; ciclo de atribuição/retorno de trabalhadores; grupos de guerra e coordenação de ataques; iniciativa diplomática; eventos específicos da missão 4; aplicação às demais missões com comparação das decisões e estados do original. Cada adaptador precisa de fixtures nativas e teste de integração antes de substituir a regra provisória correspondente.

Os arquivos originais e saves de D:/RISING foram preservados.
