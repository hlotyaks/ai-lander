using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace LanderGame
{
    public class GameEngine
    {
        // Game state
        private Lander lander = null!;
        private Terrain terrain = null!;
        private List<LandingPad> pads = new List<LandingPad>();
        private bool gameOver;
        private string crashReason = string.Empty;
        private bool landedSuccess;
        private float gravity;
        private float cameraX;
        private List<(PointF start, PointF end, float vx, float vy)> debris = new();
        private List<PointF> stars = new List<PointF>();

        // Game constants
        private const float terrainHeight = 20f;
        private const int terrainSegments = 40;
        private const float terrainVariation = 500f;
        private const float scrollMargin = 500f;
        private const int blinkIntervalMs = 500;
        private const float RadToDeg = 180f / (float)Math.PI;
        private const float LandingAngleToleranceDeg = 15f;
        private const float MaxLandingSpeed = 0.5f;
        private const int StarCount = 100;

        // Input state
        private bool thrusting, rotatingLeft, rotatingRight;

        // Events for UI communication
        public event Action? GameStateChanged;
        public event Action? RequestRedraw;

        // Properties for external access
        public bool IsGameOver => gameOver;
        public string CrashReason => crashReason;
        public bool LandedSuccessFlag => landedSuccess;
        public float CameraX => cameraX;
        public Lander LanderInstance => lander;
        public IReadOnlyList<LandingPad> Pads => pads;
        public LandingPad CurrentPad => pads.Last();
        public IReadOnlyList<PointF> Stars => stars;
        public IReadOnlyList<(PointF start, PointF end, float vx, float vy)> Debris => debris;
        public Terrain TerrainInstance => terrain;

        public void Initialize(int clientWidth, int clientHeight, float selectedGravity)
        {
            // Initialize lander and terrain
            lander = new Lander(clientWidth / 2, 50);
            terrain = new Terrain(terrainSegments, terrainVariation, clientHeight - terrainHeight);
            pads.Clear();
            debris.Clear();
            stars.Clear();

            // Set gravity
            gravity = selectedGravity;

            // Generate terrain and initial landing pad
            var rng = new Random();
            float segW = clientWidth / (float)terrainSegments;
            terrain.Generate(rng, clientWidth, clientHeight);

            // Generate stars
            GenerateStars(rng, clientHeight);

            // Create initial landing pad
            int padSegs = Math.Clamp((int)Math.Round(60f / segW), 1, terrainSegments);
            int startIdx = rng.Next(0, terrainSegments - padSegs + 1);
            terrain.Flatten(startIdx, padSegs);
            int endIdx = startIdx + padSegs;
            float padY = (terrain.Points[startIdx].Y + terrain.Points[endIdx].Y) / 2;
            float padXCoord = startIdx * segW;
            float padW = padSegs * segW;
            var initialPad = new LandingPad(padXCoord, padW, padY, blinkIntervalMs);
            pads.Add(initialPad);

            // Reset game state
            gameOver = landedSuccess = false;
            crashReason = string.Empty;
            cameraX = 0f;
            thrusting = rotatingLeft = rotatingRight = false;

            GameStateChanged?.Invoke();
        }

        public void Reset(int clientWidth, int clientHeight, float selectedGravity)
        {
            Initialize(clientWidth, clientHeight, selectedGravity);
        }

        public void UpdateInput(bool thrust, bool rotLeft, bool rotRight)
        {
            thrusting = thrust;
            rotatingLeft = rotLeft;
            rotatingRight = rotRight;
        }

        public void Tick(float delta, int clientWidth, int clientHeight)
        {
            // Skip physics update if lander is stationary on ground (already landed)
            if (landedSuccess)
            {
                // Check if lander is taking off (user applying thrust)
                if (thrusting || rotatingLeft || rotatingRight)
                {
                    landedSuccess = false; // Reset landed state when taking off
                    // Now apply physics update since we're taking off
                    lander.Update(delta, thrusting, rotatingLeft, rotatingRight, gravity);
                }
                else
                {
                    // Still landed and stationary, skip physics and collision detection
                    RequestRedraw?.Invoke();
                    return;
                }
            }
            else
            {
                // Normal flight: apply physics update
                lander.Update(delta, thrusting, rotatingLeft, rotatingRight, gravity);
            }

            float x = lander.X, y = lander.Y, vx = lander.Vx, vy = lander.Vy;            // Terrain collision and landing/crash handling
            float segW = clientWidth / (float)terrainSegments;
            float terrainY = terrain.GetHeightAt(x);
            if (!gameOver && vy >= 0f && y + 20 >= terrainY)
            {
                // find unused pad under craft
                var pad = pads.FirstOrDefault(p => !p.IsUsed && x >= p.X && x <= p.X + p.Width);
                
                // landing success check (use original velocities before resetting)
                if (pad != null
                    && Math.Abs(vx) <= MaxLandingSpeed
                    && Math.Abs(vy) <= MaxLandingSpeed
                    && Math.Abs(lander.Angle * RadToDeg) <= LandingAngleToleranceDeg)                {
                    // Successful landing: clamp to surface and stop motion
                    lander.SetState(x, terrainY - 20f, lander.Angle, 0f, 0f);
                    
                    // Refuel and mark pad as used
                    lander.Refuel();
                    pad.StopBlinking();
                    landedSuccess = true;
                    
                    // Spawn new pad
                    var rnd = new Random();
                    int offset = rnd.Next(100, 201);
                    float nextX = pad.X + offset * segW;
                    float nextW = pad.Width;
                    terrain.FlattenAt(nextX, nextW);
                    float nextY = terrain.GetHeightAt(nextX);
                    var newPad = new LandingPad(nextX, nextW, nextY, blinkIntervalMs);
                    pads.Add(newPad);
                    
                    GameStateChanged?.Invoke();
                    return;
                }                
                // Crash handling - clamp to surface and stop motion first
                lander.SetState(x, terrainY - 20f, lander.Angle, 0f, 0f);
                gameOver = true;
                if (pad == null)
                    crashReason = "Crashed: no pad";
                else if (Math.Abs(vx) > MaxLandingSpeed || Math.Abs(vy) > MaxLandingSpeed)
                    crashReason = "Crashed: excessive speed";
                else if (x < pad.X || x > pad.X + pad.Width)
                    crashReason = "Crashed: missed pad";
                else if (Math.Abs(lander.Angle * RadToDeg) > LandingAngleToleranceDeg)
                    crashReason = "Crashed: bad angle";
                else
                    crashReason = "Crashed";
                
                // generate debris
                GenerateDebris(x, y);
                GameStateChanged?.Invoke();
                return;
            }

            // Camera follow (only during flight)
            if (!gameOver)
            {
                if (x - cameraX < scrollMargin)
                    cameraX = Math.Max(0, x - scrollMargin);
                else if (x - cameraX > clientWidth - scrollMargin)
                    cameraX = x - (clientWidth - scrollMargin);
            }

            // Update debris pieces
            UpdateDebris(delta);

            RequestRedraw?.Invoke();
        }

        private void GenerateStars(Random rng, int clientHeight)
        {
            stars.Clear();
            for (int i = 0; i < StarCount; i++)
            {
                float x;
                float y;
                // ensure star is above terrain
                do
                {
                    x = (float)(rng.NextDouble() * terrain.Points[^1].X);
                    y = (float)(rng.NextDouble() * (clientHeight - terrainHeight));
                } while (y >= terrain.GetHeightAt(x));
                stars.Add(new PointF(x, y));
            }
        }

        private void GenerateDebris(float x, float y)
        {
            var rng = new Random();
            for (int i = 0; i < 30; i++)
            {
                var a = (float)(rng.NextDouble() * 2 * Math.PI);
                var start = new PointF(x, y);
                var end = new PointF(x + (float)Math.Cos(a) * 10, y + (float)Math.Sin(a) * 10);
                debris.Add((start, end, (end.X - start.X) * 0.1f, (end.Y - start.Y) * 0.1f));
            }
        }

        private void UpdateDebris(float delta)
        {
            if (debris.Count > 0)
            {
                for (int i = debris.Count - 1; i >= 0; i--)
                {
                    var (start, end, vx0, vy0) = debris[i];
                    // update positions
                    start.X += vx0 * delta;
                    start.Y += vy0 * delta;
                    end.X += vx0 * delta;
                    end.Y += vy0 * delta;
                    // apply gravity to vy
                    vy0 += gravity * delta;
                    debris[i] = (start, end, vx0, vy0);
                }
            }
        }

        // Helper methods for testing
        public float GetTerrainYAt(float xPos) => terrain.GetHeightAt(xPos);
    }
}
