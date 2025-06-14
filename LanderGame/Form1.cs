using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace LanderGame
{
    public partial class Form1 : Form
    {
        // Object‐oriented game state
        private Lander lander = null!;   // initialized in Form1_Load
        private Terrain terrain = null!; // initialized in Form1_Load
        private List<LandingPad> pads = new List<LandingPad>();
        private bool thrusting, rotatingLeft, rotatingRight;
        private bool gameOver;
        private string crashReason = string.Empty;
        private bool landedSuccess;
        private bool paused;
        private float gravity;
        private const float terrainHeight = 20f;
        private int terrainSegments = 40;
        private float terrainVariation = 500f;
        private float cameraX;
        private float scrollMargin = 500f;
        private List<(PointF start, PointF end, float vx, float vy)> debris = new();
        private const int blinkIntervalMs = 500;
        // conversion factor from radians to degrees
        private const float RadToDeg = 180f / (float)Math.PI;
        private const float LandingAngleToleranceDeg = 15f; // acceptable landing angle in degrees
        private const float MaxLandingSpeed = 0.5f; // max speed for successful landing
        private List<PointF> stars = new List<PointF>();
        private const int StarCount = 100;

        public Form1()
        {
            InitializeComponent();
            // Start in full screen
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            // Set initial gravity based on UI selection
            SetGravityFromSelection();
            // Hook up rendering
            this.Load += Form1_Load;
            this.Paint += Form1_Paint;
        }

        // Set gravity based on chosen environment
        internal void SetGravityFromSelection()
        {
            switch (envComboBox.SelectedIndex)
            {
                case 0: gravity = 0.0005f; break; // Moon
                case 1: gravity = 0.001f; break;  // Earth
                case 2: gravity = 0.00037f; break; // Mars
            }
        }

        private void envComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetGravityFromSelection();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Toggle pause on Escape (only during active play)
            if (!gameOver && e.KeyCode == Keys.Escape)
            {
                paused = !paused;
                if (paused) gameTimer.Stop(); else gameTimer.Start();
                Invalidate();
                return;
            }
         // Quit on 'X' after game over, successful landing, or pause menu
         if ((gameOver || paused) && e.KeyCode == Keys.X)
         {
             this.Close();
             return;
         }
         // Restart on 'R' after game over, successful landing, or pause menu
         if ((gameOver || paused) && e.KeyCode == Keys.R)
         {
             ResetGame();
             return;
         }
         // Start the game on first input
         if (!gameTimer.Enabled && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right))
             gameTimer.Start();

         if (e.KeyCode == Keys.Up) thrusting = true;
         if (e.KeyCode == Keys.Left) rotatingLeft = true;
         if (e.KeyCode == Keys.Right) rotatingRight = true;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up) thrusting = false;
            if (e.KeyCode == Keys.Left) rotatingLeft = false;
            if (e.KeyCode == Keys.Right) rotatingRight = false;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            // initialize lander and terrain now that ClientSize is set
            lander = new Lander(ClientSize.Width/2, 50);
            terrain = new Terrain(terrainSegments, terrainVariation, ClientSize.Height - terrainHeight);
            pads.Clear();
            // Generate jagged terrain and instantiate landing pad
            var rng = new Random();
            // Build terrain
            float segW = ClientSize.Width / (float)terrainSegments;
            terrain.Generate(rng, ClientSize.Width, ClientSize.Height);
            // generate initial star field
            GenerateStars(rng);
            // Choose pad segment count and location
            int padSegs = Math.Clamp((int)Math.Round(60f / segW), 1, terrainSegments);
            int startIdx = rng.Next(0, terrainSegments - padSegs + 1);
            // Flatten terrain under pad
            terrain.Flatten(startIdx, padSegs);
            int endIdx = startIdx + padSegs;
            float padY = (terrain.Points[startIdx].Y + terrain.Points[endIdx].Y) / 2;
            // Instantiate initial landing pad
            float padXCoord = startIdx * segW;
            float padW = padSegs * segW;
            var initialPad = new LandingPad(padXCoord, padW, padY, blinkIntervalMs);
            pads.Add(initialPad);
            // Pause simulation until first input
            gameTimer.Stop();
        }

        /// <summary>For testing: initialize game state as if loaded.</summary>
        internal void InitializeForTest()
        {
            Form1_Load(this, EventArgs.Empty);
        }

        /// <summary>For testing: advance game by one timer tick.</summary>
        internal void Tick()
        {
            gameTimer_Tick(this, EventArgs.Empty);
        }

        // Expose terrain height lookup for testing
        internal float GetTerrainYAt(float xPos)
        {
            return terrain.GetHeightAt(xPos);
        }
        // Expose internal state for testing
        internal Lander LanderInstance => lander;
        internal IReadOnlyList<LandingPad> Pads => pads;
        internal LandingPad CurrentPad => pads.Last();
        internal bool LandedSuccessFlag => landedSuccess;

        // Expose gravity for testing
        internal float Gravity => gravity;
        // Expose environment selection for testing
        internal int EnvironmentIndex
        {
            get => envComboBox.SelectedIndex;
            set => envComboBox.SelectedIndex = value;
        }

        // Expose crash reason for testing
        internal string CrashReason => crashReason;
        private void gameTimer_Tick(object sender, EventArgs e)
        {
            float delta = gameTimer.Interval;

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
                    Invalidate();
                    return;
                }
            }
            else
            {
                // Normal flight: apply physics update
                lander.Update(delta, thrusting, rotatingLeft, rotatingRight, gravity);
            }

            float x = lander.X, y = lander.Y, vx = lander.Vx, vy = lander.Vy;

            // Terrain collision and landing/crash handling
            float segW = ClientSize.Width / (float)terrainSegments;
            float terrainY = terrain.GetHeightAt(x);
            if (!gameOver && vy >= 0f && y + 20 >= terrainY)
            {
                // clamp to surface and stop motion
                lander.SetState(x, terrainY - 20f, lander.Angle, 0f, 0f);                // find unused pad under craft
                var pad = pads.FirstOrDefault(p => !p.IsUsed && x >= p.X && x <= p.X + p.Width);
                // landing success check
                if (pad != null
                    && Math.Abs(lander.Vx) <= MaxLandingSpeed
                    && Math.Abs(lander.Vy) <= MaxLandingSpeed
                    && Math.Abs(lander.Angle * RadToDeg) <= LandingAngleToleranceDeg)
                {
                    // Successful landing: refuel and spawn next pad
                    lander.Refuel();
                    // Ensure lander is completely stationary after landing
                    lander.SetState(lander.X, lander.Y, lander.Angle, 0f, 0f);
                    pad.StopBlinking();
                    landedSuccess = true;
                    var rnd = new Random();
                    int offset = rnd.Next(100, 201);
                    float nextX = pad.X + offset * segW;
                    float nextW = pad.Width;
                    terrain.FlattenAt(nextX, nextW);
                    float nextY = terrain.GetHeightAt(nextX);
                    var newPad = new LandingPad(nextX, nextW, nextY, blinkIntervalMs);
                    pads.Add(newPad);
                    return;
                }
                // Crash handling
                gameOver = true;
                if (pad == null)
                    crashReason = "Crashed: no pad";
                else if (Math.Abs(lander.Vx) > MaxLandingSpeed || Math.Abs(lander.Vy) > MaxLandingSpeed)
                    crashReason = "Crashed: excessive speed";
                else if (x < pad.X || x > pad.X + pad.Width)
                    crashReason = "Crashed: missed pad";
                else if (Math.Abs(lander.Angle * RadToDeg) > LandingAngleToleranceDeg)
                    crashReason = "Crashed: bad angle";
                else
                    crashReason = "Crashed";
                // generate debris
                var rng2 = new Random();
                for (int i = 0; i < 30; i++)
                {
                    var a2 = (float)(rng2.NextDouble() * 2 * Math.PI);
                    var start = new PointF(x, y);
                    var end = new PointF(x + (float)Math.Cos(a2) * 10, y + (float)Math.Sin(a2) * 10);
                    debris.Add((start, end, (end.X - start.X) * 0.1f, (end.Y - start.Y) * 0.1f));
                }
                return;
            }
            // Camera follow (only during flight)
            if (!gameOver)
            {
                if (x - cameraX < scrollMargin)
                    cameraX = Math.Max(0, x - scrollMargin);
                else if (x - cameraX > ClientSize.Width - scrollMargin)
                    cameraX = x - (ClientSize.Width - scrollMargin);
            }
            // Always redraw to show landing result
            Invalidate();

            // Update debris pieces
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
        }  // end of gameTimer_Tick

        // Reset game state for new play
        private void ResetGame()
        {
            lander.Reset(ClientSize.Width/2,50);
            thrusting=rotatingLeft=rotatingRight=false;
            gameOver=landedSuccess=false; cameraX=0f;
            debris.Clear();
            pads.Clear();
            // regenerate terrain and pad
            var rng=new Random(); float segW=ClientSize.Width/terrainSegments;
            int padSegs=Math.Clamp(1,(int)Math.Round(60/segW),terrainSegments);
            int start=rng.Next(0,terrainSegments-padSegs+1);
            terrain.Generate(rng, ClientSize.Width, ClientSize.Height);
            terrain.Flatten(start, padSegs);
            int endIdx2=start+padSegs; float padY2=(terrain.Points[start].Y+terrain.Points[endIdx2].Y)/2;
            // instantiate new pad
            var newPad = new LandingPad(start*segW,padSegs*segW,padY2,blinkIntervalMs);
            pads.Add(newPad);
            // regenerate stars
            GenerateStars(new Random());
            SetGravityFromSelection();
            gameTimer.Stop();
            // Force redraw so screen resets immediately
            Invalidate();
        }

        /// <summary>Randomly place stars above the terrain profile.</summary>
        private void GenerateStars(Random rng)
        {
            stars.Clear();
            float screenH = ClientSize.Height;
            for (int i = 0; i < StarCount; i++)
            {
                float x;
                float y;
                // ensure star is above terrain
                do
                {
                    x = (float)(rng.NextDouble() * terrain.Points[^1].X);
                    y = (float)(rng.NextDouble() * (screenH - terrainHeight));
                } while (y >= terrain.GetHeightAt(x));
                stars.Add(new PointF(x, y));
            }
        }

        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);
            // Apply camera offset for world rendering
            g.ResetTransform();
            g.TranslateTransform(-cameraX, 0);
            // draw star field
            foreach (var star in stars)
                g.FillRectangle(Brushes.White, star.X, star.Y, 2, 2);
            float wrapWidth = ClientSize.Width; // width used for terrain and pad tiling

            // Draw terrain
            terrain.Draw(g, cameraX, wrapWidth);

            // Draw all landing pads
            foreach (var pad in pads)
                pad.Draw(g);

            // Draw debris explosion on crash
            if (gameOver && debris.Count > 0)
            {
                using var debrisPen = new Pen(Color.Orange, 2);
                foreach (var (start, end, _, _) in debris)
                    g.DrawLine(debrisPen, start, end);
            }

            // Draw lander
            if(!gameOver) lander.Draw(g,thrusting);

            // Reset transform for HUD (screen space)
            g.ResetTransform();

            // HUD: velocity, altitude, fuel next to ship (only when not crashed)
            if (!gameOver)
            {
                float craftHalfW = 10f, craftHalfH = 20f, hudMargin = 10f;
                float screenX = lander.X - cameraX, screenY = lander.Y;
                float hudX = screenX + craftHalfW + hudMargin;
                float hudY = screenY - craftHalfH;
                var velText = $"Vx:{lander.Vx:0.00} Vy:{lander.Vy:0.00}";
                g.DrawString(velText, this.Font, Brushes.White, hudX, hudY);
                hudY += this.Font.Height + 5;
                // display lander angle in degrees
                var angleText = $"Ang:{lander.Angle * RadToDeg:0.00}°";
                g.DrawString(angleText, this.Font, Brushes.White, hudX, hudY);
                hudY += this.Font.Height + 5;
                var altText = $"Alt:{ClientSize.Height - lander.Y:0.00}";
                g.DrawString(altText, this.Font, Brushes.White, hudX, hudY);
                hudY += this.Font.Height + 5;
                var fuelText = $"Fuel:{lander.Fuel:0.00}";
                g.DrawString(fuelText, this.Font, Brushes.White, hudX, hudY);
            }

            // Draw game over or success message
            if (gameOver)
            {
                using (var font = new Font("Arial", 24))
                using (var brush = new SolidBrush(Color.Red))
                {
                    g.DrawString("Game Over", font, brush, ClientSize.Width / 2 - 60, ClientSize.Height / 2 - 30);
                    // Restart and quit prompts
                    g.DrawString("Press R to restart", font, Brushes.White,
                        ClientSize.Width / 2 - 100, ClientSize.Height / 2);
                    g.DrawString("Press X to quit", font, Brushes.White,
                        ClientSize.Width / 2 - 80, ClientSize.Height / 2 + 30);
                    // display crash reason
                    g.DrawString(crashReason, font, Brushes.White,
                        ClientSize.Width / 2 - 100, ClientSize.Height / 2 + 60);
                }
            }
            // Pause menu overlay
            if (paused)
            {
                using (var font = new Font("Arial", 24))
                using (var brush = new SolidBrush(Color.Yellow))
                {
                    var cx = ClientSize.Width / 2f;
                    var cy = ClientSize.Height / 2f;
                    g.DrawString("Paused", font, brush, cx - 50, cy - 60);
                    g.DrawString("Press R to restart", font, Brushes.White, cx - 100, cy - 20);
                    g.DrawString("Press X to quit", font, Brushes.White, cx - 80, cy + 20);
                }
            }
        } // end Form1_Paint
    }
}
