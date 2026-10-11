# Rising Lands — investigação da mecânica original de comércio

Data: 02/10/2026. Escopo: investigar o original e comparar com o remake v70. Nenhuma regra do remake foi alterada nesta investigação.

## Atualização após o relato do jogador e leitura do manual

O manual fornecido pelo jogador foi lido em 02/10/2026. As folhas 19 e 20 do PDF foram renderizadas e conferidas visualmente: correspondem às páginas impressas 34–37. A folha 24, páginas impressas 44–45, também foi conferida. A camada de texto contém erros de reconhecimento; as conclusões abaixo foram comparadas com a imagem das páginas.

Esta seção atualiza os pontos anteriormente classificados apenas por análise do executável. **CONFIRMADO NO MANUAL** significa documentado na edição inglesa, não que o comportamento já esteja implementado ou testado no remake. **RELATO DO JOGADOR** identifica sua observação prática, sem transformá-la em ensaio nosso.

### Regras agora esclarecidas

| Ponto | Funcionamento | Fonte e certeza |
|---|---|---|
| Mensageiro sob ataque | Não é alvo de ataques automáticos; continua podendo ser atacado por ordem explícita com Alt. Não é invulnerável. | **CONFIRMADO NO MANUAL**, p.34 e p.36. O relato de que as tropas não o atacam corresponde à proteção automática. |
| Diplomacia | Entrar no Santuário da outra tribo inicia a negociação de aliança. A boa vontade influencia seu sucesso. | **CONFIRMADO NO MANUAL**, p.36; também **RELATO DO JOGADOR**. |
| Comércio | Entrar no Mercado estrangeiro disponibiliza uma cartola acima da interface inferior. Clicar na cartola abre a janela de troca. | **CONFIRMADO NO MANUAL**, p.34. A chegada não deve virar apenas um contato genérico permanente. |
| Edifícios necessários | O manual exige mensageiro, Mercado e Bolsa de Trocas; informa que a outra tribo também necessita da infraestrutura para negociar. | **CONFIRMADO NO MANUAL**, p.34 e p.45; consistente com a verificação das Bolsas dos dois lados no executável. Não se deve presumir que o jogo crie esses edifícios invisivelmente. |
| Recursos | O Mercado tem ligação direta com os recursos do armazém. A aceitação debita e credita os números sem viagem de um carro. | Ligação direta **CONFIRMADA NO MANUAL**, p.45; transferência numérica corroborada pelos ensaios nativos; ausência de transporte de recursos e momento da aceitação também **RELATO DO JOGADOR**. |
| Tropas e veículos | A Bolsa recebe as unidades que serão oferecidas. Após a oferta ser aceita, há prazo para entregar as unidades exigidas; os ícones pendentes desaparecem conforme elas entram. Quando a entrega está completa, a troca é processada e as unidades recebidas emergem da Bolsa. | **CONFIRMADO NO MANUAL**, p.35; também **RELATO DO JOGADOR**. Não confundir esse procedimento com entrega física de comida/pedra. |
| Oferta enviada pelo jogador | Seleciona o que deseja receber e o que oferece. O adversário avalia e pode recusar. A interface mostra sua satisfação durante o prazo de negociação. | **CONFIRMADO NO MANUAL**, p.35; também **RELATO DO JOGADOR**. |
| Oferta enviada pela IA | A parte inferior mostra o pedido da IA, que não pode ser alterado. A parte superior contém bens da IA que o jogador deseja receber. O jogador pode aceitar ou recusar, como relatado. | Orientação dos painéis **CONFIRMADA NO MANUAL**, p.35; aceitar/recusar descrito também no **RELATO DO JOGADOR**. Isso exige sessões recebidas, ausentes no remake atual. |
| Satisfação e preço | Boa vontade afeta a qualidade das ofertas e o sucesso de alianças. As próprias ofertas também influenciam a boa vontade. | **CONFIRMADO NO MANUAL**, p.35–36; a influência numérica de simpatia/aliança já foi confirmada nos ensaios. Os exemplos 50 por 100/200 do relato são ilustrativos, não uma tabela fixa universal. |
| Cancelamento e prazo | Oferta recusada ou negociação expirada encerra a janela. Não preencher a Bolsa dentro do prazo cancela a transação e desagrada a outra tribo. | **CONFIRMADO NO MANUAL**, p.35. A duração exata ainda precisa ser recuperada do executável. |
| Aliança recebida | O mensageiro inimigo entra no Santuário; aparece bandeira piscando por tempo limitado. Clicar aceita; deixar passar recusa. | **CONFIRMADO NO MANUAL**, p.36. |
| Fim da missão diplomática | O manual informa que o mensageiro desaparece depois de cumprir sua missão. Alianças e boa vontade da IA passam de uma missão à seguinte. | **CONFIRMADO NO MANUAL**, p.36. O instante exato de desaparecimento em cada tipo de sessão ainda deve ser rastreado. |

O manual p.45 acrescenta que Santuário e Mercado podem receber mensageiros como alternativa quando falta o Mercado. Isso não comprova que toda troca comercial esteja disponível sem Mercado/Bolsa: o mesmo trecho e a p.34 exigem esses edifícios para transações. A regra exata dessa alternativa permanece pendente de análise do código; não deve ser implementada como exceção ampla por interpretação.

O manual p.35 também avisa que o conteúdo transportado por um veículo não entra no valor da negociação e acompanha o veículo entregue. Oferecer um veículo carregado exige preservar sua carga, sem contabilizá-la como um segundo pagamento.

### Consequências para a implementação

1. Separar sessão diplomática no Santuário e sessão comercial no Mercado, com entrada real do mensageiro, indicador original e término da missão.
2. Proteger mensageiros da escolha automática de alvo, preservando a possibilidade de ataque explícito.
3. Criar estados de preparação, avaliação, aceitação/recusa, prazo e cancelamento, incluindo propostas recebidas da IA. Não executar toda troca no primeiro clique do jogador.
4. Na troca somente de recursos, atualizar os estoques após a aceitação, sem criar rota de carro. Conservar os saldos e esclarecer os campos RMI/estoque antes de integrar ao controlador.
5. Na troca de personagens, exigir o conteúdo necessário na Bolsa, mostrar pendências pelos ícones originais e entregar as unidades recebidas por ela após completar a transação.
6. Usar relações reais e persistir boa vontade/alianças entre missões. Recuperar os tempos, condições de recusa e efeitos exatos no executável, sem inventar uma tabela de preços.

O relato do jogador e o manual tornam o objetivo de reprodução muito mais preciso. Esta atualização é documental: **a v70 ainda não recebeu essas mecânicas**.

### Fontes desta atualização

- Manual preservado em [Rising-Lands_Manual_Win_EN.pdf](Rising-Lands_Manual_Win_EN.pdf), páginas impressas 34–36 e 45; folhas PDF 19, 20 e 24.
- Capturas: `../reports/manual-commerce-19.png`, `manual-commerce-20.png`, `manual-market-24.png`.
- Relato do jogador nesta conversa em 02/10/2026, distinguido acima das verificações independentes.
- Executável e ensaios documentados nas seções seguintes.

## Resultado principal

**O comércio atual está incompleto.** O original relaciona mensageiros, Mercado, Bolsa de Trocas, materiais, personagens e diplomacia. O remake conserva parte da interface e do cálculo de materiais, mas substitui esse fluxo por contato simplificado e uma transferência imediata de três recursos.

A diferença mais concreta encontrada é a **verificação de Bolsa de Trocas dos dois participantes antes de abrir a negociação pelo mensageiro**. Essa condição não existe no controlador atual. Também faltam a negociação de personagens e o efeito real das relações diplomáticas sobre a avaliação.

É necessário corrigir uma interpretação anterior: o executável original contém uma interface de oferta e pedido (`AffMarcheProposition`). Isso não comprova que existisse um botão escrito “Proposta de troca”. Remover a tela genérica moderna foi adequado ao pedido do jogador; eliminar a própria negociação de oferta/pedido não seria uma reprodução comprovada do original.

## Método e limites

Fonte primária: cópia de `working/RISING/RISING-ORIG.EXE`, SHA-256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. Os nomes das funções vêm dos símbolos recuperados do jogo. Os endereços abaixo são endereços virtuais do PE, não posições de arquivo; a transcrição técnica inclui ambos.

Foram examinadas as instruções e executadas rotinas originais isoladamente em um emulador, com memória sintética. Os testes não iniciam o processo do jogo nem leem/escrevem partidas do usuário. Tipo de jogador, simpatia e aliança foram controlados nos ensaios; as limitações constam no resultado de cada teste.

Classificação: **CONFIRMADO** para instruções/dados ou comportamento executado no ensaio indicado; **FORTE EVIDÊNCIA** para fluxo identificado estaticamente; **PROVÁVEL** para interpretação consistente ainda sem reprodução completa; **HIPÓTESE** para proposta não demonstrada; **DESCONHECIDO** para questões abertas. Confirmar uma rotina isolada não confirma toda a experiência de jogo.

## 1. Edifícios e personagens envolvidos

| Elemento | Evidência original | Conclusão e certeza |
|---|---|---|
| Mercado | Tipo 16, nome interno `MARCHE`, nome inglês `Market`; configuração original `LOGEMENT=2` | **CONFIRMADO:** é um edifício distinto da Bolsa de Trocas. A configuração indica alojamento 2; todas as restrições de operação ainda precisam de verificação contextual. |
| Bolsa de Trocas | Tipo 8, nome interno `HANGAR`, nome inglês `Exchange`; `LOGEMENT=8` | **CONFIRMADO:** não é o Mercado. “Hangar” é o nome interno, não uma prova de transporte aéreo obrigatório. |
| Mensageiro | Tipo 10, `MESSAGER`; teste explícito `cmp eax,0x0a` em `BatimentLogeEngin`, 0x41f842 | **FORTE EVIDÊNCIA:** o mensageiro tem um tratamento especial ao interagir com edifícios de outra tribo. |
| Bolsas dos dois jogadores | Chamadas a `RechercheHangar` em 0x41f8e5 e 0x41f906; retorno 0xffff impede continuação | **FORTE EVIDÊNCIA:** esse caminho de início da negociação exige uma Bolsa registrada para cada participante. |
| Estado da negociação | Verificações da tabela 0x4e2e68 em 0x41f8af e 0x41f8cc | **FORTE EVIDÊNCIA:** existem restrições de estado; não basta manter um contato booleano com um prédio. |

`RechercheHangar` (0x46107a) procura entre dez referências de edifícios por jogador, na tabela 0x4e2958. `SetTableauHangar` (0x461d02) registra referências nessa tabela. Isso é infraestrutura de Bolsa e conteúdo negociável, não o inventário normal de um armazém.

## 2. Fluxo de negociação recuperado

1. **O mensageiro chega ao edifício de outra tribo.** `BatimentLogeEngin` diferencia o tipo do personagem, o tipo do edifício e os proprietários. Para o caminho de Mercado, verifica os estados e as Bolsas dos dois participantes. **FORTE EVIDÊNCIA**, região 0x41f833–0x41f92e.
2. **O jogo prepara a negociação entre dois jogadores.** `InitTableauMarche` (0x4631ab) inicializa listas de materiais/personagens, associa os participantes em 0x496f3c e define estados da sessão. **FORTE EVIDÊNCIA**.
3. **O painel apresenta o que é oferecido e o que é pedido.** Existem tabelas separadas de materiais e de personagens. O desenho original usa `AffMarcheProposition` (0x465a20), fundo 0x2af8, além de rotinas de recursos, personagens, contato e aliança. **CONFIRMADO** quanto à existência das tabelas, funções e fundo; a reprodução anterior de imagem cobriu apenas a categoria de materiais.
4. **A oferta é avaliada.** `EvalueCoutProposition` (0x466a40) combina os valores de materiais e personagens e aplica simpatia e aliança. **CONFIRMADO** por execução isolada dos casos de materiais.
5. **Há preparação e aceitação.** `PreparationEchange` compara a avaliação com 100 em 0x466eda. `MarcheOrdreEchange` (0x461a7e) alterna a confirmação de um participante; no ensaio com jogadores de tipo 1, a primeira confirmação não executou a troca, e a segunda chamou as rotinas de transferência. Para tipo 2, existe um caminho distinto. **CONFIRMADO no ensaio de controle**, com as transferências interceptadas somente nesse teste.
6. **A troca aplica materiais e/ou personagens.** As rotinas `MarcheEchangePrend` (0x4614b5) e `MarcheEchangeDonne` (0x46172e) percorrem os dois conjuntos separadamente. **CONFIRMADO para materiais no ensaio isolado; FORTE EVIDÊNCIA para personagens**.

Esse fluxo é uma reconstrução do código. A sequência exata de cliques, rótulos em português, comportamento do mensageiro durante a tela e cancelamento completo ainda não foram filmados ou reproduzidos em uma partida original nesta investigação.

## 3. Valores e influência da diplomacia

Os bytes reais em 0x496f7c são `[2,1,1]`, na ordem interna **engrenagens, comida, pedra**. Portanto, para a avaliação da oferta, uma engrenagem vale duas unidades-base; comida e pedra valem uma cada. **CONFIRMADO** por leitura do PE, sem substituir essa tabela no teste novo.

No ramo de jogador tipo 1, sem personagens, o cálculo recuperado é:

```text
valor oferecido = 2 × engrenagens + comida + pedra
valor pedido    = 2 × engrenagens + comida + pedra
fator           = 80 + simpatia + 40 × indicador de aliança
oferta ajustada = truncar(valor oferecido × fator / 100)
avaliação       = truncar(oferta ajustada × 100 / valor pedido)
```

A implementação usa divisão inteira com sinal; os exemplos positivos abaixo coincidem com o truncamento usual. O ramo de tipo 2 usa outra direção de avaliação, também registrada nos testes. Os nomes humanos/IA são a interpretação do papel desses ramos; os ensaios controlam diretamente os valores 1 e 2.

| Oferta → pedido | Simpatia | Aliança | Avaliação, ramo tipo 1 |
|---|---:|---:|---:|
| 10 comida → 8 pedra | 0 | Não | 100 |
| 10 comida → 10 pedra | 0 | Não | 80 |
| 10 comida → 10 pedra | 0 | Sim | 120 |
| 10 comida → 10 pedra | +20 | Não | 100 |
| 10 comida → 10 pedra | −20 | Não | 60 |

Assim, **80% não é uma taxa fixa universal de comércio**: é o caso neutro desse ramo. O remake utiliza o caso neutro para todas as transações, embora sua função aceite argumentos de simpatia e relação. O controlador não fornece as relações reais.

O executável também possui `MarcheChangePlayerContact` (0x463674), que consulta e altera aliança por `PlayerEstAllie`, `PlayerSetAllie` e `PlayerResetAllie`. **FORTE EVIDÊNCIA:** o painel integra diplomacia. Custos, recusas e efeitos de campanha desse comando permanecem **DESCONHECIDOS** nesta auditoria.

## 4. Troca de personagens e papel da Bolsa

Há 26 posições nas listas de personagens de oferta/pedido: bases 0x4e29a8 e 0x4e2a78, com 52 bytes por participante. Existe uma tabela separada de valores em 0x496f60. **CONFIRMADO**. São valores de avaliação comercial, não necessariamente custos para recrutar essas unidades.

`EnginEchangeable` (0x440f1c), executada para os tipos 0–25, aceita **0–15 e 25** e rejeita **16–24**. Isso inclui trabalhadores, colono, mensageiro, várias tropas e veículos terrestres; Stomper tipo 25 é aceito. Não se deve oferecer indistintamente toda classe no comércio.

As rotinas de transferência procuram personagens nas Bolsas, consultam seu conteúdo e usam `RechercheTroupeHangar` e `BatimentRetireType`. `MarcheOrdreSortie` (0x461b79) procura personagens guardados e chama `EnginSetPlayer` (0x4431ac). **FORTE EVIDÊNCIA:** conteúdo das Bolsas e mudanças de proprietário fazem parte da mecânica. O percurso visual exato de entrada/saída e o transporte completo de unidades não foram executados aqui.

**Não foi comprovado que a troca de materiais precise de um carro levando carga de um Mercado ao outro.** O teste executou a transferência de materiais sem simular deslocamento. Isso confirma a rotina de débito/crédito, mas não exclui etapas anteriores ou posteriores do fluxo completo. Também não permite usar “hangar” como justificativa para inventar voo ou transporte de materiais.

## 5. Débito, crédito e contabilidade

No ensaio nativo, os jogadores 0 e 1 começaram com 200 de cada recurso. Os vetores do participante 1 ofereceram 10 comida e pediram 8 pedra. `Prend(0,1)` e `Donne(0,1)` produziram:

| Jogador | Comida antes → depois | Pedra antes → depois | Engrenagens |
|---|---:|---:|---:|
| 0 | 200 → 210 | 200 → 192 | 200 |
| 1 | 200 → 190 | 200 → 208 | 200 |

**CONFIRMADO:** as quantidades totais foram conservadas, e os vetores consumidos foram zerados. A rotina real `BatimentEnleveMatiere` e seus débitos foram executados, não substituídos por uma implementação moderna.

O original possui mais de um contador: `GetRessource` (0x467504) retorna os totais, `GetRessourceRMI` (0x4674d4) retorna campos separados, e `GetRessourceStock` (0x467531) calcula diferenças. Os créditos da troca aumentam dois campos simultaneamente. No estado sintético, a comida recebida aumentou o total do jogador 0, mas não a diferença retornada por `GetRessourceStock`.

**CONFIRMADO para os campos e getters; DESCONHECIDO para toda a consequência visual e logística desse estado numa partida real.** Não se deve chamar o primeiro campo de “inventário do Mercado”, “reserva de negociação” ou “carga em trânsito” sem provas adicionais. A documentação anterior usou “reserva/total”; essa expressão não basta para explicar o significado completo do campo RMI. O remake só tem uma atualização direta dos estoques gerais neste controlador.

## 6. Comparação com o remake v70

| Ponto | Original recuperado | Remake atual | Prioridade |
|---|---|---|---|
| Início | Caminho do mensageiro ao Mercado valida Bolsas dos dois lados e estado de sessão | Chegar perto de Mercado estrangeiro habitado registra contato | Alta |
| Bolsa de Trocas | Estrutura própria, registro e conteúdo de personagens | Não participa de `CanTrade` | Alta |
| Avaliação | Materiais + personagens + simpatia + aliança; ramos distintos | Apenas materiais, sempre relação neutra | Alta |
| Confirmação | Estado dos participantes e caminho específico para tipo 2 | Uma ação executa toda a transferência | Alta |
| Personagens | Classes permitidas, conteúdo das Bolsas, mudança de proprietário | Ausente | Alta |
| Contabilidade | Totais, RMI e estoque calculado separadamente | Débito/crédito em `World.Resources` | Alta, exige investigação do campo RMI |
| Interface | Recursos, personagens, contato, aliança e estados | Reprodução parcial da categoria de materiais com controles adaptados | Média |
| Encerramento | Funções de saída, limpeza e restauração de sessão | Fecha painel ou cancela edição de quantidade | Média |

Há ainda problemas concretos no código moderno: o contato continua válido se o mensageiro caminhar para longe, desde que permaneça vivo e da classe correta; `MarketPartners(market)` não usa o argumento `market` para vincular o contato ao Mercado próprio; `CanTrade` não verifica capacidade física dos armazéns, apenas saldo e limite numérico de 65535. **CONFIRMADO por leitura do controlador atual.** Essas observações não estabelecem, por si, as regras originais de distância ou capacidade que deverão substituí-las.

## 7. Ordem proposta para a reconstrução

1. Recuperar a sessão original: entrada do mensageiro, condições das Bolsas, participantes, estados, confirmação, recusa e cancelamento. Fazer um cenário reproduzível no original antes de substituir o controlador moderno.
2. Esclarecer os campos RMI/total/estoque durante oferta, aceitação e saída, evitando duplicar ou tornar indisponíveis os recursos recebidos.
3. Restaurar o painel completo e seus comandos de recursos, personagens e diplomacia, com assets originais.
4. Implementar troca de personagens usando somente classes aceitas, conteúdo real das Bolsas e mudança de proprietário; depois conferir entrada/saída e transporte visual.
5. Aplicar simpatia e aliança reais à avaliação e persistir a sessão corretamente no save moderno.
6. Validar falta de Bolsa em cada lado, Bolsa vazia/cheia, mensageiro morto, negociação simultânea, recusa, cancelamento, save/load e ausência de criação/perda de materiais ou personagens.

Após a leitura do manual, a sequência de entrada/cartola e a orientação de propostas da IA estão documentadas acima. Permanecem **DESCONHECIDOS**: algoritmo completo de propostas e recusas da IA, tempos absolutos, regra integral de capacidade, instante exato de término de cada sessão e significado operacional do RMI na troca. Não foram preenchidos com hipóteses apresentadas como fatos.

## Evidências e reprodução

- `../tools/audit_commerce_original.py`: ensaios reproduzíveis e extração da transcrição.
- `../reports/commerce_original_audit.json`: **36 casos de avaliação, 26 classes, transferência bilateral nativa e controle de confirmação**, todos aprovados.
- `../reverse_engineering/commerce_original_evidence.txt`: instruções, endereços virtuais e offsets de arquivo das funções utilizadas. Desmontagem linear pode atravessar tabelas internas de dados; os fluxos executados foram verificados separadamente.
- `../source_remake/Data/buildings.json` e `units.json`: nomes, IDs e referências da configuração original.
- `../source_remake/RisingLandsRemake/Engine/MarketSimulation.cs` e `NativeMarket.cs`: implementação moderna comparada.
- `interactions_v67.md`, `../reverse_engineering/market_values_v67.json` e `../source_remake/Data/market_visuals_v67.json`: investigação anterior, com escopo parcial explicitado.

Na investigação inicial, a tentativa de obter o manual pela internet falhou. Posteriormente o jogador forneceu o arquivo local, que foi lido e conferido visualmente. A seção de atualização registra as regras confirmadas nessa fonte primária; a análise numérica continua baseada no executável e nos ensaios. Resenhas não foram usadas como prova de regras técnicas.

Os originais e saves permaneceram intactos. Foram criados somente ferramenta de pesquisa, transcrições, resultados e este relatório na área separada. O executável jogável continua sendo a v70.


## Implementação posterior — v71

A investigação acima foi concluída sobre a v70 e permanece como registro dessa análise. A implementação posterior está em [interactions_v71.md](interactions_v71.md): cartola, chegada, estados da negociação, materiais diretos, troca de personagens pela Bolsa, ofertas recebidas e diplomacia. A IA que compõe ofertas/decide alianças e a conversão dos prazos ainda não estão totalmente reconstruídas; esses limites não foram convertidos em fatos.
