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
        }        [Fact]
        public void GetFlamePolygon_ThrustingWithFuel_ReturnsValidFlame()
        {
            var lander = new Lander(0, 0, 100f);
            var flame = lander.GetFlamePolygon(true);
            Assert.NotNull(flame);
            Assert.Equal(3, flame.Length);
            
            // Flame base should be at bottom of lander (Y=12)
            Assert.Equal(new PointF(-5, 12), flame[0]); // Left base
            Assert.Equal(new PointF(5, 12), flame[2]);  // Right base
            
            // Tip should be centered and extend downward from base
            Assert.Equal(0f, flame[1].X);
            Assert.True(flame[1].Y > 12f, "Flame tip should extend below lander base");
            
            // For initial flame (stage 0), length should be around 8 pixels (plus random variation)
            float flameLength = flame[1].Y - flame[0].Y;
            Assert.InRange(flameLength, 5f, 15f); // Allow for random variation
        }

        [Fact]
        public void GetFlamePolygon_GrowsWithThrustDuration()
        {
            var lander = new Lander(0, 0, 100f);
            
            // Test different thrust durations and verify flame grows
            lander.SetThrustDuration(0f); // Stage 0
            var flame0 = lander.GetFlamePolygon(true);
            Assert.NotNull(flame0);
              lander.SetThrustDuration(1000f); // Stage 2 (middle)
            var flame2 = lander.GetFlamePolygon(true);
            Assert.NotNull(flame2);
            
            lander.SetThrustDuration(2000f); // Stage 4 (maximum)
            var flame4 = lander.GetFlamePolygon(true);
            Assert.NotNull(flame4);
            
            // Calculate flame lengths (accounting for random variation)
            float length0 = flame0![1].Y - flame0[0].Y;
            float length2 = flame2![1].Y - flame2[0].Y;
            float length4 = flame4![1].Y - flame4[0].Y;
            
            // Progressive growth (allowing for random variation)
            Assert.True(length2 >= length0 - 3f, $"Stage 2 flame ({length2}) should be >= stage 0 ({length0}) within tolerance");
            Assert.True(length4 >= length2 - 3f, $"Stage 4 flame ({length4}) should be >= stage 2 ({length2}) within tolerance");
            
            // Maximum flame should be around 32 pixels (lander height)
            Assert.InRange(length4, 25f, 35f);
        }
    }
}
