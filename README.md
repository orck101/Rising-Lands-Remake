# Rising Lands Remake

Snapshot atual do remake em C#/.NET8 e MonoGame: **v130**, com monitor dos inimigos.

## Executar a partir do codigo

Requisitos: Windows64bits e SDK .NET8. Na raiz deste repositorio:

```powershell
dotnet restore source_remake/RisingLandsRemake/RisingLandsRemake.csproj
dotnet run --project source_remake/RisingLandsRemake/RisingLandsRemake.csproj -- --assets ./Recursos --map MONDE01.ETA
```

Troque MONDE01.ETA pelo mapa desejado. Os arquivos de recursos sao incluidos no repositorio. Os dados/regras e recursos extraidos da interface, animacoes e sons sao copiados automaticamente no build.

## Monitor

```powershell
dotnet run --project source_remake/EnemyMonitor/EnemyMonitor.csproj
```

O monitor permite observar os dados publicados pelo jogo; os caminhos locais de uma instalacao anterior podem exigir configuracao. Consulte source_remake/EnemyMonitor/Program.cs.

## Estrutura

- source_remake/RisingLandsRemake: engine, interface, testes e recursos extraidos.
- source_remake/Data: regras, catalogos, sons e cenas.
- source_remake/EnemyMonitor: codigo do monitor.
- Recursos: mapas, bancos graficos e paleta usados pelo remake.
- tools: empacotamento portatil e bibliotecas nativas necessarias.
- documentation: documentacao das missoes e reconstrucoes.
- reports: relatorios recentes de validacao.

## Estado atual

Menu de testes removido da partida. Velocidade ajustavel entre1x e3x. Fila de treinamento, economia e perfis dos inimigos, diplomacia, combate, pesquisas e save/load conforme implementacao atual. Missao1 e segunda descoberta da missao3 verificadas no pacote portatil; publicacao do projeto verificada por build. Nem todas as campanhas foram aprovadas por partida humana completa.

A versao jogavel compilada deve ser distribuida pelas Releases, separada deste codigo. Saves pessoais, logs, builds e instalacao original completa nao sao versionados.

Mapas, imagens e sons reutilizados continuam derivados de Rising Lands original. Este repositorio nao atribui licenca propria a recursos de terceiros nem inclui executaveis ou instaladores do jogo original. Ensaios de emulacao x86/reverse engineering dependem de arquivos originais locais, que nao estao aqui.

Documentacao historica pode citar caminhos da maquina de desenvolvimento; para esta copia, use os caminhos relativos acima.
