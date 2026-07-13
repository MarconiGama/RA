# RealidadeA

Aplicativo educacional de realidade aumentada criado em Unity e Vuforia. O projeto reconhece targets do alfabeto e exibe conteúdos associados, com foco em uso offline e evolução para uma plataforma educacional configurável.

## Ambiente congelado do projeto legado

- Unity: `2019.4.20f1`;
- cenas obrigatórias: `Assets/Scenes/Menu.unity` e `Assets/Scenes/SampleScene.unity`;
- target inicial: Android;
- Vuforia: integração legada incorporada ao projeto;
- Android SDK, NDK e JDK: instalar os módulos recomendados pelo Unity Hub para a versão `2019.4.20f1`.

Não atualize Unity ou Vuforia diretamente na branch principal. Faça qualquer migração em branch separada e valide todos os targets A–Z.

## Builds Android

O projeto possui um único pipeline em `Assets/Editor/AndroidBuildPipeline.cs`.

### Linux/macOS/Git Bash

```bash
./build-android.sh development
```

### Windows PowerShell

```powershell
./build-android.ps1 -Profile development
```

Saída padrão:

```text
Builds/Android/RealidadeA-development.apk
```

### AAB de produção

Defina as variáveis de assinatura antes do build:

```text
RA_APPLICATION_ID
RA_BUNDLE_VERSION
RA_VERSION_CODE
RA_KEYSTORE_PATH
RA_KEYSTORE_PASSWORD
RA_KEY_ALIAS
RA_KEY_ALIAS_PASSWORD
```

Depois execute:

```bash
./build-android.sh production
```

Saída padrão:

```text
Builds/Android/RealidadeA-production.aab
```

O keystore e suas senhas nunca devem ser adicionados ao Git.

## Validação automática

Antes de cada build, o pipeline confirma que:

- as duas cenas obrigatórias existem;
- ambas estão habilitadas no Build Settings;
- `Menu.unity` é a primeira cena;
- o target Android pode ser ativado;
- AAB de produção possui application ID e assinatura configurados.

No Editor, também é possível executar `RA > Validate Project`.

## Conteúdo configurável

O pacote inicial fica em:

```text
Assets/Resources/Content/alphabet-pt-br.json
```

Cada item define target, título, descrição, áudio e prefab. Isso permite adicionar novos pacotes sem concentrar toda a regra nas cenas.

## Serviços adicionados

- `ContentRepository`: carrega e valida pacotes educacionais;
- `AudioService`: desacopla reprodução de áudio;
- `ProgressService`: mantém progresso local por perfil;
- `LocalTelemetryService`: registra eventos offline para exportação futura.

A integração progressiva desses serviços às cenas deve preservar os objetos Vuforia existentes até a conclusão dos testes em aparelho real.

## Testes

Os testes de Editor estão em `Assets/Tests/Editor`. Execute pelo Unity Test Runner:

```text
Window > General > Test Runner > EditMode > Run All
```

## Privacidade e piloto

- política inicial: `docs/PRIVACY_POLICY.md`;
- plano de piloto: `docs/PILOT_PLAN.md`.

A política precisa de revisão jurídica e preenchimento dos dados do responsável antes da publicação.
