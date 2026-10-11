# v59 - barra de pesquisa na moldura original

Solicitacao: usar a barra horizontal original no rodape, retirando o progresso da posicao anterior.

Alteracao restrita a Engine/ResearchMenu.cs: preenchimento no canal nativo x=36, y=557, largura=289, altura=7 (coordenadas do menu 800x600), com bordas e extremidades do asset Content/original-research.png intactas. Estado em y=572; mensagem deslocada para y=534 e limitada a largura disponivel. Removida a barra x=28, y=428. Sem mudanca de regras, tempos ou formato de save.

Publicacao concluida sem erros. Capturas do renderer real com cliques e cancelamento/confirmacao verificados: reports/v59-research-active.png (50%) e reports/v59-research-complete.png (100%). Ambas inspecionadas visualmente: preenchimento dentro da moldura original, nenhuma barra no local antigo e mensagens sem sobrepor a barra. Logs em logs/v59-capture-active.log e v59-capture-complete.log. Originais nao alterados.
