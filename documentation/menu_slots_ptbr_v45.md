# v45 — cinco slots, objetivos em português e controles atualizados

Versão: releases/interactions-v45/Jogar-Primeira-Missao.cmd.

## Salvar e carregar

O menu principal abre páginas de Salvar partida ou Carregar partida, com cinco slots e Voltar. Exibe Vazio, data/hora de salvamento ou Arquivo inválido. Um clique em um slot da página Salvar grava naquele slot, substituindo apenas esse arquivo. Carregar restaura o slot escolhido e fecha o menu; não seleciona personagens automaticamente. Slot vazio, inválido ou incompatível mantém a partida atual.

São cinco slots por mapa/missão. Armazenamento estável em `UserSaves` na raiz de RisingLandsResearch, fora das publicações numeradas. Saves atuais continuam v17 e são escritos pela rotina existente de arquivo temporário e troca atômica. O save único de versões anteriores aparece como alternativa do slot 1 quando ainda não há arquivo novo nesse slot; é apenas lido e não alterado. Não foram migrados saves do jogo original.

F5 abre escolha de slot para salvar; F9 abre escolha para carregar. Salvar e sair oferece os cinco slots e fecha somente após salvar com sucesso. Voltar/Esc cancela essa escolha e retorna à confirmação de saída. Sair sem salvar e reiniciar continuam com os comportamentos anteriores.

## Objetivos

Traduzidos os textos das 25 missões canônicas: 39 páginas, mantendo 18 linhas por página, paginação e associações para 34 variantes de mapas. Fonte em inglês preservada em objective_pages.json; tradução do remake em objective_pages_ptbr.json. Narrativa traduzida com frases ajustadas ao espaço do painel; requisitos numéricos, destinos, direções e objetivos preservados. Na primeira missão, o termo cave foi apresentado como armazém, coerente com a construção identificada na lógica original: armazenar 50 de comida e 50 de pedra. Tradução não representa implementação das regras das outras missões.

A tradução usa grafia sem acentos na tela, compatível com os glifos ASCII da fonte original atualmente carregada. Não houve troca da moldura ou criação de arte de menu.

## Controles

Duas páginas com navegação Anterior/Próxima ou setas esquerda/direita e PageUp/PageDown. Incluem seleção por clique/arraste/Shift, cancelamento, coleta em dois cliques com Ctrl mantido, redirecionamento de carga e retirada do armazém, entrada em grupo, retirada por duplo clique, construtores cooperando, B/R e M/E/L, câmera por teclado/bordas, zoom, centralização, slider 1x–5x, parar, barras de vida, grupos, formação G/H, pesquisa T, F5/F9, alerta X, pausa, tela cheia e atalhos de treino/mapa existentes.

## Verificação

Publicação Windows x64 sem avisos. Primeira missão: **537 PASS**; regressão geral: **351 PASS**. Novos testes cobrem cinco estados independentes, carregar em ordem arbitrária, sobrescrever sem alterar outros slots, save corrompido, slot vazio, limites de índice e leitura do legado no slot 1. Os testes usam pastas isoladas em reports/v45-slot-tests, sem escrever nos slots do jogador.

Capturas executadas e inspecionadas: salvar, carregar, controles nas duas páginas, prólogo e objetivos finais da primeira missão. Leitor de objetivos verifica correspondência de todas as variantes, limites de página e retorno ao menu. Não repetida a bateria após a alteração final exclusivamente de disposição das páginas de controles; as duas capturas finais dessa alteração foram verificadas.

## Arquivos e preservação

Mudanças em Engine/SaveSlots.cs, PauseMenu.cs, ResearchGame.cs, ObjectiveReader.cs, projeto de publicação; catálogo objective_pages_ptbr.json; Tests/SaveSlotTests.cs, MissionOneTests.cs e ObjectiveTests.cs. Criados relatórios, testes isolados, capturas e nova publicação. Originais e saves de D:\RISING não alterados; saves antigos do remake não sobrescritos pelos testes.
