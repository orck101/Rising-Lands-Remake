# v97 — entrada no Mercado e negociação

## Correções

- O mensageiro conserva a rota calculada até a porta. O código anterior guardava uma referência à lista e depois a esvaziava ao interromper as outras ordens. Agora preserva uma cópia independente. O teste falhando antes da correção está em `reports/v97-before.txt`.
- A ordem de entrar no Mercado/Santuário estrangeiro e de entregar tropas à Bolsa usa a mesma identificação pela imagem usada nas outras interações. Antes, apenas o retângulo lógico do prédio era aceito.
- O cursor de entrada original (`0x1e16..18`) indica a interação do mensageiro; o indicador proibido original (`0x1e0e`) aparece quando falta um requisito.
- Mensagens distinguem prédio sem operador, Mercado próprio ausente/inativo, Bolsa própria/estrangeira ausente, negociação em andamento e caminho bloqueado.
- A avaliação avança com o painel de negociação aberto. A edição da oferta continua pausada antes de confirmar; abrir o painel durante a avaliação não congela mais o contador.
- Os personagens pedidos numa oferta recebida não podem ser removidos da proposta. A validação permite os personagens reservados pela própria negociação, sem permitir usar os comprometidos com outra sessão.
- A entrega de tropas à Bolsa interrompe ordens anteriores dos dois lados. A defesa e o planejador econômico/militar não desviam unidades já a caminho para cumprir a troca. Na transferência de dono, destinos antigos de coleta/entrega são limpos, preservando a carga.

## Como negociar

1. Tenha seu Mercado concluído e com colono, sua Bolsa de Trocas e um mensageiro. O outro clã precisa ter Mercado com atendimento e Bolsa.
2. Selecione o mensageiro e clique na imagem do Mercado estrangeiro. Ele caminha até a porta e entra.
3. Clique na cartola animada acima da interface inferior. Escolha o que oferece e o que deseja receber nos ícones originais; confirme as quantidades e envie a oferta.
4. A oferta é avaliada conforme valor dos recursos/personagens, simpatia e aliança. Materiais aceitos são debitados/creditados diretamente, sem carro.
5. Se envolver tropas, selecione as unidades prometidas e clique na sua Bolsa. Os ícones da Bolsa mostram o que ainda falta entregar. Quando os dois lados cumprem o contrato, as unidades mudam de dono e saem pela porta da Bolsa correspondente.
6. X recusa/cancela; Escape fecha a janela. Encerrar o prazo de entrega cancela a troca e aplica a penalidade recuperada do original.

## Evidências e certeza

**CONFIRMADO no manual fornecido, seção Advanced Controls / Trading, página 19 do PDF:** mensageiro enviado ao Mercado estrangeiro; Mercado e Exchange necessários nos clãs envolvidos; cartola abre a negociação; oferta/pedido e avaliação; tropas entregues às Bolsas, ícones do que falta, saída das unidades recebidas; conteúdo de veículos acompanha a unidade sem fazer parte de sua avaliação. O pedido do computador é fixo, mas o jogador pode definir o que quer em contrapartida. A leitura reproduzível e o hash do PDF estão em `reports/v97-trade-manual-evidence.json`.

**FORTE EVIDÊNCIA no binário copiado:** `BatimentLogeEngin` compara a classe 10 em `0x41f842`, aceita os tipos de prédio 16 e 2, valida sessões em `0x41f8af`/`0x41f8cc`, exige Bolsas nos dois participantes (`RechercheHangar` em `0x41f8e5` e `0x41f906`) e inicia a negociação em `0x41f92e`. Arquivo: `reverse_engineering/disassembly/RISING-ORIG.symbols.asm`.

**CONFIRMADO nos ensaios nativos anteriores:** avaliação inclui simpatia/aliança; materiais são conservados pelo débito/crédito; apenas classes 0–15 e 25 são negociáveis. Fontes completas: `documentation/commerce_original_audit.md` e `reports/commerce_original_audit.json`. Cartola e painel: `reports/v71-trade-ui-evidence.json`.

**Limites:** a composição e a frequência exatas das ofertas feitas pela IA, todos os efeitos de simpatia e o significado completo dos contadores RMI/Stock não estão integralmente reconstruídos. O comportamento existente de encerramento do mensageiro também não é apresentado como reconstrução comprovada. Esta versão corrige o acesso e o fluxo de negociação; não declara fidelidade integral desses pontos ainda desconhecidos.

## Validação

- Publicação Windows autocontida, sem erros de compilação.
- 24 verificações específicas de comércio: rota real, entrada, pré-requisito ausente sem efeitos, conservação de recursos, entrega bilateral, ordens antigas, reservas, salvamento e pedido recebido.
- 96 verificações anteriores de comércio/muros mantidas.
- 21 verificações de estratégia da IA e 13 da abertura comercial, com simulações longas.
- Regressão do fechamento durante coleta: 33 minutos simulados, 40 estados validados, além dos testes de coleta/excedente.
- Teste da interface real: clique em imagem fora do contorno lógico, chegada, botão de confirmar e contador avançando com painel aberto. Captura: `reports/v97-market.png`.
- Quinze saves existentes carregados em inspeção somente leitura, sem alterar os arquivos.

Os testes antigos de comércio usavam uma tropa inimiga artificial perto das tropas do jogador. O caminho agora correto demora mais; esse soldado podia morrer antes da negociação. A preparação desses testes foi ajustada para colocá-lo perto da Bolsa estrangeira, sem alterar a configuração das missões reais.

## Abrir

`releases/interactions-v97/Jogar-Quarta-Missao-Com-Monitor.cmd` ou `Jogar-Quarta-Missao.cmd`. Os cinco atalhos de missão foram preservados. Originais e saves do usuário não foram modificados.
