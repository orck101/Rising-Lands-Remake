# v100 — Bolsa de Trocas e bloqueio da ofensiva na missão 4

## Bolsa de Trocas

- Personagens próprios podem receber a ordem de entrar na Bolsa mesmo antes de existir uma negociação. Entrada em grupo utiliza as vagas disponíveis, incluindo a reserva de personagens ainda a caminho.
- Todas as classes de personagens do catálogo são admitidas, incluindo tropas aéreas: tipos 0–17 e 25. Mensageiros já envolvidos em uma negociação e cópias temporárias de magia não são admitidos simultaneamente.
- A unidade caminha até a porta com os frames originais já verificados na v99; somente após a entrada fica oculta e aguardando. Sua classe, dono, vida e carga são preservados durante a espera.
- A negociação registra as classes e quantidades prometidas. A entrega pode usar qualquer personagem da mesma classe, não apenas o indivíduo escolhido ao montar a oferta.
- A aprovação prepara o acordo. O jogo espera as classes e quantidades completas nas Bolsas dos dois participantes, verifica os recursos e transfere tudo em uma única conclusão. Não transfere tropas parcialmente, nem repete uma troca concluída.
- Personagens extras permanecem na Bolsa. Personagens que já estavam guardados também aparecem entre as unidades disponíveis para oferecer no Mercado.
- O adversário envia as unidades combinadas à sua Bolsa. A entrega aceita permanece pendente até ser cumprida ou cancelada; a avaliação e as propostas ainda não aceitas mantêm seus prazos anteriores.
- Clicar na Bolsa mostra os retratos originais das classes guardadas. Clicar num retrato retira um personagem daquela classe; duplo clique no prédio também retira um personagem. A saída não o seleciona automaticamente.
- A capacidade usa a tabela de moradores existente: oito lugares para a Bolsa, descontando operadores e entradas reservadas. Não são aceitos acordos com mais tropas que a capacidade utilizável de um dos participantes. Pode ser necessário retirar tropas extras para abrir vagas.
- Cancelar um acordo mantém os personagens guardados por vontade do jogador. Destruir a Bolsa remove seu estado de armazenamento e trata os personagens internos como baixas, conforme a regra de destruição dos moradores.

O armazenamento é separado dos colonos operadores e da negociação. A lista antiga de moradores podia reter o identificador de personagens já treinados; a entrada na Bolsa remove essa associação antiga para não registrar a mesma unidade em dois lugares ao salvar.

## Investigação do slot 5

Fonte preservada: `UserSaves/campaign.slot-5.research-v33.json`; cópia de trabalho: `experiments/v100/slot-five-snapshot.json`.

SHA256: `c8d07f62ef2fc6e36c19c76a55db3e321b35c98e0d401eeb92978efc6c379a72`.

CONFIRMADO no save e na reprodução:

- Missão 4, tick 794.794: 7h21min de tempo de simulação.
- Vermelho com 245 alimentos, 206 pedras, capacidade ativa total de 400 e nenhum espaço livre.
- Oito soldados vivos, oito fazendeiros vivos e quatro construtores vivos. Não havia aliança com o jogador.
- Política em Recuperação, estabilidade zero, produção medida zero, manutenção estimada de 3 alimentos/min e reserva exigida de 41. Havia 2.606 alimentos conhecidos em áreas avaliadas como seguras/acessíveis.
- O histórico dos últimos três minutos permanecia em produção acumulada 191. O estoque alto, isoladamente, não satisfazia o requisito de produção sustentável.
- Avançar mais 18.000 ticks, dez minutos, na cópia antes da correção manteve a produção medida em zero e não lançou ataques.

CAUSA CONFIRMADA: `FoodProduction` somava entregas ao estoque e ao cultivo, mas não a colheita nova despejada no chão quando o armazém estava cheio. A política interpretava a ausência de depósitos como incapacidade de sustentar tropas e nunca saía da recuperação.

Correção: alimento recém-colhido de plantas e efetivamente descarregado por excedente passa a integrar o histórico produtivo daquele clã. Mover pilhas existentes ou retirar recursos de um armazém não conta como produção nova. A contabilização não cria alimento nem altera o estoque disponível.

Na cópia do mesmo save, a produção medida voltou a crescer e a IA liberou três soldados no tick 803.700, após 296,87 segundos adicionais de simulação, aproximadamente 4min57 em 1x ou 59s em 5x. Esse resultado é o tempo observado nesse cenário, não um novo prazo fixo para ataque. Permanecem reserva econômica, estabilidade, proteção da base, limite de três unidades por ofensiva da missão 4 e exigências de tecnologia/custo.

O teste comprova o desbloqueio e a formação da ofensiva; não mede vitória nem afirma que as três unidades chegaram à base do jogador nesse intervalo.

## Evidências e limites

- `reports/v100-before.txt`: falha reproduzida da entrada sem acordo.
- `reports/v100-slot5-before.txt` / `v100-slot5-after.txt`: comparação no mesmo save, com hash e relógio registrados.
- `reports/v100-tests.txt`: armazenamento, classes, capacidade, substituição por classe, entrega parcial, extras, cancelamento, aeronave, destruição e salvamento.
- `reports/v100-ui.txt` / `v100-bag-ui.png`: clique em grupo, chegada, retratos e retirada testados pela interface real.
- `reports/v100-regression-trade.txt`, `v100-regression-entrance.txt`, `v100-regression-v71.txt`, `v100-regression-policy.txt`: regressões de comércio, animação, mecânicas e economia.
- `reports/v100-player-saves.txt`: leitura dos saves existentes sem sobrescrevê-los.
- `reports/v100-publish.txt` e `logs/v100-release-audit.json`: pacote jogável e verificação da entrega.

O comportamento de espera por classe e a ampliação a todas as tropas atendem à instrução do usuário. Não são apresentados como uma nova comprovação do algoritmo original. A caminhada da porta conserva a adaptação geométrica documentada na v99. O valor de negociação das aeronaves ainda não foi comprovado no executável original; para Heliped foi adotado o custo de produção da tabela do remake. Balloon não tem preço de produção nessa tabela e não recebeu um preço inventado.

Saves gerados passam à versão interna 34, com leitura das versões anteriores. Os nomes dos cinco slots permanecem iguais para que as partidas existentes continuem aparecendo no menu. A migração acontece em memória; não regrava saves antigos por conta própria.

## Para testar

Abra `releases/interactions-v100/Jogar-Quarta-Missao-Com-Monitor.cmd` e carregue o slot 5. A medição da produção será atualizada conforme os trabalhadores concluírem entregas reais. Para testar a Bolsa, reúna personagens dentro dela antes ou depois de um acordo e confira que somente as classes/quantidades exigidas são transferidas.
