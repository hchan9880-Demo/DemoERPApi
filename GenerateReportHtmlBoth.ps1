# Configuration
$searchRoot = "C:\Users\KingBoo\source\repos\DemoERPApi"
$outputFile = "Milestone_7_Report.html"

# 1. Locate the TRX file dynamically (newest one)
$trxFile = Get-ChildItem -Path $searchRoot -Filter "test_results.trx" -Recurse -File | 
           Sort-Object LastWriteTime -Descending | 
           Select-Object -First 1

if (-not $trxFile) {
    Write-Error "Could not find 'test_results.trx' in $searchRoot"
    exit
}

# 2. Load and Parse XML
[xml]$xml = Get-Content $trxFile.FullName
$ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
$ns.AddNamespace("t", $xml.DocumentElement.NamespaceURI)

# Extract Summary Counters
$counters = $xml.SelectSingleNode("//t:ResultSummary/t:Counters", $ns)
$outcome = $xml.SelectSingleNode("//t:ResultSummary", $ns).outcome

# 3. Build HTML Header
$html = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Test Run Summary</title>
    <style>
        body { font-family: sans-serif; margin: 40px; background-color: #f8f9fa; }
        .summary-card { background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); margin-bottom: 20px; }
        .badge { padding: 4px 8px; border-radius: 4px; font-weight: bold; }
        .badge-passed { background-color: #d4edda; color: #155724; }
        .badge-failed { background-color: #f8d7da; color: #721c24; }
        details { background: #fff; margin: 10px 0; padding: 10px; border: 1px solid #ccc; border-radius: 4px; }
        summary { font-weight: bold; cursor: pointer; padding: 5px; }
        .failure-card { border-left: 5px solid #dc3545; padding: 10px; margin: 10px 0; background: #fff; }
        .success-card { border-left: 5px solid #28a745; padding: 10px; margin: 10px 0; background: #fff; }
        .error-msg { background: #fdf2f2; padding: 5px; font-family: monospace; white-space: pre-wrap; font-size: 0.9em; }
    </style>
</head>
<body>
    <h1>Milestone 7 Test Run Summary</h1>
    <div class="summary-card">
        <div><strong>Total:</strong> $($counters.total)</div>
        <div><strong>Passed:</strong> <span class="badge badge-passed">$($counters.passed)</span></div>
        <div><strong>Failed:</strong> <span class="badge badge-failed">$($counters.failed)</span></div>
        <div><strong>Overall Outcome:</strong> $outcome</div>
    </div>
"@

# 4. Process Failed Tests (Sorted A-Z)
$html += '<details open><summary style="color: #b02a37;">[X] Failed Tests</summary>'
$failedTests = @($xml.SelectNodes("//t:UnitTestResult[@outcome='Failed']", $ns))
$failedTests | Sort-Object -Property testName | ForEach-Object {
    $testName = $_.testName
    $msg = if ($_.Output.ErrorInfo.Message) { $_.Output.ErrorInfo.Message.Trim() } else { "No error message." }
    $html += "<div class='failure-card'><strong>[FAIL] $testName</strong><div class='error-msg'>$msg</div></div>"
}
$html += '</details>'

# 5. Process Passed Tests (Sorted A-Z)
$html += '<details><summary style="color: #2b8a3e;">[OK] Passed Tests</summary>'
$passedTests = @($xml.SelectNodes("//t:UnitTestResult[@outcome='Passed']", $ns))
$passedTests | Sort-Object -Property testName | ForEach-Object {
    $testName = $_.testName
    $html += "<div class='success-card'><strong>[PASS] $testName</strong></div>"
}
$html += '</details></body></html>'

# 6. Save and Finish
$html | Out-File -FilePath $outputFile -Encoding utf8
Write-Host "Success! '$outputFile' generated in $(Get-Location)." -ForegroundColor Green