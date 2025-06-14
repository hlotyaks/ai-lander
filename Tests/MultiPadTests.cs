using Xunit;
using System.Drawing;
using System.Linq;
using LanderGame;

namespace Tests
{
    public class MultiPadTests
    {
        [Fact]
        public void FirstSuccessfulLanding_MarksInitialPadAsUsed()
        {
            // Arrange
            var form = new Form1();
            form.ClientSize = new Size(800, 600);
            form.InitializeForTest();
            var initialPad = form.CurrentPad;

            // Position lander for perfect landing
            var lander = form.LanderInstance;
            float landingX = initialPad.X + initialPad.Width / 2f;
            float terrainY = form.GetTerrainYAt(landingX);
            lander.SetState(landingX, terrainY - 20f, 0f, 0f, 0f);

            // Act
            form.Tick();

            // Assert
            Assert.True(initialPad.IsUsed, "Initial pad should be marked as used after landing");
        }

        [Fact]
        public void FirstSuccessfulLanding_AddsANewPad()
        {
            // Arrange
            var form = new Form1();
            form.ClientSize = new Size(800, 600);
            form.InitializeForTest();
            var initialPad = form.CurrentPad;

            // Position lander for perfect landing
            var lander = form.LanderInstance;
            float landingX = initialPad.X + initialPad.Width / 2f;
            float terrainY = form.GetTerrainYAt(landingX);
            lander.SetState(landingX, terrainY - 20f, 0f, 0f, 0f);

            // Act
            form.Tick();

            // Assert
            Assert.Equal(2, form.Pads.Count);
            var newPad = form.Pads.Last();
            Assert.NotSame(initialPad, newPad);
            Assert.False(newPad.IsUsed, "New pad should not be marked as used initially");
        }
    }
}
