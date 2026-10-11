# v51 — alcance delicado para qualquer tropa selecionada

Abrir `releases/interactions-v51/Jogar-Primeira-Missao.cmd`. Fechar a versão anterior antes de testar esta entrega.

## Alteração

Selecionar uma tropa de combate visível mostra seu alcance como um círculo pontilhado sem preenchimento. Inclui combate próximo, tropas à distância e os tipos especiais do catálogo. Seleções individuais e grupos funcionam nas missões 1 e 2; unidades inimigas selecionadas também têm indicador. Trabalhadores, colonos, mensageiros e balões sem ataque não recebem círculo. Unidades mortas ou residentes invisíveis também não.

O raio vem de `Data/combat_rules.json`: alcance × 32 pixels no mundo, escalado pelo zoom. Arqueiro: 10 células; espadachim: 1 célula. Não altera dano, distância de ataque, tempo de ataque, pesquisa ou comportamento de combate. O controlador de combate continua com as limitações já documentadas; desenhar o alcance de outros tipos não implementa seus ataques pendentes.

Pontos de 2 × 2 pixels, aproximadamente 8 pixels entre seus centros ao longo da circunferência, cor dourada clara com transparência. O espaçamento é calculado usando o raio na tela, preservando a delicadeza com zoom e com alcances diferentes. O círculo acompanha a unidade e desaparece ao desmarcá-la. Não há preenchimento, contorno contínuo ou retângulo sob o personagem.

## Causa e evidência

**CONFIRMADO no remake:** o filtro v50 exigia alcance maior que 1 e propriedade do jogador, excluindo tropas de combate próximo e inimigos. O teste inicial desta entrega reproduziu a exclusão do espadachim em MONDE01.ETA (`logs/v51-range-before.err`). O círculo do arqueiro ainda era desenhado, mas seus 96 pontos com alpha 90 produziam intervalos de aproximadamente 21 pixels em zoom 1 e pouca distinção no terreno. A v51 usa espaçamento menor e alpha 170, sem engrossar os pontos.

**REQUISITO DO JOGADOR:** círculo pontilhado delicado para qualquer tropa de ataque selecionada. A aparência deste indicador é uma adaptação solicitada; não se declara um recurso visual extraído do original.

## Verificação

- 769 verificações de missão, regressões e seleção passaram (`logs/v51-mission.log`), incluindo 62 novas verificações de alcance nas missões 1 e 2.
- Todos os 19 tipos de combate do catálogo têm indicador; sete tipos sem combate são excluídos. Mortos e residentes são excluídos.
- Capturas conferidas: `reports/v51-range-archer.png`, `reports/v51-range-melee.png`, `reports/v51-range-map2-mixed.png` e `reports/v51-range-zoom.png`.
- Preparação das capturas modifica tipos/posições somente em memória para comparar seleção individual, grupo misto e zoom 1,5. Não é uma partida salva e não representa formação paga.
- Save permanece v20. Arquivos e saves originais não foram modificados. Esta entrega mantém as correções de coleta da v50.

Campanha e funcionalidades pendentes seguem registradas nas entregas anteriores.
