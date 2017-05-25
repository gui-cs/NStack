Tables.cs: UnicodeData.txt maketables.go UnicodeData.txt Makefile maketables
	./maketables --tables=all > Tables.cs

UnicodeData.txt:
	curl -O http://www.unicode.org/Public/9.0.0/ucd/UnicodeData.txt

CaseFolding.txt:
	curl -O http://www.unicode.org/Public/9.0.0/ucd/CaseFolding.tx

maketables: maketables.go
	go build maketables.go
