# v55 — vitória da missão 2 e passagem para missão 3

Abrir `releases/interactions-v55/Jogar-Segunda-Missao.cmd` para retomar diretamente a segunda missão e carregar um de seus slots. `Jogar-Primeira-Missao.cmd` mantém o início da campanha e a passagem 1 → 2.

## Objetivo e funcionamento

A missão 2 conclui quando o jogador possui santuário e quartel na região inicial da base e pelo menos 50 alimentos disponíveis, mantendo personagens vivos. A região confirmada é x = 36–63, y = 0–23, no canto superior direito de MONDE02.ETA. Não exige eliminar inimigos nem um estoque mínimo de pedra. Os recursos no chão ou carregados não contam para os 50 alimentos disponíveis.

O jogo mostra “Você cumpriu a missão / Missão 2 concluída” e bloqueia novas ordens durante essa tela. Clicar em “Próxima missão” abre a escolha de tecnologia existente, incluindo os próximos benefícios e as animações originais. Confirmar abre MONDE03.ETA com seus objetivos em português e nenhuma unidade automaticamente selecionada.

Níveis desenvolvidos, ramo escolhido e acumulador de pesquisa são transferidos para a missão seguinte; mínimos definidos pelo catálogo de cada missão continuam sendo aplicados. Recursos, personagens e construções da nova missão vêm de seu próprio mapa. Salvar/carregar preserva vitória e tela de escolha; reiniciar a missão 2 restaura seu início e remove essa vitória.

## Evidência original

**CONFIRMADO:** `working/RISING/CD/WORLD/MONDE02.TXT` pede santuário, quartel e 50 alimentos. `MONDE02.ETA` contém os registros de término no offset 340338 (100 bytes), após o header no offset 340334. O avaliador original em 0x4585d2, carregado por 0x458ad1, foi executado em emulação isolada para 48 combinações de presença de prédios, propriedade, região e alimento 49/50/51. Vitória somente com ambos os prédios próprios na região 0 e alimento ≥ 50. Não foi fornecida pedra ao avaliador nesses casos; ela não é requisito adicional.

Região confirmada por 12 chamadas ao getter original 0x47bbf6, usando os bytes de retângulo `240000003f0017000000` do mapa. Dentro: x 36–63, y 0–23; fora retorna 255. Ferramenta `tools/trace_mission_two_v55.py`; offsets, hashes e vetores em `reverse_engineering/mission_two_v55.json`.

Limites do trace: consultas a prédios e contagem de sobreviventes foram fornecidas como fixtures; comparação de recursos e avaliador de término executam código original. O lookup da região executa o getter original em casos separados. IA, processamento integral da campanha nativa e duração em segundos não foram emulados.

**FORTE EVIDÊNCIA / comportamento preservado:** a condição nativa usa existência do prédio, cuja consulta compartilhada já foi confirmada na v21/lifecycle como estado não zero, inclusive construção em andamento. Como na missão 1, o remake aceita uma obra iniciada do tipo requerido na região; não acrescenta um requisito de ocupação ou conclusão visual que não apareceu no avaliador. O teste integrado desta entrega constrói os dois prédios completamente antes de estocar os 50 alimentos finais.

O limiar de 100 avaliações positivas segue o gate compartilhado já confirmado. Uma avaliação por tick moderno é provisória; não se declara o tempo de espera original em segundos.

## Validação

- 782 verificações de missão 1/interações, 68 da missão 2 e 351 gerais passaram: 1.201 no total.
- Execução integrada sem recursos gratuitos: construir e habitar armazém com colono inicial, coletar comida e pedra solta/minério disponível, pagar santuário e quartel, terminar construções e estocar 50 alimentos.
- Vitória e transição salvas/carregadas; vitória falsa em save rejeitada sem mudar a partida; reinício limpa progresso.
- Quatro ramos de escolha abrem a missão 3 com níveis herdados e histórico de conclusão 2. Estado da missão 3 salva/carrega corretamente.
- Capturas conferidas `reports/v55-mission2-victory.png`, `reports/v55-mission2-transition.png`, `reports/v55-mission2-third.png`. A captura executa o mesmo percurso integrado e os mesmos manipuladores de botões do jogo, incluindo confirmação e ausência de seleção automática.
- Logs `logs/v55-test-mission-one.log`, `logs/v55-test-mission-two.log`, `logs/v55-self-test.log`.

Save permanece v20 e aceita partidas anteriores. Arquivos e saves originais preservados. Esta entrega implementa término/avanço da missão 2; não conclui a IA nativa nem o término da missão 3 ou os efeitos de pesquisa ainda pendentes.
