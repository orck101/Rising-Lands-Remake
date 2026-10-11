# Rising Lands — versão v67

Atualização de 02/10/2026. Executável separado em `releases/interactions-v67`. Os três atalhos abrem as missões 1, 2 e 3. O jogo continua em reconstrução; os limites abaixo fazem parte da entrega.

## Resultado das oito solicitações

| Item | Implementação e limite |
|---|---|
| 1. Carro de alimento | Reaper, tipo 13, utiliza os frames originais de trabalho nas 16 direções. Coleta, transporta e entrega comida em ciclo. Capacidade básica nativa: 5. A sondagem não encontrou uma troca de aparência entre movimento vazio e carregado; não foi inventado um sprite adicional. A cadência absoluta do executável original ainda não foi medida em execução real. |
| 2. Fome | Indicador gráfico original, reserva individual, relógio de atividade e perda de 20 pontos de vida quando esgotada. A unidade procura comida solta alcançável e depois armazém próprio habitado, interrompendo e retomando a tarefa. A carga de trabalho é preservada. Bônus nativos de agricultura ampliam o intervalo de fadiga. A ordem de busca, a preservação da carga e a retomada são adaptações explícitas do controlador; não foram declaradas equivalentes a toda a IA original. Redução gradual e genérica de inteligência/eficiência ainda não comprovada. |
| 3. Sons | Bancos originais A/B/P integrados aos eventos implementados: seleção, ordens, deslocamento, trabalho, martelo, entrada/saída, recrutamento, ataques, dano, morte, fome, pesquisa e vitória. Todos os 551 clips estão no pacote e coincidem com o decodificador original. Isso não significa que todos os 391 grupos de eventos já ocorram no remake: eventos de mecânicas ainda ausentes permanecem pendentes. |
| 4. Ajustes de áudio | Menu → Ajustes de som: volume geral, música, efeitos, avisos e vozes, com aumentar/diminuir/silenciar e persistência entre sessões. Há falas nos bancos, portanto o canal separado de vozes foi incluído. As faixas de música do CD não estão na cópia disponível; seu controle guarda a preferência, mas não há música sendo reproduzida. A ausência aparece no próprio menu. |
| 5. Capitães | Formação reserva posições protegidas em setores diferentes para múltiplos capitães, buscando cobrir o grupo com raio 10. Maior alcance mantém o centro; combate próximo ocupa as posições externas. Grupos pequenos não impõem separação rígida. É uma melhoria solicitada de formação, não uma alegação de algoritmo original recuperado. |
| 6. Stomper | Retrato original `0x29e1` no circo, na seleção individual e na lista de classes. Clique no retrato foi testado até a conclusão da produção da unidade tipo 25. |
| 7. Comércio | Removida a ação e a tela genérica de “proposta de troca”. Selecionar o mercado habitado abre o painel compacto original de materiais, com retratos, miniaturas e confirmação originais. Contato do mensageiro, estoques, débito/crédito e avaliação neutra foram mantidos/corrigidos. Comércio de tropas, seus transportes/hangares e diplomacia completa ainda precisam ser reconstruídos; este item está parcial. |
| 8. Desempenho | Consultas de prédios/obras usam índice por célula; regras e ocupantes usam consultas reutilizadas; objetos decorativos são desenhados somente na região visível. Foram comparados os mesmos cenários e medidos desenho/atualização com 313 unidades e 200 obras em 1x e 5x. Caminhos simultâneos, combates grandes, latência da GPU e todas as causas possíveis de travadinhas ainda não estão cobertos por esse perfil. |

## Como conferir no jogo

- Produza o Reaper na oficina após sua liberação. Selecione-o e use Ctrl mantido: fonte de alimento → armazém. Observe trabalho, transporte e repetição.
- Selecione uma unidade humana, passe o mouse ou mantenha Espaço para ver o indicador de alimento original. Ele é um pequeno marcador circular segmentado junto aos pés, não uma barra retangular nova.
- Fome considera atualizações de atividade. A reserva importada varia conforme o personagem; deixar todo mundo parado não é um teste equivalente a trabalhar/marchar continuamente. Veículos que a classificação nativa exclui não recebem uma fome inventada.
- Use Esc → Ajustes de som. Os valores ficam em `UserSettings/audio.json`, separados dos saves. Vozes e avisos podem ser silenciados independentemente dos efeitos.
- Selecione tropas com dois ou mais capitães e escolha quadrado, linha ou triângulo. O alcance combinado pontilhado permanece disponível.
- No circo, confira o retrato do Stomper e sua produção. O menu Testes permite liberar pesquisas mais cedo para esses testes; não concede recursos nas partidas normais.
- Para o mercado, envie um mensageiro até um mercado habitado de outro jogador. No painel de materiais, escolha os retratos de cima/baixo para definir quantidades, use dígitos ou setas, Enter para confirmar a quantidade e o símbolo original inferior direito para executar. Shift altera dez unidades. Esc cancela a quantidade ou fecha o painel.

## Evidência técnica

Arquivo examinado: `working/RISING/RISING-ORIG.EXE`, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. As sondagens executam rotinas isoladas em memória e interceptam desenho/áudio. Não executam o processo Windows original nem escrevem seus arquivos.

| Conclusão | Fonte / região | Método / certeza |
|---|---|---|
| Frames de trabalho e movimento do Reaper | `AffEngin`, VA `0x415a27`; registro compacto +10/+112 e recurso +16 | CONFIRMADO nas 96 amostras de direção/estado/fase. `reverse_engineering/reaper_v67.json`; `tools/trace_reaper_v67.py`. Equivalência da aparência nas amostras não prova todos os estados possíveis. |
| Reserva, relógio e dano de fome | `CalculFatigue`, VA `0x452cc8`; +4 reserva, +114 atividade, +116 relógio, +118 máximo, +154 vida | CONFIRMADO: 10 vetores de limiar/dano. Sem comida, dano 20; relógio após dano não fatal volta à metade do período básico, inclusive com bônus. `reverse_engineering/hunger_v67.json`. |
| Classes sujeitas à fadiga | `TypeEnginEstFatigable`, VA `0x442cd8`; `RISING.INI`, FATIGUE | CONFIRMADO: 26 classificações; período básico configurado de 3000 atualizações. Não representa uma medição de segundos do original. |
| Pesquisas de fadiga | `InventionSetEffet`, VA `0x470e88`; getter `0x428d58`, parâmetro 0 | CONFIRMADO: 520 combinações de nível/tipo. Bônus do getter somado ao período no código nativo. Para o fazendeiro, níveis de agricultura 14/18 produzem intervalos 7500/9000. Bônus de jogadores inimigos ainda não reconstruídos a partir de suas campanhas. `fatigue_technology_v67.json`. |
| Indicador gráfico de alimento | `AffInfoSelection`, região `0x410dba` | CONFIRMADO: reserva somada ao sprite `0x1e2b`, posição relativa -8/-2. O contexto de exibição usa as regras já extraídas de seleção/mouse/Espaço. |
| Retrato do Stomper | `IconeFillContenu`, região `0x45e3f8`; `AffPersonnage`, VA `0x4643b3` | CONFIRMADO: tipo 25 usa índice de retrato 18, base `0x29cf`, resultando `0x29e1`. Captura verifica também a unidade produzida. |
| Bancos e chamadas de som | `PlaySnd_PostSound`, VA `0x48a625`; A.HED/SND, B.HED/SND, P.HED/SND | CONFIRMADO: 36 contextos de chamada catalogados, 391 grupos, 551 clips validados byte a byte. `audio_events_v67.json`, `audio_full_original_validation.json`. |
| Seleção/ordens | `SelectionEngin` região `0x4744ea`; movimento `0x473e28`; coleta `0x473204` | FORTE EVIDÊNCIA dos argumentos: ações P 8 / 0x0b / 0x0f. Seleção e movimento compartilham ação 8 em contextos nativos distintos. Frequência/mistura moderna adaptadas. |
| Trabalho, martelo e combate | Regiões `0x44d9e5`, `0x44b33d`, `0x440bfd`, `0x45063f`, `0x444ac3` | FORTE EVIDÊNCIA: ações P 0x10 / 0x25 / 7 / 1 / 5. Eventos modernos seguem suas fases de simulação; não representam mistura DirectSound perfeitamente reconstruída. |
| Painel de materiais | `AffMarcheProposition`, VA `0x465a20`; fundo `0x2af8` | CONFIRMADO para comandos de desenho capturados; estoque/contadores preenchidos pelo estado moderno. Digitação de quantidades é uma adaptação, não a janela nativa inteira recuperada. `tools/trace_market_visuals_v67.py`. |
| Valor dos materiais | `EvalueCoutProposition`, VA `0x466a40`; tabela `0x496f7c` | CONFIRMADO em 180 vetores com diplomacia neutra: ordem nativa engrenagem/comida/pedra, pesos 2/1/1. Oferta neutra recebe desconto de 20%, com divisão inteira antes do percentual final. `market_values_v67.json`. |
| Aceitação movimenta recursos | `MarcheEchangePrend` `0x4614b5`; `MarcheEchangeDonne` `0x46172e`; `BatimentEnleveMatiere` `0x423655` | FORTE EVIDÊNCIA por fluxo de chamadas e escrita de estoque. Créditos/débitos de materiais ocorrem na aceitação; o transporte/hangar encontrado é da negociação de personagens. Não se aplicou um transporte fictício aos materiais. |

## Correções de interpretações anteriores

A v66 interpretava os pesos nativos como comida/pedra/engrenagem. A ordem nativa correta é engrenagem/comida/pedra; com relação neutra, 10 alimentos compram 8 pedras, não 16. Os vetores dos saves modernos continuam na ordem comida/pedra/engrenagem para manter compatibilidade. Cada nova transação identifica a avaliação correta; transações antigas preservam sua avaliação histórica ao carregar.

O tipo 13 é o Reaper de alimento. A opção indevida de reparador associada a ele foi retirada da interface. Isso não afirma que todos os demais comportamentos da oficina já foram recuperados.

## Som e controles de volume

Máximo de 16 instâncias simultâneas. Avisos/vozes podem liberar uma vaga ocupada por efeito. Há limitação por som/fonte, escolha entre variantes originais, panorama espacial e rejeição de efeitos fora da visão/tela. Preferências são escritas de forma atômica. Essas regras evitam acúmulo em 5x, mas são adaptações modernas: prioridades e loops completos de DirectSound não estão todos emulados.

Todos os 551 clips também passaram por decodificação no próprio runtime do jogo e reprodução silenciosa no dispositivo de áudio. Isso verifica formato/dispositivo, não substitui avaliação auditiva da mistura. Ver `reports/v67-audio-device-final.json` e a captura correspondente.

## Desempenho medido

Mesmo mapa, 313 unidades, 131.072 consultas por cenário, mesmos resultados das consultas. Tempo para consultar cobertura de obras e passagem:

| Obras/prédios sintéticos | Antes | Depois, pacote final |
|---|---:|---:|
| 0 | 33,44 ms | 16,58 ms |
| 50 | 1026,06 ms | 5,69 ms |
| 200 | 2947,66 ms | 5,32 ms |

Esse ganho é da consulta específica, não um multiplicador de FPS. A média de passo da simulação no cenário com 200 passou de 5,55 para 4,80 ms. Dados: `reports/v67-performance-before.json` e `v67-performance-after-release.json`.

Perfil adicional de atualização/desenho, 313 unidades e 200 obras, mesma câmera, descoberta do mapa ativa, descartando dez quadros iniciais:

| Velocidade | Atualização média / P95 | Desenho médio / P95 |
|---|---:|---:|
| 1x | 0,56 / 0,71 ms | 7,76 / 12,70 ms |
| 5x | 2,56 / 3,22 ms | 8,32 / 21,82 ms |

As unidades adicionadas nesse perfil estão paradas. Medição de CPU, excluindo espera da GPU. Há picos nos relatórios; não foi declarado que toda travadinha acabou. Estudos posteriores devem incluir muitas rotas emitidas juntas, combate e coleta simultâneos. Dados: `reports/v67-performance-1-release.json` e `v67-performance-5-release.json`.

## Saves, preservação e validação

Formato moderno v29 inclui reserva/relógio de alimento, tarefas interrompidas e registro de comida consumida. Guarda carga, destinos, progresso e conservação dos recursos. Novos slots recebem sufixo `research-v29.json`; anteriores continuam visíveis. A migração inicializa os indicadores ausentes em saves antigos e não reescreve os seis arquivos reais usados na inspeção.

As 18 baterias somam **4.169 verificações aprovadas**, incluindo 47 específicas desta versão. Foram conferidos coleta/entrega depois de comer, estoque e carga conservados, morte por fome, continuidade após carregar, rejeição de dados inconsistentes, formações, 65.536 células de cobertura, cinco slots, transações, as três missões e funcionalidades anteriores. Relatório: `reports/v67-validation.json`.

`original/RISING` e `working/RISING`: todos os 670 arquivos coincidem com o inventário inicial. Na instalação viva `D:/RISING`, os quatro arquivos save0..save3 já diferem daquele inventário histórico; a comparação registra essa diferença sem sobrescrevê-los e não determina quando ou por quem mudou. Este trabalho não escreveu na instalação. Ver `reports/v67-preservation.json`.

Capturas: Reaper `v67-reaper-final2.png`; fome `v67-hunger-final.png`; áudio `v67-audio-device-final.png`; Stomper `v67-stomper-final.png`; capitães `v67-captains-release.png`; mercado `v67-market-final2.png`. Todas ficam em `reports`.

## Pendências preservadas para continuidade

1. Comércio de personagens: estoques de tropas nos hangares, transporte, entrega/recebimento e mudanças de proprietário; diplomacia além da relação neutra.
2. Música original: localizar as faixas ausentes do CD antes de integrar a reprodução.
3. IA nativa de alimentação e consequências adicionais da fome: comprovar estados/regras restantes, sem inventar uma penalidade genérica de “inteligência”.
4. Eventos sonoros de funcionalidades ainda ausentes e reprodução exata de loops/prioridades.
5. Perfil de grandes ordens simultâneas e combates, com medição de GPU se necessária.

Não use “oito itens totalmente concluídos” como descrição desta versão. O comércio completo, a música e os limites de fidelidade acima continuam abertos.
