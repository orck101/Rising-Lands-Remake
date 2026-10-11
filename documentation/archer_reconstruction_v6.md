# Arqueiro e projéteis — versão 6, 27/09/2026

Release: `releases/archer-v6/RisingLandsRemake.exe`. O combate inicial da v5 e sua economia continuam disponíveis. Esta etapa não conclui campanha, IA ou fidelidade integral do remaster.

## Controles

F3 abre o treino de arqueiros. Selecione a unidade de barra verde e clique direito na de barra vermelha. F3 novamente volta ao cenário normal. F2 abre o treino de espadachins. Trocar de treino reinicia o estado de laboratório atual; use F5 antes para conservar a partida. Os dois treinos têm identidades próprias, separadas das missões.

No jogo normal, o arqueiro já importado do primeiro mapa também recebe ataques explícitos. Arrastar seleciona grupos, botão direito no chão move, X interrompe novas ordens, Espaço pausa. Uma flecha já disparada continua depois de X. O círculo pontilhado indica o raio de alcance; é um auxílio moderno de interface.

F5 grava saves `research-v6.json`. F9 procura v6, depois v5 e v4 na mesma pasta `PrototypeSaves`. O carregador aceita os três formatos; saves antigos começam sem projéteis. Nenhum save do jogo original é alterado ou importado como se fosse save moderno.

## Comportamento integrado

- Espadachins e arqueiros atacam unidades humanas de outros jogadores 0–3; alvos grandes, construções e outros tipos de ataque ainda não estão liberados.
- Arqueiros param dentro do alcance, disparam, aguardam recarga e revidam quando atingidos. Continuam sem IA estratégica ou aquisição espontânea de inimigos.
- O dano de uma flecha ocorre ao fim do trajeto. O alvo pode sair da célula mirada, resultando em erro. A flecha não acompanha o alvo nem procura outro ocupante.
- Flechas já disparadas continuam mesmo se o atacante for derrotado. Flechas destinadas a um alvo já derrotado são descartadas.
- Posição, progresso, origem, destino, dano e momento de lançamento são salvos; retomada mantém a sequência determinística.
- A animação do arqueiro e o gráfico do projétil são extraídos dos comandos de desenho do original. Os gráficos antigos não foram editados.

As regras de acerto, persistência após derrota e descarte acima são decisões **EXPERIMENTAIS** da integração. Não foram comprovadas como reprodução da rotina completa de impacto original.

## Evidências e graus de certeza

Origem: cópia `working/RISING/RISING-ORIG.EXE`, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`; mapa copiado MONDE01 e configuração exportada em `source_remake/Data/units.json`.

| Descoberta | Fonte/região e método | Certeza |
|---|---|---|
| Arqueiro é tipo 8, seção VOLEUR, PORTEE=10, RECHARGE=30 | INI; exportação com offsets em units.json | CONFIRMADO como valores de configuração |
| Instância do arqueiro contém limiar quadrático 100 | MONDE01, unidade tipo 8/slot 15; registro após índice: byte+688 no arquivo = memória+176 | CONFIRMADO para a amostra |
| Alcance entre unidades usa distância quadrática | `EnginAPortee` `0x446cb5`, caminho alvo tipo 1 e atacante com memória+120=1; `Distance2d` `0x4791e5`; comparação em `0x446ed2–0x446ee0` | CONFIRMADO com bônus zero: 1.875 posições/limiares |
| Atacante utiliza célula memória+8; alvo utiliza memória+124 | leituras em `0x446cf9` e `0x446d32` | CONFIRMADO na rotina; atualização dessas células durante caminhada ainda parcial |
| Tipo de projétil do arqueiro amostrado é 9 | MONDE01 registro+121; `CalculAttaqueEngin` lê memória+121 em `0x4527ce` e chama `ProjectileInit` em `0x4527d1` | FORTE EVIDÊNCIA da ligação estática; não emulamos toda a máquina de ataque |
| Ataque possui 10 fases, atraso 2 na amostra e 16 direções aceitas | registro memória+166/+168; emulação `AffAttaqueEngin` `0x41374c` | CONFIRMADO: 160 combinações de desenho |
| Projétil tipo 9 desenha sprite 10896 com sombra, deslocado −1/−1 | `AffProjectile` `0x46fe60`, tipo e posição controlados, iluminação forçada | CONFIRMADO para esse estado gráfico |
| Trajeto linear usa rasterização inteira de linha | `ProjectileLineaire` `0x46df39` inicializa; `ProjectileCalculLineaire` `0x46e3ba` avança uma posição | CONFIRMADO: 40 caminhos, 9.255 posições, inclusive origens fora do centro da célula |

O algoritmo de linha usa eixo maior, erro inicial `2*menor-maior`, passo diagonal quando erro≥0, atualização `2*(menor-maior)` no diagonal e `2*menor` no passo simples. O destino nativo desses testes é o centro da célula: `32*célula + 16`. Há casos horizontais, verticais, diagonais, todos os sinais, início=destino e coordenadas iniciais deslocadas.

O sprite 10896 é desenhado como retornado pela rotina nativa. Não foi inferida uma rotação de flecha por aparência. A sombra da engine moderna continua com a aproximação visual documentada nas versões anteriores.

## Reprodução e verificação

`tools/trace_archer.py` emula exclusivamente as rotinas listadas, utilizando cópias do EXE e do mapa. Bônus de alcance é zero, iluminação é controlada e chamadas gráficas são capturadas. Nenhum processo legado ou API do Windows é executado.

Saídas:

- `reports/archer_original_validation.json`: contagens, chamadas interceptadas e endereços.
- `source_remake/Data/archer_rules.json`: vetores de alcance, trajetórias e comandos do projétil.
- `source_remake/Data/archer_attack.json`: animações e metadados originais do arqueiro.
- `logs/selftest_archer_v6_published.log`: regressões anteriores e testes novos do executável final.
- `reports/archer_v6_validation.json`: verificação da publicação, hashes e integridade das cópias.
- `reports/prototype_archer_v6_final.png`: captura inspecionada da versão final.

Os testes C# comparam todas as posições exportadas e todos os casos de alcance. Também cobrem disparo sem dano instantâneo, permanência em posição quando já está no alcance, duelo, salva/carrega com flechas voando, parar após disparar, esquiva, atacante derrotado, alvo fora do alcance, trajeto de tamanho zero e migração de saves antigos. Saves inválidos são rejeitados antes de alterar o mundo.

## Limites e próximos passos

**EXPERIMENTAL:** velocidade de 8 pixels/tick a 30 Hz, momento de disparo na animação, origem visual do lançamento, cálculo de acerto por célula no fim do voo, percurso de aproximação e retaliação. A geometria do trajeto é validada; isso não demonstra velocidade, arco, obstáculos ou impacto fiéis ao original.

**PENDENTE:** interpretação integral de `ProjectileAvance` e `ProjectileInfligeDegat`, dispersão/RNG, interceptação por terreno/construções, arcos e altura, outros projéteis, efeitos sonoros, diplomacia, defesa automática, produção militar e missões completas. A primeira prioridade de fidelidade é ligar o disparo à fase correta da animação e reconstruir o caminho de impacto do projétil tipo 9.

O círculo usa um raio contínuo para orientação visual; a regra de alcance atua nas células e produz uma fronteira discreta. Carga de trabalhadores derrotados permanece contabilizada e ainda não pode ser recuperada, como na v5.
