param([string]$Output)
$repoRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if(-not $Output){$Output=Join-Path $repoRoot ('artifacts\portable-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))}
if(Test-Path -LiteralPath $Output){throw 'Escolha uma pasta de saida nova.'}
$raw=Join-Path $repoRoot ('artifacts\build-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
$project=Join-Path $repoRoot 'source_remake\RisingLandsRemake\RisingLandsRemake.csproj'
dotnet publish $project -c Release -r win-x64 --self-contained true -p:AssemblyName=Rising -p:PortableDistribution=true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -o $raw
if($LASTEXITCODE -ne 0){throw 'Compilacao falhou.'}
New-Item -ItemType Directory -Path $Output -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $raw 'Rising.exe') -Destination $Output
Copy-Item -LiteralPath (Join-Path $repoRoot 'Recursos') -Destination $Output -Recurse
Copy-Item -LiteralPath (Join-Path $repoRoot 'tools\Native') -Destination (Join-Path $Output 'Dependencias') -Recurse
$resources=Join-Path $Output 'Recursos'
Get-ChildItem -LiteralPath (Join-Path $raw 'Content') -Recurse -File | ForEach-Object {
 $relative=$_.FullName.Substring((Join-Path $raw 'Content').Length+1).Replace('\','/')
 if($relative.StartsWith('Data/Audio/')){$dest='Sons/'+$relative.Substring(11)}
 elseif($relative.StartsWith('Data/Scenes/')){$dest='Mapas/Cenas/'+$relative.Substring(12)}
 elseif($relative.StartsWith('Data/') -and $relative.EndsWith('.wav')){$dest='Sons/'+$relative.Substring(5)}
 elseif($relative.StartsWith('Data/')){$dest='Dados/'+$relative.Substring(5)}
 elseif($relative.StartsWith('ResearchAnimation') -or $relative.StartsWith('ResearchCompletion')){$dest='Animacoes/'+$relative}
 elseif($relative.EndsWith('.png')){$dest='Graficos/Interface/'+$relative}
 else{$dest='Dados/Fontes/'+$relative}
 $fileTarget=Join-Path $resources $dest
 New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($fileTarget)) -Force | Out-Null
 Copy-Item -LiteralPath $_.FullName -Destination $fileTarget
}
'UserSaves','UserSettings' | ForEach-Object {New-Item -ItemType Directory -Path (Join-Path $Output $_) -Force | Out-Null}
Write-Host "Jogo portatil: $Output\Rising.exe"
