# TCC - Aplicativo de Detecção de Tentativas de Phishing

Aplicativo Windows para detectar possíveis tentativas de phishing.

## Requisitos

- Windows e .NET 10 SDK.
- Para executar o aplicativo: serviço MongoDB ativo em `localhost:27017`, com a base `phishing_poc` carregada.

## Executar o aplicativo

Na raiz do repositório, rode:

```powershell
dotnet run --project DetectorPhishing\DetectorPhishing.csproj
```

O aplicativo inicia na bandeja do sistema. Use o menu do ícone e escolha **Sair** para fechá-lo.

## Executar os testes

Os testes locais não precisam do MongoDB:

```powershell
dotnet run --project DetectorPhishing\DetectorPhishing.csproj -- --testes
```

Para rodar também os testes de integração, inicie o MongoDB e carregue a base de validação antes. **Atenção:** o seed limpa e recria as coleções do banco `phishing_poc`:

```powershell
mongosh --file base\seed.mongo.js
dotnet run --project DetectorPhishing\DetectorPhishing.csproj -- --testes-integracao
```
