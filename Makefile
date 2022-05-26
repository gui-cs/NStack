all: dotnet-build doc-update yaml

dotnet-build:
	dotnet-gitversion /updateprojectfiles
	msbuild NStack.sln /t:clean /p:configuration=Release
	msbuild NStack.sln -t:restore -p:RestorePackagesConfig=true /p:configuration=Release
	msbuild NStack.sln /p:Configuration=Release /p:DocumentationFile="bin/Release/NStack.xml"

# Target for using mdoc and my old doc template
rebuild-docs: odocs/template
	mdoc export-html --force-update -o odocs --template=odocs/template ecmadocs/en/

# Used to fetch XML doc updates from the C# compiler into the ECMA docs
doc-update:
	mdoc update -i NStack/bin/Release/NStack.xml -o ecmadocs/en NStack/bin/Release/netstandard2.0/NStack.dll

yaml:
	-rm /cvs/NStack/ecmadocs/en/ns-.xml
	mono /cvs/ECMA2Yaml/ECMA2Yaml/ECMA2Yaml/bin/Debug/ECMA2Yaml.exe --source=/cvs/NStack/ecmadocs/en --output=/cvs/NStack/docfx/api
	(cd docfx; mono ~/Downloads/docfx/docfx.exe build)
