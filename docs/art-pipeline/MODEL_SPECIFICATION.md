# Especificação dos modelos 3D do alfabeto

## Linguagem visual

Letras maiúsculas, amigáveis, arredondadas e imediatamente legíveis. Não há detalhes pequenos, transparência, textura externa, subsurface, metal ou shader complexo. A paleta alterna azul, verde, amarelo, laranja, vermelho, roxo, rosa e ciano.

## Geometria e escala

| Propriedade | Padrão |
|---|---|
| Unidade | 1 unidade Blender = 1 unidade Unity |
| Dimensão máxima | 1,80 |
| Limite de aprovação | até 2,00 em qualquer eixo |
| Profundidade aplicada | 0,28 |
| Extrusão de curva | 0,16 antes da normalização |
| Bevel | 0,035, aplicado ao converter para mesh |
| Resolução da curva | 8 |
| Resolução do bevel | 4 |
| Faixa observada | 792–4.224 vértices; aproximadamente 850–4.600 faces |
| Limites de validação | 50–20.000 vértices |
| Origem | centro da base, com Z mínimo igual a zero |

A fonte preferencial é Arial Rounded MT Bold, seguida de Arial Bold; Bfont do Blender é o fallback seguro. O relatório de geração registra a fonte efetivamente usada.

## Eixos

No Blender: X é largura, Y é profundidade e Z é altura. O modelo fica em pé no plano XZ. Na exportação FBX: forward -Z, up Y, escala 1, unidade aplicada e sem bake de transformação espacial.

## Nomes

- Objeto: RA_Letter_A até RA_Letter_Z.
- Mesh: RA_Letter_A_Mesh até RA_Letter_Z_Mesh.
- Material: RA_Mat_A até RA_Mat_Z.
- Fonte: SourceAssets/Blender/Alphabet/blends/RA_Letter_A.blend.
- Intercâmbio: Assets/Models/Alphabet/FBX/RA_Letter_A.fbx.
- Prefab: Assets/Models/Alphabet/Prefabs/RA_Letter_A.prefab.

## Materiais

Um material por letra, Standard/Principled simples, opaco, metallic 0 e brilho moderado. Cores base são incorporadas como referência no FBX e recriadas como materiais Unity versionados, sem texturas obrigatórias.

## Critérios de aprovação

Cada letra precisa ter um único objeto mesh e material, transformação aplicada, origem/base válidas, normais externas, volume positivo, zero faces degeneradas, vértices duplicados ou soltos, nenhuma aresta não manifold, câmera, luz, armature, modificador ou animação. O conjunto só é integralmente aprovado com A–Z presente em .blend, FBX e prefab.
