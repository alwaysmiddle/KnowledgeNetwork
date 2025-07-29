# Test script to verify all API endpoints are working
Write-Host "Testing Knowledge Network API on localhost:5000" -ForegroundColor Green

# Test health endpoint
Write-Host "`n1. Testing /api/health..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "http://localhost:5000/api/health" -Method GET
    Write-Host "✅ Health check successful: $($health.status) at $($health.timestamp)" -ForegroundColor Green
} catch {
    Write-Host "❌ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test nodes GET endpoint  
Write-Host "`n2. Testing GET /api/nodes..." -ForegroundColor Yellow
try {
    $nodes = Invoke-RestMethod -Uri "http://localhost:5000/api/nodes" -Method GET
    Write-Host "✅ Nodes retrieved successfully: $($nodes.Count) nodes found" -ForegroundColor Green
    if ($nodes.Count -gt 0) {
        Write-Host "   Sample node: $($nodes[0].title)" -ForegroundColor Cyan
    }
} catch {
    Write-Host "❌ Nodes GET failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test Swagger UI access
Write-Host "`n3. Testing Swagger UI..." -ForegroundColor Yellow
try {
    $swagger = Invoke-WebRequest -Uri "http://localhost:5000/swagger/index.html" -Method GET
    Write-Host "✅ Swagger UI accessible: Status $($swagger.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "❌ Swagger UI failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test POST endpoint
Write-Host "`n4. Testing POST /api/nodes..." -ForegroundColor Yellow
$testNode = @{
    title = "API Test Node - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    content = "Created by test script to verify POST endpoint"
    nodeType = "concept"
    xPosition = 150
    yPosition = 250
} | ConvertTo-Json

try {
    $newNode = Invoke-RestMethod -Uri "http://localhost:5000/api/nodes" -Method POST -Body $testNode -ContentType "application/json"
    Write-Host "✅ Node created successfully: ID $($newNode.id) - '$($newNode.title)'" -ForegroundColor Green
} catch {
    Write-Host "❌ Node creation failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎉 API Testing Complete!" -ForegroundColor Green