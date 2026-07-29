# Pipeline de arte do alfabeto RA

Este pipeline substitui a dependência operacional de .blend dentro do Unity por FBX estáveis, mantendo os 26 arquivos legados intactos até a validação final em dispositivo.

~~~mermaid
flowchart LR
    A["generate_alphabet.py<br/>Blender 2.93.18"] --> B["26 fontes .blend<br/>SourceAssets"]
    B --> C["validate_alphabet.py<br/>geometria e nomes"]
    C --> D["export_alphabet_fbx.py<br/>-Z / Y"]
    D --> E["26 FBX<br/>Assets/Models"]
    E --> F["AlphabetModelImporter.cs<br/>Unity 2019.4"]
    F --> G["26 materiais + 26 prefabs"]
    G --> H["JSON de conteúdo"]
    G --> I["Substituição manual<br/>Image Targets Vuforia"]
    J["26 .blend legados<br/>checksums SHA-256"] -. preservados .-> I
~~~

## Camadas

1. **Fonte** — o gerador usa texto, extrusão, bevel, conversão para mesh, limpeza topológica e material simples. Um arquivo independente é salvo por letra.
2. **Validação Blender** — abre fontes e reimporta FBX. Mede topologia, volume, transformações, origem, dimensões, nomes e materiais.
3. **Intercâmbio** — o exportador gera FBX binário, somente objeto selecionado, sem animação, câmera, luz ou armature.
4. **Unity** — o importador configura escala, compressão, normais, tangentes e leitura; cria material e prefab normalizado.
5. **Conteúdo** — o JSON conserva IDs e targets A–Z e aponta para Models/Alphabet/Prefabs/RA_Letter_X.
6. **Vuforia** — a cena não é alterada automaticamente. A troca ocorre target por target conforme o plano de migração.

## Reprodutibilidade e fail-closed

Tools/Blender/build-alphabet-assets.ps1 exige a branch autorizada, Git limpo e Blender 2.93.18. Cada subprocesso precisa retornar zero. Relatórios intermediários ficam em Builds/Art/Alphabet, pasta ignorada. Arquivos existentes só são substituídos com -Overwrite.

O projeto legado contém scripts Vuforia 8.3.8 vendorizados, mas não declara a DLL/pacote correspondente no manifest. Para não misturar versões nem modificar o legado, a geração e os testes dos prefabs foram executados em sandbox Unity 2019.4 dentro de Builds/. Antes de um APK, a dependência Vuforia do projeto principal deve ser restaurada na mesma versão ou migrada de forma dedicada.

## Preservação

Os checksums originais estão em legacy-model-checksums.sha256. A remoção de Assets/Objetos/RaA.blend–RaZ.blend só pode ocorrer após: 26 FBX aprovados, prefabs aprovados, cena testada, APK gerado, A–Z testado em aparelho e baseline preservada.
