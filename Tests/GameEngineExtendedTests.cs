using Xunit;
using System.Drawing;
using LanderGame;
using System.Linq;

namespace Tests
{
    public class GameEngineExtendedTests
    {
        [Fact]
        public void Initialize_CreatesRequiredComponents()
        {
            // Arrange
            var gameEngine = new GameEngine();
            int clientWidth = 800;
            int clientHeight = 600;
            float gravity = 0.001f;
            
            // Act
            gameEngine.Initialize(clientWidth, clientHeight, gravity);
            
            // Assert
            Assert.NotNull(gameEngine.LanderInstance);
            Assert.NotNull(gameEngine.TerrainInstance);
            Assert.Single(gameEngine.Pads);
            Assert.Equal(100, gameEngine.Stars.Count);
            Assert.Empty(gameEngine.Debris);
            Assert.False(gameEngine.IsGameOver);
            Assert.False(gameEngine.LandedSuccessFlag);
            Assert.Equal(string.Empty, gameEngine.CrashReason);
            Assert.Equal(0f, gameEngine.CameraX);
        }
        
        [Fact]
        public void Reset_ResetsAllGameState()
        {
            // Arrange
            var gameEngine = new GameEngine();
            int clientWidth = 800;
            int clientHeight = 600;
            float gravity = 0.001f;
            
            gameEngine.Initialize(clientWidth, clientHeight, gravity);
            
            // Simulate some game state changes
            gameEngine.UpdateInput(true, false, false);
            gameEngine.Tick(16f, clientWidth, clientHeight);
            
            // Act
            gameEngine.Reset(clientWidth, clientHeight, gravity);
            
            // Assert
            Assert.False(gameEngine.IsGameOver);
            Assert.False(gameEngine.LandedSuccessFlag);
            Assert.Equal(string.Empty, gameEngine.CrashReason);
            Assert.Equal(0f, gameEngine.CameraX);
            Assert.Single(gameEngine.Pads);
            Assert.Empty(gameEngine.Debris);
        }
        
        [Fact]
        public void UpdateInput_SetsInputStatesCorrectly()
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            var lander = gameEngine.LanderInstance;
            float initialFuel = lander.Fuel;
            
            // Act - Apply thrust input and tick
            gameEngine.UpdateInput(true, false, false);
            gameEngine.Tick(100f, 800, 600); // Longer delta to see fuel consumption
            
            // Assert - Lander should have consumed fuel due to thrust
            Assert.True(lander.Fuel < initialFuel, "Fuel should be consumed when thrusting");
        }
          [Theory]
        [InlineData(-5f, 1f)] // Moving too fast left
        [InlineData(5f, 1f)]  // Moving too fast right
        [InlineData(0f, 2f)]  // Moving too fast down
        public void Tick_WithExcessiveVelocity_CausesSpeedCrash(float vx, float vy)
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            
            var pad = gameEngine.CurrentPad;
            var lander = gameEngine.LanderInstance;
            
            // Ensure lander is positioned well within the pad bounds
            float landingX = pad.X + (pad.Width / 2f); // Center of pad
            float terrainY = gameEngine.GetTerrainYAt(landingX);
              // Debug output to verify positioning
            System.Console.WriteLine($"=== Test with vx={vx}, vy={vy} ===");
            System.Console.WriteLine($"Pad count: {gameEngine.Pads.Count}");
            System.Console.WriteLine($"Current pad IsUsed: {pad.IsUsed}");
            System.Console.WriteLine($"Pad bounds: X={pad.X}, Width={pad.Width}, Right={pad.X + pad.Width}");
            System.Console.WriteLine($"Landing X: {landingX}, Terrain Y: {terrainY}");
            
            lander.SetState(landingX, terrainY - 19f, 0f, vx, vy);
            
            // Verify lander is positioned on pad before crash
            System.Console.WriteLine($"Lander position: X={lander.X}, Y={lander.Y}, Vx={lander.Vx}, Vy={lander.Vy}");
            System.Console.WriteLine($"Lander is on pad: {lander.X >= pad.X && lander.X <= pad.X + pad.Width}");
              // Check what the collision detection will find
            var foundPad = gameEngine.Pads.FirstOrDefault(p => !p.IsUsed && lander.X >= p.X && lander.X <= p.X + p.Width);
            System.Console.WriteLine($"Found unused pad at lander position: {foundPad != null}");
            
            // Act
            gameEngine.Tick(1f, 800, 600); // Use very small delta to prevent overshooting
            
            // Assert
            Assert.True(gameEngine.IsGameOver, $"Game should be over. Crash reason: {gameEngine.CrashReason}");
            Assert.Contains("excessive speed", gameEngine.CrashReason);
        }
        
        [Fact]
        public void Tick_WithBadAngle_CausesAngleCrash()
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            
            var pad = gameEngine.CurrentPad;
            var lander = gameEngine.LanderInstance;
            
            // Position lander for landing with bad angle (>15 degrees)
            float landingX = pad.X + pad.Width / 2f;
            float terrainY = gameEngine.GetTerrainYAt(landingX);
            float badAngle = 20f * (float)System.Math.PI / 180f; // 20 degrees in radians
            lander.SetState(landingX, terrainY - 21f, badAngle, 0f, 0.1f);
            
            // Act
            gameEngine.Tick(16f, 800, 600);
            
            // Assert
            Assert.True(gameEngine.IsGameOver);
            Assert.Contains("bad angle", gameEngine.CrashReason);
        }
        
        [Fact]
        public void Tick_LandingOffPad_CausesNoPadCrash()
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            
            var pad = gameEngine.CurrentPad;
            var lander = gameEngine.LanderInstance;
            
            // Position lander for landing outside of pad bounds
            float landingX = pad.X - 50f; // Land well outside pad
            float terrainY = gameEngine.GetTerrainYAt(landingX);
            lander.SetState(landingX, terrainY - 21f, 0f, 0f, 0.1f);
            
            // Act
            gameEngine.Tick(16f, 800, 600);
            
            // Assert
            Assert.True(gameEngine.IsGameOver);
            Assert.Contains("no pad", gameEngine.CrashReason);
        }
        
        [Fact]
        public void Tick_CameraFollowsLander()
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            
            var lander = gameEngine.LanderInstance;
            float initialCameraX = gameEngine.CameraX;
            
            // Move lander far to the right
            lander.SetState(1000f, 50f, 0f, 0f, 0f);
            
            // Act
            gameEngine.Tick(16f, 800, 600);
            
            // Assert
            Assert.True(gameEngine.CameraX > initialCameraX, "Camera should follow lander movement");
        }
        
        [Fact]
        public void Tick_DebrisMovesWithPhysics()
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            
            // Force a crash to generate debris
            var pad = gameEngine.CurrentPad;
            var lander = gameEngine.LanderInstance;
            float landingX = pad.X + pad.Width / 2f;
            float terrainY = gameEngine.GetTerrainYAt(landingX);
            lander.SetState(landingX, terrainY - 21f, 0f, 0f, 2f); // Excessive speed
            
            gameEngine.Tick(16f, 800, 600); // This should crash and create debris
            
            var initialDebrisCount = gameEngine.Debris.Count;
            var initialDebrisPositions = gameEngine.Debris.ToList();
            
            // Act - Tick again to move debris
            gameEngine.Tick(16f, 800, 600);
            
            // Assert
            Assert.Equal(initialDebrisCount, gameEngine.Debris.Count);
            // Check that debris has moved (at least some pieces should have different positions)
            var currentDebris = gameEngine.Debris.ToList();
            bool debrisHasMoved = false;
            for (int i = 0; i < initialDebrisPositions.Count; i++)
            {
                if (initialDebrisPositions[i].start.X != currentDebris[i].start.X ||
                    initialDebrisPositions[i].start.Y != currentDebris[i].start.Y)
                {
                    debrisHasMoved = true;
                    break;
                }
            }
            Assert.True(debrisHasMoved, "Debris should move with physics");
        }
        
        [Fact]
        public void GetTerrainYAt_ReturnsValidHeight()
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            
            // Act
            float terrainHeight = gameEngine.GetTerrainYAt(400f);
            
            // Assert
            Assert.True(terrainHeight > 0f);
            Assert.True(terrainHeight < 600f);
        }
        
        [Fact]
        public void CurrentPad_ReturnsLastPadInList()
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            
            // Force successful landing to create a second pad
            var initialPad = gameEngine.CurrentPad;
            var lander = gameEngine.LanderInstance;
            float landingX = initialPad.X + initialPad.Width / 2f;
            float terrainY = gameEngine.GetTerrainYAt(landingX);
            lander.SetState(landingX, terrainY - 21f, 0f, 0f, 0.1f);
            
            gameEngine.Tick(16f, 800, 600);
            
            // Act & Assert
            Assert.Equal(2, gameEngine.Pads.Count);
            Assert.Same(gameEngine.Pads.Last(), gameEngine.CurrentPad);
            Assert.NotSame(initialPad, gameEngine.CurrentPad);
        }
    }
}
