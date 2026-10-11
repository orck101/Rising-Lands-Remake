# Variantes da campanha e textos — v16, 28/09/2026

Release: `releases/campaign-text-v16/RisingLandsRemake.exe`.

## Correção no jogo

Todos os **34 mapas** agora encontram o texto de sua missão em Esc → Objetivo da missão. Os mapas MONDE09A/B/C/D e MONDE10/12/17A/B/C usam os textos MONDE09/10/12/17.TXT. São 25 textos únicos, 39 páginas únicas e 53 páginas contando a reutilização pelas variantes.

A conclusão anterior de que faltavam os mapas de quatro missões estava incorreta: faltam os nomes sem sufixo, mas os mapas existem como variantes com letras. Os arquivos estavam preservados; o erro estava na correspondência por nome. ObjectiveReader agora consulta uma tabela de aliases produzida a partir da rotina nativa de nomes, com comparação sem distinção de maiúsculas/minúsculas. Treinos continuam sem objetivo de campanha. Navegação, saves e demais regras não mudaram.

## Evidência nativa de nomes e avanço

Ferramenta: `tools/trace_campaign_transition.py`; relatório: `reports/campaign_transition_validation.json`; dados integrados: `source_remake/Data/objective_aliases.json`.

**CONFIRMADO em emulação:** MissionNext (`0x46bf20`) incrementa o número antes de consultar FileExists e não desfaz o incremento quando o arquivo não existe. 50 casos cobrem números1–25 e existência falsa/verdadeira. MakeMissionName (`0x46c04c`) executa nativamente; a consulta de existência é interceptada, sem abrir arquivos pelo processo legado.

**CONFIRMADO em 34 configurações:** a missão9 usa um seletor de quatro valores em `0x4e1d85` para sufixos a/b/c/d. As missões10,12,17 consultam PlayerGetSympathie para escolher a/b/c; os testes fornecem três combinações distintas de pontuações, cada uma selecionando uma variante. Não foram reconstruídos a origem do seletor nem os mecanismos que alteram simpatia. Cada nome gerado foi comparado com um arquivo real da cópia preservada.

**CONFIRMADO no bloco de continuação `0x42cb06`:** quando existe próxima missão, chama IaRechercheAjuste, Engin_StoreExperimente, RechercheStore e PlayerStore nessa ordem e solicita menu17. Sem arquivo, solicita menu24. As quatro funções de transferência foram interceptadas; seus efeitos internos não estão comprovados por este teste. Duas ramificações verificadas. Total desta ferramenta: **86 casos nativos**.

**CONFIRMADO por leitura estática:** menu19 carrega `\ecranfin.lzs` pelo endereço de string `0x494aa3` e solicita texto116, “Mission Completed”, em `0x4689ca–0x4689d4`. O caminho de derrota referencia `\events\Over_fr.avi` em `0x494472`. O caminho de vitória aguarda Enter ou clique antes de chamar MissionNext (`0x42cad0–0x42cb06`). A renderização e reprodução de mídia não foram executadas nesta etapa.

A primeira tentativa de emulação de nomes usou um seletor de variante não inicializado e falhou em leitura de memória. A fixture foi corrigida com valores explicitamente controlados; nenhum endereço inválido foi mascarado.

## Validação e limites

**251 verificações aprovadas** em `logs/selftest_campaign_text_v16.log`. Cobertura de correspondência dos34mapas e testes explícitos de variantes das quatro missões. Captura `reports/prototype_v16_mission09d.png` inspecionada em1280×800: MONDE09D mostra o texto de Mission9. Dados empacotados comparados aos exports; integridade das1.340 cópias preservadas verificada.

Esta versão corrige acesso aos textos. Não ativa vitória automática, transferência de experiência/pesquisas/jogador, escolha de variante por diplomacia nem campanha completa. Essas dependências impedem tratar a transição como simples troca de mapa.

Alterações: ObjectiveReader, ObjectiveTests, projeto de compilação, novo catálogo de aliases e ferramenta de transição. Evidências anteriores de condições e contagem permanecem em mission_one_conditions.md e mission_lifecycle.md. Próximo passo: mapear os dados transferidos e a transição dos estados de morte antes da integração do encerramento.
