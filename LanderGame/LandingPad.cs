using System;
using System.Drawing;
using System.Windows.Forms;

namespace LanderGame
{
    public class LandingPad
    {
        public float X { get; private set; }
        public float Width { get; private set; }
        public float Y { get; private set; }
        public bool LightsVisible { get; private set; }

        private readonly System.Windows.Forms.Timer blinkTimer;

        public LandingPad(float x, float width, float y, int blinkIntervalMs)
        {
            X = x;
            Width = width;
            Y = y;
            LightsVisible = true;
            blinkTimer = new System.Windows.Forms.Timer { Interval = blinkIntervalMs };
            blinkTimer.Tick += (s, e) => ToggleLights();
            blinkTimer.Start();
        }

        private void ToggleLights()
        {
            LightsVisible = !LightsVisible;
        }

        public void StopBlinking()
        {
            blinkTimer.Stop();
            LightsVisible = true;
        }

        public void Draw(Graphics g)
        {
            using var padPen = new Pen(Color.Green, 5);
            g.DrawLine(padPen, X, Y, X + Width, Y);

            if (LightsVisible)
            {
                using var lightBrush = new SolidBrush(Color.Yellow);
                float r = 5f;
                g.FillEllipse(lightBrush, X - r, Y - 2 * r, 2 * r, 2 * r);
                g.FillEllipse(lightBrush, X + Width - r, Y - 2 * r, 2 * r, 2 * r);
            }
        }
    }
}
