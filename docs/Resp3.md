# RESP3 and StackExchange.Redis

RESP2 and RESP3 are evolutions of the Redis protocol; the main differences are:

1. RESP3 can carry out-of-band / "push" messages on a single connection, where-as RESP2 requires a separate connection for these messages
2. RESP3 can (when appropriate) convey additional semantic meaning about returned payloads

For most people, the first point is the main reason to consider RESP3, as in high-usage servers, this can halve the number of connections required.
This is particularly useful in hosted environments where the number of inbound connections to the server is capped as part of a service plan.

There are no significant other differences, i.e. security, performance, etc all perform identically under both RESP2 and RESP3.

RESP3 requires a Redis server version 6 or above; since the library cannot automatically know the server version *before* it has successfully connected,
the library currently requires a hint to enable this mode, in particular, configuring the `ConfigurationOptions.Version` property to 6 (or above), or using
`,version=6.0` (or above) on the configuration string.

When using StackExchange.Redis, the second point only applies to commands (in particular: *modules*) that are invoked via the ad-hoc
`Execute[Async](string command, ...)` APIs, which return `RedisResult`. **If you are not using this API, you do not need to do anything.**

Historically, you could use the `RedisResult.Type` property to query the type of data returned (integer, string, etc). In particular:

- two new properties are added: `RedisResult.Resp2Type` and `RedisResult.Resp3Type`
  - the `Resp3Type` property exposes the new semantic data (when using RESP3), for example it can indicate that a value is a double-precision number, a boolean, a map, etc (types that did not historically exist)
  - the `Resp2Type` property exposes the same value that *would* have been returned if this data had been returned over RESP2
  - the `Type` property is now marked obsolete, but functions identically to `Resp2Type`, so that pre-existing code (for example, that has a `switch` on the type) is not impacted by RESP3
- the `ResultType.MultiBulk` is superseded by `ResultType.Array` (this is a nomenclature change only; they are the same value and function identically)

No changes to existing code are *required*, but:

1. to prevent build warnings, replace usage of `ResultType.MultiBulk` with `ResultType.Array`, and usage of `RedisResult.Type` with `RedisResult.Resp2Type`
2. if you wish to explit the additional semantic data when using RESP3 and `Execute[Async](string command, ...)`, use `RedisResult.Resp3Type` where appropriate