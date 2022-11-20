tables: unicodeData caseFolding maketables
	maketables --tables=all --url=https://www.unicode.org/Public/15.0.0/ucd/ > Tables.cs

unicodeData:
	curl -O https://www.unicode.org/Public/15.0.0/ucd/UnicodeData.txt

caseFolding:
	curl -O https://www.unicode.org/Public/15.0.0/ucd/CaseFolding.txt

maketables: maketables.go
	go build maketables.go
