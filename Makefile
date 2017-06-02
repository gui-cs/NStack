all: doc-update rebuild-docs

rebuild-docs: docs/template
	mdoc export-html --force-update -o docs --template=docs/template ecmadocs/en/

# Used to fetch XML doc updates from the C# compiler into the ECMA docs
doc-update:
	mdoc update -i NStack/bin/Debug/NStack.xml -o ecmadocs/en NStack/bin/Debug/NStack.dll 

ecma2yaml: 
	curl -O https://api.nuget.org/packages/microsoft.docascode.ecma2yaml.1.0.105.nupkg
	unzip -d ecma2yaml microsoft.docascode.ecma2yaml.1.0.105.nupkg
	(cd ecma2yaml; nuget restore; msbuild)
