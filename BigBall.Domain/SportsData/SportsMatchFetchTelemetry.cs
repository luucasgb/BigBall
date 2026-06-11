namespace BigBall.Domain.SportsData;

/// <summary>Mutable bag for outbound HTTP counting when syncing from a provider (<see cref="ISportsDataSource"/>).</summary>
public sealed class SportsMatchFetchTelemetry
{
    public int HttpGetCount { get; private set; }

    public void AddHttpGets(int count)
    {
        if (count > 0)
            HttpGetCount += count;
    }
}
