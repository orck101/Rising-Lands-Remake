# Construção conjunta e barra original — v32

Data: 2026-09-30. Lançador: `releases/cooperative-ui-v32/Jogar-Primeira-Missao.cmd`.

## Comportamento entregue

Selecionar outro construtor e clicar com o esquerdo na obra em andamento faz esse trabalhador se juntar. O primeiro continua trabalhando. Cada trabalhador reserva um ponto diferente no perímetro e contribui somente após chegar, com seu próprio ciclo de animação, espera e golpe de martelo. Dois trabalhadores presentes fazem o dobro do trabalho em ciclos equivalentes. A aproximação pode ter tempos diferentes, portanto o tempo total desde o clique não necessariamente é metade.

Adicionar construtores ou repetir o clique não cobra a obra novamente. Parar, mover, iniciar coleta/mineração, trocar de obra ou perder um trabalhador remove somente esse trabalhador da equipe. Pausa total mantém o progresso. A conclusão cria um único prédio.

O campo antigo de construtor foi mantido para compatibilidade, com uma lista de ajudantes e seus relógios individuais. Save do remake passou a versão 15, com arquivo separado `research-v15.json`, mantendo leitura dos formatos 4–14. Saves originais não foram editados.

## Interface entregue

A barra inferior usa o fundo original `PLATGAME.PIE` e sprites originais de `OTHERS.CMP`. Santuário: clicar nos retratos de fazendeiro, construtor ou engenheiro inicia a formação correspondente. Quartel: mostra retratos das unidades liberadas pela tecnologia. Construtor: os prédios disponíveis aparecem pelos ícones; clicar escolhe a construção e depois o ponto no terreno verde. Barra possui quatro posições e setas para páginas adicionais.

Removidos da apresentação os painéis genéricos de construção e recrutamento, além do cabeçalho e painel lateral do protótipo. Recursos usam os algarismos e desenhos originais na barra. Dicas curtas continuam no remake. O minimapa auxiliar pode ser alternado com Tab.

**A exigência de apenas menus originais ainda não está concluída por inteiro:** pausa, objetivos, vitória e pesquisa conservam painéis auxiliares do protótipo. Sua reconstrução precisa de evidência adicional. Também não foram reconstruídas todas as ações de todas as estruturas. Essa versão entrega a barra de ação/recrutamento/construção, não a interface inteira fiel.

## Evidência

| Elemento | Fonte / região | Método | Certeza |
|---|---|---|---|
| Barra de quatro posições; retratos em x=33,93,153; y=549 no quadro 800x600 | EXE, `AffZoneIconeAction`, VA 0x417031, offset 0x16431 | emulação isolada interceptando PasteBobCmp em 0x45b1eb | CONFIRMADO para estado controlado |
| Ícone de unidade = 0x29cf + tipo; decodificação devolve 5,7,6 | `IconeGetRecrute`, VA 0x45dac9, offset 0x5cec9 | rotina original executada para os três retratos | CONFIRMADO |
| Ícone de prédio = 0x29b9 + tipo | `IconeGetConstruction`, VA 0x45daf7, offset 0x5cef7; `IconeFillContenu` | leitura do cálculo de índice | FORTE EVIDÊNCIA; parte da associação dinâmica ainda pendente |
| Fundo, retratos, molduras e algarismos | `working/RISING/PLATGAME.PIE`, `GRAPHICS/OTHERS.CMP`; conversões já preservadas | leitura de assets originais, sem redesenho | CONFIRMADO como assets originais |
| Golpe individual contribui +2, quadro 2 e recarga 20 | evidências construction_v29/v30, rotina CalculConstruireBatiment | regras já rastreadas aplicadas a cada trabalhador | CONFIRMADO para ciclo individual; combinação de vários atende requisito do usuário |
| Dobro de contribuição com dois presentes | `Tests/CooperativeConstructionTests.cs` | comparação no mesmo estado e nos mesmos ciclos | CONFIRMADO no remake; algoritmo nativo de várias unidades ainda não emulado |

`tools/trace_native_action_bar.py` é reprodutível e grava `reports/native_action_bar_v32.json`. Ordem dos retratos foi fornecida ao ensaio; posições, desenho e decodificação foram executados pelo original. Adaptar a barra à janela moderna é uma decisão do remake, não uma posição nativa nova.

## Validação

352 verificações focadas e 347 gerais aprovadas, com sobreposição entre baterias. Logs `reports/v32-focused.log` e `reports/v32-regression.log`, erros vazios. Verificações incluem equipe sem substituição, posições distintas, ausência de cobrança adicional, dobro de trabalho, save/load, parada do primeiro com continuação do ajudante, migração v14 e rejeição atômica de equipe duplicada.

Captura `v32-native-recruit.png`: clique injetado pelo mesmo tratamento da barra, comprovando formação do fazendeiro e débito de quatro alimentos. Captura `v32-cooperative-build.png`: segundo construtor selecionado, clique no desenho da obra, chegada dos dois, poses de martelo. O progresso de 50% foi preparado para ilustração dessa captura, não medição de tempo.

`v32-building-2.png` e `v32-construction.png`: barra original inspecionada. Não houve sessão manual completa com mouse/teclado físico. O ensaio inicial de percurso revelou uma condição inválida no próprio teste (trajeto de comprimento zero encerrava no mesmo tick); foi substituído por caminho real alcançável. A rejeição de versão inválida no teste geral foi corrigida para alterar JSON estruturalmente, sem depender do número antigo do save.

## Limites mantidos

Estoques continuam compartilhados por jogador; ocupação individual de armazém não foi reconstruída. Interface de informações detalhadas e árvores de pesquisa completas permanecem pendentes. Posicionamento responsivo da barra, minimapa auxiliar, tooltips e fonte de texto do remake não são integralmente nativos. Barras numéricas mostram até 999; comportamento original além desse intervalo ainda precisa ser conferido. A seleção de construção por desenho utiliza retângulos, inclusive pixels transparentes, e pode precisar de refinamento.

## Como experimentar

1. Clicar no santuário e no retrato do construtor para formar outro.
2. Selecionar um construtor, clicar no ícone do armazém e escolher a grama.
3. Selecionar o segundo construtor e clicar sobre a obra: os dois passam a construir.
4. Parar ou mover somente um: o outro mantém a obra.
5. Salvar/carregar durante a construção para conferir continuidade.
