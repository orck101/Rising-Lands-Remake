# Mapa do executável

Referência: RISING-ORIG.EXE, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. PE32 x86, image base0x400000, entryRVA0x8ceec. Imports incluem DDRAW, DSOUND, DPLAYX, WINMM e MSVFW32. Cabeçalhos de todos os PE identificados em `../reverse_engineering/pe_all.json`. A tabela completa dos1981 símbolos está em symbols.json. Compilador Watcom tem forte evidência pelos símbolos e convenções; versão exata não estabelecida.

| Símbolo original | VA | Offset EXE |
|---|---|---|
| FileOpen | 0x458420 | 358432 |
| FileExists | 0x458430 | 358448 |
| FileClose | 0x45844c | 358476 |
| FileRead | 0x458461 | 358497 |
| FileWrite | 0x458476 | 358518 |
| FileSeek | 0x45848b | 358539 |
| FileReadLine | 0x4584a3 | 358563 |
| LoadBatimentTechnologie | 0x46a812 | 433170 |
| LoadEnginTechnologie | 0x46a8b7 | 433335 |
| LoadEngin | 0x46ab53 | 434003 |
| LoadEnginGame | 0x46aeb2 | 434866 |
| LoadBatiment | 0x46b08f | 435343 |
| GameLoad | 0x46b721 | 437025 |
| GameSave | 0x46bbd9 | 438233 |
| MissionLoad | 0x46c163 | 439651 |
| MissionLoadName | 0x46c477 | 440439 |
| LoadPalette | 0x479579 | 493945 |
| LoadEngin | 0x4799a0 | 495008 |
| LoadBatiment | 0x479aa2 | 495266 |
| LoadBatimentGhost | 0x47b957 | 503127 |
| iaProbLoadBinaryRules | 0x47ddff | 512511 |
| imaadpcmDecode4Bit_M16 | 0x48b475 | 567413 |
| debugSymLoad | 0x48d1a6 | 574886 |
| debugSymUnload | 0x48d242 | 575042 |
| debugSymName | 0x48d266 | 575078 |
| debugSymAdr | 0x48d2a7 | 575143 |

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
