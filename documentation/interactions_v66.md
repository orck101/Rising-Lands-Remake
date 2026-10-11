# v66 — feitiços, comércio de recursos e voo

Entrega: `releases/interactions-v66`. Iniciadores das missões 1, 2 e 3. Save v28; leitura dos anteriores mantida. F9 abre os cinco slots. A instalação em D:/RISING e os saves existentes não foram alterados.

## Como utilizar

- **Biblioteca habitada:** os retratos originais mostram os feitiços liberados pela espiritualidade. Clique para aprender. Recursos são descontados uma vez; o progresso permanece ao salvar. Retirar todos os colonos interrompe o aprendizado, preservando seu avanço. A seta original abre a quinta opção.
- **Druida:** aprenda o feitiço na biblioteca, selecione somente o druida, clique no ícone e depois no alvo. Botão direito cancela a escolha. As opções ficam cinza sem mana. Aproximar o druida de um cristal seu recupera mana. As pesquisas de capacidade e recuperação modificam esses valores.
- **Mercado habitado:** forme um mensageiro e mande-o até um mercado habitado de outro jogador. Depois do contato, o ícone do seu mercado abre a proposta. Ajuste comida, pedra e engrenagens oferecidas/recebidas; Shift altera dez de cada vez. A confirmação valida os dois estoques e transfere os recursos sem criá-los. Propostas recusadas não debitam nada.
- **Helípede:** após sua liberação, produza no heliporto habitado. Ordens de movimento atravessam água, vegetação e construções, permanecendo dentro do mapa. A classe aérea também se aplica ao balão; seu transporte e obtenção por comércio ainda não estão implementados.
- **Outras tropas:** capitão beneficia ataques próximos; reparador recebe ordem de consertar um veículo seu danificado. Ataques das tropas avançadas usam seus sprites e momentos nativos de disparo.

Essas funções respeitam as tecnologias da campanha. O menu Testes permite elevar os níveis para verificá-las mais cedo. Os recursos aumentados nos testes técnicos e nas capturas não são concedidos às partidas normais.

## Regras e evidências

Executável examinado: `working/RISING/RISING-ORIG.EXE`, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`. As sondagens executam rotinas isoladas em memória, com chamadas de desenho interceptadas. O processo Windows original não foi executado por esses scripts.

| Descoberta | Origem / endereço | Método e certeza |
|---|---|---|
| Tipos aéreos 16 e 17 | TypeEnginEstAerien, 0x442373 | CONFIRMADO: consulta dos 27 tipos emulados |
| Cinco feitiços e seus parâmetros | RISING.INI; tabela 0x497120, registros de 46 bytes | CONFIRMADO: 25 consultas dos getters. SortLoadCaract eleva PORTEE ao quadrado; o remake compara distância quadrática |
| Liberação por espiritualidade | InventionSetEffet 0x470e88; flags 0x508f34 | CONFIRMADO: 81 invenções sondadas; níveis 4/6/9/10/13 liberam Visão/Petrificação/Espelho/Armadura/Nova |
| Flags e duração dos efeitos | EnginSetSortilegeEffet 0x44ff7f; EnginSubTempsSortilege 0x44ff4a | CONFIRMADO: máscara no registro +220; temporizadores +228/+232/+236, inicializados e decrementados |
| Mana inicial | Registro compacto da unidade +218, equivalente a +730 na parte expandida importada | CONFIRMADO: layout do registro e acessos EnginGetMana; importador preserva a mana de druidas nos mapas |
| Mana máxima básica 10 | EnginGetMaxMana 0x448270 | CONFIRMADO em sondagem com modificador interceptado; efeitos de pesquisa consultados na tabela de invenções |
| Recuperação perto do cristal | CristalCherchePretre 0x424a76 | FORTE EVIDÊNCIA: distância quadrática até 16, período básico de 60 atualizações, ou 600/bonus9 |
| Capitão próximo concede +2 | EnginInfligeDegat 0x4502f0 e DIVERS/BONUSCAPITAINE | FORTE EVIDÊNCIA: chamada de busca do tipo 2 em raio 10 e soma do valor configurado |
| Recursos de comércio valem 2/1/1 | Tabela 0x496f7c, EvalueCoutProposition 0x466a40 | CONFIRMADO para valores; FORTE EVIDÊNCIA para aritmética da proposta e limiar 100% em 0x466e83 |
| Trocas entre dois estoques | MarcheOrdreEchange 0x461a7e e rotinas Prend/Donne | FORTE EVIDÊNCIA de transferências bilaterais; controlador moderno valida e aplica a transferência de forma indivisível |
| Momentos de ataque e desenhos | CalculAttaque 0x451914; AffEngin 0x415a27 | CONFIRMADO: primeiro disparo/impacto de 12 tipos nas 16 direções; sprites nativos extraídos |
| Desenhos de projéteis/efeitos | AffProjectile 0x46fe60, fase no registro +24 | CONFIRMADO: 192 registros de desenho, 12 tipos em 16 fases |
| Movimento do balão | InitTroupe 0x457b87; AffEngin 0x415a27 | CONFIRMADO: 48 frames originais extraídos; sem inventar botão de produção ausente nos menus nativos |

Os comentários de RISING.INI confundem Espelho e Armadura nos níveis 9/10. A implementação usa os IDs efetivamente escritos pela rotina de invenções.

| Feitiço | Mana | Alcance em quadrados | Duração interna | Custo de aprendizado: comida/pedra |
|---|---:|---:|---:|---:|
| Visão | 2 | 20 | 200 | 6/4 |
| Petrificação | 2 | 10 | 400 | 14/10 |
| Espelho | 6 | 10 | perde 1 vida a cada 20 atualizações | 30/16 |
| Armadura | 4 | 10 | 400 | 20/12 |
| Nova | 8 | 20 | efeito gráfico 10 | 16/10 |

Esses números são atualizações internas, não segundos comprovados do jogo antigo. O relógio atual do remake executa 30 atualizações por segundo em 1x. Espelho cria uma unidade temporária que pode receber ordens e atacar; não pode entrar no santuário para gerar colonos permanentes. Nova aplica 90 de dano na área 3×3, considerando o efeito de armadura e atingindo também aliados nessa área. Visão contribui para o sistema de descoberta do mapa e expira normalmente.

## O que permanece adaptado ou incompleto

- **Comércio:** esta entrega troca recursos. O original também oferece unidades, diplomacia, simpatia e transporte. A avaliação atual usa relação e simpatia neutras; exemplo: 10 alimentos por 16 pedras atende ao limiar de 100%. A tela reutiliza o fundo e os ícones originais, mas sua disposição é uma adaptação do remake, não a reprodução do painel de comércio original.
- **Voo:** navegação aérea está funcional. Não inclui embarque/desembarque do balão nem todos os estados de altitude do original. O balão não recebeu um botão de produção inventado.
- **Feitiços:** custos, IDs, liberação, mana e durações têm evidência nativa. A resolução e a apresentação de alguns efeitos são adaptadas: Nova resolve dano imediatamente; Espelho não duplica ocupantes de veículos. Armadura não recebeu um efeito gráfico de outro feitiço para disfarçar a falta da extração de seu desenho específico.
- **Ataques avançados:** momento de disparo e sprites são originais. Projéteis avançados usam o movimento linear do controlador existente; todas as trajetórias, explosões secundárias e recargas ainda não foram reproduzidas. Armadilhas do Bomber permanecem pendentes.
- **Reparador:** restaura dois pontos de vida a cada 20 atualizações em veículos aliados. Custos adicionais e reparo de prédios ainda precisam de confirmação e implementação.

Essas limitações impedem afirmar que toda a jogabilidade avançada já é idêntica ao original. As três áreas passam a ter implementação jogável; a documentação diferencia isso de uma reconstrução completa.

## Validação e continuidade

Relatório: `reports/v66-validation.json`. Logs: `logs/v66-*.log`. Testes novos: `Tests/SpecialV66Tests.cs`; 71 verificações de aprendizado, custos, alvos, mana, status, clone, combate avançado, voo, comércio, conservação e salvar/carregar, incluindo rejeição de transação adulterada sem alterar a partida.

Regressões abrangem missões 1/2, coleta, cultivos, construção, pesquisa, produção, descoberta, estoque, zoom, formações e alcance combinado. Cinco saves reais anteriores foram lidos sem regravar, com verificação de preservação. Capturas conferidas visualmente: `reports/v66-library.png`, `v66-druid.png`, `v66-market.png`, `v66-flight-final.png`. A captura do druida também testa o clique real no ícone: isso identificou e corrigiu o conflito com o painel de formação na seleção individual.

Ferramentas reproduzíveis: `tools/trace_special_v66.py`, `trace_advanced_attack_v66.py`, `trace_effect_visuals_v66.py`, `trace_balloon_visuals_v66.py`. Dados: `source_remake/Data/special_rules.json`, `advanced_attack.json`, `special_visuals.json`, `unit_rendering.json`. Evidências adicionais em `reverse_engineering/*v66.json`. Registro de alterações: `logs/v66-changes.json`.
