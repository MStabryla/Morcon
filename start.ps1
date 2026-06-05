if(-not (Test-Path -Path ./wwwroot/))
{
    Write-Host "wwwroot folder not found. Please run build.ps1 first."
    Exit
}
$executable = Get-ChildItem -Filter ./wwwroot/*.exe
if($null -ne $executable)
{
    # Handle Control + C to stop the process gracefully
    $process = Start-Process -NoNewWindow -WorkingDirectory ./wwwroot/ -FilePath "./wwwroot/$executable" -PassThru
    try
    {
        $process | Wait-Process
    }
    finally
    {
        if ($process -and -not $process.HasExited)
        {
            Stop-Process -InputObject $process -Force
        }
    }
}
else
{
    Write-Host "No executable found in wwwroot. Please run build.ps1 first."
}