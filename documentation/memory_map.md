# Memória

| Endereço | Uso comprovado ou sustentado |
|---|---|
|0x4e19a0|ponteiro para tabela de unidades, stride276|
|0x49a0c8|base da tabela de construções, stride276|
|0x4965ae|tabela de tipos de unidade, stride50|
|0x495d8c|tabela de tipos de construção, stride52|
|0x507d94|ponteiro plano0 ETA|
|0x507d5c|ponteiro plano1 de tiles u16|
|0x507d60|ponteiro plano2|
|0x507d98|ponteiro plano3|
|0x508f6c/70/74|contadores de simulação|

Getters e loaders na desmontagem confirmam acessos. Nenhuma leitura de memória de processo do jogo foi realizada; endereços vêm da análise estática e de emulação limitada. Layout parcial em entity_fields.json; [unidades](units.md) e [construções](buildings.md).
