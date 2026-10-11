# v87 — revisão da produção vermelha e migração de saves

## Evidência real

Cópia de campanha.slot-1.research-v33.json em experiments/v87-existing-save.json, SHA256 0FA32921918D65A43516641DC6789CBDF3C811D6333FDEF92CC08D88A94A0D98. MONDE04, tick 5933, jogador 1 com Armamento 2; dois treinamentos concluídos de Archer (tipo 8), slots 31 e 33, além do arqueiro inicial slot 12. A cópia reproduziu a falha em reports/v87-production-before.txt.

A v84 passou a exigir tecnologia para ordens novas, mas os treinamentos concluídos ou pendentes de versões antigas eram preservados ao carregar. Essa lacuna explica os arqueiros adicionais deste save. Não foi observado neste ensaio um novo desbloqueio válido de arqueiro no nível 2.

## Correção

EnemyRecruitmentMigration revê o último treinamento de cada personagem inimigo no carregamento. Se a classe não está liberada e ainda pertence ao mesmo inimigo, a unidade treinada é retirada de jogo. O arqueiro nativo permanece: ele não possui treinamento associado. Personagens já revertidos para colonos, transferidos para outro jogador e tropas atualmente liberadas são preservados.

Treinamentos pendentes inválidos são encerrados como registros inativos, mantendo custo e identidade, sem devolver recurso gratuito, permitindo à construção voltar a produzir unidades válidas. RecruitStep também valida tecnologia antes de avançar/finalizar um treinamento inimigo. Não se altera diretamente o arquivo carregado; o ajuste ocorre na simulação e será registrado quando o jogador salvar.

## Correção associada ao Reaper

A cópia real tinha Reaper slot 20 com carga/ordem de entrega ativa. A retirada antiga apenas zerava vida/rota e mantinha FoodJob=3. Isso causava Dead actor has active orders no save posterior. RemoveEnemyStartingCart agora limpa ordens completas e a viagem de alimentação, preservando carga contabilizada inativa e o estoque. O carro inicial não reaparece.

## Testes

- Nove verificações v87 com cópia real e variante pendente: migração, arqueiro inicial, estoque inalterado, retomada determinística, bloqueio de fila e roundtrip.
- Oito v85: Reaper e compatibilidade.
- Teste geral: ALL TESTS PASSED.
- Durante a revisão, também passaram v84 (88), v75 (28 mais slots), v80 (seis/24 minutos) e v86 (16/passagem 3→4).

Relatórios finais: reports/v87-final-0.txt, v87-final-1.txt, v87-final-2.txt. Save do jogador rehashado após os testes com o mesmo SHA256. Arquivos originais não alterados. Publicação interactions-v87.
