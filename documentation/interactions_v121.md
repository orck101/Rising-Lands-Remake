# Alimentação pela fonte mais próxima — v121

2026-10-09. Política de remake solicitada pelo usuário; substitui os raios200 e a preferência chão→próprio→aliado.

Todos os personagens que precisam comer, dos quatro proprietários, buscam até20 quadrados (distância euclidiana em células) da posição onde iniciam a busca. Comida solta no chão, armazéns próprios e armazéns aliados ocupados/com estoque entram na mesma lista. Ordena por distância geométrica até a pilha ou ponto de interação da porta; tenta primeiro a fonte válida mais próxima com caminho aceito. Não prefere uma categoria ou propriedade. Empates determinísticos por célula/id. Fontes inacessíveis são ignoradas. O percurso aceito continua limitado ao dobro do raio, agora40 passos, para evitar desvios excessivos.

Armazéns hostis não são candidatos. Estoque consumido é debitado do dono e o saldo de alimentação compartilhada preservado. Regras de conhecimento de recursos da IA e rotas de segurança já existentes continuam; não foi implantada nova política de segurança do jogador.

Se não existe fonte válida, ação/carga são mantidas e fome/dano continuam. Se uma pilha acaba ou um armazém deixa de estar disponível durante a viagem, cancela a viagem alimentar e restaura tarefa/carga antes da próxima busca. Ao carregar, viagens antigas fora do raio ou obsoletas são canceladas em memória e a tarefa interrompida é retomada. Não modifica arquivos de saves.

Arquivos: Engine/HungerSimulation.cs; Tests/NearestMealTests.cs, HungerContinuesTests.cs, MealRadiusTests.cs; Program.cs. Os testes existentes foram atualizados porque suas expectativas antigas exigiam raio200/preferência de armazém próprio. Backup antes-alimentacao-v121-20261009. Formato41 e nomes de slotsv33 mantidos; minimapa v120 e observação sem derrota na missão5 preservados.

Pacote releases/interactions-v121. Atalhos1–6 atualizados; anterioresv120 preservados. Reabrir pela nova versão e carregar slot1 para aplicar normalização. Save analisado: MONDE05.ETA tick68409, espadachins118/122 tinham viagens105,65/98,11 quadrados; testes confirmam seu cancelamento no load.

Verificação no pacote publicado: 29 checks nearest-meal,24 hunger-continues,9 meal-radii,36 missão5 =98 positivos. Cobertura: quatro proprietários, próprio versus chão, aliado versus próprio, hostil negado, fronteira20/21, tarefa/carga, fome/dano, consumo/conservação aliado, save/load determinístico e missão5.21 arquivos de saves com hashes preservados. Build/publish sem erro. Simulações controladas e snapshot real, sem partida manual de balanceamento.

Evidências experiments/alimentacao-v121: slot1.snapshot.json, nearest-published.json, hunger-continues.json, meal-radii.json, mission-five.json e saves-preserved.json. Primeiro ensaio do teste controlado precisou registrar recurso novo no conhecimento privado da IA; não se alterou a regra real para contornar isso.
