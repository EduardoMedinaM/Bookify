namespace Bookify.Infrastructure.Outbox;

public class OutboxOptions
{
    public int IntervalInSeconds { get; init; } 
    public int BatchSize { get; init; }
}
