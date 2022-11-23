ifeq ($(OS),Windows_NT)
    EXEC := maketables --tables=all --url=https://www.unicode.org/Public/15.0.0/ucd/ > Tables.cs
else
    EXEC := ./maketables --tables=all --url=https://www.unicode.org/Public/15.0.0/ucd/ > Tables.cs
endif

tables: unicodeData caseFolding maketables
	$(EXEC)

unicodeData:
	curl -O https://www.unicode.org/Public/15.0.0/ucd/UnicodeData.txt

caseFolding:
	curl -O https://www.unicode.org/Public/15.0.0/ucd/CaseFolding.txt

maketables: maketables.go
	go build maketables.go
