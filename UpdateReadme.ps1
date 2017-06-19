# Little trick to make vscode-omnisharp work with .csx file (script files can't have namespaces but ps needs them) and make powershell happy
$Source = Get-Content "$PSScriptRoot\TodayILearned.csx" -Raw
Add-Type -TypeDefinition $Source

[ReadmeGenerator]::Generate($(Convert-Path .))
