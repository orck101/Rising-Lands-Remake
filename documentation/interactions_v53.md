# v53 — classes do grupo na barra de seleção

Abrir `releases/interactions-v53/Jogar-Primeira-Missao.cmd`.

Ao selecionar vários personagens visíveis, a parte esquerda da barra inferior mostra um retrato original por classe presente. Classes iguais não repetem o retrato. Inclui trabalhadores, colonos e soldados; a ordem é a dos tipos do catálogo. Passar o mouse informa a classe, a quantidade selecionada e a ação de filtrar.

Clicar num retrato mantém somente os personagens daquela classe que já pertenciam ao grupo. Não inclui outros personagens no mapa, não para unidades nem modifica carga ou ordens. A unidade principal é atualizada para uma das restantes. Escolhas de ordens ainda não confirmadas e a prévia de construção são canceladas ao filtrar; as ordens já dadas na simulação permanecem intactas.

Até oito classes ocupam uma linha; acima disso, a lista usa duas linhas, mantendo todas as classes visíveis sem paginação. Retratos são os sprites existentes `0x29cf + tipo`, com proporções preservadas, e moldura original `0x29b7`. Recursos e velocidade continuam visíveis. Quando sobra um único personagem, a barra volta à apresentação individual e às ações existentes. Construções continuam com suas próprias opções.

**REQUISITO DO JOGADOR:** esta organização por classes e o clique para filtrar são as alterações solicitadas. A composição da barra é adaptada; os retratos e moldura são originais.

## Verificação

- 780 verificações da missão/interações passaram, incluindo seis verificações de deduplicação, filtro limitado ao grupo, carga/ordens intactas, classe ausente e unidade derrotada.
- Capturas `reports/v53-classes-v53.png` e `reports/v53-classes-v53-filtered.png` conferidas antes e depois de um clique pelo mesmo manipulador usado no jogo. Comparação do estado serializado confirma que filtrar não altera a simulação.
- Geometria da lista verificada para 1–26 classes: sem sobreposição de retratos e sem invadir contadores de recursos. Captura adicional `reports/v53-classes-layout-verified.png`.
- Tipos das seis unidades são preparados em memória somente para a captura. Não representam formação paga ou uma partida salva.
- Log: `logs/v53-mission.log`.

Save permanece v20; nenhuma mudança nos arquivos ou saves originais. Correções de alcance e capacidade das versões anteriores preservadas. As pendências da campanha permanecem registradas anteriormente.
