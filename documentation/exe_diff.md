# Diferenças de executáveis
CONFIRMADO: RISING-ORIG.EXE, RISING-ORIGINAL.EXE e CD/INSTALHD/RISING.EXE possuem conteúdo idêntico. RISING.EXE atual é outra variante e não deve fornecer o baseline silenciosamente.

FORTE EVIDÊNCIA: hook em VA0x474f38 (raw0x74338) redireciona para seção .spd em VA0x52e000 (raw0xa2200). Rotina espera GetTickCount com limiar `30-7*(byte[0x497234]-1)` e retorna a0x474f3e. Isso altera temporização. O efeito em velocidade de partida não foi medido.

MOD versus V1.0 possui alterações incluindo hook raw0x19c13 e código raw0x93044. Artefatos de diff/desmontagem em reverse_engineering permitem recuperar bytes exatos. Comparações brutas por offset entre builds deslocados não equivalem a alterações semânticas. Bugs introduzidos: DESCONHECIDO; nenhum foi demonstrado por teste integrado.
