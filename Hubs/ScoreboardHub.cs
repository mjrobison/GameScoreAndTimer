using Microsoft.AspNetCore.SignalR;
using Scoreboard.Services;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Scoreboard.Models;

namespace Scoreboard.Hubs
{
    public class ScoreboardHub : Hub
    {
            private static readonly Stopwatch _stopwatch = new();
            private static int _quarterTime = 360;
            private static int _halfTime = 900;
            private static int _timeouts = 3;
            private static string _team1 = "Team A";
            private static string _team2 = "Team B";
            private static int _scoreTeam1 = 0;
            private static int _scoreTeam2 = 0;
            // private readonly IHttpClientFactory _httpClientFactory;
            // private CancellationTokenSource _cts = new();
            private readonly ScoreboardService _scoreboardService;
            // private static string apiUrl = "https://api.mhacsports.com/";
            

            public ScoreboardHub(ScoreboardService scoreboardService)
            {
                _scoreboardService = scoreboardService;
            }

            public override async Task OnConnectedAsync()
            {
                await Clients.Caller.SendAsync("UpdateGameState", _stopwatch.ElapsedMilliseconds / 1000.0, _quarterTime, _halfTime, _timeouts, _team1, _team2, _scoreTeam1, _scoreTeam2);
                
                // await Clients.Caller.SendAsync("UpdateShotLog", GetShotHistory());
                await base.OnConnectedAsync();
            }
            public async Task<object> GetGameState()
            {
                return _scoreboardService.GetCurrentState();
            }
            
            public Task StartClock()
            {
                return _scoreboardService.StartClock();
            }

            public Task StopClock()
            {
                return _scoreboardService.StopClock();
            }
            
            public Task incrementHome(int value)
            {
                return _scoreboardService.incrementHome(value);
            }
            
            public Task incrementAway(int value)
            {
                return _scoreboardService.IncrementAwayScore(value);
            }
            public Task decrementHomeTimeouts(int value)
            {
                return _scoreboardService.DecrementHomeTimeouts(value);
            }
            public Task decrementAwayTimeouts(int value)
            {
                return _scoreboardService.DecrementAwayTimeouts(value);
            }
            public Task incrementHomeFouls(int value)
            {
                return _scoreboardService.IncrementHomeFouls(value);
            }
            public Task incrementAwayFouls(int value)
            {
                return _scoreboardService.IncrementAwayFouls(value);
            }
            public Task decrementHomeFouls(int value)
            {
                return _scoreboardService.DecrementHomeFouls(value);
            }
            public Task decrementAwayFouls(int value)
            {
                return _scoreboardService.DecrementAwayFouls(value);
            }
            public Task setPossession(string value)
            {
                return _scoreboardService.SetPossession(value);
            }
            public Task incrementPeriod(int value)
            {
                return _scoreboardService.IncrementPeriod(value);
            }
            public Task decrementPeriod(int value)
            {
                return _scoreboardService.DecrementPeriod(value);
            }
            public Task SetHomeTeam(string team)
            {
                Console.WriteLine(team);
                return _scoreboardService.SetHomeTeam(team);
                
            }
            public Task SetAwayTeam(string team)
            {
                Console.WriteLine(team);
                return _scoreboardService.SetAwayTeam(team);
            }

            public Task SetGameconfig(string Level)
            {
                Console.WriteLine(Level);
                return _scoreboardService.setGameConfig(Level);
            }
            
            public Task SetTime(object time)
            {
                var timeJson = JsonSerializer.Deserialize<TimeObject>(time.ToString());
                Console.WriteLine(timeJson);
                return _scoreboardService.setTime(timeJson);

            }
           
            // public async Task LoadTeams(string apiUrl)
            // {
            //     var client = _httpClientFactory.CreateClient();
            //     var response = await client.GetStringAsync(apiUrl);
            //     var teams = JsonSerializer.Deserialize<string[]>(response);
            //     if (teams?.Length == 2)
            //     {
            //         _team1 = teams[0];
            //         _team2 = teams[1];
            //         SaveGameState();
            //         await Clients.All.SendAsync("UpdateTeams", _team1, _team2);
            //     }
            // }

            public Task toggleClockDisplay(object value)
            {
                Console.WriteLine(value);
                return _scoreboardService.ToggleClockDisplay(value);
            }

            private List<ScoringLogEntry> GetShotHistory()
            {
                var shotHistory = new List<ScoringLogEntry>();
                using var connection = new SqliteConnection("Data Source=game_state.db");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT GameTime, Quarter, Team, Player, ShotType, Points FROM ScoringLog ORDER BY GameTime";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    shotHistory.Add(new ScoringLogEntry
                    {
                        GameTime = reader.GetInt32(0),
                        Quarter = reader.GetInt32(1),
                        Team = reader.GetString(2),
                        Player = reader.GetString(3),
                        ShotType = reader.GetString(4),
                        Points = reader.GetInt32(5)
                    });
                }
                return shotHistory;
            }
            
            public async Task RecordScore(int quarter, string team, string player, string shotType, int points)
            {
                int gameTime = (int)_stopwatch.Elapsed.TotalSeconds; // Capture game time
                using var connection = new SqliteConnection("Data Source=game_state.db");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO ScoringLog (GameTime, Quarter, Team, Player, ShotType, Points) VALUES (@gameTime, @quarter, @team, @player, @shotType, @points)";
                command.Parameters.AddWithValue("@gameTime", gameTime);
                command.Parameters.AddWithValue("@quarter", quarter);
                command.Parameters.AddWithValue("@team", team);
                command.Parameters.AddWithValue("@player", player);
                command.Parameters.AddWithValue("@shotType", shotType);
                command.Parameters.AddWithValue("@points", points);
                command.ExecuteNonQuery();

                // Update running score
                if (team == _team1)
                    _scoreTeam1 += points;
                else
                    _scoreTeam2 += points;

                var updateScoreCmd = connection.CreateCommand();
                updateScoreCmd.CommandText = "UPDATE GameState SET ScoreTeam1 = @score1, ScoreTeam2 = @score2 WHERE Id = 1";
                updateScoreCmd.Parameters.AddWithValue("@score1", _scoreTeam1);
                updateScoreCmd.Parameters.AddWithValue("@score2", _scoreTeam2);
                updateScoreCmd.ExecuteNonQuery();

                await Clients.All.SendAsync("UpdateScore", _scoreTeam1, _scoreTeam2);
            }

        }
}
