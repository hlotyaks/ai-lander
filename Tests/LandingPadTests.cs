using Xunit;
using LanderGame;

namespace Tests
{
    public class LandingPadTests
    {
        [Fact]
        public void LandingPad_Initializes_Correctly()
        {
            var pad = new LandingPad(10, 50, 400, 500);
            Assert.Equal(10, pad.X);
            Assert.Equal(50, pad.Width);
            Assert.Equal(400, pad.Y);
            Assert.True(pad.LightsVisible);
        }

        [Fact]
        public void LandingPad_ToggleLights_Changes_Visibility()
        {
            var pad = new LandingPad(0, 10, 10, 100);
            bool initial = pad.LightsVisible;
            // Simulate timer tick
            pad.GetType().GetMethod("ToggleLights", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(pad, null);
            Assert.NotEqual(initial, pad.LightsVisible);
        }

        [Fact]
        public void StopBlinking_MarksAsUsedAndShowsRedLights()
        {
            // Arrange
            var pad = new LandingPad(100, 50, 400, 500);
            
            // Verify initial state
            Assert.False(pad.IsUsed, "Pad should not be marked as used initially");
            Assert.True(pad.LightsVisible, "Lights should be visible initially");
            
            // Act - simulate landing (which calls StopBlinking)
            pad.StopBlinking();
            
            // Assert
            Assert.True(pad.IsUsed, "Pad should be marked as used after StopBlinking");
            Assert.True(pad.LightsVisible, "Lights should remain visible after StopBlinking for red light display");
        }
    }
}
