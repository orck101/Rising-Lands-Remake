# Raios de alimentação — v111 — 2026-10-04

Regra solicitada pelo usuário, comum às missões e personagens sujeitos à fome: comida solta no chão até 20 quadrados; armazém próprio ou de clã aliado até 25 quadrados. Distância circular no mapa desde o ponto onde a busca começou; para armazéns considera o ponto de interação acessível. Comida no chão permanece prioritária. Caminhos até duas vezes o raio permitem aproximação e desvios locais sem excursões ilimitadas (40 para chão, 50 para armazém). Fonte inacessível, vazia, prédio sem operador ou hostil são rejeitados. Sem alimento acessível, mantém fome e dano existentes. Carga/tarefa e retorno após alimentação preservados.

Armazéns aliados são acessíveis conforme alianças reais da campanha; não basta terem outra cor. Consome o estoque do proprietário, contabiliza a refeição no personagem e registra saldo de alimento compartilhado para conservar recursos no save/load. Ruptura da aliança invalida a alimentação naquele armazém. Não concede alimento ou remove custo.

Busca emergencial de colheita de fazendeiros inimigos também foi limitada a fontes num raio de 20 do próprio personagem. Ordens normais de coleta não tiveram seu alcance alterado.

Formato 37 aceita saves anteriores até 36; arrays de saldo compartilhado novos iniciam zerados para os antigos. Slots continuam research-v33. Não regrava os saves do usuário em testes.

Verificações: nove checks específicos (limites 20/21 e 25/26, consumo aliado, conservação, persistência, continuidade e ruptura), onze de fome local (incluindo dano sem alimento), 35 de conclusão da missão 5 e cinco slots copiados carregados/continuados deterministicamente passaram. A abertura econômica foi retestada separadamente. 21 arquivos de saves preservados por SHA256. Publicação Windows passou. São simulações do motor; não teste humano pela interface nem aprovação global do balanceamento inimigo.

Arquivos: Engine/HungerSimulation.cs, FoodSimulation.cs, Simulation.cs, OpponentDevelopment.cs; Tests/MealRadiusTests.cs, LocalHungerDevelopmentTests.cs; Program.cs. Evidências: experiments/raios-fome-v111. Backup: backups/antes-raios-fome-v111-20261004 e pacote v110 preservado.

Entrega: releases/interactions-v111. Atalho: D:\CHATGPT\Jogar-Quinta-Missao.cmd.
Abertura econômica: 47 checks passaram no pacote final.
