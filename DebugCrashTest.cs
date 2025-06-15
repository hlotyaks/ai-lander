using System;
using LanderGame;

class DebugCrashTest
{
    static void Main()
    {
        Console.WriteLine("=== Debug Crash Test ===");
        
        var gameEngine = new GameEngine();
        gameEngine.Initialize(800, 600, 0.001f);
        
        var pad = gameEngine.CurrentPad;
        var lander = gameEngine.LanderInstance;
        
        Console.WriteLine($"Pad count: {gameEngine.Pads.Count}");
        Console.WriteLine($"Current pad IsUsed: {pad.IsUsed}");
        Console.WriteLine($"Pad bounds: X={pad.X}, Width={pad.Width}, Right={pad.X + pad.Width}");
        
        // Ensure lander is positioned well within the pad bounds
        float landingX = pad.X + (pad.Width / 2f); // Center of pad
        float terrainY = gameEngine.GetTerrainYAt(landingX);
        
        Console.WriteLine($"Landing X: {landingX}, Terrain Y: {terrainY}");
        
        // Test with excessive horizontal velocity
        lander.SetState(landingX, terrainY - 21f, 0f, 5f, 1f);
        
        Console.WriteLine($"Lander position before tick: X={lander.X}, Y={lander.Y}, Vx={lander.Vx}, Vy={lander.Vy}");
        Console.WriteLine($"Lander is on pad: {lander.X >= pad.X && lander.X <= pad.X + pad.Width}");
        
        // Simulate the pad lookup logic from GameEngine
        var foundPad = gameEngine.Pads.FirstOrDefault(p => !p.IsUsed && lander.X >= p.X && lander.X <= p.X + p.Width);
        Console.WriteLine($"Found unused pad at lander position: {foundPad != null}");
        
        gameEngine.Tick(16f, 800, 600);
        
        Console.WriteLine($"After tick - Game over: {gameEngine.IsGameOver}");
        Console.WriteLine($"Crash reason: '{gameEngine.CrashReason}'");
    }
}
