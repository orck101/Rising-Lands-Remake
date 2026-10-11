# Coleta v26

## Problemas reproduzidos e corrigidos

1. WorkerResourceOrders removia o trabalhador da lista de escolha de destino após a primeira entrega ser definida. Mesmo carregado, um segundo clique deixava de passar pelo comando de entrega. WaitingFor agora reconhece carga presente; o jogador pode redirecionar a entrega enquanto a transporta.
2. SourceGroup encaminhava todos os agricultores para a mesma planta. A busca agora prioriza plantas que ainda têm produção disponível para os trabalhadores já designados, mantendo a âncora escolhida e o limite da área. Pilhas continuam sendo compartilhadas; não são desviadas automaticamente para outro recurso.
3. FoodStep iniciava entrega assim que uma planta acabava, mesmo com espaço na carga e outras plantas próximas. Com repetição ligada, o trabalhador agora procura outra planta antes de retornar. Repetição desligada continua encerrando após a coleta escolhida.
4. O comando de destino interceptava cliques sobre outras unidades amigas. Esses cliques agora seguem para a seleção normal, preservando as ordens existentes.
5. O indicador de destino mostrava qualquer local como válido. Agora verifica o tipo de estrutura, ocupação da célula e capacidade da pilha. Essa prévia não calcula caminho a cada movimento do mouse; a acessibilidade é validada na emissão da ordem.

## Desempenho

A busca de caminho criava um vetor de 65.536 inteiros por chamada (aproximadamente 256 KiB), além da fila e de vetores de direções em cada célula expandida. Os vetores e a fila agora são reutilizados por instância da simulação; as direções são constantes. A ordem de expansão permanece a mesma. Dez pedidos de caminho no cenário de teste alocaram 408.960 bytes no total, incluindo demais operações de consulta. Isso não é uma medição de FPS nem garante ausência de travamentos em mapas maiores.

## Validação

Dois testes falharam antes da correção: redirecionamento e distribuição dos agricultores. Logs: `collection_v26_before.log` e `collection_v26_before_errors.log`.

Após as mudanças: **249 verificações na bateria focada**, incluindo nove novos casos de coleta/desempenho, e **346 na regressão geral**, com sobreposição. Testes existentes verificam conservação de recursos, disputa pela última unidade, repetição, bloqueio de caminhos, determinismo de partidas gravadas e missão até a vitória. Os novos casos verificam distribuição, âncora, redirecionamento, carga parcial e rotas determinísticas com memória reutilizada.

`reports/v26-harvest.png` foi revisada no renderizador real: fonte verde, área delimitada e destino azul. É cenário preparado, não uma sessão manual de mouse. Clique em unidade amiga e prévia de entrega foram verificados no código; não houve automação de interação manual nesta revisão.

## Fidelidade e limites

As permissões por papel, transições de plantas, capacidade e contadores previamente demonstrados permanecem. **A distribuição por disponibilidade de plantas é uma política do remake; ainda não foi demonstrada como o algoritmo original.** O raio local continua provisório em seis células. Não há promessa de rotas ótimas, formação, colisão entre todas as unidades ou controle completo de Cultivo/fome. Trabalhadores voltam com carga parcial se nenhuma planta alcançável permanece. Destino cheio mantém a carga com o trabalhador para nova escolha; não há despejo automático em outra área.

A atualização é compatível com o formato moderno v14 e não modifica saves do original. Versões anteriores do remake permanecem disponíveis. Foco exclusivo no remake e na primeira missão.
