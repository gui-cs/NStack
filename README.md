# NStack

An exploration of what the .NET APIs for IO looked like if they only
used exceptions for either invalid parameters being passed to
methods and used results/error codes for most IO operations.

Exceptions have a role, but IO code tends to become ugly in its presence.