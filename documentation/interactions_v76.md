# v76 â€” dados originais da IA reutilizÃ¡veis entre missÃµes

## Entrega

O motor de adversÃ¡rios deixa de depender exclusivamente do nome MONDE04. A partir da missÃ£o 4, um mapa original com catÃ¡logo correspondente e populaÃ§Ã£o adversÃ¡ria ativa o mesmo motor. As missÃµes iniciais mantÃªm o comportamento anterior. Isso prepara a campanha; nÃ£o comprova a reproduÃ§Ã£o dos objetivos e eventos de todas as fases.

A expansÃ£o de armazÃ©ns usa agora posiÃ§Ãµes registradas no arquivo original, em vez de procurar posiÃ§Ãµes arbitrÃ¡rias ao redor do depÃ³sito. A mÃ¡scara de tipos de construÃ§Ã£o Ã© respeitada. A construÃ§Ã£o ainda exige construtor, recursos reais, terreno vÃ¡lido e Ã¡rea disponÃ­vel. Os estoques dos jogadores continuam separados.

## EvidÃªncias confirmadas

- ExecutÃ¡vel analisado: cÃ³pia RISING-ORIG.EXE, SHA256 `af05f6e4ce93c116bff4b08b4d0e37e59e185b448bcc51f161a838bdbd3fca9e`.
- `LoadBatimentGhost`, endereÃ§o `0x47b957`: lÃª quantidade u16 e, para cada posiÃ§Ã£o, cÃ©lula u16 seguida de mÃ¡scara u32. Cada registro tem **6 bytes**; nÃ£o Ã© um registro de prÃ©dio de 293 bytes.
- `GetCaseOfGhost`, `0x47b72e`: consulta a cÃ©lula do registro.
- `TestTypeBatimentPossible`, `0x47b714`: um bit **zerado** permite o tipo; um bit marcado o proÃ­be.
- `GetNumIndiceTypeGhost`, `0x47b7ed`: percorre os registros na ordem do arquivo, ignora cÃ©lula 65535 e retorna a primeira posiÃ§Ã£o compatÃ­vel, ou 65535.
- A extraÃ§Ã£o de 34 mapas contÃ©m 12.332 posiÃ§Ãµes. Foram executadas 13.114 verificaÃ§Ãµes diferenciais contra o cÃ³digo nativo: leitura completa, cÃ©lulas e consultas dos 22 tipos.
- `source_remake/Data/opponent_plans.json` preserva hash do mapa e offset de cada registro. O remake rejeita um catÃ¡logo que nÃ£o corresponda ao mapa carregado.

## IntegraÃ§Ã£o e limites

Os registros nÃ£o possuem um campo de proprietÃ¡rio. Portanto, associar uma posiÃ§Ã£o Ã  cidade de um adversÃ¡rio continua sendo uma polÃ­tica provisÃ³ria: procura perto do depÃ³sito daquele jogador e nÃ£o ocupa posiÃ§Ãµes jÃ¡ construÃ­das/reservadas. O gatilho de estoque baixo de espaÃ§o e o limite de trÃªs armazÃ©ns tambÃ©m continuam provisÃ³rios.

A exploraÃ§Ã£o foi limitada a oito candidatos por decisÃ£o, preservando o prÃ³ximo Ã­ndice para retomar depois. Isso limita buscas repetidas em mapas grandes; nÃ£o traduz uma regra comprovada do original.

Save continua v32. O catÃ¡logo Ã© imutÃ¡vel, associado ao hash do mapa, e nÃ£o requer novo campo no save. Ordens e estado adversÃ¡rio continuam salvos.

## Trabalho necessÃ¡rio para fidelidade integral

1. Traduzir o interpretador RULE e seus 1.157 registros, com comparaÃ§Ã£o das decisÃµes contra o executÃ¡vel.
2. Reproduzir grupos, prioridades e interrupÃ§Ãµes dos nÃºcleos de unidades, cidade, recursos e guerra.
3. Decodificar as condiÃ§Ãµes das ondas de ataque, zonas de influÃªncia e eventos de vitÃ³ria por missÃ£o.
4. Reconstruir tecnologias adversÃ¡rias, planos completos de produÃ§Ã£o e pesquisa.
5. Reproduzir iniciativa diplomÃ¡tica, habilidades e destruiÃ§Ã£o de prÃ©dios com efeitos originais.

A v76 **nÃ£o equivale Ã  IA integral original**. Substitui uma parte inventada por dados comprovados e torna o motor atual reutilizÃ¡vel. Os testes de outras missÃµes sÃ£o testes de funcionamento e continuidade de save, nÃ£o uma aprovaÃ§Ã£o da fidelidade de toda a campanha.

Originais e saves de D:/RISING nÃ£o foram modificados.

## Validação realizada

543 verificações específicas da v76, testes gerais, regressão dos 28 cenários adversários da v75 e seus testes de slots, inspeção de saves existentes sem escrita. As missões disponíveis entre 1 e 25 foram simuladas por 60 ticks, carregadas de um save e continuadas por mais 30 ticks em duas simulações idênticas. A missão 4 também conserva a simulação prolongada da regressão v75. Isso não substitui partidas completas de cada fase. Logs e contagens: reports/v76-validation.json. Manifesto de arquivos: reports/v76-source-release-manifest.json.
