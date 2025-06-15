using System;
using LanderGame;

namespace TestDebugger
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting test debug session...");
            
            // Put your failing test logic here
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            
            var pad = gameEngine.CurrentPad;
            var lander = gameEngine.LanderInstance;
            
            // Example: Test excessive velocity crash
            float landingX = pad.X + (pad.Width / 2f);
            float terrainY = gameEngine.GetTerrainYAt(landingX);
            lander.SetState(landingX, terrainY - 21f, 0f, -5f, 1f); // Excessive velocity
            
            Console.WriteLine($"Before tick - IsGameOver: {gameEngine.IsGameOver}");
            Console.WriteLine($"Lander position: ({lander.X}, {lander.Y})");
            Console.WriteLine($"Lander velocity: ({lander.Vx}, {lander.Vy})");
            
            // Set breakpoint here
            gameEngine.Tick(16f, 800, 600);
            
            Console.WriteLine($"After tick - IsGameOver: {gameEngine.IsGameOver}");
            Console.WriteLine($"Crash reason: {gameEngine.CrashReason}");
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
