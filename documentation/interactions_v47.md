# v47 — aproximação à arte dos prédios e engenheiro carregado

Abrir `releases/interactions-v47/Jogar-Primeira-Missao.cmd`.

## Comportamento implementado

- Entrega de comida, pedra e engrenagens no armazém passa a usar a faixa da entrada visível, em vez do perímetro completo da área reservada. O personagem termina a aproximação antes de creditar os recursos. A carga permanece com ele até esse momento.
- Retirada de recursos do armazém usa a mesma faixa. Estoque só é descontado após o trabalhador chegar ao ponto e terminar a coleta.
- Entrega ao cultivo, entrada de colonos, chegada ao laboratório, retorno ao santuário e chegada para treinamento no quartel usam pontos junto à arte. Reservas de capacidade, propriedade e regras de cada ação continuam valendo.
- Saída/nascimento de personagens foi deslocado 24 pixels para baixo da porta (três quartos de um quadrado de 32 pixels). Linha de três posições mantida; posições livres são preferidas antes de sobrepor unidades. Não há seleção automática.
- Engenheiro tipo 6 usa a arte original de carga com engrenagens: todas as 16 direções e 8 fases de movimento. Sem carga, retorna aos frames originais normais.

## Causa dos problemas

Entrega/retirada/entrada ainda usavam `WorkCells`, que enumera o perímetro da caixa reservada à construção. Essa caixa é maior que a imagem do prédio. A saída v46 já tinha referência visual de porta, mas colocava os pés diretamente nela. A seleção gráfica de trabalhadores carregados contemplava somente tipos 5 e 7, e o catálogo extraído ainda não incluía o tipo 6 carregado.

## Evidência do engenheiro

**CONFIRMADO:** extração da rotina original de desenho `0x415a27` usando registros controlados, sem iniciar o executável original. Tipo 6, recurso 0 (engrenagem), direção 1–16, fase 0–7. Quantidades em `0x0a` e `0x70`; tipo do recurso em `0x10`. As 128 chamadas com carga diferem das chamadas sem carga e possuem sprite de unidade.

Ferramenta reproduzível: `tools/export_engineer_visuals.py`. Evidência com chamadas e sprites por direção/fase: `reverse_engineering/engineer_cargo_v47.json`. Catálogo de uso: `source_remake/Data/worker_visuals.json`.

**FORTE EVIDÊNCIA VISUAL:** aproximação usa as portas calibradas nos sprites nativos completos, documentadas na v46. As capturas v47 mostram o resultado no santuário e no armazém da primeira missão. A distância externa de 24 pixels é uma adaptação pedida pelo jogador; não é apresentada como algoritmo comprovado do original. Os outros prédios compartilham o mecanismo, mas ainda podem precisar de ajuste visual específico.

## Verificação

575 verificações da primeira missão e 351 gerais passaram. A primeira missão continua completável. Incluem coleta/entrega dos três recursos, retirada do armazém, crédito/débito somente na posição de interação, saída e entrada do colono, formação, pesquisa, capacidade, conservação de recursos e save/load.

O teste de arte carregada passou a incluir o engenheiro em todas as direções e fases, verificando que os comandos diferem dos comandos sem carga. Capturas conferidas: `reports/v47-arrival-food.png`, `v47-arrival-stone.png`, `v47-arrival-engineer.png`, `v47-front-exit.png`.

A captura do engenheiro cria uma unidade e carga controladas somente em memória, para ilustrar a aparência. Os testes de transferência utilizam recursos efetivamente coletados na simulação.

## Compatibilidade e preservação

Save do remake permanece v17, com leitura de v4–v17. Posições fracionárias de aproximação são persistidas nos campos existentes de posição. Validação das viagens reconhece os pontos visuais e os antigos pontos de perímetro. Ordens já salvas em versões anteriores podem completar no ponto antigo; uma nova indicação de entrega, retirada ou entrada usa o novo ponto. Saves em disco do jogador e arquivos em D:/RISING não foram alterados.

Mudanças somente no remake: BuildingExits, BuildingPopulation, RecruitmentSimulation, ResearchSimulation, FoodSimulation, MiningSimulation, WarehouseTransfer, VisualCatalog e captura ResearchGame/InteractionV47Capture. Adicionados InteractionV47Tests e export_engineer_visuals; ampliado WorkerVisualTests e incluído no conjunto MissionOneTests. Catálogo worker_visuals atualizado com a extração original. Nova publicação separada v47.

## Conferência recomendada

Comparar a entrega dos três trabalhadores na arte do armazém; retirar recursos com Ctrl e mandá-los ao chão; observar a troca de aparência do engenheiro com/sem carga; retirar colonos do santuário e formá-los em tropas, conferindo a posição abaixo da porta; entrar no cultivo/laboratório/quartel conforme as estruturas e tecnologias disponíveis. Ao carregar uma ordem antiga, indicar novamente o destino para utilizar imediatamente a aproximação visual.
