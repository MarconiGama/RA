# RA
Realidade Aumentada

## Gerar APK Android

Este projeto já contém um APK compilado em `primeiroteste.apk`.

Para gerar um novo APK via linha de comando (Unity em modo batch):

```bash
./build-android-apk.sh
```

Saída padrão: `Builds/android/RA-debug.apk`.

Se o executável do Unity não estiver no `PATH`, informe:

```bash
UNITY_BIN=/caminho/para/Unity ./build-android-apk.sh
```
