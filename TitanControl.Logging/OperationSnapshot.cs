using System;

namespace TitanControl.Logging;

public sealed record OperationSnapshot(
    Guid Id,
    Guid? ParentId,
    string Name,
    string? Category,
    DateTimeOffset StartedAt,
    int StartedOnThreadId);
