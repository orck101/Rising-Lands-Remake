# Navegação ao último ataque — v10, 27/09/2026

Release: `releases/alerts-v10/RisingLandsRemake.exe`.

X leva a câmera ao último alerta de ataque recebido por uma unidade do jogador. Backspace passa a interromper ordens; é uma escolha provisória moderna, não um atalho comprovado do original. Seleção e ordens não são alteradas ao localizar um ataque. Sem alerta recente, a câmera permanece parada e aparece uma mensagem. Grupos numéricos e treinos F2/F3 continuam disponíveis.

## Evidência

Fonte: working/RISING/RISING-ORIG.EXE, SHA256 af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e.

CONFIRMADO por leitura: InputGame consulta VK_X (0x58) em 0x429810–0x429819 e chama GoLastAttaque (0x440c07) em 0x429828. A tecla não corresponde ao comando de parar usado nas versões anteriores do protótipo.

CONFIRMADO por emulação: GoLastAttaque só altera a câmera se a diferença unsigned entre relógio 0x508f6c e timestamp 0x4e19c0 não excede 1000 e a célula 0x496acc não é 65535. Extrai coluna/linha, subtrai metade inteira do tamanho da área visível e limita às bordas do mapa. Foram comparados 60 casos com expiração, célula ausente, cantos e tamanhos pares/ímpares de viewport.

CONFIRMADO por emulação isolada: TryLanceAttaqueSound (0x440b64) só registra unidades do jogador local (0x508031), espera diferença de pelo menos 200 no relógio ou aceita reinício do relógio (timestamp maior que relógio). Copia a célula +124 da unidade para 0x496acc. Foram comparados 16 casos; chamada de áudio 0x48a625 interceptada, sem executar som/API legada. Região semelhante aparece em InfligeDegat, 0x451029–0x45108d, após verificações de jogador.

Ferramenta: tools/trace_attack_alert.py. Vetores: source_remake/Data/attack_alert.json. Resultado: reports/attack_alert_original_validation.json. A ferramenta usa apenas a cópia em memória, não inicia o executável legado.

## Integração e limites

O remake registra ataques com dano positivo de outro dono contra unidade do jogador. O alerta usa a célula da posição moderna. Isso não reconstrói todos os chamadores originais, ataques sem dano, edifícios ou áudio.

A primeira ocorrência da sessão é aceita imediatamente: decisão moderna explícita, pois a inicialização original do timestamp ainda não foi reconstruída. As demais respeitam 200 atualizações entre registros; o alerta expira após 1000. Essas contagens são aplicadas ao relógio de laboratório. A equivalência em segundos ao jogo original permanece DESCONHECIDA.

A câmera moderna tem zoom e viewport variável; convertemos a área visível para células inteiras e aplicamos a fórmula nativa. Quando a janela mostra mais que o mapa inteiro, a posição é limitada a zero como adaptação moderna.

Alerta é informação da sessão, não entra nos saves v7. Carregar uma partida limpa a referência; trocar de mapa também. Grupos continuam sendo da sessão. O carregador mantém suporte v4–v7. Nada é gravado nos saves originais.

## Verificação

216 testes aprovados em logs/selftest_alerts_v10.log, incluindo os 76 vetores nativos, primeiro alerta, filtro de dono, bloqueio de atualizações repetidas, novo alerta no limite, retaliação real no treino, limpeza no carregamento e viewport maior que o mapa. Captura reports/prototype_alerts_v10.png inspecionada; comandos legíveis e cena de combate renderizada. A captura não valida sozinha o gesto da tecla X.

A reconstrução dos cliques, menus e pausa original ainda está pendente. Esta release corrige um atalho específico; não torna o conjunto de controles integralmente fiel.
