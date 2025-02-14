using System;
using System.Threading;
using Scoreboard.Services;

public class KeyListenerService
{
    private readonly GameClockService _gameClockService;
    private readonly ScoreboardService _scoreboardService;

    public KeyListenerService(GameClockService gameClockService, ScoreboardService scoreboardService)
    {
        _gameClockService = gameClockService;
        _scoreboardService = scoreboardService;
    }

    public void StartListening()
    {
        Console.WriteLine("Key Listener Started. Press F11 (Start), F12 (Stop), F3 (Reset).");

        Thread keyListenerThread = new Thread(() =>
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    Console.WriteLine(key.Key);
                    switch (key.Key)
                    {
                        // case ConsoleKey.D8:
                        //     Console.WriteLine("F11 Pressed: Start Clock");
                        //     _scoreboardService.incrementAway(+3994);
                        //     break;
                        case ConsoleKey.D9:
                            Console.WriteLine(_gameClockService.checkRunning());
                            if(_gameClockService.checkRunning())
                            {
                                Console.WriteLine("F12 Pressed: Stop Clock");
                            _gameClockService.StopClock();
                            }
                            else 
                            {
                                Console.WriteLine("F911 Pressed: Start Clock");
                            _gameClockService.StartClock();
                            }
                            
                            break;
                        case ConsoleKey.D1:
                            // Console.WriteLine("F12 Pressed: Stop Clock");
                            _scoreboardService.incrementHome(+1);
                            break;
                        case ConsoleKey.D2:
                            // Console.WriteLine("F12 Pressed: Stop Clock");
                            _scoreboardService.incrementHome(-1);
                            break;
                         case ConsoleKey.D4:
                            // Console.WriteLine("F12 Pressed: Stop Clock");
                            _scoreboardService.incrementHome(2);
                            break;
                        case ConsoleKey.D7:
                            // Console.WriteLine("F12 Pressed: Stop Clock");
                            _scoreboardService.incrementHome(3);
                            break;
                        case ConsoleKey.D3:
                            // Console.WriteLine("F12 Pressed: Stop Clock");
                            _scoreboardService.IncrementAwayScore(1);
                            break;
                        case ConsoleKey.D6:
                            // Console.WriteLine("F12 Pressed: Stop Clock");
                            _scoreboardService.IncrementAwayScore(2);
                            break;
                        case ConsoleKey.D8:
                            // Console.WriteLine("F12 Pressed: Stop Clock");
                            _scoreboardService.IncrementAwayScore(3);
                            break;
                        // case ConsoleKey.F3:
                        //     Console.WriteLine("F3 Pressed: Reset Clock");
                        //     _gameClockService.ResetClock(600); // Example: Resets to 10 minutes
                        //     break;
                    }
                }
                Thread.Sleep(100); // Prevents CPU overuse
            }
        });

        keyListenerThread.IsBackground = true;
        keyListenerThread.Start();
    }
}
