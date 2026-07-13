# Checklist de Release Android

## APK de homologação

- [ ] Unity 2019.4.20f1 instalado com Android Build Support.
- [ ] `RA > Validate Project` executado sem erros.
- [ ] Testes EditMode executados.
- [ ] `build-android.ps1 -Profile development` ou `./build-android.sh development` executado.
- [ ] APK instalado em pelo menos três aparelhos.
- [ ] Permissão de câmera validada.
- [ ] Targets A–Z testados.
- [ ] Progresso e telemetria local validados.

## AAB de produção

- [ ] Application ID definitivo definido.
- [ ] Keystore definitivo armazenado em local seguro e com backup.
- [ ] Senhas mantidas fora do Git.
- [ ] `RA_BUNDLE_VERSION` atualizado.
- [ ] `RA_VERSION_CODE` incrementado.
- [ ] Política de privacidade revisada juridicamente.
- [ ] Formulários de segurança de dados e público-alvo revisados.
- [ ] AAB gerado com perfil production.
- [ ] Teste interno ou fechado realizado antes da produção.
