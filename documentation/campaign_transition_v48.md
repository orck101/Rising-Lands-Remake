# v48 — transição da primeira para a segunda missão

Abra `releases/interactions-v48/Jogar-Primeira-Missao.cmd`.

## Entrega

Após cumprir a primeira missão, clique em Próxima missão. A tela reutiliza a arte original de pesquisa, com os quatro ramos na ordem original: Agricultura, Engenharia, Armamento e Espiritualidade. Clique na coluna desejada (ou use 1–4) e confirme no símbolo original de confirmação ou com Enter. Sem escolher um ramo, a confirmação não avança. Esc retorna à vitória mantendo a escolha.

A confirmação abre MONDE02.ETA e seu texto de missão em português. Nenhuma unidade é selecionada automaticamente. A escolha define a preferência de pesquisa, sem conceder um nível instantâneo nem recursos gratuitos. A campanha preserva os níveis do jogador e o acumulador; o mapa novo usa seus próprios personagens, recursos e construções. Reiniciar a segunda missão mantém a herança e a escolha iniciais daquela missão.

F5/F9 na vitória e na transição abrem os cinco slots de salvar/carregar. Save v18 inclui a campanha. Slots v17 continuam disponíveis; ao salvar a migração é gravada em outro arquivo v18, preservando o v17. O estoque e os arquivos originais de D:/RISING não foram alterados.

## Evidência e grau de certeza

**CONFIRMADO em emulação controlada do binário copiado:** escolha de ramo nos blocos 0x42cb6d, 0x42cb7d, 0x42cb86 e 0x42cb8f modifica o ramo, preservando os quatro níveis e o acumulador de pesquisa. Ordem de exibição [0,2,1,3]. Os registros foram preparados com níveis [2,2,4,0] e acumulador 123,5. A ferramenta intercepta UI/OS; não executa o jogo original no Windows.

**CONFIRMADO em emulação controlada:** RechercheStore (0x4713e0) transfere 64 bytes, correspondentes aos registros de pesquisa dos quatro jogadores. RechercheCalculNew (0x4713ff) restaura os registros e aplica o mínimo por missão. Doze combinações de níveis e ramos confirmam o mínimo [2,2,4,0] para a missão 2 sem diminuir níveis superiores. A transferência implementada nesta entrega refere-se ao jogador controlado; não equivale a uma reconstrução completa da campanha dos adversários.

Método reproduzível: `tools/trace_transition_choice_v48.py`. Evidência: `reverse_engineering/transition_choice_v48.json`. A sequência de vitória até menu 17 está registrada em `tools/trace_campaign_transition.py` e `reports/campaign_transition_validation.json`. Arte reaproveitada: `source_remake/RisingLandsRemake/Content/original-research.png`.

**ADAPTAÇÃO do remake:** legendas em português, seleção por teclas 1–4, contorno de escolha e integração com os cinco slots. Estes detalhes não são apresentados como comportamento comprovado do executável original.

## Verificação

- 606 verificações da primeira missão e interações passaram; 351 verificações gerais passaram.
- Campanha bloqueada antes da vitória; quatro escolhas sem aumento instantâneo de nível/estoque; round trip de cada escolha na transição e na missão 2; reinício; herança de níveis e acumulador; rejeição de campanha corrompida preservando estado; migração v17 e precedência v18 sem alterar save antigo.
- Captura `reports/v48-mission-two.png` verifica cliques nas quatro colunas, bloqueio de confirmação sem escolha e clique de confirmação abrindo MONDE02.ETA sem seleção automática.
- Capturas da transição e do texto da missão 2 conferidas visualmente. Logs: `logs/v48-mission-one.log`, `logs/v48-self-test.log`, `logs/v48-mission-two.err`.
- Corrigida a validação de save do mapa 2: pilhas vazias já presentes no mapa original podem permanecer intactas. Alterações inválidas de pilha continuam rejeitadas. Corrigida a liberação de prédios/unidades para evitar tratar pesquisa saltada como descoberta concluída.

## Limites e próximo trabalho

Esta entrega implementa a passagem 1 → 2, a escolha e sua persistência. A segunda missão abre e usa os sistemas existentes; seus objetivos, vitória, combate, IA e fidelidade de gameplay ainda precisam de investigação e validação específicos. Apenas a primeira missão possui avaliação de vitória reconstruída neste momento. Não existe progressão completa das demais fases.

Agricultura e Engenharia têm os efeitos já implementados nas versões anteriores. Selecionar Armamento ou Espiritualidade preserva a preferência; não completa seus efeitos de pesquisa ainda pendentes. A tela não implica que os quatro sistemas de pesquisa estão finalizados. Alguns bônus associados a tecnologias herdadas também precisam ser confrontados com o original nas próximas missões.

## Teste manual sugerido

1. Carregue um save da primeira missão próximo da vitória ou cumpra os objetivos; abra Próxima missão.
2. Escolha um ramo, salve em um slot, saia e carregue: a escolha deve voltar na tela de transição.
3. Confirme; leia os objetivos em português e feche a tela para explorar o mapa 2.
4. Salve/carregue a segunda missão e confira o ramo e os níveis. Reiniciar deve voltar ao começo da missão 2 com a mesma escolha.
