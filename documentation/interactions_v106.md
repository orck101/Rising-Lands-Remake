# v106 — passagem por rampas e planaltos

Entrega em 2026-10-04. Pacote: D:\CHATGPT\outputs\RisingLandsResearch\releases\interactions-v106.

## Causa e evidências

**CONFIRMADO no remake:** Engine/Simulation.cs, Passable, bloqueava todos os tiles em [0x1dd,0x261), isto é, [477,609). A faixa inclui paredes, rampas e pisos de planalto. Na MONDE05, (14,77) usa tile 569 com colisão 0; (14,75) usa tile 599 com colisão 0. Ambos eram rejeitados. O teste antes da correção falhou na primeira dessas células.

**CONFIRMADO no original:** RISING-ORIG.EXE, TerrainEnCollision em VA 0x476100, offset de arquivo 0x75500, lê o plano apontado por 0x507d94 e testa a máscara recebida em DL. Emulação isolada das instruções, sem iniciar o executável, confirmou 768 combinações (256 valores × máscaras 1/2/3). CheminCalcul em 0x42f5e8–0x42f603, offsets 0x2e9e8–0x2ea03, também testa o byte do plano de colisão contra a máscara; não rejeita a categoria visual de montanha nesse teste. Os bytes do plano vêm de ETA offset 4 + índice da célula, com alterações de estruturas reproduzidas pelo VisualCatalog.

**FORTE EVIDÊNCIA de correspondência visual:** o atlas de BLOCS.CMP e os tiles da MONDE05 mostram as rampas em [565,599) e o topo em [599,609). Nem todo tile de borda dessa faixa é transitável: a colisão original continua sendo decisiva. Exemplos (12,77) e (16,78) permanecem bloqueados.

## Correção

Removido somente o veto geral aos tiles de montanha de Passable. Mantidos colisão bit 1, árvores, obras, muros, portões e os acessos particulares de construções/minas. A mesma regra é compartilhada pelo jogador e pelos três clãs, pelas ordens diretas e pelo cálculo de regiões alcançáveis da IA. Nenhum tile, dado original, recurso, tecnologia ou save do usuário foi editado. O algoritmo completo de CheminCalcul não foi reconstruído nesta mudança.

## Verificações

- Falha reproduzida antes da alteração em before-fix.log.
- 54 verificações de rampas: subida e descida física na MONDE05; rota ao planalto do lado vermelho pelos quatro donos; classes terrestres presentes; percurso sem atravessar penhasco; restauração exata de rota e continuação determinística; checagem de todos os tiles de montanha aplicáveis dos 34 mapas.
- 768 combinações da rotina nativa de colisão.
- 23 testes v73: muros/portões continuam bloqueando tropas terrestres; portões abertos permitem passagem; balão/helicóptero conservam passagem aérea.
- Suíte v62 de ambiente, construção e recálculo diante de obstáculo dinâmico passou.
- 12 testes do planejador, incluindo restauração do cenário avançado e continuação determinística.
- Os 15 saves JSON existentes carregaram pelo pacote publicado. Hashes de todos os arquivos de UserSaves iguais aos coletados antes dos ensaios.
- Build/publish sem erros; verificações de rampas executadas também no executável publicado, com descoberta automática dos assets em D:.

## Continuação econômica

Foi restaurado um snapshot isolado v105 da missão 5 aos 75 minutos e simulado até 90 minutos sem conceder recursos ou mudar tecnologias. Aos 75 minutos o vermelho tinha 87 alimentos, 13 pedras e 3 prédios; aos 80 passou a 14 pedras; aos 90 tinha 100 alimentos, 0 pedras e 4 prédios. Assim, a correção permitiu retomar movimentação/coleta e algum desenvolvimento no cenário antes estacionado. Não comprova economia totalmente equilibrada, conclusão da missão nem ofensiva completa. Quatro checkpoints preservaram save/load exato. Mudanças de combate nos outros clãs decorrem da acessibilidade corrigida; não houve ajuste de balanceamento nesta versão.

## Entrega e limites

Testes feitos por simulação/código; sem partida controlada pela interface. Confirmar visualmente a passagem apontada pelo usuário na nova versão. Saves em andamento podem ser carregados; ordens já canceladas no pacote antigo devem ser dadas novamente. Não fechar uma partida do usuário nem sobrescrever pacotes em execução.

Arquivos de código alterados: Engine/Simulation.cs, Tests/RampNavigationTests.cs, Program.cs. Atalhos Jogar-* na raiz D:\CHATGPT apontam à v106; atalhos .cmd da v105 foram copiados ao novo pacote preservando sua configuração de monitor. Versões anteriores permanecem disponíveis.

Evidências: D:\CHATGPT\outputs\RisingLandsResearch\experiments\rampas-v106 (atlas, screenshot do usuário, análise dos mapas, emulação nativa, logs, snapshots isolados e hashes dos saves).

DLL publicada SHA256: 965D13754EDEFE384608FEFAA67CBE899DACC9FA369DE4B0A794F072AC82F25F.
