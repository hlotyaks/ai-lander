using Xunit;
using LanderGame;

namespace Tests
{
    public class Form1Tests
    {       [Fact]
        public void Gravity_ShouldBeMoonGravity()
        {
            var form = new Form1();
            Assert.Equal(0.0004f, form.Gravity, 5);
        }
    }
}
