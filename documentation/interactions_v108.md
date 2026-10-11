# Missão 5 — conclusão v108, 2026-10-04

CONFIRMADO: MONDE05 estava ausente de HasMissionEnding, MissionSatisfied e validação de vitória. Agora participa dos avaliadores vivo e salvo e da transição de campanha.

## Regra aprovada pelo usuário
Clã eliminado: não há personagem real vivo independente e não há construção existente com colono vivo residente. Personagens em minas (estado 10), negociações e Bolsa (12), treinamento válido e pesquisadores continuam contando. Reservas de entidades ainda não nascidas e espelhos temporários não contam. Prédios vazios, recursos, muralhas e obras sem trabalhadores não impedem a vitória. Ordens de produção/construção/pesquisa da IA eliminada ficam inativas; registros históricos são preservados para compatibilidade do save. Cultivo exige operador vivo para gerar colono.

Esta é regra de design aprovada para o remake, não uma equivalência comprovada com a eliminação nativa. Nesta entrega a aplicação ao encerramento e bloqueio de desenvolvimento fica condicionada à missão 5; não altera critérios das outras missões.

## Objetivos
Encontrar três clãs por visão real de personagem ou prédio e eliminar todos os não aliados. Encontro persiste mesmo sem visão atual. Lista de hostis é recalculada: aliança remove e ruptura recoloca. Vitória exige sobrevivência do jogador e 100 avaliações consecutivas (~3,33 segundos em 1x), reiniciadas se requisito falhar. Estado de derrota impede simulação e oferece menu de reinício/carregamento. Menu de objetivos exibe progresso e estados vermelho/amarelo/verde no espaço existente. Transição comum permite pesquisa e inicia MONDE06 sem inimigos.

## Persistência
Formato 35 aceita os anteriores. Nomes dos slots v33 preservados. Saves anteriores não registravam encontros; importação infere encontros pelos terrenos explorados das construções iniciais dos clãs. É aproximação explícita: encontros exclusivamente com unidades distantes que já morreram podem precisar de nova exploração. Não inventa precisão histórica inexistente. Vitória salva sem descoberta dos três é rejeitada.

## Verificações
35 verificações específicas passaram no executável publicado: abertura, alianças/ruptura, descoberta via visão, progresso parcial, três rotas de vitória em fixtures isoladas, mina/Bolsa, residentes vivos/mortos, laboratório, treinamento, prédios vazios, derrota/reinício, importação antiga, rejeição de vitória inválida, save/load e passagem 5→6. 40 verificações da missão 6 passaram. Build/publicação sem erros ou avisos.

Ensaios do motor com estados controlados, não campanhas manuais completas nem aprovação visual da interface. O problema separado de tropas não atingirem algumas obras não foi corrigido; obras vazias deixam de bloquear o encerramento. Demais achados da auditoria (fome militar, áudio e economia amarela) não integram esta entrega.

## Arquivos
Engine/MissionFive.cs; MissionOne.cs; MissionSix.cs; MapDiscovery.cs; Simulation.cs; OpponentSimulation.cs; EnemyCampaignResearch.cs; RecruitmentSimulation.cs; ConstructionSimulation.cs; ObjectiveMenu.cs; Tests/MissionFiveEndingTests.cs; Program.cs.

Pacote: D:\CHATGPT\outputs\RisingLandsResearch\releases\interactions-v108
Atalho: D:\CHATGPT\Jogar-Quinta-Missao.cmd
Evidências: D:\CHATGPT\outputs\RisingLandsResearch\experiments\mission5-ending-v108
