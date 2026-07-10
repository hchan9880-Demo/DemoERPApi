# Locate the file (searches recursively if nested)
$trxFile = Get-ChildItem -Path . -Filter "test_results.trx" -Recurse | Select-Object -First 1

if ($trxFile) {
    [xml]$xml = Get-Content $trxFile.FullName
    
    # Extract overall summary statistics
    $counters = $xml.TestRun.ResultSummary.Counters
    $outcome = $xml.TestRun.ResultSummary.outcome

    # Set up the HTML document wrapper and CSS styles
    $html = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Milestone 4.7 Test Run Summary</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif; margin: 40px; background-color: #f8f9fa; color: #333; }
        h1 { border-bottom: 2px solid #dee2e6; padding-bottom: 10px; color: #212529; }
        h2 { margin-top: 30px; color: #495057; }
        .summary-card { background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); display: inline-block; min-width: 300px; margin-bottom: 20px; }
        .summary-item { margin: 10px 0; font-size: 1.1em; }
        .badge { padding: 4px 8px; border-radius: 4px; font-weight: bold; font-size: 0.9em; }
        .badge-total { background-color: #e9ecef; color: #495057; }
        .badge-passed { background-color: #d4edda; color: #155724; }
        .badge-failed { background-color: #f8d7da; color: #721c24; }
        .failure-card { background: #fff; border-left: 5px solid #dc3545; padding: 15px; margin-bottom: 15px; border-radius: 0 8px 8px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.02); }
        .failure-title { font-weight: bold; color: #c92a2a; font-size: 1.1em; margin-bottom: 5px; }
        .error-msg { background: #fdf2f2; color: #b02a37; border: 1px solid #f5c2c7; padding: 10px; border-radius: 4px; font-family: SFMono-Regular, Menlo, Monaco, Consolas, monospace; font-size: 0.9em; white-space: pre-wrap; margin-top: 5px; }
    </style>
</head>
<body>

    <h1>Milestone 4.7 Test Run Summary</h1>
    
    <div class="summary-card">
        <div class="summary-item"><strong>Total Tests:</strong> <span class="badge badge-total">$($counters.total)</span></div>
        <div class="summary-item"><strong>Passed:</strong> <span class="badge badge-passed">$($counters.passed)</span></div>
        <div class="summary-item"><strong>Failed:</strong> <span class="badge badge-failed">$($counters.failed)</span></div>
        <div class="summary-item"><strong>Overall Outcome:</strong> <strong>$outcome</strong></div>
    </div>

    <h2>Failed Test Details ($($counters.failed))</h2>
"@

    # Loop through each failed unit test execution
    $xml.TestRun.Results.UnitTestResult | Where-Object { $_.outcome -eq "Failed" } | ForEach-Object {
        $testName = $_.testName
        $errorMessage = $_.Output.ErrorInfo.Message.Trim()
        
        # Escape any raw HTML characters in the C# exception message to avoid rendering breaks
        $safeError = [System.Security.SecurityElement]::Escape($errorMessage)

        $html += @"
    <div class="failure-card">
        <div class="failure-title">[FAIL] $testName</div>
        <div class="error-msg">$safeError</div>
    </div>
"@
    }

    $html += @"
</body>
</html>
"@

    # Output to an HTML file using standard ASCII safe formatting
    $html | Out-File -FilePath "Milestone_4_7_Report.html" -Encoding ascii
    Write-Host "Success! Beautiful report written to Milestone_4_7_Report.html" -ForegroundColor Green
} else {
    Write-Error "Could not find test_results.trx file. Please run 'dotnet test --logger trx' first."
}