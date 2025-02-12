namespace Scoreboard.Models
{
    public class Game
    {
        public int _team1Score { get; set; }
        public int _team2Score { get; set; }
        public string _team1 { get; set; }
        public string _team2 { get; set; }
        public bool display_clock { get; set; }
        public string _gameLevel { get; set; }
        public double _quarterTime { get; set; }
        public int _quarter { get; set; }
        public int _team1TimeOuts { get; set; }
        public int _team2TimeOuts { get; set; }
        public int _homeTeamFouls { get; set; }
        public int _awayTeamFouls { get; set; }
    }
}


