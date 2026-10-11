# Visão geral
O PDF de 33 páginas e 70 seções passou a orientar o projeto. Seu texto está preservado em PROMPT_PDF.txt e o controle de atendimento em prompt_status.md. Esta é uma reconstrução incremental, não uma remasterização concluída.

## Entregas disponíveis
- [Inventário](file_inventory.md), [arquitetura](architecture_overview.md), [formatos](FORMATOS.md), [executáveis](executable_map.md).
- [Mapa de dados](data_relationships.md), [especificação em evolução](game_specification.md), [roadmap](remake_plan.md).
- Protótipo Windows x64 em `../releases/prototype/RisingLandsRemake.exe`: carrega ETA/CMP/PIE das cópias, exibe terreno, seleção, câmera e movimento experimental; save moderno de navegação.

## Riscos e limites
O movimento não reproduz CheminCalcul. Todas as unidades usam um sprite representativo nesta versão; animações por tipo e direção ainda não foram vinculadas. Construções aparecem como marcadores de âncora. Estado interno de unidade é importado, mas a máquina de estados ainda não governa o protótipo. Não há economia, combate ou campanha funcional. A compatibilidade foi testada no Windows deste computador, não em uma matriz Windows 10/11.

As cópias do corpus permanecem o baseline da primeira coleta. save1.sav na origem mudou posteriormente; não foi incorporado silenciosamente. Ver source_save_change.json.
