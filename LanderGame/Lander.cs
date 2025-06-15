using System;
using System.Drawing;

namespace LanderGame
{    public class Lander
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
        
        // Thrust plume growth tracking
        private float thrustDuration = 0f;
        private const float MaxThrustDuration = 2000f; // 2 seconds in milliseconds
        private const int FlameStages = 5;

        public Lander(float startX, float startY, float startFuel = 100f)
        {
            maxFuel = startFuel;
            Reset(startX, startY, startFuel);
        }

        /// <summary>Refills fuel to maximum capacity.</summary>
        public void Refuel()
        {
            Fuel = maxFuel;
        }        public void Reset(float startX, float startY, float startFuel = 100f)
        {
            X = startX;
            Y = startY;
            Vx = Vy = Angle = 0f;
            Fuel = startFuel;
            thrustDuration = 0f;
        }        public void Update(float delta, bool thrusting, bool rotatingLeft, bool rotatingRight, float gravity)
        {
            if (rotatingLeft) Angle -= rotationSpeed * delta;
            if (rotatingRight) Angle += rotationSpeed * delta;

            // Update thrust duration for flame growth
            if (thrusting && Fuel > 0f)
            {
                thrustDuration += delta;
                if (thrustDuration > MaxThrustDuration) thrustDuration = MaxThrustDuration;
                
                Vx += (float)Math.Sin(Angle) * thrustPower * delta;
                Vy += -(float)Math.Cos(Angle) * thrustPower * delta;
                Fuel -= fuelConsumptionRate * delta;
                if (Fuel < 0f) Fuel = 0f;
            }
            else
            {
                thrustDuration = 0f; // Reset when not thrusting
            }

            Vy += gravity * delta;
            X += Vx * delta;
            Y += Vy * delta;
        }public void Draw(Graphics g, bool thrusting)
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
        }        /// <summary>Returns the flame polygon if thrusting and fuel available; otherwise null.</summary>
        public PointF[]? GetFlamePolygon(bool thrusting)
        {
            if (!thrusting || Fuel <= 0f)
                return null;
                  // Calculate flame length based on thrust duration (5 stages over 2 seconds)
            // Lander height is about 32 pixels (-20 to +12), so max flame = 32 pixels
            float progress = thrustDuration / MaxThrustDuration; // 0.0 to 1.0
            int stage = Math.Min((int)(progress * FlameStages), FlameStages - 1); // 0 to 4
            
            // Base flame lengths for each stage (reaching 32 pixels at stage 4)
            float[] stageLengths = { 8f, 14f, 20f, 26f, 32f };
            float flameLength = stageLengths[stage];
            
            // Add slight randomness for visual effect
            var rand = new Random();
            float randomVariation = rand.Next(-2, 3); // -2 to +2 pixels
            flameLength += randomVariation;
            
            // Ensure minimum length
            if (flameLength < 5f) flameLength = 5f;
            
            return new PointF[] 
            { 
                new PointF(-5, 12), // Left base (bottom of lander)
                new PointF(0, 12 + flameLength), // Tip
                new PointF(5, 12) // Right base
            };
        }        /// <summary>For testing: set internal state directly.</summary>
        internal void SetState(float x, float y, float angle, float vx, float vy)
        {
            X = x;
            Y = y;
            Angle = angle;
            Vx = vx;
            Vy = vy;
        }
        
        /// <summary>For testing: get current thrust duration.</summary>
        internal float GetThrustDuration() => thrustDuration;
        
        /// <summary>For testing: set thrust duration directly.</summary>
        internal void SetThrustDuration(float duration) => thrustDuration = duration;
    }
}
