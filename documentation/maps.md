# Mapas

[Layout completo conhecido](FORMATOS.md#missões-eta--confirmado-na-sequência-de-leitura). O parser sequencial cobre34 mapas e delimita cauda fixa desconhecida. `../source_remake/Data/Maps` contém largura/altura, recursos por ID, unidades, construções, coordenadas, registros brutos e hashes. IDs de tiles e demais planos são preservados em `../extracted/maps`. O protótipo carrega diretamente ETA; PgUp/PgDn muda missão.

Estado inicial é extraído, mas campanhas não são reproduzidas por apenas abrir mapas. Veja os documentos individuais em missions/.
