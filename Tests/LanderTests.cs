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
    }
}
