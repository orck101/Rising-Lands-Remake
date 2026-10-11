# Timing
CONFIRMADO por instruções do original em VA0x474f38: o contador primário0x508f6c é incrementado; contador0x508f70 avança quando primário é par; contador0x508f74 quando primário é divisível por3. Saves armazenam esses dwords em37,41,45.

```text
primary += 1
if primary % 2 == 0: secondary += 1
if primary % 3 == 0: tertiary += 1
```

DESCONHECIDO: chamadas por segundo, todas as dependências da velocidade e escalas temporais de cada subsistema. A multiplicação por30 em entradas de monstros não comprova segundos.

O protótipo usa30ticks/s e passo64/1024 tile por tick como parâmetros de laboratório, sem alegação de fidelidade. `Simulation.Step()` não recebe tempo de renderização; save guarda tick, posição inteira e rota, permitindo continuidade determinística. Ainda falta demonstrar independência sob todos os padrões de carga/renderização.
