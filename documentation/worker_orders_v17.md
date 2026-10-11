# Ordens dos trabalhadores — v17

Data: 28/09/2026. Executável: `releases/worker-orders-v17/RisingLandsRemake.exe`.

## Mudanças jogáveis

- B abre uma lista clicável com os 22 tipos do catálogo original, imagens, custos e requisitos. Escolher um tipo disponível ativa a colocação; só a colocação válida cobra recursos. Botão direito/Esc cancelam.
- O construtor selecionado é preservado; B busca outro somente quando a seleção não é um construtor.
- Primeira missão: quartel, santuário, armazém e cultivo são tecnologias disponíveis. Santuário custa 50 alimentos/30 rochas: os 30 alimentos iniciais não bastam. Farol não é tecnologia inicial.
- Catálogo geométrico e visual ampliado de 3 para 22 tipos. O portão permanece indisponível porque o modelo visual estático extraído não contém seu desenho dinâmico.
- Agricultor coleta e conserva a carga até haver um destino escolhido pelo jogador. Não escolhe automaticamente uma base.
- E ou botão Entregar, seguido de clique no armazém/santuário próprio, fixa a estrutura de destino. Também funciona clicar diretamente na estrutura com um agricultor selecionado.
- L ou botão No chão, seguido de clique em terreno livre, fixa um destino no chão.
- Destino pode ser escolhido antes da coleta. C ou clique no alimento inicia a coleta. Repetir sim/não controla o retorno à mesma fonte após a entrega. Não escolhe outra fonte automaticamente.
- Rotas de entrega usam o perímetro acessível do prédio; não tentam entrar em uma célula bloqueada da construção.
- Destino cheio/bloqueado mantém a comida com o agricultor. Não muda o destino nem larga excedentes por conta própria. É possível redirecionar a carga. Backspace interrompe ordens sem perder carga.
- Save v8 preserva destino, repetição, carga, rota e trabalho. Aceita saves v4–v7: entregas automáticas antigas são interrompidas na migração, preservando a carga para uma nova escolha. Cada distribuição continua tendo sua própria pasta PrototypeSaves.

## Evidência e certeza

**CONFIRMADO:** `InventionSetEffet`, RISING-ORIG.EXE VA 0x470e88, chama `SetBatimentRecrutable` 0x418b73 para desbloquear tipos. `RechercheCalculNew` 0x4713ff aplica os valores APPARITION/MISSION e os níveis guardados. `tools/trace_build_unlocks.py` injeta os campos literais de RISING.INI na tabela nativa de invenções de 12 bytes em 0x50816c e executa 81 efeitos + 25 estados iniciais de missão. Casos e interceptações estão em `reports/build_unlocks_original_validation.json`; dados usados pelo remake em `source_remake/Data/build_unlocks.json`. Condição dos testes: níveis herdados zerados. Efeitos de unidades, bônus, mensagens e atualizações derivadas de prédios foram interceptados; não estão provados por esse teste.

**CONFIRMADO nas condições do extrator:** dimensões da tabela 0x495d8c, inicialização visual 0x41c54c e desenho 0x407dd3 produzem os modelos em `extracted/construction_catalog_all.json`. `tools/trace_construction_all.py` deriva da ferramenta anterior e usa registros dos mapas, normalizados para estado 5/owner 0/posição controlada. Tipos sem amostra usam registro sintético. Progressos 101 casos e colocação 26 casos continuam restritos ao armazém. Custos são literais das seções de RISING.INI em `buildings.json` com linhas/offsets. Isso não comprova todas as funções de cada prédio.

**CONFIRMADO anteriormente:** capacidade inicial do agricultor de um alimento, transições de coleta, desenho das pilhas e limite de estoque compartilhado por armazéns. Referências: `food_original_validation.json` e `food_deposit_original_validation.json`, funções 0x4475ac, 0x478114, 0x47658e e 0x4234a7.

**IMPLEMENTAÇÃO MODERNA / fidelidade ainda não demonstrada:** painel, teclas B/C/E/L, seleção explícita de célula, repetição, tempos de coleta, navegação e política de manter excedente com o agricultor. Foram implementados para atender ao controle solicitado pelo usuário; não devem ser confundidos com transcrição integral da máquina de estados original. O estoque continua compartilhado: entrega no santuário usa a capacidade dos armazéns. Sem armazém, o jogador deve escolher depósito no chão; o remake não descarta automaticamente.

## Limitações que permanecem

- Não há desenvolvimento de pesquisas durante a partida. Os bloqueios refletem a tecnologia inicial da missão com progresso herdado zero. Desbloqueios por ciência, bônus e transferência entre missões precisam ser integrados; não afirmar que a árvore tecnológica está pronta.
- Construir/desenhar um tipo não implementa sua função econômica. Cultivo, mercado, laboratório, oficina, hospital e outras funções especializadas permanecem incompletos. Santuários novos não geram moradores automaticamente. Entrada de alimento no cultivo e transformação em colonos não foram implementadas nesta versão. A outra estrutura mencionada pelo usuário ainda não foi identificada com certeza.
- Limite de pilha no chão do protótipo permanece 31; escolha de posição e navegação são modernas. Agricultura não replanta nem faz crescer alimento.
- A IA estratégica, cadeia de campanha, mineração, sons e balanceamento integral continuam pendentes.

## Validação

268 verificações no executável, em `logs/selftest_worker_orders_v17.log`. Incluem coleta manual, destino antes da coleta, entrega de toda a pilha a armazém real construído, repetição desligada, depósito no chão sem crédito de estoque, dois agricultores disputando o último alimento, destino enchendo durante o trajeto, redirecionamento, parada, saves inválidos rejeitados sem mutação, restauração determinística, requisitos iniciais e construção de quartel/santuário/cultivo. O teste de santuário usa recursos iniciais controlados suficientes para pagar seu custo original.

Capturas inspecionadas: `reports/prototype_v17_build_menu_b.png` e `reports/prototype_v17_farmer_a.png`. Captura é verificação visual, não uma sessão humana completa.

1340 hashes das cópias original e working conferidos contra o inventário inicial. Nenhuma alteração ao corpus. As primeiras tentativas de build/teste encontraram um arquivo de dados não incluído no pacote e um teste de santuário sem recursos suficientes; ambos corrigidos antes da execução final aprovada.
