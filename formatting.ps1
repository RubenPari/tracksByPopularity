# Get the directory where the script is located
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Format all C# files in the src directory
dotnet csharpier format "$scriptDir\src"
