# v46 — portas, coleta próxima, câmera e confirmação de save

Abrir `releases/interactions-v46/Jogar-Primeira-Missao.cmd`.

## Mudanças

1. Slot ocupado abre confirmação com Cancelar em primeiro lugar. Nada é gravado antes de Substituir partida salva. Esc cancela; slot vazio salva diretamente. Salvar e sair respeita a confirmação e só fecha depois da gravação. Saves do remake continuam em `UserSaves`, cinco por mapa. Saves legados são apenas lidos como alternativa do slot 1.
2. Saída calibrada na porta visível dos sprites completos, com três posições em uma linha: centro, esquerda e direita. A rotina prefere posições livres e permite sobreposição somente depois que as posições transitáveis ficam ocupadas. Retirada de colonos, formação de trabalhadores/soldados e deslocamento entre prédios usam a mesma referência de porta. Não há seleção automática. Personagens no corredor são desenhados à frente do prédio para não ficarem encobertos.
3. Construtor caminha pela frente da mina até o ponto visual da entrada, ainda visível. Somente depois inicia a extração e desaparece. Reaparece na entrada com a carga; ordem de entrega retoma o caminho normalmente. Não pode despejar cargas dentro do espaço da jazida. Ordens no mesmo quadrado agora também concluem o deslocamento quando a unidade começa fora da grade, como na porta.
4. Fazendeiro continua em fontes de comida próximas quando a fonte acaba, incluindo pilhas e plantas, sem trocar o destino escolhido. Busca limitada à área de seis quadrados de distância Manhattan em torno da fonte originalmente indicada; prioriza distância e tenta alternativas acessíveis. Não recolhe a pilha usada como seu próprio destino. Ordens iniciais de grupos ainda distribuem trabalhadores por plantas disponíveis. Sem comida válida, para; sem acesso, espera e tenta novamente. Carga é preservada. Novas ordens em pilhas também guardam a área no save. Ordens muito antigas que não guardam área mantêm o comportamento anterior até receberem nova indicação de fonte.
5. Borda de câmera ampliada de 12 para 36 pixels, intensidade crescente conforme a proximidade da extremidade, aceleração e desaceleração suaves. Mantidos diagonais, WASD, setas, zoom e limites do mapa. Movimento usa tempo real da interface e não a velocidade 1x–5x da simulação. Perda de foco/menu interrompe a navegação.

## Evidências e grau de certeza

- **CONFIRMADO:** imagens reutilizadas dos bancos nativos, coordenadas de desenho em `source_remake/Data/construction_rules.json` e `Scenes/MONDE01.json`, cenário copiado `working/RISING/CD/WORLD/MONDE01.ETA`. Atlases `reports/v46-door-atlas.png` e `v46-door-atlas-rest.png` permitem conferir as portas de cada prédio.
- **FORTE EVIDÊNCIA VISUAL:** pontos das portas calibrados sobre as imagens nativas completas. Valores relativos em `Engine/BuildingExits.cs`, vetor `DoorFeet`; a posição da unidade considera a referência visual já usada de pés (+12,+24 pixels). Captura `v46-front-exit.png` mostra três colonos na linha em frente ao santuário.
- **FORTE EVIDÊNCIA VISUAL:** a primeira mina ocupa a célula 2091 (x=43,y=8), conforme a tabela de minas do ETA. `v46-mine-context.png` mostra o portal no terreno original. O construtor chega à origem dessa célula mais (+17,+9) pixels antes de desaparecer; `v46-worker-stone.png` mostra sua saída carregado. Mesma geometria aplicada às demais minas desse formato.
- **ADAPTAÇÃO DO REMAKE:** linha de três saídas, corredor de navegação, raio de seis quadrados e suavização de câmera são regras desta implementação para atender ao comportamento solicitado; não constituem comprovação dos algoritmos internos originais. Ajustes finos de posição de outros prédios e sensação da câmera podem precisar de comparação jogável.

## Verificação

- 544 verificações da primeira missão: todas passaram; inclui execução completa até a vitória, economia, extração, formação, construção e persistência.
- 351 verificações gerais: todas passaram.
- Captura de confirmação verifica slot vazio, abertura sem gravar, Cancelar, Esc e substituição confirmada em pasta de teste separada; não utiliza saves do jogador.
- Capturas finais: `v46-overwrite.png`, `v46-front-exit.png`, `v46-worker-stone.png`.
- Testes antigos de transferência de uma única pilha usam fontes isoladas para continuar medindo conservação exata. Um teste específico novo verifica a transferência de fontes vizinhas e a preservação do destino.
- Corredores de porta armazenados em cache, invalidados ao concluir prédios ou carregar; teste de alocação da navegação continua abaixo do limite existente de 500 KB por dez caminhos curtos.

## Preservação e arquivos alterados

Nenhum arquivo em D:/RISING ou save original foi alterado. Código alterado somente no remake: BuildingExits, BuildingPopulation, RecruitmentSimulation, ResearchSimulation, Simulation, ConstructionSimulation, FoodSimulation, MiningSimulation, EdgeCamera, PauseMenu, ResearchGame e comentário de Actor.FoodField. Testes ajustados para a nova semântica e acrescentado ImprovementV46Tests. Publicação separada v46, documentação, logs e imagens de evidência. Save permanece v17, com leitura de versões 4–17.

## Conferência jogável recomendada

Retirar vários colonos do santuário e conferir a linha e a sobreposição apenas quando cheia; formar uma unidade e movê-la para fora da porta; coletar pedra e acompanhar a aproximação à mina; mandar comida para o chão e esgotar uma fonte com outra próxima; salvar em slot ocupado e cancelar/substituir; experimentar a câmera nas bordas e cantos. A sensação da câmera ainda depende dessa avaliação humana.
