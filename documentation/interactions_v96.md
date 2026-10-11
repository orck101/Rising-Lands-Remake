# v96 — economia protegida e defesa dos inimigos

## Resultado

Política de decisão nova do remake, solicitada pelo jogador. Aplicada aos clãs ativos da missão 4 em diante, incluindo os três adversários da missão 5. Não é apresentada como tradução integral da IA nativa.

- Coleta e exploração econômica restritas a até 48 casas de santuário, cultivo ou armazém próprio, priorizando fontes próximas e já descobertas. Eliminado o recurso de procurar uma fonte no mapa inteiro quando não há nenhuma local.
- Áreas de prédios hostis descobertos recebem uma margem de proteção de 12 casas além da dimensão aproximada do prédio. Tropas hostis atualmente visíveis recebem margem de alcance de ataque + 4 casas, com mínimo de 5.
- O caminho dos trabalhadores e colonos evita essas áreas, inclusive quando o destino é seguro mas o caminho direto atravessa perigo. A fuga de alguém que já está em perigo admite movimentos que não aumentem a exposição até alcançar uma área segura.
- A busca automática de comida usa fontes locais e caminhos seguros; reavalia uma fonte quando ela se torna perigosa. Ao recuar, mantém a carga. A política não revela o mapa inteiro: lembra prédios descobertos e usa a visão atual das tropas.
- Fazendeiros e construtores ausentes são prioridade de reposição no santuário, usando colonos, recursos e tempo de treinamento reais. Não depende mais de uma primeira entrega de alimento para começar essa reposição.
- Mercado e bolsa de trocas continuam sendo as duas primeiras obras do vermelho na missão 4; o quartel vem depois. O planejamento limita temporariamente novos fazendeiros e reserva menos comida para construções futuras enquanto falta formar a defesa/grupo mínimo de ataque.
- A geração de colonos considera a necessidade de operadores dos prédios, tropas e reservas de pessoal, corrigindo o bloqueio fixo em oito colonos que parava a expansão.
- Conserva três combatentes para defesa; envia somente tropas excedentes, com pelo menos dois atacantes saudáveis e alimentados. Os primeiros três minutos seguem sem ofensiva; entre três e cinco minutos, somente grupos pequenos de soldados/incendiários.
- Recolhe tropas gravemente feridas (25% ou menos da vida) e grupos diante de força visível superior (estimativa de força maior que 1,4×), ou quando a base sofre invasão. Aguarda antes de nova investida. A força estimada considera vida, dano, alcance e ciclo de ataque; é uma heurística de balanceamento, não uma regra original confirmada.

O mapa de perigo é reaproveitado por 30 ticks e por proprietário para evitar recalcular toda a política a cada passo da busca de caminhos. A movimentação do jogador, seus comandos e seus limites de tecnologia permanecem os mesmos.

## Evidências

`reports/v96-before-fix.txt` registra um teste que falhava: o fazendeiro aceitava coletar comida ao lado de uma base hostil conhecida. O rastreamento mostrou buscas de recurso/fome em todo o mapa e falta de reserva militar em `PrepareOpponentWave`. Os testes longos também identificaram o limite fixo de colonos e o gasto do pessoal em fazendeiros antes de completar a força militar mínima.

## Validação da versão publicada

- Compilação/publicação Windows autocontida sem erros.
- `reports/v96-validation-0.txt`: 21 verificações; coleta segura, controles do jogador, fome, fuga com carga, reposição dos trabalhadores, paz inicial, reserva de três guardas, retirada por ferimentos/força superior, mina hostil, desvio de caminho e 66 minutos simulados da missão 4. Trabalhadores vivos: mínimo 3, máximo 4; cinco soldados reais ao final. Dez validações intermediárias e roundtrip final do estado.
- `reports/v96-validation-1.txt`: 13 verificações da abertura comercial, produção real, ataque com defesa, aliança e reinício. Primeira investida no cenário sem ordens do jogador: 110160 ticks, aproximadamente 61.2 minutos simulados (1x), ou 12.2 minutos de tempo real em 5x. Essa medição é do cenário de teste, não um tempo fixo nem uma garantia de balanceamento final.
- `reports/v96-validation-2.txt`: 36 verificações de reparo, visuais de dano, defesa e desenvolvimento com recursos reais de todos os inimigos da missão 5; estado carregável e conservado.
- `reports/v96-validation-3.txt`: regressão de fechamento corrigido na v95, coleta dos três recursos, armazém cheio, repetição, excedentes, salvamento e 33 minutos simulados com validação frequente.
- `reports/v96-final-3.txt`: inspeção somente de leitura de 15 saves existentes, sem alteração dos arquivos.
- `reports/v96-mission4.png`: inicialização/desenho real da missão 4 concluídos normalmente.

Total: 129 verificações aprovadas nos quatro grupos finais. Alguns relatórios de falha em `runtime/crashes` foram gerados por testes controlados desta implementação; seus argumentos contêm `--test-strategy-v96` ou `--test-trade-opening-v90`.

## Limites e utilização

Heurísticas de segurança e avaliação de força ainda precisam de partidas humanas para ajuste fino. Uma base totalmente sitiada pode ficar sem recursos seguros; a IA não cria recursos nem trabalhadores gratuitamente. Reposição exige santuário operante, colonos e estoque para pagar. O desenvolvimento da missão 4 ainda é gradual, conforme a medição acima.

[Abrir missão 4 com monitor](../releases/interactions-v96/Jogar-Quarta-Missao-Com-Monitor.cmd) ou [missão 5](../releases/interactions-v96/Jogar-Quinta-Missao.cmd). Carregar um save compatível aplica as decisões novas às próximas ações, mas não recupera personagens que já morreram; reiniciar a missão oferece o teste completo desde a abertura.
