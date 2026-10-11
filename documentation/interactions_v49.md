# v49 — santuário, controles do mapa e tecnologia animada

Abrir `releases/interactions-v49/Jogar-Primeira-Missao.cmd`.

## Comportamentos entregues

1. Personagens próprios visíveis, incluindo trabalhadores e soldados, podem receber ordem de entrar no santuário. A conversão acontece somente após chegar à entrada, respeitando vagas e reservas de personagens em trânsito. Ao chegar, tornam-se colonos residentes e podem ser retirados e treinados novamente. Um grupo usa a mesma ordem de entrada. Outros prédios continuam recebendo colonos conforme suas regras.
2. Segurar o botão direito sobre o minimapa e arrastar reposiciona o painel dentro da área de jogo. Soltar fixa a posição, sem desfazer a seleção da unidade nem cancelar suas ordens. O clique esquerdo continua centralizando a câmera pelo minimapa; Tab continua mostrando/ocultando o painel. Posição mantida durante a sessão e entre mapas; não incluída no save.
3. Segurar o botão central na área de jogo e arrastar move a câmera nos dois eixos, respeitando zoom e limites do mapa. O cenário acompanha o arraste; soltar encerra o gesto. WASD/setas e bordas continuam disponíveis. Durante os gestos de arrastar, o movimento automático das bordas não é aplicado. Menus e perda de foco encerram o gesto.
4. A transição mostra duas linhas com as próximas pesquisas possíveis segundo os níveis atuais e a missão seguinte. Não concede benefícios pela escolha. Para a passagem 1 → 2 com níveis iniciais: Agricultura mostra colheita 1,5x e 2x; Engenharia mostra Laboratório e Bolsa de trocas; Armamento mostra dano +1 e Incendiário. Espiritualidade informa que não há evolução naquela missão e que começa na missão 7. APPARITION e MISSION são campos distintos: a primeira é o limiar de aparecimento automático, a segunda é elegibilidade para pesquisa; não se deve confundir os dois.
5. A tecnologia selecionada anima seus sprites originais tanto na transição quanto na tela de pesquisa: abóbora, engrenagens, esfera com espinhos e cristal. Demais ramos mantêm sua imagem estática. As instruções de controles foram atualizadas.

## Carga e histórico de personagens

A carga não desaparece na conversão. Antes de virar colono, o trabalhador deixa comida, pedra ou engrenagens em pilhas no chão próximo ao santuário, sem creditar recursos ao estoque geral. A busca é local, em uma janela de 11 × 11 células, e respeita terreno, minas, construções e espaço das pilhas. Se faltar espaço suficiente, ele aguarda conservando toda a carga e sua profissão. Este tratamento é uma adaptação de preservação dos recursos: o comportamento exato do original com carga ainda não foi comprovado.

O histórico de formação continua registrando os custos pagos, mesmo após a volta a colono. Isso permite formar novamente o mesmo personagem sem apagar gastos anteriores ou duplicar recursos. A conversão troca tipo, aparência e animação pelo colono; a vida é limitada ao máximo do colono, preservando ferimentos, sem cura gratuita.

## Persistência

Save v19 inclui os registros de conversão por personagem e a quantidade de formações anteriores. Carrega v18/v17 e os demais formatos já suportados. Os cinco slots procuram primeiro v19, depois v18/v17. Salvar cria v19 e preserva o arquivo de versão anterior. Saves e arquivos originais de D:/RISING não foram alterados.

## Evidências

**CONFIRMADO — animação em emulação controlada do executável copiado:** 64 chamadas de desenho, quatro ramos × 16 fases, provenientes dos blocos 0x41786d (agricultura), 0x4178d8 (engenharia), 0x4178b6 (armamento), 0x4178fa (espiritualidade). Índices da bank INVENTE.CMP: 1–16, 17–32, 33–48, 49–64 respectivamente. Posições nativas: (397,484), (507,473), (610,475), (716,455). As imagens existentes foram copiadas, sem redesenho. Ferramenta: `tools/export_research_animation_v49.py`. Frames, offsets e hashes: `reverse_engineering/research_animation_v49.json`.

**CONFIRMADO — elegibilidade das tecnologias:** execução isolada de InventionRealisable, endereço 0x470ba0, para todo o catálogo na missão 2. O código lê MISSION em 0x508171; APPARITION é lido separadamente em InventionApparait (0x470bfd), 0x508170. Ferramenta e casos: `tools/trace_research_preview_v49.py`, `reverse_engineering/research_preview_v49.json`. Efeitos descritos também têm comentários e parâmetros em `working/RISING/RISING.INI`, seções AGRICULTURE2/3, GENIECIVIL4/5 e MILITAIRE2/3.

**REQUISITO DO JOGADOR:** conversão de qualquer personagem próprio no santuário e gestos de navegação. A implementação atende ao comportamento descrito pelo jogador; não se declara que o algoritmo nativo de conversão, cura ou descarte de carga foi reconstruído.

**ADAPTAÇÕES:** textos dos benefícios em português e reprodução da animação a 12 frames por segundo, independente da velocidade da simulação. Ordem, desenhos e posições são nativos; o tempo exato de cada frame no original ainda não foi medido. A paleta dos PNGs previamente convertidos foi conferida visualmente, sem declarar medição integral da paleta de execução.

## Verificação

- 655 verificações da missão 1/interações e 351 gerais passaram: 1.006 no total.
- Fazendeiro, construtor, engenheiro e soldados retornando a colono; conversão após chegada; capacidade reservada; salvar/carregar em trânsito e convertido; cargo de comida e pedra preservado; nova formação e conversões repetidas; conservação dos custos; compatibilidade com slots anteriores.
- Verificador de UI usa os handlers reais do minimapa e botão central, conferindo movimento diagonal com zoom, fim do arraste e seleção preservada.
- Capturas dos quatro ramos conferidas visualmente: `reports/v49-final-transition-v49.png`, `reports/v49-transition-v49-2.png`, `reports/v49-final-armamento.png`, `reports/v49-transition-v49-3.png`. Navegação: `reports/v49-final-navigation-v49.png`.
- Logs finais: `logs/v49-mission-one.log`, `logs/v49-self-test.log`. Capturas finais e erros vazios correspondentes também estão em logs. Duas recapturas iniciais não puderam sobrescrever PNGs existentes; foram refeitas com nomes novos e concluíram sem erro. Não houve alteração das permissões dos arquivos.

## Limites mantidos

A missão 2 ainda precisa de sua validação própria de objetivos, vitória e IA. Os textos antecipam os efeitos do catálogo original; não significam que todos os efeitos de pesquisa, especialmente Armamento/Espiritualidade, estejam implementados no remake. Os ramos existentes continuam sujeitos às limitações da pesquisa já documentadas. Não foi executado o jogo original no Windows nesta entrega.

## Teste manual sugerido

- Selecione um trabalhador ou soldado, clique no santuário, espere entrar e retire um colono com duplo clique. Confira também um personagem carregado: sua carga deve ficar no chão. Repita a formação com esse colono.
- Mostre o minimapa com Tab; arraste usando o botão direito. Teste o clique esquerdo depois de reposicionar.
- Segure a rodinha no cenário e arraste em várias direções; solte e confira se a câmera para. Teste também WASD e bordas.
- Na vitória da primeira fase, escolha cada ramo e confira os dois textos e a animação; a confirmação continua abrindo a segunda missão.
