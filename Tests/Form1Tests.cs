using Xunit;
using LanderGame;

namespace Tests
{
    public class Form1Tests
    {
        [Fact]
        public void DefaultEnvironment_ShouldBeEarth()
        {
            var form = new Form1();
            Assert.Equal(1, form.EnvironmentIndex);
            Assert.Equal(0.001f, form.Gravity, 5);
        }

        [Theory]
        [InlineData(0, 0.0005f)]
        [InlineData(1, 0.001f)]
        [InlineData(2, 0.00037f)]
        public void SetGravityFromSelection_EnvIndex_SetsCorrectGravity(int index, float expectedGravity)
        {
            var form = new Form1();
            form.EnvironmentIndex = index;
            Assert.Equal(expectedGravity, form.Gravity, 5);
        }
    }
}
