$filePath = "**\*.csproj"
# Get the files from the folder and iterate using Foreach
Get-ChildItem $filePath -Recurse | ForEach-Object {
# Read the file and use replace()
(Get-Content $_).Replace('net6.0','net7.0') | Set-Content $_
}