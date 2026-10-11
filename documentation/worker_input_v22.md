# Fonte com Ctrl e destino pelo mouse — v22

## Correção solicitada

O usuário confirmou: manter Ctrl pressionado enquanto clica na fonte; soltar Ctrl para clicar no destino. A v21 possuía ordens contextuais independentes e atalhos C/E/M; não representava essa sequência. O diagnóstico veio da leitura de IssuePointerOrder/Update: modificador Ctrl não participava da escolha da fonte, e não havia estado de destino pendente.

WorkerResourceOrders agora valida a fonte, inicia a tarefa, limpa o destino anterior somente após sucesso e registra o trabalhador aguardando destino. WorkerResourceInput intercepta clique simples antes da seleção/movimento comum. Ctrl é capturado no início do clique; arrastar continua seleção, não coleta. Ctrl+1..9 conserva sua função de grupo. C/E/M continuam alternativas, não etapas necessárias. Os botões de coleta agora explicam o uso do Ctrl, e o rodapé mostra o fluxo. Cursor verde indica fonte válida; vermelho indica ausência de recurso coletável. Não há movimento automático ao clicar com Ctrl numa fonte inválida.

Depois de soltar Ctrl, clique no armazém/santuário próprio ou no chão para definir a entrega de alimento/pilhas. Destino inválido mantém a escolha pendente. O trabalhador continua andando à fonte, mas conserva a carga enquanto não houver destino. Save/load entre as duas escolhas recupera a espera a partir da tarefa e da ausência de destino, sem depender do estado transitório do mouse. Construir, retomar obra, parar e movimentar cancelam trabalhos incompatíveis preservando a carga.

## Recursos

O transporte de pilhas passou a aceitar códigos66/67/68: engrenagem/alimento/rocha. Agricultor e construtor têm capacidade básica já extraída do original. Plantas são colhidas pelo agricultor; jazidas são mineradas pelo construtor. Transporte de pilhas usa CargoKind0/1/2 e o campo de carga Food existente (nome interno histórico); a mineração continua usando Rock. Validação impede carga combinada acima da capacidade e tarefas simultâneas de construção/mineração/transporte.

Savev13 armazena CargoKind, enquanto versões4–12 migram para carga de alimento sem alterar reservas antigas. Conservação verifica separadamente alimento, pedra (incluindo jazidas e pilhas) e engrenagem, somando cargas, estoques e gastos. Pilhas só podem ser depositadas em chão livre ou pilha do mesmo tipo. Imagens das pilhas foram extraídas do renderizador original; estoques de engrenagem aparecem no cabeçalho.

## Evidência e limites

**CONFIRMADO PELO USUÁRIO:** sequência Ctrl+clique na fonte e clique sem Ctrl no destino; fontes restritas a recursos existentes. A equivalência exata de todos os ramos do controlador original de mouse ainda não foi emulada. Não chamar a reconstrução de Ctrl de comprovação por execução do binário.

**CONFIRMADO POR EXECUÇÃO EMULADA:** TerrainRetireMatiere0x47658e reduz uma unidade e retorna o tipo de recurso esperado para códigos66 e68; quantidade1 limpa a célula. Foram62 casos (quantidades1..31), com extração de imagens via AffVegetTerrain0x410085 em ambiente controlado. Ferramenta tools/trace_resource_piles.py; relatório reports/resource_piles_original_validation.json; imagens source_remake/Data/resource_pile_visuals.json. Código67 foi validado anteriormente em trace_food.py.

**INTEGRAÇÃO MODERNA / PARCIAL:** seleção de um trabalhador por vez, a elegibilidade completa de todos os tipos de unidades, tempos de transporte, busca de plantas, animações e cores do cursor. O descarte de rocha extraída diretamente da jazida no chão continua não implementado; pilhas de pedra já existentes aceitam transporte para chão ou estrutura. Não há coleta automática em chão vazio nem produção espontânea de engrenagem.

## Verificação

Teste focado --test-mission-one inclui fluxo fonte/destino para agricultor e construtor, cada um com os três recursos; vazio inválido sem alteração da ordem, origem igual ao destino rejeitada, save/load antes e depois de escolher destino, conservação e depósito no estoque correto. Pilhas de engrenagem/pedra desses casos são fixtures isoladas do mapa1; o mapa normal não recebe recursos adicionais.

O percurso completo até vitória passou a usar WorkerResourceOrders (mesma camada chamada pelo mouse) para indicar fonte antes do destino. Não usa C/E/M nem injeta estoques. Não equivale a uma sessão manual de teclado/mouse; a captura confere visualmente o estado após escolher a fonte. Conclusão e botão de próxima missão sem ação foram preservados. Foco permanece exclusivamente na primeira fase.

Relatórios: logs/mission_one_v22.log e reports/worker_input_v22_validation.json. Originais de D:/RISING não escritos; integridade das cópias em original/working conferida contra inventário.
