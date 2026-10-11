# Rising Lands — reconstrução Windows x64, v102

Abra `../../releases/interactions-v102/Jogar-Primeira-Missao.cmd`, `Jogar-Segunda-Missao.cmd` ou `Jogar-Terceira-Missao.cmd`. O pacote usa a cópia local dos assets em `working/RISING`, procurada nos diretórios ancestrais. Para outra cópia, use `--assets <pasta>`.

A entrega e suas evidências estão em `../../documentation/interactions_v102.md`. Na Missão 4, após 60 minutos efetivos de simulação, ondas começam com dez combatentes, persistem até a eliminação completa e crescem 25% arredondando para cima. Abrir `../../releases/interactions-v102/Jogar-Quarta-Missao.cmd`; também é possível carregar um save existente da fase. Armazéns inimigos expandem preventivamente aos 70%, com reserva de colono e orçamento, prioridade à ativação e proteção da sobrevivência. As correções da Bolsa e da produção econômica da v100 permanecem. Música original ausente, magia da IA e fidelidade integral do remake permanecem pendentes.

## Controles principais

- Clique esquerdo: seleção e ordem; arrastar seleciona grupo; Shift adiciona à seleção.
- Ctrl mantido: fonte válida e depois entrega; soltar antes do segundo clique cancela. Trabalhador carregado aceita redirecionar a carga.
- Colono selecionado + clique no prédio: entrar; duplo clique no prédio: retirar residente.
- Construtor + clique na obra: ajudar; B abre as construções liberadas.
- WASD/setas, bordas ou arrastar com botão central: câmera. Rodinha: zoom; Home: centralizar seleção.
- Clique direito e arrastar no minimapa: reposicionar; Tab mostra/esconde.
- Seleção somente de combate: opções de quadrado, linha e triângulo; clique no retrato de classe filtra a seleção.
- Esc: menu; F5 salvar / F9 carregar em cinco slots com confirmação para substituição.
- Menu → Ajustes de som: geral, música, efeitos, avisos e vozes. A música original não está disponível nesta cópia.
- Slider: 1x a 5x. Botão Testes: concessões, níveis de pesquisa, mapa e conclusão de objetivos.
- Espaço: indicadores; Backspace: parar; Pause: pausar; F11: tela cheia.

Saves do remake ficam no diretório compartilhado `../../UserSaves` em relação à cópia `working/RISING`; preferências de áudio ficam em `../../UserSettings`. Novos saves usam formato e sufixo v29, preservando a leitura das versões antigas. Arquivos reais não são migrados automaticamente por escrita. Arquivos `.sav` e executáveis originais não são modificados.

## Reprodução técnica

.NET 8 com MonoGame DesktopGL. A simulação usa 30 Hz de laboratório; a velocidade multiplica passos sem alterar as regras por passo. Cadência absoluta do executável original ainda não totalmente comprovada.

Testes: `--test-v70`, `--self-test`, `--test-mission-one`, `--test-mission-two`, `--test-v57`, `--test-research-v58`, `--test-discovery-v60`, `--test-crop-v61`, `--test-environment-v62`, `--test-menu-v63`, `--test-production-v65`, `--test-special-v66`, `--test-collection-v50`, `--test-range-outline`, `--test-attack-range`, `--test-warehouse-accounting`, `--test-viewport-zoom`, `--check-player-saves`.

`--benchmark-v70 --output <json>` compara cenários de consulta/passos. Capturas com `--capture <png> --capture-menu v70-performance-1` ou `v70-performance-5` medem atualização/desenho da mesma cena sintética. `v70-audio` valida 551 clips com reprodução silenciosa. Capturas/fixtures não concedem recursos nas partidas normais.
