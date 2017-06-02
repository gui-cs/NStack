
# NStack 

NStack contains a new API for .NET based on modern C# and .NET idioms.

You can start with the new [UTF8 ustring
class](api/NStack/NStack.ustring.html), which is powered by an updated
[Unicode library](api/NStack/NStack.Unicode.html) and modern support for
[UTF8 parsing and decoding](api/NStack/NStack.Utf8.html)

# Future

The long term plan is to use a new model for IO that only uses
exceptions for things like invalid parameters, but uses tuples and
error codes for the rest.

Other areas include making an IO layer that does not surface "string"
for filenames, as in Unix there are really no filenames as we treat
them in .NET, but rather file names are a collection of bytes, which
do not necessarily can be decoded into UTF8 [1].

To make things simple, this assumes that UTF8 strings (ustring in this
code) can exist without them being valid UTF8 strings, but rather a
collection of bytes.


