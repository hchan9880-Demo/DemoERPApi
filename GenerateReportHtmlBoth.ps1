# Configuration Paths
$in_result = "C:\Users\KingBoo\source\repos\DemoERPApi\DemoERPApi.Tests\TestResults\test_results.trx"
$out_result = "C:\Users\KingBoo\source\repos\DemoERPApi\Milestone_7_Report.html"







# 1. Load the XML file
[xml]$testRun = Get-Content $in_result

$allResults = $testRun.TestRun.Results.UnitTestResult

# Sort results by testName (A to Z)
$passedResults = $allResults | Where-Object { $_.outcome -eq "Passed" } | Sort-Object -Property testName
$failedResults = $allResults | Where-Object { $_.outcome -ne "Passed" } | Sort-Object -Property testName

# Build the HTML
$htmlOutput = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Test Execution Report</title>
    <style>
        body { font-family: sans-serif; margin: 40px; background-color: #f8f9fa; }
        .summary-card { background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); margin-bottom: 20px; }
        .failure-card { background: #fff; border-left: 5px solid #dc3545; padding: 15px; margin-bottom: 15px; }
        .success-card { background: #fff; border-left: 5px solid #28a745; padding: 12px; margin-bottom: 10px; }
        summary { cursor: pointer; padding: 10px; font-weight: bold; }
    </style>
</head>
<body>
    <h1>Test Execution Report</h1>
    <div class="summary-card">
        <div><strong>Total Tests:</strong> $($allResults.Count)</div>
        <div><strong>Passed:</strong> $($passedResults.Count)</div>
        <div><strong>Failed:</strong> $($failedResults.Count)</div>
    </div>
"@

# Append Failed Tests (Sorted)
$htmlOutput += '<details open><summary style="color: #b02a37;">[X] Failed Tests</summary>'
foreach ($result in $failedResults) {
    $htmlOutput += "<div class='failure-card'><strong>[FAIL] $($result.testName)</strong></div>"
}
$htmlOutput += '</details>'

# Append Passed Tests (Sorted)
$htmlOutput += '<details><summary style="color: #2b8a3e;">[OK] Passed Tests</summary>'
foreach ($result in $passedResults) {
    $htmlOutput += "<div class='success-card'><strong>[PASS] $($result.testName)</strong></div>"
}
$htmlOutput += '</details></body></html>'

# Save to file

$htmlOutput | Out-File "MileStone 7 TestReport.html"