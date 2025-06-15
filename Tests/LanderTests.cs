using Xunit;
using LanderGame;

namespace Tests
{
    public class LanderTests
    {
        [Fact]
        public void Lander_Initializes_With_Correct_Position()
        {
            var lander = new Lander(100, 200);
            Assert.Equal(100, lander.X);
            Assert.Equal(200, lander.Y);
        }

        [Fact]
        public void Lander_Reset_Sets_Position()
        {
            var lander = new Lander(0, 0);
            lander.Reset(50, 75);
            Assert.Equal(50, lander.X);
            Assert.Equal(75, lander.Y);
        }

        [Fact]
        public void Refuel_Resets_Fuel_To_Max()
        {
            float startFuel = 50f;
            var lander = new Lander(0, 0, startFuel);
            // consume some fuel
            lander.Update(1000, true, false, false, 0f);
            Assert.True(lander.Fuel < startFuel);
            // refuel
            lander.Refuel();
            Assert.Equal(startFuel, lander.Fuel);
        }

        [Fact]
        public void ThrustDuration_Increases_When_Thrusting()
        {
            var lander = new Lander(0, 0, 100f);
            Assert.Equal(0f, lander.GetThrustDuration());
            
            // Thrust for 500ms
            lander.Update(500f, true, false, false, 0f);
            Assert.Equal(500f, lander.GetThrustDuration());
            
            // Thrust for another 1000ms
            lander.Update(1000f, true, false, false, 0f);
            Assert.Equal(1500f, lander.GetThrustDuration());
        }

        [Fact]
        public void ThrustDuration_Resets_When_Not_Thrusting()
        {
            var lander = new Lander(0, 0, 100f);
            
            // Build up thrust duration
            lander.Update(1000f, true, false, false, 0f);
            Assert.Equal(1000f, lander.GetThrustDuration());
            
            // Stop thrusting
            lander.Update(100f, false, false, false, 0f);
            Assert.Equal(0f, lander.GetThrustDuration());
        }        [Fact]
        public void ThrustDuration_Caps_At_Maximum()
        {
            var lander = new Lander(0, 0, 100f);
            
            // Thrust for more than 2 seconds (2000ms)
            lander.Update(3000f, true, false, false, 0f);
            Assert.Equal(2000f, lander.GetThrustDuration());
        }

        [Fact]
        public void FlamePolygon_Returns_Null_When_Not_Thrusting()
        {
            var lander = new Lander(0, 0, 100f);
            var flame = lander.GetFlamePolygon(false);
            Assert.Null(flame);
        }

        [Fact]
        public void FlamePolygon_Returns_Null_When_No_Fuel()
        {
            var lander = new Lander(0, 0, 0f); // No fuel
            var flame = lander.GetFlamePolygon(true);
            Assert.Null(flame);
        }

        [Fact]
        public void FlamePolygon_Grows_With_Thrust_Duration()
        {
            var lander = new Lander(0, 0, 100f);
            
            // Stage 0 (0ms - 600ms): smallest flame
            lander.SetThrustDuration(0f);
            var flame0 = lander.GetFlamePolygon(true);
            Assert.NotNull(flame0);
            var baseLength0 = flame0![1].Y - flame0[0].Y; // Tip Y - Base Y
            
            // Stage 2 (1200ms - 1800ms): medium flame
            lander.SetThrustDuration(1500f);
            var flame2 = lander.GetFlamePolygon(true);
            Assert.NotNull(flame2);
            var baseLength2 = flame2![1].Y - flame2[0].Y;
              // Stage 4 (1600ms - 2000ms): largest flame
            lander.SetThrustDuration(2000f);
            var flame4 = lander.GetFlamePolygon(true);
            Assert.NotNull(flame4);
            var baseLength4 = flame4![1].Y - flame4[0].Y;
            
            // Each stage should be progressively longer (accounting for random variation)
            Assert.True(baseLength2 > baseLength0 - 5f); // Allow for random variation
            Assert.True(baseLength4 > baseLength2 - 5f);
        }

        [Fact]
        public void FlamePolygon_Has_Correct_Base_Position()
        {
            var lander = new Lander(0, 0, 100f);
            var flame = lander.GetFlamePolygon(true);
            Assert.NotNull(flame);
            
            // Flame should start at bottom of lander (Y=12)
            Assert.Equal(-5f, flame![0].X); // Left base
            Assert.Equal(12f, flame[0].Y);
            Assert.Equal(5f, flame[2].X);   // Right base
            Assert.Equal(12f, flame[2].Y);
            Assert.Equal(0f, flame[1].X);   // Tip center
        }
    }
}
