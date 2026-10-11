# v72 — fome das tropas e portões sobre muralhas

Entrega de 02/10/2026. Jogável em `releases/interactions-v72`, com atalhos para as três primeiras missões.

## Fome

A causa foi `ActiveForFatigue`: somente ordens de movimento, coleta, mineração, ataque ou obra avançavam a reserva. Uma tropa militar parada permanecia alimentada indefinidamente. A v72 inclui as classes vivas de combate 0, 1, 2, 3, 4, 8 e 25 mesmo sem ordens. Unidades ocultas dentro de prédios e unidades petrificadas continuam sob as restrições existentes.

Ao esgotar a reserva, a unidade busca uma pilha de comida no chão e, na ausência de uma acessível, um armazém seu atendido. O consumo desconta uma unidade e entra no registro de conservação. Rotas e tarefas permanecem preservadas. Uma tropa em guarda registra a posição e retorna depois da refeição. Sem comida, aplica dano gradual e pode morrer. O indicador usa os sprites originais já extraídos.

**CONFIRMADO:** classificação original `TypeEnginSeFatigue` em `0x442cd8`; período base 3000 e dano 20 em `CalculFatigue`, `0x452cc8`, além dos bônus agrícolas extraídos. Tipos mecânicos 11–24 não recebem fome artificial. Ver `tools/trace_hunger_v67.py` e `source_remake/Data/hunger_rules.json`.

**Limite:** a cadência integral dos chamadores nativos, especialmente espera, não foi totalmente reconstruída. A continuidade da fome em tropas ociosas implementa a demanda aprovada. Na simulação atual de 30 atualizações/s, 3000 atualizações correspondem a 100 segundos por unidade da reserva, antes de bônus e aceleração; quatro unidades iniciais correspondem a aproximadamente 6m40s em 1x até procurar uma refeição.

## Portão

Duas causas: o catálogo tinha `Objects=[]` para o tipo 12, bloqueando o botão; a validação genérica rejeitava qualquer obra sobre um muro. Recuperamos o modelo real e implementamos a substituição de um segmento próprio e concluído. Não exige grama, mas exige muralhas em lados opostos. Pontas e cantos sem par oposto, muros adversários, obra inconclusa e célula ocupada são rejeitados.

O trecho anterior fica no histórico de custos e desaparece da lista ativa quando o portão termina. A substituição invalida os índices espaciais mesmo sem mudar a quantidade total de prédios. Saves conservam materiais e não recriam a muralha sob o portão ao carregar.

**CONFIRMADO:** `ValideHerse`, `0x41ad2b`, testa vizinhos de muralha/portão e produz orientação 0 para leste/oeste, 1 para norte/sul ou 255 para posição inválida. Os 16 padrões foram executados. A orientação está no offset 124 do registro; o frame está no byte 118. Desenho em `0x407dd3` / região `0x40809f`. Sprites 7576–7607 para orientação 0 e 7608–7639 para orientação 1, em `BUILDING.CMP`. `HerseEstBaisser` em `0x4193ef` identifica fechado no frame 15; `HerseEstLever` em `0x419419` identifica aberto em 0 ou 31.

O script `tools/trace_gate_v72.py` gera `reports/gate_v72_orientation.json` e `source_remake/Data/gate_visuals_v72.json`: 64 frames e 202 amostras de construção com os recortes originais. O catálogo copiado recebeu o modelo frontal fechado. Nenhum asset novo substitui as imagens recuperadas.

Depois da construção, o portão começa fechado. Selecioná-lo mostra um botão com seu retrato original para abrir/fechar. A abertura percorre 16–31; o fechamento percorre 0–15. Passagem só é liberada após abrir completamente. Não fecha com unidade na célula, e tropas aéreas também respeitam o fechamento. Portões preexistentes próprios possuem controle salvo. **Adaptação:** o botão usa o retrato original, mas a disposição do controle e a duração de um frame por atualização não foram comprovadas como idênticas às do executável original.

## Saves e verificação

Save v31, leitura de versões 4–30. Os cinco slots novos usam nome v31, com descoberta dos anteriores. Testes verificaram obra pendente, aberta, fechada, em animação, portão preexistente, refeição e retorno ao posto, continuação determinística, conservação, morte e rejeição atômica de dados inválidos. Os saves reais foram somente lidos.

O teste amplo encontrou uma transição legítima antes recusada: uma pilha inicial de comida consumida pode virar vegetação em solo fértil. Corrigimos `ValidGrowthMutation` sem retirar as verificações de quantidade e conservação. Há teste específico de carregar essa sequência.

Os testes longos de pesquisa recebem tropa abastecida com reserva 30 somente no cenário de teste, para isolar os tempos da pesquisa. A reserva normal continua 4. Também atualizamos um teste antigo de ambiente que já falhava na v71 por esperar construção com o próprio trabalhador ocupando a área. Ele agora verifica a restrição atual, sem mudar essa regra no jogo.

Compilação e publicação sem erros ou avisos. **812 verificações aprovadas**, incluindo **86 específicas da v72**. Evidências em `reports/v72-validation.json` e `logs/v72/`.

![Portões fechados e fome da tropa](../reports/v72-gates-closed.png)

![Portões abertos pelo botão da interface](../reports/v72-gates-open-ui.png)

![Menu do portão](../reports/v72-gate-menu.png)
