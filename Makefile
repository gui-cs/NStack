rebuild-docs: docs/template
	mdoc export-html --force-update -o docs --template=docs/template ecmadocs/en/

# Used to fetch XML doc updates from the C# compiler into the ECMA docs
doc-update:
	mdoc update -i NStack/bin/Debug/NStack.xml -o ecmadocs/en NStack/bin/Debug/NStack.dll 

