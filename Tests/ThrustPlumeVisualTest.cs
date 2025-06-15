using Xunit;
using LanderGame;
using System.Drawing;
using System.Linq;

namespace Tests
{
    public class ThrustPlumeVisualTest
    {
        [Fact]
        public void ThrustPlume_Growth_Stages_Documentation()
        {
            var lander = new Lander(0, 0, 100f);
              // Document the expected flame lengths for each stage
            float[] expectedStageLengths = { 8f, 14f, 20f, 26f, 32f };
            float[] stageDurations = { 0f, 400f, 800f, 1200f, 1600f };
            
            for (int stage = 0; stage < 5; stage++)
            {
                lander.SetThrustDuration(stageDurations[stage]);
                var flame = lander.GetFlamePolygon(true);
                Assert.NotNull(flame);
                
                // Base flame length (without random variation)
                float expectedLength = expectedStageLengths[stage];
                float actualLength = flame![1].Y - flame[0].Y;
                
                // Allow for ±2 pixels random variation
                Assert.InRange(actualLength, expectedLength - 2f, expectedLength + 2f);
                
                // Output for documentation (this will show in test output)
                Assert.True(true, $"Stage {stage}: Duration={stageDurations[stage]}ms, ExpectedLength={expectedLength}px, ActualLength={actualLength:F1}px");
            }
        }
        
        [Fact]
        public void ThrustPlume_MaximumLength_Equals_LanderHeight()
        {
            var lander = new Lander(0, 0, 100f);
            
            // Get lander dimensions
            var shipPoly = lander.GetShipPolygon();
            float landerHeight = shipPoly.Max(p => p.Y) - shipPoly.Min(p => p.Y);
              // Set maximum thrust duration
            lander.SetThrustDuration(2000f);
            var flame = lander.GetFlamePolygon(true);
            Assert.NotNull(flame);
            
            float maxFlameLength = flame![1].Y - flame[0].Y;
            
            // Maximum flame should be approximately equal to lander height (±5px tolerance for random variation)
            Assert.InRange(maxFlameLength, landerHeight - 5f, landerHeight + 5f);
            
            Assert.True(true, $"Lander height: {landerHeight}px, Max flame length: {maxFlameLength:F1}px");
        }
    }
}
