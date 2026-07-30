# Gerador do catálogo de targets

Este utilitário valida as fontes canônicas do banco `Alfabeto` e gera os três PDFs e a folha de contato solicitados. Ele falha antes da geração se a estrutura A-Z, os hashes, a decodificação JPEG ou as proporções divergirem.

## Fontes

- `Assets/StreamingAssets/Vuforia/Alfabeto.xml`
- `Assets/StreamingAssets/Vuforia/Alfabeto.dat`
- `Assets/Editor/Vuforia/ImageTargetTextures/Alfabeto/[A-Z]_scaled.jpg`
- `docs/target-catalog/target-inventory.json`

O processo é somente leitura sobre essas fontes. As imagens são incorporadas aos PDFs sem recorte, filtro ou recompressão intencional, e sempre com a proporção de pixels preservada. O XML é usado como contrato nominal e para validar a diferença histórica máxima de 0,15%.

## Dependências

Python 3 com as versões conhecidas registradas em `requirements-target-catalog.txt`. Para preparar um ambiente isolado:

```powershell
python -m pip install -r Tools/TargetCatalog/requirements-target-catalog.txt
```

Não é necessário instalar nada quando Pillow, ReportLab e pypdf compatíveis já estiverem disponíveis.

## Executar

```powershell
Tools/TargetCatalog/build-target-catalog.ps1
```

Ou diretamente:

```powershell
python Tools/TargetCatalog/build-target-catalog.py
python Tools/TargetCatalog/build-target-catalog.py --validate-only
```

Saída padrão: `Builds/TargetCatalog`. O diretório é ignorado pelo Git. Para uma saída alternativa, use `--output-dir CAMINHO` no Python ou `-OutputDirectory CAMINHO` no PowerShell.

## Artefatos

- `Catalogo_Alfabeto_RA_A4_2_por_pagina.pdf`: A4 retrato, 2 targets por página, 13 páginas.
- `Catalogo_Alfabeto_RA_A4_1_por_pagina.pdf`: A4, 1 target por página, 26 páginas.
- `Catalogo_Alfabeto_RA_Contato.pdf`: miniaturas para conferência, 1 página.
- `target-contact-sheet.png`: folha de contato para QA visual.
- `validation-results.json`: métricas reproduzíveis de validação.

Os PDFs usam geração determinística do ReportLab (`invariant=1`). A versão contato e a folha PNG não são adequadas para tracking.

