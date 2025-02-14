using Microsoft.AspNetCore.SignalR;
using System;
using System.Timers;
using Scoreboard.Hubs;

namespace Scoreboard.Services
{
    public class GameClockService
    {
        private readonly IHubContext<ScoreboardHub> _hubContext;

        private double _remainingTime = 480; // Time left in seconds
        private System.Timers.Timer _timer; 
        private DateTime? _lastStartTime; // Time when clock was last started
        private bool _isRunning; // True if clock is running
        private double _timeRemainingAtStart = 0;

        public GameClockService(IHubContext<ScoreboardHub> hubContext)
        {
            Console.WriteLine("GameClockService");
            _hubContext = hubContext;
            _timer = new System.Timers.Timer(100); // Tick every 100ms
            _timer.Elapsed += TimerTick;
            _timer.AutoReset = true;
            _isRunning = false;
        }

        public void InitializeClock(double quarterTime)
        {
            _remainingTime = quarterTime; // Set countdown start time
            _lastStartTime = null;
            _isRunning = false;
        }


        public bool checkRunning()
        {
            return _isRunning;
        }

        public void StartClock()
        {
            if (!_isRunning)
            {
                _lastStartTime = DateTime.UtcNow; // Store when clock starts
                _timeRemainingAtStart = _remainingTime;
                Console.WriteLine(_lastStartTime);
                _isRunning = true;
                _timer.Start();
                Console.WriteLine("Clock started");
            }
        }

        public void StopClock()
        {
            if (_isRunning)
            {
                // Calculate elapsed time only while running
                Console.WriteLine(DateTime.UtcNow);
                Console.WriteLine(_lastStartTime.Value);
                Console.WriteLine((DateTime.UtcNow - _lastStartTime.Value).TotalSeconds);
                _timeRemainingAtStart -= (DateTime.UtcNow - _lastStartTime.Value).TotalSeconds;
                if (_remainingTime != _timeRemainingAtStart)
                {
                    _remainingTime = _timeRemainingAtStart;
                }
                _remainingTime = Math.Max(_remainingTime, 0); // Prevent negative time
                _timer.Stop();
                _isRunning = false;
                _lastStartTime = null;
                Console.WriteLine($"Clock stopped. Remaining time: {_remainingTime} sec");
            }
        }

        public double GetTime()
        {
            return ComputeRemainingTime();
        }

        private void TimerTick(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine($"HERE: {_remainingTime} sec");
            if (!_isRunning) return; // Only update when running
            _remainingTime -= 0.1; // Subtract 100ms
            _remainingTime = Math.Max(_remainingTime, 0); // Prevent negative values

            if (_remainingTime <= 0)
            {
                StopClock();
                _hubContext.Clients.All.SendAsync("TimerEnded");
            }
            else
            {
                _hubContext.Clients.All.SendAsync("UpdateGameState", FormatTimeObject(_remainingTime));
            }
        }

        private double ComputeRemainingTime()
        {
            if (!_isRunning) return _remainingTime; // Use stored time when paused
            return Math.Max(_remainingTime - (DateTime.UtcNow - _lastStartTime.Value).TotalSeconds, 0);
        }

        public object FormatTimeObject(double time)
        {
            return new
            {
                minutes = (int)(time / 60),
                seconds = (int)(time % 60),
                tenth_seconds = (int)((time * 10) % 10)
            };
        }
        
        public double ConvertTimeObjectToSeconds(Scoreboard.Models.TimeObject timeObject)
        {
            
            // Console.WriteLine($"HERE {timeObject.Minutes * 60}");
            return (timeObject.Minutes * 60) + timeObject.Seconds + (timeObject.Tenths * 0.1);
        }
    }
}
