# v75 — adversário jogável da missão 4

O Clã Vermelho passa a agir na missão 4. Esta versão implementa uma IA funcional sobre os dados originais. **A política de decisão ainda não é uma tradução completa da IA original.**

## Implementado

- Estado inicial importado de MONDE04.ETA: 29 registros de personagens do jogador 1, incluindo moradores e o passageiro oculto, quatro construções e estoques separados. Nenhuma tropa extra é criada na abertura.
- Exploração própria e aquisição de alvos em sua visão. Parte das tropas guarda a região inicial e parte explora. Unidades do jogador defendem-se automaticamente quando paradas, sem substituir ordens de movimento.
- Trabalhadores exploram a região próxima, coletam fontes descobertas e entregam no armazém de seu proprietário. Um fazendeiro abastece o cultivo; colonos excedentes podem ocupar o quartel.
- Treinamento de classes militares já presentes no estado original da missão, respeitando produtor, colonos, preço e estoque próprios. O último morador é preservado.
- Expansão limitada de armazéns quando falta capacidade, usando construtor, terreno e custos reais; ocupação posterior por colono. Não constrói qualquer prédio ou tecnologia arbitrariamente.
- Ataque a tropas e prédios, reação a invasores e interrupção da hostilidade pela aliança. Mensageiro protegido contra ataques nessa missão.
- Clique de tropas militares numa construção inimiga inicia ataque. Dano acumulado, destruição, moradores derrotados, passagem liberada e ausência do prédio são preservados ao carregar.
- Missão 4: mercado próprio concluído e ocupado, mais aliança com o vermelho ou ausência de seus prédios e unidades visíveis. Mantém a avaliação por 100 atualizações e abre a transição para a missão 5. A regra detalhada dos eventos nativos ainda precisa ser confrontada com essa interpretação do objetivo.
- Cores originais dos jogadores aplicadas às camadas de unidades e às chamadas gráficas de cor de jogador. Somente a faixa de cores da tribo muda.
- Save v32: memória da exploração inimiga, decisões de patrulha, histórico de depósitos, propriedade de construções/recrutamento e cerco. Cinco slots v32; leitura dos antigos, sem sobrescrever arquivos de versões anteriores.

## Evidências

| Certeza | Origem/região | Método e conclusão |
|---|---|---|
| CONFIRMADO | MONDE04.ETA, recursos em 327684, prédios em 327732, unidades em 328766 | Leitura estruturada e comparação com JSON extraído: jogador 1 inicia com 45 alimentos, 50 pedras, zero peças, quatro prédios e 29 registros de unidades. |
| CONFIRMADO | MONDE04.TXT | Objetivo: construir mercado, formar mensageiro para aliança com o vermelho ou destruir o clã se não houver aliança. |
| CONFIRMADO | RISING-ORIG.EXE, iaCalculPlayer 0x47f2ef | Cinco sondagens com kernels interceptados: ordem unidades → cidade → recursos → guerra → solver; retorno não zero interrompe a sequência. Isso **não** prova as decisões usadas no remake. |
| CONFIRMADO | PE, tabela 0x495d90 + tipo × 52; BatimentGetDegat 0x41a9e0 | Limites padrão de dano: santuário 600, quartel 400, armazém e cultivo 100. Tabela completa no relatório nativo. |
| CONFIRMADO | RISING.INI, DEGAT, último índice 26 | Campo de dano contra edifícios, distinguido dos danos contra personagens. |
| CONFIRMADO | PE, tabela 0x496cfc; inicialização em 0x45fdcb | Remapeamento de oito índices de cor de jogador, preservando os outros pixels do sprite. |
| DESCONHECIDO | MONDE04.ETA, 360240–360252 | Um registro inicial de ataque de oito bytes, `010210001700ffff`; IALoadAttaqueIni 0x47bedd confirma cópia para registro de dez bytes. O significado dos campos ainda não foi provado; não foram tratados como coordenadas. |
| DESCONHECIDO | RISING.RUL; ghost buildings da missão | Programas, prioridades e planos de construção ainda não traduzidos integralmente. |

Executable copiado analisado: SHA-256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`.

Sondagem reproduzível: `tools/trace_opponent_v75.py`; saída `reports/opponent-native-v75.json`. Não inicia o executável original nem DirectDraw; rotinas de decisão são interceptadas na emulação.

## Limites que permanecem

Patrulha, divisão entre guardas e exploradores, busca local, expansão, composição de treinamento e cadência de decisões são políticas provisórias do remake. Tecnologias e pesquisas próprias da IA original não foram decodificadas: a IA usa valores base para seus trabalhadores e só treina classes já comprovadas no mapa. A diplomacia continua usando a avaliação provisória já documentada na v71. Não há reprodução integral dos eventos/ataques programados, iniciativas diplomáticas, habilidades especiais ou prioridades de guerra do original.

O cerco usa valores originais e a animação direcional de ataque, mas impactos de projéteis em edifícios, fogo, etapas de ruína e efeitos da destruição ainda precisam de reconstrução. O prédio desaparece quando atinge seu limite de dano. A eliminação de passageiros ocultos e o fim exato do clã exigem revisão adicional dos eventos nativos. Nenhuma dessas aproximações é apresentada como fidelidade comprovada.

## Validação e uso

28 verificações específicas do adversário, mais testes de cinco slots, passaram na versão publicada. Cobrem coleta física, treinamento pago, propriedades, construção inimiga, visão, defesa, aliança, imunidade do mensageiro, ataque/destruição de prédio, estados de missão 4, transição, rejeição de estoque falsificado e continuação determinística depois de salvar/carregar. Uma simulação prolongada também permaneceu carregável.

Baterias anteriores aprovadas: geral, v71 (96), v72 (86), cultivo v61 e gameplay v70 (42). Saves reais do jogador foram inspecionados somente para leitura e continuaram carregáveis. Captura gráfica conferida: `reports/v75-clan-vermelho-final.png`.

Executar `releases/interactions-v75/Jogar-Quarta-Missao.cmd`. O adversário está no sudeste do mapa. O menu de testes pode revelar o mapa para inspeção; a IA mantém sua própria visão mesmo com essa opção ligada.

Originais em D:\RISING e saves originais não foram modificados. Código, ferramentas, resultados e publicação ficam na área separada de pesquisa. O manifesto registra os hashes dos arquivos da entrega.
