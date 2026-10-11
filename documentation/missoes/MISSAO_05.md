# Missão 5 — contexto de trabalho

Mapa: MONDE05.ETA. Atualização: 2026-10-03. Conversa dedicada ainda a criar pelo usuário. Pedido: identificar tudo que falta para uma fase completa e jogável; não implementar o plano inteiro sem ordem de desenvolvimento.

## Estado

Base publicada v103, engine compartilhada. Mapa e três clãs funcionam; abertura reduzida 15 comida/10 pedra por inimigo e arqueiros iniciais apenas do jogador. Não repetir abertura especial da missão 4 nesta fase. Código atual não inclui MONDE05 em HasMissionEnding/MissionSatisfied: vitória normal ainda ausente.

Objetivos extraídos: encontrar os três clãs e eliminar os não aliados. Detalhes nativos de encontro/eliminação e eventos ainda precisam ser comprovados.

## Plano e evidências

Ler reports/plano_missao_05.md, relatório completo desta auditoria, antes de novas investigações. Medição reports/mission5_baseline_v103.json: 0/3/5/10 minutos, sem ordens do jogador, save/load exato em quatro checkpoints. Aos dez minutos os três clãs estavam em recuperação econômica, sem ofensiva. Estoques comida/pedra: 1/4, 7/8, 9/1; prédios 2/4/3. Isso não prova que nunca atacarão.

Arquivos prioritários: Engine/MissionOne.cs (objetivos/final); TradeSessions.cs (diplomacia); OpponentBasicPolicy.cs e OpponentDevelopment.cs (economia/obras); OpponentSimulation.cs (aliança interrompe o ciclo de combate); EnemyStartingBalance.cs (15/10); CampaignProgress.cs (transição). Usar os nomes de arquivos relativos a source_remake/RisingLandsRemake.

## Pendências prioritárias

1. Registrar encontro dos três clãs e implementar conclusão/derrota com critérios verificáveis.
2. Conferir os eventos nativos relevantes, sem inventar semântica.
3. Economia sustentável e recuperação dos três clãs; manter defesa sem bloquear ofensiva para sempre.
4. Mercado/Bolsa/mensageiros: a lista genérica de construções da fase 5 não oferece Mercado/Bolsa à IA.
5. Aliados devem continuar reagindo a terceiros hostis; testar diplomacia e combate com quatro participantes.
6. Tecnologias e produção, mapa e caminhos, transições 4→5→6, save/load, áudio, desempenho e três rotas completas de vitória.

## Verificação/entrega

### Simulações ampliadas — 2026-10-04

Usuário autorizou testes pelo ambiente de programação, sem abrir/jogar pela interface. Nenhuma regra de gameplay corrigida nesta primeira rodada, nenhuma entrega nova publicada; base v103. Relatório: reports/simulacao_missao05_20261004/relatorio.md. Evidências: experiments/simulacao-missao05-20261004.

Ensaios: 60 min naturais sem ordens; 30 min de jogador programado com comandos/custos reais e fontes descobertas; controles separados de aliança/combate e perda de trabalhadores; testes comuns de troca (68 checks), pesquisa e zoom (1.224 combinações) passaram. Save/load exato em 14 checkpoints naturais, sete com jogador e controles; continuação do jogador determinística por dez segundos. Hashes dos UserSaves preservados.

Achados: final da missão 5 ausente confirmado; pacote v103 ainda aplica fome aos militares (cinco iniciais do jogador morreram sem atacante até 15 min no cenário ocioso), divergindo da regra aprovada; clãs 1/3 terminaram com armazéns lotados e nenhum operador reservável para expansão, cultivo/quartel ausentes ou vazios; clã 1 sem minas conhecidas. Após perder seis fazendeiros ou dois construtores em ensaios separados, clã 1 não repôs ninguém em dez min, apesar de 96 alimentos. Clã 2 expandiu até sete prédios e estado Equilibrada. Mercado/Bolsa ausentes do planejamento genérico. Aliança limpa alvos contra terceiros, mas o controle curto teve dano igual com/sem aliança: não afirmar que aliados não se defendem.

Auxiliares: Tests/MissionFiveAudit.cs (--audit-minutes até 120), MissionFiveScript.cs (--script-mission-five) e MissionFiveCases.cs (--cases-mission-five), acionados por Program.cs. Não há aprovação de rotas completas: bloqueadas pelo final ausente. Interface, áudio, desempenho de desenho, campanha e slots do menu continuam pendentes. Não confundir snapshots internos com teste pela interface.

Somente diagnóstico: código auxiliar Tests/MissionFiveAudit.cs, opção --audit-mission-five --output <arquivo separado>. Build passou e ensaio concluiu. Nenhuma regra de gameplay ou save de usuário alterada; nenhuma nova versão jogável publicada por este plano. Consultar AGENTS.md e CONTEXTO_ATUAL.md e conferir alterações de outras conversas antes de editar. Personalidades dos clãs serão definidas posteriormente pelo usuário.

## Navegação por rampas — v106, 2026-10-04

Corrigido o bloqueio geral de tiles de montanha que impedia tropas e trabalhadores de usar rampas/topos com colisão original livre. Mudança comum a todos os mapas, sem liberar penhascos, muros ou árvores. 54 verificações de rampas/terreno nos 34 mapas, 768 comparações nativas, 23 testes de muros/voo, ambiente v62 e 12 do planejador passaram; saves reais preservados. Continuação isolada 75→90 min: vermelho passou de 3 para 4 prédios e voltou a obter/gastar pedra. Não é aprovação do balanceamento nem resolução do final ausente da missão. Pacote e relatório: documentation/interactions_v106.md; evidências experiments/rampas-v106. Atalho D:\CHATGPT\Jogar-Quinta-Missao.cmd usa v106.

## Perfis dos clãs — v104, 2026-10-04

Usuário associou vermelho a Exército sustentado (Agricultura/Armamento), amarelo a Fortaleza produtiva (Agricultura/Engenharia) e verde a Indústria militar (Engenharia/Armamento). Configuração comum implementada em Engine/EnemyPersonalities.cs, seleção na próxima missão, prioridades de apoio e ciclos de recrutamento legais. Ramo em progresso de saves antigos preservado. Entrega releases/interactions-v104; detalhes/verificações em documentation/interactions_v104.md. Isso não corrige os bloqueadores econômicos/final da missão 5 descritos no relatório de simulações. Personalidades específicas já estão associadas às cores; refinamentos táticos posteriores dependem de orientação do usuário.

## v105 — inteligência situacional
Perfis integrados a população, economia, pesquisa disponível, construção e recuperação. Fornecimento ao cultivo consistente; entradas reservadas não são interrompidas por obras. Exploração segura e ampliada de pedra quando necessário. Testes 12/40/28, simulação natural e reposição de todos os construtores do verde com recursos reais. Pacote releases/interactions-v105; detalhes e limites em documentation/interactions_v105.md. Final da missão continua fora deste pedido.

## Auditoria de 47 cenários — v107, 2026-10-04

Pedido do usuário: pelo menos 30 testes com jogabilidades diferentes e feedback de bugs/melhorias. Executados 47 cenários do motor publicado, sem correção de gameplay/publicação nova. Trinta sem concessões e dezessete controles isolados. Inclui 90 min naturais, 30 min de economia programada, exploração dos três clãs, coleta/retirada/lotação, formações, pesquisa, persistência, defesa, recuperação por proprietário, Bolsa, reparo e reconversão. Nenhuma partida manual pela interface.

Resultado: 34 expectativas cumpridas; sete observações, um alerta de fome, quatro reproduções de três bugs, um áudio parcial. CONFIRMADOS: final normal ausente para missão 5, derrota não reconhecida, militares sujeitos à fome apesar da isenção aprovada. No ensaio longo, amarelo perdeu santuário/trabalhadores e permaneceu apenas com seis colonos dos 70 aos 90 min (causa integral da recuperação bloqueada ainda não confirmada). Vermelho/verde pesquisaram Armamento 2→4, amarelo Agricultura 2→4. Mercado/Bolsa apareceram nos três até 90 min: pendência antiga de ausência universal já não descreve a v107. Funcionaram reposição simples de profissionais, coleta/entrega, pesquisa 7/4 min e extra 50, oito personagens de seis classes na Bolsa, reparo e reconversão. 21 saves preservados e DLL publicada sem alteração.

Relatório e catálogo de todos os testes: reports/missao05_47cenarios_20261004/relatorio.md. Evidências/harness: experiments/missao05-30cenarios-20261004. A missão permanece bloqueada para conclusão normal; testes não aprovam áudio reproduzido, UI, herança 4→5 ou três rotas completas de vitória. O atalho antigo da missão 5 ainda usa v106; esta auditoria usa a última entrega v107 explicitamente.

## Conclusão v108 — 2026-10-04

Implementada regra de eliminação aprovada: nenhuma unidade independente viva e nenhum prédio com colono vivo; conta mina/Bolsa/negociação, laboratório e treinamento. Vazios/obras não bloqueiam. Encontro dos três registrado pela visão, aliados removidos da lista dinâmica, ruptura recoloca, confirmação consecutiva, derrota, save/load e transição 5→6. Formato 35 importa antigos e infere encontros de bases em terreno explorado (limite histórico explícito). 35 checks específicos e 40 da missão 6 passaram; não campanha manual. Pacote releases/interactions-v108, atalho D:\CHATGPT\Jogar-Quinta-Missao.cmd. Relatório reports/missao05_conclusao_v108.md; evidências experiments/mission5-ending-v108. O bug de atingir obras, fome militar e demais achados não foram corrigidos neste escopo. Critérios de outras missões preservados.

## Reteste v108 — 2026-10-04
48 cenários comparativos, 35 checks de conclusão e cópias dos cinco slots atuais testados. Vitória/derrota corrigidas; fome militar permanece. Amarelo perdeu santuário e profissionais aos 70 min; jogador ocioso perdeu aos 73m23s, encerrando corretamente a simulação. Nenhuma nova regressão confirmada; áudio/renderização/campanhas humanas continuam pendentes. 21 saves preservados. Relatório reports/missao05_reteste_v108_20261004/relatorio.md; evidências experiments/missao05-reteste-v108-20261004. Nenhuma correção de gameplay neste reteste.

## Abertura econômica v109 — 2026-10-04
Prioridade comum aos três clãs nos dez primeiros minutos simulados; armazém/cultivo/operadores/fazendeiros, reservas, adiamento de ofensivas/serviços secundários, defesa ativa. Backup em backups/antes-abertura-economica-v109-20261004; v108 preservada e atalho anterior disponível. 47 checks de abertura e 35 de conclusão passaram; comparação amarelo 2→19 comida, 2→6 fazendeiros aos dez min; vermelho 2→3 fazendeiros, verde estoque ainda baixo. Cultivo vermelho atrasado e depois sem operador: limitação registrada, não balanceamento final. Relatório reports/abertura_economica_v109.md; evidências experiments/abertura-economica-v109. Atalho da missão 5 aponta v109.

## Fome local e desenvolvimento — v110, 2026-10-04
Orientação atual substitui isenção militar antiga: militares sentem fome, procuram comida solta/armazém próximos (raio 6, caminho máximo 12); sem comida permanecem locais e perdem vida. Saves antigos normalizam viagens distantes. Histórico de treinamento concluído não bloqueia saída de colonos reconvertidos; distribuição tenta outros prédios e preserva operador do santuário. Replay natural do slot 1 por dez minutos: verde treinou quatro tropas, amarelo concluiu uma pesquisa; vermelho continua abaixo da reserva e amarelo ainda sem população excedente no quartel. Não é aprovação global da economia. 11 checks locais, 47 abertura, 35 conclusão, cinco slots carregados/determinísticos e 21 saves preservados. Pacote interactions-v110, formato 36 mantendo slots v33. Relatório reports/fome_producao_v110.md; evidências experiments/fome-producao-v110. Atalho D:\CHATGPT\Jogar-Quinta-Missao.cmd; v109 preservada.

## Raios de fome — v111, 2026-10-04
Regra comum solicitada: comida solta até 20 quadrados, armazém próprio ou aliado até 25 (ponto de interação). Prioriza chão, mantém carga/tarefa, fome e dano sem opção acessível. Débito no estoque do dono, saldo compartilhado persistido; formato 37 importa antigos sem alterar nomes dos slots. Busca emergencial de fazendeiro inimigo também local. Nove checks novos, onze de fome, 35 conclusão e cinco slots determinísticos passaram; 21 saves preservados. Evidências experiments/raios-fome-v111; documentação interactions_v111.md. Gargalos militares/econômicos da v110 seguem separados deste ajuste. Pacote interactions-v111.

## Fome sem pausa sem alimento — v112, 2026-10-07
Regra comum: personagens mantêm a ação quando falta alimento acessível. Só pausam ao encontrar comida no raio 200, prioridade chão → armazém próprio → aliado, mantendo tarefa/carga para retomada. Fome e dano permanecem. Viagens antigas sem fonte removidas ao carregar; formato 38, slots v33. 24 checks de continuidade, nove de alimentação, 47 abertura e cinco slots determinísticos passaram. Relatório reports/fome_continua_v112.md; evidências experiments/fome-continua-v112. Pacote interactions-v112; limites de teste declarados no relatório.

## Bolsa e simpatia — v113, 2026-10-08
Colonos entram como operadores, nunca mercadoria. Ganho de simpatia após troca efetivada segue excedente ponderado/5 e limite −50..50, comprovado em 32 vetores nativos. Saves antigos normalizados; cinco slots preservados/carregados. 21 específicos, 24/67 comércio e 35 conclusão passaram. Ver documentation/interactions_v113.md e reports/troca_simpatia_v113.md. Pendência transitória de save durante entrada sob alvo de combate documentada; sem partida manual. Pacote interactions-v113; atalhos 1–6 atualizados.


## Diplomacia de combate — v114, 2026-10-08
Dano unitário −3; atacar aliado −35/rompimento; evento global nativo −50 por avanço de estágio de dano em prédio. Atinge relações dos demais clãs também; engine ainda sem matriz inimigo→inimigo. 14/89/21/35 verificações e cinco slots passaram. Sem partida manual; v113 preservado. Evidências/limites: reports/diplomacia_ataques_v114.md. Atalhos regulares usam v114, formato39 preservado.


## Defesa coordenada v115 — 2026-10-09
Base comum dos três clãs: avaliação de ameaça/força, resposta a ataques em prédios, setores, guarnição/reserva, perseguição limitada, recrutamento e apoio legal por proprietário. Perfis preservados; fortificações amarelas limitadas e pagas. Emergência pode recolher membros de ondas e impedir novas saídas enquanto faltar defesa. Formato40 importa anteriores; slots v33 e 21 arquivos reais preservados. 59 específicos, 71 poderes, 89 combate, 47 abertura, 21 comércio, 10 planejamento, 35 conclusão e cinco slots passaram. Medida engine com 57/147 personagens, sem teste manual de dificuldade/FPS. Pacote interactions-v115; v114 e backup preservados. Relatório reports/defesa_bases_v115.md; evidências experiments/base-defense-v115. As regras econômicas anteriores continuam sujeitas aos gargalos documentados; não se declara fidelidade da IA original.

## Ofensiva verde v116 — 2026-10-09
Base de personalidade ampliada: produção/apoio/ataques fortes, maior grupo, seleção por poder de cerco e defesa conservada. Alianças inalteradas. Regra de pressão tardia específica da missão4 preservada. Formato40, slotsv33 e saves reais preservados; 21/40/59/10/47/35 checks e cinco slots passaram. Ver reports/ofensiva_verde_v116.md; sem aprovação manual de equilíbrio. Pacote interactions-v116.

## Visão aliada v117 — 2026-10-09
Base comum: visão recíproca direta, memória após ruptura, inimigos conhecem terreno/recursos iniciais mas observam unidades/prédios novos. Conhecimento privado por clã e patrulhas sem visão atual. Formato41/slotsv33; cinco slots preservados. Missão6 continua sem adversários. Ver reports/visao_aliada_v117.md e documentation/interactions_v117.md para 443 checks, capturas, migração e limites. Pacote interactions-v117; atalhos1–6 atualizados, v116 preservada. Sem partida manual completa.


## Diagnóstico slot1 — 2026-10-09
Save formato40 com4h56m30s, processo do usuário ainda v116. Vermelho12 soldados todos guarnição/cap12; amarelo12, dois livres com20%vida e reservas sobrepostas; verde1fazendeiro, estoque0, cultivo4/5, reserva antiga de armazém bloqueia reposição. Houve contatos antigos de combate registrados, sem clã eliminado. Continuações naturais v1162min/v11720min sem novo treino/obra/onda; sondas causais em cópia confirmam conflitos. Sem correções aplicadas; saves preservados. Relatório reports/slot1_desenvolvimento_20261009.md; evidências experiments/slot1-desenvolvimento-20261009.


## Recuperação e bônus do verde — v118, 2026-10-09

Meta militar cobre guarnição mais ofensiva, com reposição limitada para feridos; reserva defensiva unificada. Recuperação protege produção, usa cultivo/estoque conforme falta colono ou verba, suspende reserva de armazém e reconverte feridos excedentes via santuário. Verde começa com piso nativo por missão+2 por ramo (fase5: Agr4/Eng9/Arm4/Esp2), sem acumular no save/reinício; progresso herdado maior preservado. Slot1 copiado em10min naturais:1→5 fazendeiros e renda1,67→7/min, quatro treinos pagos; ainda sem ofensiva verde no intervalo. Vermelho voltou a produzir/formar grupos, amarelo produziu sob pressão defensiva. 421 verificações positivas, cinco slots carregados/21 arquivos preservados. Sem partida manual completa. Pacote interactions-v118; relatórios/limites em reports/regras_ia_v118.md.

## Observação sem derrota — v119, 2026-10-09

Solicitação temporária: extinção do jogador não encerra a missão 5. Não concede imortalidade/recursos/visão; adversários seguem normalmente. Vitória e eliminação inimiga mantidas, derrota da missão 6 preservada. 36+40 checks passaram, incluindo economia após extinção e save/load determinístico; 21 arquivos de saves preservados. Pacote interactions-v119; somente atalho da missão 5 atualizado, v118 preservada. Ver documentation/interactions_v119.md e reports/observacao_v119.md. Sem partida manual.

## Cores do minimapa — v120, 2026-10-09

Cor por proprietário unificada para tropas, prédios, obras, muros e portões: jogador azul, vermelho1, amarelo2, verde3. Memórias escurecidas preservam a cor e opacidade. Neblina/diplomacia/save/derrota temporariamente desativada preservados. 11 checks de cores +36 de conclusão, captura do renderer conferida,21 saves preservados. Atalhos1–6 usam interactions-v120; anteriores preservados. Ver documentation/interactions_v120.md e experiments/minimapa-v120. Sem partida manual.

## Alimentação local mais próxima — v121, 2026-10-09

Fonte válida mais próxima até20 quadrados: chão, armazém próprio ou aliado, sem preferência. Caminho máximo40. Viagens antigas longas/obsoletas canceladas no load restaurando tarefa/carga; fontes esgotadas reavaliadas durante viagem.98 checks no pacote publicado;21 saves preservados. Slot1 viagens118/122 canceladas em memória. Regra comum aos quatro clãs, sem nova política de segurança do jogador. Minimapa e derrota temporariamente desativada preservados. Pacote interactions-v121, atalhos1–6 atualizados, v120 preservada. Ver documentation/interactions_v121.md; sem partida manual.

## v126 — pesquisa do slot 1 e aliados, 2026-10-10

Slot1 em Agricultura6 com dois pesquisadores não avançava por rejeição dos desbloqueios agrícolas de unidades já implementadas. Corrigido Stomper/carro coletor, sem editar save real. Dano hostil não diminui simpatia; valores herdados na transição confirmados; aliados com marcadores visuais e cor preservada. Ver reports/diplomacia_pesquisa_v126.md. 12 específicos+14 combate+53 pesquisa+55 progressão; cinco slots copiados carregados,21 saves reais inalterados. Pacote v126-final, sem campanha manual.
