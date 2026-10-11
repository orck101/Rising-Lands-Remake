# Controles: primeira correção de seleção — v8

Release: `releases/controls-v8/RisingLandsRemake.exe`, 27/09/2026. Esta etapa corrige seleção, mas não conclui a reconstrução dos comandos originais.

## Alterações jogáveis

- Arrastar seleciona pela área de células da unidade, em vez de incluir qualquer desenho cujo retângulo toque a caixa. Os limites da área são inclusivos, como na rotina nativa.
- Seleção em grupo limitada a 255 unidades.
- Shift adiciona unidades próprias sem duplicatas. Um inimigo inspecionado não fica misturado ao grupo comandado. Clicar sozinho num inimigo continua mostrando suas informações.
- Ao perder o foco, a janela cancela o arraste pendente, não recebe comandos e suspende a atualização da simulação. A janela volta ao funcionamento normal quando recupera foco. Esta é uma decisão moderna de usabilidade, não uma descoberta sobre o original.
- O rodapé informa a seleção adicional com Shift.

Combate, economia, construção e formato de save permanecem v7. A nova pasta tem seu próprio `PrototypeSaves`; saves de outra release não são localizados automaticamente. O carregador aceita os formatos v4–v7 quando colocados nessa pasta. Não manipula saves legados.

## Evidência original

Fonte: cópia `working/RISING/RISING-ORIG.EXE`, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`.

**CONFIRMADO:** `SelectionEngin` 0x4741cc converte dois índices de célula em limites, normaliza sua ordem e chama `EnginInZone` 0x4461f4 em 0x4742c6. Não usa o retângulo dos pixels do sprite para essa consulta. A rotina de zona consulta o campo +8 do registro. A segunda célula +124 foi deliberadamente deslocada durante os testes e não afetou o resultado.

**CONFIRMADO nos vetores:** `tools/trace_selection_zone.py` executa `EnginInZone` em memória isolada, sem iniciar processo legado. São 26 tipos, cada um com 49 consultas de célula e seis retângulos, totalizando 1.430 casos. A área de cada tipo é exportada em `source_remake/Data/selection_rules.json`, com todos os resultados e hash da fonte. Saída resumida: `reports/selection_zone_original_validation.json`.

**CONFIRMADO por inspeção estática:** `EnginAddSelection` 0x4740bd verifica o dono em 0x47413c–0x474143, ignora unidade já selecionada em 0x474160–0x474167 e impede inclusão quando a contagem alcança 255 em 0x474172–0x47417f. A v8 aplica esses critérios ao acréscimo de unidades no grupo. Isso não comprova que Shift seja a tecla física usada pelo original: a ligação de teclado ainda não foi reconstruída.

**APROXIMAÇÃO explícita:** o remake usa a célula de sua posição simulada para substituir o campo nativo +8. A atualização desse campo durante deslocamento ainda exige pesquisa. O filtro de visibilidade continua sendo o do protótipo, não a máquina completa de seleção original. Clique simples ainda utiliza os limites gráficos para inspeção.

## Verificação

`logs/selftest_controls_v8.log`: 201 verificações aprovadas no executável publicado. Os novos testes confrontam todos os vetores nativos, arraste invertido, propriedade do grupo, duplicatas, inspeção isolada, seleção retangular, teto de unidades e exclusão de mortos. `reports/prototype_controls_v8.png` foi inspecionado para apresentação e inicialização do cenário; a captura não equivale a teste manual de todos os gestos.

## Ainda provisório

Botões de ação, menus de construção e treinamento, seleção de prédios, grupos memorizados, duplo clique, combinações de teclas e atalhos continuam pendentes de reconstrução. F2/F3 continuam sendo comandos de laboratório. B/C/G/H/X/R não devem ser apresentados como atalhos fiéis ao original. O mapeamento físico dos botões do mouse ainda precisa ser rastreado; não foi invertido por suposição.

Próxima etapa: seguir a geração das ordens que chegam ao despachante em 0x45ed21 (seleção por área) e 0x45ed3d (acréscimo individual), ligando os eventos físicos às ações antes de remapear os controles.
