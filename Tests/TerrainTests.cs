using Xunit;
using LanderGame;
using System;

namespace Tests
{
    public class TerrainTests
    {
        [Fact]
        public void Terrain_Initializes_With_Correct_Segments()
        {
            var terrain = new Terrain(10, 100, 400);
            Assert.Equal(10, terrain.Segments);
            Assert.Equal(100, terrain.Variation);
            Assert.Equal(400, terrain.BaseY);
        }

        [Fact]
        public void Terrain_Generate_Creates_Points()
        {
            var terrain = new Terrain(5, 50, 300);
            terrain.Generate(new Random(0), 500, 400);
            // now precomputes a fixed number of points
            Assert.Equal(200, terrain.Points.Length);
            foreach (var pt in terrain.Points)
            {
                // Y is clamped between 0 and BaseY
                Assert.InRange(pt.Y, 0f, 300f);
            }
        }

        [Fact]
        public void Terrain_Flattens_Segment()
        {
            var terrain = new Terrain(5, 50, 300);
            terrain.Generate(new Random(0), 500, 400);
            terrain.Flatten(1, 2);
            float y1 = terrain.Points[1].Y;
            float y2 = terrain.Points[2].Y;
            float y3 = terrain.Points[3].Y;
            Assert.Equal(y1, y2, 2);
            Assert.Equal(y2, y3, 2);
        }
    }
}
