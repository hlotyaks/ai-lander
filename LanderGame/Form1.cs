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
        private float padWidth = 40f;
        private float padX;
        private bool landedSuccess = false;
        private float fuel = 100f;
        private float fuelConsumptionRate = 0.02f;

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
            this.Paint += Form1_Paint;
            this.Load += Form1_Load;
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
            // Random landing pad location within screen width
            var rng = new Random();
            padX = rng.Next(0, ClientSize.Width - (int)padWidth);
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
            // Clamp to window bounds
            x = Math.Clamp(x, 0, ClientSize.Width);
            y = Math.Clamp(y, 0, ClientSize.Height);
            // Terrain collision detection
            float terrainY = ClientSize.Height - terrainHeight;
            if (!gameOver && !landedSuccess && y + 20 >= terrainY)
            {
                // Position the lander on the ground
                y = terrainY - 20;
                gameTimer.Stop();
                // Check for successful landing on pad
                if (Math.Abs(vx) <= 0.5f && Math.Abs(vy) <= 0.5f
                    && x >= padX && x <= padX + padWidth)
                {
                    landedSuccess = true;
                }
                else
                {
                    gameOver = true;
                }
            }
            // Trigger redraw
            Invalidate();
        }

        // Adjusted sender nullability to match PaintEventHandler
        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            // Transform to lander position and rotation
            g.TranslateTransform(x, y);
            g.RotateTransform(angle * 180f / (float)Math.PI);
            // Draw lander triangular shape
            PointF[] tri = { new PointF(0, -20), new PointF(-10, 20), new PointF(10, 20) };
            g.FillPolygon(Brushes.White, tri);
            // Draw thrust flame
            if (thrusting)
            {
                var rand = new Random();
                PointF[] flame = { new PointF(-5, 20), new PointF(0, 20 + rand.Next(5, 15)), new PointF(5, 20) };
                g.FillPolygon(Brushes.Orange, flame);
            }
            g.ResetTransform();
            
            // Draw terrain
            float terrainY = ClientSize.Height - terrainHeight;
            g.FillRectangle(Brushes.Gray, 0, terrainY, ClientSize.Width, terrainHeight);
            // Draw landing pad
            g.FillRectangle(Brushes.Green, padX, terrainY - 5, padWidth, 5);
            
            // Landing or crash messages
            if (landedSuccess)
            {
                // Use a bigger font for end-game text
                using var endFont = new Font(this.Font.FontFamily, this.Font.Size * 3, this.Font.Style);
                var txt = "Landed!";
                var size = g.MeasureString(txt, endFont);
                g.DrawString(txt, endFont, Brushes.Green,
                    (ClientSize.Width - size.Width) / 2, (ClientSize.Height - size.Height) / 2);
                // Restart prompt
                var prompt = "Press R to play again";
                var psize = g.MeasureString(prompt, endFont);
                g.DrawString(prompt, endFont, Brushes.White,
                    (ClientSize.Width - psize.Width) / 2,
                    (ClientSize.Height - size.Height) / 2 + size.Height + 5);
                // Quit prompt
                var quitPrompt = "Press X to quit the game";
                var qsize = g.MeasureString(quitPrompt, endFont);
                g.DrawString(quitPrompt, endFont, Brushes.White,
                    (ClientSize.Width - qsize.Width) / 2,
                    (ClientSize.Height - size.Height) / 2 + size.Height + 5 + psize.Height + 5);
                return;
            }
            if (gameOver)
            {
                // Explosion
                g.FillEllipse(Brushes.Red, x - 20, y - 20, 40, 40);
                // Use a bigger font for end-game text
                using var endFont2 = new Font(this.Font.FontFamily, this.Font.Size * 3, this.Font.Style);
                var goText = "Game Over";
                var size = g.MeasureString(goText, endFont2);
                g.DrawString(goText, endFont2, Brushes.Red,
                    (ClientSize.Width - size.Width) / 2, (ClientSize.Height - size.Height) / 2);
                // Restart prompt
                var prompt = "Press R to play again";
                var pSize = g.MeasureString(prompt, endFont2);
                g.DrawString(prompt, endFont2, Brushes.White,
                    (ClientSize.Width - pSize.Width) / 2,
                    (ClientSize.Height - size.Height) / 2 + size.Height + 5);
                // Quit prompt
                var quitPrompt2 = "Press X to quit the game";
                var qSize2 = g.MeasureString(quitPrompt2, endFont2);
                g.DrawString(quitPrompt2, endFont2, Brushes.White,
                    (ClientSize.Width - qSize2.Width) / 2,
                    (ClientSize.Height - size.Height) / 2 + size.Height + 5 + pSize.Height + 5);
                return;
            }

            // HUD: draw velocity, altitude, and fuel relative to craft
            float hudMargin = 10f;
            float craftHalfWidth = 10f;
            float craftHalfHeight = 20f;
            float hudX = x + craftHalfWidth + hudMargin;
            float hudY = y - craftHalfHeight;
            // Velocity
            var velText = $"Vx:{vx:0.00} Vy:{vy:0.00}";
            var velSize = g.MeasureString(velText, this.Font);
            g.DrawString(velText, this.Font, Brushes.White, hudX, hudY);
            // Altitude
            hudY += velSize.Height + 5;
            var altValue = ClientSize.Height - y;
            var altText = $"Alt:{altValue:0.00}";
            var altSize = g.MeasureString(altText, this.Font);
            g.DrawString(altText, this.Font, Brushes.White, hudX, hudY);
            // Fuel
            hudY += altSize.Height + 5;
            var fuelText = $"Fuel:{fuel:0.00}";
            g.DrawString(fuelText, this.Font, Brushes.White, hudX, hudY);
        }

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
            // Randomize new landing pad
            var rng = new Random();
            padX = rng.Next(0, ClientSize.Width - (int)padWidth);
            // Reset gravity in case environment changed
            SetGravityFromSelection();
            // Stop timer and wait for first control
            gameTimer.Stop();
            Invalidate();
        }
    }
}
