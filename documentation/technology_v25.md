# Remake — tecnologia v25

Esta entrega integra os bônus nativos de carga e recarga dos trabalhadores à simulação. O foco de desenvolvimento e teste jogável continua sendo a primeira missão. Não importa nem aplica ao remake as alterações feitas anteriormente em saves do original.

## Alterações

- Capacidade de transporte passa a considerar tecnologia, em vez de usar somente a capacidade básica por personagem.
- A primeira extração de pedra usa RECHARGE menos o bônus tecnológico. A continuação da mesma carga mantém o contador nativo de 50.
- Engenharia civil 12 permite ao construtor carregar duas pedras; nível 13 reduz o contador inicial de 100 para 66. Esses valores foram extraídos das rotinas nativas.
- Pesquisas de capacidade/recarga do construtor receberam suporte e nomes no painel. Outros efeitos não implementados continuam bloqueados; a árvore ainda NÃO oferece um caminho completo até todos esses níveis.
- Validação de partidas do remake calcula capacidade usando a pesquisa presente no estado carregado, evitando comparar contra a tecnologia da sessão anterior.

Na missão 1, os níveis iniciais permanecem agricultura 2, militar 2, engenharia 4 e religião 0. Portanto, o construtor continua com capacidade 1 e contador inicial 100 nessa fase. A integração não dá tecnologias avançadas gratuitamente.

## Evidência

`source_remake/Data/worker_technology.json` contém 2.000 combinações de níveis de agricultura/engenharia e tipos 5, 6, 7 e 13, extraídas com 4.000 consultas nativas. InventionSetEffet 0x470e88 e BonusGetValEngin 0x428d58 fornecem os bônus. O carregamento real de 2.048 bytes de bônus do save analisado anteriormente foi reproduzido byte a byte, como validação da interpretação. O save não é carregado pelo remake.

O exportador é uma ferramenta de pesquisa, separada do jogo. A simulação usa apenas o JSON empacotado. Atualizações de entidades e interface do original não são executadas pelo exportador.

## Validação

**240 verificações na bateria focada** e **346 na regressão geral**, com sobreposição. A bateria focada inclui 13 verificações adicionais de tecnologia em cenário controlado, e não uma campanha jogada até os níveis avançados. Confirma contadores, carga de duas pedras, conservação, espera por destino e reinício. Não houve partida manual nesta revisão.

A versão usa o formato moderno v14 existente; nenhuma mudança estrutural foi necessária. A validação geral de partidas passou, mas um percurso completo de pesquisa avançada → gravar → carregar ainda não foi coberto por um cenário jogável legítimo.

## Limites e próxima prioridade

Agricultura não tem progressão de pesquisa integrada. Construção acelerada, fome, combate tecnológico e demais ramos continuam parciais. O tempo real da simulação ainda é provisório. A auditoria v24 continua válida, exceto pela capacidade e recarga de trabalhadores agora integradas parcialmente.

Prioridade para a missão 1: completar o papel do Cultivo e o destino de alimento nessa estrutura, com armazenamento local, consumo e geração de colonos confirmados no original. Não usar crédito direto no estoque global como substituto dessa mecânica.

## Diretriz atual do usuário

Foco exclusivo no remake. Não voltar a alterar saves originais ou produzir versões modificadas deles. Os artefatos antigos são apenas histórico; só retomar esse assunto mediante novo pedido explícito.
