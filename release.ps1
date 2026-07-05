# Release gate script - guidelines/TESTING.md section 5 steps 1 + 4, in order, fail-fast.
# Run from the repo root. Steps 2-3 (full sandbox suite) and 5 (git tag + NuGet upload)
# remain manual and yours.
#
#   .\release.ps1            # verify + pack into .\artifacts
#   .\release.ps1 -SkipPack  # verify only

param(
    [switch]$SkipPack
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot

function Step($name, [scriptblock]$action) {
    Write-Host "==> $name" -ForegroundColor Cyan
    & $action
    if ($LASTEXITCODE -ne 0) { Write-Host "FAILED: $name" -ForegroundColor Red; exit 1 }
}

Step 'Build solution (Release)' {
    dotnet build "$root\OpenAkeneo.RestApiClient.sln" -c Release -p:GeneratePackageOnBuild=false -v q --nologo
}

Step 'Unit suite (all target frameworks)' {
    dotnet test "$root\OpenAkeneo.RestApiClient.UnitTests" -c Release --no-build -v q --nologo
}

Step 'Regenerate LLM docs from the compiled surface' {
    dotnet run --project "$root\tools\DocGen" -c Release --no-build -- $root
}

Step 'Docs staleness check (generated output must be committed)' {
    git -C $root diff --exit-code -- llms.txt docs/api
    if ($LASTEXITCODE -ne 0) {
        Write-Host 'llms.txt / docs/api changed after regeneration - the committed docs were stale.' -ForegroundColor Yellow
        Write-Host 'Review the diff, commit it, and run release.ps1 again.' -ForegroundColor Yellow
    }
}

if (-not $SkipPack) {
    Step 'Pack' {
        dotnet pack "$root\OpenAkeneo.RestApiClient\OpenAkeneo.RestApiClient.csproj" -c Release --no-build -o "$root\artifacts" -v q --nologo
    }
    Write-Host "==> Package in $root\artifacts - remaining manual steps: full sandbox suite, git tag, NuGet upload." -ForegroundColor Green
}
else {
    Write-Host '==> Verification passed (pack skipped).' -ForegroundColor Green
}
