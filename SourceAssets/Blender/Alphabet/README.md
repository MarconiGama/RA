# Fontes Blender do alfabeto

Esta pasta guarda os 26 arquivos-fonte reproduzíveis. blends/ contém RA_Letter_A.blend–RA_Letter_Z.blend; references/ e textures/ são reservadas a insumos opcionais que não podem se tornar dependências obrigatórias.

Não edite nomes, eixos ou materiais fora de MODEL_SPECIFICATION.md. Para alterar uma letra, modifique Tools/Blender/generate_alphabet.py, gere apenas a seleção desejada com Blender 2.93.18, valide e então exporte. Não salve estes arquivos em Blender incompatível, especialmente Blender 5.2.

Os arquivos em Assets/Objetos são legados separados e não devem ser abertos, sobrescritos ou removidos pelo pipeline. A exportação oficial é sempre feita por build-alphabet-assets.ps1.
