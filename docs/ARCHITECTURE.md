# Arquitetura de Evolução

## Princípio

A cena e o SDK de realidade aumentada não devem concentrar conteúdo, progresso, áudio e telemetria. Cada responsabilidade possui um contrato separado para permitir testes e futura troca do mecanismo de rastreamento.

## Componentes

- `ContentRepository`: carrega e valida pacotes JSON;
- `TargetTrackingService`: traduz eventos do SDK de RA para eventos de domínio;
- `AudioService`: carrega e reproduz áudio por recurso;
- `ProgressService`: persiste conclusão por perfil;
- `LocalTelemetryService`: mantém uma fila offline de eventos;
- `AndroidBuildPipeline`: valida e gera artefatos Android.

## Fluxo planejado

```text
Target Vuforia reconhecido
        ↓
TargetTrackingService.NotifyFound
        ↓
ContentRepository localiza o item
        ↓
Prefab/áudio são apresentados
        ↓
ProgressService registra conclusão
        ↓
LocalTelemetryService registra evento
```

A adaptação específica ao Vuforia deve ficar em um componente de cena pequeno. O restante da aplicação deve depender apenas dos contratos acima.
