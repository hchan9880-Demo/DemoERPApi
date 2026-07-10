# Locate the file (searches recursively if nested)
$trxFile = Get-ChildItem -Path . -Filter "test_results.trx" -Recurse | Select-Object -First 1

if ($trxFile) {
    [xml]$xml = Get-Content $trxFile.FullName
    
    # Extract overall summary statistics
    $counters = $xml.TestRun.ResultSummary.Counters
    
    $report = @"
# Milestone 4.7 Test Run Summary

* **Total Tests:** $($counters.total)
* **Passed:** $($counters.passed)
* **Failed:** $($counters.failed)
* **Outcome:** $($xml.TestRun.ResultSummary.outcome)

---

## [X] Failed Test Details ($($counters.failed))

"@

    # Loop through each failed unit test execution
    $xml.TestRun.Results.UnitTestResult | Where-Object { $_.outcome -eq "Failed" } | ForEach-Object {
        $testName = $_.testName
        $errorMessage = $_.Output.ErrorInfo.Message.Trim()
        
        $report += "### FAIL: $testName`n"
        $report += ">>> **Error:** $errorMessage`n`n"
    }

    # Output to a markdown file
    $report | Out-File -FilePath "Milestone_4_7_Report.md" -Encoding utf8
    Write-Host "Success! Report written to Milestone_4_7_Report.md" -ForegroundColor Green
} else {
    Write-Error "Could not find test_results.trx file. Please run 'dotnet test --logger trx' first."
}