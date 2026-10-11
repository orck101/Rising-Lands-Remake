# Interações v31 — primeira missão

Data: 2026-09-30. Escopo: remake, sem alteração dos originais ou dos saves originais.

## Implementado

- A escolha com Ctrl é um rascunho: primeiro clique apenas marca a fonte; o segundo clique válido, ainda com Ctrl, confirma fonte e destino juntos. Soltar Ctrl cancela o rascunho e mantém a ordem anterior. Perder foco, cancelar, trocar seleção ou abrir construção cancela a escolha pendente.
- Validação por profissão: fazendeiro/comida, construtor/pedra, engenheiro/engrenagens. Cursores de coleta, entrega e proibido; contorno da fonte marcada.
- Armazém como fonte: selecionar e confirmar não debitam estoque. Ao chegar e concluir a retirada, o recurso sai do estoque global e entra na carga. Carga pode virar pilha no chão; save/load preserva essa transferência.
- Clique no prédio abre painel. Santuário oferece os três trabalhadores. Quartel oferece catálogo militar com bloqueios por tecnologia; espadachim é a opção disponível na missão 1. Formação militar utiliza um colono existente, que sai do santuário e vai ao quartel. Laboratório abre a pesquisa de engenharia já existente.
- Armazém mostra estoque compartilhado da rede e a capacidade do modelo atual. As outras construções mostram informações básicas e indicam ações pendentes.
- Atalhos C/M e clique simples em jazida não iniciam mais coleta automática. E/L e botões de destino continuam como atalhos auxiliares de redirecionamento. Não substituem os dois cliques para escolher nova fonte.

## Evidência e grau de certeza

| Descoberta / regra | Origem / região | Método | Certeza |
|---|---|---|---|
| Ctrl contínuo, cancelamento e pares de recurso/profissão solicitados | instruções do usuário nesta conversa | especificação aplicada ao remake | CONFIRMADO como requisito; não alegamos emulação da interface original |
| Custos e duração configurada das 10 unidades do catálogo | `source_remake/Data/units.json`, seções INI, offsets/linhas por campo em `reports/recruitment_catalog_v31.json` | comparação exata dos campos COUTPATATE/COUTROCHE/COUTPIECE/TMPSRECRUTE | CONFIRMADO como valores configurados |
| Vida e parâmetros de animação das unidades | registros de mapas, offsets 666, 677, 679 após cabeçalho de slot; mapa/slot no mesmo relatório | comparação com amostras extraídas | CONFIRMADO para essas amostras |
| Desbloqueios das unidades | `Data/build_unlocks.json`, efeitos 0, Param1=1 e Param2=tipo | nível inicial da missão e pesquisas concluídas | CONFIRMADO no catálogo extraído; pesquisa completa ainda incompleta |
| Conservação do estoque e carga na retirada | `Engine/WarehouseTransfer.cs`, `Tests/InteractionTests.cs` | integração de três profissões e round-trip de save/load | CONFIRMADO nos testes do remake |
| Formação militar, viagem do colono e taxa de progresso | `Engine/BarracksRecruitment.cs`, `RecruitmentSimulation.cs` | integração no remake | PROVISÓRIO; ciclo completo original não emulado |

O verificador `tools/verify_recruitment_catalog_v31.py` lê os dados e produz a proveniência sem sobrescrever o catálogo. O antigo `probe_recruitment.py` cobre somente fazendeiro/construtor e sobrescreve esse arquivo; não o executar como exportador geral.

## Limites importantes

- **A interação com todas as construções ainda não está concluída.** Ver auditoria abaixo.
- Estoque é global por jogador. Não existe ainda inventário físico por armazém. A ocupação herdada conta comida e pedra acima da reserva inicial; não representa uma lotação fiel de cada prédio nem contabiliza engrenagens nessa ocupação. O painel explicita essa limitação. A retirada de engrenagens desconta corretamente o estoque global.
- Retirada tem espera de 10 ticks no remake, ainda não confrontada com a rotina original do armazém.
- Formação dos três trabalhadores e espadachim foi integrada. Associação dos demais tipos militares ao quartel, habilidades especiais e ciclo completo de produção original precisam de validação; permanecem bloqueados pela tecnologia na missão 1.
- UI dos painéis e ícones é do remake, não uma reprodução visual comprovada da interface original. Seleção por sprite usa retângulos, inclusive transparência.
- Testes automatizados verificam simulação/rascunho; capturas verificam desenho dos painéis/cursor. Não houve validação manual completa de teclado/mouse físico.
- Próxima missão permanece fora do escopo jogável. Não foram alteradas regras dos arquivos originais.

## Auditoria das 22 construções

Identidades: `Data/buildings.json` e `Engine/ConstructionSimulation.cs`. Estado abaixo descreve o código do remake, não afirma comportamento desconhecido do original.

| Tipo | Construção | Interação v31 | Falta reconstruir/verificar |
|---|---|---|---|
| 0 | Farol | informações básicas | ações/efeitos nativos |
| 1 | Quartel | formação e tecnologia | ocupação, catálogo completo por prédio, habilidades e ritmo nativos |
| 2 | Santuário | três trabalhadores, colonos e progresso | reposição/migração de colonos e demais regras nativas |
| 3 | Armazém | consulta da rede, coleta e entrega | inventário individual, lotação real, atendimento da rede |
| 4 | Cultivo | informações básicas | produção/interação da construção; coleta de plantas já existentes continua funcionando |
| 5 | Oficina | informações básicas | ações/produção nativas |
| 6 | Biblioteca | informações básicas | pesquisas/ocupação nativas |
| 7 | Laboratório | acesso à engenharia existente | árvore completa e ocupação |
| 8 | Bolsa de trocas | informações básicas | regras e ações de troca |
| 9 | Muralha | informações básicas | comportamento defensivo nativo |
| 10 | Torre de catapulta | informações básicas | ocupação, alvos e ataque |
| 11 | Torre de canhão | informações básicas | ocupação, alvos e ataque |
| 12 | Portão | informações básicas | abertura, fechamento e acesso |
| 13 | Circo | informações básicas | funções/ações nativas |
| 14 | Garagem | informações básicas | produção/veículos nativos |
| 15 | Templo | informações básicas | pesquisas e demais funções nativas |
| 16 | Mercado | informações básicas | comércio nativo |
| 17 | Cristal | informações básicas | funções/ações nativas |
| 18 | Heliporto | informações básicas | produção/transporte nativos |
| 19 | Hospital | informações básicas | cura, ocupação e custos |
| 20 | Templo de fusão | informações básicas | funções/ações nativas |
| 21 | Tenda de campanha | informações básicas | funções/ações nativas |

## Verificação

- `reports/v31-focused.log`: testes de missão 1 e novas interações.
- `reports/v31-regression.log`: bateria geral.
- `reports/v31-edge-before-errors.log`: reprodução do bloqueio incorreto de depósito no chão junto ao armazém, antes da correção.
- `reports/v31-building-1.png`, `v31-building-2.png`, `v31-building-3.png`: painéis inspecionados.
- `reports/v31-resource-order.png`: fonte marcada, fazendeiro aguardando e cursor de entrega.
- `reports/recruitment_catalog_v31.json`: proveniência dos dez registros de recrutamento.

Para experimentar: abrir a missão pelo lançador v31; clicar no santuário/quartel/armazém; selecionar fazendeiro, segurar Ctrl e clicar na comida; ainda segurando, escolher chão/armazém. Repetir soltando Ctrl antes do destino: nenhuma nova ordem deve começar. Para retirar, usar o armazém como primeiro clique e o chão como segundo, observando o débito somente quando o trabalhador pegar a carga.
