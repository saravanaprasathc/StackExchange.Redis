using System;
using System.Diagnostics;

namespace StackExchange.Redis
{
    internal static class ResultTypeExtensions
    {
        public static bool IsError(this ResultType value)
            => value == ResultType.Error | value == ResultType.BlobError;

        public static ResultType ToResp2(this ResultType value)
        {
            switch (value)
            {
                case ResultType.BulkString:
                case ResultType.Error:
                case ResultType.Integer:
                case ResultType.MultiBulk:
                case ResultType.None:
                case ResultType.SimpleString:
                    return value;
                case ResultType.VerbatimString:
                    return ResultType.BulkString;
                case ResultType.Double:
                case ResultType.Boolean:
                case ResultType.BigInteger:
                    return ResultType.SimpleString;
                case ResultType.BlobError:
                    return ResultType.Error;
                case ResultType.Map:
                case ResultType.Set:
                case ResultType.Push:
                case ResultType.Hello:
                case ResultType.Attribute:
                    return ResultType.MultiBulk;

                // include Null here because we can't unambiguously reconstruct this; there are two
                // nulls in RESP2, and one (or three, depending on how you look at it) in RESP3
                case ResultType.Null:
                default:
                    Debug.Assert(false, $"unexpected result-type value in {nameof(ToResp2)}: {value}");
                    return value;
            }
        }
    }
}
