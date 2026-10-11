# Saves

[Header SAV6 e offsets conhecidos](FORMATOS.md#saves-sav6--parcial). Importador completo ainda não existe. A alteração detectada em save1.sav na origem não foi causada pelas ferramentas e tem relatório separado.

O protótipo grava JSON version1 com SHA256 do mapa, nome, tick, unidades e rotas. Aceita somente o mesmo mapa e slots válidos, rejeita versão desconhecida e aplica carregamento somente após validar. Esse formato é exclusivo da navegação experimental: não salva economia/combate ainda inexistentes, nem substitui o save original. Escrita usa arquivo temporário e renomeação; tolerância a queda de energia não foi ensaiada.

| Símbolo original | VA | Offset EXE |
|---|---|---|
| LoadEnginGame | 0x46aeb2 | 434866 |
| LoadVar | 0x46b44c | 436300 |
| SauveVar | 0x46b58c | 436620 |
| GameLoad | 0x46b721 | 437025 |
| GameSave | 0x46bbd9 | 438233 |
| LoadPlayer | 0x46dcec | 446700 |
| SavePlayer | 0x46dd91 | 446865 |

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
