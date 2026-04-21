param(
    [string]$ClientId     = "your-client-id",
    [string]$Username     = "your-username",
    [string]$Password     = "your-password",
    [string]$ClientSecret = ""
)

$Realm    = "flash-sales-dev"
$BaseUrl  = "http://localhost:8080"
$TokenUrl = "$BaseUrl/realms/$Realm/protocol/openid-connect/token"

$body = @{
    grant_type = "password"
    client_id  = $ClientId
    username   = $Username
    password   = $Password
}

if ($ClientSecret -ne "") {
    $body["client_secret"] = $ClientSecret
}

try {
    $response = Invoke-RestMethod -Uri $TokenUrl -Method Post -Body $body -ContentType "application/x-www-form-urlencoded"

    Write-Host "`n=== Token generated with success ===" -ForegroundColor Green
    Write-Host "`nAccess Token:" -ForegroundColor Cyan
    Write-Host $response.access_token
    Write-Host "`nExpires in: $($response.expires_in) seconds" -ForegroundColor Yellow

    if ($response.refresh_token) {
        Write-Host "`nRefresh Token:" -ForegroundColor Cyan
        Write-Host $response.refresh_token
    }

    $response.access_token | Set-Clipboard
    Write-Host "`n[Access token copied to clipboard]" -ForegroundColor DarkGray

    return $response
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorBody  = $_.ErrorDetails.Message

    Write-Host "`n=== Fail to get token ===" -ForegroundColor Red
    Write-Host "Status: $statusCode" -ForegroundColor Red
    Write-Host "Detalhe: $errorBody" -ForegroundColor Red
}
