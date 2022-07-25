# NStack

[![Build Status](https://travis-ci.org/migueldeicaza/NStack.svg?branch=master)](https://travis-ci.org/migueldeicaza/NStack)

NOTE: Nstack has moved to the guo-cs org: https://github.com/orgs/gui-cs/Currently, this library contains a port of the Go string, and Go rune support as well as other Unicode helper methods.

You can browse the [API documentation](https://migueldeicaza.github.io/NStack/api/NStack.html).

Install the [NuGet package from NuGet.org](https://www.nuget.org/packages/NStack.Core) by installing `NStack.Core`.

# Future Additions

The long term goal is to make this module an exploration of what the .NET APIs for IO looked like if they only
used exceptions for either invalid parameters being passed to
methods and used results/error codes for most IO operations:

* Exceptions have a role, but IO code tends to become ugly in its presence.

* Other areas include making an IO layer that does not surface "string" for
filenames, as in Unix there are really no filenames as we treat them in
.NET, but rather file names are a collection of bytes, which do not necessarily
can be decoded into UTF8 [1].  

To make things simple, this assumes that UTF8 strings (ustring in this code)
can exist without them being valid UTF8 strings, but rather a collection of bytes.

[1] For example, older file systems can have filenames that made sense with
a particular character set and are effectively not possible to map into strings.
