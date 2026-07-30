# Relatório de recuperação do catálogo A-Z

## Resumo

O catálogo físico foi reconstruído a partir das 26 texturas JPEG preservadas no projeto e do banco Vuforia `Alfabeto.xml/.dat`. Foram gerados dois catálogos imprimíveis e uma versão de contato, além de uma folha PNG para QA. Nenhum original, banco, cena, GUID ou configuração Vuforia foi modificado.

## Origem e relação com o banco

- Banco: `Assets/StreamingAssets/Vuforia/Alfabeto.xml` e `Alfabeto.dat`.
- Texturas: `Assets/Editor/Vuforia/ImageTargetTextures/Alfabeto/[A-Z]_scaled.jpg`.
- XML: 26 targets únicos; ordem serializada Z-A; conjunto normalizado A-Z completo.
- Imagens: 26/26 JPEGs válidos, 2.286.348 bytes, sem hashes duplicados.
- Relação: cada target XML `A` a `Z` corresponde a `<letra>_scaled.jpg`.

Os hashes completos estão em `checksums.sha256`; dimensões, bytes e proporções estão em `target-inventory.json`.

## Preservação

O total de 2.286.348 bytes foi calculado antes da decisão de cópia. Como os 26 JPEGs já são versionados e cópias em `SourceAssets` seriam byte-idênticas, a duplicação foi evitada. O gerador usa diretamente as fontes canônicas e interrompe na primeira divergência de hash, conjunto, decodificação ou proporção.

Os PDFs incorporam cada fluxo JPEG original byte a byte, codificado como stream PDF sem recompressão do JPEG. A escala de apresentação conserva a razão de pixels. As pequenas diferenças históricas entre a razão de pixels e a razão XML ficam entre 0,006463% e 0,121934%; nenhuma imagem foi esticada para mascará-las.

## Artefatos gerados

| Artefato | Páginas | Bytes | SHA-256 | Status |
|---|---:|---:|---|---|
| `Builds/TargetCatalog/Catalogo_Alfabeto_RA_A4_2_por_pagina.pdf` | 13 | 2.920.431 | `1891178BA4B585322C81DCDE27638737208E13497A85C648C17009978F49BFA0` | PASS |
| `Builds/TargetCatalog/Catalogo_Alfabeto_RA_A4_1_por_pagina.pdf` | 26 | 2.930.244 | `49DE03B262FDD0FAA3A86B63DAC7B941438123CB495AB278B457B4ACD8A1242D` | PASS |
| `Builds/TargetCatalog/Catalogo_Alfabeto_RA_Contato.pdf` | 1 | 2.908.435 | `76352BA404D5FC5ECF43F8EE2197E3B63FC7423D22E12100B8BFEC151AE182CB` | PASS |
| `Builds/TargetCatalog/target-contact-sheet.png` | - | 932.111 | `5D046D3F9C7DA354B3DA456D1D87A197B114C2FEDE9C1D1A747291BA9F47CB32` | PASS |

Os três PDFs são A4 retrato. A versão 2 por página usa largura de imagem de 165 mm, intervalo de 18 mm e margens superiores às mínimas de 12 mm. Identificação, dimensões, marcas de corte, aviso e página ficam fora da imagem.

## Verificação

- Testes Python: 5/5 PASS.
- Manifesto: XML, DAT e 26 JPEGs, 28/28 PASS.
- Regeneração determinística: hashes idênticos em duas execuções consecutivas.
- Renderização Poppler: 40/40 páginas inspecionadas.
- Folha de contato: 26/26 targets conferidos.
- Originais após geração: 26/26 hashes inalterados.

## Limitações

- As texturas preservadas podem ter resolução diferente do material gráfico original perdido.
- Métricas de luminância/entropia são auxiliares e não substituem a classificação do Vuforia.
- A versão contato não é apropriada para tracking.
- Reconhecimento, distância e estabilidade físicos continuam pendentes de teste humano.

## Impressão

Usar escala 100%, papel fosco, boa qualidade, target plano e iluminação difusa. Não usar “Ajustar à página”, não recortar dentro da imagem e não plastificar com material reflexivo. O procedimento completo está em `PRINTING_GUIDE.md`.

## Rollback

Os artefatos em `Builds/TargetCatalog` são ignorados e podem ser regenerados pelo script. Para desfazer alterações versionadas sem reescrever histórico, reverter os quatro commits desta branch em ordem inversa. As fontes canônicas não precisam de restauração porque não foram modificadas.

