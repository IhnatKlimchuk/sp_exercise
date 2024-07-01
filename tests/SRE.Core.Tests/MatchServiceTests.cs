namespace SRE.Core.Tests
{
    public class MatchServiceTests
    {
        private IMatchService _matchService;

        public MatchServiceTests()
        {
            // Arrange
        }

        [Fact]
        public void MatchService_StartNewMatch_Success()
        {
            var match = _matchService.StartNewMatch("Uruguay", "Italy");

            Assert.NotNull(match);
            Assert.Equal("Uruguay", match.HomeTeam);
            Assert.Equal("Italy", match.AwayTeam);
            Assert.Equal(MatchStatus.InProgress, match.Status);
            Assert.Equal(0, match.HomeScore);
            Assert.Equal(0, match.AwayScore);
        }

        [Fact]
        public void MatchService_GetMatch_WhenMatchExists_Success()
        {
            var createdMatch = _matchService.StartNewMatch("Uruguay", "Italy");
            var foundMatch = _matchService.GetMatch(createdMatch.MatchId);

            Assert.NotNull(foundMatch);
            Assert.Equal(createdMatch, foundMatch);
        }

        [Fact]
        public void MatchService_GetMatch_WhenMatchDontExist_Throws()
        {
            Assert.Throws<ArgumentException>(() => _matchService.GetMatch(42));
        }

        [Fact]
        public void MatchService_UpdateScore_Success()
        {
            var createdMatch = _matchService.StartNewMatch("Uruguay", "Italy");
            var updatedMatch = _matchService.UpdateScore(createdMatch.MatchId, createdMatch.HomeTeam, 1);

            Assert.Equal(1, updatedMatch.HomeScore);
            Assert.Equal(0, updatedMatch.AwayScore);

            updatedMatch = _matchService.UpdateScore(createdMatch.MatchId, createdMatch.AwayTeam, 1);

            Assert.Equal(1, updatedMatch.HomeScore);
            Assert.Equal(1, updatedMatch.AwayScore);
        }

        [Fact]
        public void MatchService_UpdateScore_NegativeScore_Throws()
        {
            var createdMatch = _matchService.StartNewMatch("Uruguay", "Italy");

            Assert.Throws<ArgumentException>(() => _matchService.UpdateScore(createdMatch.MatchId, createdMatch.HomeTeam, -1));
        }

        [Fact]
        public void MatchService_UpdateScore_InvalidTeam_Throws()
        {
            var createdMatch = _matchService.StartNewMatch("Uruguay", "Italy");

            Assert.Throws<ArgumentException>(() => _matchService.UpdateScore(createdMatch.MatchId, "Pizza", 1));
        }

        [Fact]
        public void MatchService_UpdateScore_WhenMatchDontExist_Throws()
        {
            Assert.Throws<ArgumentException>(() => _matchService.UpdateScore(42, "Uruguay", 1));
        }

        [Fact]
        public void MatchService_UpdateScore_WhenMatchIsCompleted_Throws()
        {
            var createdMatch = _matchService.StartNewMatch("Uruguay", "Italy");
            _matchService.CompleteMatch(createdMatch.MatchId);

            Assert.Throws<ArgumentException>(() => _matchService.UpdateScore(createdMatch.MatchId, createdMatch.HomeTeam, 1));
            Assert.Throws<ArgumentException>(() => _matchService.UpdateScore(createdMatch.MatchId, createdMatch.AwayTeam, 1));
        }

        [Fact]
        public void MatchService_CompleteMatch_Success()
        {
            var createdMatch = _matchService.StartNewMatch("Uruguay", "Italy");
            var completedMatch = _matchService.CompleteMatch(createdMatch.MatchId);

            Assert.Equal(MatchStatus.Completed, completedMatch.Status);
            Assert.NotNull(completedMatch.EndTime);
        }

        [Fact]
        public void MatchService_CompleteMatch_WhenMatchDontExist_Throws()
        {
            Assert.Throws<ArgumentException>(() => _matchService.CompleteMatch(42));
        }

        [Fact]
        public void MatchService_CompleteMatch_WhenMatchIsAlreadyCompleted_Throws()
        {
            var createdMatch = _matchService.StartNewMatch("Uruguay", "Italy");
            _matchService.CompleteMatch(createdMatch.MatchId);

            Assert.Throws<ArgumentException>(() => _matchService.CompleteMatch(createdMatch.MatchId));
        }

        [Fact]
        public void MatchService_GetMatches_Success()
        {
            var match1 = _matchService.StartNewMatch("Uruguay", "Italy");
            var match2 = _matchService.StartNewMatch("Brazil", "Argentina");

            var matches = _matchService.GetScoreBoard();

            Assert.Equal(2, matches.Count);
            Assert.Contains(match1, matches);
            Assert.Contains(match2, matches);
        }

        [Fact]
        public void MatchService_GetMatches_WhenNoMatches_Success()
        {
            var matches = _matchService.GetScoreBoard();

            Assert.Empty(matches);
        }

        [Fact]
        public void MatchService_GetMatches_WhenMatchesAreCompleted_Success()
        {
            var match1 = _matchService.StartNewMatch("Uruguay", "Italy");
            var match2 = _matchService.StartNewMatch("Brazil", "Argentina");

            _matchService.CompleteMatch(match1.MatchId);
            _matchService.CompleteMatch(match2.MatchId);

            var matches = _matchService.GetScoreBoard();

            Assert.Empty(matches);
        }

        [Fact]
        public void MatchService_GetMatches_WhenSomeMatchesAreCompleted_Success()
        {
            var match1 = _matchService.StartNewMatch("Uruguay", "Italy");
            var match2 = _matchService.StartNewMatch("Brazil", "Argentina");

            _matchService.CompleteMatch(match1.MatchId);

            var matches = _matchService.GetScoreBoard();

            Assert.Single(matches);
            Assert.Contains(match2, matches);
        }

        [Fact]
        public void MatchService_GetMatches_CorrectOrder()
        {
            Match SetupMatch(string homeTeam, string awayTeam, int homeScore, int awayScore)
            {
                var match = _matchService.StartNewMatch(homeTeam, awayTeam);
                match = _matchService.UpdateScore(match.MatchId, match.HomeTeam, homeScore);
                match = _matchService.UpdateScore(match.MatchId, match.AwayTeam, awayScore);
                return match;
            }

            var match1 = SetupMatch("Mexico", "Canada", 0, 5);
            var match2 = SetupMatch("Spain", "Brazil", 10, 2);
            var match3 = SetupMatch("Germany", "France", 2, 2);
            var match4 = SetupMatch("Uruguay", "Italy", 6, 6);
            var match5 = SetupMatch("Argentina", "Australia", 3, 1);

            var matches = _matchService.GetScoreBoard();

            Assert.Equal(5, matches.Count);
            Assert.Collection(matches,
                m => Assert.Equal(match4, m),
                m => Assert.Equal(match2, m),
                m => Assert.Equal(match1, m),
                m => Assert.Equal(match5, m),
                m => Assert.Equal(match3, m));
        }
    }
}
