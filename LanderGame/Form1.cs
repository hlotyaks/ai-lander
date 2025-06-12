using System;
using System.Drawing;
using System.Windows.Forms;

namespace LanderGame
{
    public partial class Form1 : Form
    {
        // Game state fields
        private float x, y, vx, vy, angle;
        private bool thrusting, rotatingLeft, rotatingRight;
        private float gravity, thrustPower = 0.002f, rotationSpeed = 0.005f;
        private float terrainHeight = 20f;
        private bool gameOver = false;
        private float padWidth = 60f; // widened landing pad for visibility
        private float padX;
        private bool landedSuccess = false;
        private float fuel = 100f;
        private float fuelConsumptionRate = 0.02f;
        private PointF[] terrainPoints = Array.Empty<PointF>();
        private int terrainSegments = 40;
        private float terrainVariation = 500f;
        private float cameraX = 0f;
        private float scrollMargin = 500f;
        private List<(PointF start, PointF end, float vx, float vy)> debris = new List<(PointF, PointF, float, float)>();
        // blink logic removed
        private System.Windows.Forms.Timer blinkTimer;
        private int blinkIntervalMs = 500;
        private bool showPadLights = true;

        public Form1()
        {
            InitializeComponent();
            // Start in full screen
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            // Initialize lander position and physics
            x = ClientSize.Width / 2;
            y = 50;
            vx = vy = angle = 0;
            // Set initial gravity based on UI selection
            SetGravityFromSelection();
            // Hook up rendering
            this.Load += Form1_Load;
            this.Paint += Form1_Paint;
            // init blinking pad lights timer
            blinkTimer = new System.Windows.Forms.Timer();
            blinkTimer.Interval = blinkIntervalMs;
            blinkTimer.Tick += BlinkTimer_Tick;
            blinkTimer.Start();
        }

        // Set gravity based on chosen environment
        private void SetGravityFromSelection()
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
            // Quit on 'X' after game over or successful landing
            if ((gameOver || landedSuccess) && e.KeyCode == Keys.X)
            {
                this.Close();
                return;
            }
            // Restart on 'R' after game over or successful landing
            if ((gameOver || landedSuccess) && e.KeyCode == Keys.R)
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
            // Align pad to terrain segments so it sits on horizontal terrain vertices
            var rng = new Random();
            float segmentWidth = ClientSize.Width / (float)terrainSegments;
            int padSegCount = Math.Max(1, (int)Math.Round(padWidth / segmentWidth));
            padSegCount = Math.Min(padSegCount, terrainSegments);
            int startIdx = rng.Next(0, terrainSegments - padSegCount + 1);
            padX = startIdx * segmentWidth;
            padWidth = padSegCount * segmentWidth;

            // Place landing pad within 3 screens of start
            int screenW = ClientSize.Width;
            int lower = Math.Max(0, (int)x - 3 * screenW);
            int upper = (int)x + 3 * screenW - (int)padWidth;
            // Generate jagged terrain
            terrainPoints = new PointF[terrainSegments + 1];
            float segmentWidth2 = ClientSize.Width / (float)terrainSegments;
            float baseY = ClientSize.Height - terrainHeight;
            for (int i = 0; i <= terrainSegments; i++)
            {
                float tx = i * segmentWidth2;
                float ty = baseY + (float)(rng.NextDouble() * 2 - 1) * terrainVariation;
                // ensure terrain does not rise above half the screen and stays above bottom terrainHeight
                float minY = ClientSize.Height / 2f;
                float maxY = ClientSize.Height - terrainHeight;
                ty = Math.Clamp(ty, minY, maxY);
                terrainPoints[i] = new PointF(tx, ty);
            }
            // Flatten terrain under the pad to make landing pad horizontal
            int endIdx = startIdx + padSegCount;
            float padY = (terrainPoints[startIdx].Y + terrainPoints[endIdx].Y) / 2;
            padY = Math.Clamp(padY, 1, ClientSize.Height); // clamp pad Y to be above 0
            for (int i = startIdx; i <= endIdx; i++)
                terrainPoints[i].Y = padY;
        }

        // Helper to get terrain Y at given X via interpolation
        private float GetTerrainYAt(float xPos)
        {
            if (terrainPoints == null) return ClientSize.Height - terrainHeight;
            float segmentWidth = ClientSize.Width / (float)terrainSegments;
            int idx = (int)(xPos / segmentWidth);
            idx = Math.Clamp(idx, 0, terrainSegments - 1);
            PointF p0 = terrainPoints[idx];
            PointF p1 = terrainPoints[idx + 1];
            float t = (xPos - p0.X) / (p1.X - p0.X);
            return p0.Y + t * (p1.Y - p0.Y);
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            float delta = gameTimer.Interval;
            // Rotation
            if (rotatingLeft) angle -= rotationSpeed * delta;
            if (rotatingRight) angle += rotationSpeed * delta;
            // Thrust with fuel consumption
            if (thrusting)
            {
                if (fuel > 0f)
                {
                    vx += (float)Math.Sin(angle) * thrustPower * delta;
                    vy += -(float)Math.Cos(angle) * thrustPower * delta;
                    fuel -= fuelConsumptionRate * delta;
                    if (fuel < 0f) fuel = 0f;
                }
                else
                {
                    thrusting = false; // disable thrust when out of fuel
                }
            }
            // Gravity
            vy += gravity * delta;
            // Position update
            x += vx * delta;
            y += vy * delta;
            x = Math.Max(0, x);
            y = Math.Clamp(y, 0, ClientSize.Height);

            // Wrap x for terrain collision
            float wrapWidth = ClientSize.Width;
            float modX = x % wrapWidth;
            if (modX < 0) modX += wrapWidth;

            float terrainY = GetTerrainYAt(modX);
            if (!gameOver && !landedSuccess && y + 20 >= terrainY)
            {
                // Position the lander on the ground
                y = terrainY - 20;
                // Check for successful landing on pad
                if (Math.Abs(vx) <= 0.5f && Math.Abs(vy) <= 0.5f
                    && modX >= padX && modX <= padX + padWidth)
                {
                    // Stop only on successful landing, keep pad lights on
                    gameTimer.Stop();
                    landedSuccess = true;
                    showPadLights = true;
                    blinkTimer.Stop();
                }
                else
                {
                    // Crash: trigger game over and spawn debris immediately
                    gameOver = true;
                    // Create debris explosion
                    var rng2 = new Random();
                    int pieces = 30;
                    for (int j = 0; j < pieces; j++)
                    {
                        float a2 = (float)(rng2.NextDouble() * Math.PI * 2);
                        float length = rng2.Next(5, 15);
                        var start = new PointF(x, y);
                        var end = new PointF(x + (float)Math.Cos(a2) * length, y + (float)Math.Sin(a2) * length);
                        float dvx = (end.X - start.X) * 0.1f;
                        float dvy = (end.Y - start.Y) * 0.1f;
                        debris.Add((start, end, dvx, dvy));
                    }
                }
            }
            // Update camera to follow ship when near edges
            if (x - cameraX < scrollMargin)
                cameraX = Math.Max(0, x - scrollMargin);
            else if (x - cameraX > ClientSize.Width - scrollMargin)
                cameraX = x - (ClientSize.Width - scrollMargin);
            // Trigger redraw
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
            // Reset physics and flags
            x = ClientSize.Width / 2;
            y = 50;
            vx = vy = angle = 0;
            thrusting = rotatingLeft = rotatingRight = false;
            gameOver = false;
            landedSuccess = false;
            fuel = 100f;
            // Reset camera and debris
            cameraX = 0f;
            debris.Clear();
            // Reset pad lights and blinking timer
            showPadLights = true;
            blinkTimer.Start();
            // Align new pad to terrain segments
            var rng = new Random();
            float segW = ClientSize.Width / (float)terrainSegments;
            int padSegs = Math.Max(1, (int)Math.Round(padWidth / segW));
            padSegs = Math.Min(padSegs, terrainSegments);
            int idx0 = rng.Next(0, terrainSegments - padSegs + 1);
            padX = idx0 * segW;
            padWidth = padSegs * segW;
            // Generate and flatten terrain under new pad
            terrainPoints = new PointF[terrainSegments + 1];
            float segmentWidth = ClientSize.Width / (float)terrainSegments;
            float baseY = ClientSize.Height - terrainHeight;
            for (int i = 0; i <= terrainSegments; i++)
            {
                float tx = i * segmentWidth;
                float ty = baseY + (float)(rng.NextDouble() * 2 - 1) * terrainVariation;
                // ensure terrain does not rise above half the screen and stays above bottom terrainHeight
                float minY = ClientSize.Height / 2f;
                float maxY = ClientSize.Height - terrainHeight;
                ty = Math.Clamp(ty, minY, maxY);
                terrainPoints[i] = new PointF(tx, ty);
            }
            // Flatten the terrain under this pad to make it horizontal
            int startIdx = idx0;
            int endIdx = startIdx + padSegs;
            float padY = (terrainPoints[startIdx].Y + terrainPoints[endIdx].Y) / 2;
            padY = Math.Clamp(padY, 1, ClientSize.Height);
            for (int i = startIdx; i <= endIdx; i++)
                terrainPoints[i].Y = padY;
            // Place landing pad within 3 screens of start
            int screenW = ClientSize.Width;
            int lower = Math.Max(0, (int)x - 3 * screenW);
            int upper = (int)x + 3 * screenW - (int)padWidth;
            // Reset gravity in case environment changed
            SetGravityFromSelection();
            // Stop timer and wait for first control
            gameTimer.Stop();
            // blink logic removed
        }
        
        // toggle blinking pad lights
        private void BlinkTimer_Tick(object? sender, EventArgs e)
        {
            if (landedSuccess)
            {
                // Stop blinking after successful landing
                showPadLights = true;
                blinkTimer.Stop();
            }
            else
            {
                showPadLights = !showPadLights;
            }
            Invalidate();
        }

        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);
            // Apply camera offset for world rendering
            g.ResetTransform();
            g.TranslateTransform(-cameraX, 0);
            float wrapWidth = ClientSize.Width; // width used for terrain and pad tiling

            // Draw terrain
            // Draw terrain lines vector-style
            if (terrainPoints != null)
            {
                using var terrainPen = new Pen(Color.Gray, 2);
                int baseTile = (int)Math.Floor(cameraX / wrapWidth);
                for (int t = baseTile - 1; t <= baseTile + 1; t++)
                {
                    g.DrawLines(terrainPen, terrainPoints);
                    g.TranslateTransform(wrapWidth, 0);
                }
                g.TranslateTransform(-wrapWidth * 3, 0);
            }

            // Draw landing pad once in world coordinates so it never disappears
            float padY = GetTerrainYAt(padX);
            using (var padPen = new Pen(Color.Green, 5))
            {
                g.DrawLine(padPen,
                    padX, padY,
                    padX + padWidth, padY);
            }
            // Draw blinking lights at pad ends
            if (showPadLights)
            {
                using var lightBrush = new SolidBrush(Color.Yellow);
                float r = 5f;
                g.FillEllipse(lightBrush, padX - r, padY - 2*r, 2*r, 2*r);
                g.FillEllipse(lightBrush, padX + padWidth - r, padY - 2*r, 2*r, 2*r);
            }

            // Draw debris explosion on crash
            if (gameOver && debris.Count > 0)
            {
                using var debrisPen = new Pen(Color.Orange, 2);
                foreach (var (start, end, _, _) in debris)
                    g.DrawLine(debrisPen, start, end);
            }

            // Draw vector lander (outlined triangle) with thrust flame only when not crashed
            if (!gameOver)
            {
                var worldXform = g.Transform;
                g.TranslateTransform(x, y);
                g.RotateTransform(angle * 180f / (float)Math.PI);
                PointF[] shipTri = { new PointF(0, -20), new PointF(-10, 20), new PointF(10, 20) };
                using var shipPen = new Pen(Color.White, 2);
                g.DrawPolygon(shipPen, shipTri);
                if (thrusting)
                {
                    var rand = new Random();
                    PointF[] flame = { new PointF(-5, 20), new PointF(0, 20 + rand.Next(5, 15)), new PointF(5, 20) };
                    using var flamePen = new Pen(Color.Orange, 2);
                    g.DrawPolygon(flamePen, flame);
                }
                g.Transform = worldXform;
            }
            // Reset transform for HUD (screen space) so HUD follows ship correctly
            g.ResetTransform();

            // HUD: velocity, altitude, fuel next to ship (only when not crashed)
            if (!gameOver)
            {
                float craftHalfW = 10f, craftHalfH = 20f, hudMargin = 10f;
                float screenX = x - cameraX, screenY = y;
                float hudX = screenX + craftHalfW + hudMargin;
                float hudY = screenY - craftHalfH;
                var velText = $"Vx:{vx:0.00} Vy:{vy:0.00}";
                g.DrawString(velText, this.Font, Brushes.White, hudX, hudY);
                hudY += this.Font.Height + 5;
                var altText = $"Alt:{ClientSize.Height - y:0.00}";
                g.DrawString(altText, this.Font, Brushes.White, hudX, hudY);
                hudY += this.Font.Height + 5;
                var fuelText = $"Fuel:{fuel:0.00}";
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
                }
            }
            else if (landedSuccess)
            {
                using (var font = new Font("Arial", 24))
                using (var brush = new SolidBrush(Color.Lime))
                {
                    g.DrawString("Landed Successfully!", font, brush, ClientSize.Width / 2 - 120, ClientSize.Height / 2 - 30);
                    // Restart and quit prompts
                    g.DrawString("Press R to restart", font, Brushes.White, ClientSize.Width / 2 - 100, ClientSize.Height / 2);
                    g.DrawString("Press X to quit", font, Brushes.White, ClientSize.Width / 2 - 80, ClientSize.Height / 2 + 30);
                }
            }
        } // end Form1_Paint
    }
}
