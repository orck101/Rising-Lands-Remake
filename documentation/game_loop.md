# Loop principal

Status: PARCIAL. Foram localizados contadores e rotinas de simulação/apresentação; ordem completa e condições de agendamento ainda não transcritas. [Timing](timing.md) separa contadores confirmados de frequência desconhecida.

| Símbolo original | VA | Offset EXE |
|---|---|---|
| AffGame | 0x4177a0 | 93088 |
| CalculGame | 0x42c900 | 179456 |
| EnginGetTimeRecrute | 0x44115b | 263515 |
| _WinMain@16 | 0x45ffd8 | 390104 |
| __WinMain | 0x48eec6 | 582342 |

Fonte dos símbolos: `RISING.SBL`, decodificado e vinculado ao hash original em `../reverse_engineering/symbols.json`. **CONFIRMADO:** nomes e endereços. Nomes sozinhos não confirmam o algoritmo; consulte a desmontagem `RISING-ORIG.symbols.asm` antes de implementar.
