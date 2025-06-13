using System;
using System.Drawing;

namespace LanderGame
{
    public class Lander
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Vx { get; private set; }
        public float Vy { get; private set; }
        public float Angle { get; private set; }
        public float Fuel { get; private set; }

        private readonly float thrustPower = 0.002f;
        private readonly float rotationSpeed = 0.005f;
        private readonly float fuelConsumptionRate = 0.02f;
        private const float RadToDeg = 180f / (float)Math.PI;

        public Lander(float startX, float startY, float startFuel = 100f)
        {
            Reset(startX, startY, startFuel);
        }

        public void Reset(float startX, float startY, float startFuel = 100f)
        {
            X = startX;
            Y = startY;
            Vx = Vy = Angle = 0f;
            Fuel = startFuel;
        }

        public void Update(float delta, bool thrusting, bool rotatingLeft, bool rotatingRight, float gravity)
        {
            if (rotatingLeft) Angle -= rotationSpeed * delta;
            if (rotatingRight) Angle += rotationSpeed * delta;

            if (thrusting && Fuel > 0f)
            {
                Vx += (float)Math.Sin(Angle) * thrustPower * delta;
                Vy += -(float)Math.Cos(Angle) * thrustPower * delta;
                Fuel -= fuelConsumptionRate * delta;
                if (Fuel < 0f) Fuel = 0f;
            }

            Vy += gravity * delta;
            X += Vx * delta;
            Y += Vy * delta;
        }

        public void Draw(Graphics g, bool thrusting)
        {
            var worldXform = g.Transform;
            g.TranslateTransform(X, Y);
            g.RotateTransform(Angle * RadToDeg);
            using var shipPen = new Pen(Color.White, 2);
            g.DrawPolygon(shipPen, GetShipPolygon());
            var flame = GetFlamePolygon(thrusting);
            if (flame != null)
            {
                using var flamePen = new Pen(Color.Orange, 2);
                g.DrawPolygon(flamePen, flame);
            }
            g.Transform = worldXform;
        }

        /// <summary>Returns the ship's polygon relative to its origin.</summary>
        public PointF[] GetShipPolygon()
        {
            return new PointF[] { new PointF(0, -20), new PointF(-10, 20), new PointF(10, 20) };
        }

        /// <summary>Returns the flame polygon if thrusting and fuel available; otherwise null.</summary>
        public PointF[]? GetFlamePolygon(bool thrusting)
        {
            if (!thrusting || Fuel <= 0f)
                return null;
            // simple flame shape
            var rand = new Random();
            return new PointF[] { new PointF(-5, 20), new PointF(0, 20 + rand.Next(5, 15)), new PointF(5, 20) };
        }
    }
}
