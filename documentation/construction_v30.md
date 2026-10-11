# Construção: som e orientação — v30

30/09/2026. Executar `releases/construction-v30/Jogar-Primeira-Missao.cmd`.

Som de martelada extraído do original associado ao evento de avanço da obra. Uma emissão por impacto, sem replay no carregamento ou na pausa. Obras fora da câmera e capturas automáticas ficam silenciosas. Volume 0,45 e panorama conforme posição horizontal são escolhas do remake, não mistura original reconstruída. Falta de dispositivo de áudio desativa o efeito sem impedir o jogo.

O construtor agora olha para o ponto mais próximo da área construída durante o trabalho. Mapeamento das oito direções confirmado em execução isolada de `CalculDirection` VA 0x476f6e; resultado em `reports/construction_facing_native.json`. A escolha do ponto de trabalho e rotação instantânea ainda são adaptações do remake.

## Origem do som

CONFIRMADO no código original em 0x44b322–0x44b33d: banco 0x50 (P), chave formada pelo tipo 7 deslocado 16 bits e ação 0x2500, resultando em 0x72500. `SOUND/P.HED` grupo 143, registro no offset 1732, descritor 250 no offset 7352. Dados em `SOUND/P.SND`, offset 4104192, tamanho comprimido 3072 bytes. PCM convertido: 6102 amostras mono, 16 bits, 22050 Hz (aproximadamente 0,277 s). SHA256 PCM `5138b42429363dc855042a448f6ba14face3776f6d5031784ded21df99a27a6a`, conferido novamente contra a extração anteriormente validada pela rotina original. Relatório `reports/construction_audio_mapping.json`. Arquivo de distribuição `Content/Data/builder_hammer.wav`.

## Verificação

314 verificações focadas e 347 de regressão passaram, com sobreposição. Novos casos verificam emissão única por impacto, ausência de sons na pausa e direção voltada para a obra. Compilação concluída; captura `reports/v30-construction.png` inspecionada. O acionamento do evento e PCM foram verificados; saída audível pelo dispositivo e mixagem subjetiva não foram confirmadas em sessão manual. Sons de outros personagens, ambiente e música continuam pendentes.

Estado moderno v14 preservado; eventos sonoros não fazem parte do save nem alteram o resultado da simulação. Nenhum arquivo ou save original modificado.
