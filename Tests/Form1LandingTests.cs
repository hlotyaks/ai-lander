using Xunit;
using System.Drawing;
using LanderGame;

namespace Tests
{
    public class Form1LandingTests
    {
        [Fact]
        public void SuccessfulLanding_RefuelsAndSpawnsNewPad()
        {
            // Arrange
            var form = new Form1();
            // Set client size for consistent segment width
            form.ClientSize = new Size(800, 600);
            form.InitializeForTest();
            var initialPad = form.CurrentPad;
            float initialPadX = initialPad.X;
            float initialPadW = initialPad.Width;
            float segW = form.ClientSize.Width / 40f; // terrainSegments default=40

            // Position lander above pad for perfect landing
            var lander = form.LanderInstance;
            float landingX = initialPadX + initialPadW / 2f;
            float terrainY = form.GetTerrainYAt(landingX);
            // Set state: just touches terrain with zero velocity and angle
            lander.SetState(landingX, terrainY - 20f, 0f, 0f, 0f);

            // Act
            form.Tick();

            // Assert
            Assert.True(form.LandedSuccessFlag, "LandedSuccessFlag should be true after a successful landing");
            // Fuel should be full
            Assert.Equal(100f, lander.Fuel);            // New pad should be spawned ahead by 120-160 segments (3-4 screen widths)
            var newPad = form.CurrentPad;
            Assert.NotSame(initialPad, newPad);
            float minX = initialPadX + 120f * segW;
            float maxX = initialPadX + 160f * segW;
            Assert.InRange(newPad.X, minX, maxX);
            // Width should remain the same
            Assert.Equal(initialPadW, newPad.Width);
        }
    }
}
