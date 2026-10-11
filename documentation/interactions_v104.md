# v104 — personalidades fixas por cor

Entrega: 2026-10-04. Configuração solicitada pelo usuário para a IA comum do remake; não é uma regra recuperada do original.

| Dono/cor | Perfil | Ramos prioritários |
|---|---|---|
| 1 — Vermelho | Exército sustentado | Agricultura e Armamento |
| 2 — Amarelo | Fortaleza produtiva | Agricultura e Engenharia |
| 3 — Verde | Indústria militar | Engenharia e Armamento |

Mapeamento conferido na tabela de paleta usada por PlayerColors.cs e no PLATGAME.PIE da cópia de trabalho. Implementação central: Engine/EnemyPersonalities.cs. A configuração é comum a todas as missões que usam a IA econômica atual.

## Comportamento aplicado

- No começo de uma nova missão, escolhe um dos ramos prioritários disponíveis. Mantém Engenharia mínima para laboratório quando necessário; compara avanço relativo dos dois ramos com referências do estudo v99. Não concede níveis. Referências vermelha 11/20, amarela 15/16 e verde 18/18 orientam a proporção, não são níveis iniciais ou tetos.
- Ramos concluídos ou bloqueados pela missão são excluídos da escolha. Há alternativa legal para evitar paralisar a pesquisa por causa da especialização. Espiritualidade só pode ser alternativa a partir da missão 7.
- A pesquisa atual de um save restaurado não é trocada nem perde progresso. A escolha nova acontece ao entrar na próxima missão. O algoritmo original de sorteio é preservado como ferramenta de pesquisa/validação, mas a seleção da IA moderna usa a configuração do usuário.
- Depois dos serviços básicos, da reserva e da segurança, vermelho prioriza suporte militar; amarelo prioriza circo e infraestrutura produtiva; verde prioriza oficina, heliporto e infraestrutura mecanizada. Nenhum prédio é desbloqueado pela personalidade.
- Ciclos de recrutamento ponderados diferenciam a composição: vermelho favorece arqueiros/linha terrestre/capitão; amarelo favorece Killer/Stomper com cobertura; verde favorece aeronaves e veículos com suporte terrestre. Há alternativas legais quando o ramo, produtor, recurso ou tecnologia não permite a preferência. A identificação de uma preferência não garante que será treinada imediatamente.
- Permanecem os mesmos recursos iniciais, custos, colonos, produtores, reservas de sobrevivência, guarnições e dificuldade por missão. Preserva a regra especial de pressão da missão 4. Nenhum novo comportamento de voo, muralha, magia, formação ou flanqueamento foi criado nesta configuração; esses sistemas existentes são usados quando disponíveis.

A associação por dono não exige mudar o formato dos saves. Perfil é derivado da cor; não aparece um novo menu de personalidades. Os problemas econômicos e de final da missão 5 identificados no relatório de simulações continuam pendentes: a configuração não representa correção desses problemas nem aprovação de balanceamento.

## Arquivos

Engine/EnemyPersonalities.cs; EnemyCampaignResearch.cs; OpponentBasicPolicy.cs; OpponentSimulation.cs; Tests/EnemyPersonalityTests.cs; Program.cs.

## Verificação

- Build e publish sem erros.
- 40 verificações de perfis, cores, disponibilidade, prioridades, missões 4/5/7, roundtrip e continuação determinística; repetidas no executável publicado.
- Snapshot v103 restaurado exatamente, preservando ramo, recursos e progresso.
- 28 verificações da missão 4 v102 passaram, incluindo fronteira da missão 5.
- Comparações nativas RULE preservadas: 935 comparações e 500 retomadas; algoritmo original de pesquisa mantido sem alteração.
- Simulação natural de dez minutos na missão 5 com quatro checkpoints restaurados exatamente. Ramos novos iniciais: vermelho Armamento, amarelo Agricultura, verde Armamento, conforme disponibilidade/proporção inicial. A prioridade dupla não significa pesquisar dois ramos simultaneamente.

Evidências: experiments/personalidades-v104 (tests.log, published-tests.log, native-rules.log, mission-four.log, mission-five.log, mission-five.json).

## Pacote

releases/interactions-v104. Atalhos de missão copiados da v103, usando o executável desta nova pasta. Pacotes antigos e UserSaves não foram substituídos. Não foi aberta outra partida nem fechado o jogo do usuário.
