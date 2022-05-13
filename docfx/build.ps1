# Builds the NStack API documentation using docfx

dotnet clean --configuration Release ../NStack.sln
dotnet build --configuration Release ../NStack.sln

rm ../docs -Recurse -Force

docfx --metadata
docfx --serve
