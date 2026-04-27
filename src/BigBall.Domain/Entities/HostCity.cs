namespace BigBall.Domain.Entities;

public sealed class HostCity
{
    public int Id { get; set; }
    public required string CityName { get; set; }
    public required string Country { get; set; }
    public required string VenueName { get; set; }
    public required string RegionCluster { get; set; }
    public required string AirportCode { get; set; }

    public ICollection<Match> Matches { get; } = new List<Match>();
}
