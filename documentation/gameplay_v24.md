# Gameplay v24 — economia e papéis originais

## Entrega

Inicie `releases/gameplay-v24/Jogar-Primeira-Missao.cmd`. O trabalho continua focado na missão 1. As regras são compartilhadas pela simulação, mas esta revisão não certifica as outras missões.

- Permissões nativas de coleta: fazendeiro (5) e colheitadeira (13) recolhem comida, artesão (6) peças, construtor (7) pedra. Os papéis 6/13 não ganham recrutamento ou funções completas nesta entrega.
- Uma seleção de trabalhadores compatíveis recebe a fonte e a entrega para cada membro; grupos incompatíveis não alteram as ordens. A distribuição espacial/caminhos do grupo ainda não foi comparada ao original.
- Construtor pode extrair pedra e deixá-la no chão. Recolher essa pilha e entregar ao armazém só contabiliza o recurso na entrega final.
- Santuário deixa de aceitar entrega manual, seguindo o filtro original. Cultivo é um depósito elegível no original, mas seu armazenamento local e criação de colonos continuam pendentes no remake.
- Colheita do fazendeiro passa de 60 para 250 atualizações; primeira extração de pedra de 50 para 100. Continuação dentro da mesma carga usa 50. Esses valores provêm da configuração e dos trechos nativos; não representam equivalência de segundos ou bônus de pesquisa.
- Save v14 guarda destino de pedra no chão. O menu procura v14 e versões anteriores; migração remove destinos antigos de santuário preservando carga e estoque. Não altera os arquivos de save anteriores.
- Marca azul identifica destino de pedra no chão; painel informa a posição escolhida. Cesta/proibido de Ctrl permanece.

## Evidências e validação

Os scripts `trace_worker_policy.py` e `trace_work_timing.py` e os relatórios de mesmo nome preservam entradas, endereços e limites. A primeira bateria verifica 906 casos de elegibilidade nativa. A segunda verifica 11 trechos de inicialização/recarga de contador, com configuração e bônus controlados. Eles não executam uma partida do original.

Testes automatizados cobrem conservação, grupos, pilhas concorrentes, saves, migração, rejeição atômica, temporização e conclusão da missão. O relatório `reports/gameplay_v24_validation.json` registra resultados finais e hashes. Capturas mostram o renderizador do remake em cenário preparado, sem constituir uma sessão manual de mouse.

O teste histórico que exigia entrega no santuário foi substituído pela regra demonstrada: rejeitar santuário, segurar carga e aguardar escolha explícita de armazém. Os diagnósticos iniciais ficam em `logs/*gameplay_v24_initial_failure.log`. Uma falha durante a edição da mineração (entrega omitida por comentário na mesma linha) foi corrigida antes da validação final.

## Limites de fidelidade

Ver [auditoria por sistema](gameplay_fidelity_audit.md) e `reports/gameplay_fidelity_matrix.json`. Movimento e caminhos, fome, IA, diplomacia, bônus, edifícios especializados, veículos, efeitos de combate, campanha completa e cadência geral ainda não são equivalentes. F2/F3 ainda são atalhos de treino conflitantes com as teclas de velocidade descritas no README original. A interação Ctrl solicitada pelo usuário está preservada, mas o ciclo nativo completo precisa ser conciliado com a limpeza dos modos na liberação da tecla.

A vitória existente e o botão Próxima missão sem ação permanecem. Esta revisão melhora componentes comprovados e torna as diferenças restantes explícitas; não certifica toda a gameplay como fiel.
