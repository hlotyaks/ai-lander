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
    }
}
