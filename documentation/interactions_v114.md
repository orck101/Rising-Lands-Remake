# Ataques e simpatia — v114, 2026-10-08

Pedido: investigar no original e aplicar queda de simpatia por ataques. Pacote novo releases/interactions-v114; v113 preservado. Formato 39 e nomes dos slots research-v33 preservados.

## Evidências do executável original
CONFIRMADO: MAJSympathie em 0x475044 usa tabela 0x497238, registros de 16 bytes (delta, modo, contador, período). Eventos 0/1 valem −3 e são chamados por EnginInfligeDegat em 0x45084a. Avalia opinião da vítima sobre o atacante, com limite −50..50; dados de jogador especial podem impedir atualização. Vinte e dois ensaios isolados confirmaram valores, limites, ataque ao próprio dono ignorado e eventos 10+6 combinados.

CONFIRMADO: InfligeDegat, ramo aliado, chama evento 10 (−25) e evento 6 (−10), em 0x450a3d/0x450a55 para prédio e 0x450ee5/0x450efe, 0x450fb2/0x450fcb para unidades. Em seguida rompe a aliança nos dois sentidos. Penalidade total −35, além do evento de dano posterior.

CONFIRMADO: BatimentInfligeDegat em 0x41a361 compara estágio inteiro floor(dano*200/resistência máxima), novo/antigo. Ao avançar, chama evento 8 em 0x41a4fa. Evento 8 tem delta −50 e modo 1: atualiza todas as relações dirigidas entre jogadores distintos. Não é simplesmente −3 por golpe em prédios. Quatro ensaios executaram a rotina nativa completa com despacho diplomático interceptado, confirmando limiar de avanço e ausência de evento sem mudança. Tabela e modo global confirmados pelo código 0x4750c2–0x475136. Sem executar o processo original, DirectX ou alterar D:\RISING.

## Implementação
- Dano positivo em unidade provocado pelo jogador diminui simpatia da vítima em 3. Corpo a corpo, projéteis e habilidades que usam ApplyHit convergem no mesmo ponto; sem duplicação por animação ou ordem repetida.
- Ordem válida do jogador contra aliado reduz simpatia em 35 e rompe aliança. Ordem repetida após ruptura não cobra novamente. No remake a ruptura continua no momento da ordem aceita, como já acontecia; no trecho nativo analisado ocorre na resolução/lançamento do ataque. Componente de dano unitário posterior ainda soma −3.
- Avanço de estágio de dano em prédio por atacante de outro dono reproduz evento global −50. Logo também piora simpatia dos outros clãs perante jogador, mesmo quando não são o alvo. Ataque inimigo a prédio do jogador também pode disparar esse evento global, conforme modo nativo. Relações seguem limitadas a −50..50. Isso não rompe outras alianças automaticamente.
- Dano de ferramenta de pesquisa sem atacante, dano zero e alvo já removido não geram penalidade.
- Trocas continuam capazes de aumentar simpatia; não foi modificada a aceitação de aliança por limite zero.

A engine guarda apenas simpatia de cada inimigo perante jogador, sem matriz completa de opinião inimigo→inimigo. A atualização direcional por dano entre dois inimigos não é persistida, pois ainda não existe essa matriz. O evento global atualiza todas as três relações disponíveis. Zero dano não penaliza no remake: escolha para contabilizar dano efetivo, não afirmação de equivalência do caminho nativo com dano zero.

## Arquivos
Engine/CombatDiplomacy.cs, CombatSimulation.cs, BuildingCombat.cs e ProjectileSimulation.cs; Tests/CombatDiplomacyTests.cs e Program.cs. Backup: backups/antes-diplomacia-v114-20261008.
Ferramentas: tools/trace_combat_goodwill_v114.py e tools/trace_building_goodwill_v114.py. Evidências: experiments/combat-diplomacy-v114/original.json e building-native.json; testes/logs na mesma pasta.

## Verificação
14 verificações específicas, incluindo combate real pelo loop do motor, golpe nulo, limite, ruptura única, dano global, dano sem atacante e save/load. 89 verificações de combate v88 (classes, projéteis, cerco, destruição, aproximação), 21 regressões de comércio v113 e 35 de conclusão da missão 5 passaram. Cópias dos cinco slots atuais carregadas e retomadas deterministicamente por 300 ticks. Não houve partida manual nem validação auditiva. Saves reais não editados; hashes registrados em saves-after.json. Limitação transitória de save durante entrada sob ataque da v113 permanece separada.

## Entrega
Atalhos regulares D:\CHATGPT\Jogar-Primeira-Missao.cmd até Jogar-Sexta-Missao.cmd usam v114. Partidas já abertas continuam com a versão antiga até reabertura; nenhuma foi encerrada.
