using Xunit;
using System.Drawing;
using LanderGame;

namespace Tests
{
    public class GameEngineTests
    {        [Fact]
        public void Tick_WithSuccessfulLanding_SetsLandedSuccessFlag()
        {
            // Arrange
            var gameEngine = new GameEngine();
            int clientWidth = 800;
            int clientHeight = 600;
            float gravity = 0.001f; // Earth gravity
            
            gameEngine.Initialize(clientWidth, clientHeight, gravity);
            gameEngine.StartGame(clientWidth, clientHeight);
            
            // Get the initial landing pad
            var initialPad = gameEngine.CurrentPad;
            var lander = gameEngine.LanderInstance;
            
            // Position lander for a perfect landing
            float landingX = initialPad.X + initialPad.Width / 2f;
            float terrainY = gameEngine.GetTerrainYAt(landingX);
            
            // Set lander state: just above terrain, perfect angle, low speed
            lander.SetState(landingX, terrainY - 21f, 0f, 0f, 0.1f); // Small downward velocity
            
            // Verify initial state
            Assert.False(gameEngine.LandedSuccessFlag);
            Assert.False(gameEngine.IsGameOver);
            Assert.Single(gameEngine.Pads); // Should start with one pad
            
            // Act
            float deltaTime = 16f; // Simulate 16ms frame time
            gameEngine.Tick(deltaTime, clientWidth, clientHeight);
              // Assert
            Assert.True(gameEngine.LandedSuccessFlag, "LandedSuccessFlag should be true after successful landing");
            Assert.False(gameEngine.IsGameOver, "Game should not be over after successful landing");
            Assert.Equal(100f, lander.Fuel);
            Assert.Equal(2, gameEngine.Pads.Count);
            Assert.True(initialPad.IsUsed, "Initial pad should be marked as used after landing");
            
            // Verify lander is stationary after landing
            Assert.Equal(0f, lander.Vx);
            Assert.Equal(0f, lander.Vy);
        }
          [Fact]
        public void Tick_WithCrashConditions_SetsGameOverState()
        {
            // Arrange
            var gameEngine = new GameEngine();
            int clientWidth = 800;
            int clientHeight = 600;
            float gravity = 0.001f;
            
            gameEngine.Initialize(clientWidth, clientHeight, gravity);
            gameEngine.StartGame(clientWidth, clientHeight);
            
            var initialPad = gameEngine.CurrentPad;
            var lander = gameEngine.LanderInstance;
            
            // Position lander for a crash (too fast landing)
            float landingX = initialPad.X + initialPad.Width / 2f;
            float terrainY = gameEngine.GetTerrainYAt(landingX);
            
            // Set lander state: perfect position and angle, but excessive speed
            lander.SetState(landingX, terrainY - 21f, 0f, 0f, 2.0f); // Too fast downward velocity
            
            // Verify initial state
            Assert.False(gameEngine.IsGameOver);
            Assert.False(gameEngine.LandedSuccessFlag);
            
            // Act
            float deltaTime = 16f;
            gameEngine.Tick(deltaTime, clientWidth, clientHeight);
              // Assert
            Assert.True(gameEngine.IsGameOver, "Game should be over after crash");
            Assert.False(gameEngine.LandedSuccessFlag, "LandedSuccessFlag should remain false after crash");
            Assert.Contains("excessive speed", gameEngine.CrashReason);
            Assert.True(gameEngine.Debris.Count > 0, "Debris should be generated after crash");
        }
          [Fact]
        public void Tick_WithLandedLander_SkipsPhysicsWhenStationary()
        {
            // Arrange
            var gameEngine = new GameEngine();
            int clientWidth = 800;
            int clientHeight = 600;
            float gravity = 0.001f;
            
            gameEngine.Initialize(clientWidth, clientHeight, gravity);
            gameEngine.StartGame(clientWidth, clientHeight);
            
            var initialPad = gameEngine.CurrentPad;
            var lander = gameEngine.LanderInstance;
            
            // Simulate a successful landing first
            float landingX = initialPad.X + initialPad.Width / 2f;
            float terrainY = gameEngine.GetTerrainYAt(landingX);
            lander.SetState(landingX, terrainY - 21f, 0f, 0f, 0.1f);
            
            // First tick to land
            gameEngine.Tick(16f, clientWidth, clientHeight);
            Assert.True(gameEngine.LandedSuccessFlag);
            
            // Store position after landing
            float landedX = lander.X;
            float landedY = lander.Y;
            
            // Act - tick again without input (should remain stationary)
            gameEngine.UpdateInput(false, false, false); // No input
            gameEngine.Tick(16f, clientWidth, clientHeight);
              // Assert - lander should remain in exact same position
            Assert.Equal(landedX, lander.X);
            Assert.Equal(landedY, lander.Y);
            Assert.Equal(0f, lander.Vx);
            Assert.Equal(0f, lander.Vy);
            Assert.True(gameEngine.LandedSuccessFlag, "LandedSuccessFlag should remain true");
        }
          [Fact]
        public void Tick_WithTakeoffFromLandedState_ResetsLandedFlag()
        {
            // Arrange
            var gameEngine = new GameEngine();
            int clientWidth = 800;
            int clientHeight = 600;
            float gravity = 0.001f;
            
            gameEngine.Initialize(clientWidth, clientHeight, gravity);
            gameEngine.StartGame(clientWidth, clientHeight);
            
            var initialPad = gameEngine.CurrentPad;
            var lander = gameEngine.LanderInstance;
            
            // Simulate a successful landing
            float landingX = initialPad.X + initialPad.Width / 2f;
            float terrainY = gameEngine.GetTerrainYAt(landingX);
            lander.SetState(landingX, terrainY - 21f, 0f, 0f, 0.1f);
            gameEngine.Tick(16f, clientWidth, clientHeight);
            Assert.True(gameEngine.LandedSuccessFlag);
            
            // Act - apply thrust to take off
            gameEngine.UpdateInput(true, false, false); // Thrust input
            gameEngine.Tick(16f, clientWidth, clientHeight);
            
            // Assert - landed flag should be reset
            Assert.False(gameEngine.LandedSuccessFlag, "LandedSuccessFlag should be reset when taking off");
            Assert.False(gameEngine.IsGameOver, "Game should not be over during takeoff");
        }
    }
}
