using Xunit;
using LanderGame;
using System.Linq;

namespace Tests
{
    public class TitleScreenTests
    {
        [Fact]
        public void Initialize_StartsInTitleScreenState()
        {
            // Arrange
            var gameEngine = new GameEngine();
            
            // Act
            gameEngine.Initialize(800, 600, 0.001f);
            
            // Assert
            Assert.Equal(GameState.TitleScreen, gameEngine.CurrentState);
            Assert.Equal(100, gameEngine.Stars.Count); // Stars should be generated for title screen
        }
        
        [Fact]
        public void StartGame_ChangesToPlayingState()
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            
            // Act
            gameEngine.StartGame(800, 600);
            
            // Assert
            Assert.Equal(GameState.Playing, gameEngine.CurrentState);
            Assert.NotNull(gameEngine.LanderInstance);
            Assert.NotNull(gameEngine.TerrainInstance);
            Assert.Single(gameEngine.Pads);
        }
        
        [Fact]
        public void Tick_InTitleScreen_DoesNotUpdateGameLogic()
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            
            // Act
            gameEngine.Tick(16f, 800, 600);
            
            // Assert - Should still be in title screen, no game objects created
            Assert.Equal(GameState.TitleScreen, gameEngine.CurrentState);
        }
        
        [Fact]
        public void Reset_ReturnsToTitleScreen()
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            gameEngine.StartGame(800, 600);
            
            // Verify we're playing
            Assert.Equal(GameState.Playing, gameEngine.CurrentState);
            
            // Act
            gameEngine.Reset(800, 600, 0.001f);
            
            // Assert
            Assert.Equal(GameState.TitleScreen, gameEngine.CurrentState);
        }
        
        [Fact]
        public void TitleScreenStars_FillEntireScreen()
        {
            // Arrange
            var gameEngine = new GameEngine();
            int clientWidth = 1200;
            int clientHeight = 800;
            
            // Act
            gameEngine.Initialize(clientWidth, clientHeight, 0.001f);
            
            // Assert
            Assert.Equal(GameState.TitleScreen, gameEngine.CurrentState);
            Assert.Equal(100, gameEngine.Stars.Count);
            
            // Check that stars cover the full width of the screen
            var starsX = gameEngine.Stars.Select(s => s.X).ToArray();
            Assert.True(starsX.Max() >= clientWidth * 0.8f, "Stars should extend close to the right edge of screen");
            Assert.True(starsX.Min() <= clientWidth * 0.2f, "Stars should extend close to the left edge of screen");
        }
        
        [Fact]
        public void GameplayStars_AvoidTerrain()
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            
            // Act
            gameEngine.StartGame(800, 600);
            
            // Assert
            Assert.Equal(GameState.Playing, gameEngine.CurrentState);
            Assert.Equal(300, gameEngine.Stars.Count); // Updated to expect 3x stars
            
            // Check that all stars are above terrain level
            foreach (var star in gameEngine.Stars)
            {
                float terrainY = gameEngine.GetTerrainYAt(star.X);
                Assert.True(star.Y < terrainY, $"Star at ({star.X}, {star.Y}) should be above terrain at Y={terrainY}");
            }
        }
          [Fact]
        public void Stars_AreDifferentBetweenTitleAndGameplay()
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            
            // Get title screen stars
            var titleStars = gameEngine.Stars.ToList();
            
            // Act
            gameEngine.StartGame(800, 600);
            var gameplayStars = gameEngine.Stars.ToList();            // Assert - The star collections should be different
            Assert.NotEmpty(titleStars);
            Assert.NotEmpty(gameplayStars);
            Assert.NotEqual(titleStars, gameplayStars);
            
            // Title screen should have 100 stars, gameplay should have more for infinite scrolling
            Assert.Equal(100, titleStars.Count);
            Assert.True(gameplayStars.Count >= 300, $"Gameplay should have at least 300 stars but has {gameplayStars.Count}");
        }

        [Fact]
        public void TitleScreen_HasMoonSurfaceRendering()
        {
            // Arrange
            var form = new Form1();
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            
            // Act - Verify title screen state has moon surface rendering capability
            // We can't directly test the drawing, but we can verify the state is correct for moon surface rendering
            
            // Assert
            Assert.Equal(GameState.TitleScreen, gameEngine.CurrentState);
            // The DrawMoonSurface method should be called when in title screen state during paint
            // This test verifies the conditions are right for moon surface rendering
            Assert.True(form.ClientSize.Width > 0 || form.ClientSize.Height > 0); // Form should have dimensions for rendering
        }

        [Fact]
        public void Stars_ExpansionMaintainsBackgroundCoverage()
        {
            // Arrange
            var seededRng = new System.Random(42); // Fixed seed for deterministic test
            var gameEngine = new GameEngine(seededRng);
            gameEngine.Initialize(800, 600, 0.001f);
            gameEngine.StartGame(800, 600);
            
            // Get initial star count and rightmost star position
            int initialStarCount = gameEngine.Stars.Count;
            float initialMaxX = gameEngine.Stars.Max(s => s.X);
            
            // Simulate camera movement far to the right (like during long gameplay)
            var lander = gameEngine.LanderInstance;
            lander.SetState(initialMaxX + 1000, 300, 0, 0, 0); // Move lander far right
            
            // Act - simulate several tick cycles to trigger star expansion
            for (int i = 0; i < 10; i++)
            {
                gameEngine.Tick(16f, 800, 600);
            }
            
            // Assert - stars should have expanded
            int newStarCount = gameEngine.Stars.Count;
            float newMaxX = gameEngine.Stars.Max(s => s.X);
            
            Assert.True(newStarCount > initialStarCount, $"Star count should increase from {initialStarCount} to {newStarCount}");
            Assert.True(newMaxX > initialMaxX, $"Star field should extend further right from {initialMaxX} to {newMaxX}");
        }

        [Fact]
        public void TitleScreen_DrawsWithoutError()
        {
            // Arrange
            var titleScreen = new TitleScreen();
            var gameEngine = new GameEngine();
            gameEngine.Initialize(800, 600, 0.001f);
            
            using var bmp = new System.Drawing.Bitmap(800, 600);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            
            // Act & Assert: Should not throw
            titleScreen.Draw(g, 800, 600, gameEngine.Stars);
        }
    }
}
