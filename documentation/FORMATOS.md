# Formatos recuperados

Todos os inteiros abaixo são little-endian, salvo indicação contrária. Offsets são decimais e relativos ao arquivo; endereços `0x…` são VAs do executável original. Referência: `RISING-ORIG.EXE`, SHA-256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. Cada arquivo tem hash em `reports/inventory.json`. As ferramentas são especificações executáveis parciais; não substituem os limites de certeza descritos aqui.

## Símbolos SBL — CONFIRMADO

`RISING.SBL`, SHA-256 `cbaa48114ce0e0a82d4c0b32708a21f5ef7133d2870ac3d506d02d16c0ac32f6`: inverter cada byte com XOR 255. Desde offset 0 há pares u32 `(offset_codigo, offset_string)`. O sentinela `(0xffffffff,0)` começa em 15848, após 1981 entradas. Os offsets de strings são absolutos no arquivo decodificado; os nomes terminam com espaço/NUL. VA = `0x401000 + offset_codigo`. A base foi obtida do resolvedor de símbolos, não presumida igual à base PE.

Evidência: `debugSymLoad` em `0x48d1a6`; inversão em `0x48d225`; resolução da base em `0x48d2f4`. Ferramenta: `tools/symbols.py`. Resultados: `reverse_engineering/symbols.json`, `call_graph.json`, `rule_callbacks.json`, desmontagem `RISING-ORIG.symbols.asm`. Todos os 140 nomes de callbacks extraídos de RUL encontram correspondência SBL. Nomes fornecem pistas sem provar por si só a semântica. Limites calculados pelo próximo símbolo são limites superiores, pois há epílogos compartilhados.

## Imagens e sprites

### PIE — CONFIRMADO estruturalmente

Assinatura `Pie` em 0; largura u16 em 3; altura em 5; campo u16 em 7, observado 256; paleta de 768 bytes em 9; fluxo em 777. Controle signed8: não negativo copia `controle+1` literais; negativo repete o byte seguinte `1-controle` vezes. Parar quando largura×altura pixels forem produzidos; alguns arquivos têm byte residual. Não reinterpretar controle -128 como NOP.

Paleta: cada componente é `(valor << 2) & 255`; índice zero é preto. Evidência `LoadPalette`, `0x479579–0x479653`, e carregador PIE `0x479654`. Exportações anteriores com paleta sem transformação foram substituídas; o histórico registra esta correção.

### LZS — CONFIRMADO por comparação com código original emulado

`LZSS` em 0; largura/altura u16 em 4/6; `CMAP` em 8; paleta em 12, 768 bytes; `FORM` em 780; tamanho comprimido u32 em 784; dados em 788. Paleta já usa componentes de 8 bits. LZSS: janela 4096, posição inicial 4078; primeiras 4078 posições contêm espaços e 18 finais começam zeradas. Flags do bit menos significativo ao mais significativo: 1 literal, 0 referência de dois bytes; posição `lo | ((hi & 240)<<4)`, comprimento `(hi & 15)+3`, com sobreposição permitida.

Evidência: rotina `0x479110`; `reports/emulated_codec_validation.json` compara os dois fluxos completos com instruções originais em memória emulada. A igualdade de pixels não comprova posicionamento de tela ou timing.

### CMP — CONFIRMADO na estrutura; associação de paleta com FORTE EVIDÊNCIA

u16 quantidade de frames; para cada frame: u16 largura, u16 altura, u32 tamanho comprimido e payload. Controle signed8 >=0 pula essa quantidade de pixels transparentes; <0 copia `-controle` índices. Transparência é máscara separada, não a suposição de índice zero. Os 12.268 frames dos oito arquivos fecharam exatamente dimensões e comprimentos. Ver `extracted/graphics/*.json` para offset e hashes por frame. A paleta usada nas prévias é `PLATGAME.PIE`; mudanças de paleta em execução ainda precisam ser caracterizadas. Hipótese antiga CMP=LZSS foi refutada e preservada em `experiments/REJECTED_cmp_lzss_hypothesis.json`.

## Missões ETA — CONFIRMADO na sequência de leitura

Evidência: `MissionLoad` em `0x46c163`; extração em `tools/parse_eta.py`. A sequência foi aplicada aos 34 arquivos com bounds checks. `extracted/maps/<nome>/full_structure.json` guarda cada bloco, offset, tamanho, hash e registros brutos.

| Offset | Conteúdo | Tamanho |
|---:|---|---:|
| 0 | assinatura preservada | 4 |
| 4 | plano 0, colisões consultadas por máscara | 65536 |
| 65540 | plano 1, IDs u16 de terreno | 131072 |
| 196612 | plano 2, códigos auxiliares | 65536 |
| 262148 | plano 3, dados auxiliares | 65536 |
| 327684 | três grupos de quatro u16 de recursos, IDs chamados na ordem 1,2,0 | 24 |
| 327708 | largura, altura u16 | 4 |
| 327712 | quatro pares u16; provável câmera inicial por jogador | 16 |
| 327728 | quantidade e tamanho de registro de construções, u16 | 4 |
| 327732 | registros: slot u16 + payload | variável |

Depois seguem, nesta ordem:

| Bloco | Codificação | Rotina |
|---|---|---|
| Unidades | count u16, size u16; count × (slot u16 + size bytes) | LoadEngin `0x46ab53` |
| Minas | count u16; count × 4 bytes | LoadMine `0x475276` |
| Ataques iniciais IA | count u16, size u16; count × size bytes | IALoadAttaqueIni `0x47bedd` |
| Construções ghost | count u16; count × 6 bytes | LoadBatimentGhost `0x47b957` |
| Quadros de influência | count u16, size u16; count × size bytes | LoadCadreInfluence `0x47bcda` |
| Fim de missão | count u16, size u16; count × size bytes | LoadFinMission `0x458ad1` |
| Diversos | 16 bytes lidos e descartados nesta rotina | LoadCalculMissionDivers `0x47bf9f` |
| Objetivos | 4 jogadores × 100 entradas × (u16 ID + u8 extra) | LoadObjectif `0x47c094` |
| Entradas de monstros | count u16, size u16; count × size bytes | LoadEntrerMonstre `0x47a8c0` |
| Cauda | 917504 bytes em todos os 34 mapas | DESCONHECIDO |

A cauda contém dados não triviais. Não foi demonstrado que possa ser descartada: apenas que esse caminho de carregamento fecha o arquivo sem consumi-la. Hipótese de dados de editor ainda não confirmada.

Construções têm registro serializado de 204 bytes; stride em memória é 276. Unidades observadas têm registro serializado de 784 bytes; o carregador lê os primeiros 140, salta 512 e lê o restante. Não usar o tamanho serializado como sizeof da entidade moderna. O campo u16 +4 das entradas de monstros é multiplicado por 30 (`0x47a937`), mas isso sozinho não demonstra unidade em segundos.

Grade: stride fixo 256, índice `y*256+x`; dimensões ativas podem ser menores. As prévias usam o plano 1 como índices de BLOCS e não representam uma renderização completa com objetos, interface e neblina.

## RUL — estrutura CONFIRMADA, interpretador incompleto

Header `<4sIIHH`: `RULE`,26055,2915,1157,64. Seguem 1157 registros de 20 bytes, 64 valores u16, tabela de strings de 2915 bytes. Primeiro dword com bit31 ligado representa offset de nome nos 31 bits restantes; 1084 registros resolvem nomes, 140 distintos. Loader `iaProbLoadBinaryRules` em `0x47ddff`. Ver `extracted/rules.json` e vínculos em `reverse_engineering/rule_callbacks.json`. Semântica das instruções, probabilidades, avaliação e agendamento não está reconstruída; não executar esses registros como se fossem regras já entendidas.

## HED/SND — estrutura CONFIRMADA; áudio parcialmente validado contra original

HED: `ABCD`, u32 tamanho sem header, u32 tamanho SND, u32 grupos. Cada grupo tem 12 bytes: chave, flags, offset relativo à posição 16 para descritores. Descritor de 16 bytes `<IIHHBBH`: offset, comprimento, flags, frequência, bits, byte extra, bloco. Observações: flags 0x1001, 22050 Hz, 4 bits, bloco 512; byte extra 200 em seis descritores P permanece desconhecido.

Bancos A/B/P: 14/102/435 clips, 551 total, cobrindo integralmente cada SND. Cada bloco IMA começa com preditor int16, índice de step u8, reservado u8; primeiro PCM é o preditor, depois nibble baixo/alto. Bloco de 512 gera 1017 amostras. Aritmética soma parcelas deslocadas separadamente, como o original. Tabelas no EXE: offsets 620812 e 620780; função `imaadpcmDecode4Bit_M16`, VA `0x48b475`.

Todos os 551 clips completos deram igualdade byte a byte com código original emulado: `reports/audio_full_original_validation.json`. FFmpeg produziu mesmo comprimento, mas PCM diferente; seu resultado não é o oráculo de fidelidade deste jogo. Audição integrada, seleção dos sons, volumes e mistura ainda permanecem pendentes. Relatórios preservam tanto a divergência independente quanto a igualdade com o original.

## Saves SAV6 — PARCIAL

Assinatura em 0; rótulo de 32 bytes em 4. `GameSave` `0x46bbd9` e `LoadVar` `0x46b44c` sustentam o cabeçalho abaixo. Estado restante não foi decodificado por completo. Não existe ainda importação/exportação de save compatível.

| Offset | Tipo | Origem em memória / interpretação |
|---:|---|---|
|36|u8|0x496f94; missão provável|
|37,41,45|u32 cada|0x508f6c/70/74; contadores|
|49,51|u16 cada|0x508710/12|
|53,55|u16 cada|0x497118/1a; dimensões|
|57,59|u16 cada|0x507d54/56|
|61,65|u32 cada|0x50d98c, 0x4e19bc|

## Outros arquivos e limites

INI/textos foram extraídos com offsets e linhas. 38 FLC e 3 AVI passaram pelo decodificador FFmpeg sem código de saída de erro; isso não demonstra sincronização ou ausência de avisos internos. FNT, COL, MAP, LBM e demais famílias estão na matriz de cobertura; não são descartados por serem pequenos. `COULEUR.MAP` tem 609 bytes, mas `TerrainLoadColor` solicita 702 em `0x47554c`: discrepância registrada, efeito de leitura curta ainda desconhecido. Drivers e instaladores são inventariados e identificados; não há alegação de engenharia reversa completa de componentes terceiros.
