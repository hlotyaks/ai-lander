using System;
using System.Drawing;

namespace LanderGame
{
    public class Terrain
    {
        // total precomputed points, only first "visibleCount" are drawn
        private const int InitialTotalPoints = 200;
        private const int ExtendThresholdPoints = 100;
        private const int ExtendPointsCount = 200;
        private readonly int visibleCount;
        private float segmentWidth;
        public PointF[] Points { get; private set; } = Array.Empty<PointF>();
        public int Segments => visibleCount;
        public float Variation { get; }
        public float BaseY { get; }

        public Terrain(int visiblePoints, float variation, float baseY)
        {
            visibleCount = visiblePoints;
            Variation = variation;
            BaseY = baseY;
        }

        public void Generate(Random rng, float width, float screenHeight)
        {
            // compute spacing matching Form1’s segment width (width/visibleCount)
            segmentWidth = width / visibleCount;
            Points = new PointF[InitialTotalPoints];
            for (int i = 0; i < Points.Length; i++)
            {
                // X position matches original segment width
                float tx = i * segmentWidth;
                // variation only above baseline so terrain stays near bottom
                float ty = BaseY + (float)(rng.NextDouble() * 2 - 1) * Variation;
                // clamp so terrain never goes above top or below baseline
                ty = Math.Clamp(ty, 0f, BaseY);
                Points[i] = new PointF(tx, ty);
            }
        }

        public void ExpandIfNeeded(float cameraX)
        {
            int visibleLastIdx = (int)((cameraX + (visibleCount - 1) * segmentWidth) / segmentWidth);
            if (Points.Length - visibleLastIdx <= ExtendThresholdPoints)
                Expand(ExtendPointsCount);
        }

        // Grow the terrain by extraPoints at the end
        private void Expand(int extraPoints)
        {
            var rng = new Random();
            int oldCount = Points.Length;
            int newCount = oldCount + extraPoints;
            var newPoints = new PointF[newCount];
            Array.Copy(Points, newPoints, oldCount);
            // start X from last existing point
            float lastX = Points[oldCount - 1].X;
            for (int i = oldCount; i < newCount; i++)
            {
                float tx = lastX + segmentWidth;
                lastX = tx;
                float ty = BaseY + (float)(rng.NextDouble() * 2 - 1) * Variation;
                // clamp so terrain never goes above top or below baseline
                ty = Math.Clamp(ty, 0f, BaseY);
                newPoints[i] = new PointF(tx, ty);
            }
            Points = newPoints;
        }

        /// <summary>Flattens terrain between startIdx and startIdx+padSegments inclusive.</summary>
        public void Flatten(int startIdx, int padSegments)
        {
            int endIdx = Math.Min(startIdx + padSegments, Points.Length - 1);
            if (startIdx < 0 || startIdx >= Points.Length) return;
            float padY = (Points[startIdx].Y + Points[endIdx].Y) / 2;
            for (int i = startIdx; i <= endIdx; i++)
                Points[i].Y = padY;
        }

        /// <summary>Flattens terrain under given startX and width in world units.</summary>
        public void FlattenAt(float startX, float width)
        {
            // compute indices based on segmentWidth
            int startIdx = (int)Math.Round(startX / segmentWidth);
            int count = (int)Math.Round(width / segmentWidth);
            // reuse existing Flatten logic
            Flatten(startIdx, count);
        }

        public void Draw(Graphics g, float cameraX, float wrapWidth)
        {
            // Expand terrain if approaching end
            ExpandIfNeeded(cameraX);
            if (Points == null || Points.Length == 0)
                return;
            // extract only visible points
            int startIdx = Math.Clamp((int)(cameraX / segmentWidth), 0, Points.Length - visibleCount);
            var visiblePoints = new PointF[visibleCount];
            Array.Copy(Points, startIdx, visiblePoints, 0, visibleCount);
            using var terrainPen = new Pen(Color.Gray, 2);
            // draw continuous lines for visible terrain
            g.DrawLines(terrainPen, visiblePoints);
        }

        /// <summary>Interpolates terrain height at given world X position.</summary>
        public float GetHeightAt(float xPos)
        {
            if (Points == null || Points.Length < 2)
                return BaseY;
            // clamp X to terrain bounds
            float pos = Math.Max(0, xPos);
            // find segment index
            int idx = (int)(pos / segmentWidth);
            idx = Math.Clamp(idx, 0, Points.Length - 2);
            PointF p0 = Points[idx];
            PointF p1 = Points[idx + 1];
            float t = (pos - p0.X) / (p1.X - p0.X);
            return p0.Y + t * (p1.Y - p0.Y);
        }        /// <summary>
        /// Finds a suitable location for a landing pad of the given width.
        /// Returns the X coordinate where the pad should be placed, or -1 if no suitable location found.
        /// </summary>
        public float FindSuitableLandingPadLocation(float padWidth, float minX, float maxX)
        {
            float segW = segmentWidth;
            int padSegs = Math.Max(1, (int)Math.Ceiling(padWidth / segW));
            
            // Try to find existing flat areas first
            for (float testX = minX; testX <= maxX - padWidth; testX += segW)
            {
                if (IsAreaSuitableForPad(testX, padWidth, padSegs))
                {
                    return testX;
                }
            }
            
            // If no suitable flat area found, find the least mountainous area to flatten
            float bestX = -1;
            float minVariation = float.MaxValue;
            
            for (float testX = minX; testX <= maxX - padWidth; testX += segW)
            {
                float variation = GetTerrainVariationInArea(testX, padWidth);
                if (variation < minVariation)
                {
                    minVariation = variation;
                    bestX = testX;
                }
            }
            
            return bestX;
        }
        
        /// <summary>
        /// Checks if an area is already suitable for a landing pad (relatively flat).
        /// </summary>
        private bool IsAreaSuitableForPad(float startX, float padWidth, int padSegs)
        {
            int startIdx = (int)Math.Round(startX / segmentWidth);
            int endIdx = Math.Min(startIdx + padSegs, Points.Length - 1);
            
            if (startIdx < 0 || endIdx >= Points.Length) return false;
            
            // Check if the area is already relatively flat
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            
            for (int i = startIdx; i <= endIdx; i++)
            {
                minY = Math.Min(minY, Points[i].Y);
                maxY = Math.Max(maxY, Points[i].Y);
            }
            
            // Consider flat if height variation is less than 10 pixels
            return (maxY - minY) <= 10f;
        }
        
        /// <summary>
        /// Gets the height variation in a terrain area.
        /// </summary>
        private float GetTerrainVariationInArea(float startX, float width)
        {
            int startIdx = (int)Math.Round(startX / segmentWidth);
            int endIdx = (int)Math.Round((startX + width) / segmentWidth);
            endIdx = Math.Min(endIdx, Points.Length - 1);
            
            if (startIdx < 0 || startIdx >= Points.Length) return float.MaxValue;
            
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            
            for (int i = startIdx; i <= endIdx; i++)
            {
                minY = Math.Min(minY, Points[i].Y);
                maxY = Math.Max(maxY, Points[i].Y);
            }
            
            return maxY - minY;
        }
    }
}
