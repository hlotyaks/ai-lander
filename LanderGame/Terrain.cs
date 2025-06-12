using System;
using System.Drawing;

namespace LanderGame
{
    public class Terrain
    {
        public PointF[] Points { get; private set; } = Array.Empty<PointF>();
        public int Segments { get; }
        public float Variation { get; }
        public float BaseY { get; }

        public Terrain(int segments, float variation, float baseY)
        {
            Segments = segments;
            Variation = variation;
            BaseY = baseY;
        }

        public void Generate(Random rng, float width, float screenHeight)
        {
            Points = new PointF[Segments + 1];
            float segW = width / Segments;
            for (int i = 0; i <= Segments; i++)
            {
                float tx = i * segW;
                // variation only above baseline so terrain stays near bottom
                float ty = BaseY + (float)(rng.NextDouble() * 2 - 1) * Variation;
                // clamp so terrain never goes above top or below BaseY (near bottom)
                ty = Math.Clamp(ty, 0f, BaseY);
                Points[i] = new PointF(tx, ty);
            }
        }

        public void Flatten(int startIdx, int padSegments)
        {
            int endIdx = startIdx + padSegments;
            float padY = (Points[startIdx].Y + Points[endIdx].Y) / 2;
            for (int i = startIdx; i <= endIdx; i++)
                Points[i].Y = padY;
        }

        public float GetHeightAt(float xPos, float width)
        {
            if (Points == null || Points.Length == 0)
                return BaseY;
            float segW = width / Segments;
            int idx = (int)(xPos / segW);
            idx = Math.Clamp(idx, 0, Segments - 1);
            PointF p0 = Points[idx];
            PointF p1 = Points[idx + 1];
            float t = (xPos - p0.X) / (p1.X - p0.X);
            return p0.Y + t * (p1.Y - p0.Y);
        }

        public void Draw(Graphics g, float cameraX, float wrapWidth)
        {
            if (Points == null || Points.Length == 0)
                return;
            using var terrainPen = new Pen(Color.Gray, 2);
            int baseTile = (int)Math.Floor(cameraX / wrapWidth);
            for (int t = baseTile - 1; t <= baseTile + 1; t++)
            {
                g.DrawLines(terrainPen, Points);
                g.TranslateTransform(wrapWidth, 0);
            }
            g.TranslateTransform(-wrapWidth * 3, 0);
        }
    }
}
