using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace LanderGame
{
    public partial class Form1 : Form
    {
        private GameEngine gameEngine = null!;
        private bool thrusting, rotatingLeft, rotatingRight;
        private bool paused;
        private float gravity;

        // conversion factor from radians to degrees
        private const float RadToDeg = 180f / (float)Math.PI;

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
            if (!gameEngine.IsGameOver && e.KeyCode == Keys.Escape)
            {
                paused = !paused;
                if (paused) gameTimer.Stop(); else gameTimer.Start();
                Invalidate();
                return;
            }
            // Quit on 'X' after game over, successful landing, or pause menu
            if ((gameEngine.IsGameOver || paused) && e.KeyCode == Keys.X)
            {
                this.Close();
                return;
            }
            // Restart on 'R' after game over, successful landing, or pause menu
            if ((gameEngine.IsGameOver || paused) && e.KeyCode == Keys.R)
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
            // Initialize game engine
            gameEngine = new GameEngine();
            gameEngine.GameStateChanged += () => Invalidate();
            gameEngine.RequestRedraw += () => Invalidate();
            gameEngine.Initialize(ClientSize.Width, ClientSize.Height, gravity);

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
            return gameEngine.GetTerrainYAt(xPos);
        }
        // Expose internal state for testing
        internal Lander LanderInstance => gameEngine.LanderInstance;
        internal IReadOnlyList<LandingPad> Pads => gameEngine.Pads;
        internal LandingPad CurrentPad => gameEngine.CurrentPad;
        internal bool LandedSuccessFlag => gameEngine.LandedSuccessFlag;

        // Expose gravity for testing
        internal float Gravity => gravity;
        // Expose environment selection for testing
        internal int EnvironmentIndex
        {
            get => envComboBox.SelectedIndex;
            set => envComboBox.SelectedIndex = value;
        }

        // Expose crash reason for testing
        internal string CrashReason => gameEngine.CrashReason;

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            // Update input state
            gameEngine.UpdateInput(thrusting, rotatingLeft, rotatingRight);

            // Tick the game engine
            float delta = gameTimer.Interval;
            gameEngine.Tick(delta, ClientSize.Width, ClientSize.Height);
        }

        // Reset game state for new play
        private void ResetGame()
        {
            thrusting = rotatingLeft = rotatingRight = false;
            paused = false;
            gameEngine.Reset(ClientSize.Width, ClientSize.Height, gravity);
            gameTimer.Stop();
            // Force redraw so screen resets immediately
            Invalidate();
        }

        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);
            // Apply camera offset for world rendering
            g.ResetTransform();
            g.TranslateTransform(-gameEngine.CameraX, 0);

            // draw star field
            foreach (var star in gameEngine.Stars)
                g.FillRectangle(Brushes.White, star.X, star.Y, 2, 2);

            float wrapWidth = ClientSize.Width; // width used for terrain and pad tiling

            // Draw terrain
            gameEngine.TerrainInstance.Draw(g, gameEngine.CameraX, wrapWidth);

            // Draw all landing pads
            foreach (var pad in gameEngine.Pads)
                pad.Draw(g);

            // Draw debris explosion on crash
            if (gameEngine.IsGameOver && gameEngine.Debris.Count > 0)
            {
                using var debrisPen = new Pen(Color.Orange, 2);
                foreach (var (start, end, _, _) in gameEngine.Debris)
                    g.DrawLine(debrisPen, start, end);
            }

            // Draw lander
            if (!gameEngine.IsGameOver) gameEngine.LanderInstance.Draw(g, thrusting);

            // Reset transform for HUD (screen space)
            g.ResetTransform();

            // HUD: velocity, altitude, fuel next to ship (only when not crashed)
            if (!gameEngine.IsGameOver)
            {
                var lander = gameEngine.LanderInstance;
                float craftHalfW = 10f, craftHalfH = 20f, hudMargin = 10f;
                float screenX = lander.X - gameEngine.CameraX, screenY = lander.Y;
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
            if (gameEngine.IsGameOver)
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
                    g.DrawString(gameEngine.CrashReason, font, Brushes.White,
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
