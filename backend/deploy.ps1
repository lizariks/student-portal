Write-Host "Deploying Keycloak..." -ForegroundColor Cyan

cd  @"C:\Program Files (x86)\Pulumi\pulumi.exe"
pulumi up --yes

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed!" -ForegroundColor Red
    exit 1
}

cd ..
Start-Sleep -Seconds 5

.\configure-keycloak.ps1

Write-Host "`nDone! Test at https://localhost:7048/swagger" -ForegroundColor Green