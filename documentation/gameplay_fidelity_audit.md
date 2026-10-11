# Auditoria de fidelidade da gameplay — v24

A gameplay completa ainda não foi reproduzida. Esta auditoria separa comportamento demonstrado em rotinas do executável, comportamento implementado e trabalho restante. O foco de integração permanece a primeira missão.

## Critério de evidência

CONFIRMADO se refere apenas à regra ou trecho especificado, nunca ao sistema inteiro. FORTE EVIDÊNCIA indica correspondência parcial com fontes primárias e testes, sem sessão completa comparativa. PROVÁVEL e HIPÓTESE não autorizam tratar uma regra como original. DESCONHECIDO indica que a equivalência ainda não foi demonstrada. A coluna de implementação é independente da certeza da fonte.

Fonte principal: `working/RISING/RISING-ORIG.EXE`, SHA-256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. Os endereços abaixo são endereços virtuais; offsets correspondentes constam em `reverse_engineering/disassembly/RISING-ORIG.symbols.asm`. Nenhum processo do executável original foi executado nesta revisão: trechos foram emulados com entradas controladas.

## Comparação por sistema

### Comandos e seleção

**Evidência: FORTE EVIDÊNCIA.** Seleção, grupos, parar e alertas reconstruídos parcialmente; Ctrl fonte e clique de entrega conforme contrato do usuário.

Fonte/método: InputGameCtrl; documentação mouse_controls_v12, control_groups_v9, worker_input_v22.

Lacunas: Confrontar ciclo completo de Ctrl e cursores nativos; retirar atalhos de laboratório conflitantes. F2/F3 ainda abrem treino, enquanto README original reserva F1–F4 à velocidade.

### Papéis na coleta

**Evidência: CONFIRMADO.** Matriz nativa aplicada: 5/13 comida, 6 peças, 7 pedra. Grupos incompatíveis rejeitados.

Fonte/método: BatimentEstDepot 0x419269; EnginSelectPeutDeposer 0x472195; EnginTravailSelectionne 0x47237f; EnginSelectPeuventTransporter 0x4729a2; 906 casos.

Lacunas: Comportamentos especiais de veículos e outros personagens não são definidos por esta matriz.

### Alimento e transporte

**Evidência: FORTE EVIDÊNCIA.** Plantas/pilhas, carga, chão, repetição, armazém e conservação implementados.

Fonte/método: TerrainRecolte 0x478114; TerrainRetireMatiere 0x47658e; worker_input_v22 e farmer_v23.

Lacunas: Busca de plantas, redistribuição entre trabalhadores, depósito quando cheio e animações ainda parciais.

### Depósitos

**Evidência: CONFIRMADO.** Armazém aceita entrega manual e contabiliza recursos; santuário removido dessa ação.

Fonte/método: Filtro nativo classifica tipos 3 e 4 como depósitos; somente fazendeiro é aceito no tipo 4.

Lacunas: Cultivo (4) ainda NÃO aceita entrega no remake. Precisa de estoque local, consumo e criação de colonos antes da integração; não deve somar diretamente ao estoque global.

### Mineração

**Evidência: FORTE EVIDÊNCIA.** Jazidas finitas, carga de pedra, entrega em chão ou armazém e retomada implementadas.

Fonte/método: mining_v20; CalculWorkEngin 0x44bd56 e 0x44c175.

Lacunas: Posição exata do trabalhador, reserva simultânea de jazida e sua animação ainda não reproduzem toda a máquina de estados.

### Tempos de trabalho

**Evidência: CONFIRMADO.** Sem bônus: fazendeiro 250 atualizações para planta; construtor 100 na entrada, 50 na continuação da mesma carga; pilhas 10.

Fonte/método: Trechos 0x44ce24→0x44d8f7→0x44d549; 0x44bd56→0x44d90c; 0x44c175; 11 casos controlados.

Lacunas: Não equivale a tempo real confirmado. Cálculo e aplicação dos bônus de pesquisa pendentes. Reaper mantém tempo provisório; percurso específico ainda não demonstrado.

### Movimento e caminhos

**Evidência: DESCONHECIDO.** Remake usa busca em quatro direções e velocidade provisória, com colisão importada.

Fonte/método: pathfinding.md; rotina nativa DeplaceEngin 0x44930c identificada, não reconstruída integralmente.

Lacunas: Medir velocidade por tipo, diagonais, filas, bloqueios, formação e desvio dinâmico.

### Construção

**Evidência: FORTE EVIDÊNCIA.** Catálogo, custos, posicionamento, progresso, interrupção e desbloqueios parciais.

Fonte/método: worker_orders_v17, engineering_v18, build_unlocks.json.

Lacunas: Funções dos prédios, população interna, bônus e danos de estruturas incompletos. Portão sem modelo integrado.

### Recrutamento e população

**Evidência: FORTE EVIDÊNCIA.** Fazendeiro e construtor recrutáveis; reserva de colonos e custos básicos.

Fonte/método: spawn_system.md; dados de recrutamento existentes.

Lacunas: Restantes dos 26 tipos, profissões, ocupação de prédios, produção simultânea e conversões pendentes.

### Pesquisas

**Evidência: FORTE EVIDÊNCIA.** Ramo de engenharia e parte dos desbloqueios; laboratório e limite de descobertas.

Fonte/método: engineering_v18; README.TXT seções de produtividade e inventions.

Lacunas: Agricultura, militar, religião/magia e efeitos dos bônus ainda incompletos. Ordem descrita no README precisa ser conciliada com tabelas nativas.

### Combate

**Evidência: FORTE EVIDÊNCIA.** Espadachim e arqueiro, dano básico, alcance e projéteis parcialmente reconstruídos.

Fonte/método: combat_original_validation.json; timing_reconstruction_v7; relatórios de arqueiro.

Lacunas: Faltam demais armas, ataques a prédios, dano em área, experiência completa, habilidades e coordenação.

### Fome e sobrevivência

**Evidência: DESCONHECIDO.** Sem sistema completo equivalente ao original.

Fonte/método: FATIGUE na configuração; README menciona intervalos de fome e bônus.

Lacunas: Reconstruir consumo, abastecimento, penalidades, morte e interação com pesquisa.

### IA e diplomacia

**Evidência: DESCONHECIDO.** Regras extraídas, execução completa ainda ausente.

Fonte/método: RUL: 1157 registros de 20 bytes, 64 raízes; README descreve comércio, alianças e simpatia.

Lacunas: Implementar avaliador, prioridades, ataques, negociação, mercado e transporte comercial.

### Visibilidade e interface

**Evidência: FORTE EVIDÊNCIA.** Sprites originais, seleção e parte das informações; interface e cursores modernos.

Fonte/método: visual_reconstruction_v2; unit_info_v11.

Lacunas: Névoa, descobrimento, informação permitida, layout e feedback sonoro precisam de comparação funcional.

### Primeira missão

**Evidência: CONFIRMADO.** Predicado de vitória integrado, tela de conclusão, próxima missão sem ação.

Fonte/método: mission_one_conditions.md; mission_lifecycle.md; mission_one_v21.md.

Lacunas: Predicado confirmado não significa cenário inteiro fiel: faltam eventos/IA e validação jogada contra o original.

### Campanha e eventos

**Evidência: DESCONHECIDO.** Mapas e textos recuperados; foco jogável permanece missão 1.

Fonte/método: campaign_text_v16.md; arquivos de mapas e eventos.

Lacunas: Interpretar todos os triggers e testar sequência das missões após estabilizar a primeira.

### Save/load

**Evidência: CONFIRMADO.** JSON moderno v14 preserva estado implementado e migra destinos antigos de santuário; originais preservados.

Fonte/método: Testes de serialização, migração e conservação.

Lacunas: Não é compatibilidade completa com saves nativos; sistemas ausentes também não são persistidos.

### Áudio

**Evidência: FORTE EVIDÊNCIA.** Arquivos recuperados/decodificados.

Fonte/método: Inventário e documentação de recursos.

Lacunas: Associar eventos, vozes, prioridade e repetição à lógica original.

## Contrato de Ctrl: discrepância explícita

O usuário determinou segurar Ctrl para clicar na fonte e soltar para escolher a entrega. Esse fluxo foi preservado. A leitura estática de InputGameCtrl em 0x4296f2–0x42970d mostra limpeza dos modos 8/9 quando Ctrl deixa de estar pressionado. Isso exige rastrear todo o ciclo de entrada/ordens antes de afirmar que o fluxo adotado é idêntico ao original. O teste de elegibilidade não resolve essa discrepância.

## Métodos e limitações dos novos ensaios

`tools/trace_worker_policy.py` executa quatro rotinas completas de elegibilidade nativa, incluindo seus getters de tipo/estado, sobre registros sintéticos. São 22 classificações de depósito, 572 pares prédio/personagem e 312 combinações de grupo/recurso. Estados mortos são desconsiderados como no nativo. Evidência reproduzível: `reverse_engineering/worker_policy_vectors.json` e `reports/worker_policy_native_validation.json`.

`tools/trace_work_timing.py` executa trechos que inicializam o contador +0x8b. A configuração RECHARGE do INI é inserida na tabela 0x4965de+tipo*50; o retorno de BonusGetValEngin é uma entrada controlada. Dez combinações confirmam a subtração em byte; uma confirma recarga 50 após extração. Não confirma o carregador de INI, cálculo dos bônus, tratamento do contador zero ou frequência real. O remake só integra o caso básico sem bônus do fazendeiro/construtor. Evidência: `reports/work_timing_native_validation.json`.

## Próximos critérios de aceitação

1. Fechar economia da missão 1: Cultivo, estoque local, fome, população e ritmo real; observar uma sequência equivalente no original e no remake.
2. Reproduzir deslocamento e congestionamento por unidade; comparar trajetos, tempos e ordens interrompidas.
3. Fechar funções das estruturas e efeitos das pesquisas necessários na missão 1.
4. Reconstruir adversários/eventos presentes nessa missão e validar início → objetivo → vitória sem comandos de laboratório.
5. Só então expandir a mesma base validada às próximas fases.

Uma bateria automatizada demonstra invariantes e ausência das regressões cobertas. Ela não substitui uma comparação visual e interativa de partida completa contra o original.

## Permissões por personagem

A tabela limita-se à seleção de fonte e entrega manual, sem presumir capacidades completas de gameplay.

| ID | Nome original | Fonte permitida | Depósito elegível no original |
|---|---|---|---|
| 0 | Swordsman | Nenhuma nessa rotina | Nenhum nessa rotina |
| 1 | Scorcher | Nenhuma nessa rotina | Nenhum nessa rotina |
| 2 | Captain | Nenhuma nessa rotina | Nenhum nessa rotina |
| 3 | Killer | Nenhuma nessa rotina | Nenhum nessa rotina |
| 4 | Druid | Nenhuma nessa rotina | Nenhum nessa rotina |
| 5 | Farmer | Comida | Armazém e Cultivo |
| 6 | Mechanic | Peças | Armazém |
| 7 | Builder | Pedra | Armazém |
| 8 | Archer | Nenhuma nessa rotina | Nenhum nessa rotina |
| 9 | Settler | Nenhuma nessa rotina | Nenhum nessa rotina |
| 10 | Messenger | Nenhuma nessa rotina | Nenhum nessa rotina |
| 11 | Speeder | Nenhuma nessa rotina | Nenhum nessa rotina |
| 12 | Boomer | Nenhuma nessa rotina | Nenhum nessa rotina |
| 13 | Reaper | Comida | Armazém |
| 14 | Bomber | Nenhuma nessa rotina | Nenhum nessa rotina |
| 15 | Hellfire | Nenhuma nessa rotina | Nenhum nessa rotina |
| 16 | Heliped | Nenhuma nessa rotina | Nenhum nessa rotina |
| 17 | Balloon | Nenhuma nessa rotina | Nenhum nessa rotina |
| 18 | ARAIGNEE | Nenhuma nessa rotina | Nenhum nessa rotina |
| 19 | FOURMI | Nenhuma nessa rotina | Nenhum nessa rotina |
| 20 | Basher | Nenhuma nessa rotina | Nenhum nessa rotina |
| 21 | Raptor | Nenhuma nessa rotina | Nenhum nessa rotina |
| 22 | Hurler | Nenhuma nessa rotina | Nenhum nessa rotina |
| 23 | Bukka | Nenhuma nessa rotina | Nenhum nessa rotina |
| 24 | Snapper | Nenhuma nessa rotina | Nenhum nessa rotina |
| 25 | Stomper | Nenhuma nessa rotina | Nenhum nessa rotina |
