# Status da execução da prioridade imediata

## Concluído nesta fundação

- pipeline Android unificado;
- APK de desenvolvimento;
- AAB de produção;
- assinatura por variáveis de ambiente;
- versionamento configurável;
- ambiente legado documentado e congelado;
- validação automática das cenas;
- alfabeto A–Z convertido em pacote JSON configurável;
- contratos separados para conteúdo, áudio, progresso, telemetria e rastreamento;
- testes iniciais do repositório de conteúdo;
- política inicial de privacidade;
- plano operacional do piloto educacional.

## Próxima etapa dentro do Unity Editor

- integrar `ContentRepository` aos objetos existentes da `SampleScene`;
- mapear cada target Vuforia para o item JSON correspondente;
- vincular os prefabs e áudios reais aos caminhos configurados;
- chamar `ProgressService` e `LocalTelemetryService` nos eventos de reconhecimento;
- executar os testes EditMode;
- gerar APK em máquina com Unity e Android Build Support;
- testar câmera e targets A–Z em aparelhos físicos;
- gerar keystore definitivo e AAB assinado.

A integração de cena foi deliberadamente isolada para não reserializar ou quebrar o projeto Vuforia legado sem o Unity Editor disponível para validação visual.
