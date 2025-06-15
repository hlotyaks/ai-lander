using System;
using LanderGame;
using System.Linq;

namespace StarfieldTest
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Starfield Expansion Test ===");
            
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            gameEngine.StartGame(800, 600);
            
            Console.WriteLine($"Initial star count: {gameEngine.Stars.Count}");
            var initialMaxX = gameEngine.Stars.Any() ? gameEngine.Stars.Max(s => s.X) : 0;
            Console.WriteLine($"Initial max star X: {initialMaxX:F2}");
            
            // Simulate moving far to the right
            var lander = gameEngine.LanderInstance;
            lander.SetState(initialMaxX + 2000, 300, 0, 0, 0);
            
            // Run several game ticks to trigger expansion
            for (int i = 0; i < 20; i++)
            {
                gameEngine.Tick(16f, 800, 600);
            }
            
            Console.WriteLine($"Final star count: {gameEngine.Stars.Count}");
            var finalMaxX = gameEngine.Stars.Any() ? gameEngine.Stars.Max(s => s.X) : 0;
            Console.WriteLine($"Final max star X: {finalMaxX:F2}");
            
            Console.WriteLine($"Star expansion: {(gameEngine.Stars.Count > 300 ? "SUCCESS" : "FAILED")}");
            Console.WriteLine($"Coverage expansion: {(finalMaxX > initialMaxX + 1000 ? "SUCCESS" : "FAILED")}");
        }
    }
}
