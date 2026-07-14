# Ambiente de Desenvolvimento Congelado

## Linha legada suportada

- projeto originalmente criado no Unity Editor `2019.4.20f1`;
- versão operacional adotada: Unity Editor `2019.4.41f2`;
- projeto Android com Vuforia legado incorporado;
- cenas: `Menu.unity` e `SampleScene.unity`;
- módulos Android instalados pelo Unity Hub da mesma versão do Editor.

## Instalação recomendada

No Unity Hub, instale a versão `2019.4.41f2` com:

- Android Build Support;
- Android SDK & NDK Tools;
- OpenJDK.

Use os componentes fornecidos pelo Hub para evitar incompatibilidade entre Gradle, SDK, NDK e JDK.

A revisão `2019.4.41f2` permanece na mesma linha LTS do projeto original e inclui a correção de segurança disponibilizada para a série 2019.4, além de uma correção posterior de licenciamento.

## Migração futura

A atualização para uma versão moderna do Unity/Vuforia deve ocorrer em branch separada. Critérios obrigatórios:

1. projeto abre sem reserialização destrutiva;
2. Menu e SampleScene carregam;
3. câmera inicializa em Android;
4. targets A–Z são reconhecidos;
5. APK de desenvolvimento é gerado;
6. AAB assinado é gerado;
7. desempenho é medido em aparelhos de entrada e intermediários.

A branch principal não deve receber atualização automática para Unity 6 antes desses testes.
