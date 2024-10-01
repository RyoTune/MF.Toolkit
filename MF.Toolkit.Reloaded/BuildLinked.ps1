# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/MF.Toolkit.Reloaded/*" -Force -Recurse
dotnet publish "./MF.Toolkit.Reloaded.csproj" -c Release -o "$env:RELOADEDIIMODS/MF.Toolkit.Reloaded" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location