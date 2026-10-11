# v73 — aeronaves sobre muralhas

Correção de regra solicitada em 02/10/2026: helicóptero e balão (classes aéreas 16 e 17 do catálogo existente) passam sobre muralhas e portões. Esta regra substitui o bloqueio aéreo explicitamente aplicado nas v71/v72.

`CanTraverse` agora verifica limites do mapa e voo antes do bloqueio terrestre. A navegação aérea usa novamente a rota direta mesmo quando há muralhas. A mesma regra é usada durante o deslocamento, evitando ordens aceitas seguidas de travamento no muro. Aeronave sobre o portão não impede seu fechamento; unidade terrestre ainda impede.

276 verificações passaram, incluindo 23 específicas: travessia real dos dois tipos sobre muro e portão fechado, rota direta, limites do mapa, salvar/carregar em voo e continuação determinística. Também conferimos bloqueio terrestre e passagem pelo portão aberto. Atualizado o teste v71 que esperava a antiga restrição aérea. Sem alteração de formato do save (v31), assets, originais ou saves existentes.

Versão jogável: `releases/interactions-v73`. Evidências: `reports/v73-validation.json` e `logs/v73/`.
