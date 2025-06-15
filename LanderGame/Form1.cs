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
        }        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle title screen - any key starts the game
            if (gameEngine.CurrentState == GameState.TitleScreen)
            {
                gameEngine.StartGame(ClientSize.Width, ClientSize.Height);
                gameTimer.Start();
                return;
            }

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
            }            // Restart on 'R' after game over, successful landing, or pause menu
            if ((gameEngine.IsGameOver || paused) && e.KeyCode == Keys.R)
            {
                ResetGame();
                return;
            }

            // Only process game controls if actually playing
            if (gameEngine.CurrentState != GameState.Playing) return;

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
        }        /// <summary>For testing: initialize game state as if loaded.</summary>
        internal void InitializeForTest()
        {
            Form1_Load(this, EventArgs.Empty);
            // For tests, start the game immediately so game objects are available
            gameEngine.StartGame(ClientSize.Width, ClientSize.Height);
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
        }        // Reset game state for new play
        private void ResetGame()
        {
            thrusting = rotatingLeft = rotatingRight = false;
            paused = false;
            gameEngine.Initialize(ClientSize.Width, ClientSize.Height, gravity);
            gameTimer.Stop();
            // Force redraw so screen resets immediately
            Invalidate();
        }private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);            // Handle title screen
            if (gameEngine.CurrentState == GameState.TitleScreen)
            {
                // Draw star field for title screen
                foreach (var star in gameEngine.Stars)
                    g.FillRectangle(Brushes.White, star.X, star.Y, 2, 2);

                // Draw moon surface at bottom
                DrawMoonSurface(g);

                // Draw title
                using (var titleFont = new Font("Arial", 48, FontStyle.Bold))
                using (var subtitleFont = new Font("Arial", 24))
                {
                    var titleText = "Lunar Invasion";
                    var subtitleText = "Press any key to play";
                    
                    var titleSize = g.MeasureString(titleText, titleFont);
                    var subtitleSize = g.MeasureString(subtitleText, subtitleFont);
                    
                    var centerX = ClientSize.Width / 2f;
                    var centerY = ClientSize.Height / 2f;
                    
                    // Draw title
                    g.DrawString(titleText, titleFont, Brushes.White, 
                        centerX - titleSize.Width / 2, centerY - titleSize.Height / 2 - 50);
                    
                    // Draw subtitle
                    g.DrawString(subtitleText, subtitleFont, Brushes.Gray, 
                        centerX - subtitleSize.Width / 2, centerY - subtitleSize.Height / 2 + 50);
                }
                return;
            }

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
        } // end Form1_Paint        /// <summary>
        /// Draws a curved moon surface at the bottom of the title screen with craters
        /// </summary>
        private void DrawMoonSurface(Graphics g)
        {
            var surfaceHeight = ClientSize.Height / 4; // Moon surface takes up bottom quarter
            var surfaceBaseY = ClientSize.Height - surfaceHeight;
              // Create points for curved surface
            var curvePoints = new List<PointF>();
            var numPoints = 50; // Number of points to create smooth curve
            
            for (int i = 0; i <= numPoints; i++)
            {
                var x = (float)(i * ClientSize.Width) / numPoints;
                // Create a gentle curve that rises in the middle (inverted sine)
                var curveOffset = (float)(Math.Sin((double)i / numPoints * Math.PI) * surfaceHeight * 0.4); // 40% of surface height
                var y = ClientSize.Height - curveOffset; // Start from bottom and rise up
                curvePoints.Add(new PointF(x, y));
            }
            
            // Add bottom corners to close the shape
            curvePoints.Add(new PointF(ClientSize.Width, ClientSize.Height));
            curvePoints.Add(new PointF(0, ClientSize.Height));
            
            // Draw curved moon surface with darker gray
            using (var surfaceBrush = new SolidBrush(Color.FromArgb(140, 140, 140))) // Darker gray
            {
                g.FillPolygon(surfaceBrush, curvePoints.ToArray());
            }            // Add some craters for visual interest
            using (var craterBrush = new SolidBrush(Color.FromArgb(100, 100, 100))) // Even darker gray for craters
            {
                // Large crater on the left (wide ellipse)
                var crater1X = ClientSize.Width * 0.2f;
                var crater1Y = GetSurfaceYAt(crater1X, curvePoints) + 30;
                var crater1Width = 50;
                var crater1Height = 30;
                g.FillEllipse(craterBrush, crater1X - crater1Width/2, crater1Y - crater1Height/2, crater1Width, crater1Height);
                  // Medium crater in the center (wide ellipse on the peak)
                var crater2X = ClientSize.Width * 0.5f;
                var crater2Y = GetSurfaceYAt(crater2X, curvePoints) + 20;
                var crater2Width = 35;
                var crater2Height = 20;
                g.FillEllipse(craterBrush, crater2X - crater2Width/2, crater2Y - crater2Height/2, crater2Width, crater2Height);
                
                // Small crater on the right (wide ellipse)
                var crater3X = ClientSize.Width * 0.8f;
                var crater3Y = GetSurfaceYAt(crater3X, curvePoints) + 25;
                var crater3Width = 22;
                var crater3Height = 12;
                g.FillEllipse(craterBrush, crater3X - crater3Width/2, crater3Y - crater3Height/2, crater3Width, crater3Height);
                
                // Additional small craters for detail
                var crater4X = ClientSize.Width * 0.3f;
                var crater4Y = GetSurfaceYAt(crater4X, curvePoints) + 35;
                var crater4Width = 18;
                var crater4Height = 10;
                g.FillEllipse(craterBrush, crater4X - crater4Width/2, crater4Y - crater4Height/2, crater4Width, crater4Height);
                
                var crater5X = ClientSize.Width * 0.7f;
                var crater5Y = GetSurfaceYAt(crater5X, curvePoints) + 15;
                var crater5Width = 25;
                var crater5Height = 15;
                g.FillEllipse(craterBrush, crater5X - crater5Width/2, crater5Y - crater5Height/2, crater5Width, crater5Height);
            }
        }
        
        /// <summary>
        /// Helper method to get the Y coordinate of the curved surface at a given X position
        /// </summary>
        private float GetSurfaceYAt(float x, List<PointF> curvePoints)
        {
            // Find the closest point in the curve
            var closestPoint = curvePoints.OrderBy(p => Math.Abs(p.X - x)).First();
            return closestPoint.Y;
        }

    }
}
