# v78 — defesa das tropas e pesquisa da IA original

## Entregue na versão jogável

- Corrigida a exclusão do jogador humano na reação ao receber dano. Tropas de combate paradas podem reagir ao atacante.
- Defesa automática em todas as missões, inclusive quando o controlador estratégico do adversário está desativado.
- Tropas procuram adversários visíveis dentro da sua área de visão; priorizam quem está atacando a própria unidade ou aliados próximos.
- Combate próximo aproxima a unidade; combate à distância preserva a posição quando o alvo já está ao alcance.
- Procura de posições de combate considera o alcance da classe. Uma posição ocupada não cancela a intenção de atacar; novas tentativas têm intervalo.
- Perseguição automática limitada à vizinhança do ponto onde a defesa começou. Comandos de movimento e tarefas ativas do jogador são preservados.
- Aliados, mensageiros e personagens sem papel de combate não são adquiridos pela defesa automática. Monstros do proprietário 4 mantêm seu tratamento separado.
- Parar cancela a ordem atual e adia por um segundo uma nova aquisição passiva. Receber um golpe pode provocar reação imediata. Movimento ou ataque explícito tem prioridade.
- Estado de defesa, último atacante e temporização acompanham o save do remake. Leitura dos saves anteriores preservada; formato v33 mantido.

**CONFIRMADO no remake:** 11 casos dedicados, teste geral e testes de campanha/save/load. Isto corrige a passividade; não comprova reprodução integral da inteligência do original.

## Pesquisa executada

`OriginalOpponentGroups` foi comparado com criação, remoção, capacidade, seleção e transferência de grupos no executável original: 1.233 estados comparados/restaurados. Transferência a partir do grupo de reserva está coberta; empréstimo entre outros grupos ainda não está demonstrado.

`OriginalOpponentBrain` carrega o PE32 verificado, os registros RULE e dados da cópia do mapa em uma CPU emulada. Foram executados 120 ciclos para MONDE04 e MONDE05, com 60 comparações de estado após restauração por mapa. As rotinas observadas foram 34 e 22, respectivamente. **Não houve ordens físicas capturadas no trecho final desses testes; eles não validam economia ou guerra completas.**

A chamada direta do emulador dentro do processo .NET encerrou o processo com 0xC0000409. O caminho de pesquisa passou a usar um auxiliar isolado, sem mudar as configurações de segurança do Windows. Há um [problema documentado pelo projeto Unicorn](https://github.com/unicorn-engine/unicorn/issues/2281) compatível com essa falha. O auxiliar exige o Python e as bibliotecas existentes na área de trabalho; não é necessário para jogar a versão v78.

O executável analisado possui SHA-256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. A imagem original não é lançada como processo e o ambiente emulado não oferece funções do Windows, arquivos ou rede ao código do jogo.

## O que ainda falta para a reprodução integral

O backend original e seu adaptador **não estão ativados na partida comum**. O controlador estratégico jogável ainda contém decisões provisórias da v77. A defesa acima é uma melhoria do remake, não uma tradução comprovada de todas as decisões originais.

Faltam: carregar e aplicar ataques iniciais e regiões de influência; conferir os demais estados globais da inicialização; integrar resultados e notificações de ações de coleta, construção, reparo, entrada/saída e recrutamento; sincronizar morte, nascimento e mudança de grupo; integrar guerra, diplomacia, eventos e habilidades; incluir o estado desse backend no save da simulação; validar partidas prolongadas e desempenho nas demais missões. O método `EnableOriginalOpponentForResearch` é experimental e não deve ser tratado como uma implementação concluída.

## Evidências

- `reports/v78-defense-tests.txt`: defesa e continuidade após save/load.
- `reports/v78-self-test.txt`: verificação geral.
- `reports/v78-v75-tests.txt`, `v78-v76-tests.txt`, `v78-v77-tests.txt`: adversários, mapas, estoques, laboratórios e continuidade.
- `reports/opponent-groups-native-v78.json`, `source_remake/Data/opponent_group_cases.json`: comparação dos grupos com o original.
- `reports/native-brain-v78-MONDE04.json`, `native-brain-v78-MONDE05.json`: rotinas observadas e limites do ensaio da CPU original.
- `tools/original_x86_worker.py`, `tools/probe_full_opponent_v78.py`: experimentos reproduzíveis.
- `reports/v78-performance.json`: cenário sintético com 313 unidades e até 200 construções. As primeiras medições foram concorrentes com a versão anterior; não sustentam uma afirmação de ganho de desempenho nem medem renderização em 5x.

Originais e saves originais continuam intactos. Modificações restritas ao código, ferramentas, testes, documentação e publicação do remake na área de trabalho.
