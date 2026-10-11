# v88 — combate de estruturas e novo início do vermelho

## Comportamento entregue
- Vida das estruturas obtida pelo limite original menos o dano acumulado; barra acima da arte ao apontar, selecionar, manter Espaço, atacar ou quando estiver danificada e visível.
- Ordens de ataque às construções independem da ativação do diretor de IA. Aliados não são atacados automaticamente.
- Unidades à distância buscam uma posição de disparo alcançável. Seus projéteis usam os gráficos existentes e aplicam dano na chegada; ataques corpo a corpo usam o instante de golpe original.
- Estruturas destruídas saem da cena, liberam seu espaço e cancelam os ataques contra elas. Construções novas nesse espaço continuam respeitando colisões.
- Vida, dano, projéteis e ordens permanecem no save. O novo campo TargetBuilding tem padrão -1 para projéteis antigos contra unidades.

## Novo início da missão 4 (ajuste solicitado pelo jogador)
CONFIRMADO no remake: vermelho começa com 15 alimento, 15 pedra, zero engrenagens, um fazendeiro, um construtor, três colonos dentro do santuário e nenhum soldado. Os demais prédios iniciais são retirados. O primeiro armazém precisa ser construído, concluído e ocupado. Desenvolvimento e recrutamento aguardam a primeira entrega real de alimento.
Esse é um balanceamento solicitado, não uma afirmação sobre a missão original.
A configuração é persistida por Opponent.ReducedOpening. A redução do estoque integra as verificações de conservação, sem alterar os arquivos originais. Saves anteriores sem esse campo mantêm seu desenvolvimento; iniciar/reiniciar a missão 4 aplica o novo início.

## Evidências e limites
CONFIRMADO por catálogo já extraído: limites das estruturas em RISING-ORIG.EXE, tabela 0x495d90 + tipo*52, reutilizada por Engine/BuildingCombat.cs; danos e alcance em Content/Data/combat_rules.json; fases, tempos e projéteis em Content/Data/advanced_attack.json.
Revisadas as 12 classes do catálogo de ataque. Onze possuem ataque direto contra guardas; o Bomber (14) tem dano direto zero nessa tabela, e o druida (4) tem dano zero contra estruturas. Não foram inventados danos para contornar essas restrições. A ação especial do Bomber e o combate das criaturas neutras fora deste catálogo ainda exigem análise específica; esta versão não representa uma reconstrução integral dessas habilidades.
Animação do Hellfire (15) possui uma única pose de ataque no catálogo, preservada; demais classes verificadas avançam os frames originais.
O cálculo de aproximação segue o navegador do remake, ainda não é o algoritmo original reconstruído. A barra de vida é uma indicação funcional do remake; fidelidade exata de sua aparência ao original não foi demonstrada.

## Validação
- reports/v88-combat.txt: 89 verificações; dano por classe, animações, guarda contra guarda e retaliação, ataque às estruturas fora de missão com IA, voo do projétil e save durante o voo, destruição persistente e arqueiro disparando à distância.
- reports/v88-opening.txt: nove verificações; população/estoque exatos, santuário, primeiro armazém real, ocupação, depósito físico, conservação, save/load e reinício.
- reports/v88-legacy-saves.txt: nove verificações de migração de arqueiros antigos e filas ilegais.
- reports/v88-mission3.txt: 16 verificações da terceira missão e passagem à quarta.
- reports/v88-self-test.txt: suite geral aprovada.
- reports/v88-building-health.png: captura visual controlada de um santuário com 50% da vida, inspecionada. É uma fixture visual, não uma partida real.

## Jogar
releases/interactions-v88/Jogar-Quarta-Missao.cmd — iniciar uma nova missão 4 para avaliar o balanceamento.
