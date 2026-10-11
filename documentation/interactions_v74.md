# v74 — fundo opaco do minimapa

A captura fornecida mostrava objetos do cenário atrás da área explorada do minimapa. `UpdateMiniFog`, em `Engine/MapDiscoveryView.cs`, usava `miniTerrain[n] * .4f`. Essa operação reduz também o canal alfa; a área explorada ficava aproximadamente 40% opaca, misturando o cenário atrás dela.

O escurecimento agora reduz somente RGB e fixa alfa em 255. Áreas desconhecidas continuam pretas, as exploradas continuam escuras e as vistas continuam claras. Mantidos arrasto, informações de construções/unidades, navegação e demais controles. Nenhuma alteração no save v31 nem nos originais.

Verificação: publicação concluída; 46 verificações da suíte de descoberta passaram. Duas capturas da interface (exploração normal e revelação) executaram a verificação de alfa 255 em todos os pixels do minimapa. A imagem de exploração foi inspecionada visualmente.

![Minimapa opaco](../reports/v74-minimap-explored.png)
