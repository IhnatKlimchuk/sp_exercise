using SRE.Core;

namespace SRE
{
    public interface IMatchService
    {
        public Match StartNewMatch(string homeTeam, string awayTeam);
        public Match GetMatch(int matchId);
        public Match UpdateScore(int matchId, string team, int score);
        public void DeleteMatch(int matchId);
        public Match CompleteMatch(int matchId);
        public IReadOnlyCollection<Match> GetScoreBoard(int limit = 5);
    }
}
