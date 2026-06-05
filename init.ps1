$repo_name = Get-Item -Path . | Select-Object -ExpandProperty Name

$sln_text = Get-Content -Path "ASPVue.sln"
$sln_text = $sln_text -replace "ASPVue", $repo_name
Set-Content -Path "ASPVue.sln" -Value $sln_text

$http_text = Get-Content -Path "ASPVue.http"
$http_text = $http_text -replace "ASPVue", $repo_name
Set-Content -Path "ASPVue.http" -Value $http_text

Rename-Item -Path "./ASPVue.sln" -NewName $repo_name".sln"
Rename-Item -Path "./ASPVue.http" -NewName $repo_name".http"
Rename-Item -Path "./ASPVue.csproj" -NewName $repo_name".csproj"