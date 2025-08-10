Write-Host "Installing k6..." -ForegroundColor Cyan
choco install k6 -y

Write-Host "Running k6 tests..." -ForegroundColor Cyan
k6 run "k6-tests\spike.js"
k6 run "k6-tests\mixed.js"
k6 run "k6-tests\concurrent-edit.js"
k6 run "k6-tests\baseline.js"

Write-Host "Load test complete." -ForegroundColor Green