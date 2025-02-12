using System.Text.Json.Serialization;

namespace Scoreboard.Models
{
    public class TimeObject
    {
        [JsonPropertyName("minutes")]
        public int Minutes { get; set; }

        [JsonPropertyName("seconds")]
        public int Seconds { get; set; }

        [JsonPropertyName("tenth_seconds")]
        public int Tenths { get; set; }
    }
}


