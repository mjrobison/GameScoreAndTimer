using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using Scoreboard.Models;
using Scoreboard.Hubs;
using Scoreboard.Services;

namespace Scoreboard.Services
{
    public class ScoreboardService
    {
        private readonly IHubContext<ScoreboardHub> _hubContext;
        private readonly GameClockService _gameClock;
        private readonly IHttpClientFactory _httpClientFactory;


        private int _team1Score = 0;
        private int _team2Score = 0;
        private static string _team1 = "chattanooga_patriots";
        private static string _team2 = "tennessee_heat";
        private bool display_clock = true;
        private string _gameLevel = "18U";
        private double _quarterTime = 520;
        private int _quarter = 1;
        private int _team1TimeOuts = 5;
        private int _team2TimeOuts = 5;
        private int _homeTeamFouls = 0;
        private int _awayTeamFouls = 0;

        private const string DatabaseFile = "game_state.db";

        public ScoreboardService(IHubContext<ScoreboardHub> hubContext,
                                 GameClockService gameClock,  
                                 IHttpClientFactory httpClientFactory)
        {
            _hubContext = hubContext;
            _gameClock = gameClock;
            _httpClientFactory = httpClientFactory;
            
            LoadGameState();  // Load game state on startup
        }

        public async Task StartClock() => _gameClock.StartClock();
        
        public async Task StopClock() 
        {
            _gameClock.StopClock();
            _quarterTime = _gameClock.GetTime();
            Console.WriteLine(_gameClock.GetTime());
            SaveGameState();
        }  
        
        public double GetRemainingTime() => _gameClock.GetTime();

        public async Task incrementHome(int value) 
        {
            _team1Score += value;
            SaveGameState();
            Console.WriteLine(_team1Score);
            await _hubContext.Clients.All.SendAsync("UpdateHomeScore", value);
        }

        public object GetCurrentState()
        {
            return new
            {
                GameLevel = _gameLevel,
                Quarter = _quarter,
                QuarterTime = _gameClock.FormatTimeObject(_quarterTime),
                homeTeam = _team1,
                awayTeam = _team2,
                homeTeamScore = _team1Score,
                awayTeamScore = _team2Score,
                homeTeamTimeouts = _team1TimeOuts,
                awayTeamTimeouts = _team2TimeOuts,
                homeTeamFouls = _homeTeamFouls,
                awayTeamFouls = _awayTeamFouls
            };
        }

        public object PushGameState()
        {
            return new
            {
                GameLevel = _gameLevel,
                Quarter = _quarter,
                QuarterTime = _gameClock.FormatTimeObject(_quarterTime),
                homeTeam = _team1,
                awayTeam = _team2,
                homeTeamScore = _team1Score,
                awayTeamScore = _team2Score,
                homeTeamTimeouts = _team1TimeOuts,
                awayTeamTimeouts = _team2TimeOuts,
                homeTeamFouls = _homeTeamFouls,
                awayTeamFouls = _awayTeamFouls
            };
        }

        private void LoadGameState()
        {
            // Load saved game state logic
            Console.WriteLine("Game state loaded.");
            using var connection = new SqliteConnection($"Data Source={DatabaseFile};");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT GameLevel, QuarterTime, Team1, Team2, Team1Score, Team2Score, Team1TimeoutsRemaining, Team2TimeoutsRemaining, HomeTeamFouls, AwayTeamFouls FROM GameState WHERE Id = 1;";
            using var reader = command.ExecuteReader();
            if (reader.Read())
            { 
                _gameLevel = reader.GetString(0);
                _quarterTime = reader.GetDouble(1);
                _team1 = reader.GetString(2);
                _team2 = reader.GetString(3);
                _team1Score = reader.GetInt32(4);
                _team2Score = reader.GetInt32(5);
                _team1TimeOuts = reader.GetInt32(6);
                _team2TimeOuts = reader.GetInt32(7);
                _homeTeamFouls = reader.GetInt32(8);
                _awayTeamFouls = reader.GetInt32(9);                
                
                Console.WriteLine("Game state loaded");
            }
            else
            {
                Console.WriteLine("No previous game state found.");
            }
            // _gameClock.InitializeClock(_quarterTime);
        }

        private void SaveGameState()
        {   
            
            Console.WriteLine($"Saving Data: {_gameLevel}, {_team1TimeOuts}");
            using var connection = new SqliteConnection($"Data Source={DatabaseFile}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "INSERT OR REPLACE INTO GameState (Id, GameLevel, Quarter, QuarterTime, Team1, Team2, Team1Score, Team2Score, Team1TimeoutsRemaining, Team2TimeoutsRemaining, HomeTeamFouls, AwayTeamFouls) VALUES (1, @gameLevel, @quarter, @quarterTime, @team1, @team2, @team1Score, @team2Score, @team1Timeouts, @team2Timeouts, @homeTeamFouls, @awayTeamFouls)";
            command.Parameters.AddWithValue("@gameLevel", _gameLevel);
            command.Parameters.AddWithValue("@quarter", _quarter);
            command.Parameters.AddWithValue("@quarterTime",  _quarterTime);
            command.Parameters.AddWithValue("@team1", _team1);
            command.Parameters.AddWithValue("@team2", _team2);
            command.Parameters.AddWithValue("@team1Score", _team1Score);
            command.Parameters.AddWithValue("@team2Score", _team2Score);
            command.Parameters.AddWithValue("@team1Timeouts", _team1TimeOuts);
            command.Parameters.AddWithValue("@team2Timeouts", _team2TimeOuts);
            command.Parameters.AddWithValue("@homeTeamFouls", _homeTeamFouls);
            command.Parameters.AddWithValue("@awayTeamFouls", _awayTeamFouls);
            command.ExecuteNonQuery();
        }
        
        public async Task IncrementAwayScore(int value)
        {
            _team2Score += value;
            SaveGameState();
            Console.WriteLine(_team2Score);
            await _hubContext.Clients.All.SendAsync("UpdateAwayScore", value);
        }

        public async Task DecrementHomeTimeouts(int value)
        {
            if (_team1TimeOuts > 0)
            {
                _team1TimeOuts += value;
                _gameClock.StopClock();
                SaveGameState();
            }
            
            await _hubContext.Clients.All.SendAsync("decrementHomeTimeouts", value);
        }

        public async Task IncrementHomeTimeouts(int value)
        {
            _team1TimeOuts += value;
            SaveGameState();
            await _hubContext.Clients.All.SendAsync("decrementHomeTimeouts", value);
        } 

        public async Task DecrementAwayTimeouts(int value)
        {
            if (_team2TimeOuts > 0)
            {
                Console.WriteLine($"Timeouts Left Away: {value}");
                _team2TimeOuts += value;
                _gameClock.StopClock();
                SaveGameState();
            }
            
            await _hubContext.Clients.All.SendAsync("decrementAwayTimeouts", value);
        }

        public async Task IncrementHomeFouls(int value) 
        {
            _homeTeamFouls += value;
            SaveGameState();
            await _hubContext.Clients.All.SendAsync("UpdateHomeFouls", value);
        }

        public async Task DecrementHomeFouls(int value)
        {
            _homeTeamFouls -= value;
            SaveGameState();
            await _hubContext.Clients.All.SendAsync("UpdateHomeFouls", -value);
        }
        public async Task IncrementAwayFouls(int value)
        {
            _awayTeamFouls += value;
            SaveGameState();
            await _hubContext.Clients.All.SendAsync("UpdateAwayFouls", value);
        }
        public async Task DecrementAwayFouls(int value) 
        {
            _awayTeamFouls += value;
            SaveGameState();
            await _hubContext.Clients.All.SendAsync("UpdateAwayFouls", -value);
        }

        public async Task SetPossession(string team) => await _hubContext.Clients.All.SendAsync("UpdatePossession", team);
        

        public async Task ToggleClockDisplay(object value) 
        {
            display_clock = !display_clock;
            await _hubContext.Clients.All.SendAsync("toggleClock", display_clock);
        }
        
        public async Task SetAwayTeam(string team) 
        {
            _team2 = team;
            SaveGameState();
            await _hubContext.Clients.All.SendAsync("setAwayTeam", team);
        }
        
        public async Task SetHomeTeam(string team)
        {
            _team1 = team;  // Assigns the new team object
            Console.WriteLine(_team1);
            SaveGameState();
            await _hubContext.Clients.All.SendAsync("setHomeTeam", _team1);
        }
        
        public async Task SetAwayScore(int value) 
        {
            _team2Score = value;
            SaveGameState();
            await _hubContext.Clients.All.SendAsync("setAwayScore", _team2Score);
        }
        
        public async Task SetHomeScore(int value)
        {
            _team1Score = value;
            SaveGameState();
            await _hubContext.Clients.All.SendAsync("setHomeScore", _team1Score);
        }


        public async Task setGameConfig(string level)
        {
            Console.WriteLine($"SettingConfig {level}");
        
            using var connection = new SqliteConnection($"Data Source={DatabaseFile};");
            connection.Open();
            _gameLevel = level;
            var command = connection.CreateCommand();
            command.CommandText = "SELECT QuarterTime, TimeOuts FROM GameRules WHERE GameLevel = @level";
            command.Parameters.AddWithValue("@level", level);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                _quarterTime = reader.GetInt32(0);
                _team1TimeOuts = reader.GetInt32(1);
                _team2TimeOuts = reader.GetInt32(1);
    
                Console.WriteLine($"Game Config loaded: Time={_quarterTime}s, TimeOuts={_team1TimeOuts}");
            }
            connection.Close();
            SaveGameState();
            await _hubContext.Clients.All.SendAsync("UpdateGameRules", level);
            
        }

        public async Task setTime(Scoreboard.Models.TimeObject time)
        {
            var seconds = _gameClock.ConvertTimeObjectToSeconds(time);
            _gameClock.InitializeClock(seconds);
            await _hubContext.Clients.All.SendAsync("UpdateGameState", _gameClock.FormatTimeObject(_gameClock.GetTime()));
        }

        public async Task DecrementPeriod(int value)
        {   
            _quarter += value;
            // TODO: RESET THE QUARTER TIME
            SaveGameState() ;
            await _hubContext.Clients.All.SendAsync("DecrementPeriod", -value);
        } 
        
        public async Task IncrementPeriod(int value) 
        {
            _quarter += value;
            // TODO: RESET THE QUARTER TIME
            setGameConfig(_gameLevel);
            SaveGameState();
            await _hubContext.Clients.All.SendAsync("IncrementPeriod", value);
            _gameClock.InitializeClock(_quarterTime);
            await _hubContext.Clients.All.SendAsync("UpdateGameState", _gameClock.FormatTimeObject(_quarterTime));
        }

        public async Task SetFinal()
        {
            _team1Score = 0;
            _team2Score = 0;
            _homeTeamFouls = 0;
            _awayTeamFouls = 0;
            _team1TimeOuts = 5;
            _team2TimeOuts = 5;
            _quarter = 1;
            SaveGameState();
            await _hubContext.Clients.All.SendAsync("ResetGame", PushGameState());
        }
        
    }

}