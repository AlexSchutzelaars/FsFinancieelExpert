# Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

# Huidige script-locatie ophalen
$currentDir = Split-Path -Leaf (Get-Location)

# Pad naar bronbestand (xxx.exe in map Y)
$sourceFile = "C:\Program Files (x86)\WIN-PROLOG 8100\PRO386W.exe"

# Doelbestand: Z.exe en Z.ovl in huidige map
$destinationFile = Join-Path (Get-Location) "$currentDir.exe"


# Bestand kopiëren en hernoemen
Copy-Item -Path $sourceFile -Destination $destinationFile

# Dummy-bestand aanmaken (leeg bestand)
# Doelpad voor het dummy-bestand
$dummyFile = Join-Path (Get-Location) "$currentDir.ovl"

New-Item -Path $dummyFile -ItemType File -Force

# Zoek naar een .pl-bestand in de huidige map.

$plFile = Get-ChildItem -Path (Get-Location) -Filter *.pl | Select-Object -First 1

# Als er zo'n .pl-bestand is, hernoem het.

if ($plFile) {
    $newPlName = Join-Path (Get-Location) "$currentDir.pl"
    Rename-Item -Path $plFile.FullName -NewName $newPlName
}

