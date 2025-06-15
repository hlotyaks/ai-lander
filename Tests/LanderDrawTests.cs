using Xunit;
using LanderGame;
using System.Drawing;
using System.Linq;

namespace Tests
{
    public class LanderDrawTests
    {        [Fact]
        public void GetShipPolygon_ReturnsExpectedLEMShape()
        {
            var lander = new Lander(0, 0, 100f);
            var poly = lander.GetShipPolygon();
            
            // LEM design should have 10 points forming an octagonal main body
            Assert.Equal(10, poly.Length);
            
            // Check key points: top of command module, and main body corners
            Assert.Equal(new PointF(-3, -20), poly[0]); // Top left of command module
            Assert.Equal(new PointF(3, -20), poly[1]);  // Top right of command module
            Assert.Equal(new PointF(8, -8), poly[3]);   // Right side of descent stage
            Assert.Equal(new PointF(-8, -8), poly[8]);  // Left side of descent stage
            
            // Verify the shape is roughly centered and has reasonable dimensions
            Assert.True(poly.All(p => p.X >= -8 && p.X <= 8), "Ship width should be within reasonable bounds");
            Assert.True(poly.All(p => p.Y >= -20 && p.Y <= 12), "Ship height should be within reasonable bounds");
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
