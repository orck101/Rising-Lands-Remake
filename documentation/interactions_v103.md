# v103 — cliques da pesquisa extra

## Problema confirmado

A barra inferior de comandos recebia entrada antes da tela de pesquisa que a cobre. `UpdateNativeActions` consome qualquer clique novo abaixo de `NativeTop`, mesmo sem uma ação naquele ponto. Parte dos ícones de escolha da pesquisa extra aparece nessa região; o clique era consumido e não chegava à seleção do ramo. Em janelas mais largas a sobreposição aumenta.

Reprodução com uma cópia do slot 4, MONDE04: clicar dentro da parte inferior do ícone de Agricultura, em 1280×800, não selecionava o ramo. O teste falhou antes da correção com `Visible Agriculture icon click intercepted: extra research branch was not selected`. O andamento da simulação não estava bloqueado neste save: um pesquisador, duas descobertas obrigatórias, 164 alimentos, sem pausa manual; Agricultura, Armamento e Engenharia estavam disponíveis. Espiritualidade estava bloqueada até a missão 7, conforme o catálogo.

## Correção

A tela de pesquisa agora tem prioridade exclusiva sobre a barra de comandos enquanto estiver aberta. A mesma rotina de entrada é utilizada pelo jogo e pela verificação. Custos, tempos, requisitos, pausa explícita, tecnologias e balanceamento da IA não foram alterados.

Arquivos: `Engine/ResearchMenu.cs`, `Engine/ResearchGame.cs`, `Engine/ExtraResearchInputCapture.cs`, `Engine/ResearchV58Capture.cs`. Diagnóstico da simulação: `Tests/ExtraResearchProbe.cs` e opção `--probe-extra-research --save <snapshot>`.

## Verificações

- Falha reproduzida antes e aprovação depois usando o mesmo clique visível.
- Capturas e teste de entrada em 1280×800 e 1600×800: seleção, cancelamento sem gasto, confirmação com cobrança única de 50, rejeição de cobrança repetida, avanço e restauração de compra/progresso.
- Suíte `--test-research-v58`: aprovada; inclui tempos de 7/4 minutos, pausa, ausência de colonos, cotas, compra, benefícios e save/load.
- Compilação e publicação Windows em pasta nova, sem sobrescrever a versão em uso.

Snapshot: `experiments/extra-research/slot4.json`. SHA-256 do snapshot e do slot 4 original após os testes: `8BC559E0CA083AAC6A330C2CF31766E3B91CEBAAE5E270F3403940FF887D0BA9`. Nenhuma partida do usuário foi sobrescrita.

## Entrega

Pacote: `releases/interactions-v103`. Os atalhos de missões e o monitor de relação militar v1 da entrega anterior foram preservados. Abra `Jogar-Quarta-Missao.cmd` e carregue o slot 4 pelo menu, ou use o executável e carregue sua partida. Não é necessário reiniciar a campanha.

Limite da conclusão: o defeito de entrada foi reproduzido e corrigido; não houve observação direta dos cliques anteriores do usuário. Pausa explícita, laboratório vazio, falta de alimento ou ramo ainda indisponível continuam sendo restrições válidas e aparecem no estado da pesquisa.
