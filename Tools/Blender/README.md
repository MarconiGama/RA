# Automação Blender do alfabeto

## Requisitos

- Windows 11.
- Blender 2.93.18 em C:\Program Files\Blender Foundation\Blender 2.93\blender.exe.
- Git e branch agent/scale-ra-foundation.
- Nunca use Blender 5.2 para abrir/salvar os arquivos deste pipeline.

## Build completo

~~~powershell
.\Tools\Blender\build-alphabet-assets.ps1
.\Tools\Blender\build-alphabet-assets.ps1 -Letters A,B,C
.\Tools\Blender\build-alphabet-assets.ps1 -Overwrite
~~~

O build exige Git limpo, gera fontes, valida, exporta FBX, reimporta e produz relatórios. Sem -Overwrite, qualquer destino existente causa falha deliberada.

## Comandos diretos

~~~powershell
& "C:\Program Files\Blender Foundation\Blender 2.93\blender.exe" --background --factory-startup --python Tools\Blender\generate_alphabet.py -- --letters A-Z
& "C:\Program Files\Blender Foundation\Blender 2.93\blender.exe" --background --factory-startup --python Tools\Blender\export_alphabet_fbx.py -- --letters A-Z
& "C:\Program Files\Blender Foundation\Blender 2.93\blender.exe" --background --factory-startup --python Tools\Blender\validate_alphabet.py -- --letters A-Z --stage all
~~~

generate_alphabet.py aceita --output-dir, --letters, --overwrite e --report. export_alphabet_fbx.py aceita diretórios de entrada/saída, seleção, overwrite e relatório. validate_alphabet.py aceita --stage blend|fbx|all, limites fixos e saídas JSON/Markdown. inspect_blend.py inspeciona uma fonte individual sem salvá-la.

## Relatórios

Arquivos transitórios ficam em Builds/Art/Alphabet: geração, exportação, validação e logs. O relatório Markdown aprovado é versionado em docs/art-pipeline/VALIDATION_REPORT.md.

## Solução de erros

- **Blender ausente/versão incorreta**: instale 2.93.18 no caminho obrigatório; não altere o script para outra versão.
- **Git sujo/branch incorreta**: revise e preserve mudanças antes de executar. Não use reset destrutivo.
- **Arquivo existente**: use -Overwrite somente quando a regeneração for intencional.
- **Face degenerada/topologia**: corrija o gerador e regenere; não edite o FBX manualmente.
- **FBX vazio/nome duplicado**: consulte o log, interrompa e não importe no Unity.
- **.blend1**: o gerador desativa backups e remove backups apenas dos destinos novos; a validação estática reprova qualquer temporário.
- **Unity/Vuforia não compila**: restaure exatamente a dependência Vuforia legada ou use sandbox para validar arte; não misture versões.
