# Fluxo de trabalho Git

Regras definidas pelo dono do projeto. Valem para qualquer pessoa ou agente de IA que trabalhe neste repositório.

## Branches

| Branch | Uso |
| :--- | :--- |
| `main` | Protegida. Push direto bloqueado. Só recebe alterações por pull request vindo de `dev`. |
| `dev` | Branch de trabalho. Todos os commits do dia a dia vão aqui. |

## Regras

1. **Commits só na `dev`.** Nunca faça commit nem push direto na `main`.
2. **Sem branches extras.** Não crie branches de feature, fix ou qualquer outra sem pedido explícito do dono do projeto.
3. **PR `dev` → `main` só quando pedido.** Quando um marco de desenvolvimento for atingido, o dono do projeto pede a abertura do pull request. Não abra PR por conta própria.
4. **Antes de commitar, confira a branch** (`git branch --show-current` deve retornar `dev`). Se estiver em outra, pare e pergunte.

## Remoto

- `origin`: https://github.com/marcalsantarem/m-ops.git

## Ambientes publicados (a partir da branch `dev`)
- API: https://mops-api-n2ql.onrender.com
- Front: https://m-ops.netlify.app
