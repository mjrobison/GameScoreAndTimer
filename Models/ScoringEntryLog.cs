namespace Scoreboard.Models
{
    public class ScoringLogEntry
    {
        public int GameTime { get; set; }
        public int Quarter { get; set; }
        public string Team { get; set; }
        public string Player { get; set; }
        public string ShotType { get; set; }
        public int Points { get; set; }
    }
}