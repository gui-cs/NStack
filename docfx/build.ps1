# Builds the NStack API documentation using docfx

dotnet clean --configuration Release ../NStack.sln
dotnet build --configuration Release ../NStack.sln

rm ../docs -Recurse -Force


$env:DOCFX_SOURCE_BRANCH_NAME="main"

docfx --metadata
docfx --serve --force