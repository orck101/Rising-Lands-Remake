# v65 — pesquisa extra e produção nos prédios originais

Versão: releases/interactions-v65. Iniciadores das missões 1, 2 e 3. Save v27; leitura das versões anteriores preservada. F9 abre o carregamento dos slots. Originais e saves reais não foram regravados.

## Pesquisa extra

O laboratório apresenta um botão maior, com custo explícito de 50 alimentos e indicação de disponibilidade, alimento insuficiente ou compra já utilizada. Depois das duas descobertas obrigatórias, um atalho aparece na tela principal acima do botão Testes e da velocidade. Abre diretamente a escolha do ramo. Abrir ou cancelar não cobra alimento; a confirmação desconta exatamente 50. Continua sendo uma terceira descoberta opcional por missão, sem quarta compra. Este é um acréscimo solicitado pelo usuário, não uma regra atribuída ao original.

## Produção confirmada no original

A rotina BatimentSetTechnologie, VA 0x41c80c do executável copiado working/RISING/RISING-ORIG.EXE, gera menus conforme os flags de recrutamento em 0x4e1934. As bolsas de opções começam em 0x4dd6e8, com 104 bytes por prédio. A ação 6 indica produção de personagem. Emulação isolada de 22 tipos de prédios, com todos os flags habilitados, e 572 casos de habilitação individual confirmam a distribuição e a ordem abaixo. Os números são identificadores internos, não níveis de tecnologia.

| Prédio | Tipo | Personagens, na ordem original |
|---|---:|---|
| Quartel | 1 | 0, 1, 2, 8 |
| Santuário | 2 | 5, 6, 7 |
| Oficina | 5 | 11, 12, 13, 14, 15 |
| Circo | 13 | 3, 25 |
| Templo | 15 | 4 |
| Mercado | 16 | 10 |
| Heliporto | 18 | 16 |
| Templo de fusão | 20 | 4 |

O remake aplica essa tabela para mostrar os ícones originais e validar novas ordens de produção. Veículos deixam de aparecer no quartel. Falta de recursos, colonos ou prédio ocupado mantém as opções indisponíveis. O menu da oficina usa a seta original para alcançar a quinta opção.

CONFIRMADO: InventionSetEffet (0x470e88), executada com a tabela original de invenções, escreve as disponibilidades através de SetTroupeRecrutable (0x4424f1). Foram examinadas 81 invenções. Isso também identificou liberações indiretas: agricultura 4 libera o tipo 3; engenharia 6, o tipo 10; engenharia 9, os tipos 16 e 17. O tipo 17 não aparece como produção direta nesses 22 menus e não recebeu um botão inventado.

CONFIRMADO: a rotina comum de avanço em 0x423c33 usa a ocupação relativa à capacidade do próprio prédio. 64 casos de população foram emulados. O incremento é 100 + floor(100 × colonos/capacidade), em unidades internas por atualização. O remake passa a usar a capacidade de cada produtor e mantém o colono reservado na contagem até sua transformação.

Custos e durações foram obtidos dos dados originais das unidades. Foram acrescentadas sete regras de recrutamento que faltavam e 512 frames originais de movimento para quatro tipos sem amostras nos mapas. InitTroupe em 0x457b87 fornece os parâmetros de animação; o renderizador em 0x415a27 fornece comandos de desenho em 16 direções e oito fases. A saúde zero após inicialização parcial não foi adotada: CreerEngin escreve 100 em 0x457f2f antes de finalizar a criação. Evidências detalhadas estão em reverse_engineering/production_catalog_v65.json e production_visuals_v65.json.

## Compatibilidade e limites

Filas antigas de veículos 11, 12 ou 14 criados incorretamente no quartel são preservadas ao carregar saves até v26, com marca explícita LegacyBarracks no estado migrado. Novas ordens exigem o produtor correto. O carregamento não regrava automaticamente os saves reais.

A distribuição dos menus, ícones, liberações e produção foi implementada. Habilidades especiais, comércio e movimentação aérea dessas unidades ainda dependem de reconstrução; sua produção não significa que todo seu comportamento original esteja completo. A fila usa o fluxo de produção existente do remake, com as verificações e o avanço comum confirmados acima; rotinas específicas de cada máquina não foram integralmente emuladas.

## Validação

2047 verificações automáticas aprovadas, incluindo 206 sobre produção, custos, moradores, avanço, frames, persistência, rejeição de prédios incorretos e migração de filas antigas. Regressões incluem missões, coleta, cultivo, ambiente, pesquisa, menu de testes e leitura de saves. As contagens representam asserções, não partidas independentes.

Capturas verificaram quartel, oficina, circo, templo, heliporto e os estados disponível, insuficiente, escolha e utilizado da pesquisa extra. Os cenários de captura usam simulações temporárias. A compilação final é Windows x64 autocontida. Ferramentas reproduzíveis: tools/trace_production_v65.py, trace_production_unlocks_v65.py, trace_production_visuals_v65.py e trace_production_progress_v65.py. Logs: logs/v65-*.log. Relatório: reports/v65-validation.json.
