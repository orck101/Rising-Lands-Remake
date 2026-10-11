# Combate

DEGAT contém matrizes de27 valores nas unidades; não reduzir a um dano escalar sem interpretar indexação. PORTEE, BLINDAGE e RECHARGE são literais configurados. Fórmula final e ordem dos modificadores permanecem desconhecidas.

## Pontos de investigação

| Símbolo original | VA | Offset EXE |
|---|---|---|
| AffAttaqueEngin | 0x41374c | 76620 |
| TypeBatimentGetDegat | 0x418c2a | 98346 |
| BatimentGetDegat | 0x41a9e0 | 105952 |
| BatimentGetDegatEffectif | 0x41aa16 | 106006 |
| BatimentInitAttaqueSelect | 0x424364 | 145252 |
| BatimentPeutAttaquer | 0x424404 | 145412 |
| RechercheDecalageProjectile | 0x425198 | 148888 |
| TryLanceAttaqueSound | 0x440b64 | 261988 |
| GoLastAttaque | 0x440c07 | 262151 |
| SetAttaqueBatiment | 0x445add | 282333 |
| SetAttaqueEngin | 0x445ade | 282334 |
| EnginAttaque | 0x446cb2 | 286898 |
| AnimAttaqueTermineEngin | 0x447977 | 290167 |
| AnimAttaqueEngin | 0x4479c4 | 290244 |
| EnginGetDegat | 0x44f98a | 322954 |
| TypeEnginGetSortieProjectile | 0x4511a5 | 329125 |
| EnginGetProjectileSortie | 0x4513c7 | 329671 |
| CalculAttaqueEngin | 0x451914 | 331028 |
| TypeEnginPeutAttaquer | 0x453d7d | 340349 |
| PlayerAAttaqueSet | 0x46d4f0 | 444656 |
| PlayerAttaqueReset | 0x46d518 | 444696 |
| PlayerAttaqueCalculSympa | 0x46d519 | 444697 |
| ProjectileAlloc | 0x46de90 | 447120 |
| ProjectileFree | 0x46dea0 | 447136 |
| LoadProjectile | 0x46deaa | 447146 |
| SaveProjectile | 0x46ded2 | 447186 |
| ProjectileReset | 0x46defa | 447226 |
| ProjectileExiste | 0x46df1c | 447260 |
| ProjectileLineaire | 0x46df39 | 447289 |
| ProjectileSet | 0x46e0f8 | 447736 |
| ProjectileGetLibre | 0x46e312 | 448274 |
| ProjectileInit | 0x46e33c | 448316 |
| ProjectileCalculLineaire | 0x46e3ba | 448442 |
| ProjectileAnim | 0x46e41c | 448540 |
| ProjectileInfligeDegat | 0x46e4ee | 448750 |
| ProjectileAvance | 0x46e9e8 | 450024 |
| ProjectileCalcul | 0x46faff | 454399 |
| ProjectileTrierAffichage | 0x46fb7b | 454523 |
| ProjectileGetCase | 0x46fd8a | 455050 |
| AffProjectile | 0x46fe60 | 455264 |
| EnginSelectPeutAttaquer | 0x472245 | 464453 |
| SetAttaqueEnginSelect | 0x473caf | 471215 |
| AffEnginProjectile | 0x478930 | 490800 |
| IAResetAttaque | 0x47bdf2 | 504306 |
| IASauveAttaqueIni | 0x47be4a | 504394 |
| IALoadAttaqueIni | 0x47bedd | 504541 |
| IAAttaqueCodeExiste | 0x47bf7c | 504700 |
| IAAttaqueCodeTest | 0x47bf9e | 504734 |
| I_DonneBatimentAAttaquer | 0x4804b5 | 522421 |
| V_AttaquerBatiment | 0x481d00 | 528640 |

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
