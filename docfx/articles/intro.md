
NStack contains a new API for .NET based on modern C# and .NET idioms,
the long term plan is to use a new model for IO that only uses
exceptions for things like invalid parameters, but uses tuples and
error codes for the rest.

Other areas include making an IO layer that does not surface "string"
for filenames, as in Unix there are really no filenames as we treat
them in .NET, but rather file names are a collection of bytes, which
do not necessarily can be decoded into UTF8 [1].

To make things simple, this assumes that UTF8 strings (ustring in this
code) can exist without them being valid UTF8 strings, but rather a
collection of bytes.

[1] For example, older file systems can have filenames that made sense with
a particular character set and are effectively not possible to map into strings.
