# v71 — comércio, minimapa e muralhas

Versão jogável: `../releases/interactions-v71/Jogar-Terceira-Missao.cmd`. Primeira e segunda missões têm atalhos próprios na mesma pasta. Os arquivos da instalação original e os saves existentes foram preservados. Novos saves usam v30; a leitura de versões anteriores continua disponível.

## O que mudou e como testar

1. **Muralhas conectadas:** selecione o construtor, escolha a muralha na barra de construções e segure o botão esquerdo, arrastando o trecho. Ao soltar, os segmentos válidos entram na fila e o construtor trabalha em sequência. Um trecho diagonal é convertido em degraus conectados, evitando lacunas. Recursos são cobrados pelos segmentos efetivamente criados; um terreno sem acesso pode interromper a criação da fila. Ordens de movimento e Stop interrompem a fila do construtor.
2. **Bloqueio:** muralhas prontas e obras de muralha bloqueiam a navegação antes das exceções usadas para alcançar portas. Vale para trabalhadores, combatentes e, conforme solicitado, aeronaves. A rota procura contornar o muro; um recinto fechado impede a travessia. Portões continuam indisponíveis: a regra de terreno já existe, mas o modelo gráfico original do portão ainda precisa ser recuperado.
3. **Minimapa:** prédios e obras aparecem por proprietário; obras em andamento recebem cor própria. Apenas as partes descobertas aparecem normalmente. A revelação do menu de testes permite inspecionar o mapa todo sem marcar exploração.
4. **Comércio:** tenha um mercado atendido e uma Bolsa de Trocas em cada tribo. Selecione o mensageiro e clique no mercado adversário. Ele caminha até a entrada e fica dentro; a **cartola animada** abre a negociação. Use os ícones originais de materiais/personagens para alternar a oferta. Clique no recurso, indique a quantidade, confirme a quantidade e depois confirme a oferta. O estoque não é descontado durante a avaliação. Aceitação transfere os materiais diretamente; recusa não cria transação. X recusa/cancela, enquanto Esc fecha apenas o painel.
5. **Troca de personagens:** escolha unidades disponíveis nos retratos. Depois de aceita a oferta, selecione os personagens prometidos e clique na sua Bolsa. Os retratos pendentes aparecem ao selecionar a Bolsa. Unidades adversárias prometidas recebem a ordem de entrar na Bolsa da outra tribo. Quando todas chegam, a transferência acontece e as unidades recebidas saem pela frente da Bolsa. Expiração devolve as unidades guardadas e preserva a propriedade; não duplica recursos ou personagens.
6. **Mensageiros recebidos e alianças:** uma negociação recebida permite confirmar ou recusar; seu pedido fica fixo e o painel tem as posições invertidas conforme o manual. Mensageiro no santuário adversário abre o caminho de aliança. A aliança melhora a avaliação de ofertas, persiste entre missões e pode ser rompida por uma ordem explícita de ataque. Mensageiros não provocam retaliação automática; continuam sujeitos a ataque explícito.

O menu Controles foi atualizado. A fila de muralhas, a negociação em andamento, os personagens guardados na Bolsa, as transferências de propriedade e a diplomacia são preservados no save.

## Evidências originais

**CONFIRMADO por execução isolada da cópia do PE:** `ChercheMurCombinaisons` em `0x41c22a` usa N=1, E=2, S=4, O=8 e a variante `max(0, máscara-1)`. `0x407dd3` desenha os sprites 7360–7374. As 16 combinações foram sondadas, incluindo extremidades, cantos, encontros em T e cruzamento. Arquivo: `../source_remake/Data/wall_connections_v71.json`; ferramenta: `../tools/trace_walls_v71.py`. O remake anterior usava sempre o primeiro segmento.

**CONFIRMADO no código e nos gráficos:** `AffMarche @ 0x466611` usa a cartola `0x2efd + (contador & 31)` e sua sombra `0x2f1d + (contador & 31)`. A cartola reaproveita os frames extraídos de `OTHERS.CMP`. A escala mantém as proporções; a posição foi adaptada à interface do remake. `AffJaugeMarche @ 0x465989` usa altura máxima 84 e cores de paleta 145, 180 e 48 conforme a satisfação. Essa régua substitui a ausência anterior de indicação gráfica de aceitação.

**CONFIRMADO no manual e nos ensaios anteriores:** mercado/Bolsa dos dois lados, mensageiro chegando antes de abrir a negociação, transferência direta de materiais, personagens entregues na Bolsa e negociação recebida com pedido fixo. Pesos dos materiais: peças=2, alimento=1, pedra=1. Classes negociáveis: 0–15 e 25. Avaliação considera simpatia e aliança. Ver `commerce_original_audit.md` e `../reports/commerce_original_audit.json`, preservando a distinção entre manual, relato do jogador e ensaio nativo.

**CONFIRMADO na tabela do PE:** evento 11 em `0x4972e8` reduz simpatia em 15; evento 16 em `0x497338` aumenta em 5. `MAJSympathie @ 0x475044` limita essas atualizações a -50..50. O código de expiração da entrega chama evento 11; confirmação de aliança chama evento 16. O remake aplica esses eventos ao contrato não entregue e à aliança aceita. Evidências: `../reports/v71-trade-ui-evidence.json`, `../reverse_engineering/v71-trade-ui-evidence.txt` e `../tools/trace_trade_ui_v71.py`.

## Limites de fidelidade registrados

- A **IA que compõe ofertas recebidas e decide alianças ainda é simplificada**. Usa os estoques disponíveis e uma decisão provisória de aliança pela simpatia; não é apresentada como reconstrução integral da IA original. A campanha não cria automaticamente novas populações ou mercados adversários: precisa haver mensageiro e construções no cenário.
- A duração foi adaptada à simulação atual: avaliação 850 atualizações, entrega 1.650. Os contadores originais 17/32 e períodos 50 foram identificados, mas a correspondência exata com segundos do relógio original continua desconhecida. A cartola usa 12 frames por segundo no remake; os 32 desenhos são originais, a cadência real ainda precisa de comparação.
- A contabilidade original RMI/total/estoque permanece em investigação. O remake usa sua contabilidade conservativa atual; não inventa transporte de materiais nem concede estoque antes da aceitação.
- A construção por arrasto e a fila são uma melhoria de controle. As conexões visuais vêm do original, mas o comportamento exato dos marcadores de obra em forma de cruz ainda não foi reconstruído.
- O minimapa mostra prédios existentes conhecidos; não reconstrói ainda a memória de prédios inimigos destruídos fora da visão.

## Verificação

Testes específicos cobrem as 16 conexões, muralha fechada para unidades terrestres e aéreas, fila de construção, negociações e retomada determinística após carregar, recusa, pré-requisito da Bolsa, prazo, recursos, trocas bilaterais de personagens, rejeição de histórico de propriedade adulterado e pedido recebido fixo. As suítes anteriores verificam coleta, pesquisa, descoberta, áudio, construções, combate, produção e leitura dos saves reais sem reescrita.

Capturas conferidas: `../reports/v71-walls-final.png`, `../reports/v71-market-confirmed.png`, `../reports/v71-hat.png`. A captura do mercado verifica também o clique no botão original de confirmação. Contagens e resultados finais ficam em `../reports/v71-validation.json`; relação de arquivos e hashes em `../logs/v71-change-manifest.json`.

Resultado final: **732 verificações aprovadas**, incluindo 96 específicas da v71. Compilação e publicação concluídas, sem erros ou avisos.
