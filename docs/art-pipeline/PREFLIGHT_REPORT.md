# Relatório de preflight — pipeline de arte do alfabeto

- Gerado em: 2026-07-29T18:18:40.1842696-04:00
- Repositório: C:\Projetos\RA
- Branch: agent/scale-ra-foundation
- Commit base: 2488ef89b376f827283e55561e18af98129ae043
- Estado inicial do Git: limpo
- Blender: C:\Program Files\Blender Foundation\Blender 2.93\blender.exe
- Versão confirmada: 2.93.18
- Unity configurado: 2019.4.41f1
- Cenas: Assets/Scenes/Menu.unity e Assets/Scenes/SampleScene.unity
- Conteúdo: Assets/Resources/Content/alphabet-pt-br.json

## Modelos legados

Foram encontrados 26 de 26 arquivos, letras A–Z. Arquivos ausentes: nenhum. Os hashes estão em legacy-model-inventory.json e legacy-model-checksums.sha256.

## Riscos e controles

- Os arquivos .blend legados continuam dentro de Assets/Objetos; serão preservados até validação em aparelho e APK.
- A cena usa referências serializadas aos modelos antigos; nenhuma substituição automática será feita.
- Arquivos Blender não são compatíveis entre todas as versões; o pipeline fixa Blender 2.93.18.
- FBX e prefabs devem ser regenerados de forma determinística e validados antes de integração.
- Alterações param em caso de branch divergente, Git sujo no início do build, Blender ausente ou artefato inválido.

