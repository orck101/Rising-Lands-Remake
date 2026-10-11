# Engenharia durante a partida — v18

Data: 28/09/2026. Versão: `releases/engineering-v18/RisingLandsRemake.exe`. Inclui v17 integralmente; os textos de requisitos foram corrigidos para distinguir a missão mínima MISSION de desbloqueio automático APPARITION. Uma construção pode ser pesquisável antes do desbloqueio automático.

## Entrega funcional

T ou botão lateral abre o painel de engenharia. Laboratórios construídos recebem colonos reais do santuário; eles saem por uma posição acessível, percorrem o mapa e passam a residir no laboratório quando chegam. O painel permite enviar/retornar colonos, iniciar/pausar pesquisa, ver o laboratório, trocar de laboratório e abrir as construções. Colonos em deslocamento/produção não podem ser usados simultaneamente no recrutamento. Backspace interrompe o trajeto, sem transformar um colono parado no mapa em cientista escondido. Residentes escondidos não aceitam ordens de movimento comuns.

Capacidade de laboratório: 2 colonos, literal de RISING.INI `[LABORATOIRE] LOGEMENT=2`, linha 476, offset 8254. A soma de colonos presentes em laboratórios próprios fornece o número de pesquisadores. Com zero, o remake não progride; com N>0, acumula `2 - 1/N` pontos por atualização. Custos COUT são pontos de pesquisa, não são subtraídos da comida/rocha. No máximo duas descobertas por missão. O catálogo B consulta os desbloqueios adquiridos e é atualizado durante a mesma partida.

Save v9 preserva níveis/descobertas, progresso, pausa e viagens/residência de colonos. Migração v4–v8 usa tecnologia inicial e nenhuma transferência de colono, sem inventar avanços. Reiniciar recupera o estado inicial. Validação rejeita histórias de pesquisa impossíveis, destinos inexistentes e reutilização de colonos, antes de alterar a partida atual.

## Fluxo testado sem recursos artificiais

MONDE03 inicia com 21 alimentos/20 rochas; o laboratório custa 24/12. O primeiro teste tentou laboratório direto e falhou legitimamente por falta de alimento. O cenário definitivo constrói armazém (10/6), coleta alimentos de fontes reais até 30, constrói laboratório (24/12), envia dois dos colonos existentes e inicia engenharia.

A primeira descoberta é GENIECIVIL5 (Bolsa de trocas, ID8, COUT200). GENIECIVIL6 (Mercado, ID16) ainda depende de MISSION4. A rotina nativa pula esse índice sem conceder seu efeito na missão3. A próxima pesquisa executada é GENIECIVIL7 (Muralha, ID9, COUT7500). Ambas desbloqueiam a construção; um muro é efetivamente construído no teste. O limite de duas descobertas interrompe o avanço. Um colono retorna ao santuário e é recrutado como agricultor. Caminhada, pesquisa e retorno são comparados antes/depois de save/load.

`Jogar-Missao-3.cmd` é um lançador local com `--map MONDE03.ETA`; não modifica o mapa, não acrescenta recursos e não aplica o cenário automatizado. O cenário de captura `--capture-menu research-active` prepara laboratório por construção/coleta reais, usando o helper de teste, exclusivamente quando se pede essa captura explicitamente.

## Evidências nativas

**CONFIRMADO:** `tools/trace_research_progress.py` executa 396 combinações em `RechercheCalcul` VA0x47114c, incluindo cientistas 0/1/2/4/16/32, progresso perto do limite, missão3/4 e contadores0/1/2. Resultado armazenado em `reports/research_progress_original_validation.json`. Tabela nativa injetada com valores literais de RISING.INI em 0x50816c; estado de pesquisa em 0x5080e4 e contador de descobertas em 0x50870c. Constante double 2.0 lida de 0x494ca7. Bônus/efeitos laterais não relacionados a desbloqueio de prédios são interceptados.

**CONFIRMADO:** `BatimentNextEtape` VA0x42696c encaminha laboratório ID7 ao bloco0x426df9. `BagContient` conta colonos tipo9 e `RechercheAddScientifique` VA0x470ae7 soma pesquisadores. Foram executados 17 estados controlados de bolsas com 0–16 colonos; esses estados sintéticos testam a contagem, não autorizam ultrapassar a capacidade real2 do laboratório.

**CONFIRMADO:** `RechercheSetBranche` VA0x470a27 muda o ramo sem apagar o acumulador em fixture. A UI entregue permanece restrita à engenharia; isso não equivale à integração dos quatro ramos.

**IMPLEMENTAÇÃO MODERNA / limite de fidelidade:** envio e retorno de colonos, escolha de saídas e trajetos usam a navegação do protótipo; a máquina original completa de entrada/saída e velocidades não foi portada. O avanço de pesquisa ocorre uma vez por atualização de simulação a30Hz, sem prova da cadência temporal integral original. A pausa voluntária de pesquisa e o desenho do painel são modernos.

## Limites explícitos

A integração da pesquisa aceita somente efeitos de desbloqueio de construção de engenharia suportados. Melhorias de velocidade, capacidade, mineração e unidades não são aplicadas nem silenciosamente marcadas como concluídas. Por exemplo, após Muralha na missão4, a próxima melhoria de velocidade permanece bloqueada nesta versão. Agricultura, militar e religião não têm painel/produção de pesquisa integrados. Transferência de avanços entre missões e campanha completa ainda pendentes.

O catálogo visual não significa que cada prédio tenha toda a função do original. Bolsa de trocas, mercado, cultivo, hospital, etc. ainda dependem de lógica especializada. As limitações de depósito de comida, capacidade compartilhada e terreno da v17 permanecem. Não há reprodução integral de Rising Lands ainda.

## Verificação

293 verificações aprovadas em `logs/selftest_engineering_v18.log`; além das268 anteriores, testes de laboratório, economia real para financiá-lo, reserva de colonos contra recrutamento simultâneo, ritmo de pesquisa, pausa, requisitos de missão, desbloqueio na mesma partida, limite de descobertas, retorno, save/load, rejeição de estado inválido e reinício.

Captura `reports/prototype_v18_research_a.png` inspecionada: pesquisa ativa, dois colonos no laboratório, porcentagem e botões legíveis. Não foi feita uma sessão humana completa de campanha. 1340 hashes das cópias original/working permanecem iguais ao baseline.
