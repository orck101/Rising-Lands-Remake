# Relações entre dados
```mermaid
graph TD
  SBL[RISING.SBL] --> SYM[1981 símbolos do EXE original]
  RUL[RISING.RUL] --> CB[140 nomes de callbacks]
  CB --> SYM
  EXE[Tabelas estáticas do EXE] --> IDS[IDs e nomes de tipos]
  IDS --> INI[Seções RISING.INI]
  ETA[Missão ETA] --> ENT[Slots e tipos de entidades]
  ENT --> IDS
  ETA --> TILE[Plano de tiles u16]
  TILE --> CMP[BLOCS.CMP]
  PIE[PLATGAME.PIE] --> PAL[Paleta usada nas prévias]
  PAL --> CMP
  HED[Descritores HED] --> SND[Faixas SND]
  SND --> WAV[PCM validado]
```
Vínculo IDs↔nomes e callbacks↔símbolos comprovado. Paleta compartilhada para todos os estados continua com forte evidência, sem prova para cada transição de runtime. IDs de eventos e vídeos ainda precisam ligação comportamental.
