# M-Ops post-commit hook (PowerShell): envia o ultimo commit para POST /api/commits/hook.
# Config: MOPS_API_URL / MOPS_API_TOKEN (env) ou git config mops.apiUrl / mops.apiToken.
# Nunca falha o commit: qualquer problema vira aviso em stderr e exit 0.
# Compativel com Windows PowerShell 5.1 e pwsh 7+.

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

function Write-HookWarning([string]$Text) {
    [Console]::Error.WriteLine("m-ops hook: $Text")
}

function Invoke-Git([string[]]$GitArgs) {
    $out = & git @GitArgs 2>$null
    if ($LASTEXITCODE -ne 0) { return $null }
    return (@($out) -join "`n")
}

function Send-Commit {
    # Saida do git em UTF-8 (o padrao do console no 5.1 e a codepage OEM).
    try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch { }

    $msg = Invoke-Git @('log', '-1', '--format=%B')
    if ($null -eq $msg) { Write-HookWarning 'nao foi possivel ler o ultimo commit'; return }
    $msg = $msg.TrimEnd()

    # Sem referencia #<id>: nada a enviar.
    if ($msg -notmatch '#[0-9]+') { return }

    $hash = Invoke-Git @('log', '-1', '--format=%H')
    $author = Invoke-Git @('log', '-1', '--format=%an <%ae>')
    $date = Invoke-Git @('log', '-1', '--format=%cI')
    $short = if ($hash) { $hash.Substring(0, [Math]::Min(7, $hash.Length)) } else { '?' }

    $url = $env:MOPS_API_URL
    if ([string]::IsNullOrWhiteSpace($url)) { $url = Invoke-Git @('config', '--get', 'mops.apiUrl') }
    $token = $env:MOPS_API_TOKEN
    if ([string]::IsNullOrWhiteSpace($token)) { $token = Invoke-Git @('config', '--get', 'mops.apiToken') }
    if ([string]::IsNullOrWhiteSpace($url) -or [string]::IsNullOrWhiteSpace($token)) {
        Write-HookWarning 'MOPS_API_URL/MOPS_API_TOKEN (ou git config mops.apiUrl/mops.apiToken) nao definidos; commit nao enviado'
        return
    }
    $url = $url.Trim().TrimEnd('/')

    $body = [ordered]@{ commitHash = $hash; message = $msg; author = $author; committedAt = $date } |
        ConvertTo-Json -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($body)

    # TLS 1.2 no Windows PowerShell 5.1 (no pwsh e ignorado/inofensivo).
    try {
        [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
    } catch { }

    try {
        $resp = Invoke-WebRequest -Uri "$url/api/commits/hook" -Method Post -UseBasicParsing -TimeoutSec 5 `
            -ContentType 'application/json; charset=utf-8' -Headers @{ Authorization = "Bearer $($token.Trim())" } `
            -Body $bytes
        $code = [int]$resp.StatusCode
        if ($code -ne 200 -and $code -ne 201) { Write-HookWarning "API respondeu HTTP $code para o commit $short" }
    } catch {
        $code = $null
        try { if ($_.Exception.Response) { $code = [int]$_.Exception.Response.StatusCode } } catch { }
        if ($code) { Write-HookWarning "API respondeu HTTP $code para o commit $short" }
        else { Write-HookWarning "API inacessivel em $url (commit $short nao enviado)" }
    }
}

try { Send-Commit } catch { Write-HookWarning 'erro inesperado; commit nao enviado' }
exit 0
