using Xunit;
using LanderGame;
using System.Drawing;

namespace Tests
{
    public class LanderDrawTests
    {
        [Fact]
        public void GetShipPolygon_ReturnsExpectedTriangle()
        {
            var lander = new Lander(0, 0, 100f);
            var poly = lander.GetShipPolygon();
            Assert.Equal(3, poly.Length);
            Assert.Equal(new PointF(0, -20), poly[0]);
            Assert.Equal(new PointF(-10, 20), poly[1]);
            Assert.Equal(new PointF(10, 20), poly[2]);
        }

        [Fact]
        public void GetFlamePolygon_NotThrusting_ReturnsNull()
        {
            var lander = new Lander(0, 0, 100f);
            var flame = lander.GetFlamePolygon(false);
            Assert.Null(flame);
        }

        [Fact]
        public void GetFlamePolygon_ThrustingButNoFuel_ReturnsNull()
        {
            var lander = new Lander(0, 0, 0f);
            var flame = lander.GetFlamePolygon(true);
            Assert.Null(flame);
        }

        [Fact]
        public void GetFlamePolygon_ThrustingWithFuel_ReturnsValidFlame()
        {
            var lander = new Lander(0, 0, 100f);
            var flame = lander.GetFlamePolygon(true);
            Assert.NotNull(flame);
            Assert.Equal(3, flame.Length);
            // flame tips at fixed points
            Assert.Equal(new PointF(-5, 20), flame[0]);
            Assert.Equal(new PointF(5, 20), flame[2]);
            // middle point X should be 0, Y should be between 25 and 35
            Assert.Equal(0f, flame[1].X);
            Assert.InRange(flame[1].Y, 25f, 35f);
        }
    }
}
