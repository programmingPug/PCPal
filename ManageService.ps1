# Define the service name and path to the published service
$ServiceName = "PCPalService"
$ServiceExePath = "$PSScriptRoot\PCPalService\bin\Release\net8.0\publish\win-x64\PCPalService.exe"

# Function to check if the service exists
function ServiceExists {
    return Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
}

function Install-Service {
    if (ServiceExists) {
        Write-Host "Service '$ServiceName' already exists."
        return
    }
    Write-Host "Installing service '$ServiceName'..."
    sc.exe create $ServiceName binPath= "`"$ServiceExePath`"" start= auto obj= "LocalSystem"
    sc.exe failure $ServiceName reset= 0 actions= restart/5000
    sc.exe description $ServiceName "PCPal Background Monitoring Service"
    Start-Service -Name $ServiceName
    Write-Host "Service installed and started successfully with auto-restart enabled."
}

# Start the service
function Start-Service {
    if (ServiceExists) {
        Write-Host "Starting service '$ServiceName'..."
        sc.exe start $ServiceName
        Write-Host "Service started successfully."
    } else {
        Write-Host "Service '$ServiceName' is not installed."
    }
}

# Stop the service
function Stop-Service {
    if (ServiceExists) {
        Write-Host "Stopping service '$ServiceName'..."
        sc.exe stop $ServiceName
        Write-Host "Service stopped successfully."
    } else {
        Write-Host "Service '$ServiceName' is not installed."
    }
}

# Uninstall the service
function Uninstall-Service {
    if (ServiceExists) {
        Stop-Service
        Write-Host "Uninstalling service '$ServiceName'..."
        sc.exe delete $ServiceName
        Write-Host "Service uninstalled successfully."
    } else {
        Write-Host "Service '$ServiceName' is not installed."
    }
}

# Menu for user selection
Write-Host "Choose an option:"
Write-Host "1) Install Service"
Write-Host "2) Start Service"
Write-Host "3) Stop Service"
Write-Host "4) Uninstall Service"
Write-Host "5) Exit"

$choice = Read-Host "Enter your choice (1-5)"

switch ($choice) {
    "1" { Install-Service }
    "2" { Start-Service }
    "3" { Stop-Service }
    "4" { Uninstall-Service }
    "5" { Write-Host "Exiting script..." }
    default { Write-Host "Invalid choice. Please select a valid option." }
}
