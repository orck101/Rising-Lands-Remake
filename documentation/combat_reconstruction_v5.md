# Combate inicial — versão 5, 27/09/2026

Executável: `releases/combat-v5/RisingLandsRemake.exe`. Versão anterior preservada em `releases/economy-v4`. O remaster completo continua em desenvolvimento.

## Uso

- F2 abre um treino de combate baseado numa cópia em memória do primeiro cenário. F2 novamente volta ao cenário normal. A troca reinicia a partida de laboratório atual; salve antes se quiser preservá-la.
- Selecione o espadachim de barra verde e clique com o botão direito no de barra vermelha. O adversário reage ao ser atingido. Não existem ataques automáticos antes da primeira agressão.
- X interrompe suas ordens. Clique direito no chão manda mover. Arraste para selecionar um grupo.
- Espaço pausa; F5 salva; F9 carrega. Saves novos usam `research-v5.json`. O carregador aceita o formato v4; F9 procura o arquivo v4 na mesma pasta quando não existe v5. Não importa automaticamente saves de outra pasta de release.
- Construção, formação de trabalhadores e coleta continuam com os controles da v4: B, G/H, C e R.

O treino adiciona um único adversário em memória e reposiciona o espadachim numa área livre. Usa identidade de mapa/hash própria, para que seu save não seja confundido com a missão original. Não escreve nos ETA, saves ou executáveis antigos. PgUp/PgDn sai do treino e navega pelos cenários importados.

## O que foi integrado

Espadachins recebem ordens explícitas contra unidades humanas visíveis de outro jogador (0–3). Aproximam-se, atacam, aguardam recarga e mostram a animação original de golpe. Outro espadachim atingido revida. Barras de vida e um breve indicador branco mostram o dano. Ao chegar a zero, a unidade deixa de agir e de ser desenhada. Grupos podem atacar o mesmo alvo; papéis sem ataque implementado não recebem essa ordem.

Uma obra paga fica pausada quando seu construtor é derrotado. Carga de alimento de um trabalhador derrotado continua registrada naquele ator, sem movimentação ou entrega. **A recuperação dessa carga ainda não foi implementada**; retê-la é uma decisão provisória para preservar a contabilidade, não uma regra comprovada do original.

## Evidências reproduzíveis

Origem: cópia `working/RISING/RISING-ORIG.EXE`, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. Valores de configuração vêm das seções do INI, com linhas e offsets preservados em `source_remake/Data/units.json`.

| Conclusão | Evidência | Certeza |
|---|---|---|
| Matriz de dano indexada por tipo atacante/alvo | `EnginGetDegat` VA `0x44f98a`; ponteiro `0x4e199c`; 702 leituras de 26 linhas × 27 colunas | CONFIRMADO no getter com tabela controlada |
| Dano entre unidades incorpora bytes de ataque e defesa | `EnginInfligeDegat` VA `0x4502f0`; soma em `0x4503de`, defesa em `0x45059c`; campos atacante+247 e vítima+246 | CONFIRMADO na rotina isolada |
| Dano intermediário é byte com sinal; negativo vira 1, zero permanece zero | `0x4505c8–0x4505d3`, offsets de arquivo `0x4f9c8–0x4f9d3` | CONFIRMADO por desassemblagem e emulação |
| Flag no byte+220, bit 3, reduz dano depois do ajuste anterior | `0x450655–0x450675`; caso limite pode resultar em dano −1 | CONFIRMADO na aritmética; significado da flag DESCONHECIDO |
| Vida é palavra em unidade+154; dano letal zera vida antes de chamar morte | `0x450682`, `0x450754–0x4507aa` | CONFIRMADO; efeitos completos da morte não emulados |
| Bônus 5 soma ataque; bônus 6 subtrai na defesa | chamadas `BonusGetValEngin` em `0x4503f4` e `0x4505bd` | CONFIRMADO como chamadas; domínio dos bônus não reconstruído |
| A consulta do bônus 6 usa dono/tipo do atacante nesse caminho | registradores preparados a partir de EBP em `0x4505b3–0x4505bd` | FORTE EVIDÊNCIA estática; não reinterpretar automaticamente como bônus da vítima |
| Presença consultada de capitão pode somar byte global `0x4965ac` | `0x450422–0x45043b` | CONFIRMADO com presença controlada; busca espacial não reconstruída |
| Recarga aumenta contador+258 enquanto menor que tabela menos bônus 3 | `EnginRechargeArme` VA `0x4516d3`, tabela `0x4965de + tipo*50` | CONFIRMADO em 416 casos controlados |
| Espadachim possui 10 fases de ataque e atraso 2 no registro amostrado | ETA MONDE01, registro do tipo 0; campos em memória+166/+168; rotina `AffAttaqueEngin` VA `0x41374c` | CONFIRMADO para a amostra; frequência global ainda não estabelecida |

`tools/trace_combat.py` executa 702 consultas de dano, 1.426 casos de aplicação de dano e 416 casos de recarga: 2.544 comparações, sem divergências. Os 1.426 vetores são exportados e também conferidos contra o cálculo C#.

Os testes de dano cobrem todos os pares dos 26 tipos com vida 1 e 100, além de bônus, defesa, capitão, zero e estouros de byte. Configuração é carregada explicitamente em tabelas controladas. Áudio/visibilidade, mensagens de IA, experiência, projéteis, morte e simpatia são interceptados e declarados no relatório. A morte é contada, mas seus efeitos subsequentes não são executados. Não se trata de uma partida original emulada por completo.

`tools/trace_swordsman_attack.py` captura 160 conjuntos de comandos gráficos do original: 16 direções × 10 fases, incluindo sombra e corpo. Os IDs continuam na convenção original de base 1. Registro exportado em `source_remake/Data/swordsman_attack.json`.

## Limites de fidelidade

**EXPERIMENTAL:** perseguição, BFS, proximidade por distância de até uma célula em cada eixo, ordem de processamento por slot, seleção de alvo, retaliação, instante do dano e relógio de 30 Hz. O valor de recarga 20 vem do INI; a equivalência de um tick moderno a uma atualização do original não foi demonstrada. A sequência gráfica é original, mas sua sincronização com impacto ainda é provisória.

**PROVÁVEL, não equivalência completa:** aplicação de BLINDAGE do INI como defesa base no controlador moderno. O original usa um campo da instância; atualizações de experiência, efeitos e tecnologias desse campo ainda não foram reconstruídas. A aritmética foi comprovada para instâncias controladas.

**PENDENTE:** arqueiros/projéteis, outros atacantes, alvos grandes como ursos e veículos, combate contra prédios, diplomacia original, capitães e tecnologias em jogo, IA de missão, morte animada, efeitos sonoros, fog of war e condições de vitória/derrota. A integração usa bônus zero e não ativa a flag especial de redução; o cálculo isolado suporta esses vetores para pesquisa.

O treino é uma ferramenta jogável de validação, não uma missão recuperada. Nenhum resultado desta etapa comprova campanha completa ou compatibilidade em todos os Windows 10/11.

## Verificação e continuidade

`--self-test` cobre a economia anterior, os vetores x86, perseguição, retaliação, intervalo entre golpes, derrota, cancelamento, saves durante/depois do combate, rejeição atômica de saves inválidos, migração v4 e interação com coleta/construção. `--capture-combat --capture <PNG>` inicia automaticamente o treino e registra uma imagem do executável moderno.

Resultados: `reports/combat_original_validation.json`, `reports/combat_v5_validation.json`, `logs/selftest_combat_v5_published.log`, `reports/prototype_combat_v5_final.png`.

Próxima prioridade: reconstruir a máquina de estados de ataque/recarga e o instante de impacto; depois projéteis, defesa da base, IA e objetivos das missões. Manter os testes da economia e de continuidade do save em todas as alterações.
