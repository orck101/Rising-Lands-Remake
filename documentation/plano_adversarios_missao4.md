> Andamento v75: adversário funcional da missão 4 publicado. O plano de fidelidade integral continua aberto; consultar [implementação e limites](interactions_v75.md).

# Plano de análise e implantação dos adversários — Rising Lands

Data: 02/10/2026. Base: remake v74. Escopo inicial: missão 4; expansão para a campanha somente após validação dessa missão.

## Objetivo e critério de fidelidade

Reconstruir as decisões dos adversários, sua economia, combate, diplomacia, eventos e condições de missão usando as regras do original. Não basta criar inimigos que atacam: o adversário deve produzir, escolher, esperar e reagir como o original nos mesmos estados.

“Exatamente igual” é um objetivo verificável, não uma conclusão antecipada. Quando faltarem evidências, registrar DESCONHECIDO; não preencher com uma regra genérica apresentada como original. Toda aproximação necessária para experimentação fica identificada e separada do perfil de fidelidade.

Cada regra terá: arquivo e hash; offset/endereço; estado de entrada; método de análise; decisão e efeitos; grau de certeza; teste de comparação; limitações. Graus: CONFIRMADO, FORTE EVIDÊNCIA, PROVÁVEL, HIPÓTESE, DESCONHECIDO. Nomes de funções comprovam sua existência, não seus algoritmos.

## Diagnóstico inicial confirmado

- `CD/WORLD/MONDE04.TXT` pede construir mercado, formar mensageiro e estabelecer aliança com o Clã Vermelho; destruir o rival se a aliança não for possível. Isso não define sozinho a expressão exata de vitória.
- `MONDE04.ETA`: mapa 128×128. A extração atual contém 40 registros de unidades e cinco construções: jogador 0 com 11 unidades e um prédio; jogador 1 com 29 unidades e quatro prédios. Parte das unidades pode estar oculta ou residente. Registros não equivalem a unidades visíveis.
- Recursos iniciais preservados em `MONDE04.json`: ordem dos recursos e dos quatro jogadores precisa ser respeitada, sem confundir total de rede com estoque de cada prédio.
- `full_structure.json` identifica ataques iniciais, construções fantasma, regiões de influência, final de missão e objetivos. Identificação das seções não comprova a semântica integral dos registros.
- Há `RISING.RUL`, binário com assinatura RULE, e símbolos para carregamento de regras, solução de problemas, cálculo por jogador e alianças.
- O remake tem atores de outros jogadores, combate, retaliação e partes de diplomacia. Ainda não possui o ciclo estratégico completo. Construção, recrutamento, pesquisas e diversas validações assumem jogador 0.
- A composição de ofertas recebidas e a decisão de aliança atuais são aproximações documentadas. Há divergência a investigar na proteção do mensageiro: o relato do usuário diz que não é atacado; o remake ainda admite ataque explícito.
- O combate atual mostrado em `CombatSimulation.cs` mira unidades. Ataques a construções e a definição de “clã destruído” precisam ser auditados antes de viabilizar a alternativa militar da missão 4.

Fontes existentes: `documentation/missions/monde04.md`, `documentation/events.md`, `documentation/commerce_original_audit.md`, `documentation/interactions_v71.md`, `source_remake/Data/Maps/MONDE04.json`, `extracted/maps/MONDE04/full_structure.json`, `reverse_engineering/disassembly/RISING-ORIG.symbols.asm`.

## Etapa 1 — estabelecer a referência da missão 4

Trabalhar exclusivamente em cópias da instalação e em saves experimentais separados. Não alterar saves do usuário. Registrar configuração, dificuldade se houver, tecnologia herdada, diplomacia herdada, velocidade, ordem das atualizações e estado aleatório.

Preparar sessões originais com: jogador parado; aproximação pacífica; entrada em visão; ataque a trabalhador; ataque a soldado; ataque a prédio; mensageiro no santuário; mensageiro no mercado; falta de alimento; mina esgotada; passagem bloqueada. Separar adversário autônomo de resposta disparada pelo jogador e de evento da missão.

Gravar vídeo e checkpoints antes/depois de decisões, com relógio da simulação e tempo real. Repetir ensaios idênticos para descobrir quais ações são determinísticas e quais dependem de aleatoriedade, IDs ou desempates. Sessões aceleradas só após medir a relação entre velocidade e tempo lógico.

Entregáveis: manifesto da sessão, cenário inicial por jogador, cronologia e lista de comportamentos observados. Saída: referência reproduzível, sem exigir que uma única partida represente todo o comportamento da IA.

## Etapa 2 — decodificar regras e estado da IA

Priorizar a cadeia `iaProbLoadBinaryRules @ 0x47ddff`, `iaProbLoadTextRules @ 0x47d46c`, `iaSolver @ 0x47ccdc`, `iaCalculPlayer @ 0x47f2ef`, `IaLoad @ 0x47f534`, `IaSave @ 0x47f573`, `IaPropositionAlliance @ 0x47f5e3`, `IALoadAttaqueIni @ 0x47bedd`. Endereços vêm dos símbolos existentes; algoritmos continuam sujeitos a investigação.

Rastrear o carregamento de `RISING.RUL`: cabeçalho, tabelas, instruções, parâmetros, condições, saltos, ordem de avaliação, prioridades e chamadas ao motor. Investigar filas de problemas, suspensão/retomada, modo de sobrevivência, valores de configuração, mapas de influência e criação de ordens. Não assumir árvore de comportamento, pesos ou modelo de IA moderno.

Rastrear a execução de regras relevantes em emulação isolada e comparar com sessões completas. Uma função isolada pode omitir estado global ou dependências: validar chamadores e efeitos, não apenas retorno.

Se o original utilizar um interpretador, reconstruir sua semântica e importar as regras. Se utilizar lógica compilada, traduzir funções em módulos com testes de equivalência. Preservar bytes ainda desconhecidos em extrações; analisar os bytes finais do ETA antes de considerá-los irrelevantes.

Entregáveis: catálogo de regras, mapa de instruções/rotinas, estruturas de estado e ferramenta que explique qual regra levou a cada decisão. Saída: caminho demonstrado entre entrada, regra e ação, incluindo ordem e temporização.

## Etapa 3 — objetivos, gatilhos e roteiro da missão

Blocos da missão 4 a auditar:

| Seção | Offset decimal | Bytes | O que investigar |
|---|---:|---:|---|
| Ataques iniciais | 360244 | 8 | Códigos, jogadores, ligação com regras e ativação |
| Construções fantasma | 360254 | 1758 | Planejamento, alvos, posições reservadas ou outra função |
| Regiões de influência | 362016 | 660 | Território, ameaça, objetivo e prioridade |
| Fim de missão | 362680 | 100 | Vitória, derrota, alternativa diplomática/militar |
| Objetivos | 362796 | 1200 | Condições por jogador, flags e conclusão |

Os offsets descrevem seções do extrator; os nomes não comprovam seu funcionamento. Relacionar com `PlayerGetObjectif`, `LoadObjectif`, `FinMissionLoad`, consumidores e alterações observadas.

Determinar se mercado precisa estar atendido, se mensageiro precisa concluir a missão diplomática, se aliança encerra imediatamente a fase, se é possível vitória militar sem tentar aliança e quais entidades precisam ser eliminadas. Conferir fracasso, perda do mensageiro e efeitos herdados da fase 3.

Eventos cinematográficos, mensagens, áudio e cenas de aliança devem usar arquivos originais, somente quando seu vínculo for comprovado.

Entregável: expressão de vitória/derrota e grafo de eventos com evidência por condição. Saída: vitória nunca antecipada por iniciar construção, aproximar mensageiro ou destruir apenas uma unidade sem respaldo original.

## Etapa 4 — preparar uma simulação por jogador

Generalizar a camada de ordens para proprietário explícito, mantendo a interface humana restrita ao jogador controlado. Cada ação deve debitar o proprietário correto, validar sua tecnologia e usar seus prédios e trabalhadores.

Cobrir construção e ajudantes; reservas de posição; coleta e retirada; armazenamento e capacidade; colonos e cultivo; ocupação; treinamento; pesquisas; fome; feitiços; diplomacia e comércio. Confirmar quais regras de economia se aplicam efetivamente à IA antes de impor igualdade total ou conceder vantagens.

Adicionar proprietário às obras, filas e estados que hoje o omitem. Atualizar conservação, validações e migração de saves: registros antigos continuam do jogador 0; estados novos registram cada participante. Não permitir gasto da IA no estoque humano ou aceitação de propriedade forjada.

Separar visão por jogador, memória conhecida e informação disponível ao decisor. Investigar se a IA original possui conhecimento global ou informação especial; reproduzir o que for comprovado, em vez de presumir que ela usa a névoa do jogador humano.

Entregável: infraestrutura compartilhada de ações por proprietário. Saída: os mesmos testes econômicos e de ordens passam para jogadores diferentes sem interferência entre estoques e sem regressão nas missões 1–3.

## Etapa 5 — reconstruir a economia adversária

Investigar como escolhe fontes, depósitos, trabalhadores, estruturas, pesquisas e unidades. Medir limiares de reserva, filas, frequência de decisão, prioridades de sobrevivência e resposta à perda de unidades/prédios. Conferir extrações, crescimento de população, consumo e eventuais benefícios especiais da IA.

Cenários: fonte acaba; armazém lota; coletor morre; trabalhador fica bloqueado; reserva não permite produção; prédio perde colonos; área de construção fica ocupada; falta comida. Comparar tempo e decisão com o original, não “otimizar” o adversário para agir melhor.

Implantar primeiro somente as regras acionadas na missão 4. Outras classes e estruturas entram conforme evidências e missões posteriores, sem inventar desenvolvimento ilimitado.

Saída: adversário mantém seu ciclo econômico e produz a sequência original nos cenários de referência, sem recursos artificiais não comprovados.

## Etapa 6 — percepção, defesa, combate e destruição

Reconstruir detecção, escolha de alvo, prioridade de trabalhador/soldado/prédio, reação a dano, chamadas de ajuda, limite de perseguição, retorno ao posto, formação de grupos e condições de iniciar ataque. Não programar ondas periódicas ou agressividade constante sem confirmação.

Auditar alcance, dano, defesa, pesquisas por jogador, capitães, consumo, efeitos, munições e alvos aéreos/terrestres. Reproduzir caminhos e desempates quando afetarem decisões. A navegação atual é provisória; conservar ordem de vizinhos e decisões nativas se comprovadas. A regra aprovada no remake mantém helicóptero e balão sobre muralhas e portões.

Investigar e implementar dano a prédios, destruição, liberação de ocupantes, ruínas, liberação de colisão, suspensão de produção e destino dos recursos. Determinar consequências de perder santuário ou armazém e condição exata de derrota de cada clã.

Saída: defesa e ataque comparáveis ao original; mensageiro recebe o tratamento comprovado; alternativa militar da missão 4 é realmente concluível.

## Etapa 7 — diplomacia e comércio do Clã Vermelho

Reconstruir a decisão real de aceitar/recusar aliança, influência da simpatia, eventos que a modificam, necessidade de construções/colonos e consumo ou desaparecimento do mensageiro. Investigar rompimento, hostilidade posterior e persistência entre missões.

Substituir a composição provisória das ofertas recebidas e os tempos adaptados pelos cálculos e contadores nativos quando comprovados. Conferir transferências diretas de materiais, entrega de personagens na Bolsa, expiração e restituição. Ações comerciais precisam conviver com economia, combate e destruição dos prédios.

Saída: mesma decisão e mesmos efeitos para os casos de simpatia, recursos, aliança e oferta testados no original; caminhos diplomático e militar não se confundem.

## Etapa 8 — save/load, reprodução e desempenho

Salvar filas, estados das regras, ordens, objetivos, temporizadores, conhecimento, grupos, reservas, relações, referências a entidades e estado aleatório. Confirmar no original quais dados persistem e quais são recalculados. Um save no meio de decisão não pode criar tropas, duplicar gastos, reiniciar ataque ou mudar a diplomacia.

Comparar continuação em execução ininterrupta e após carregar. Migrar saves antigos sem declarar tecnologias inimigas ou memórias que nunca existiram.

Medir antes/depois com a missão 4 e cenário de estresse idênticos em 1x e 5x: tempo de simulação, desenho, regras, caminhos, percepção e influência. Evitar custo de cada unidade procurar todas as outras a cada atualização; usar índices e cache com invalidação correta. Nunca reduzir artificialmente a frequência de decisão para melhorar desempenho sem validar fidelidade.

Saída: mesmas decisões relevantes após save/load; resultados equivalentes entre velocidades quando o original também o permitir; custos medidos e ausência de pausas recorrentes atribuíveis à implantação.

## Etapa 9 — validação comparativa e fechamento da missão 4

Criar casos documentados para: jogador parado; fronteira de visão; aproximação pacífica; ataque e retaliação; alvo que foge; bloqueio de caminho; recurso esgotado; produção interrompida; fome; mensageiro; aliança aceita/recusada; oferta recebida; comércio expirado; destruição e morte; vitória diplomática; vitória militar; derrota; salvar/carregar durante cada fluxo.

Comparar estados discretos, consumos, decisões, prioridades, ordem, contadores e efeitos visuais/sonoros. Não escolher uma tolerância global arbitrária: equivalência exata para saídas determinísticas; tolerância visual/temporal apenas fundamentada na medição; testes por semente ou distribuição para aleatoriedade comprovada. Conservar IDs e critério de desempate quando relevantes.

Fazer sessões humanas completas dos dois caminhos da missão 4; depois regressão das missões 1–3. Preservar o controle de testes sem confundi-lo com critérios normais de vitória. Conferir a transição, pesquisas, diplomacia herdada e entrada da missão 5, sem considerá-la reconstruída apenas por carregar o mapa.

A missão 4 só será declarada fiel quando as decisões críticas tiverem evidência, os dois caminhos aplicáveis forem concluíveis, saves mantiverem o comportamento, e nenhuma condição essencial permanecer substituída por regra genérica. Registrar explicitamente qualquer diferença remanescente.

## Ordem das entregas

| Entrega | Conteúdo | Condição para avançar |
|---|---|---|
| A | Referência original, mapa por proprietário, regras e objetivos auditados | Fontes e casos reproduzíveis; lacunas identificadas |
| B | Ações por proprietário e estado salvo | Estoques, colonos, tecnologias e ordens isolados corretamente |
| C | Economia, percepção e defesa da missão 4 | Ciclo autônomo comparado com a referência |
| D | Combate contra prédios e decisão diplomática | Caminhos de guerra e aliança demonstrados |
| E | Missão 4 completa com transição e validação | Comparação, regressões, save/load e desempenho aprovados |

Essas são entregas verificáveis, não cinco estimativas iguais de duração. A investigação de RISING.RUL e dos eventos pode revelar dependências adicionais. Não fixar prazo de “fidelidade exata” antes dessas descobertas.

## Artefatos previstos

- Dossiê inicial da missão e inventário de entidades/tecnologias por jogador.
- Especificação do formato RUL e catálogo de funções/instruções com evidências.
- Cronologia, grafo de eventos e regras de vitória/derrota.
- Ferramentas de captura, comparação e explicação de decisões.
- Matriz comportamento × evidência × implementação × teste × diferença.
- Relatórios de conservação, saves, desempenho e sessões completas.
- Versões jogáveis separadas, preservando a v74 e os arquivos originais.

## Próximo trabalho concreto

Auditar `MONDE04.ETA` junto do carregador de `RISING.RUL`, identificar as regras efetivamente atribuídas ao jogador 1 e reconstruir as condições de aliança/vitória. Isso define o comportamento que a infraestrutura por jogador precisa suportar. Não iniciar por uma IA genérica de patrulha/ataques que depois teria de ser substituída.
