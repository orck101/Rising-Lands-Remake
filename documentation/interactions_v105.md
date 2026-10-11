# v105 — planejamento situacional dos três clãs

Entrega em 2026-10-04. Política do remake solicitada pelo usuário; não é uma reconstrução comprovada do algoritmo original.

## Integração

Os perfis v104 permanecem: vermelho Exército sustentado (Agricultura/Armamento), amarelo Fortaleza produtiva (Agricultura/Engenharia), verde Indústria militar (Engenharia/Armamento). O planejador usa população, prédios operados, renda observada, manutenção, reservas, fontes descobertas, caminhos seguros, tecnologias liberadas, invasão e perdas. Não concede recursos, colonos ou pesquisas.

- A abertura limita o crescimento de trabalhadores enquanto não existir cultivo operado e população suficiente. Preserva operadores essenciais em vez de consumir todos os colonos na formação de fazendeiros.
- Distribui colonos entre cultivo, santuário, armazéns, produtores e laboratório. Alimentação do cultivo usa um fornecedor consistente entre os dois ciclos de coleta; elimina ordens conflitantes de entrega.
- Recupera profissões essenciais com recrutamento normal. Quando falta população, pode reconverter trabalhadores excedentes ou combatentes pelo santuário, caminhando e pagando custos normais para o recrutamento subsequente. Não recupera uma base sem sobreviventes por mágica.
- Metas de fazendeiros derivam da produção e manutenção existentes; metas de construtores e engenheiros consideram serviços, excedente e necessidades de pedra/engrenagens. Se falta pedra, o segundo construtor tem prioridade sobre a expansão continuada de fazendeiros.
- Planeja serviços essenciais, armazenamento aos 70%, laboratório quando há pesquisa possível, mercado e Bolsa após população/defesa mínimas. Mantém os prédios de apoio prioritários de cada personalidade e todos os requisitos de tecnologia/custo.
- Laboratório recebe pesquisadores conforme disponibilidade de pesquisa; não exige dois colonos sem uma pesquisa disponível. Conserva os tempos e o limite de duas pesquisas existentes para os inimigos.
- Exploração procura setores ao redor da base e respeita caminhos seguros. Construtores sem pedra conhecida acessível podem ampliar a busca de 48 para 96 quadrados após cinco minutos. Isso não revela minas automaticamente nem permite atravessar terreno bloqueado ou ameaças conhecidas.
- Regiões acessíveis são separadas por profissão e dono, respeitando ameaças específicas de cada clã. O raio ampliado é calculado uma vez por trabalhador/atualização, evitando milhares de decisões repetidas.
- Construtores livres reparam prédios próprios danificados em locais seguros. Não interrompe uma obra nem uma entrada reservada para reparar.
- Produção mantém o ciclo da personalidade; pode priorizar arqueiro legal diante de aeronave hostil realmente vista, ou um capitão legal em um grupo de seis combatentes sem capitão. Não desbloqueia unidades.
- Mantém defesa antes da ofensiva, reserva econômica, recuperação após perdas e agressividade crescente pela campanha da política comum. Preserva a abertura comercial e a pressão especial v102 da missão 4.

## Correções associadas

Um trabalhador caminhando para entrar num prédio não pode receber simultaneamente uma obra ou tarefa econômica da IA. Reutilização de colono anteriormente treinado considera apenas recrutamentos ainda ativos. A validação de uma obra concluída preserva a identidade do trabalhador histórico sem exigir que ele continue sendo construtor após reconversão; obras em andamento continuam exigindo construtores válidos.

DecisionFor expõe personalidade, prioridade, motivo, metas de mão de obra, reservas, próxima obra e prontidão econômica para ofensiva. A decisão é derivada do estado existente, sem alterar o formato 34 dos saves. O monitor visual não foi modificado nesta entrega.

## Verificações

- Compilação e publicação sem erros.
- 12 verificações do planejador: abertura dos três clãs, ausência de concessões, limites de trabalhadores, progressão da campanha, restauração exata e continuação determinística.
- 40 verificações de personalidades e persistência nas missões 4/5/7.
- 28 verificações de pressão da missão 4, incluindo preservação das restrições econômicas da missão 5.
- Simulação natural da missão 5 do início até 60 minutos, com restauração exata nos checkpoints. Nos primeiros 30 minutos foi identificado e corrigido conflito entre reconversão e construção. A priorização final de pedra foi testada em continuação posterior do mesmo cenário.
- Ensaio isolado: todos os construtores do verde foram eliminados de um snapshot v103 de uma hora. Em dez minutos houve reposição por recrutamento real, com cinco checkpoints save/load exatos. O snapshot fonte não foi alterado.

Ao final de 60 minutos, antes do último ajuste de prioridade de pedra: vermelho 3 prédios, 87 alimentos/13 pedras; amarelo 8 prédios, Agricultura de 2 para 3; verde 9 prédios, Armamento de 2 para 4. Não tratar contagem total de recrutamentos como contagem de soldados: inclui trabalhadores. O vermelho ainda cresceu mais devagar por logística de pedra, o que motivou o ajuste final.

## Limites

Simulações pelo código, sem uma partida jogada pela interface. Não é aprovação do balanceamento de todas as missões nem garantia de resposta perfeita em todo mapa. Personalidades usam preferências com alternativas legais; capacidade inicial pode impedir a unidade preferida. Saves conservam o ramo de pesquisa em andamento. Vitória da missão 5 e problemas de gameplay externos à IA não foram implementados neste pedido. O intérprete nativo experimental continua separado do planejador jogável.

## Arquivos e pacote

Engine/OpponentPlanning.cs, OpponentDevelopment.cs, OpponentSimulation.cs, OpponentBasicPolicy.cs, OpponentSafety.cs, OpponentWarehouses.cs, BuildingPopulation.cs, ConstructionSimulation.cs; Tests/OpponentPlanningTests.cs, MissionFiveAudit.cs; Program.cs.

Pacote: releases/interactions-v105. Evidências: experiments/inteligencia-v105. Compilações temporárias desta rodada foram removidas após falta de espaço; logs, snapshots e pacote permanecem. Originais, UserSaves e versões já entregues não foram alterados.

## Continuação final e limite encontrado
A build publicada final passou novamente 12+40+28 verificações. Continuação de 60 a 75 minutos, quatro checkpoints exatos: amarelo continuou treinando e verde passou de 9 para 10 prédios e de 20 para 23 recrutamentos totais. Vermelho preservou 87 alimentos/13 pedras e três prédios; possui dois construtores, mas a logística de pedra continuou lenta/bloqueada nesse cenário. Portanto a nova inteligência está integrada, mas esse gargalo específico de movimentação/coleta do vermelho não está resolvido e não deve ser descrito como IA plenamente aprovada. O ensaio não contou ofensivas completas entre checkpoints; não prova que cada clã atacou.
DLL publicada SHA256: C81A91EAC2C06AF2A21E3165DBE074E0412DE925EB79FF5DBC5272356C8344B1.

