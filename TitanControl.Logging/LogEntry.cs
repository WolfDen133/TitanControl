using System;
using System.Collections.Generic;

namespace TitanControl.Logging;

public sealed record LogEntry(
    long Sequence,
    DateTimeOffset Timestamp,
    LogLevel Level,
    string Message,
    string? Category,
    Exception? Exception,
    int ManagedThreadId,
    Guid? OperationId,
    Guid? ParentOperationId,
    IReadOnlyDictionary<string, object?>? Properties);
