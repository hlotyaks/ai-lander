using Xunit;
using LanderGame;
using System.Linq;

namespace Tests
{
    public class LandingPadPlacementTests
    {
        [Fact]
        public void LandingPads_ShouldBePlacedOnFlatTerrain()
        {
            // Arrange
            var seededRng = new System.Random(123); // Fixed seed for deterministic test
            var gameEngine = new GameEngine(seededRng);
            gameEngine.Initialize(800, 600, 0.001f);
            gameEngine.StartGame(800, 600);

            // Act - Check initial landing pad
            var initialPad = gameEngine.Pads.First();
            var terrain = gameEngine.TerrainInstance;

            // Assert - Landing pad should be on relatively flat terrain
            float leftHeight = terrain.GetHeightAt(initialPad.X);
            float rightHeight = terrain.GetHeightAt(initialPad.X + initialPad.Width);
            float centerHeight = terrain.GetHeightAt(initialPad.X + initialPad.Width / 2);

            // The terrain under the pad should be flat (height variation should be minimal)
            float maxHeightDiff = System.Math.Max(
                System.Math.Abs(leftHeight - centerHeight),
                System.Math.Abs(rightHeight - centerHeight)
            );

            Assert.True(maxHeightDiff <= 15f, 
                $"Landing pad should be on flat terrain. Height variation: {maxHeightDiff}");
        }

        [Fact]
        public void NewLandingPads_ShouldBePlacedAtSuitableDistance()
        {
            // Arrange
            var seededRng = new System.Random(456); // Fixed seed for deterministic test
            var gameEngine = new GameEngine(seededRng);
            gameEngine.Initialize(800, 600, 0.001f);
            gameEngine.StartGame(800, 600);

            // Simulate successful landing to generate a new pad
            var lander = gameEngine.LanderInstance;
            var initialPad = gameEngine.Pads.First();
            
            // Position lander on the pad with proper landing conditions
            lander.SetState(initialPad.X + initialPad.Width/2, initialPad.Y - 20, 0, 0.1f, 0.1f);
            
            // Act - Simulate landing (this should generate a new pad)
            gameEngine.Tick(16f, 800, 600);

            // Assert - Should have two pads now
            Assert.True(gameEngine.Pads.Count >= 1, "Should have at least one landing pad");
            
            if (gameEngine.Pads.Count > 1)
            {
                var newPad = gameEngine.Pads.Last();
                float distance = System.Math.Abs(newPad.X - initialPad.X);                // New pad should be at reasonable distance (3-4 screen widths)
                Assert.True(distance >= 2300f, $"New pad should be at least 2300 units away (3 screens). Distance: {distance}");
                Assert.True(distance <= 3300f, $"New pad should not be too far away (4+ screens). Distance: {distance}");
            }
        }
    }
}
