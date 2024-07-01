using SRE.Core;

namespace SRE
{
    public class MatchService : IMatchService
    {
        /// <summary>
        /// In-memory storage for matches that can replaced with a database or other storage.
        /// </summary>
        private readonly Dictionary<int, Match> _matches = [];
        /// <summary>
        /// TimeProvider is a dependency that provides the current time and used to make the MatchService testable.
        /// </summary>
        private readonly TimeProvider _timeProvider;

        public MatchService(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public MatchService() : this(TimeProvider.System) { }

        public Match CompleteMatch(int matchId)
        {
            if (!_matches.TryGetValue(matchId, out var existingMatch))
            {
                throw new ArgumentException("Match not found", nameof(matchId));
            }

            if (existingMatch.Status == MatchStatus.Completed)
            {
                throw new InvalidOperationException("Match is already completed");
            }

            var completedMatch = existingMatch with
            {
                Status = MatchStatus.Completed,
                EndTime = _timeProvider.GetUtcNow()
            };

            _matches[matchId] = completedMatch;
            return completedMatch;
        }

        public void DeleteMatch(int matchId)
        {
            _matches.Remove(matchId);
        }

        public Match GetMatch(int matchId)
        {
            if (!_matches.TryGetValue(matchId, out var existingMatch))
            {
                throw new ArgumentException("Match not found", nameof(matchId));
            }
            return existingMatch;
        }

        public IReadOnlyCollection<Match> GetScoreBoard(int limit = 5)
        {
            if (limit <= 0)
            {
                throw new ArgumentException("Limit cannot be negative", nameof(limit));
            }

            return _matches.Values
                .Where(m => m.Status == MatchStatus.InProgress)
                .OrderByDescending(m => m.HomeScore + m.AwayScore)
                .ThenByDescending(m => m.StartTime)
                .Take(limit)
                .ToList();
        }

        public Match StartNewMatch(string homeTeam, string awayTeam)
        {
            if (string.IsNullOrWhiteSpace(homeTeam))
            {
                throw new ArgumentException("Home team cannot be null or empty", nameof(homeTeam));
            }

            if (string.IsNullOrWhiteSpace(awayTeam))
            {
                throw new ArgumentException("Away team cannot be null or empty", nameof(awayTeam));
            }

            var match = new Match
            {
                MatchId = _matches.Count + 1,
                HomeTeam = homeTeam,
                AwayTeam = awayTeam,
                StartTime = _timeProvider.GetUtcNow(),
                Status = MatchStatus.InProgress,
                HomeScore = 0,
                AwayScore = 0,
            };
            _matches.Add(match.MatchId, match);
            return match;
        }

        public Match UpdateScore(int matchId, string team, int score)
        {
            if (score < 0)
            {
                throw new ArgumentException("Score cannot be negative", nameof(score));
            }

            if (!_matches.TryGetValue(matchId, out var existingMatch))
            {
                throw new ArgumentException("Match not found", nameof(matchId));
            }

            if (existingMatch.Status == MatchStatus.Completed)
            {
                throw new InvalidOperationException("Match is already completed");
            }

            Match expectedMatch;
            if (existingMatch.HomeTeam == team)
            {
                expectedMatch = existingMatch with { HomeScore = score };
            }
            else if (existingMatch.AwayTeam == team)
            {
                expectedMatch = existingMatch with { AwayScore = score };
            }
            else
            {
                throw new ArgumentException("Team not found", nameof(team));
            }

            _matches[matchId] = expectedMatch;
            return expectedMatch;
        }
    }
}
