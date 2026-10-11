# v95 — correção de fechamento durante a partida

## Causa comprovada e limite da conclusão

O jogador informou fechamento na v94, missão 4. Os eventos do Windows registraram dois fechamentos anteriores com `Invalid warehouse overflow order` (v57 e v71, em 1/2 de outubro), durante `ResourceGesture.Commit/Redirect → OrderPreview → Restore → ValidateOverflow`. Evidência preservada em `reports/v95-windows-crashes.json`. Esses eventos anteriores não identificam por si só a causa do fechamento recente.

Na implementação atual foi reproduzida a mesma exceção com um cenário da missão 4: fazendeiro inimigo carregando a última unidade de alimento, entregando no chão ao redor do armazém, fica faminto sem comida em estoque; ele consome sua carga e reinicia a coleta. O marcador `OverflowCell` continuava apontando para a entrega anterior. Uma nova ordem do jogador valida o estado de TODOS os personagens numa cópia (`OrderPreview`), encontra a ordem inválida do inimigo e lança a exceção. O teste falhava antes da correção (`reports/v95-before-fix.txt`). Esta causa é CONFIRMADA; não há garantia de que explique todo possível fechamento.

## Correção

- Limpar o destino de excedente quando a última comida carregada é consumida pelo inimigo.
- Limpar o marcador de entrega anterior sempre que uma coleta começa, incluindo a busca automática de uma fonte próxima.
- Manter as validações de saves e conservação de recursos, sem ignorar erros nem conceder comida.

## Registro de futuras falhas

Exceções tratadas no encerramento principal e exceções não tratadas passam a gerar relatório único em `runtime/crashes`, contendo exceção completa, caminho da versão, missão, tick, velocidade, fase (carregamento/atualização/desenho), seleção e mensagem. Quando possível, uma cópia separada do estado é gravada como `.snapshot.json` para reprodução. Não substitui os cinco slots, não altera saves originais e não promete recuperação automática. Falhas nativas abruptas ou falta de memória podem impedir a gravação.

## Validação

- Publicação autocontida Windows concluída sem erros.
- 59 verificações de regressão/coleta, incluindo o cenário que falhava, nova ordem do jogador e 33 minutos simulados da missão 4 com 40 validações de estado.
- Quatro verificações do relatório, contexto, cópia recuperável e ausência de sobrescrita em notificações duplicadas; resultados artificiais isolados em `reports/v95-diagnostic-test`.
- 36 verificações de reparos, defesa, visuais e desenvolvimento dos três inimigos aprovadas.
- 15 arquivos de partidas existentes carregados sem modificação na inspeção de compatibilidade.
- Inicialização/desenho real da missão 4: captura `reports/v95-mission4.png`, processo encerrou normalmente.

## Abrir

[Jogar missão 4 com monitor](../releases/interactions-v95/Jogar-Quarta-Missao-Com-Monitor.cmd). Usar a v95 para receber a correção. Carregar uma partida compatível preserva seu progresso; não é necessário reiniciar a campanha. Se ainda fechar, consultar os relatórios novos em `runtime/crashes`.
