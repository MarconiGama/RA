# Catálogo físico recuperado A-Z

Este diretório documenta a recuperação do catálogo imprimível do banco Vuforia `Alfabeto`, sem alterar o banco ou as 26 texturas preservadas.

## Estado

- Inventário A-Z: **PASS (26/26)**
- Integridade SHA-256: **PASS (28/28: XML, DAT e 26 JPEGs)**
- Validação automatizada e visual: **PASS**
- PDFs: **PASS (13, 26 e 1 página)**
- Teste físico de reconhecimento: **PENDENTE DE EXECUÇÃO HUMANA**

## Documentos

- `PREFLIGHT_REPORT.md`: gate inicial e decisão de não duplicar imagens.
- `target-inventory.json`: dimensões, proporções, bytes e hashes.
- `checksums.sha256`: manifesto das fontes.
- `TARGET_CATALOG_RECOVERY_REPORT.md`: relatório consolidado.
- `PRINTING_GUIDE.md`: instruções de produção física.
- `VISUAL_VALIDATION_REPORT.md`: métricas A-Z e inspeção dos layouts.
- `PHYSICAL_TARGET_TEST_PLAN.md`: roteiro humano inicial A, B, M, O, U e Z.

O gerador e os testes ficam em `Tools/TargetCatalog`. Os artefatos gerados ficam em `Builds/TargetCatalog` e não são versionados.

