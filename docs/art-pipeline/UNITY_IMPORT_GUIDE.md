# Guia de importação Unity do alfabeto

## Requisitos

- Unity 2019.4.41f1.
- 26 FBX em Assets/Models/Alphabet/FBX.
- Scripts compilando sem erros.
- Backup/commit da cena antes de qualquer migração.

## Configurar e gerar

No Unity, execute em ordem:

1. **RA > Art Pipeline > Configure Alphabet Import**.
2. **RA > Art Pipeline > Generate Alphabet Prefabs**.
3. **RA > Art Pipeline > Validate Alphabet Models**.
4. **RA > Art Pipeline > Generate Model Report**.

Em batch:

~~~powershell
& "C:\Program Files\Unity\Hub\Editor\2019.4.41f1\Editor\Unity.exe" -quit -batchmode -nographics -projectPath "C:\Projetos\RA" -executeMethod AlphabetModelImporter.RunBatch -logFile "Builds\Art\Alphabet\unity-import.log"
~~~

O importador fixa escala 1, file scale, sem animação/blend shapes/câmeras/luzes/visibilidade, leitura desativada, compressão média, malha otimizada, sem collider, normais importadas e tangentes desativadas.

## Inspeção

Abra cada prefab e confirme um MeshFilter, um MeshRenderer, material RA_Mat_X, posição/rotação zero e escala um. Compare silhueta, leitura frontal, base e espessura. O relatório versionado fica em docs/art-pipeline/UNITY_MODEL_REPORT.md.

## Substituição manual

Não edite SampleScene até criar commit de segurança. Use UNITY_MODEL_MIGRATION_PLAN.md; para cada Image Target, anote transformação do filho legado, instancie o prefab correspondente, replique apenas a transformação necessária e remova a instância antiga somente após validar tracking. Salve e teste um target por vez.

## Vuforia e testes

A cópia legada Vuforia 8.3.8 está em Assets/Vuforia, porém a biblioteca compilada correspondente não está declarada em Packages/manifest.json. Não instale Vuforia 8.5 sobre esses scripts: há conflito de GUID/API. Restaure o pacote 8.3.8 compatível ou planeje uma migração completa separada antes de executar o batch no projeto principal.

Os artefatos atuais foram gerados e os testes EditMode executados em sandbox Unity 2019.4, usando os mesmos FBX e .meta. Resultado: 2 testes, 2 aprovados.

## Rollback

1. Não salve a cena se a inspeção falhar.
2. Restaure apenas o commit da migração da cena pelo fluxo Git normal; não apague os modelos legados.
3. Revalide os hashes SHA-256.
4. Reabra a cena e confirme as referências anteriores.
5. Só retome após corrigir a causa em branch dedicada.
