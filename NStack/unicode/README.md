To regenerate the Tables.cs file from the reference unicode files, execute the Makefile
which will dump the updated tables.

There is some work to be done to dump the tables as a binary blob,
without going through the various data structures that we have now, it would 
avoid all these constructors triggered on the static class.