using System;
using System.Drawing;

namespace LanderGame
{
    public class Lander
    {
        private readonly float maxFuel;
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
            maxFuel = startFuel;
            Reset(startX, startY, startFuel);
        }

        /// <summary>Refills fuel to maximum capacity.</summary>
        public void Refuel()
        {
            Fuel = maxFuel;
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
        }        public void Draw(Graphics g, bool thrusting)
        {
            var worldXform = g.Transform;
            g.TranslateTransform(X, Y);
            g.RotateTransform(Angle * RadToDeg);
            
            using var shipPen = new Pen(Color.White, 2);
            using var legPen = new Pen(Color.LightGray, 1.5f);
            
            // Draw landing legs first (behind main body)
            DrawLandingLegs(g, legPen);
            
            // Draw main body
            g.DrawPolygon(shipPen, GetShipPolygon());
            
            // Draw thrust flame
            var flame = GetFlamePolygon(thrusting);
            if (flame != null)
            {
                using var flamePen = new Pen(Color.Orange, 2);
                g.DrawPolygon(flamePen, flame);
            }
            
            g.Transform = worldXform;
        }        /// <summary>Returns the ship's polygon relative to its origin - Apollo LEM style.</summary>
        public PointF[] GetShipPolygon()
        {
            // Apollo LEM main body - octagonal shape representing command module + descent stage
            return new PointF[] 
            {
                // Command module (top, narrow)
                new PointF(-3, -20),
                new PointF(3, -20),
                new PointF(5, -15),
                
                // Descent stage (wider body)
                new PointF(8, -8),
                new PointF(8, 8),
                new PointF(4, 12),
                new PointF(-4, 12),
                new PointF(-8, 8),
                new PointF(-8, -8),
                new PointF(-5, -15)
            };
        }

        /// <summary>Draws the four landing legs extending from the main body.</summary>
        private void DrawLandingLegs(Graphics g, Pen legPen)
        {
            // Four landing legs extending outward from the descent stage
            // Each leg: strut from body to foot, with landing pad
            
            // Front-right leg
            g.DrawLine(legPen, 6, 6, 14, 16);      // main strut
            g.DrawLine(legPen, 14, 16, 16, 18);    // foot
            g.DrawLine(legPen, 13, 18, 17, 18);    // landing pad
            
            // Front-left leg  
            g.DrawLine(legPen, -6, 6, -14, 16);    // main strut
            g.DrawLine(legPen, -14, 16, -16, 18);  // foot
            g.DrawLine(legPen, -17, 18, -13, 18);  // landing pad
            
            // Back-right leg
            g.DrawLine(legPen, 6, 0, 12, 14);      // main strut
            g.DrawLine(legPen, 12, 14, 14, 16);    // foot
            g.DrawLine(legPen, 11, 16, 15, 16);    // landing pad
            
            // Back-left leg
            g.DrawLine(legPen, -6, 0, -12, 14);    // main strut  
            g.DrawLine(legPen, -12, 14, -14, 16);  // foot
            g.DrawLine(legPen, -15, 16, -11, 16);  // landing pad
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

        /// <summary>For testing: set internal state directly.</summary>
        internal void SetState(float x, float y, float angle, float vx, float vy)
        {
            X = x;
            Y = y;
            Angle = angle;
            Vx = vx;
            Vy = vy;
        }
    }
}
