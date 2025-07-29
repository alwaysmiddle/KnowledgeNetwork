# PowerShell script to test the Knowledge Network API
Write-Host "Testing Knowledge Network API..." -ForegroundColor Green

# Start the API in background
$apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory "." -PassThru -WindowStyle Hidden
Write-Host "Starting API server..." -ForegroundColor Yellow

# Wait for API to start
Start-Sleep -Seconds 5

try {
    # Test health endpoint
    Write-Host "`nTesting health endpoint..." -ForegroundColor Yellow
    $healthResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/health" -Method GET
    Write-Host "Health check passed: $($healthResponse | ConvertTo-Json)" -ForegroundColor Green

    # Test get nodes (should be empty initially)
    Write-Host "`nTesting GET nodes endpoint..." -ForegroundColor Yellow
    $nodesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/nodes" -Method GET
    Write-Host "Nodes retrieved: $($nodesResponse.Count) nodes" -ForegroundColor Green

    # Test create node
    Write-Host "`nTesting POST node endpoint..." -ForegroundColor Yellow
    $newNode = @{
        title = "Test Node"
        content = "This is a test node created by PowerShell"
        nodeType = "concept"
        xPosition = 100.0
        yPosition = 200.0
    }
    $createResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/nodes" -Method POST -Body ($newNode | ConvertTo-Json) -ContentType "application/json"
    Write-Host "Node created successfully: ID $($createResponse.id)" -ForegroundColor Green

    # Test get nodes again (should have 1 now)
    Write-Host "`nTesting GET nodes endpoint again..." -ForegroundColor Yellow
    $nodesResponse2 = Invoke-RestMethod -Uri "http://localhost:5000/api/nodes" -Method GET
    Write-Host "Nodes retrieved: $($nodesResponse2.Count) nodes" -ForegroundColor Green
    Write-Host "First node: $($nodesResponse2[0].title)" -ForegroundColor Green

    Write-Host "`n✅ All API tests passed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "❌ API test failed: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    # Stop the API process
    if ($apiProcess -and !$apiProcess.HasExited) {
        Stop-Process -Id $apiProcess.Id -Force
        Write-Host "`nAPI server stopped." -ForegroundColor Yellow
    }
}