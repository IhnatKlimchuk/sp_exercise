namespace SRE.Core
{
    public record Match
    {
        public int MatchId { get; init; }
        public required string HomeTeam { get; init; }
        public required string AwayTeam { get; init; }
        public int HomeScore { get; init; }
        public int AwayScore { get; init; }
        public MatchStatus Status { get; init; }
        public DateTimeOffset StartTime { get; init; }
        public DateTimeOffset? EndTime { get; init; }
    }
}
