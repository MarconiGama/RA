# Preflight - recuperação do catálogo de targets

## Resultado

**PASS.** O preflight fail-closed encontrou o banco canônico e exatamente 26 JPEGs válidos, correspondentes ao conjunto completo A-Z. Nenhum arquivo-fonte foi alterado.

## Estado do repositório

- Branch obrigatória: `agent/target-catalog-recovery`
- Commit inicial: `98702eefd68972dfc8fc9e6ecca24f6db4738429`
- Upstream inicial: `origin/agent/target-catalog-recovery`
- Worktree no início: limpo
- `Builds/`: ignorado por `.gitignore`

## Banco canônico

- XML: `Assets/StreamingAssets/Vuforia/Alfabeto.xml`
- DAT: `Assets/StreamingAssets/Vuforia/Alfabeto.dat`
- XML SHA-256: `114CD504997C75088203970B2F09DA3575A75EB0F24C87E66281F3CD32881255`
- DAT SHA-256: `9AEE11000CC9D5DE5E4624E0AC201C0F50F04A7938E249D25966550DDE642F73`
- Targets no XML: 26
- Ordem física no XML: Z-A
- Conjunto normalizado: A-Z completo, sem ausência ou repetição

A ordem serializada no XML é descendente, mas os nomes formam exatamente o conjunto A-Z. O catálogo é emitido em ordem alfabética ascendente.

## Imagens preservadas

- Diretório: `Assets/Editor/Vuforia/ImageTargetTextures/Alfabeto`
- Padrão aceito: `[A-Z]_scaled.jpg`
- JPEGs esperados/encontrados/válidos: 26 / 26 / 26
- Ausentes: nenhum
- Duplicatas por SHA-256: nenhuma
- Dimensões: 23 imagens com 1024 x 721 px; E com 1024 x 719 px; I com 1024 x 723 px; O e P com 1024 x 722 px
- Total: 2.286.348 bytes
- Decodificação Pillow e `verify()`: PASS em todos os arquivos

As razões de pixel diferem das razões declaradas no XML entre 0,006463% e 0,121934%. Esta pequena diferença histórica é aceita pelo limite conservador de 0,15%. Para não deformar nem reamostrar os JPEGs, o gerador usa a razão de pixels ao desenhar e registra as dimensões XML como referência nominal.

## Preservação e decisão de duplicação

Os 26 arquivos já são versionados no Git e somam 2.286.348 bytes. Cópias em `SourceAssets/Targets/Alfabeto/Originals` seriam duplicações byte-idênticas sem ganho de preservação; por isso não foram criadas, conforme a regra de preferência do gate. O gerador lê diretamente as origens canônicas e valida seus hashes antes de gerar.

## Evidências

- Inventário estruturado: `docs/target-catalog/target-inventory.json`
- Manifesto de integridade: `docs/target-catalog/checksums.sha256`

