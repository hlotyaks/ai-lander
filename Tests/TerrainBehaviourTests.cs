using System;
using Xunit;
using LanderGame;
using System.Drawing;

namespace Tests
{
    public class TerrainBehaviourTests
    {
        [Fact]
        public void ExpandIfNeeded_DoesNotExpand_WhenFarFromEnd()
        {
            var terrain = new Terrain(5, 0f, 100f);
            terrain.Generate(new Random(0), 500f, 200f);
            int initialCount = terrain.Points.Length;
            // cameraX at start should not trigger expansion
            terrain.ExpandIfNeeded(0f);
            Assert.Equal(initialCount, terrain.Points.Length);
        }

        [Fact]
        public void ExpandIfNeeded_Expands_WhenNearEnd()
        {
            var terrain = new Terrain(5, 0f, 100f);
            terrain.Generate(new Random(0), 500f, 200f);
            int initialCount = terrain.Points.Length;
            float segW = 500f / 5f;
            // choose cameraX so visibleLastIdx = initialCount - 100
            float cameraX = (initialCount - 100 - (5 - 1)) * segW;
            terrain.ExpandIfNeeded(cameraX);
            Assert.Equal(initialCount + 200, terrain.Points.Length);
        }

        [Fact]
        public void GetHeightAt_Returns_BaseY_WhenNoVariation()
        {
            var terrain = new Terrain(5, 0f, 150f);
            terrain.Generate(new Random(0), 400f, 300f);
            // all Y values are BaseY (150)
            Assert.Equal(150f, terrain.GetHeightAt(0f));
            Assert.Equal(150f, terrain.GetHeightAt(100f));
            Assert.Equal(150f, terrain.GetHeightAt(1000f));
        }

        [Fact]
        public void GetHeightAt_InterpolatesBetweenPoints()
        {
            var terrain = new Terrain(5, 0f, 200f);
            terrain.Generate(new Random(0), 400f, 300f);
            // manually set first two points to known Y
            float segW = 400f / 5f;
            terrain.Points[0] = new PointF(0f, 50f);
            terrain.Points[1] = new PointF(segW, 150f);
            // midpoint should average to 100
            float midX = segW / 2f;
            Assert.Equal(100f, terrain.GetHeightAt(midX), 3);
        }

        [Fact]
        public void ExpandIfNeeded_MultipleExpansions_AddsMorePointsEachTime()
        {
            var terrain = new Terrain(5, 0f, 100f);
            terrain.Generate(new Random(1), 500f, 200f);
            int count1 = terrain.Points.Length;
            float segW = 500f / 5f;
            float cameraX = (count1 - 100 - 4) * segW;
            terrain.ExpandIfNeeded(cameraX);
            int count2 = terrain.Points.Length;
            cameraX = (count2 - 100 - 4) * segW;
            terrain.ExpandIfNeeded(cameraX);
            int count3 = terrain.Points.Length;
            Assert.Equal(count1 + 200, count2);
            Assert.Equal(count2 + 200, count3);
        }

        [Fact]
        public void Expand_KeepsXSpacingConstant()
        {
            var terrain = new Terrain(5, 0f, 100f);
            terrain.Generate(new Random(2), 500f, 200f);
            int originalCount = terrain.Points.Length;
            float segW = 500f / 5f;
            float cameraX = (originalCount - 100 - 4) * segW;
            terrain.ExpandIfNeeded(cameraX);
            var pts = terrain.Points;
            for (int i = 1; i < pts.Length; i++)
            {
                Assert.Equal(segW, pts[i].X - pts[i - 1].X, 3);
            }
        }

        [Fact]
        public void FlattenAt_Works_Correctly_OnRange()
        {
            var terrain = new Terrain(20, 0f, 300f);
            float width = 400f;
            terrain.Generate(new Random(3), width, 400f);
            // set specific heights
            for (int i = 10; i < 15; i++) terrain.Points[i] = new PointF(terrain.Points[i].X, i * 10);
            float startX = terrain.Points[10].X;
            float flattenWidth = (15 - 10) * (width / 20f);
            terrain.FlattenAt(startX, flattenWidth);
            float y0 = terrain.Points[10].Y;
            for (int i = 10; i <= 15; i++) Assert.Equal(y0, terrain.Points[i].Y);
        }
    }
}
