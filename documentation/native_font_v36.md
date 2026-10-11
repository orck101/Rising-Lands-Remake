# Fonte original — v36

Executar releases/native-font-v36/Jogar-Primeira-Missao.cmd.

CONFIRMADO: fonte raster da rotina AffString 0x459094 usa sprites OTHERS.CMP a partir de0x23d5 para caracteres ASCII. FonteGetIndice0x458c44 identifica seis bases:23d5,24b5,2595,2675,2755,2835 (branco/vermelho, três tamanhos, inspeção dos sprites). A fonte aplicada é a branca pequena; tintas atuais e escala responsiva são adaptações.

95 caracteres32..126 extraídos dos PNGs convertidos; ampliação2x por vizinho mais próximo gera atlas native-font.png com células32x32. Espaço avança a largura do sprite base+76 (letra l), conforme código original. TextGetLargeur0x458f28 executado em Unicorn com dimensões dos sprites interceptadas em BobGetTaille0x45ad78: cinco frases apresentam exatamente a soma das larguras reconstruídas. Evidência reports/native_font_v36.json; ferramenta tools/extract_native_font_v36.py.

FONTE.FNT possui assinatura FNTO, mas não é consultado pela rotina analisada. Seu formato integral permanece desconhecido; não foi convertido nem modificado. Não se atribui a ele a fonte aplicada.

Fonte moderna anterior preservada como arquivo, porém a interface carrega agora o atlas original. Menus, pesquisas, objetivos, vitória e mensagens usam os mesmos desenhos originais. A composição SpriteFont, escala, cores e texto traduzido permanecem adaptados. Caracteres fora do conjuntoASCII usam ?. Famílias maiores/vermelhas não foram integradas a cada contexto original ainda.

Validação: build/publicação e suites missão1/regressão aprovadas. Cinco telas revisadas visualmente: pausa, última página do objetivo, pesquisa, vitória, santuário. Capturas de pesquisa/objetivo/vitória executam verificações dos handlers introduzidas nas versões anteriores. Nenhum original/save original alterado. Arquivos em cópias alterados: Engine/ResearchGame.cs e csproj; novos atlas/medidas em Content, ferramenta e relatório.

Pendências de gameplay preservadas: pesquisa demais ramos, inventário individual de armazéns, interações ainda ausentes e apresentação exata da vitória original.
