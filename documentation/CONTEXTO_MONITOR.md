# Monitor do jogo — contexto para conversa exclusiva

Atualizado em 2026-10-03. Escopo: ferramenta externa de monitoramento do Rising Lands. Consultar `AGENTS.md` e `CONTEXTO_ATUAL.md` na raiz da pesquisa, aplicando somente as partes pertinentes ao monitor.

## Caminhos e arquitetura confirmados

Raiz da pesquisa:
`C:\Users\Victor Freitas\Documents\Codex\2026-09-25\quero-que-acesse-uma-pasta-do\outputs\RisingLandsResearch`

Código do monitor: `source_remake/EnemyMonitor`.
Projeto: `EnemyMonitor.csproj`, Windows Forms, .NET 8 Windows.
Interface, leitura das sessões e atualização: `Program.cs`, classes Program e MonitorWindow.
Contrato JSON e leitura/escrita: `MonitorModel.cs`, namespace RisingLands.Monitoring. Conferir o vínculo desse contrato no projeto do jogo antes de modificá-lo.
Coleta/publicação pelo jogo: `source_remake/RisingLandsRemake/Engine/EnemyMonitorFeed.cs`.
Verificações de integração: `source_remake/RisingLandsRemake/Tests/MonitorV89Tests.cs`.
Dados em execução: `runtime/enemy-monitor/game-<processo>.json`.
Pacote do monitor atualmente referenciado pelos lançadores consultados: `releases/enemy-monitor-v89/RisingLandsEnemyMonitor.exe`. Os lançadores da entrega interactions-v102 também apontam para esse pacote; isso não confirma testes ou estado de toda a entrega v102.

## Comportamento atual

O jogo publica snapshots aproximadamente a cada 500 ms de tempo real. O monitor permite escolher a partida/processo, acompanha os três inimigos e exibe abas Economia e Tecnologia, detalhes das descobertas e progresso da pesquisa. O snapshot informa missão, tempo simulado, velocidade, estado da janela/partida e se o processo continua rodando. O contrato atual tem Schema=1 e valida identidades, recursos e tecnologias.

## Limites desta conversa

- Implementar os pedidos do usuário sobre informações exibidas, abas, interface, filtros, gráficos e funcionamento da ferramenta.
- Preservar combate, economia, custos, tecnologias, produção, missões e save/load. Um campo novo no monitor não deve mudar a simulação.
- Se um dado solicitado não existe no snapshot, ampliar somente o contrato e a coleta/publicação necessários, mantendo compatibilidade e a frequência de atualização controlada.
- Não inventar valores indisponíveis; mostrar indisponível ou explicar a limitação. Não implementar ferramentas de alterar recursos/tropas/IA, salvo pedido explícito do usuário para isso.
- Evitar leitura de memória arbitrária, alteração de processos ou manipulação dos saves: existe uma integração JSON própria.
- Usar dados de teste em diretório separado. Não sobrescrever arquivos da sessão ativa para testar o monitor.
- Coordenar mudanças no contrato e em EnemyMonitorFeed.cs com trabalhos em andamento no jogo; não sobrescrever alterações de outra conversa.

## Compilação e entrega

Usar o executável .NET local e DOTNET_CLI_HOME descritos no contexto comum. Não procurar novamente um runtime pelo disco. Publicar o monitor em um novo pacote próprio de `releases/`; não substituir o executável em uso nem selecionar um número sem conferir os pacotes existentes.

Se atualizar lançadores, preservar o vínculo com a versão correta do jogo. Quando a coleta mudar, o jogo também precisa de uma entrega compatível; não afirmar que uma interface nova recebe campos que o executável antigo não publica.

## Verificação proporcional

Conferir interface/renderização e atualização real; partida ausente, processo encerrado, dados incompletos, inimigos ausentes e múltiplas sessões quando relevantes. Para mudanças no contrato, verificar produtor e consumidor juntos. O monitor possui --data para fixtures, --check para validação de exibição e --capture/--tab technology para capturas. O jogo possui --test-monitor-v89 --output <diretório separado> para fixtures e integração; ler o teste antes de usá-lo.

## Registro das próximas entregas

Ainda não há mudanças de interface solicitadas nesta conversa. Após cada entrega, registrar aqui pedido, implementação, arquivos alterados, verificações, caminho do pacote e pendências. Não atualizar por conta própria a memória das missões para incluir regras novas de jogabilidade.
 
## Entrega 2026-10-03 — relação e força militar v1

Pedido: relação comigo com indicador numérico existente e total de combatentes por tipo.
Nova aba "Relação e força militar": Aliado/Hostil conforme Allied(owner), Simpatia numérica conforme Goodwill(owner), total militar e quantidades por tipo, inclusive zero. Simpatia é o inteiro real do jogo, não percentual nem estimativa. Ausentes mostram travessão. Dados antigos mostram Indisponível.
Coleta somente leitura: tipos com regras em AdvancedAttacks, excluindo mensageiro; unidades vivas do proprietário, inclusive recolhidas em prédios. Druidas e Helipede entram; trabalhadores, mortos e mensageiros não. Tipos avançados mantêm nomes do catálogo para evitar traduções inventadas.
Contrato Schema=1 ampliado com propriedades opcionais Relation, Sympathy e Military, mantendo leitura de snapshots antigos. Validação de tipos únicos e contagens não negativas.
Arquivos: source_remake/EnemyMonitor/MonitorModel.cs, Program.cs; source_remake/RisingLandsRemake/Engine/EnemyMonitorFeed.cs e Tests/MonitorV89Tests.cs.
Sem repositório Git na raiz consultada: originais dos quatro arquivos preservados antes da edição em experiments/monitor-relacao-militar/originals. Não foram alteradas regras da simulação ou saves.
Verificações: compilação de ambos; integração com fixtures isoladas, simulação serializada antes/depois, aliança e simpatia 23, contagens por tipo, tropas recolhidas, mortos e mensageiros; monitor --check para mixed, legacy, allied e three; captura visual da aba militar. Evidências em experiments/monitor-relacao-militar/. Nenhum arquivo de runtime/enemy-monitor foi usado como fixture.
Monitor para testar:
C:\Users\Victor Freitas\Documents\Codex\2026-09-25\quero-que-acesse-uma-pasta-do\outputs\RisingLandsResearch\releases\enemy-monitor-relacao-militar-v1\RisingLandsEnemyMonitor.exe
Jogo com coleta compatível:
C:\Users\Victor Freitas\Documents\Codex\2026-09-25\quero-que-acesse-uma-pasta-do\outputs\RisingLandsResearch\releases\interactions-v102-monitor-v1\RisingLandsRemake.exe
Lançador da missão 4 com monitor:
C:\Users\Victor Freitas\Documents\Codex\2026-09-25\quero-que-acesse-uma-pasta-do\outputs\RisingLandsResearch\releases\interactions-v102-monitor-v1\Jogar-Quarta-Missao-Com-Monitor.cmd
Pacotes existentes e lançadores da v102 original preservados. Novo pacote baseado no código compartilhado disponível nesta sessão, com alteração de produção restrita ao feed/contrato. Não substitui a entrega principal das missões.
Limites: validação com partidas de teste; não foi aberta a partida ativa do usuário nem feita bateria completa de campanhas. Executáveis antigos do jogo não publicam os novos campos; reiniciar usando a entrega compatível é necessário. Atualização usa o mesmo timer de 500 ms.

## Correção 2026-10-03 — abas separadas v2

Pedido: Relação comigo e Força militar em abas independentes.
Program.cs do monitor agora cria quatro abas: Economia, Tecnologia, Relação comigo e Força militar. Relação tem uma linha por clã com situação, aliança e simpatia; militar tem total e quantidade por tipo sem repetir diplomacia.
Diagnóstico somente leitura dos feeds reais: processo 12724 não publica os campos; processo 12304 publica Relation e Military. Portanto o Indisponível da captura veio da sessão selecionada sem coleta compatível. Consulta de caminho de processo via CIM negada; não inferimos qual executável iniciou 12724.
Seletor agora identifica Indicadores completos/limitados e prefere uma sessão completa na seleção inicial entre sessões conectadas. Escolha explícita do usuário permanece preservada. Rodapé orienta sobre versão limitada.
Somente Program.cs alterado; contrato, jogo e saves não alterados nesta correção. Cópia anterior em experiments/monitor-abas-v2/Program.before.cs.
Verificações: publicação sem erros; --check em mixed, legacy, allied, three e duas sessões simultâneas isoladas (completa escolhida apesar de limitada mais recente); capturas visuais das duas abas em experiments/monitor-abas-v2/relation.png e military.png.
Monitor para testar:
C:\Users\Victor Freitas\Documents\Codex\2026-09-25\quero-que-acesse-uma-pasta-do\outputs\RisingLandsResearch\releases\enemy-monitor-relacao-militar-v2\RisingLandsEnemyMonitor.exe
Lançador apenas do monitor (usa partida existente):
C:\Users\Victor Freitas\Documents\Codex\2026-09-25\quero-que-acesse-uma-pasta-do\outputs\RisingLandsResearch\releases\enemy-monitor-relacao-militar-v2\Abrir-Monitor.cmd
Lançador missão 4 com jogo compatível da entrega anterior:
C:\Users\Victor Freitas\Documents\Codex\2026-09-25\quero-que-acesse-uma-pasta-do\outputs\RisingLandsResearch\releases\enemy-monitor-relacao-militar-v2\Jogar-Quarta-Missao-Com-Monitor.cmd
Partida ativa preservada, sem fechar ou reiniciar processos. Pacotes anteriores preservados. Jogo compatível permanece interactions-v102-monitor-v1; monitor novo não cria dados faltantes em executáveis antigos.

## Entrega 2026-10-03 — força militar em matriz v3

Pedido: uma linha por inimigo e uma coluna por tipo de tropa, com quantidade na interseção.
Somente source_remake/EnemyMonitor/Program.cs alterado após conferir versão corrente; anterior preservada em experiments/monitor-matriz-v3/Program.before.cs. Economia, Tecnologia e Relação comigo mantidas separadas.
Força militar tem exatamente três linhas e colunas Espadachim, Incendiário, Capitão, Killer, Druida, Arqueiro, Speeder, Boomer, Bomber, Hellfire, Helipede e Stomper. Tipos adicionais publicados pelo contrato recebem coluna própria. A coluna Inimigo fica fixa durante rolagem horizontal; cabeçalhos e números legíveis. Tipo presente na coleta com contagem zero mostra 0; inimigo ausente mostra travessão; campo/tipo sem coleta mostra Indisponível, sem inventar zeros.
Verificações: publicação, --check em mixed, legacy, allied e three, conferindo contagem em cada coluna; captura visual experiments/monitor-matriz-v3/military.png. Todos os comandos concluíram com código zero. Fixture mixed mostrou 1 espadachim e 1 arqueiro na linha Inimigo 1, demais tipos com zero. Dados separados; nenhuma alteração ao jogo, contrato, runtime ou saves.
Monitor para testar:
C:\Users\Victor Freitas\Documents\Codex\2026-09-25\quero-que-acesse-uma-pasta-do\outputs\RisingLandsResearch\releases\enemy-monitor-relacao-militar-v3\RisingLandsEnemyMonitor.exe
Abrir apenas monitor:
C:\Users\Victor Freitas\Documents\Codex\2026-09-25\quero-que-acesse-uma-pasta-do\outputs\RisingLandsResearch\releases\enemy-monitor-relacao-militar-v3\Abrir-Monitor.cmd
Missão 4 com monitor e jogo compatível:
C:\Users\Victor Freitas\Documents\Codex\2026-09-25\quero-que-acesse-uma-pasta-do\outputs\RisingLandsResearch\releases\enemy-monitor-relacao-militar-v3\Jogar-Quarta-Missao-Com-Monitor.cmd
Jogo compatível permanece interactions-v102-monitor-v1; versões antigas continuam sem os campos. Pacotes anteriores e partida ativa preservados. Para ver todas as tropas na janela estreita, use a barra horizontal inferior.

## Entrega 2026-10-04 — migração, atalhos e identidades v4

Raiz atual confirmada: D:\CHATGPT\outputs\RisingLandsResearch. Caminhos em C: nos registros anteriores são históricos.
Causa da interface antiga: fonte migrada já tinha matriz v3, mas releases/interactions-v105/Abrir-Monitor.cmd e Jogar-Quarta-Missao-Com-Monitor.cmd apontavam para enemy-monitor-relacao-militar-v1.
Publicado monitor v4 com as quatro abas e matriz de três linhas por tipos de tropa. Identidade exibida em todas as tabelas e seletor de pesquisa: Inimigo 1 · Vermelho, Inimigo 2 · Amarelo, Inimigo 3 · Verde; mapeamento confirmado em EnemyPersonalities.For(owner). Nomes são identificação por clã/cor, sem inventar nomes próprios. Nenhuma alteração a regras, coleta, contrato, executável do jogo ou saves.
Program.cs preservado antes da edição em experiments/monitor-identidades-v4/Program.before.cs. Dois lançadores da v105 conferidos e preservados nessa pasta antes de apontarem para v4. Atalhos do novo pacote usam jogo v105, mantendo os avanços atuais do jogo.
Verificações: publicação sem erros/avisos; --check em mixed, legacy, allied e three; captura visual de três inimigos com cores em experiments/monitor-identidades-v4/military.png. Sem alterar feeds da partida ativa.
Monitor para testar:
D:\CHATGPT\outputs\RisingLandsResearch\releases\enemy-monitor-relacao-militar-v4\RisingLandsEnemyMonitor.exe
Abrir somente o monitor:
D:\CHATGPT\outputs\RisingLandsResearch\releases\enemy-monitor-relacao-militar-v4\Abrir-Monitor.cmd
Missão 4 com monitor:
D:\CHATGPT\outputs\RisingLandsResearch\releases\interactions-v105\Jogar-Quarta-Missao-Com-Monitor.cmd
Pendências/limites: versões históricas dos pacotes mantêm seus monitores; usar o novo atalho ou lançadores corrigidos da v105. Nenhum processo do usuário foi fechado. Identificação Verde acompanha personalidade atual do jogo; minimapa antigo tem representação azul no terceiro proprietário, divergência visual fora do escopo desta entrega.
