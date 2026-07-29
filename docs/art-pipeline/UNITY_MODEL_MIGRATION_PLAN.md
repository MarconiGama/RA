# Plano de migração dos modelos Unity

Este plano foi criado antes de qualquer substituição. A cena Assets/Scenes/SampleScene.unity não foi alterada.

| Image Target | Letra | Objeto legado atual | Prefab novo sugerido | Referência existente | Risco |
|---|---|---|---|---|---|
| Image Target A | A | RaA (legado) | RA_Letter_A | Sim (19 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target B | B | RaB (legado) | RA_Letter_B | Sim (28 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target C | C | RaC (legado) | RA_Letter_C | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target D | D | RaD (legado) | RA_Letter_D | Sim (23 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target E | E | RaE (legado) | RA_Letter_E | Sim (20 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target F | F | RaF (legado) | RA_Letter_F | Sim (18 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target G | G | RaG (legado) | RA_Letter_G | Sim (28 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target H | H | RaH (legado) | RA_Letter_H | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target I | I | RaI (legado) | RA_Letter_I | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target J | J | RaJ (legado) | RA_Letter_J | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target K | K | RaK (legado) | RA_Letter_K | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target L | L | RaL (legado) | RA_Letter_L | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target M | M | RaM (legado) | RA_Letter_M | Sim (22 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target N | N | RaN (legado) | RA_Letter_N | Sim (18 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target O | O | RaO (legado) | RA_Letter_O | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target P | P | RaP (legado) | RA_Letter_P | Sim (18 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target Q | Q | RaQ (legado) | RA_Letter_Q | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target R | R | RaR (legado) | RA_Letter_R | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target S | S | RaS (legado) | RA_Letter_S | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target T | T | RaT (legado) | RA_Letter_T | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target U | U | RaU (legado) | RA_Letter_U | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target V | V | RaV (legado) | RA_Letter_V | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target W | W | RaW (legado) | RA_Letter_W | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target X | X | RaX (legado) | RA_Letter_X | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target Y | Y | RaY (legado) | RA_Letter_Y | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |
| Image Target Z | Z | RaZ (legado) | RA_Letter_Z | Sim (17 ocorrências do GUID na cena) | Alto: validar hierarquia, escala e tracking no aparelho |

## Estratégia segura

1. Preservar o commit-base e os checksums dos 26 arquivos em Assets/Objetos.
2. Validar FBX, materiais e prefabs pelo menu **RA > Art Pipeline**.
3. Duplicar a cena para ensaio ou trabalhar em branch dedicada de migração.
4. Substituir um target por vez, preservando transformações locais e componentes Vuforia.
5. Testar A–Z no Editor e em aparelho real; gerar APK antes de remover qualquer legado.
6. Fazer rollback restaurando a cena/prefab de baseline. Os legados não devem ser removidos até aprovação integral.

Os números acima contam ocorrências do GUID serializado de cada arquivo .blend na cena; incluem malha, material e demais referências internas, portanto não representam quantidade de instâncias.

