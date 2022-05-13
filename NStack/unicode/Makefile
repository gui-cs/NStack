Tables.cs: UnicodeData.txt maketables.go CaseFolding.txt Makefile maketables
	./maketables --tables=all > Tables.cs

UnicodeData.txt:
	curl -O http://www.unicode.org/Public/14.0.0/ucd/UnicodeData.txt

CaseFolding.txt:
	curl -O http://www.unicode.org/Public/14.0.0/ucd/CaseFolding.txt

maketables: maketables.go
	go build maketables.go
