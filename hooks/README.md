# Hook post-commit do M-Ops

Após cada commit cuja mensagem tenha `#<id>` (ex.: `feat: ajusta menu (#42)`), o hook envia hash, mensagem, autor e data para `POST /api/commits/hook`. Ele nunca bloqueia o commit: falhas viram aviso e o timeout é de 5 s.

- `post-commit`: versão Bash (Linux, macOS e Git for Windows). Requer `curl`; usa `jq` se existir.
- `post-commit.ps1`: versão PowerShell (Windows PowerShell 5.1 ou pwsh 7+).

## Configurar URL e token

Por variável de ambiente (vale para todos os repositórios):

```sh
export MOPS_API_URL=https://sua-api.onrender.com
export MOPS_API_TOKEN=seu-token
```

Ou por repositório (fallback quando as variáveis não existem):

```sh
git config mops.apiUrl https://sua-api.onrender.com
git config mops.apiToken seu-token
```

O token é o mesmo `Auth__ApiToken` da API. Com `git config` ele fica em `.git/config` (não versionado).

## Instalar em outro repositório (Bash)

Copie o hook:

```sh
cp /caminho/m-ops/hooks/post-commit /caminho/outro-repo/.git/hooks/post-commit
chmod +x /caminho/outro-repo/.git/hooks/post-commit
```

Ou aponte o repositório para a pasta de hooks do M-Ops (substitui todos os hooks locais dele):

```sh
git -C /caminho/outro-repo config core.hooksPath /caminho/m-ops/hooks
```

## Windows

O Git for Windows executa hooks pelo Bash que vem com ele, então o `post-commit` Bash já funciona. Para usar a versão PowerShell, crie `.git/hooks/post-commit` no repositório com uma linha:

```sh
#!/bin/sh
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/caminho/m-ops/hooks/post-commit.ps1"
```

Troque `powershell` por `pwsh` se preferir o PowerShell 7.
