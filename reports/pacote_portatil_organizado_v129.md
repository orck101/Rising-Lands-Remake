# Pacote portatil organizado v129 — 10/10/2026

Entrega: D:\RISING\COMP\Rising.exe. Espelho: releases/portable-v129-final.
Backup da pasta COMP anterior: backups/COMP-antes-organizacao-v129.

Raiz: Rising.exe, LEIA-ME.txt, Dependencias, Recursos, UserSaves e UserSettings.
Recursos: Mapas (34 mapas ETA e cenas), Graficos (bancos CMP usados pelo catalogo, interface e paleta), Animacoes, Sons (WAV extraidos) e Dados. runtime e criado para diagnosticos e monitor. Nao ha working/RISING, executaveis originais, instaladores, desinstaladores, arquivos do CD, bancos sonoros legados ou unicorn.dll externos. Os mapas, imagens e sons reutilizados ainda sao recursos derivados do original; nao foram redesenhados.

3571 arquivos, aproximadamente409MB, contra aproximadamente1029MB do primeiro pacote. Dependencias .NET incorporadas em Rising.exe; SDL2 e OpenAL separados em Dependencias por carregamento dinamico do MonoGame. Nao exige .NET nem jogo original instalados. O runtime usa cache local de extracao; nao exige instalacao administrativa.

Codigo: novo Engine/PackageLayout.cs resolve pastas organizadas com fallback para arvores historicas. Chamadas de caminhos de recursos no programa/engine/testes passaram pelo resolvedor; nao altera regras de gameplay. UserRoot conserva saves, configuracoes e telemetria no pacote, sem apontar para D:\CHATGPT. csproj: PortableDistribution omite emulador de pesquisa e mantem recursos externos ao arquivo unico. Codigo anterior preservado em backups/antes-pacote-v129. Atalhos do projeto continuam na v128; esta entrega atualiza apenas COMP.

Verificacao: 54 checks da fila/consumo/reembolso/save, 21 da recuperacao de ordens no pacote e21 no layout antigo da arvore fonte. Capturas graficas da missao1, segunda descoberta da missao3 e menu de audio da missao5 passaram.551 sons decodificados e reproduzidos silenciosamente no dispositivo, concorrencia/vozes verificadas. Abertura do executavel entregue de outra pasta passou. Manifesto completo3571 arquivos conferido por SHA256 entre espelho eCOMP; saves/configuracoes preexistentes deCOMP comparados ao backup. UserSaves do projeto nao utilizados/escritos nos testes.

Falhas de desenvolvimento preservadas: primeiro single-file incorporou JSON exigido em disco; metadados ExcludeFromSingleFile corrigidos. MonoGame nao encontrava SDL2 embutido: Dependencias e diretorio de busca nativa explicito corrigiram. Relatorios dessas tentativas em experiments/portable-v129/runtime-ensaios; nao foram entregues junto ao jogo.

Limites: sem campanha manual completa, teste em outro PC ou pendrive fisico; qualidade auditiva nao foi avaliada por escuta. Nao adiciona faixas de musica que nao existiam na entrega anterior. Ensaios x86 do jogo original nao disponiveis no pacote jogavel.

Evidencias: experiments/portable-v129, delivery.json, final-manifest.json, training-queue.json, worker-order-recovery.json, legacy-layout.json, audio.json, capturas e logs. Tentativas e builds intermediarios preservados como evidencias; nao sao a entrega final.
