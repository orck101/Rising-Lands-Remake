# Terreno

CONFIRMADO por testes diferenciais:16 predicados,65536 entradas cada, zero divergências. Faixas abaixo usam limite final exclusivo; categorias podem se sobrepor.

| Predicado | VA | IDs [início,fim) |
|---|---|---|
| TerrainEstMarecage | 0x476239 | [] |
| TerrainEstSable | 0x47623c | [(285, 349)] |
| TerrainEstFrontiereHerbe | 0x47626b | [(16, 31)] |
| TerrainEstHerbe | 0x476296 | [(349, 421)] |
| TerrainEstFrontiereCailloux | 0x4762ec | [(46, 61)] |
| TerrainEstCailloux | 0x476317 | [(221, 285)] |
| TerrainEstFrontiereArride | 0x476346 | [(31, 46)] |
| TerrainEstArride | 0x476371 | [(157, 221), (132, 135)] |
| TerrainEstFrontiereEau | 0x4763c7 | [(78, 94)] |
| TerrainEstEau | 0x4763f2 | [(148, 157)] |
| TerrainEstFrontiereCharbon | 0x476421 | [(110, 125)] |
| TerrainEstCharbon | 0x47644c | [(126, 127)] |
| TerrainEstMontagne | 0x476468 | [(477, 608)] |
| TerrainEstHautMontagne | 0x476497 | [(599, 609)] |
| TerrainEstFrontiereMagma | 0x4764c6 | [(94, 109)] |
| TerrainEstMagma | 0x4764f1 | [(139, 147)] |

Plano0 participa de testes por máscara em TerrainEnCollision0x476100; significado de cada bit e máscara por unidade permanece pendente. O filtro de navegação do protótipo é simplificado, não equivalente ao original.
