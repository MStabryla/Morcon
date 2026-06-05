# Cleaning previous output
if (Test-Path -Path ./dist/)
{
    Remove-Item -Recurse -Force ./dist/
}
if (Test-Path -Path ./wwwroot/)
{
    Remove-Item -Recurse -Force ./wwwroot/*
}
else
{
    New-Item -ItemType Directory -Path ./wwwroot/
}
# Compiling the application to dist
dotnet publish -c Release --property:OutputPath=./dist/
# Moving .NET publish output to wwwroot
Copy-Item -Recurse -Force ./dist/publish/* ./wwwroot/
# Compiling Vue.js app
Set-Location Client
npm run --silent build
# Moving Vue.js app to wwwroot
Copy-Item -Recurse -Force dist/* ../wwwroot/Client/
Copy-Item -Recurse -Force src/assets ../wwwroot/Client
# Cleaning up
Set-Location ..
Remove-Item -Recurse -Force ./dist/